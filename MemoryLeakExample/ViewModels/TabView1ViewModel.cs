using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MemoryLeakExample.ViewModels.Messages;
using MemoryLeakExampleDatabase;
using MvvmHelpers;

namespace MemoryLeakExample.ViewModels
{
    public partial class TabView1ViewModel : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
    {
        [ObservableProperty]
        private ObservableRangeCollection<StoreSelection> selectedStoreItems;

        [ObservableProperty]
        private ObservableRangeCollection<GroupedStoreSelectionItemList> groupedStoreItems;
        


        public TabView1ViewModel()
        {
            //DisplayMemoryConsumption();

            WeakReferenceMessenger.Default.Register<RefreshDataBaseMessage>(this, HandleDatabaseChangedMessage);
            WeakReferenceMessenger.Default.Register<UpdateSelectedStoreItemsMessage>(this, HandleUpdateSelectedStoreItemsMessage);


            SelectedStoreItems = new ObservableRangeCollection<StoreSelection>();
            GroupedStoreItems = new ObservableRangeCollection<GroupedStoreSelectionItemList>();

            _ = RefreshDatabase();

            //DisplayMemoryConsumption();

            GC.Collect();

            //DisplayMemoryConsumption();
        }

        private List<Store> GetSomeStoreExamples()
        {
            var store1 = new Store
            {
                Id = 1,
                Name = "Apple Store",
                Street = "Apple Store Street",
                Manager = 1,
                SalesPerson = 10.0,
                Clerk = 5
            };

            var store2 = new Store
            {
                Id = 2,
                Name = "Banana Store",
                Street = "Banana Store Street",
                Manager = 2,
                SalesPerson = 20.0,
                Clerk = 12.0
            };

            var store3 = new Store
            {
                Id = 3,
                Name = "Kiwi Store",
                Street = "Kiwi Store Street",
                Manager = 3,
                SalesPerson = 30.0,
                Clerk = 23.0
            };



            var storeSelection1 = new StoreSelection()
            {
                StoreItem = store1,
                SortOrderIndex = 1,
                IsSelected = true,
                Min = 1,
                Max = 5,
                OptimalProfit = 3
            };

            var storeSelection2 = new StoreSelection()
            {
                StoreItem = store2,
                SortOrderIndex = 2,
                IsSelected = true,
                Min = 0,
                Max = 2,
                OptimalProfit = 1
            };

            var storeSelection3 = new StoreSelection()
            {
                StoreItem = store3,
                SortOrderIndex = 3,
                IsSelected = true,
                Min = 2,
                Max = 8,
                OptimalProfit = 4
            };

            store1.StoreSelections.Add(storeSelection1);
            store2.StoreSelections.Add(storeSelection2);
            store3.StoreSelections.Add(storeSelection3);

            return new List<Store> { store1, store2, store3 };
        }



        #region Message Command Handlers

        private async void HandleDatabaseChangedMessage(object recipient, RefreshDataBaseMessage refreshDatabaseMessage)
        {
            //DisplayMemoryConsumption();

            await RefreshDatabase();

            //DisplayMemoryConsumption();
        }

        private async void HandleUpdateSelectedStoreItemsMessage(object recipient, UpdateSelectedStoreItemsMessage updateSelectedStoreItemsMessage)
        {
            GC.Collect();

            //DisplayMemoryConsumption();

            ResetMinMaxAndOptimalProfitForNewSelections();
            
            await CollectSelectedStoreItems();

            DisplayMemoryConsumption();

            GC.Collect();
        }

        #endregion

        #region Command Handler

        [RelayCommand]
        private Task DisplayBottomSheet()
        {
            WeakReferenceMessenger.Default.Send(new DisplayBottomSheetMessage(true));

            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task CalculateStoreProfits()
        {
            // ... do some calculations here
            CalculateReachedProfits(SelectedStoreItems);

            await Task.CompletedTask;
        }

        private void CalculateReachedProfits(IEnumerable<StoreSelection> selectedStores)
        {
            foreach (var selectedStore in selectedStores)
            {
                selectedStore.OptimalProfit = 10;
            }            
        }


        [RelayCommand]
        private async Task RefreshDatabase()
        {
            //DisplayMemoryConsumption();

            GroupStoreItemsApplyingSearchFilter();

            await CollectSelectedStoreItems();

            //DisplayMemoryConsumption();

            await Task.CompletedTask;
        }

        [RelayCommand]
        private async Task SaveStore()
        {
            // ... save store and selected stores in the DB

            await Task.CompletedTask;
        }

        [RelayCommand]
        private async Task ExcludeFromStoreSelection(StoreSelection storeItemToExclude)
        {
            await Task.Run(() =>
            {
                foreach (var group in GroupedStoreItems)
                {
                    // Find the item with matching Store Id in the current group
                    var itemToExclude = group.FirstOrDefault(item => item.StoreItem.Id == storeItemToExclude.StoreItem.Id);

                    if (itemToExclude != null)
                    {
                        itemToExclude.IsSelected = false;
                    }
                }
            });

            OnPropertyChanged(nameof(GroupedStoreItems));

            HandleUpdateSelectedStoreItemsMessage(this, new UpdateSelectedStoreItemsMessage(true));

            await Task.CompletedTask;
        }

        [RelayCommand]
        private async Task DragAndDropEnded()
        {
            UpdateSortOrderIndices(SelectedStoreItems);

            await Task.CompletedTask;
        }

        #endregion

        #region Search Filter

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;

                    OnPropertyChanged(nameof(SearchText));
                    GroupStoreItemsApplyingSearchFilter();
                }
            }
        }

        #endregion

        #region Grouping

        private void GroupStoreItemsApplyingSearchFilter()
        {   
            var storeItems = GetSomeStoreExamples();

            var query = storeItems
                .Where(storeItem => string.IsNullOrEmpty(SearchText) || storeItem.Name.ToLower().StartsWith(SearchText.ToLower()))
                .OrderBy(storeItem => storeItem.Name)
                .GroupBy(storeItem => storeItem.Name.First().ToString(), StringComparer.OrdinalIgnoreCase)
                .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
                .Select(group =>
                {
                    // Create a list of StoreSelections by mapping each StoreItem in the group to a new StoreSelection instance
                    var storeSelections = group.Select(storeItem =>
                    {
                        var selectedStoreItem = SelectedStoreItems.FirstOrDefault(selectedItem => selectedItem.StoreItem.Id == storeItem.Id);

                        if (selectedStoreItem != null)
                        {
                            return selectedStoreItem;
                        }

                        return new StoreSelection
                        {
                            Id = selectedStoreItem?.Id ?? 0,
                            StoreId = storeItem?.Id ?? 0,
                            StoreItem = storeItem,
                            IsSelected = selectedStoreItem != null && selectedStoreItem.IsSelected,
                            Min = selectedStoreItem?.Min ?? 0,
                            Max = selectedStoreItem?.Max ?? 0,
                            OptimalProfit = selectedStoreItem?.OptimalProfit ?? 0,
                            SortOrderIndex = selectedStoreItem?.SortOrderIndex ?? -1,
                        };

                    }).ToList();

                    // Create a GroupedStoreSelectionItemList using the key and the list of StoreSelections
                    return new GroupedStoreSelectionItemList(group.Key, storeSelections);
                });

            GroupedStoreItems.Clear();
            GroupedStoreItems.AddRange(query);
            
            OnPropertyChanged(nameof(GroupedStoreItems));
        }

        #endregion

        #region Selected StoreItems

        private async Task CollectSelectedStoreItems()
        {
            // Retrieve the selected StoreItems from GroupedStoreItems and order them based on their SortOrderIndex
            var currentlySelectedItems = GroupedStoreItems
                .SelectMany(groupedStoreSelectionItem => groupedStoreSelectionItem)
                .Where(storeSelectionItem => storeSelectionItem != null && storeSelectionItem.IsSelected == true)
                .OrderBy(storeSelectionItem => storeSelectionItem.SortOrderIndex == -1 ? int.MaxValue : storeSelectionItem.SortOrderIndex)
                .ToList();

            // Set IsSelected to true for all selectedStoreItems
            currentlySelectedItems.ForEach(item => item.IsSelected = true);

            // Set IsSelected to false for all items not in selectedStoreItems
            foreach (var groupedItem in GroupedStoreItems.SelectMany(group => group))
            {
                if (!currentlySelectedItems.Any(item => item.StoreItem.Id == groupedItem.StoreItem.Id))
                {
                    groupedItem.IsSelected = false;
                }
            }

            UpdateSortOrderIndices(currentlySelectedItems);

            GC.Collect();

            var toBeDeleted = SelectedStoreItems.Where(item => !currentlySelectedItems.Contains(item)).ToList();
            var newlySelected = currentlySelectedItems.Where(item => !SelectedStoreItems.Contains(item)).ToList();


            // Delete unselected items
            foreach (var itemToDelete in toBeDeleted)
            {
                SelectedStoreItems.Remove(itemToDelete);
            }

            //SelectedStoreItems.RemoveRange(toBeDeleted/*, System.Collections.Specialized.NotifyCollectionChangedAction.Reset*/);

            // Add newly selected items
            SelectedStoreItems.AddRange(newlySelected, System.Collections.Specialized.NotifyCollectionChangedAction.Reset);

            GC.Collect();

            await Task.CompletedTask;
        }

        private static void DisplayMemoryConsumption()
        {
            long memoryUsage = GC.GetTotalMemory(false) / (1024 * 1024);
            App.Current.MainPage.DisplayAlert("AppResources", $"{memoryUsage} MB", "OK");
        }

        /// <summary>
        /// Resets the Min, Max, Optimal Profit values for newly selected StoreSelection items in the grouped collection.
        /// </summary>
        private async void ResetMinMaxAndOptimalProfitForNewSelections()
        {
            await Task.Run(() =>
            {
                // Retrieve the selected StoreItems
                var selectedStoreItems = GroupedStoreItems
                    .SelectMany(group => group)
                    .Where(storeSelectionItem => storeSelectionItem.IsSelected)
                    .ToList();

                var newlySelectedItems = selectedStoreItems.Where(newItem => !SelectedStoreItems.Any(existingItem => existingItem.StoreItem?.Id == newItem.StoreItem?.Id)).ToList();


                // Reset Min, Max, and Optimal Profit values for the newly selected items
                foreach (var newlySelectedItem in newlySelectedItems)
                {
                    newlySelectedItem.Min = 0;
                    newlySelectedItem.Max = 0;
                    newlySelectedItem.OptimalProfit = 0;
                }
            });
            
            OnPropertyChanged(nameof(GroupedStoreItems));
        }

        /// <summary>
        /// Sets the SortOrderIndex for each element in the collection based on its position in the list starting at index 1.
        /// Updates the corresponding SortOrderIndex in the GroupedStoreItems collection.
        /// </summary>
        /// <param name="selectedStoreItems">The IEnumerable of selected StoreItems used to determine the SortOrderIndex.</param>
        private async void UpdateSortOrderIndices(IEnumerable<StoreSelection> selectedStoreItems)
        {
            await Task.Run(() =>
            {
                int index = 1;

                // Update SortOrderIndex for selectedStoreItems
                foreach (var selectedStoreItem in selectedStoreItems)
                {
                    selectedStoreItem.SortOrderIndex = index;
                    index++;
                }

                // Update SortOrderIndex for GroupedStoreItems based on selectedStoreItems
                foreach (var group in GroupedStoreItems.ToList())
                {
                    foreach (var storeSelectionItem in group)
                    {
                        // Check if the item is in the selectedStoreItems list
                        var correspondingItem = selectedStoreItems.FirstOrDefault(selectedStoreItem => selectedStoreItem.StoreId == storeSelectionItem.StoreId);

                        // Update SortOrderIndex for the item based on its presence in the selectedStoreItems list
                        storeSelectionItem.SortOrderIndex = correspondingItem != null ? correspondingItem.SortOrderIndex : -1;
                    }
                }
            });

            OnPropertyChanged(nameof(GroupedStoreItems));
        }

        #endregion
    }
}

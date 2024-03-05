using MvvmHelpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MemoryLeakExampleDatabase
{
    public class StoreSelection : ObservableObject
    {
        #region Private Variables

        private SavedStore _savedStore;
        private Store _storeItem;

        #endregion


        [Key]                                                   // Primary Key will already be indexed in a Table
        [Column(Order = 1)]
        public int Id { get; set; }


        #region SavedStoreItem

        [Column(Order = 2)]
        [ForeignKey("SavedStore")]
        public int SavedStoreId { get; set; }
        public virtual SavedStore SavedStoreItem
        {
            get => _savedStore;
            set
            {
                if (SetProperty(ref _savedStore, value))
                {
                    if (_savedStore != null)
                    {
                        SavedStoreId = _savedStore.Id;
                    }
                }
            }
        }

        #endregion

        #region StoreItem

        [Column(Order = 3)]
        [ForeignKey("Store")]
        public int StoreId { get; set; }
        public virtual Store StoreItem
        {
            get => _storeItem;
            set
            {
                if (SetProperty(ref _storeItem, value))
                {
                    if (_storeItem != null)
                    {
                        StoreId = _storeItem.Id;
                    }
                }
            }
        }

        #endregion

        #region SortOrderIndex

        [Column(Order = 4)]
        [Range(1, int.MaxValue)]
        private int _sortOrderIndex;

        [Column(Order = 4)]
        [Range(1, int.MaxValue)]
        public int SortOrderIndex
        {
            get => _sortOrderIndex;
            set => SetProperty(ref _sortOrderIndex, value);
        }

        #endregion

        #region IsSelected

        private bool _isSelected = false;

        [NotMapped]
        [Column(Order = 5)]
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        #endregion

        #region Min Profit

        private int _min = 0;

        [Column(Order = 6)]
        public int Min
        {
            get => _min;
            set
            {
                if (SetProperty(ref _min, value))
                {
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }

        #endregion

        #region Max Profit

        private int _max = 0;

        [Column(Order = 7)]
        public int Max
        {
            get => _max;
            set
            {
                if (SetProperty(ref _max, value))
                {
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }

        #endregion

        #region Optimal Profit

        private int _optimalProfit = 0;

        [Column(Order = 8)]
        public int OptimalProfit
        {
            get => _optimalProfit;
            set => SetProperty(ref _optimalProfit, value);
        }

        #endregion        

        #region IsValid

        [NotMapped]
        [Column(Order = 9)]
        public bool IsValid { get => Min <= Max; }

        #endregion
    }
}

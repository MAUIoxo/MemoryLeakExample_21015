using CommunityToolkit.Mvvm.Messaging;
using MemoryLeakExample.ViewModels;
using MemoryLeakExample.ViewModels.Messages;
using XGENO.Maui.Controls;
using XGENO.Maui.Controls.Messages;

namespace MemoryLeakExample.Pages;

public partial class MainPage : ContentPage
{
    private readonly MainPageViewModel mainPageViewModel;
    
    public MainPage(MainPageViewModel viewModel)
    {
        InitializeComponent();

        mainPageViewModel = viewModel;
        BindingContext = viewModel;

        WeakReferenceMessenger.Default.Register<SelectedViewChangedMessage>(this, HandleSelectedViewChangedMessage);
        WeakReferenceMessenger.Default.Register<DisplayBottomSheetMessage>(this, HandleDisplayBottomSheetMessage);
        WeakReferenceMessenger.Default.Register<BottomSheetClosedMessage>(this, HandleBottomSheetClosedMessage);
    }  

    private void HandleDisplayBottomSheetMessage(object recipient, DisplayBottomSheetMessage message)
    {
        DisplayMemoryConsumption();

        if (message.Value == true)
        {
            _ = BottomSheet.OpenBottomSheet();
        }

        DisplayMemoryConsumption();
    }

    private static void DisplayMemoryConsumption()
    {
        long memoryUsage = GC.GetTotalMemory(false) / (1024 * 1024);
        //App.Current.MainPage.DisplayAlert("AppTitle", $"{memoryUsage} MB", "OK");
    }

    private void HandleSelectedViewChangedMessage(object recipient, SelectedViewChangedMessage message)
    {
        var selectedViewIndex = message.Value;
        DotnetBotCircleButton.IsVisible = false;


        // Round Button to add Stores for the TabView1ViewModel only visible when this view is active
        if (selectedViewIndex == 0)
        {
            ChangeMainPageTitleLanguage();

            DotnetBotCircleButton.IsVisible = true;
            ToolbarItems.Clear();
        }
        else if (selectedViewIndex == 1)
        {
            ChangeMainPageTitleLanguage();

            CreateAddNewStoreItemToolbarItem();

            WeakReferenceMessenger.Default.Send(new RefreshDataBaseMessage(true));
        }
    }

    private void HandleBottomSheetClosedMessage(object recipient, BottomSheetClosedMessage message)
    {
        WeakReferenceMessenger.Default.Send(new UpdateSelectedStoreItemsMessage(true));
        GC.Collect();
    }

    private void CreateAddNewStoreItemToolbarItem()
    {
        var addNewToolbarItem = new ToolbarItem();

        var fontImageSource = new FontImageSource();
        fontImageSource.Glyph = "+";
        fontImageSource.Color = Colors.White;
        fontImageSource.Size = 30;
        fontImageSource.SetBinding(ToolbarItem.CommandProperty, new Binding(nameof(MainPageViewModel.DisplayAddNewStoreItemDialogCommand)));

        addNewToolbarItem.IconImageSource = fontImageSource;
        addNewToolbarItem.SetBinding(ToolbarItem.CommandProperty, new Binding(nameof(MainPageViewModel.DisplayAddNewStoreItemDialogCommand)));
        
        ToolbarItems.Clear();
        ToolbarItems.Add(addNewToolbarItem);

        NavigationPage.SetTitleView(this, CreateTitleView());
    }

    private View CreateTitleView()
    {
        var titleViewLabel = new Label
        {
            Text = mainPageViewModel.MainPageTitle
        };

        var horizontalStackLayout = new HorizontalStackLayout();
        horizontalStackLayout.Children.Add(titleViewLabel);

        return horizontalStackLayout;
    }

    private void ChangeMainPageTitleLanguage()
    {
        mainPageViewModel.MainPageTitle = "Overview";
    }

    protected override void OnAppearing()
    {
        Shell.SetTabBarIsVisible(this, false);
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (height > 0)
        {
            BottomSheet.SheetHeight = height * 0.75;
        }
    }
}
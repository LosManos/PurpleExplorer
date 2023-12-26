using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using PurpleExplorer.Helpers;
using PurpleExplorer.Models;
using PurpleExplorer.ViewModels;

namespace PurpleExplorer.Views;

public class MainWindow : Window
{
    private readonly IApplicationService _applicationService;
    private readonly IModalWindowService _modalWindowService;

    public MainWindow(IApplicationService applicationService, IModalWindowService modalWindowService)
    {
        _applicationService = applicationService;
        _modalWindowService = modalWindowService;

        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    protected override async void OnOpened(EventArgs e)
    {
        // In the IDE a call to this method is done.
        // I guess it is a plugin that tries to render a preview.
        // If this call is made and throws an exception the designer / app state manager
        // believes there is no state and removes the file `appstate.json`.
        // And we cannot store any settings.
        // See https://github.com/AvaloniaUI/Avalonia/discussions/13895
        // and https://stackoverflow.com/questions/77636293/avalonia-11-rxapp-suspensionhost-getappstateappstate-returns-null-while-v-0/77636618#77636618
        if (Avalonia.Controls.Design.IsDesignMode == false)
        {
            var mainWindowViewModel = DataContext as MainWindowViewModel
                ?? throw new Exception("Expected a main window viewmodel but found none");
            mainWindowViewModel.ShowConnectionStringWindow();
        }

        // Well... a bug remains. when the application stops "null'" is written to the state.
        // Or maybe not as I added a `new AppState` to the ISuspensionDriver. It is a bit complex
        // to figure out exact workings. What I want to say is that the behaviour changes between
        // debugging/starting from IDE and starting from terminal.

        base.OnOpened(e);
    }

    private async void MessagesGrid_DoubleTapped(object sender, TappedEventArgs e)
    {
        var grid = sender as DataGrid;
        var mainWindowViewModel = DataContext as MainWindowViewModel;

        if (grid?.SelectedItem == null)
        {
            return;
        }

        var viewModal = new MessageDetailsWindowViewModel
        {
            Message = grid.SelectedItem as Message, 
            ConnectionString = mainWindowViewModel.ConnectionString,
            Subscription = mainWindowViewModel.CurrentSubscription,
            Queue = mainWindowViewModel.CurrentQueue
        };

        await _modalWindowService.ShowModalWindow<MessageDetailsWindow, MessageDetailsWindowViewModel>(viewModal);
    }

    private void MessagesGrid_Tapped(object sender, TappedEventArgs e)
    {
        var grid = sender as DataGrid;
        var mainWindowViewModel = DataContext as MainWindowViewModel;

        if (grid.SelectedItem is Message message)
        {
            mainWindowViewModel.SetSelectedMessage(message);
        }
    }

    private async void TreeView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var mainWindowViewModel = DataContext as MainWindowViewModel;
        var treeView = sender as TreeView;

        ClearOtherSelections(treeView);
        mainWindowViewModel.ClearAllSelections();
            
        var selectedItem = treeView.SelectedItems.Count > 0 ? treeView.SelectedItems[0] : null;
        if (selectedItem is ServiceBusSubscription selectedSubscription)
        {
            mainWindowViewModel.SetSelectedSubscription(selectedSubscription);
            await mainWindowViewModel.FetchMessages();
            mainWindowViewModel.RefreshTabHeaders();
        }

        if (selectedItem is ServiceBusTopic selectedTopic)
        {
            mainWindowViewModel.SetSelectedTopic(selectedTopic);
        }
            
        if (selectedItem is ServiceBusQueue selectedQueue)
        {
            mainWindowViewModel.SetSelectedQueue(selectedQueue);
            await mainWindowViewModel.FetchMessages();
            mainWindowViewModel.RefreshTabHeaders();
        }
    }

    private void ClearOtherSelections(TreeView currentTreeView)
    {
        var tvQueues = this.FindControl<TreeView>("tvQueues");
        var tvTopics = this.FindControl<TreeView>("tvTopics");
        if (currentTreeView == tvQueues)
        {
            tvTopics.UnselectAll();
        }

        if (currentTreeView == tvTopics)
        {
            tvQueues.UnselectAll();
        }
    }
}
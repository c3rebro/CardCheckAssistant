using CardCheckAssistant.ViewModels;
using CardCheckAssistant.Models;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml;

namespace CardCheckAssistant.Views;

public sealed partial class HomePage : Page
{
    public HomePageViewModel ViewModel
    {
        get; private set;
    }

    public HomePage()
    {
        ViewModel = App.GetService<HomePageViewModel>();
        InitializeComponent();
    }

    private void OpenReportWritable_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.OpenReportWritableCommand.Execute(null);
    }

    private void FilterStatusInProgress_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SelectedFilter = "InProgress";
    }

    private void FilterStatusCheckFinished_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SelectedFilter = "CheckFinished";
    }

    private void FilterClear_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SelectedFilter = "All";
    }

    private void SortJobNumberAscending_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SelectedSort = "JobNumber";
    }

    private void SortCreatedAscending_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SelectedSort = "Created";
    }

    private void SortStatusAscending_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SelectedSort = "Status";
    }

    private void SearchQuery_Submitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        DataGrid.ItemsSource = ViewModel.SearchData(args.QueryText);
    }

    private void OpenPDFButton_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        var root = sender as Grid;
        var dataObject = root.DataContext as CardCheckProcess;

        if (dataObject != null && (dataObject.Status == "CheckFinished"))
        {
            DataGrid.SelectedItem = dataObject;
            dataObject.IsSelected = true;
        }
    }

    private void OpenPDFButton_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        var root = sender as Grid;
        var dataObject = root.DataContext as CardCheckProcess;
        dataObject.IsSelected = false;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.PostPageLoadedCommand.Execute(null);
    }
}

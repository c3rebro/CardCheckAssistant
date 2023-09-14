using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Navigation;

using Microsoft.UI.Xaml.Media;

using CardCheckAssistant.ViewModels;
using CardCheckAssistant.Services;

using ctWinUI = CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml.Input;
using CardCheckAssistant.Models;

namespace CardCheckAssistant.Views
{
    public sealed partial class HomePage : Page
    {
        private long _token;
        private string _grouping;

        public HomePage()
        {
            InitializeComponent();
        }


        public HomePageViewModel ViewModel => DataContext as HomePageViewModel;

        private void FilterStatusInProgress_Click(object sender, RoutedEventArgs e)
        {
            DataGrid.ItemsSource = ViewModel.FilterData(HomePageViewModel.FilterOptions.InProgress).OrderByDescending(x => x.Date);
        }

        private void FilterStatusWaitForCustomer_Click(object sender, RoutedEventArgs e)
        {
            DataGrid.ItemsSource = ViewModel.FilterData(HomePageViewModel.FilterOptions.WaitForCustomer).OrderByDescending(x => x.Date);
        }

        private void FilterStatusCheckFinished_Click(object sender, RoutedEventArgs e)
        {
            DataGrid.ItemsSource = ViewModel.FilterData(HomePageViewModel.FilterOptions.CheckFinisched).OrderByDescending(x => x.Date);
        }

        private void FilterClear_Click(object sender, RoutedEventArgs e)
        {
            DataGrid.ItemsSource = ViewModel.FilterData(HomePageViewModel.FilterOptions.All).OrderByDescending(x => x.Date);
        }

        private void SearchQuery_Submitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            DataGrid.ItemsSource = ViewModel.SearchData(args.QueryText).OrderByDescending(x => x.Date);
        }

        private void OpenPDFButton_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var root = sender as Grid;
            var dataObject = root.DataContext as CardCheckProcess;

            if(dataObject != null && (dataObject.Status == OrderStatus.WaitForCustomer || dataObject.Status == OrderStatus.CheckFinished))
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
}

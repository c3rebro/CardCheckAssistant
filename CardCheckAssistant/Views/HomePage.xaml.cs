using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using CardCheckAssistant.ViewModels;
using CardCheckAssistant.Services;

namespace CardCheckAssistant.Views
{
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
        }

        public HomePageViewModel ViewModel => DataContext as HomePageViewModel;

        private async void MessageBox_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Message Dialog will be opened.");

            await this.MessageDialogAsync("All we are saying:", "Give peace a chance.", "Got it");

            Debug.WriteLine("Message Dialog was closed.");
        }
    }
}

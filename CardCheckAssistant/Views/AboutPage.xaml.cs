using CardCheckAssistant.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace CardCheckAssistant.Views;

public sealed partial class AboutPage : Page
{
    public AboutPageViewModel ViewModel
    {
        get;
    }

    public AboutPage()
    {
        ViewModel = App.GetService<AboutPageViewModel>();
        InitializeComponent();
    }
}

using CardCheckAssistant.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CardCheckAssistant.Views;

public sealed partial class Step1Page : Page
{
    public Step1PageViewModel ViewModel
    {
        get;
    }

    public Step1Page()
    {
        ViewModel = App.GetService<Step1PageViewModel>();
        InitializeComponent();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.PostPageLoadedCommand.Execute(null);
    }
}

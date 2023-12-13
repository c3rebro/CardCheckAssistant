using CardCheckAssistant.ViewModels;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

namespace CardCheckAssistant.Views;

public sealed partial class Step2Page : Page
{
    public Step2PageViewModel ViewModel
    {
        get;
    }

    public Step2Page()
    {
        ViewModel = App.GetService<Step2PageViewModel>();
        InitializeComponent();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.PostPageLoadedCommand.Execute(null);
    }
}

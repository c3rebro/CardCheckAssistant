using CardCheckAssistant.ViewModels;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

namespace CardCheckAssistant.Views;

public sealed partial class Step3Page : Page
{
    public Step3PageViewModel ViewModel
    {
        get;
    }

    public Step3Page()
    {
        ViewModel = App.GetService<Step3PageViewModel>();
        InitializeComponent();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.PostPageLoadedCommand.Execute(null);
    }
}

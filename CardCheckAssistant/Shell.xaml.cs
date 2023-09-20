using System.Linq;
using CardCheckAssistant.Services;
using CardCheckAssistant.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using WinUICommunity.Common.ViewModel;

namespace CardCheckAssistant;

public sealed partial class Shell : Page
{
    public ShellViewModel ViewModel { get; } = new ShellViewModel();

    public Shell()
    {
        InitializeComponent();

        ViewModel.InitializeNavigation(ContentFrame, NavigationView)
            .WithKeyboardAccelerator(KeyboardAccelerators)
            .WithSettingsPage(typeof(SettingsPage));

    }

    private void UserControl_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ViewModel.OnLoaded();

        var window = (Application.Current as App)?.Window as MainWindow ?? new MainWindow();
        var navigation = window.Navigation;
        var homePage = navigation.GetNavigationViewItems(typeof(HomePage)).First();
        navigation.SetCurrentNavigationViewItem(homePage);

        using SettingsReaderWriter settings = new SettingsReaderWriter();

        window.SelectedTheme = settings.DefaultSettings.DefaultTheme;
    }

    private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        ViewModel.OnItemInvoked(args);
    }
}

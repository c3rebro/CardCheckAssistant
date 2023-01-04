using CardCheckAssistant.Services;
using CardCheckAssistant.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CardCheckAssistant;

public sealed partial class Shell : Page
{
    public Shell()
    {
        //Title = "SimonsVoss CardCheckAssistant";

        InitializeComponent();

        (Application.Current as App).EnsureSettings();
        ApplyTheme();
    }

    private void ToggleButton_Click(object sender, RoutedEventArgs e)
    {
        var settings = (Application.Current as App).Settings;
        settings.IsLightTheme = !settings.IsLightTheme;
        (Application.Current as App).SaveSettings();
        ShellRoot.ActualThemeChanged += Root_ActualThemeChanged;
        ApplyTheme();
    }

    private void ApplyTheme()
    {
        var settings = (Application.Current as App).Settings;
        ShellRoot.RequestedTheme = settings.IsLightTheme ? ElementTheme.Light : ElementTheme.Dark;
    }
    private void Root_ActualThemeChanged(FrameworkElement sender, object args)
    {
        // Theme change refinements (e.g. content dialogs and title bar).
    }
}

using CardCheckAssistant.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUICommunity.Common.Helpers;
using WinUICommunity.Common.Tools;
using WinUICommunity.Common.ViewModel;
using System.Windows.Input;

namespace CardCheckAssistant;

public sealed partial class MainWindow : Window
{
    public Grid ApplicationTitleBar => AppTitleBar;
    public INavigation Navigation => RootShell;
    internal static MainWindow Instance { get; private set; }
 
    public MainWindow()
    {
        this.InitializeComponent();
        
        Instance = this;
        TitleBarHelper.Initialize(this, TitleTextBlock, AppTitleBar, LeftPaddingColumn, IconColumn, TitleColumn, LeftDragColumn, SearchColumn, RightDragColumn, RightPaddingColumn);

        Localizer.Get().InitializeWindow(Root, Content);
    }

    public string SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (value == "Light")
            {
               RootShell.RequestedTheme = ElementTheme.Light;
            }
            else if (value == "Dark")
            {
                RootShell.RequestedTheme = ElementTheme.Dark;
            }
            _selectedTheme = value;
        }
    }
    private string _selectedTheme;
}

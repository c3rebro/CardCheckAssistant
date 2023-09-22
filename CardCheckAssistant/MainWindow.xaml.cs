using System.Runtime.InteropServices;
using CardCheckAssistant.Services;
using Microsoft.UI.Xaml;

namespace CardCheckAssistant;

public sealed partial class MainWindow : Window
{
    public INavigation Navigation => RootShell;
    internal static MainWindow Instance { get; private set; }
 
    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
    }

    public string SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (value == "Light")
            {
                RootShell.RequestedTheme = ElementTheme.Light;
                RootShell.UseLayoutRounding= true;
                AppTitleBar.RequestedTheme = ElementTheme.Light;
                AppTitleBar.UseLayoutRounding= true;
                AppTitleBar.CornerRadius = new Microsoft.UI.Xaml.CornerRadius(20);
            }
            else if (value == "Dark")
            {
                RootShell.RequestedTheme = ElementTheme.Dark;
                AppTitleBar.RequestedTheme = ElementTheme.Dark;
            }
            _selectedTheme = value;
        }
    }
    private string _selectedTheme;


    [StructLayout(LayoutKind.Sequential)]
    struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    [DllImport("dwmapi")]
    static extern IntPtr DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);
}

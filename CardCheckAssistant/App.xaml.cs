using System.Diagnostics;
using CardCheckAssistant.AppNotification;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppNotifications;
using WinUICommunity.Common.Helpers;


namespace CardCheckAssistant;

public partial class App : Application
{
    public Window Window => m_window;
    private Window m_window;
    private readonly NotificationManager notificationManager;

    public App()
    {
        InitializeComponent();

        try
        {
            if (!ApplicationHelper.IsPackaged)
            {
                AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
                var c_notificationHandlers = new Dictionary<int, Action<AppNotificationActivatedEventArgs>>();
                c_notificationHandlers.Add(ToastWithAvatar.Instance.ScenarioId, ToastWithAvatar.Instance.NotificationReceived);
                c_notificationHandlers.Add(ToastWithTextBox.Instance.ScenarioId, ToastWithTextBox.Instance.NotificationReceived);
                c_notificationHandlers.Add(ToastWithPayload.Instance.ScenarioId, ToastWithPayload.Instance.NotificationReceived);
                notificationManager = new NotificationManager(c_notificationHandlers);
            }
        }

        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }

        
    }

    public static FrameworkElement MainRoot { get; private set; }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        m_window = new MainWindow();
        ThemeHelper.Initialize(m_window, BackdropType.Mica);
        if (!ApplicationHelper.IsPackaged)
        {
            notificationManager.Init(notificationManager, OnNotificationInvoked);
        }

        MainRoot = m_window.Content as FrameworkElement;

        m_window.SetWindowSize(1150, 750);

        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(m_window);
        Microsoft.UI.WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
        Microsoft.UI.Windowing.AppWindow appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
        if (appWindow is not null)
        {
            Microsoft.UI.Windowing.DisplayArea displayArea = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(windowId, Microsoft.UI.Windowing.DisplayAreaFallback.Nearest);
            if (displayArea is not null)
            {
                var CenteredPosition = appWindow.Position;
                CenteredPosition.X = ((displayArea.WorkArea.Width - appWindow.Size.Width) / 2);
                CenteredPosition.Y = ((displayArea.WorkArea.Height - appWindow.Size.Height) / 2);
                appWindow.Move(CenteredPosition);
            }
        }
        m_window.Activate();
    }

    /*
     * This Method will be used later
     * to get Text Inputs..
     */
    private void OnNotificationInvoked(string message)
    {

    }

    void OnProcessExit(object sender, EventArgs e)
    {
        notificationManager.Unregister();
    }
    
}

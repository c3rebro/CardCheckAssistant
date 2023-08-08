using System.Collections.Generic;
using System;
using Microsoft.UI.Xaml;
using WinUICommunity.Common.Helpers;

using CardCheckAssistant.Services;
using CardCheckAssistant.Views;
using CardCheckAssistant.AppNotification;

using Microsoft.Windows.AppNotifications;
using WinUICommunity.Common.Tools;
using System.IO;
using System.Diagnostics;
using Microsoft.UI.Windowing;
using Microsoft.UI;
using WinRT.Interop;

namespace CardCheckAssistant;

public partial class App : Application
{
    public Window Window => m_window;
    private Window m_window;
    private NotificationManager notificationManager;

    public App()
    {
        InitializeComponent();

        try
        {
            if (!ApplicationHelper.IsPackaged)
            {
                AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
                var c_notificationHandlers = new Dictionary<int, Action<AppNotificationActivatedEventArgs>>();
                c_notificationHandlers.Add(ToastWithAvatar.Instance.ScenarioId, ToastWithAvatar.Instance.NotificationReceived);
                c_notificationHandlers.Add(ToastWithTextBox.Instance.ScenarioId, ToastWithTextBox.Instance.NotificationReceived);
                c_notificationHandlers.Add(ToastWithPayload.Instance.ScenarioId, ToastWithPayload.Instance.NotificationReceived);
                notificationManager = new NotificationManager(c_notificationHandlers);
            }
            /*
             * ILocalizer localizer = new LocalizerBuilder()
                // For a packaged app:
                //.AddResourcesStringsFolder(new LocalizerResourcesStringsFolder(@"C:/Projects/Strings"))
                // For a non-packaged app:
                .AddDefaultResourcesStringsFolder()
                .Build();
            */
            //Localizer.Set(localizer);
        }

        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }

        
    }

    public static FrameworkElement MainRoot { get; private set; }

    public INavigation Navigation;

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        m_window = new MainWindow();
        ThemeHelper.Initialize(m_window, BackdropType.Mica);
        if (!ApplicationHelper.IsPackaged)
        {
            notificationManager.Init(notificationManager, OnNotificationInvoked);
        }

        //m_window.Activate();

        MainRoot = m_window.Content as FrameworkElement;

        m_window.SetWindowSize(1000, 700);

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

    
    private void OnNotificationInvoked(string message)
    {
        //AppNotificationPage.Instance.NotificationInvoked(message);
    }

    void OnProcessExit(object sender, EventArgs e)
    {
        notificationManager.Unregister();
    }
    
}

using System.Collections.Generic;
using System;
using Microsoft.UI.Xaml;
using WinUICommunity.Common.Helpers;

using CardCheckAssistant.Services;
using CardCheckAssistant.Views;
//using CardCheckAssistant.AppNotification;

using Microsoft.Windows.AppNotifications;
using WinUICommunity.Common.Tools;
using System.IO;

namespace CardCheckAssistant;

public partial class App : Application
{
    private Shell shell;
    //private Window m_window;
    //private NotificationManager notificationManager;

    public App()
    {
        InitializeComponent();
        /*
        if (!ApplicationHelper.IsPackaged)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            var c_notificationHandlers = new Dictionary<int, Action<AppNotificationActivatedEventArgs>>();
            c_notificationHandlers.Add(ToastWithAvatar.Instance.ScenarioId, ToastWithAvatar.Instance.NotificationReceived);
            c_notificationHandlers.Add(ToastWithTextBox.Instance.ScenarioId, ToastWithTextBox.Instance.NotificationReceived);
            c_notificationHandlers.Add(ToastWithPayload.Instance.ScenarioId, ToastWithPayload.Instance.NotificationReceived);
            notificationManager = new NotificationManager(c_notificationHandlers);
        }
        ILocalizer localizer = new LocalizerBuilder()
            // For a packaged app:
            //.AddResourcesStringsFolder(new LocalizerResourcesStringsFolder(@"C:/Projects/Strings"))
            // For a non-packaged app:
            .AddDefaultResourcesStringsFolder()
            .Build();
        Localizer.Set(localizer);
        */
    }

    public static FrameworkElement MainRoot { get; private set; }

    public INavigation Navigation => shell;

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        shell = new Shell();
        ThemeHelper.Initialize(shell, BackdropType.MicaAlt);

        shell.Activate();
        MainRoot = shell.Content as FrameworkElement;
    }

    /*
    private void OnNotificationInvoked(string message)
    {
        AppNotificationPage.Instance.NotificationInvoked(message);
    }

    void OnProcessExit(object sender, EventArgs e)
    {
        notificationManager.Unregister();
    }
    */
}

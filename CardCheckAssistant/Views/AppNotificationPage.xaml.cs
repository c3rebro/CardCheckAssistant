using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using CardCheckAssistant.AppNotification;
using WinUICommunity.Common.Helpers;

namespace CardCheckAssistant.Views;

public sealed partial class AppNotificationPage : Page
{
    public static AppNotificationPage Instance
    {
        get; private set;
    }

    public AppNotificationPage()
    {
        Instance = this;
    }

    public void NotificationReceived(Notification notification)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            if (notification.HasInput)
            {
                txtReceived.Text = notification.Input;
            }
            else
            {
               txtReceived.Text = "Notification Received";
            }
        });
    }

    public void NotificationInvoked(string message)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            txtInvoked.Text = message;
        });
    }
    private void btnToast1_Click(object sender, RoutedEventArgs e)
    {
        if (!ApplicationHelper.IsPackaged)
        {
            ToastWithAvatar.Instance.SendToast();
        }
    }

    private void btnToast2_Click(object sender, RoutedEventArgs e)
    {
        if (!ApplicationHelper.IsPackaged)
        {
            ToastWithTextBox.Instance.SendToast();
        }
    }

    private void btnToast3_Click(object sender, RoutedEventArgs e)
    {
        if (!ApplicationHelper.IsPackaged)
        {
            ToastWithPayload.Instance.SendToast();
        }
    }
}

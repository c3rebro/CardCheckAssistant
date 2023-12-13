using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace CardCheckAssistant.Services
{
    /// <summary>
    /// Provides elementary Modal View services: display message, request confirmation, request input.
    /// </summary>
    public static class ModalView
    {
        public static ObservableCollection<ContentDialog> Dialogs { get; set; }

        public static async Task MessageDialogAsync(this FrameworkElement element, string title, string message)
        {
            await MessageDialogAsync(element, title, message, "OK");
        }

        public static async Task MessageDialogAsync(this FrameworkElement element, string title, string message, string buttonText)
        {
            await MessageDialogAsync(element, title, message, buttonText, "");
        }

        public static async Task MessageDialogAsync(this FrameworkElement element, string title, string message, string buttonText, string name)
        {
            if (Dialogs != null && Dialogs.Count != 0)
            {
                Dialogs.Clear();
            }

            Dialogs ??= new ObservableCollection<ContentDialog>();

            var dialog = new ContentDialog
            {
                Name = name,
                Title = title,
                Content = message,
                CloseButtonText = buttonText,
                XamlRoot = element.XamlRoot,
                RequestedTheme = element.ActualTheme
            };

            Dialogs.Add(dialog);

            await dialog.ShowAsync();

            Dialogs.Remove(dialog);
        }

        public static async Task<bool?> ConfirmationDialogAsync(this FrameworkElement element, string title, string message)
        {
            return await ConfirmationDialogAsync(element, title, message, "OK", string.Empty, "Cancel");
        }

        public static async Task<bool?> ConfirmationDialogAsync(this FrameworkElement element, string title, string message, string yesButtonText, string noButtonText)
        {
            return (await ConfirmationDialogAsync(element, title, message, yesButtonText, noButtonText, string.Empty)).Value;
        }

        public static async Task<bool?> ConfirmationDialogAsync(this FrameworkElement element, string title, string message, string yesButtonText, string noButtonText, string cancelButtonText)
        {
            Dialogs ??= new ObservableCollection<ContentDialog> { };

            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                PrimaryButtonText = yesButtonText,
                SecondaryButtonText = noButtonText,
                CloseButtonText = cancelButtonText,
                XamlRoot = element.XamlRoot,
                RequestedTheme = element.ActualTheme
            };

            Dialogs.Add(dialog);

            var result = await dialog.ShowAsync();

            Dialogs.Remove(dialog);

            if (result == ContentDialogResult.None)
            {
                return null;
            }

            return (result == ContentDialogResult.Primary);
        }

        public static async Task<string> InputStringDialogAsync(this FrameworkElement element, string title)
        {
            return await element.InputStringDialogAsync(title, string.Empty);
        }

        public static async Task<string> InputStringDialogAsync(this FrameworkElement element, string title, string defaultText)
        {
            return await element.InputStringDialogAsync(title, defaultText, "OK", "Cancel");
        }

        public static async Task<string> InputStringDialogAsync(this FrameworkElement element, string title, string defaultText, string okButtonText, string cancelButtonText)
        {
            Dialogs ??= new ObservableCollection<ContentDialog>();

            var inputTextBox = new TextBox
            {
                AcceptsReturn = false,
                Height = 32,
                Text = defaultText,
                SelectionStart = defaultText.Length
            };
            var dialog = new ContentDialog
            {
                Content = inputTextBox,
                Title = title,
                IsSecondaryButtonEnabled = true,
                PrimaryButtonText = okButtonText,
                SecondaryButtonText = cancelButtonText,
                XamlRoot = element.XamlRoot,
                RequestedTheme = element.ActualTheme
            };

            Dialogs.Add(dialog);

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                Dialogs.Remove(dialog);
                return inputTextBox.Text;
            }
            else
            {
                Dialogs.Remove(dialog);
                return string.Empty;
            }
        }

        public static async Task<string> InputTextDialogAsync(this FrameworkElement element, string title)
        {
            return await element.InputTextDialogAsync(title, string.Empty);
        }

        public static async Task<string> InputTextDialogAsync(this FrameworkElement element, string title, string defaultText)
        {
            Dialogs ??= new ObservableCollection<ContentDialog>();

            var inputTextBox = new TextBox
            {
                AcceptsReturn = true,
                Height = 32 * 6,
                Text = defaultText,
                TextWrapping = TextWrapping.Wrap,
                SelectionStart = defaultText.Length
            };
            var dialog = new ContentDialog
            {
                Content = inputTextBox,
                Title = title,
                IsSecondaryButtonEnabled = true,
                PrimaryButtonText = "Ok",
                SecondaryButtonText = "Cancel",
                XamlRoot = element.XamlRoot,
                RequestedTheme = element.ActualTheme
            };

            Dialogs.Add(dialog);

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                Dialogs.Remove(dialog);
                return inputTextBox.Text;
            }
            else
            {
                Dialogs.Remove(dialog);
                return string.Empty;
            }
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using CardCheckAssistant.Models;
using CardCheckAssistant.Services;
using System.Collections.ObjectModel;
using System;
using System.Linq;
using CardCheckAssistant.Views;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json.Linq;
using Windows.Storage.Pickers;

namespace CardCheckAssistant.ViewModels;

public class SettingsPageViewModel : ObservableObject
{
    public SettingsPageViewModel()
    {
        ThemeSource = new ObservableCollection<string>();
        ThemeSource.Add("Light");
        ThemeSource.Add("Dark");
        ThemeSource.Add("Default");

        //SelectedTheme.Content = "Light";
        using (SettingsReaderWriter settings = new SettingsReaderWriter())
        {
            var m_window = (Application.Current as App)?.Window as MainWindow;

            SelectedTheme = settings.DefaultSpecification.DefaultTheme;
        }
    }

    #region ObservableObjects

    public ObservableCollection<string> ThemeSource
    {
        get => _themeSource;
        set
        {
            SetProperty(ref _themeSource, value);
        }
    }
    private ObservableCollection<string> _themeSource;


    public string SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            SetProperty(ref _selectedTheme, value);
            var m_window = (Application.Current as App)?.Window as MainWindow;

            m_window.SelectedTheme = value;

            using SettingsReaderWriter settings = new SettingsReaderWriter();

            settings.DefaultSpecification.DefaultTheme = value;
            settings.SaveSettings();
        }
    }
    private string _selectedTheme;

    #endregion 

    #region Commands

    public ICommand NavigateNextStepCommand => new RelayCommand(NavigateNextStepCommand_Executed);

    public ICommand NavigateBackCommand => new RelayCommand(NavigateBackCommand_Executed);

    public ICommand InputStringCommand => new AsyncRelayCommand(InputString_Executed);

    public ICommand SelectRFIDGearExe => new AsyncRelayCommand(SelectRFIDGearExe_Executed);

    #endregion

    private async Task ConfirmationYesNo_Executed()
    {
        Debug.WriteLine("2-State Confirmation Dialog will be opened.");
        var confirmed = await App.MainRoot.ConfirmationDialogAsync(
                "What Pantone color do you prefer?",
                "Freedom Blue",
                "Energizing Yellow"
            );
        Debug.WriteLine($"2-State Confirmation Dialog was closed with {confirmed}.");
    }

    private async Task ConfirmationYesNoCancel_Executed()
    {
        Debug.WriteLine("3-State Confirmation Dialog will be opened.");
        var confirmed = await App.MainRoot.ConfirmationDialogAsync(
                "Is it wise to use artillery against a nuclear power plant?",
                "да",
                "That's insane",
                "I don't understand"
            );
        Debug.WriteLine($"3-State Confirmation Dialog was closed with {confirmed}.");
    }

    private async Task InputString_Executed()
    {
        Debug.WriteLine("Opening String Input Dialog.");
        var inputString = await App.MainRoot.InputStringDialogAsync(
                "How can we help you?",
                "I need ammunition, not a ride.",
                "OK",
                "Forget it"
            );
        Debug.WriteLine($"String Input Dialog was closed with '{inputString}'.");
    }

    private async Task SelectRFIDGearExe_Executed()
    {
        var window = (Application.Current as App)?.Window as MainWindow;
        var navigation = window.Navigation;
        var step2Page = navigation.GetNavigationViewItems(typeof(Step2Page)).First();
        navigation.SetCurrentNavigationViewItem(step2Page);
        step2Page.IsEnabled = true;
        /*
        var inputString = await App.MainRoot.InputStringDialogAsync(
        "How can we help you?",
        "I need ammunition, not a ride.",
        "OK",
        "Forget it");
        */

        var filePicker = new FileOpenPicker();

        // Get the current window's HWND by passing in the Window object
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

        // Associate the HWND with the file picker
        WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);

        filePicker.FileTypeFilter.Add("*");
        var file = await filePicker.PickSingleFileAsync();
    }

    private void NavigateNextStepCommand_Executed()
    {
        var window = (Application.Current as App)?.Window as MainWindow;
        var navigation = window.Navigation;
        var step2Page = navigation.GetNavigationViewItems(typeof(Step2Page)).First();
        navigation.SetCurrentNavigationViewItem(step2Page);
        step2Page.IsEnabled = true;
    }

    private void NavigateBackCommand_Executed()
    {
        var window = (Application.Current as App)?.Window as MainWindow;
        var navigation = window.Navigation;
        var step1Page = navigation.GetNavigationViewItems(typeof(Step1Page)).First();
        navigation.SetCurrentNavigationViewItem(step1Page);
        step1Page.IsEnabled = true;
    }

}

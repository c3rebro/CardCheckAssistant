using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using CardCheckAssistant.Models;
using CardCheckAssistant.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using CardCheckAssistant.Views;
using Windows.Storage.Pickers;
using CommunityToolkit.Common;

namespace CardCheckAssistant.ViewModels;

public class Step1PageViewModel : ObservableObject, IDisposable
{

    private readonly DispatcherTimer scanChipTimer;

    public Step1PageViewModel()
    {
        scanChipTimer = new DispatcherTimer();
        scanChipTimer.Tick += ScanChipEvent;
        scanChipTimer.Interval = new TimeSpan(0,0,0,0,3000);

        Languages = new ObservableCollection<string>
        {
            "Deutsch" 
        };

        SelectedReportLaguage = Languages.FirstOrDefault();

        NextStepCanExecute = false;
        GoBackCanExecute = true;
        HasTwoReadersInfoBarIsVisible = false;

        ScanChipEvent(this, null);
        scanChipTimer.Start();
    }

    #region ObservableObjects

    public ObservableCollection<string> Languages
    {
        get; set;
    }

    public bool HasTwoReadersInfoBarIsVisible
    {
        get => hasTwoReadersInfoBarIsVisible;
        set => SetProperty(ref hasTwoReadersInfoBarIsVisible, value);
    }
    private bool hasTwoReadersInfoBarIsVisible;

    public bool NoChipDetectedInfoBarIsVisible
    {
        get => noChipDetectedInfoBarIsVisible;
        set => SetProperty(ref noChipDetectedInfoBarIsVisible, value);
    }
    private bool noChipDetectedInfoBarIsVisible;

    public bool ChipDetectedInfoBarIsVisible
    {
        get => chipDetectedInfoBarIsVisible;
        set => SetProperty(ref chipDetectedInfoBarIsVisible, value);
    }
    private bool chipDetectedInfoBarIsVisible;

    public string ChipInfoMessage
    {
        get => chipInfoMessage;
        set => SetProperty(ref chipInfoMessage, value);
    }
    private string chipInfoMessage;

    public async void InputText_Click(object sender, RoutedEventArgs e)
    {
        /*
        Debug.WriteLine("Opening Text Input Dialog.");
        var inputText = await App.MainRoot.InputTextDialogAsync(
                "What would Faramir say?",
                "“War must be, while we defend our lives against a destroyer who would devour all; but I do not love the bright sword for its sharpness, nor the arrow for its swiftness, nor the warrior for his glory. I love only that which they defend.”\n\nJ.R.R. Tolkien"
            );

        Debug.WriteLine($"Text Input Dialog was closed with {inputText}.");
        */
    }

    public bool NextStepCanExecute
    {
        get => _nextStepCanExecute;
        set => SetProperty(ref _nextStepCanExecute, value);
    }
    private bool _nextStepCanExecute;

    public bool GoBackCanExecute
    {
        get => _goBackCanExecute;
        set => SetProperty(ref _goBackCanExecute, value);
    }
    private bool _goBackCanExecute;

    public string JobNumber => string.Format("JobNr.: {0}; ChipNummer: {1}; Kunde: {2}",CheckProcessService.CurrentCardCheckProcess.JobNr, CheckProcessService.CurrentCardCheckProcess.ChipNumber, CheckProcessService.CurrentCardCheckProcess.CName);

    public string SelectedReportLaguage
    {
        get => _selectedReportLaguage;
        set
        {
            CheckProcessService.CurrentCardCheckProcess.ReportLanguage = value;
            SetProperty(ref _selectedReportLaguage, value);
        }
    }
    private string _selectedReportLaguage;

    #endregion 

    #region Commands

    public ICommand NavigateNextStepCommand => new AsyncRelayCommand(NavigateNextStepCommand_Executed);

    public ICommand NavigateBackCommand => new RelayCommand(NavigateBackCommand_Executed);

    public ICommand InputStringCommand => new AsyncRelayCommand(InputString_Executed);

    #endregion


    private async void ScanChipEvent(object? sender, object e)
    {
        try
        {
            using (ReaderService readerService = new ReaderService())
            {
                await readerService.ReadChipPublic();

                if (readerService.MoreThanOneReaderFound)
                {
                    HasTwoReadersInfoBarIsVisible = true;
                    ChipDetectedInfoBarIsVisible = false;
                    NoChipDetectedInfoBarIsVisible = false;

                    NextStepCanExecute = false;
                }
                else
                {
                    HasTwoReadersInfoBarIsVisible = false;

                    if (readerService.GenericChip != null)
                    {
                        NoChipDetectedInfoBarIsVisible = false;
                        ChipDetectedInfoBarIsVisible = true;

                        NextStepCanExecute = true;

                        ChipInfoMessage = string.Format("Es wurde ein Chip erkannt:\nErkannt 1: {0}", readerService.GenericChip.CardType.ToString());

                        if(readerService.GenericChip.Child != null)
                        {
                            ChipInfoMessage = ChipInfoMessage + string.Format("\nErkannt 2: {0}", readerService.GenericChip.Child.CardType);
                        }
                    }

                    else
                    {
                        ChipDetectedInfoBarIsVisible = false;
                        NoChipDetectedInfoBarIsVisible = true;

                        NextStepCanExecute  = true;
                    }
                }


            }
        }
        catch
        {

        }
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
    
    private async Task NavigateNextStepCommand_Executed()
    {
        scanChipTimer.Stop();

        var window = (Application.Current as App)?.Window as MainWindow;
        var navigation = window.Navigation;
        var step2Page = navigation.GetNavigationViewItems(typeof(Step2Page)).First();
        navigation.SetCurrentNavigationViewItem(step2Page);
        step2Page.IsEnabled = true;
    }

    private void NavigateBackCommand_Executed()
    {
        scanChipTimer.Stop();

        var window = (Application.Current as App)?.Window as MainWindow;
        var navigation = window.Navigation;
        var homePage = navigation.GetNavigationViewItems(typeof(HomePage)).First();
        navigation.SetCurrentNavigationViewItem(homePage);

        this.Dispose();
        //Step1Page.IsEnabled = true;
    }

    protected void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        _disposed = false;
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    private bool _disposed;

}

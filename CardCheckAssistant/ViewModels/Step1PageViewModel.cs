using CardCheckAssistant.Services;
using CardCheckAssistant.Views;

using System.Collections.ObjectModel;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Elatec.NET;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Documents;

using Log4CSharp;

using System.Diagnostics;
using CardCheckAssistant.Contracts.ViewModels;


namespace CardCheckAssistant.ViewModels;

public partial class Step1PageViewModel : ObservableRecipient, INavigationAware
{
    /// <summary>
    /// 
    /// </summary>
    private readonly DispatcherTimer scanChipTimer;

    public Step1PageViewModel()
    {
        try
        {
            Process[] processlist = Process.GetProcesses();

            foreach (Process p in processlist)
            {
                Console.WriteLine("Process: {0} ID: {1}", p.ProcessName, p.Id);
            }
        }
        catch (Exception ex)
        {

        }

        scanChipTimer = new DispatcherTimer();
        scanChipTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000);
        scanChipTimer.Stop();

        WaitForNextStep = false;
        AskClassicKeysIsVisible = false;
        AskPICCMKIsVisible = false;

        Languages = new ObservableCollection<string>
        {
            "Deutsch"
        };

        SelectedReportLaguage = Languages.FirstOrDefault() ?? "de";

        NextStepCanExecute = false;
        GoBackCanExecute = true;
        HasTwoReadersInfoBarIsVisible = false;
    }

    #region ObservableObjects

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _languages;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private bool? _readerAccessDenied;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private bool? _waitForNextStep;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private bool? _askPICCMKIsVisible;

    [ObservableProperty]
    private bool? _askClassicKeysIsVisible;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private bool? _noReaderFound;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private bool _hasTwoReadersInfoBarIsVisible;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private bool _noChipDetectedInfoBarIsVisible;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private bool _chipDetectedInfoBarIsVisible;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private string _chipInfoMessage;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private bool _nextStepCanExecute;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private bool _goBackCanExecute;

    /// <summary>
    /// 
    /// </summary>
    public string JobNumber => string.Format("JobNr.: {0}; ChipNummer: {1}; Kunde: {2}", CheckProcessService.CurrentCardCheckProcess.JobNr, CheckProcessService.CurrentCardCheckProcess.ChipNumber, CheckProcessService.CurrentCardCheckProcess.CName);

    /// <summary>
    /// 
    /// </summary>
    public string SelectedReportLaguage
    {
        get => _selectedReportLaguage ?? "Deutsch";
        set
        {
            CheckProcessService.CurrentCardCheckProcess.ReportLanguage = value;
            SetProperty(ref _selectedReportLaguage, value);
        }
    }
    private string? _selectedReportLaguage;

    #endregion

    #region Commands

    /// <summary>
    /// 
    /// </summary>
    public ICommand PostPageLoadedCommand => new AsyncRelayCommand(PostPageLoadedCommand_Executed);

    /// <summary>
    /// 
    /// </summary>
    public IAsyncRelayCommand NavigateNextStepCommand => new AsyncRelayCommand(NavigateNextStepCommand_Executed);

    /// <summary>
    /// 
    /// </summary>
    public ICommand NavigateBackCommand => new RelayCommand(NavigateBackCommand_Executed);

    #endregion

    #region Extension Methods

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private async Task PostPageLoadedCommand_Executed()
    {
        try
        {
            scanChipTimer.Tick += ScanChipEvent;
            scanChipTimer.Start();
        }
        catch (Exception e)
        {
            LogWriter.CreateLogEntry(e);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ScanChipEvent(object? _, object? e)
    {
        try
        {
            scanChipTimer.Stop();

            using (ReaderService readerService = new ReaderService())
            {
                // ReadChipPublic != 0  is an Error
                ReaderAccessDenied = await readerService.ReadChipPublic() < 0;

                if (ReaderAccessDenied == true)
                {
                    NextStepCanExecute = false;
                    scanChipTimer.Start();
                }

                if (!TWN4ReaderDevice.Instance?.IsConnected == true)
                {
                    NoReaderFound = true;
                }

                else if (readerService?.MoreThanOneReaderFound == true)
                {
                    HasTwoReadersInfoBarIsVisible = true;
                    ChipDetectedInfoBarIsVisible = false;
                    NoChipDetectedInfoBarIsVisible = false;

                    NextStepCanExecute = false;
                }

                else
                {
                    HasTwoReadersInfoBarIsVisible = false;
                    NoReaderFound = false;

                    if (readerService.GenericChip != null)
                    {
                        NoChipDetectedInfoBarIsVisible = false;
                        ChipDetectedInfoBarIsVisible = true;

                        if (readerService.GenericChip.CardType.ToString().ToLower().Contains("mifare"))
                        {
                            AskClassicKeysIsVisible = true;
                            AskPICCMKIsVisible = false;
                        }
                        else if (readerService.GenericChip.CardType.ToString().ToLower().Contains("desfire"))
                        {
                            AskClassicKeysIsVisible = false;
                            AskPICCMKIsVisible = true;
                        }

                        NextStepCanExecute = true;

                        ChipInfoMessage = string.Format("Es wurde ein Chip erkannt:\nErkannt 1: {0}", readerService.GenericChip.CardType.ToString());

                        if (readerService.GenericChip.Child != null)
                        {
                            ChipInfoMessage = ChipInfoMessage + string.Format("\nErkannt 2: {0}", readerService.GenericChip.Child.CardType);
                        }
                    }

                    else
                    {
                        ChipDetectedInfoBarIsVisible = false;
                        NoChipDetectedInfoBarIsVisible = true;

                        AskClassicKeysIsVisible = false;
                        AskPICCMKIsVisible = false;

                        NextStepCanExecute = true;
                    }
                }


            }
            scanChipTimer.Start();
        }
        catch (Exception ex)
        {
            LogWriter.CreateLogEntry(ex);
        }
    }

    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private async Task NavigateNextStepCommand_Executed()
    {
        try
        {
            using SettingsReaderWriter settings = new SettingsReaderWriter();

            settings.ReadSettings();

            FileInfo finalPath = new FileInfo(
                settings.DefaultSettings.DefaultProjectOutputPath + "\\"
                + (settings.DefaultSettings.CreateSubdirectoryIsEnabled == true ? CheckProcessService.CurrentCardCheckProcess.JobNr + "\\" : string.Empty)
                + CheckProcessService.CurrentCardCheckProcess.JobNr + "-"
                + CheckProcessService.CurrentCardCheckProcess.ChipNumber
                + "_final.pdf");

            FileInfo semiFinalPath = new FileInfo(
                settings.DefaultSettings.DefaultProjectOutputPath + "\\"
                + (settings.DefaultSettings.CreateSubdirectoryIsEnabled == true ? CheckProcessService.CurrentCardCheckProcess.JobNr + "\\" : string.Empty)
                + CheckProcessService.CurrentCardCheckProcess.JobNr + "-"
                + CheckProcessService.CurrentCardCheckProcess.ChipNumber
                + "_.pdf");

            FileInfo preFinalPath = new FileInfo(
                settings.DefaultSettings.DefaultProjectOutputPath + "\\"
                + (settings.DefaultSettings.CreateSubdirectoryIsEnabled == true ? CheckProcessService.CurrentCardCheckProcess.JobNr + "\\" : string.Empty)
                + CheckProcessService.CurrentCardCheckProcess.JobNr + "-"
                + CheckProcessService.CurrentCardCheckProcess.ChipNumber
                + ".pdf");

            if (preFinalPath.Exists || semiFinalPath.Exists || finalPath.Exists)
            {
                if (await App.MainRoot.ConfirmationDialogAsync(
                        "Warnung",
                        "Die Datei, die erstellt werden soll, existiert bereits.\n" +
                        "Soll sie überschrieben werden?",
                        "Ja", "Nein") == true)
                {
                    try
                    {
                        // try to overwrite the file
                        FileStream fs = preFinalPath.Open(FileMode.Create);
                        fs.Close();
                        preFinalPath.Delete();

                        fs = semiFinalPath.Open(FileMode.Create);
                        fs.Close();
                        semiFinalPath.Delete();

                        fs = finalPath.Open(FileMode.Create);
                        fs.Close();
                        finalPath.Delete();
                    }
                    catch (IOException ioex)
                    {
                        await App.MainRoot.MessageDialogAsync(
                        "Fehler",
                        string.Format("Windows Fehlertext: {0}\n\n" +
                        "Bitte beende die Anwendung die auf diese Datei zugreift und versuche es im Anschluss erneut.", ioex.Message),
                        "OK");

                        return;
                    }
                    finally
                    {
                        LogWriter.CreateLogEntry("ioex");
                    }
                }
                else
                {
                    return;
                }
            }

            NextStepCanExecute = false;
            WaitForNextStep = true;

            scanChipTimer.Stop();
            scanChipTimer.Tick -= ScanChipEvent;

            await Task.Delay(1000);

            (App.MainRoot.XamlRoot.Content as ShellPage)?.ViewModel.NavigationService.NavigateTo(typeof(Step2PageViewModel).FullName ?? "");
        }

        catch (Exception e)
        {
            LogWriter.CreateLogEntry(e);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void NavigateBackCommand_Executed()
    {
        try
        {
            scanChipTimer.Stop();
            scanChipTimer.Tick -= ScanChipEvent;

            (App.MainRoot.XamlRoot.Content as ShellPage)?.ViewModel.NavigationService.NavigateTo(typeof(HomePageViewModel).FullName ?? "");
        }
        catch (Exception e)
        {
            LogWriter.CreateLogEntry(e);
        }

    }

    public void OnNavigatedTo(object parameter)
    {
        // Run code when the app navigates to this page
    }

    public void OnNavigatedFrom()
    {
        // Run code when the app navigates away from this page
    }

    protected void Dispose(bool disposing)
    {
        if (!_disposed)
        {
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

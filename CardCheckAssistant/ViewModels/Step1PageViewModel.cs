using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;
using CardCheckAssistant.Contracts.ViewModels;
using CardCheckAssistant.Services;
using CardCheckAssistant.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Elatec.NET;
using Microsoft.UI.Xaml;


namespace CardCheckAssistant.ViewModels;

public partial class Step1PageViewModel : ObservableRecipient, INavigationAware
{
    private readonly EventLog eventLog = new("Application", ".", Assembly.GetEntryAssembly().GetName().Name);

    /// <summary>
    /// 
    /// </summary>
    private readonly DispatcherTimer scanChipTimer;

    public Step1PageViewModel()
    {
        try
        {
            var processlist = Process.GetProcesses();

            foreach (var p in processlist)
            {
                Console.WriteLine("Process: {0} ID: {1}", p.ProcessName, p.Id);
            }
        }
        catch (Exception ex)
        {
            //TODO: Let the User know if MADA SW is running
        }

        scanChipTimer = new DispatcherTimer();
        scanChipTimer.Interval = new TimeSpan(0, 00, 0, 0, 1000);
        scanChipTimer.Stop();

        WaitForNextStep = false;
        AskClassicKeysIsVisible = false;
        AskPICCMKIsVisible = false;

        Languages = new ObservableCollection<string>
        {
            "Deutsch",
            "Englisch"
        };

        SelectedReportLaguage = Languages.FirstOrDefault() ?? "englisch";

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
    public IAsyncRelayCommand NavigateBackCommand => new AsyncRelayCommand(NavigateBackCommand_Executed);

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
            eventLog.WriteEntry(e.Message, EventLogEntryType.Error);
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

            await UpdateChip();

            scanChipTimer.Start();
        }
        catch (Exception ex)
        {
            eventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
        }
    }

    private async Task UpdateChip()
    {
        using var readerService = new ReaderService();

        try
        {
            // ReadChipPublic != 0 is an Error
            await readerService.ReadChipPublic();

            if (readerService.IsConnected == null)
            {
                NextStepCanExecute = false;
                NoReaderFound = true;
                scanChipTimer.Start();

                return;
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

                var detectedChips = readerService.DetectedChips;

                if (detectedChips != null && detectedChips.Count > 0)
                {
                    AskClassicKeysIsVisible = false;
                    AskPICCMKIsVisible = false;

                    ChipInfoMessage = "Es wurden folgende Chips erkannt:";

                    foreach (var chip in detectedChips)
                    {
                        ChipInfoMessage += $"\n{chip.ChipType}";

                        if (ByteArrayConverter.IsMifareClassic(chip.ChipType))
                        {
                            AskClassicKeysIsVisible = true;
                        }
                        else if (ByteArrayConverter.IsMifareDesfire(chip.ChipType))
                        {
                            AskPICCMKIsVisible = true;
                        }
                    }

                    NextStepCanExecute = true;
                    ChipDetectedInfoBarIsVisible = true;
                    NoChipDetectedInfoBarIsVisible = false;
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
        catch
        {
            NextStepCanExecute = false;
            NoReaderFound = true;

            HasTwoReadersInfoBarIsVisible = false;
            ChipDetectedInfoBarIsVisible = false;
            NoChipDetectedInfoBarIsVisible = false;
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
            using var settings = new SettingsReaderWriter();

            scanChipTimer.Stop();
            scanChipTimer.Tick -= ScanChipEvent;

            settings.ReadSettings();
            settings.DefaultSettings.DefaultReportLanguage = SelectedReportLaguage;
            settings.SaveSettings();

            var finalPath = new FileInfo(
                settings.DefaultSettings.DefaultProjectOutputPath + "\\"
                + (settings.DefaultSettings.CreateSubdirectoryIsEnabled == true ? CheckProcessService.CurrentCardCheckProcess.JobNr + "\\" : string.Empty)
                + CheckProcessService.CurrentCardCheckProcess.JobNr + "-"
                + CheckProcessService.CurrentCardCheckProcess.ChipNumber
                + "_final.pdf");

            var semiFinalPath = new FileInfo(
                settings.DefaultSettings.DefaultProjectOutputPath + "\\"
                + (settings.DefaultSettings.CreateSubdirectoryIsEnabled == true ? CheckProcessService.CurrentCardCheckProcess.JobNr + "\\" : string.Empty)
                + CheckProcessService.CurrentCardCheckProcess.JobNr + "-"
                + CheckProcessService.CurrentCardCheckProcess.ChipNumber
                + "_.pdf");

            var preFinalPath = new FileInfo(
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
                        var fs = preFinalPath.Open(FileMode.Create);
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
                        eventLog.WriteEntry("IO.EXception", EventLogEntryType.Error);
                    }
                }
                else
                {
                    return;
                }
            }

            NextStepCanExecute = false;
            WaitForNextStep = true;

            (App.MainRoot.XamlRoot.Content as ShellPage)?.ViewModel.NavigationService.NavigateTo(typeof(Step2PageViewModel).FullName ?? "");
        }

        catch (Exception e)
        {
            eventLog.WriteEntry(e.Message, EventLogEntryType.Error);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private async Task NavigateBackCommand_Executed()
    {
        try
        {
            scanChipTimer.Stop();
            scanChipTimer.Tick -= ScanChipEvent;

            (App.MainRoot.XamlRoot.Content as ShellPage)?.ViewModel.NavigationService.NavigateTo(typeof(HomePageViewModel).FullName ?? "");
        }
        catch (Exception e)
        {
            eventLog.WriteEntry(e.Message, EventLogEntryType.Error);
        }

    }

    /// <summary>
    /// INavigation Aware Event. Close Connection If Open
    /// </summary>
    /// <param name="parameter"></param>
    public async void OnNavigatedTo(object parameter)
    {
        // Run code when the app navigates to this page
        scanChipTimer.Stop();
        scanChipTimer.Tick -= ScanChipEvent;

        if (TWN4ReaderDevice.Instance?.Count > 0 && TWN4ReaderDevice.Instance[0] != null)
        {
            await TWN4ReaderDevice.Instance[0].DisconnectAsync();
        }

    }

    /// <summary>
    /// INavigation Aware Event. Close Connection If Open
    /// </summary>
    public async void OnNavigatedFrom()
    {
        // Run code when the app navigates away from this page
        scanChipTimer.Stop();
        scanChipTimer.Tick -= ScanChipEvent;

        if (TWN4ReaderDevice.Instance?.Count > 0 && TWN4ReaderDevice.Instance[0] != null)
        {
            await TWN4ReaderDevice.Instance[0].DisconnectAsync();
        }
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

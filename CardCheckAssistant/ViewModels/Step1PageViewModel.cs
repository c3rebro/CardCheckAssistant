using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using CardCheckAssistant.Services;
using CardCheckAssistant.Views;

using Log4CSharp;

using System.Diagnostics;
using System.Windows.Input;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Documents;

namespace CardCheckAssistant.ViewModels;

/// <summary>
/// 
/// </summary>
public class Step1PageViewModel : ObservableObject, IDisposable
{
    /// <summary>
    /// 
    /// </summary>
    private readonly DispatcherTimer scanChipTimer;

    /// <summary>
    /// 
    /// </summary>
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
        scanChipTimer.Interval = new TimeSpan(0,0,0,0,1000);
        scanChipTimer.Stop();

        WaitForNextStep = false;

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
    public ObservableCollection<string> Languages
    {
        get; set;
    }

    /// <summary>
    /// 
    /// </summary>
    public bool ReaderAccessDenied
    {
        get => _readerAccessDenied;
        set => SetProperty(ref _readerAccessDenied, value);
    }
    private bool _readerAccessDenied;

    /// <summary>
    /// 
    /// </summary>
    public bool WaitForNextStep
    {
        get => waitForNextStep;
        set => SetProperty(ref waitForNextStep, value);
    }
    private bool waitForNextStep;

    /// <summary>
    /// 
    /// </summary>
    public bool HasTwoReadersInfoBarIsVisible
    {
        get => hasTwoReadersInfoBarIsVisible;
        set => SetProperty(ref hasTwoReadersInfoBarIsVisible, value);
    }
    private bool hasTwoReadersInfoBarIsVisible;

    /// <summary>
    /// 
    /// </summary>
    public bool NoChipDetectedInfoBarIsVisible
    {
        get => noChipDetectedInfoBarIsVisible;
        set => SetProperty(ref noChipDetectedInfoBarIsVisible, value);
    }
    private bool noChipDetectedInfoBarIsVisible;

    /// <summary>
    /// 
    /// </summary>
    public bool ChipDetectedInfoBarIsVisible
    {
        get => chipDetectedInfoBarIsVisible;
        set => SetProperty(ref chipDetectedInfoBarIsVisible, value);
    }
    private bool chipDetectedInfoBarIsVisible;

    /// <summary>
    /// 
    /// </summary>
    public string ChipInfoMessage
    {
        get => chipInfoMessage;
        set => SetProperty(ref chipInfoMessage, value);
    }
    private string chipInfoMessage;

    /// <summary>
    /// 
    /// </summary>
    public bool NextStepCanExecute
    {
        get => _nextStepCanExecute;
        set => SetProperty(ref _nextStepCanExecute, value);
    }
    private bool _nextStepCanExecute;

    /// <summary>
    /// 
    /// </summary>
    public bool GoBackCanExecute
    {
        get => _goBackCanExecute;
        set => SetProperty(ref _goBackCanExecute, value);
    }
    private bool _goBackCanExecute;

    /// <summary>
    /// 
    /// </summary>
    public string JobNumber => string.Format("JobNr.: {0}; ChipNummer: {1}; Kunde: {2}",CheckProcessService.CurrentCardCheckProcess.JobNr, CheckProcessService.CurrentCardCheckProcess.ChipNumber, CheckProcessService.CurrentCardCheckProcess.CName);

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
    public ICommand NavigateNextStepCommand => new RelayCommand(NavigateNextStepCommand_Executed);

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

                if (ReaderAccessDenied)
                {
                    NextStepCanExecute = false;
                    scanChipTimer.Start();
                }

                else if (readerService.MoreThanOneReaderFound)
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
            scanChipTimer.Start();
        }
        catch(Exception ex)
        {
            LogWriter.CreateLogEntry(ex);
        }
    }

    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private async void NavigateNextStepCommand_Executed()
    {
        try
        {
            using SettingsReaderWriter settings = new SettingsReaderWriter();

            settings.ReadSettings();

            FileInfo fi = new FileInfo(settings.DefaultSettings.DefaultProjectOutputPath + "\\"
                + (settings.DefaultSettings.CreateSubdirectoryIsEnabled == true ? CheckProcessService.CurrentCardCheckProcess.JobNr + "\\" : string.Empty)
                + CheckProcessService.CurrentCardCheckProcess.JobNr + "-"
                + CheckProcessService.CurrentCardCheckProcess.ChipNumber
                + ".pdf");

            if (fi.Exists)
            {
                if (await App.MainRoot.ConfirmationDialogAsync(
                        "Warnung",
                        string.Format("Die Datei die erstellt werden soll existiert bereits.\n" +
                        "Soll sie überschrieben werden?"),
                        "Ja", "Nein") == true)
                {
                    try
                    {
                        // try to overwrite the file
                        FileStream fs = fi.Open(FileMode.Create);
                        fs.Close();
                        fi.Delete();
                    }
                    catch (IOException ioex)
                    {
                        await App.MainRoot.MessageDialogAsync(
                        "Fehler",
                        string.Format("Windows Fehlertext: {0}\n\n" +
                        "Bitte beende die Anwendung die auf diese Datei zugreift und versuche es im Anschluss erneut.",ioex.Message),
                        "OK");

                        return;
                    }
                    finally {
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

            var window = (Application.Current as App)?.Window as MainWindow ?? new MainWindow();
            var navigation = window.Navigation;
            var step2Page = navigation.GetNavigationViewItems(typeof(Step2Page)).First();
            navigation.SetCurrentNavigationViewItem(step2Page);
            step2Page.IsEnabled = true;
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

            var window = (Application.Current as App)?.Window as MainWindow ?? new MainWindow();
            var navigation = window.Navigation;
            var homePage = navigation.GetNavigationViewItems(typeof(HomePage)).First();
            navigation.SetCurrentNavigationViewItem(homePage);
        }
        catch(Exception e)
        {
            LogWriter.CreateLogEntry(e);
        }

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

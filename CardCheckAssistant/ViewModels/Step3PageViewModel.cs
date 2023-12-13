using CardCheckAssistant.Models;
using CardCheckAssistant.Services;
using CardCheckAssistant.Views;

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml.Controls;

using Log4CSharp;

namespace CardCheckAssistant.ViewModels;

public partial class Step3PageViewModel : ObservableRecipient
{
    public Step3PageViewModel()
    {
        RunRFiDGearCommand = new AsyncRelayCommand(ExecuteRFIDGearCommand);
        NavigateNextStepCommand = new AsyncRelayCommand(NavigateNextStepCommand_Executed);
        PostPageLoadedCommand = new AsyncRelayCommand(PostPageLoadedCommand_Executed);

        NextStepCanExecute = false;
        GoBackCanExecute = true;

        TextBlockCheckFinishedAndResultIsSuppOnlyIsVisible = false;
        TextBlockCheckFinishedAndResultIsSuppAndProgIsVisible = false;
        TextBlockCheckNotYetFinishedIsVisible = true;
        TextBlockCheckFinishedIsVisible = false;
        HyperlinkButtonReportIsVisible = false;

        ChipCount = new ObservableCollection<string>();

        NextStepButtonContent = "Weiter";
    }

    #region ObservableProperties

    /// <summary>
    /// 
    /// </summary>
    public string NextStepButtonContent
    {
        get => _nextStepButtonContent ?? "";
        set => SetProperty(ref _nextStepButtonContent, value);
    }
    private string? _nextStepButtonContent;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private bool _textBlockCheckFinishedAndResultIsSuppAndProgIsVisible;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private bool _hyperlinkButtonReportIsVisible;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private bool _textBlockCheckFinishedIsVisible;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private bool _textBlockCheckNotYetFinishedIsVisible;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private bool _textBlockCheckFinishedAndResultIsSuppOnlyIsVisible;

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
    public string ReportLanguage => string.Format("{0}", CheckProcessService.CurrentCardCheckProcess.ReportLanguage);

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _chipCount;
    #endregion

    #region Commands

    /// <summary>
    /// 
    /// </summary>
    public IAsyncRelayCommand NavigateNextStepCommand
    {
        get;
    }

    /// <summary>
    /// 
    /// </summary>
    public IAsyncRelayCommand RunRFiDGearCommand
    {
        get;
    }

    /// <summary>
    /// 
    /// </summary>
    public ICommand NavigateBackCommand => new RelayCommand(NavigateBackCommand_Executed);

    /// <summary>
    /// 
    /// </summary>
    public IAsyncRelayCommand PostPageLoadedCommand
    {
        get;
    }

    /// <summary>
    /// 
    /// </summary>
    public IAsyncRelayCommand OpenReportCommand => new AsyncRelayCommand(OpenReportCommand_Executed);

    /// <summary>
    /// 
    /// </summary>
    public IAsyncRelayCommand OpenReportPathCommand => new AsyncRelayCommand(OpenReportPathCommand_Executed);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private async Task ExecuteRFIDGearCommand()
    {
        try
        {
            using SettingsReaderWriter settings = new SettingsReaderWriter();
            using ReportReaderWriterService reportReader = new ReportReaderWriterService();
            settings.ReadSettings();

            await Task.Delay(1000);

            var finalPath =
                settings.DefaultSettings.DefaultProjectOutputPath + "\\"
                + (settings.DefaultSettings.CreateSubdirectoryIsEnabled == true ? CheckProcessService.CurrentCardCheckProcess.JobNr + "\\" : string.Empty)
                + CheckProcessService.CurrentCardCheckProcess.JobNr + "-"
                + CheckProcessService.CurrentCardCheckProcess.ChipNumber
                + "_final.pdf";

            var semiFinalPath =
                settings.DefaultSettings.DefaultProjectOutputPath + "\\"
                + (settings.DefaultSettings.CreateSubdirectoryIsEnabled == true ? CheckProcessService.CurrentCardCheckProcess.JobNr + "\\" : string.Empty)
                + CheckProcessService.CurrentCardCheckProcess.JobNr + "-"
                + CheckProcessService.CurrentCardCheckProcess.ChipNumber
                + "_.pdf";

            var p = new Process();

            var info = new ProcessStartInfo
            {
                FileName = settings.DefaultSettings.DefaultRFIDGearExePath,
                Verb = "",
                Arguments = string.Format(
                    "REPORTTARGETPATH=" + "\"" + "{0}" + "\" " +
                    "REPORTTEMPLATEFILE=" + "\"" + "{1}" + "\" " +
                    "CUSTOMPROJECTFILE=" + "\"" + "{2}" + "\" " +
                    "$JOBNUMBER=" + "\"" + "{3}" + "\" " +
                    "$CHIPNUMBER=" + "\"" + "{4}" + "\" " +
                    "{5}",
                    finalPath,
                    semiFinalPath,
                    settings.DefaultSettings.LastUsedCustomProjectPath,
                    CheckProcessService.CurrentCardCheckProcess.JobNr,
                    CheckProcessService.CurrentCardCheckProcess.ChipNumber,
                    (settings.DefaultSettings.AutoLoadProjectOnStart ?? false) ? "AUTORUN=1" : "AUTORUN=0"),

                UseShellExecute = false,
                WorkingDirectory = settings.DefaultSettings.DefaultProjectOutputPath
            };

            p.StartInfo = info;

            p.Exited += (sender, eventArgs) =>
            {
                if (File.Exists(finalPath))
                {
                    File.Copy(finalPath, semiFinalPath, true);
                }
            };

            p.Start();

            await p.WaitForExitAsync();

            reportReader.ReportTemplatePath = semiFinalPath;
            reportReader.ReportOutputPath = finalPath;

            var supported = reportReader.GetReportField("CheckBox_isChipSuppYes") != null && reportReader.GetReportField("CheckBox_isChipSuppYes") == "Yes";
            var programmable = reportReader.GetReportField("CheckBox_ChipCanUseYes") != null && reportReader.GetReportField("CheckBox_ChipCanUseYes") == "Yes";

            TextBlockCheckNotYetFinishedIsVisible = false;

            if (supported && !programmable)
            {
                Debug.WriteLine("supported only");

                TextBlockCheckFinishedAndResultIsSuppOnlyIsVisible = true;
                HyperlinkButtonReportIsVisible = true;

                NextStepButtonContent = "Fertigstellen";
            }

            if (supported && programmable)
            {
                Debug.WriteLine("programmable");

                HyperlinkButtonReportIsVisible = true;

                await App.MainRoot.MessageDialogAsync(
                    "Prüfung erfolgreich abgeschlossen.",
                    "Die Prüfung ist hiermit abgeschlossen.\n" +
                    "Bitte nimm die Karte vom Leser und bereite sie für den Rückversand vor.\n" +
                    "\n" +
                    "Hinweis: \n" +
                    "Mit dem Klick auf \"Fertigstellen\" wird das Ergebnis der Prüfung\n" +
                    "Automatisch in die Datenbank hochgeladen und an OMNI übertragen.",
                    "OK");

                TextBlockCheckFinishedAndResultIsSuppAndProgIsVisible = true;

            }
        }

        catch (Exception ex)
        {
            await App.MainRoot.MessageDialogAsync(
                "Fehler",
                string.Format("Bitte melde den folgenden Fehler an mich:\n{0}", ex.Message));

            LogWriter.CreateLogEntry(ex);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task PostPageLoadedCommand_Executed()
    {
        try
        {
            await Task.Delay(100);

            await ExecuteRFIDGearCommand();
            NextStepCanExecute = true;
        }
        catch (Exception ex)
        {
            await App.MainRoot.MessageDialogAsync(
                "Fehler",
                string.Format("Bitte melde den folgenden Fehler an mich:\n{0}", ex.Message));

            LogWriter.CreateLogEntry(ex);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private async Task OpenReportCommand_Executed()
    {
        try
        {
            using SettingsReaderWriter settings = new SettingsReaderWriter();
            settings.ReadSettings();

            var p = new Process();

            var info = new ProcessStartInfo()
            {
                FileName = settings.DefaultSettings.DefaultProjectOutputPath + "\\"
                + (settings.DefaultSettings.CreateSubdirectoryIsEnabled == true ? CheckProcessService.CurrentCardCheckProcess.JobNr + "\\" : string.Empty)
                + CheckProcessService.CurrentCardCheckProcess.JobNr + "-"
                + CheckProcessService.CurrentCardCheckProcess.ChipNumber
                + "_final.pdf",
                Verb = "",
                UseShellExecute = true
            };

            p.StartInfo = info;

            p.Start();
        }
        catch (Exception ex)
        {
            await App.MainRoot.MessageDialogAsync(
                "Fehler",
                string.Format("Bitte melde den folgenden Fehler an mich:\n{0}", ex.Message));

            LogWriter.CreateLogEntry(ex);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private async Task OpenReportPathCommand_Executed()
    {
        try
        {
            using SettingsReaderWriter settings = new SettingsReaderWriter();
            settings.ReadSettings();

            var p = new Process();

            var info = new ProcessStartInfo()
            {
                FileName = settings.DefaultSettings.DefaultProjectOutputPath + "\\"
                + (settings.DefaultSettings.CreateSubdirectoryIsEnabled == true ? CheckProcessService.CurrentCardCheckProcess.JobNr + "\\" : string.Empty),
                Verb = "",
                UseShellExecute = true
            };

            p.StartInfo = info;

            p.Start();
        }
        catch (Exception ex)
        {
            await App.MainRoot.MessageDialogAsync(
                "Fehler",
                string.Format("Bitte melde den folgenden Fehler an mich:\n{0}", ex.Message));

            LogWriter.CreateLogEntry(ex);
        }
    }

    #endregion

    #region ExtensionMethods

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private async Task NavigateNextStepCommand_Executed()
    {
        try
        {
            using SettingsReaderWriter settings = new SettingsReaderWriter();
            using ReportReaderWriterService reportReader = new ReportReaderWriterService();

            settings.ReadSettings();

            var finalPath =
                settings.DefaultSettings.DefaultProjectOutputPath + "\\"
                + (settings.DefaultSettings.CreateSubdirectoryIsEnabled == true ? CheckProcessService.CurrentCardCheckProcess.JobNr + "\\" : string.Empty)
                + CheckProcessService.CurrentCardCheckProcess.JobNr + "-"
                + CheckProcessService.CurrentCardCheckProcess.ChipNumber
                + "_final.pdf";

            var semiFinalPath =
                settings.DefaultSettings.DefaultProjectOutputPath + "\\"
                + (settings.DefaultSettings.CreateSubdirectoryIsEnabled == true ? CheckProcessService.CurrentCardCheckProcess.JobNr + "\\" : string.Empty)
                + CheckProcessService.CurrentCardCheckProcess.JobNr + "-"
                + CheckProcessService.CurrentCardCheckProcess.ChipNumber
                + "_.pdf";

            var preFinalPath =
                settings.DefaultSettings.DefaultProjectOutputPath + "\\"
                + (settings.DefaultSettings.CreateSubdirectoryIsEnabled == true ? CheckProcessService.CurrentCardCheckProcess.JobNr + "\\" : string.Empty)
                + CheckProcessService.CurrentCardCheckProcess.JobNr + "-"
                + CheckProcessService.CurrentCardCheckProcess.ChipNumber
                + ".pdf";

            reportReader.ReportTemplatePath = semiFinalPath;
            reportReader.ReportOutputPath = finalPath;
            reportReader.SetReadOnlyOnAllFields(true);

            var fileName = finalPath;
            var fileStream = File.Open(fileName, FileMode.Open);


            await SQLDBService.Instance.InsertData(CheckProcessService.CurrentCardCheckProcess.ID, fileStream);
            await SQLDBService.Instance.InsertData(CheckProcessService.CurrentCardCheckProcess.ID, OrderStatus.CheckFinished.ToString());

            fileStream.Close();

            try
            {
                if (settings.DefaultSettings.RemoveTemporaryReportsIsEnabled == true)
                {
                    File.Delete(preFinalPath);
                    File.Delete(semiFinalPath);
                }
            }
            catch (Exception ex)
            {
                await App.MainRoot.MessageDialogAsync(
                    "Fehler",
                    string.Format("Bitte melde den folgenden Fehler an mich:\n{0}", ex.Message));

                return;
            }

            (App.MainRoot.XamlRoot.Content as ShellPage)?.ViewModel.NavigationService.NavigateTo(typeof(HomePageViewModel).FullName ?? "");

        }
        catch (Exception ex)
        {
            await App.MainRoot.MessageDialogAsync(
                "Fehler",
                string.Format("Bitte melde den folgenden Fehler an mich:\n{0}", ex.Message));

            LogWriter.CreateLogEntry(ex);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void NavigateBackCommand_Executed()
    {
        try
        {
            (App.MainRoot.XamlRoot.Content as ShellPage)?.ViewModel.NavigationService.NavigateTo(typeof(Step1PageViewModel).FullName ?? "");
        }
        catch (Exception e)
        {
            LogWriter.CreateLogEntry(e);
        }
    }
    #endregion

}

using CardCheckAssistant.Views;
using CardCheckAssistant.Services;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using System.Diagnostics;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using Windows.Foundation;

namespace CardCheckAssistant.ViewModels;

public class Step3PageViewModel : ObservableObject
{
    public Step3PageViewModel()
    {
        NextStepCanExecute = false;
        GoBackCanExecute = true;

        TextBlockCheckFinishedAndResultIsSuppOnlyIsVisible = false;
        TextBlockCheckFinishedAndResultIsSuppAndProgIsVisible = false;
        TextBlockCheckNotYetFinishedIsVisible = true;
        TextBlockCheckFinishedIsVisible = false;
        HyperlinkButtonReportIsVisible = false;

        NextStepButtonContent = "Weiter";
    }

    public string NextStepButtonContent
    {
        get => _nextStepButtonContent;
        set => SetProperty(ref _nextStepButtonContent, value);
    }
    private string _nextStepButtonContent;

    public bool TextBlockCheckFinishedAndResultIsSuppAndProgIsVisible
    {
        get => _textBlockCheckFinishedAndResultIsSuppAndProgIsVisible;
        set => SetProperty(ref _textBlockCheckFinishedAndResultIsSuppAndProgIsVisible, value);
    }
    private bool _textBlockCheckFinishedAndResultIsSuppAndProgIsVisible;

    public bool HyperlinkButtonReportIsVisible
    {
        get => _hyperlinkButtonReportIsVisible;
        set => SetProperty(ref _hyperlinkButtonReportIsVisible, value);
    }
    private bool _hyperlinkButtonReportIsVisible;

    public bool TextBlockCheckFinishedIsVisible
    {
        get => _textBlockCheckFinishedIsVisible;
        set => SetProperty(ref _textBlockCheckFinishedIsVisible, value);
    }
    private bool _textBlockCheckFinishedIsVisible;

    public bool TextBlockCheckNotYetFinishedIsVisible
    {
        get => _textBlockCheckNotYetFinishedIsVisible;
        set => SetProperty(ref _textBlockCheckNotYetFinishedIsVisible, value);
    }
    private bool _textBlockCheckNotYetFinishedIsVisible;
    
    public bool TextBlockCheckFinishedAndResultIsSuppOnlyIsVisible
    {
        get => _textBlockCheckFinishedAndResultIsSuppOnlyIsVisible;
        set => SetProperty(ref _textBlockCheckFinishedAndResultIsSuppOnlyIsVisible, value);
    }
    private bool _textBlockCheckFinishedAndResultIsSuppOnlyIsVisible;

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

    public string JobNumber => string.Format("JobNr.: {0}; {1}",CheckProcessService.CurrentCardCheckProcess.JobNr, CheckProcessService.CurrentCardCheckProcess.CName);
    public string ReportLanguage => string.Format("{0}", CheckProcessService.CurrentCardCheckProcess.ReportLanguage);

    #region Commands
    public ICommand NavigateNextStepCommand => new AsyncRelayCommand(NavigateNextStepCommand_Executed);
    public ICommand NavigateBackCommand => new RelayCommand(NavigateBackCommand_Executed);

    public ICommand PostPageLoadedCommand => new AsyncRelayCommand(PostPageLoadedCommand_Executed);

    public ICommand OpenReportCommand => new RelayCommand(OpenReportCommand_Executed);
    public ICommand OpenReportPathCommand => new RelayCommand(OpenReportPathCommand_Executed);

    private async Task ExecuteRFIDGearCommand()
    {
        using SettingsReaderWriter settings = new SettingsReaderWriter();
        settings.ReadSettings();

        var p = new Process();
        var tokenSource = new CancellationTokenSource();
        var ct = tokenSource.Token;

        var info = new ProcessStartInfo()
        {
            FileName = settings.DefaultSettings.DefaultRFIDGearExePath,
            Verb = "",
            Arguments = string.Format(
                "REPORTTARGETPATH=" + "\"" + "{0}" + "\" " + 
                "$JOBNUMBER=" + "\"" + "{1}" + "\" " +
                "$CHIPNUMBER=" + "\"" + "{2}" + "\" " +
                "{3}",
                settings.DefaultSettings.DefaultProjectOutputPath + "\\" + CheckProcessService.CurrentCardCheckProcess.JobNr + "-" + CheckProcessService.CurrentCardCheckProcess.ChipNumber + ".pdf",
                CheckProcessService.CurrentCardCheckProcess.JobNr,
                CheckProcessService.CurrentCardCheckProcess.ChipNumber,
                settings.DefaultSettings.AutoLoadProjectOnStart ? "AUTORUN=1" : "AUTORUN=0"),

            UseShellExecute = false,
            WorkingDirectory = settings.DefaultSettings.DefaultProjectOutputPath
        };

        try
        {
            using ReportReaderWriterService reportReader = new ReportReaderWriterService();

            p.StartInfo = info;

            p.Exited += (sender, eventArgs) =>
            {
                reportReader.ReportTemplatePath = settings.DefaultSettings.DefaultProjectOutputPath + "\\" + CheckProcessService.CurrentCardCheckProcess.JobNr + "-" + CheckProcessService.CurrentCardCheckProcess.ChipNumber + ".pdf";                    
            };

            p.Start();

            await p.WaitForExitAsync();

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
                    "Prüfung erfolgreich abgeschlossen.\n" +
                    "Ergebnis: Die Karte sollte programmierbar sein...",
                    string.Format("Der Test hat ergeben, dass die Karte in der LSM programmierbar sein sollte.\n" +
                    "Bitte nimm die Karte vom Leser und führe einen Test mit der LSM durch.\n" +
                    "Verwende hierfür die Daten aus dem Bericht, der soeben erstellt wurde." +
                    "\n" +
                    "WICHTIG: \n" +
                    "Kehre nach dem Test hierher zurück um den freien Speicher aus zu lesen.\n" +
                    "Alles klar? Dann schließe bitte dieses Fenster und fahre mit der LSM fort."),
                    "Schliessen");

                TextBlockCheckFinishedAndResultIsSuppAndProgIsVisible = true;

            }


            //TextBlockCheckFinishedIsVisible = true;
        }

        catch (Exception e)
        {
            //LogWriter.CreateLogEntry(string.Format("{0}: {1}; {2}", DateTime.Now, e.Message, e.InnerException != null ? e.InnerException.Message : ""), FacilityName);

            return;
        }
    }

    public async Task PostPageLoadedCommand_Executed()
    {
        await ExecuteRFIDGearCommand();

        NextStepCanExecute = true;
    }

    private void OpenReportCommand_Executed()
    {
        using SettingsReaderWriter settings = new SettingsReaderWriter();
        settings.ReadSettings();

        var p = new Process();

        var info = new ProcessStartInfo()
        {
            FileName = settings.DefaultSettings.DefaultProjectOutputPath + "\\" + CheckProcessService.CurrentCardCheckProcess.JobNr + "-" + CheckProcessService.CurrentCardCheckProcess.ChipNumber + ".pdf",
            Verb = "",     
            UseShellExecute = true
        };

        p.StartInfo = info;

        p.Start();
    }

    private void OpenReportPathCommand_Executed()
    {
        using SettingsReaderWriter settings = new SettingsReaderWriter();
        settings.ReadSettings();

        var p = new Process();

        var info = new ProcessStartInfo()
        {
            FileName = settings.DefaultSettings.DefaultProjectOutputPath + "\\",
            Verb = "",
            UseShellExecute = true
        };

        p.StartInfo = info;

        p.Start();
    }

    #endregion

    public ObservableCollection<string> ChipCount { get; set; }

    private async Task NavigateNextStepCommand_Executed()
    {
        var window = (Application.Current as App)?.Window as MainWindow;
        var navigation = window.Navigation;
        var homePage = navigation.GetNavigationViewItems(typeof(HomePage)).First();
        navigation.SetCurrentNavigationViewItem(homePage);
    }

    private void NavigateBackCommand_Executed()
    {
        var window = (Application.Current as App)?.Window as MainWindow;
        var navigation = window.Navigation;
        var step1Page = navigation.GetNavigationViewItems(typeof(Step1Page)).First();
        navigation.SetCurrentNavigationViewItem(step1Page);
    }

}

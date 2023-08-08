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
using CardCheckAssistant.Models;
using Microsoft.UI.Xaml.Controls;

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

        NavigateNextStepCommand = new AsyncRelayCommand(NavigateNextStepCommand_Executed);
        RunRFiDGearCommand = new AsyncRelayCommand(ExecuteRFIDGearCommand);
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
    public IAsyncRelayCommand NavigateNextStepCommand { get; }
    public IAsyncRelayCommand RunRFiDGearCommand { get; }

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
                "REPORTTEMPLATEFILE=" + "\"" + "{1}" + "\" " +
                "CUSTOMPROJECTFILE=" + "\"" + "{2}" + "\" " +
                "$JOBNUMBER=" + "\"" + "{3}" + "\" " +
                "$CHIPNUMBER=" + "\"" + "{4}" + "\" " +
                "{5}",
                settings.DefaultSettings.DefaultProjectOutputPath + "\\" + CheckProcessService.CurrentCardCheckProcess.JobNr + "-" + CheckProcessService.CurrentCardCheckProcess.ChipNumber + "_final.pdf",
                settings.DefaultSettings.DefaultProjectOutputPath + "\\" + CheckProcessService.CurrentCardCheckProcess.JobNr + "-" + CheckProcessService.CurrentCardCheckProcess.ChipNumber + ".pdf",
                settings.DefaultSettings.LastUsedCustomProjectPath,
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
                    "Prüfung erfolgreich abgeschlossen.\n",
                    string.Format("Die Prüfung ist hiermit abgeschlossen.\n" +
                    "Bitte nimm die Karte vom Leser und bereite sie für den Rückversand vor.\n" +
                    "\n" +
                    "Hinweis: \n" +
                    "Mit dem Klick auf \"Fertigstellen\" wird das Ergebnis des Berichtes\n" +
                    "Automatisch an den im Auftrag hinterlegten Ansprechpartner versendet."),
                    "OK");

                TextBlockCheckFinishedAndResultIsSuppAndProgIsVisible = true;

            }
        }

        catch (Exception e)
        {
            //LogWriter.CreateLogEntry(string.Format("{0}: {1}; {2}", DateTime.Now, e.Message, e.InnerException != null ? e.InnerException.Message : ""), FacilityName);

            return;
        }
    }

    public async Task PostPageLoadedCommand_Executed()
    {
        await RunRFiDGearCommand.ExecuteAsync(null);

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
        using SettingsReaderWriter settings = new SettingsReaderWriter();
        settings.ReadSettings();

        var window = (Application.Current as App)?.Window as MainWindow;
        var navigation = window.Navigation;
        NavigationViewItem nextpage;
        nextpage = navigation.GetNavigationViewItems(typeof(HomePage)).First();

        List<CardCheckProcess> cardChecks = SQLDBService.Instance.CardChecks;

        var fileName = settings.DefaultSettings.DefaultProjectOutputPath + "\\" + CheckProcessService.CurrentCardCheckProcess.JobNr + "-" + CheckProcessService.CurrentCardCheckProcess.ChipNumber + "_final.pdf";
        var fileStream = File.Open(fileName, FileMode.Open);

        await SQLDBService.Instance.InsertData(CheckProcessService.CurrentCardCheckProcess.ID, fileStream);
        await SQLDBService.Instance.InsertData(CheckProcessService.CurrentCardCheckProcess.ID, OrderStatus.CheckFinished.ToString());

        navigation.SetCurrentNavigationViewItem(nextpage);
        nextpage.IsEnabled = true;
    }

    private void NavigateBackCommand_Executed()
    {
        var window = (Application.Current as App)?.Window as MainWindow;
        var navigation = window.Navigation;
        var step1Page = navigation.GetNavigationViewItems(typeof(Step1Page)).First();
        navigation.SetCurrentNavigationViewItem(step1Page);
    }

}

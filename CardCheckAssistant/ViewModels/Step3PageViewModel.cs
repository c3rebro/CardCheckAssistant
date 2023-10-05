﻿using CardCheckAssistant.Views;
using CardCheckAssistant.Services;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using System.Diagnostics;
using System.Windows.Input;
using System.Collections.ObjectModel;

using Log4CSharp;

using CardCheckAssistant.Models;
using Microsoft.UI.Xaml.Controls;

namespace CardCheckAssistant.ViewModels;

/// <summary>
/// 
/// </summary>
public class Step3PageViewModel : ObservableObject
{
    /// <summary>
    /// 
    /// </summary>
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
    public bool TextBlockCheckFinishedAndResultIsSuppAndProgIsVisible
    {
        get => _textBlockCheckFinishedAndResultIsSuppAndProgIsVisible;
        set => SetProperty(ref _textBlockCheckFinishedAndResultIsSuppAndProgIsVisible, value);
    }
    private bool _textBlockCheckFinishedAndResultIsSuppAndProgIsVisible;

    /// <summary>
    /// 
    /// </summary>
    public bool HyperlinkButtonReportIsVisible
    {
        get => _hyperlinkButtonReportIsVisible;
        set => SetProperty(ref _hyperlinkButtonReportIsVisible, value);
    }
    private bool _hyperlinkButtonReportIsVisible;

    /// <summary>
    /// 
    /// </summary>
    public bool TextBlockCheckFinishedIsVisible
    {
        get => _textBlockCheckFinishedIsVisible;
        set => SetProperty(ref _textBlockCheckFinishedIsVisible, value);
    }
    private bool _textBlockCheckFinishedIsVisible;

    /// <summary>
    /// 
    /// </summary>
    public bool TextBlockCheckNotYetFinishedIsVisible
    {
        get => _textBlockCheckNotYetFinishedIsVisible;
        set => SetProperty(ref _textBlockCheckNotYetFinishedIsVisible, value);
    }
    private bool _textBlockCheckNotYetFinishedIsVisible;

    /// <summary>
    /// 
    /// </summary>
    public bool TextBlockCheckFinishedAndResultIsSuppOnlyIsVisible
    {
        get => _textBlockCheckFinishedAndResultIsSuppOnlyIsVisible;
        set => SetProperty(ref _textBlockCheckFinishedAndResultIsSuppOnlyIsVisible, value);
    }
    private bool _textBlockCheckFinishedAndResultIsSuppOnlyIsVisible;

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
    public string JobNumber => string.Format("JobNr.: {0}; ChipNummer: {1}; Kunde: {2}", CheckProcessService.CurrentCardCheckProcess.JobNr, CheckProcessService.CurrentCardCheckProcess.ChipNumber, CheckProcessService.CurrentCardCheckProcess.CName);

    /// <summary>
    /// 
    /// </summary>
    public string ReportLanguage => string.Format("{0}", CheckProcessService.CurrentCardCheckProcess.ReportLanguage);

    /// <summary>
    /// 
    /// </summary>
    public ObservableCollection<string> ChipCount { get; set; }

    #endregion

    #region Commands

    /// <summary>
    /// 
    /// </summary>
    public IAsyncRelayCommand NavigateNextStepCommand { get; }

    /// <summary>
    /// 
    /// </summary>
    public IAsyncRelayCommand RunRFiDGearCommand { get; }

    /// <summary>
    /// 
    /// </summary>
    public ICommand NavigateBackCommand => new RelayCommand(NavigateBackCommand_Executed);

    /// <summary>
    /// 
    /// </summary>
    public IAsyncRelayCommand PostPageLoadedCommand { get; }

    /// <summary>
    /// 
    /// </summary>
    public ICommand OpenReportCommand => new RelayCommand(OpenReportCommand_Executed);

    /// <summary>
    /// 
    /// </summary>
    public ICommand OpenReportPathCommand => new RelayCommand(OpenReportPathCommand_Executed);

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

            string finalPath =
                settings.DefaultSettings.DefaultProjectOutputPath + "\\"
                + (settings.DefaultSettings.CreateSubdirectoryIsEnabled == true ? CheckProcessService.CurrentCardCheckProcess.JobNr + "\\" : string.Empty)
                + CheckProcessService.CurrentCardCheckProcess.JobNr + "-"
                + CheckProcessService.CurrentCardCheckProcess.ChipNumber
                + "_final.pdf";

            string semiFinalPath =
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
            LogWriter.CreateLogEntry(e);
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
        catch (Exception e)
        {
            LogWriter.CreateLogEntry(e);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OpenReportCommand_Executed()
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
        catch (Exception e)
        {
            LogWriter.CreateLogEntry(e);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OpenReportPathCommand_Executed()
    {
        try
        {
            using SettingsReaderWriter settings = new SettingsReaderWriter();
            settings.ReadSettings();

            var p = new Process();

            var info = new ProcessStartInfo()
            {
                FileName = settings.DefaultSettings.DefaultProjectOutputPath + "\\"
                +(settings.DefaultSettings.CreateSubdirectoryIsEnabled == true ? CheckProcessService.CurrentCardCheckProcess.JobNr + "\\" : string.Empty),
                Verb = "",
                UseShellExecute = true
            };

            p.StartInfo = info;

            p.Start();
        }
        catch (Exception e)
        {
            LogWriter.CreateLogEntry(e);
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

            string finalPath =
                settings.DefaultSettings.DefaultProjectOutputPath + "\\"
                + (settings.DefaultSettings.CreateSubdirectoryIsEnabled == true ? CheckProcessService.CurrentCardCheckProcess.JobNr + "\\" : string.Empty)
                + CheckProcessService.CurrentCardCheckProcess.JobNr + "-"
                + CheckProcessService.CurrentCardCheckProcess.ChipNumber
                + "_final.pdf";

            string semiFinalPath =
                settings.DefaultSettings.DefaultProjectOutputPath + "\\"
                + (settings.DefaultSettings.CreateSubdirectoryIsEnabled == true ? CheckProcessService.CurrentCardCheckProcess.JobNr + "\\" : string.Empty)
                + CheckProcessService.CurrentCardCheckProcess.JobNr + "-"
                + CheckProcessService.CurrentCardCheckProcess.ChipNumber
                + "_.pdf";

            string preFinalPath =
                settings.DefaultSettings.DefaultProjectOutputPath + "\\"
                + (settings.DefaultSettings.CreateSubdirectoryIsEnabled == true ? CheckProcessService.CurrentCardCheckProcess.JobNr + "\\" : string.Empty)
                + CheckProcessService.CurrentCardCheckProcess.JobNr + "-"
                + CheckProcessService.CurrentCardCheckProcess.ChipNumber
                + ".pdf";

            reportReader.ReportTemplatePath = semiFinalPath;
            reportReader.ReportOutputPath = finalPath;
            reportReader.SetReadOnlyOnAllFields(true);

            var window = (Application.Current as App)?.Window as MainWindow ?? new MainWindow();
            var navigation = window.Navigation;
            NavigationViewItem nextpage;
            nextpage = navigation.GetNavigationViewItems(typeof(HomePage)).First();

            var fileName = finalPath;
            var fileStream = File.Open(fileName, FileMode.Open);


            await SQLDBService.Instance.InsertData(CheckProcessService.CurrentCardCheckProcess.ID, fileStream);
            await SQLDBService.Instance.InsertData(CheckProcessService.CurrentCardCheckProcess.ID, OrderStatus.CheckFinished.ToString());

            fileStream.Close();

            File.Delete(preFinalPath);
            File.Delete(semiFinalPath);

            navigation.SetCurrentNavigationViewItem(nextpage);
            nextpage.IsEnabled = true;
            
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
            var window = (Application.Current as App)?.Window as MainWindow ?? new MainWindow();
            var navigation = window.Navigation;
            var step1Page = navigation.GetNavigationViewItems(typeof(Step1Page)).First();
            navigation.SetCurrentNavigationViewItem(step1Page);
        }
        catch (Exception e)
        {
            LogWriter.CreateLogEntry(e);
        }
    }
    #endregion


}

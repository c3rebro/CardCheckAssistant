﻿using CardCheckAssistant.Views;
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

using Windows.Foundation;
using Microsoft.UI.Xaml.Controls;
using CardCheckAssistant.Models;
using Log4CSharp;
using System.Reflection;

namespace CardCheckAssistant.ViewModels;

public class Step2PageViewModel : ObservableObject
{
    private readonly DispatcherTimer scanChipTimer;
    private ObservableCollection<CardCheckProcess> cardCheckProcesses;
    private int amountOfFreeMemory = 0;
    private bool notEnoughFreeMemory = true;
    private bool supported = false;
    private bool programmable = false;


#if DEBUG
    private const string DBNAME = "OT_CardCheck_Test";
#else
    private const string DBNAME = "OT_CardCheck";
#endif

    public Step2PageViewModel()
    {
        scanChipTimer = new DispatcherTimer();
        scanChipTimer.Tick += ScanChipEvent;
        scanChipTimer.Interval = new TimeSpan(0, 0, 0, 0, 3000);
        scanChipTimer.Stop();

        NextStepCanExecute = false;
        GoBackCanExecute = true;

        TextBlockCheckFinishedAndResultIsSuppOnlyIsVisible = false;
        TextBlockCheckFinishedAndResultIsSuppAndProgIsVisible = false;
        TextBlockCheckFinishedAndResultIsNotSuppIsVisible = false;
        TextBlockCheckFinishedAndResultIsMissingPICCKeyIsVisible = false;
        TextBlockCheckFinishedAndResultIsNotEnoughMemoryIsVisible = false;

        TextBlockCheckNotYetFinishedIsVisible = true;
        TextBlockCheckFinishedIsVisible = false;
        HyperlinkButtonReportIsVisible = false;
        ReaderHasNoChipInfoBarIsVisible = false;

        SelectedCustomerRequestTemplate = CustomerRequestTemplate.NA;

        NextStepButtonContent = "Weiter";
    }

    public CustomerRequestTemplate SelectedCustomerRequestTemplate
    {
        get => _selectedCustomerRequestTemplate;
        set => SetProperty(ref _selectedCustomerRequestTemplate, value);
    }
    private CustomerRequestTemplate _selectedCustomerRequestTemplate;

    public string NextStepButtonContent
    {
        get => _nextStepButtonContent;
        set => SetProperty(ref _nextStepButtonContent, value);
    }
    private string _nextStepButtonContent;

    public bool ReaderHasNoChipInfoBarIsVisible
    {
        get => _readerHasNoChipInfoBarIsVisible;
        set => SetProperty(ref _readerHasNoChipInfoBarIsVisible, value);
    }
    private bool _readerHasNoChipInfoBarIsVisible;
    
    public bool TextBlockCheckFinishedAndResultIsNotEnoughMemoryIsVisible
    {
        get => _textBlockCheckFinishedAndResultIsNotEnoughMemoryIsVisible;
        set => SetProperty(ref _textBlockCheckFinishedAndResultIsNotEnoughMemoryIsVisible, value);
    }
    private bool _textBlockCheckFinishedAndResultIsNotEnoughMemoryIsVisible;


    public bool TextBlockCheckFinishedAndResultIsMissingPICCKeyIsVisible
    {
        get => _textBlockCheckFinishedAndResultIsMissingPICCKeyIsVisible;
        set => SetProperty(ref _textBlockCheckFinishedAndResultIsMissingPICCKeyIsVisible, value);
    }
    private bool _textBlockCheckFinishedAndResultIsMissingPICCKeyIsVisible;

    public bool TextBlockCheckFinishedAndResultIsNotSuppIsVisible
    {
        get => _textBlockCheckFinishedAndResultIsNotSuppIsVisible;
        set => SetProperty(ref _textBlockCheckFinishedAndResultIsNotSuppIsVisible, value);
    }
    private bool _textBlockCheckFinishedAndResultIsNotSuppIsVisible;

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

    public string JobNumber => string.Format("JobNr.: {0}; ChipNummer: {1}; Kunde: {2}", CheckProcessService.CurrentCardCheckProcess.JobNr, CheckProcessService.CurrentCardCheckProcess.ChipNumber, CheckProcessService.CurrentCardCheckProcess.CName);
    public string ReportLanguage => string.Format("{0}", CheckProcessService.CurrentCardCheckProcess.ReportLanguage);

    #region Commands
    public ICommand NavigateNextStepCommand => new AsyncRelayCommand(NavigateNextStepCommand_Executed);
    public ICommand NavigateBackCommand => new RelayCommand(NavigateBackCommand_Executed);

    public ICommand PostPageLoadedCommand => new AsyncRelayCommand(PostPageLoadedCommand_Executed);

    public ICommand OpenReportCommand => new RelayCommand(OpenReportCommand_Executed);
    public ICommand OpenReportPathCommand => new RelayCommand(OpenReportPathCommand_Executed);

    private async Task ExecuteRFIDGearCommand()
    {
        using ReportReaderWriterService reportReader = new ReportReaderWriterService();
        using SettingsReaderWriter settings = new SettingsReaderWriter();

        await Task.Delay(1000);

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
            p.StartInfo = info;

            p.Exited += (sender, eventArgs) =>
            {
                reportReader.ReportTemplatePath = settings.DefaultSettings.DefaultProjectOutputPath + "\\" + CheckProcessService.CurrentCardCheckProcess.JobNr + "-" + CheckProcessService.CurrentCardCheckProcess.ChipNumber + ".pdf";

                supported = reportReader.GetReportField("CheckBox_isChipSuppYes") != null && reportReader.GetReportField("CheckBox_isChipSuppYes") == "Yes";
                programmable = reportReader.GetReportField("CheckBox_ChipCanUseYes") != null && reportReader.GetReportField("CheckBox_ChipCanUseYes") == "Yes";
            };

            p.Start();

            await p.WaitForExitAsync();

            if (reportReader.GetReportField("TextBox_Detail_FreeMem_1") != null)
            {
                if (int.TryParse(reportReader.GetReportField("TextBox_Detail_FreeMem_1").Split(' ')[0], out amountOfFreeMemory))
                {
                    if(amountOfFreeMemory >= 225) 
                    { 
                        notEnoughFreeMemory = false;
                    }
                    else
                    {
                        notEnoughFreeMemory = true;
                    }
                }

                else if(reportReader.GetReportField("TextBox_Detail_FreeMem_1").Split(',').Count() >= 1)
                {
                    var sectors = reportReader.GetReportField("TextBox_Detail_FreeMem_1").Split(',');

                    if (sectors.Length >= 1)
                    {
                        notEnoughFreeMemory = false;
                    }
                    else
                    {
                        notEnoughFreeMemory = true;
                    }
                }
                else
                {
                    amountOfFreeMemory = -1;
                }    
            } 

            TextBlockCheckNotYetFinishedIsVisible = false;


            if (!supported)
            {
                Debug.WriteLine("supported only");

                TextBlockCheckFinishedAndResultIsNotSuppIsVisible = true;
                HyperlinkButtonReportIsVisible = true;

                NextStepButtonContent = "Fertigstellen";
                TextBlockCheckFinishedIsVisible = true;
            }

            else if (supported && !programmable && !notEnoughFreeMemory)
            {
                Debug.WriteLine("supported but missing info e.g. key");

                TextBlockCheckFinishedAndResultIsSuppOnlyIsVisible = true;
                TextBlockCheckFinishedAndResultIsMissingPICCKeyIsVisible = true;
                HyperlinkButtonReportIsVisible = true;

                NextStepCanExecute = true;
                NextStepButtonContent = "Fertigstellen";
            }

            else if (supported && !programmable && notEnoughFreeMemory || (supported && programmable && notEnoughFreeMemory))
            {
                Debug.WriteLine("supported but no memory left");

                TextBlockCheckFinishedAndResultIsSuppOnlyIsVisible = true;
                TextBlockCheckFinishedAndResultIsNotEnoughMemoryIsVisible = true;
                HyperlinkButtonReportIsVisible = true;

                NextStepButtonContent = "Fertigstellen";
            }

            else if (supported && programmable)
            {
                Debug.WriteLine("programmable");

                TextBlockCheckFinishedAndResultIsSuppAndProgIsVisible = true;
                HyperlinkButtonReportIsVisible = true;

                NextStepCanExecute = false;
                scanChipTimer.Start();
            }
        }

        catch (Exception e)
        {
            LogWriter.CreateLogEntry(e, Application.Current.GetType().FullName);

            scanChipTimer.Stop();

            await App.MainRoot.MessageDialogAsync(
                "Fehler beim starten von RFIDGear.\n",
                string.Format("Bitte melde den folgenden Fehler an mich:\n{0}",e.Message));

            NextStepCanExecute = false;
            return;
        }
    }

    private async Task ChipIsRemoved(ReaderService readerService)
    {
        try
        {
            scanChipTimer.Stop();

            await readerService.ReadChipPublic();

            while (readerService.GenericChip != null)
            {
                await readerService.ReadChipPublic();
            }

            scanChipTimer.Start();
        } 
        
        catch (Exception e)
        {

        }
    }

    private async Task ChipIsPlacedAgain(ReaderService readerService)
    {
        try
        {
            scanChipTimer.Stop();

            await readerService.ReadChipPublic();

            while (readerService.GenericChip == null)
            {
                await readerService.ReadChipPublic();
            }

            scanChipTimer.Start();
        }

        catch (Exception e)
        {

        }
    }

    private async void ScanChipEvent(object? sender, object e)
    {
        try
        {
            using (ReaderService readerService = new ReaderService())
            {
                await readerService.ReadChipPublic();

                if (readerService.MoreThanOneReaderFound)
                {
                    NextStepCanExecute = false;
                }

                else
                {
                    if (readerService.GenericChip != null && !NextStepCanExecute)
                    {
                        await ChipIsRemoved(readerService);

                        ReaderHasNoChipInfoBarIsVisible = true;

                        await ChipIsPlacedAgain(readerService);

                        var ChipInfoMessage = string.Format("Es wurde ein Chip erkannt:\nErkannt 1: {0}", readerService.GenericChip.CardType.ToString());

                        if (readerService.GenericChip.Child != null)
                        {
                            ChipInfoMessage = ChipInfoMessage + string.Format("\nErkannt 2: {0}", readerService.GenericChip.Child.CardType);
                        }

                        NextStepCanExecute = true;
                    }

                    if (readerService.GenericChip != null && NextStepCanExecute)
                    {

                        ReaderHasNoChipInfoBarIsVisible = false;

                        var ChipInfoMessage = string.Format("Es wurde ein Chip erkannt:\nErkannt 1: {0}", readerService.GenericChip.CardType.ToString());

                        if (readerService.GenericChip.Child != null)
                        {
                            ChipInfoMessage = ChipInfoMessage + string.Format("\nErkannt 2: {0}", readerService.GenericChip.Child.CardType);
                        }

                        NextStepCanExecute = true;
                    }

                    else
                    {
                        NextStepCanExecute = true;
                        ReaderHasNoChipInfoBarIsVisible = true;
                    }
                }


            }
        }
        catch
        {

        }
    }

    public async Task PostPageLoadedCommand_Executed()
    {
        scanChipTimer.Stop();

        await ExecuteRFIDGearCommand();
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

        scanChipTimer.Stop();
        var window = (Application.Current as App)?.Window as MainWindow;
        var navigation = window.Navigation;
        NavigationViewItem nextpage;

        if (TextBlockCheckFinishedAndResultIsSuppOnlyIsVisible)
        {
            nextpage = navigation.GetNavigationViewItems(typeof(HomePage)).First();

            List<CardCheckProcess> cardChecks = SQLDBService.Instance.CardChecks;

            var fileName = settings.DefaultSettings.DefaultProjectOutputPath + "\\" + CheckProcessService.CurrentCardCheckProcess.JobNr + "-" + CheckProcessService.CurrentCardCheckProcess.ChipNumber + ".pdf" ;
            var fileStream = File.Open(fileName, FileMode.Open);

            await SQLDBService.Instance.InsertData(CheckProcessService.CurrentCardCheckProcess.ID, fileStream);

            await SQLDBService.Instance.InsertData(CheckProcessService.CurrentCardCheckProcess.ID, OrderStatus.RequestCustomerFeedback.ToString());
        }

        else if (TextBlockCheckFinishedAndResultIsNotSuppIsVisible)
        {
            nextpage = navigation.GetNavigationViewItems(typeof(HomePage)).First();

            List<CardCheckProcess> cardChecks = SQLDBService.Instance.CardChecks;


            var fileName = settings.DefaultSettings.DefaultProjectOutputPath + "\\" + CheckProcessService.CurrentCardCheckProcess.JobNr + "-" + CheckProcessService.CurrentCardCheckProcess.ChipNumber + ".pdf";
            var fileStream = File.Open(fileName, FileMode.Open);

            await SQLDBService.Instance.InsertData(CheckProcessService.CurrentCardCheckProcess.ID, fileStream);
            await SQLDBService.Instance.InsertData(CheckProcessService.CurrentCardCheckProcess.ID, OrderStatus.CheckFinished.ToString());
        }
        else
        {
            nextpage = navigation.GetNavigationViewItems(typeof(Step3Page)).First();
        }
        navigation.SetCurrentNavigationViewItem(nextpage);
        nextpage.IsEnabled = true;
    }

    private void NavigateBackCommand_Executed()
    {
        scanChipTimer.Stop();
        var window = (Application.Current as App)?.Window as MainWindow;
        var navigation = window.Navigation;
        var step1Page = navigation.GetNavigationViewItems(typeof(Step1Page)).First();
        navigation.SetCurrentNavigationViewItem(step1Page);
    }
}

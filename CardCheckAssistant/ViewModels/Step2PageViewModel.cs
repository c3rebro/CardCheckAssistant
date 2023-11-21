﻿using System.Collections.ObjectModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using CardCheckAssistant.Models;
using CardCheckAssistant.Services;
using CardCheckAssistant.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Log4CSharp;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CardCheckAssistant.ViewModels;

public class Step2PageViewModel : ObservableObject
{
    // Define the cancellation token.
    CancellationTokenSource source = new CancellationTokenSource();
    CancellationToken token;

    private readonly DispatcherTimer scanChipTimer;
    private bool isCancelled;

#if DEBUG
    private const string DBNAME = "OT_CardCheck_Test";
#else
    private const string DBNAME = "OT_CardCheck";
#endif

    /// <summary>
    /// 
    /// </summary>
    public Step2PageViewModel()
    {
        token = source.Token;
        isCancelled = false;

        NavigateNextStepCommand = new AsyncRelayCommand(NavigateNextStepCommand_Executed);
        PostPageLoadedCommand = new AsyncRelayCommand(PostPageLoadedCommand_Executed);

        scanChipTimer = new DispatcherTimer();
        scanChipTimer.Interval = new TimeSpan(0, 0, 0, 0, 4000);
        scanChipTimer.Stop();

        NextStepCanExecute = false;
        IsReportOpen = false;
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

        ChipCount = new ObservableCollection<string>();

        CardTemplates = new List<LSMCardTemplate>()
        {
            new ("N/A", "Keine Auswahl", 0, 0),
            new ("MCBasic (Speicherbedarf: 1 Sektor)", string.Empty, 0, 1),
            new ("MC1200L (Speicherbedarf: 4 Sektoren)", string.Empty, 0, 4),
            new ("MC3800L (Speicherbedarf: 11 Sektoren)", string.Empty, 0 , 11),
            new ("MC1000L_AV (Speicherbedarf: 11 Sektoren)", string.Empty, 0 , 11),
            new ("MC2400L_AV (Speicherbedarf: 19 Sektoren)", string.Empty, 0, 19),
            new ("MC8000L_AV (Speicherbedarf: 43 Sektoren / 32 + 3)", string.Empty, 0, 43),
            new ("MDBasic (Speicherbedarf: 48 + 176 = 224 Bytes)", string.Empty, 224, 0),
            new ("MD1200L (Speicherbedarf: 192 + 160 = 352 Bytes)", string.Empty, 352, 0),
            new ("MD3800L (Speicherbedarf: 528 + 176 = 704 Bytes)", string.Empty, 704, 0),
            new ("MD2500L_AV (Speicherbedarf: 1024 + 160 = 1184 Bytes)", string.Empty, 1184, 0),
            new ("MD4000L_AV (Speicherbedarf: 1600 + 160 = 1760 Bytes)", string.Empty, 1760, 0),
            new ("MD10000L_AV (Speicherbedarf: 3048 + 184 = 3232 Bytes)", string.Empty, 1760, 0),
            new ("MD32000L_AV (Speicherbedarf: 7000 + 168 = 7168 Bytes)", string.Empty, 7168, 0) 
        };

        SelectedCustomerRequestTemplate = CustomerRequestTemplate.NA;
        SelectedLSMCardTemplate = CardTemplates.Single(x => x.TemplateText == "N/A");

        TextBoxSectorsUsed = "2,3,4,5,6,7,8,9,10,11,12";

        NextStepButtonContent = "Weiter";
    }

    #region ObservableProperties
    /// <summary>
    /// 
    /// </summary>
    public List<LSMCardTemplate> CardTemplates
    {
        get => _lsmCardTemplate;
        set => SetProperty(ref _lsmCardTemplate, value);
    }
    private List<LSMCardTemplate> _lsmCardTemplate;

    /// <summary>
    /// 
    /// </summary>
    public string TextBoxSectorsUsed
    {
        get => _textBoxSectorsUsed;
        set => SetProperty(ref _textBoxSectorsUsed, value);
    }
    private string _textBoxSectorsUsed;

    /// <summary>
    /// 
    /// </summary>
    public string InfoBarSupportedChipType
    {
        get => _infoBarSupportedChipType;
        set => SetProperty(ref _infoBarSupportedChipType, value);
    }
    private string _infoBarSupportedChipType;
    
    /// <summary>
    /// 
    /// </summary>
    public string InfoBarReportOpen
    {
        get => _infoBarReportOpen;
        set => SetProperty(ref _infoBarReportOpen, value);
    }
    private string _infoBarReportOpen;

    /// <summary>
    /// 
    /// </summary>
    public string TextBoxAdditionalHints
    {
        get => _textBoxAdditionalHints;
        set 
        {
            SetProperty(ref _textBoxAdditionalHints, value);
        }
    }
    private string _textBoxAdditionalHints;

    /// <summary>
    /// 
    /// </summary>
    public bool IsReportOpen
    {
        get => _isReportOpen;
        set
        {
            SetProperty(ref _isReportOpen, value);
            NextStepCanExecute = !value;
        }
    }
    private bool _isReportOpen;

    /// <summary>
    /// 
    /// </summary>
    public bool CheckBoxChipProgrammableYes
    {
        get => _checkBoxChipProgrammableYes;
        set
        {
            SetProperty(ref _checkBoxChipProgrammableYes, value);
            if(_checkBoxChipProgrammableNo)
            {
                CheckBoxChipProgrammableNo = !value;
            }
        }
    }
    private bool _checkBoxChipProgrammableYes;

    /// <summary>
    /// 
    /// </summary>
    public bool CheckBoxChipProgrammableNo
    {
        get => _checkBoxChipProgrammableNo;
        set 
        {
            SetProperty(ref _checkBoxChipProgrammableNo, value);
            if (_checkBoxChipProgrammableYes)
            {
                CheckBoxChipProgrammableYes = !value;
            }
        } 
    }
    private bool _checkBoxChipProgrammableNo;

    /// <summary>
    /// 
    /// </summary>
    public bool CheckBoxTestOnLockSuccess
    {
        get => _checkBoxTestOnLockSuccess;
        set 
        {
            SetProperty(ref _checkBoxTestOnLockSuccess, value);
            if (_checkBoxTestOnLockFailed)
            {
                CheckBoxTestOnLockFailed = !value;
            }
        } 
    }
    private bool _checkBoxTestOnLockSuccess;

    /// <summary>
    /// 
    /// </summary>
    public bool CheckBoxTestOnLockFailed
    {
        get => _checkBoxTestOnLockFailed;
        set 
        {
            SetProperty(ref _checkBoxTestOnLockFailed, value);
            if (_checkBoxTestOnLockSuccess)
            {
                CheckBoxTestOnLockSuccess = !value;
            }
        } 
    }
    private bool _checkBoxTestOnLockFailed;

    /// <summary>
    /// 
    /// </summary>
    public bool CheckBoxTestOnLockLimitedYes
    {
        get => _checkBoxTestOnLockLimitedYes;
        set 
        {
            SetProperty(ref _checkBoxTestOnLockLimitedYes, value);
            if (_checkBoxTestOnLockLimitedNo)
            {
                CheckBoxTestOnLockLimitedNo = !value;
            }
        } 
    }
    private bool _checkBoxTestOnLockLimitedYes;

    /// <summary>
    /// 
    /// </summary>
    public bool CheckBoxTestOnLockLimitedNo
    {
        get => _checkBoxTestOnLockLimitedNo;
        set
        {
            SetProperty(ref _checkBoxTestOnLockLimitedNo, value);
            if(_checkBoxTestOnLockLimitedYes)
            {
                CheckBoxTestOnLockLimitedYes = !value;
            }           
        }
    }
    private bool _checkBoxTestOnLockLimitedNo;

    /// <summary>
    /// 
    /// </summary>
    public string TextBlockFreeMem
    {
        get => _textBlockFreeMem;
        set
        {
            SetProperty(ref _textBlockFreeMem, value);
        }
    }
    private string _textBlockFreeMem;

    /// <summary>
    /// 
    /// </summary>
    public LSMCardTemplate SelectedLSMCardTemplate
    {
        get => _selectedLSMCardTemplate;
        set => SetProperty(ref _selectedLSMCardTemplate, value);
    }
    private LSMCardTemplate _selectedLSMCardTemplate;

    /// <summary>
    /// 
    /// </summary>
    public CustomerRequestTemplate SelectedCustomerRequestTemplate
    {
        get => _selectedCustomerRequestTemplate;
        set => SetProperty(ref _selectedCustomerRequestTemplate, value);
    }
    private CustomerRequestTemplate _selectedCustomerRequestTemplate;

    /// <summary>
    /// 
    /// </summary>
    public string NextStepButtonContent
    {
        get => _nextStepButtonContent;
        set => SetProperty(ref _nextStepButtonContent, value);
    }
    private string _nextStepButtonContent;

    /// <summary>
    /// 
    /// </summary>
    public bool IsSupported
    {
        get => _isSupported;
        set => SetProperty(ref _isSupported, value);
    }
    private bool _isSupported;

    /// <summary>
    /// 
    /// </summary>
    public bool IsSupportedAndIsClassicChip
    {
        get => _isSupportedAndIsClassicChip;
        set => SetProperty(ref _isSupportedAndIsClassicChip, value);
    }
    private bool _isSupportedAndIsClassicChip;

    /// <summary>
    /// 
    /// </summary>
    public bool ReaderHasNoChipInfoBarIsVisible
    {
        get => _readerHasNoChipInfoBarIsVisible;
        set => SetProperty(ref _readerHasNoChipInfoBarIsVisible, value);
    }
    private bool _readerHasNoChipInfoBarIsVisible;

    /// <summary>
    /// 
    /// </summary>
    public bool TextBlockCheckFinishedAndResultIsNotEnoughMemoryIsVisible
    {
        get => _textBlockCheckFinishedAndResultIsNotEnoughMemoryIsVisible;
        set => SetProperty(ref _textBlockCheckFinishedAndResultIsNotEnoughMemoryIsVisible, value);
    }
    private bool _textBlockCheckFinishedAndResultIsNotEnoughMemoryIsVisible;

    /// <summary>
    /// 
    /// </summary>
    public bool TextBlockCheckFinishedAndResultIsMissingPICCKeyIsVisible
    {
        get => _textBlockCheckFinishedAndResultIsMissingPICCKeyIsVisible;
        set => SetProperty(ref _textBlockCheckFinishedAndResultIsMissingPICCKeyIsVisible, value);
    }
    private bool _textBlockCheckFinishedAndResultIsMissingPICCKeyIsVisible;

    /// <summary>
    /// 
    /// </summary>
    public bool TextBlockCheckFinishedAndResultIsNotSuppIsVisible
    {
        get => _textBlockCheckFinishedAndResultIsNotSuppIsVisible;
        set => SetProperty(ref _textBlockCheckFinishedAndResultIsNotSuppIsVisible, value);
    }
    private bool _textBlockCheckFinishedAndResultIsNotSuppIsVisible;

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
    public ICommand NavigateBackCommand => new AsyncRelayCommand(NavigateBackCommand_Executed);

    /// <summary>
    /// 
    /// </summary>
    public IAsyncRelayCommand PostPageLoadedCommand { get;  }

    /// <summary>
    /// 
    /// </summary>
    public ICommand OpenReportCommand => new AsyncRelayCommand(OpenReportCommand_Executed);

    /// <summary>
    /// 
    /// </summary>
    public ICommand OpenReportPathCommand => new AsyncRelayCommand(OpenReportPathCommand_Executed);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private async Task ExecuteRFIDGearCommand()
    {
        try
        {
            NextStepCanExecute = false;

            await Task.Delay(1000);

            int amountOfFreeMemory = 0;
            string freeMemField = "N/A";
            TextBlockFreeMem = freeMemField;

            string chipType = "";
            InfoBarSupportedChipType = chipType;

            bool notEnoughFreeMemory = true;
            bool supported = false;
            bool programmable = false;
            string addHintsText = "";

            using ReportReaderWriterService reportReader = new ReportReaderWriterService();
            using SettingsReaderWriter settings = new SettingsReaderWriter();

            settings.ReadSettings();

            if(settings?.DefaultSettings.CreateSubdirectoryIsEnabled == true)
            {
                if(!Directory.Exists(
                    settings.DefaultSettings.DefaultProjectOutputPath + "\\"
                    + (CheckProcessService.CurrentCardCheckProcess.JobNr + "\\" ))
                    )
                {
                    Directory.CreateDirectory(
                        settings.DefaultSettings.DefaultProjectOutputPath + "\\"
                    + (CheckProcessService.CurrentCardCheckProcess.JobNr + "\\"));
                }
            }

            var p = new Process();

            var info = new ProcessStartInfo
            {
                FileName = settings.DefaultSettings.DefaultRFIDGearExePath,
                Verb = "",
                Arguments = string.Format(
                    "REPORTTARGETPATH=" + "\"" + "{0}" + "\" " +
                    "$JOBNUMBER=" + "\"" + "{1}" + "\" " +
                    "$CHIPNUMBER=" + "\"" + "{2}" + "\" " +
                    "{3}",
                    settings.DefaultSettings.DefaultProjectOutputPath + "\\"
                    + (settings.DefaultSettings.CreateSubdirectoryIsEnabled == true ? CheckProcessService.CurrentCardCheckProcess.JobNr + "\\" : string.Empty) 
                    + CheckProcessService.CurrentCardCheckProcess.JobNr + "-" 
                    + CheckProcessService.CurrentCardCheckProcess.ChipNumber 
                    + ".pdf",
                    CheckProcessService.CurrentCardCheckProcess.JobNr,
                    CheckProcessService.CurrentCardCheckProcess.ChipNumber,
                    (settings.DefaultSettings.AutoLoadProjectOnStart ?? false) ? "AUTORUN=1" : "AUTORUN=0"),

                UseShellExecute = false,
                WorkingDirectory = settings.DefaultSettings.DefaultProjectOutputPath
            };

            p.StartInfo = info;

            p.Exited += (sender, eventArgs) =>
            {
                reportReader.ReportTemplatePath = settings.DefaultSettings.DefaultProjectOutputPath + "\\"
                + (settings.DefaultSettings.CreateSubdirectoryIsEnabled == true ? CheckProcessService.CurrentCardCheckProcess.JobNr + "\\" : string.Empty) 
                + CheckProcessService.CurrentCardCheckProcess.JobNr + "-" 
                + CheckProcessService.CurrentCardCheckProcess.ChipNumber 
                + ".pdf";

                supported = reportReader.GetReportField("CheckBox_isChipSuppYes") != null && reportReader.GetReportField("CheckBox_isChipSuppYes") == "Yes";
                programmable = reportReader.GetReportField("CheckBox_ChipCanUseYes") != null && reportReader.GetReportField("CheckBox_ChipCanUseYes") == "Yes";
                freeMemField = reportReader.GetReportField("TextBox_Detail_FreeMem_1") ?? "NA";
                chipType = reportReader.GetReportField("TextBox_ChipType") ?? "NA";
                addHintsText = reportReader.GetReportField("TextBox_Hints") ?? "NA";
                
            };

            p.Start();

            await p.WaitForExitAsync();

            TextBoxAdditionalHints = addHintsText;

            if (freeMemField != null)
            {
                if (int.TryParse(freeMemField.Split(' ')[0], out amountOfFreeMemory))
                {
                    //seem's to be a desfire
                    TextBlockFreeMem = freeMemField;

                    if (amountOfFreeMemory >= 225) 
                    { 
                        notEnoughFreeMemory = false;

                        //show only usable templates on desfire
                        CardTemplates = new List<LSMCardTemplate>(
                            CardTemplates.Where(x => x.TemplateText.Contains("MD"))
                            .Where(x => x.SizeInBytes <= amountOfFreeMemory));
                    }
                    else
                    {
                        notEnoughFreeMemory = true;
                    }
                }

                else if(freeMemField.Split(',').Count() >= 1)
                {
                    TextBlockFreeMem = freeMemField;

                    var sectors = freeMemField.Split(',');

                    if (sectors.Length >= 1)
                    {
                        notEnoughFreeMemory = false;

                        CardTemplates = new List<LSMCardTemplate>(
                            CardTemplates.Where(x => x.TemplateText.Contains("MC"))
                            .Where(x => x.SizeInFreeSectorsCount <= sectors.Length));
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
            // Select "MD4000L_AV" Template... if any. OR Select "MC1000L_AV" if any.
            // Select first item if no MD4000L_AV nor MC1000L_AV is available
            SelectedLSMCardTemplate = CardTemplates.Any(
                x => x.TemplateText.Contains("MD4000L_AV")) 
                ? CardTemplates.FirstOrDefault(y => y.TemplateText.Contains("MD4000L_AV")) ?? new LSMCardTemplate() 
                : CardTemplates.Any(x => x.TemplateText.Contains("MC1000L_AV")) 
                    ? CardTemplates.FirstOrDefault(y => y.TemplateText.Contains("MC1000L_AV")) ?? new LSMCardTemplate()
                    : CardTemplates.FirstOrDefault() ?? new LSMCardTemplate();


            if (!supported)
            {
                Debug.WriteLine("supported only");

                TextBlockCheckFinishedAndResultIsNotSuppIsVisible = true;
                HyperlinkButtonReportIsVisible = true;

                NextStepButtonContent = "Fertigstellen";
                NextStepCanExecute = true;
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
                IsSupported = true;
                InfoBarSupportedChipType = chipType;

                if(chipType.ToLower().Contains("classic"))
                {
                    IsSupportedAndIsClassicChip = true;
                }

                HyperlinkButtonReportIsVisible = true;

                NextStepCanExecute = false;

            }
        }

        catch (Exception e)
        {
            LogWriter.CreateLogEntry(e);

            scanChipTimer.Stop(); 
            scanChipTimer.Tick -= ScanChipEvent;

            NextStepCanExecute = false;

            await App.MainRoot.MessageDialogAsync(
                "Fehler beim starten von RFIDGear.\n",
                string.Format("Bitte melde den folgenden Fehler an mich:\n{0}",e.Message));

            throw new InvalidOperationException(e.Message);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="readerService"></param>
    /// <returns></returns>
    private async Task ChipIsRemoved(ReaderService readerService)
    {
        try
        {
            scanChipTimer.Stop();

            await Task.Delay(1000).WaitAsync(token);

            if (await readerService.ReadChipPublic().WaitAsync(token) >= 0)
            {
                while (readerService.GenericChip != null && !isCancelled)
                {
                    await readerService.ReadChipPublic().WaitAsync(token);
                    if (readerService.GenericChip == null)
                    {
                        continue;
                    }
                    else
                    {
                        await Task.Delay(1500).WaitAsync(token);
                    }
                }
            }

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
    /// <param name="readerService"></param>
    /// <returns></returns>
    private async Task ChipIsPlacedAgain(ReaderService readerService)
    {
        try
        {
            scanChipTimer.Stop();

            await Task.Delay(100).WaitAsync(token);
            await readerService.ReadChipPublic().WaitAsync(token);

            while (readerService.GenericChip == null && !NextStepCanExecute)
            {
                if(isCancelled)
                { 
                    return; 
                }

                await readerService.ReadChipPublic().WaitAsync(token);

                if (readerService.GenericChip != null)
                {
                    continue;
                }
                else
                {
                    await Task.Delay(1500).WaitAsync(token);
                }
            }

            NextStepCanExecute = true;

            scanChipTimer.Start();
        }

        catch (Exception e)
        {
            LogWriter.CreateLogEntry(e);
            scanChipTimer.Tick -= ScanChipEvent;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ScanChipEvent(object? sender, object e)
    {
        if(!isCancelled)
        {
            try
            {
                scanChipTimer.Stop();

                using (ReaderService readerService = new ReaderService())
                {
                    if(!NavigateNextStepCommand.IsRunning)
                    {
                        await readerService.ReadChipPublic();
                    }

                    if (readerService.MoreThanOneReaderFound)
                    {
                        NextStepCanExecute = false;
                    }

                    else
                    {
                        if (readerService.GenericChip != null && !NextStepCanExecute && !NavigateNextStepCommand.IsRunning)
                        {
                            try
                            {
                                await ChipIsRemoved(readerService).WaitAsync(token);

                                ReaderHasNoChipInfoBarIsVisible = true;

                                await ChipIsPlacedAgain(readerService).WaitAsync(token);

                                var ChipInfoMessage = string.Format("Es wurde ein Chip erkannt:\nErkannt 1: {0}", readerService.GenericChip?.CardType.ToString() ?? "");

                                if (!isCancelled && readerService.GenericChip?.Child != null)
                                {
                                    ChipInfoMessage = ChipInfoMessage + string.Format("\nErkannt 2: {0}", readerService.GenericChip.Child.CardType);
                                }

                                NextStepCanExecute = true;
                            }
                            catch (Exception ae)
                            {
                                //readerService.Close();
                            }
                        }

                        if (!isCancelled && readerService.GenericChip != null && NextStepCanExecute && !NavigateNextStepCommand.IsRunning)
                        {

                            ReaderHasNoChipInfoBarIsVisible = false;

                            var ChipInfoMessage = string.Format("Es wurde ein Chip erkannt:\nErkannt 1: {0}", readerService.GenericChip.CardType.ToString());

                            if (readerService.GenericChip?.Child != null)
                            {
                                ChipInfoMessage = ChipInfoMessage + string.Format("\nErkannt 2: {0}", readerService.GenericChip.Child.CardType);
                            }

                            NextStepCanExecute = true;
                        }

                        else if (!isCancelled && readerService.GenericChip == null && !NavigateNextStepCommand.IsRunning)
                        {
                            NextStepCanExecute = true;
                            ReaderHasNoChipInfoBarIsVisible = true;
                        }
                    }
                }

                scanChipTimer.Start();
            }

            catch (Exception ex)
            {
                await App.MainRoot.MessageDialogAsync(
                    "Fehler",
                    string.Format("Bitte melde den folgenden Fehler an mich:\n{0}", ex.Message));

                LogWriter.CreateLogEntry(ex);
            }
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
            scanChipTimer.Stop();
            scanChipTimer.Tick -= ScanChipEvent;

            await ExecuteRFIDGearCommand();

            scanChipTimer.Tick += ScanChipEvent;
            scanChipTimer.Start();
        }
        catch(Exception ex)
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
            IsReportOpen = true;
            settings.ReadSettings();

            var p = new Process();

            var info = new ProcessStartInfo
            {
                FileName = settings.DefaultSettings.DefaultProjectOutputPath + "\\"
                + (settings.DefaultSettings.CreateSubdirectoryIsEnabled == true ? CheckProcessService.CurrentCardCheckProcess.JobNr + "\\" : string.Empty)
                + CheckProcessService.CurrentCardCheckProcess.JobNr + "-" 
                + CheckProcessService.CurrentCardCheckProcess.ChipNumber 
                + ".pdf",
                Verb = "",     
                UseShellExecute = true
            };

            p.StartInfo = info;
            p.Start();

            await p.WaitForExitAsync();

            IsReportOpen = false;
        }
        catch (Exception ex)
        {
            await App.MainRoot.MessageDialogAsync(
                "Fehler",
                string.Format("Bitte melde den folgenden Fehler an mich:\n{0}", ex.Message));

            LogWriter.CreateLogEntry(ex);
            IsReportOpen = false;
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

            var info = new ProcessStartInfo
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

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private async Task NavigateNextStepCommand_Executed()
    {
        try
        {
            using ReaderService readerService = new ReaderService();

            scanChipTimer.Stop();
            scanChipTimer.Tick -= ScanChipEvent;

            using SettingsReaderWriter settings = new SettingsReaderWriter();
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

            using ReportReaderWriterService reportReader = new ReportReaderWriterService();

            var window = (Application.Current as App)?.Window as MainWindow ?? new MainWindow();
            var navigation = window.Navigation;
            NavigationViewItem nextpage = navigation.GetNavigationViewItems(typeof(HomePage)).First();

            if (TextBlockCheckFinishedAndResultIsSuppOnlyIsVisible)
            {
                reportReader.ReportTemplatePath = preFinalPath;
                reportReader.ReportOutputPath = finalPath;
                reportReader.SetReadOnlyOnAllFields(true);

                nextpage = navigation.GetNavigationViewItems(typeof(HomePage)).First();

                List<CardCheckProcess> cardChecks = SQLDBService.Instance.CardChecks;

                var fileName = finalPath;
                var fileStream = File.Open(fileName, FileMode.Open);

                await SQLDBService.Instance.InsertData(CheckProcessService.CurrentCardCheckProcess.ID, fileStream);
                await SQLDBService.Instance.InsertData(CheckProcessService.CurrentCardCheckProcess.ID, OrderStatus.CheckFinished.ToString());

                fileStream.Close();
            }

            else if (TextBlockCheckFinishedAndResultIsNotSuppIsVisible)
            {
                reportReader.ReportTemplatePath = preFinalPath;
                reportReader.ReportOutputPath = finalPath;
                reportReader.SetReadOnlyOnAllFields(true);

                nextpage = navigation.GetNavigationViewItems(typeof(HomePage)).First();

                List<CardCheckProcess> cardChecks = SQLDBService.Instance.CardChecks;

                var fileName = finalPath;
                var fileStream = File.Open(fileName, FileMode.Open);

                await SQLDBService.Instance.InsertData(CheckProcessService.CurrentCardCheckProcess.ID, fileStream);
                await SQLDBService.Instance.InsertData(CheckProcessService.CurrentCardCheckProcess.ID, OrderStatus.CheckFinished.ToString());

                fileStream.Close();

                if (settings.DefaultSettings.RemoveTemporaryReportsIsEnabled == true)
                {
                    try
                    {
                        File.Delete(preFinalPath);
                        File.Delete(semiFinalPath);
                    }
                    catch (Exception ex)
                    {
                        await App.MainRoot.MessageDialogAsync(
                            "Fehler",
                            string.Format("Bitte melde den folgenden Fehler an mich:\n{0}", ex.Message));
                    } //The Files may be opened

                }
            }

            else if (TextBlockCheckFinishedAndResultIsSuppAndProgIsVisible)
            {
                reportReader.ReportTemplatePath = preFinalPath;
                reportReader.ReportOutputPath = semiFinalPath;

                if (CheckBoxChipProgrammableYes == true)
                {
                    reportReader.SetReportField("CheckBox_Detail_Reserved_2_1", "Yes");
                    reportReader.SetReportField("CheckBox_Detail_Reserved_2_2", "Off");
                }

                else if (CheckBoxChipProgrammableNo == true)
                {
                    reportReader.SetReportField("CheckBox_Detail_Reserved_2_1", "Off");
                    reportReader.SetReportField("CheckBox_Detail_Reserved_2_2", "Yes");
                }

                if (CheckBoxTestOnLockSuccess == true)
                {
                    reportReader.SetReportField("CheckBox_Detail_Reserved_3_1", "Yes");
                    reportReader.SetReportField("CheckBox_Detail_Reserved_3_2", "Off");
                }

                else if (CheckBoxTestOnLockFailed == true)
                {
                    reportReader.SetReportField("CheckBox_Detail_Reserved_3_1", "Off");
                    reportReader.SetReportField("CheckBox_Detail_Reserved_3_2", "Yes");
                }

                if (CheckBoxTestOnLockLimitedNo == true)
                {
                    reportReader.SetReportField("CheckBox_Detail_Reserved_4_2", "Yes");
                    reportReader.SetReportField("CheckBox_Detail_Reserved_4_1", "Off");
                }

                else if (CheckBoxTestOnLockLimitedYes == true)
                {
                    reportReader.SetReportField("CheckBox_Detail_Reserved_4_2", "Off");
                    reportReader.SetReportField("CheckBox_Detail_Reserved_4_1", "Yes");

                    reportReader.SetReportField("TextBox_Hints", TextBoxAdditionalHints);
                }

                reportReader.SetReportField("ComboBox_UsedTemplate",SelectedLSMCardTemplate.TemplateText);
                reportReader.SetReportField("TextBox_Detail_Mem_3", TextBoxSectorsUsed);

                nextpage = navigation.GetNavigationViewItems(typeof(Step3Page)).First();
            }

            await SQLDBService.Instance.InsertData("CC-ChangedDate", CheckProcessService.CurrentCardCheckProcess.ID, DateTime.Now.ToString());

            navigation.SetCurrentNavigationViewItem(nextpage);
            nextpage.IsEnabled = true;
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
    private async Task NavigateBackCommand_Executed()
    {
        try
        {
            scanChipTimer.Stop();
            scanChipTimer.Tick -= ScanChipEvent;

            source.Cancel();

            await Task.Delay(1000);

            var window = (Application.Current as App)?.Window as MainWindow ?? new MainWindow();
            var navigation = window.Navigation;
            var step1Page = navigation.GetNavigationViewItems(typeof(Step1Page)).First();
            navigation.SetCurrentNavigationViewItem(step1Page);
        }
        catch (Exception ex)
        {
            await App.MainRoot.MessageDialogAsync(
                "Fehler",
                string.Format("Bitte melde den folgenden Fehler an mich:\n{0}", ex.Message));

            LogWriter.CreateLogEntry(ex);
        }
    }
}

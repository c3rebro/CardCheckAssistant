﻿using System.Collections.ObjectModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Windows.Input;
using CardCheckAssistant.Contracts.ViewModels;
using CardCheckAssistant.Models;
using CardCheckAssistant.Services;
using CardCheckAssistant.Views;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Elatec.NET;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json.Linq;

namespace CardCheckAssistant.ViewModels;

public partial class Step2PageViewModel : ObservableRecipient, INavigationAware
{
    private readonly DispatcherTimer scanChipTimer;
    private readonly EventLog eventLog = new("Application", ".", Assembly.GetEntryAssembly().GetName().Name);

    private bool chipWasRemovedAndPlacedAgain;

    public Step2PageViewModel()
    {
        using var settings = new SettingsReaderWriter();
        
        chipWasRemovedAndPlacedAgain = false;
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

        LsmCardTemplates = new List<LSMCardTemplate>()
        {
            new ("N/A", 0, 0),
            new ("MCBasic (Speicherbedarf: 1 Sektor)", 0, 1),
            new ("MC1200L (Speicherbedarf: 4 Sektoren)", 0, 4),
            new ("MC3800L (Speicherbedarf: 11 Sektoren)", 0 , 11),
            new ("MC1000L_AV (Speicherbedarf: 11 Sektoren)", 0 , 11),
            new ("MC2400L_AV (Speicherbedarf: 19 Sektoren)", 0, 19),
            new ("MC8000L_AV (Speicherbedarf: 43 Sektoren / 32 + 3)", 0, 43),
            new ("MDBasic (Speicherbedarf: 48 + 176 = 224 Bytes)", 224, 0),
            new ("MD1200L (Speicherbedarf: 192 + 160 = 352 Bytes)", 352, 0),
            new ("MD3800L (Speicherbedarf: 528 + 176 = 704 Bytes)", 704, 0),
            new ("MD2500L_AV (Speicherbedarf: 1024 + 160 = 1184 Bytes)", 1184, 0),
            new ("MD4000L_AV (Speicherbedarf: 1600 + 160 = 1760 Bytes)", 1760, 0),
            new ("MD10000L_AV (Speicherbedarf: 3048 + 184 = 3232 Bytes)", 1760, 0),
            new ("MD32000L_AV (Speicherbedarf: 7000 + 168 = 7168 Bytes)", 7168, 0)
        };

        TextTemplates = settings.DefaultSettings.CardCheckTextTemplates ?? new ObservableCollection<CardCheckTextTemplate>(new());

        SelectedLSMCardTemplate = LsmCardTemplates.Single(x => x.TemplateText == "N/A");

        TextBoxSectorsUsed = "";

        NextStepButtonContent = "Weiter";
    }


    #region ObservableProperties
    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private List<LSMCardTemplate> _lsmCardTemplates;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<CardCheckTextTemplate> _textTemplates;

    /// <summary>
    /// 
    /// </summary>
    public CardCheckTextTemplate SelectedCardCheckTextTemplate
    {
        get => _selectedCardCheckTextTemplate;
        set
        {
            SetProperty(ref _selectedCardCheckTextTemplate, value);
            TextBoxAdditionalHints = string.Format("{1}{0}", staticTextBoxAdditionalHints, !string.IsNullOrEmpty(SelectedCardCheckTextTemplate.TemplateTextContent) ? SelectedCardCheckTextTemplate.TemplateTextContent + "\n\n" : string.Empty);
        }
    }
    private CardCheckTextTemplate _selectedCardCheckTextTemplate;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private string _textBoxSectorsUsed;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private string _infoBarSupportedChipType;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private string _infoBarReportOpen;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private string _textBoxAdditionalHints;
    private string staticTextBoxAdditionalHints;

    /// <summary>
    /// Do not continue if PDF File is open. Show Dialog
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
            NextStepCanExecute = true;
            if (_checkBoxChipProgrammableNo)
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
            NextStepCanExecute = true;
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
            NextStepCanExecute = true;
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
            NextStepCanExecute = true;
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
            NextStepCanExecute = true;
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
            NextStepCanExecute = true;
            if (_checkBoxTestOnLockLimitedYes)
            {
                CheckBoxTestOnLockLimitedYes = !value;
            }
        }
    }
    private bool _checkBoxTestOnLockLimitedNo;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private string _textBlockFreeMem;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private LSMCardTemplate _selectedLSMCardTemplate;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private string _nextStepButtonContent;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private bool _isSupported;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private bool _isSupportedAndIsClassicChip;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private bool _readerHasNoChipInfoBarIsVisible;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private bool _textBlockCheckFinishedAndResultIsNotEnoughMemoryIsVisible;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private bool _textBlockCheckFinishedAndResultIsMissingPICCKeyIsVisible;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private bool _textBlockCheckFinishedAndResultIsNotSuppIsVisible;

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
    public bool NextStepCanExecute
    {
        get => _nextStepCanExecute;
        set
        {

            if (TextBlockCheckFinishedAndResultIsNotSuppIsVisible ||
                ((CheckBoxChipProgrammableNo || CheckBoxChipProgrammableYes) &&
                (CheckBoxTestOnLockFailed || CheckBoxTestOnLockSuccess) &&
                (CheckBoxTestOnLockLimitedNo || CheckBoxTestOnLockLimitedYes) && chipWasRemovedAndPlacedAgain))
            {
                SetProperty(ref _nextStepCanExecute, value);
            }
            else
            {
                SetProperty(ref _nextStepCanExecute, false);
            }


        }
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
    /// Show The JobNumber on each Step
    /// </summary>
    public static string JobNumber => string.Format("JobNr.: {0}; ChipNummer: {1}; Kunde: {2}", CheckProcessService.CurrentCardCheckProcess.JobNr, CheckProcessService.CurrentCardCheckProcess.ChipNumber, CheckProcessService.CurrentCardCheckProcess.CName);

    /// <summary>
    /// Show The Language on each Step
    /// </summary>
    public static string ReportLanguage => string.Format("{0}", CheckProcessService.CurrentCardCheckProcess.ReportLanguage);

    /// <summary>
    /// 
    /// </summary>
    public ObservableCollection<string> ChipCount
    {
        get; set;
    }

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
    public ICommand NavigateBackCommand => new AsyncRelayCommand(NavigateBackCommand_Executed);

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
    public ICommand OpenReportCommand => new AsyncRelayCommand(OpenReportCommand_Executed);

    /// <summary>
    /// 
    /// </summary>
    public ICommand OpenReportPathCommand => new AsyncRelayCommand(OpenReportPathCommand_Executed);

    /// <summary>
    /// launch rfidgear to test the chip and create a report
    /// </summary>
    /// <returns>false on NoChipFound, true otherwise</returns>
    private async Task<bool> ExecuteRFIDGearCommand()
    {
        try
        {
            NextStepCanExecute = false;
            using var reader = TWN4ReaderDevice.Instance[0];

            await Task.Delay(1000);
            await reader.DisconnectAsync();

            var amountOfFreeMemory = 0;
            var freeMemField = "N/A";
            TextBlockFreeMem = freeMemField;

            var chipType = "";
            InfoBarSupportedChipType = chipType;

            var notEnoughFreeMemory = true;
            var supported = false;
            var programmable = false;
            var addHintsText = "";

            using var reportReader = new ReportReaderWriterService();
            using var settings = new SettingsReaderWriter();

            settings.ReadSettings();

            if (settings?.DefaultSettings.CreateSubdirectoryIsEnabled == true)
            {
                if (!Directory.Exists(
                    settings.DefaultSettings.DefaultProjectOutputPath + "\\"
                    + (CheckProcessService.CurrentCardCheckProcess.JobNr + "\\"))
                    )
                {
                    Directory.CreateDirectory(
                        settings.DefaultSettings.DefaultProjectOutputPath + "\\"
                    + (CheckProcessService.CurrentCardCheckProcess.JobNr + "\\"));
                }
            }

            var p = new Process();

            var language = settings.DefaultSettings.DefaultReportLanguage.ToLower();
            var defaultProjectFile = settings.DefaultSettings.LastUsedDefaultProject;
            var directory = Path.GetDirectoryName(defaultProjectFile);
            var filenameWithoutExtension = Path.GetFileNameWithoutExtension(defaultProjectFile);
            var extension = Path.GetExtension(defaultProjectFile);

            // Construct the language-specific filename
            var languageSpecificFile = Path.Combine(directory, $"{filenameWithoutExtension}.{language}{extension}");

            // Use the language-specific file if it exists, otherwise use the default
            var projectFileToUse = File.Exists(languageSpecificFile) ? languageSpecificFile : defaultProjectFile;

            var info = new ProcessStartInfo
            {
                FileName = settings.DefaultSettings.DefaultRFIDGearExePath,
                Verb = "",
                Arguments = string.Format(
                    "REPORTTARGETPATH=" + "\"" + "{0}" + "\" " +
                    "CUSTOMPROJECTFILE=" + "\"" + "{1}" + "\" " +
                    "$JOBNUMBER=" + "\"" + "{2}" + "\" " +
                    "$CHIPNUMBER=" + "\"" + "{3}" + "\" " +
                    "{4}",
                    settings.DefaultSettings.DefaultProjectOutputPath + "\\"
                    + (settings.DefaultSettings.CreateSubdirectoryIsEnabled == true ? CheckProcessService.CurrentCardCheckProcess.JobNr + "\\" : string.Empty)
                    + CheckProcessService.CurrentCardCheckProcess.JobNr + "-"
                    + CheckProcessService.CurrentCardCheckProcess.ChipNumber
                    + ".step1.pdf",
                    projectFileToUse,
                    CheckProcessService.CurrentCardCheckProcess.JobNr,
                    CheckProcessService.CurrentCardCheckProcess.ChipNumber,
                    (settings.DefaultSettings.AutoRunProjectOnStart ?? false) ? "AUTORUN=1" : "AUTORUN=0"),

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
                + ".step1.pdf";

                supported = reportReader.GetReportField("CheckBox_isChipSuppYes") != null && reportReader.GetReportField("CheckBox_isChipSuppYes") == "Yes";
                programmable = reportReader.GetReportField("CheckBox_ChipCanUseYes") != null && reportReader.GetReportField("CheckBox_ChipCanUseYes") == "Yes";
                freeMemField = reportReader.GetReportField("TextBox_Detail_FreeMem_1") ?? "NA";
                chipType = reportReader.GetReportField("TextBox_ChipType") ?? "NA";
                addHintsText = reportReader.GetReportField("TextBox_Notes_Site_2") ?? "NA";

            };

            p.Start();

            await p.WaitForExitAsync();

            TextBoxAdditionalHints = addHintsText;
            staticTextBoxAdditionalHints = addHintsText;

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
                        LsmCardTemplates = new List<LSMCardTemplate>(
                            LsmCardTemplates.Where(x => x.TemplateText.Contains("MD"))
                            .Where(x => x.SizeInBytes <= amountOfFreeMemory));
                    }
                    else
                    {
                        notEnoughFreeMemory = true;
                    }
                }

                else if (freeMemField.Split(',').Length >= 1)
                {
                    TextBlockFreeMem = freeMemField;

                    var sectors = freeMemField.Split(',');

                    if (sectors.Length >= 1)
                    {
                        notEnoughFreeMemory = false;

                        LsmCardTemplates = new List<LSMCardTemplate>(
                            LsmCardTemplates.Where(x => x.TemplateText.Contains("MC"))
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
            SelectedLSMCardTemplate = LsmCardTemplates.Any(
                x => x.TemplateText.Contains("MD4000L_AV"))
                ? LsmCardTemplates.FirstOrDefault(y => y.TemplateText.Contains("MD4000L_AV")) ?? new LSMCardTemplate()
                : LsmCardTemplates.Any(x => x.TemplateText.Contains("MC1000L_AV"))
                    ? LsmCardTemplates.FirstOrDefault(y => y.TemplateText.Contains("MC1000L_AV")) ?? new LSMCardTemplate()
                    : LsmCardTemplates.FirstOrDefault() ?? new LSMCardTemplate();

            if (!programmable)
            {
                scanChipTimer.Stop();
                scanChipTimer.Tick -= ScanChipEvent;

                TextBlockCheckFinishedAndResultIsNotSuppIsVisible = true;
                HyperlinkButtonReportIsVisible = true;
                TextBlockCheckFinishedIsVisible = true;

                return false;
            }

            else if (supported && !programmable && !notEnoughFreeMemory)
            {
                Debug.WriteLine("supported but missing info e.g. key");

                TextBlockCheckFinishedAndResultIsSuppOnlyIsVisible = true;
                TextBlockCheckFinishedAndResultIsMissingPICCKeyIsVisible = true;
                HyperlinkButtonReportIsVisible = true;

                return false;
            }

            else if (supported && !programmable && notEnoughFreeMemory || (supported && programmable && notEnoughFreeMemory))
            {
                Debug.WriteLine("supported but no memory left");

                TextBlockCheckFinishedAndResultIsSuppOnlyIsVisible = true;
                TextBlockCheckFinishedAndResultIsNotEnoughMemoryIsVisible = true;
                HyperlinkButtonReportIsVisible = true;

                return false;
            }

            else if (supported && programmable)
            {
                Debug.WriteLine("programmable");

                TextBlockCheckFinishedAndResultIsSuppAndProgIsVisible = true;
                IsSupported = true;
                InfoBarSupportedChipType = chipType;

                if (chipType.ToLower().Contains("classic"))
                {
                    IsSupportedAndIsClassicChip = true;
                }

                HyperlinkButtonReportIsVisible = true;

                NextStepCanExecute = false;

            }
        }

        catch (Exception e)
        {
            eventLog.WriteEntry(e.Message, EventLogEntryType.Error);

            scanChipTimer.Stop();
            scanChipTimer.Tick -= ScanChipEvent;

            NextStepCanExecute = false;

            await App.MainRoot.MessageDialogAsync(
                "Fehler beim starten von RFIDGear.",
                string.Format("Bitte melde den folgenden Fehler an mich:\n{0}", e.Message));

            throw new InvalidOperationException(e.Message);
        }

        return true;
    }

    /// <summary>
    /// a little delay fake forcing chip needs to be tested in lsm
    /// </summary>
    /// <param name="readerService"></param>
    /// <returns></returns>
    private async Task ChipIsRemoved(ReaderService readerService)
    {
        try
        {
            scanChipTimer.Stop();

            if (await readerService.ReadChipPublic() >= 0)
            {
                while (readerService.DetectedChips != null && readerService.DetectedChips.Count > 0)
                {
                    await readerService.ReadChipPublic();

                    if (readerService.DetectedChips != null && readerService.DetectedChips.Count > 0 && chipWasRemovedAndPlacedAgain)
                    {
                        await Task.Delay(500);
                        break;
                    }

                    if (readerService.DetectedChips == null || readerService.DetectedChips.Count == 0)
                    {
                        continue;
                    }
                    else
                    {
                        await Task.Delay(1500);
                    }
                }
            }

            scanChipTimer.Start();
        }
        catch (Exception e)
        {
            eventLog.WriteEntry(e.Message, EventLogEntryType.Error);
        }
    }

    /// <summary>
    /// Chip was placed again after lsm programming
    /// </summary>
    /// <param name="readerService"></param>
    /// <returns></returns>
    private async Task ChipIsPlacedAgain(ReaderService readerService)
    {
        try
        {
            scanChipTimer.Stop();

            await Task.Delay(100);
            await readerService.ReadChipPublic();

            while ((readerService.DetectedChips == null || readerService.DetectedChips.Count == 0) && !NextStepCanExecute)
            {
                await readerService.ReadChipPublic();

                if (readerService.DetectedChips != null && readerService.DetectedChips.Count > 0)
                {
                    continue;
                }
                else
                {
                    await Task.Delay(1500);
                }
            }

            chipWasRemovedAndPlacedAgain = true;
            NextStepCanExecute = true;

            scanChipTimer.Start();
        }
        catch (Exception e)
        {
            eventLog.WriteEntry(e.Message, EventLogEntryType.Error);
            scanChipTimer.Tick -= ScanChipEvent;
        }
    }


    /// <summary>
    /// Is the Chip removed to test in LSM?
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /*
    private async void ScanChipEvent(object? sender, object e)
    {
        try
        {
            scanChipTimer.Stop();

            using (var readerService = new ReaderService())
            {
                if (!NavigateNextStepCommand.IsRunning)
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
                            await ChipIsRemoved(readerService);

                            ReaderHasNoChipInfoBarIsVisible = true;

                            await ChipIsPlacedAgain(readerService);

                            var ChipInfoMessage = string.Format("Es wurde ein Chip erkannt:\nErkannt 1: {0}",
                                readerService.GenericChip.TCard.SecondaryType == MifareChipSubType.Unspecified ?
                                readerService.GenericChip.TCard.PrimaryType.ToString() :
                                readerService.GenericChip.TCard.SecondaryType.ToString() ?? "");

                            if (readerService.GenericChip?.HasChilds == true)
                            {
                                ChipInfoMessage += string.Format("\nErkannt 2: {0}",
                                    readerService.GenericChip.Childs[0].TCard.SecondaryType == MifareChipSubType.Unspecified ?
                                    readerService.GenericChip.Childs[0].TCard.PrimaryType.ToString() :
                                    readerService.GenericChip.Childs[0].TCard.SecondaryType.ToString());
                            }

                            NextStepCanExecute = true;
                        }
                        catch
                        {
                            // No Chip found
                        }
                    }

                    if (readerService.GenericChip != null && NextStepCanExecute && !NavigateNextStepCommand.IsRunning)
                    {

                        ReaderHasNoChipInfoBarIsVisible = false;

                        var ChipInfoMessage = string.Format("Es wurde ein Chip erkannt:\nErkannt 1: {0}",
                            readerService.GenericChip.TCard.SecondaryType == MifareChipSubType.Unspecified ?
                            readerService.GenericChip.TCard.PrimaryType.ToString() :
                            readerService.GenericChip.TCard.SecondaryType.ToString());

                        if (readerService.GenericChip?.HasChilds == true)
                        {
                            ChipInfoMessage += string.Format("\nErkannt 2: {0}",
                                readerService.GenericChip.Childs[0].TCard.SecondaryType == MifareChipSubType.Unspecified ?
                                readerService.GenericChip.Childs[0].TCard.PrimaryType.ToString() :
                                readerService.GenericChip.Childs[0].TCard.SecondaryType.ToString());
                        }

                        NextStepCanExecute = true;
                    }

                    else if (readerService.GenericChip == null && !NavigateNextStepCommand.IsRunning)
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

            eventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
        }
    }
    */

    /// <summary>
    /// Is the Chip removed to test in LSM?
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ScanChipEvent(object? sender, object e)
    {
        try
        {
            scanChipTimer.Stop();

            using (var readerService = new ReaderService())
            {
                if (!NavigateNextStepCommand.IsRunning)
                {
                    await readerService.ReadChipPublic();
                }

                if (readerService.MoreThanOneReaderFound)
                {
                    NextStepCanExecute = false;
                }
                else
                {
                    var detectedChips = readerService.DetectedChips;

                    if (detectedChips != null && detectedChips.Count > 0 && !NextStepCanExecute && !NavigateNextStepCommand.IsRunning)
                    {
                        try
                        {
                            await ChipIsRemoved(readerService);

                            ReaderHasNoChipInfoBarIsVisible = true;

                            await ChipIsPlacedAgain(readerService);

                            var ChipInfoMessage = "Es wurden folgende Chips erkannt:";
                            foreach (var chip in detectedChips)
                            {
                                ChipInfoMessage += $"\n{chip.ChipType}";
                            }

                            NextStepCanExecute = true;
                        }
                        catch
                        {
                            // No Chip found
                        }
                    }

                    if (detectedChips != null && detectedChips.Count > 0 && NextStepCanExecute && !NavigateNextStepCommand.IsRunning)
                    {
                        ReaderHasNoChipInfoBarIsVisible = false;

                        var ChipInfoMessage = "Es wurden folgende Chips erkannt:";
                        foreach (var chip in detectedChips)
                        {
                            ChipInfoMessage += $"\n{chip.ChipType}";
                        }

                        NextStepCanExecute = true;
                    }
                    else if ((detectedChips == null || detectedChips.Count == 0) && !NavigateNextStepCommand.IsRunning)
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
                $"Bitte melde den folgenden Fehler an mich:\n{ex.Message}");

            eventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
        }
    }

    /// <summary>
    /// Disconnect Reader before RFIDGear Execution
    /// </summary>
    /// <returns></returns>
    public async Task PostPageLoadedCommand_Executed()
    {
        try
        {
            scanChipTimer.Stop();
            scanChipTimer.Tick -= ScanChipEvent;

            var reader = TWN4ReaderDevice.Instance[0];
            await reader.DisconnectAsync();

            if (await ExecuteRFIDGearCommand())
            {
                scanChipTimer.Tick += ScanChipEvent;
                scanChipTimer.Start();
            }
            else
            {
                NextStepCanExecute = true;
                NextStepButtonContent = "Fertigstellen";
                TextBlockCheckFinishedAndResultIsNotSuppIsVisible = true;
            }
        }
        catch (Exception ex)
        {
            await App.MainRoot.MessageDialogAsync(
                "Fehler",
                string.Format("Bitte melde den folgenden Fehler an mich:\n{0}", ex.Message));
            eventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
        }
    }

    /// <summary>
    /// Edit PDF Prior next Step
    /// </summary>
    private async Task OpenReportCommand_Executed()
    {
        try
        {
            using var settings = new SettingsReaderWriter();
            IsReportOpen = true;
            settings.ReadSettings();

            var p = new Process();

            var info = new ProcessStartInfo
            {
                FileName = settings.DefaultSettings.DefaultProjectOutputPath + "\\"
                + (settings.DefaultSettings.CreateSubdirectoryIsEnabled == true ? CheckProcessService.CurrentCardCheckProcess.JobNr + "\\" : string.Empty)
                + CheckProcessService.CurrentCardCheckProcess.JobNr + "-"
                + CheckProcessService.CurrentCardCheckProcess.ChipNumber
                + ".step1.pdf",
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

            eventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
            IsReportOpen = false;
        }
    }

    /// <summary>
    /// Show Project Directory prior next Step
    /// </summary>
    private async Task OpenReportPathCommand_Executed()
    {
        try
        {
            using var settings = new SettingsReaderWriter();
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
            eventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
        }
    }

    #endregion

    /// <summary>
    /// finalize PDF prior next Step
    /// </summary>
    /// <returns></returns>
    private async Task NavigateNextStepCommand_Executed()
    {
        try
        {
            scanChipTimer.Stop();
            scanChipTimer.Tick -= ScanChipEvent;

            using var settings = new SettingsReaderWriter();
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
                + ".step2.pdf";

            var preFinalPath =
                settings.DefaultSettings.DefaultProjectOutputPath + "\\"
                + (settings.DefaultSettings.CreateSubdirectoryIsEnabled == true ? CheckProcessService.CurrentCardCheckProcess.JobNr + "\\" : string.Empty)
                + CheckProcessService.CurrentCardCheckProcess.JobNr + "-"
                + CheckProcessService.CurrentCardCheckProcess.ChipNumber
                + ".step1.pdf";

            using var reportReader = new ReportReaderWriterService();

            if (TextBlockCheckFinishedAndResultIsSuppOnlyIsVisible)
            {
                reportReader.ReportTemplatePath = preFinalPath;
                reportReader.ReportOutputPath = finalPath;
                reportReader.SetReadOnlyOnAllFields(true);

                var cardChecks = SQLDBService.Instance.CardChecks;

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

                var cardChecks = SQLDBService.Instance.CardChecks;

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

                    reportReader.SetReportField("TextBox_Notes_Site_2", TextBoxAdditionalHints);
                }

                reportReader.SetReportField("ComboBox_UsedTemplate", SelectedLSMCardTemplate.TemplateText);
                reportReader.SetReportField("TextBox_Detail_Mem_3", TextBoxSectorsUsed);
            }

            await SQLDBService.Instance.InsertData("CC-ChangedDate", CheckProcessService.CurrentCardCheckProcess.ID, DateTime.Now.ToString());

            if (TextBlockCheckFinishedAndResultIsSuppAndProgIsVisible)
            {
                (App.MainRoot.XamlRoot.Content as ShellPage)?.ViewModel.NavigationService.NavigateTo(typeof(Step3PageViewModel).FullName ?? "");
            }
            else
            {
                (App.MainRoot.XamlRoot.Content as ShellPage)?.ViewModel.NavigationService.NavigateTo(typeof(HomePageViewModel).FullName ?? "");
            }
        }
        catch (Exception ex)
        {
            await App.MainRoot.MessageDialogAsync(
                "Fehler",
                string.Format("Bitte melde den folgenden Fehler an mich:\n{0}", ex.Message));

            eventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
        }
    }

    /// <summary>
    /// Disconnect Reader prior GoBack
    /// </summary>
    private async Task NavigateBackCommand_Executed()
    {
        try
        {
            var reader = TWN4ReaderDevice.Instance;

            scanChipTimer.Stop();
            scanChipTimer.Tick -= ScanChipEvent;

            await reader[0].DisconnectAsync();

            (App.MainRoot.XamlRoot.Content as ShellPage)?.ViewModel.NavigationService.NavigateTo(typeof(Step1PageViewModel).FullName ?? "");
        }
        catch (Exception ex)
        {
            await App.MainRoot.MessageDialogAsync(
                "Fehler",
                string.Format("Bitte melde den folgenden Fehler an mich:\n{0}", ex.Message));

            eventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
        }
    }

    /// <summary>
    /// INavigation Aware Event. Close Connection If Open
    /// </summary>
    /// <param name="parameter"></param>
    public async void OnNavigatedTo(object parameter)
    {
        // Run code when the app navigates to this page
        using var reader = TWN4ReaderDevice.Instance[0];

        await reader.DisconnectAsync();

    }

    /// <summary>
    /// INavigation Aware Event. Close Connection If Open
    /// </summary>
    public async void OnNavigatedFrom()
    {
        // Run code when the app navigates to this page
        using var reader = TWN4ReaderDevice.Instance[0];

        await reader.DisconnectAsync();
    }
}

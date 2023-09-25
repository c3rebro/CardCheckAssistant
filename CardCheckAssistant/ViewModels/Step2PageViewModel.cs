using System.Collections.ObjectModel;
using System.Diagnostics;
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
    private readonly DispatcherTimer scanChipTimer;

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
        scanChipTimer = new DispatcherTimer();
        scanChipTimer.Tick += ScanChipEvent;
        scanChipTimer.Interval = new TimeSpan(0, 0, 0, 0, 3000);
        scanChipTimer.Stop();

        NextStepCanExecute = false;
        GoBackCanExecute = true;

        NavigateNextStepCommand = new AsyncRelayCommand(NavigateNextStepCommand_Executed);

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
            new ("N/A", "Keine Auswahl"),
            new ("MCBasic (Speicherbedarf: 1 Sektor)", string.Empty),
            new ("MC1200L (Speicherbedarf: 4 Sektoren)", string.Empty),
            new ("MC3800L (Speicherbedarf: 11 Sektoren)", string.Empty),
            new ("MC1000L_AV (Speicherbedarf: 11 Sektoren)", string.Empty),
            new ("MC2400L_AV (Speicherbedarf: 19 Sektoren)", string.Empty),
            new ("MC8000L_AV (Speicherbedarf: 43 Sektoren / 32 + 3)", string.Empty),
            new ("MDBasic (Speicherbedarf: 48 + 176 = 224 Bytes)", string.Empty),
            new ("MD1200L (Speicherbedarf: 192 + 160 = 352 Bytes)", string.Empty),
            new ("MD3800L (Speicherbedarf: 528 + 176 = 704 Bytes)", string.Empty),
            new ("MD2500L_AV (Speicherbedarf: 1024 + 160 = 1184 Bytes)", string.Empty),
            new ("MD4000L_AV (Speicherbedarf: 1600 + 160 = 1760 Bytes)", string.Empty),
            new ("MD10000L_AV (Speicherbedarf: 3048 + 184 = 3232 Bytes)", string.Empty),
            new ("MD32000L_AV (Speicherbedarf: 7000 + 168 = 7168 Bytes)", string.Empty) 
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
    public bool CheckBoxChipProgrammableYes
    {
        get => _checkBoxChipProgrammableYes;
        set
        {
            SetProperty(ref _checkBoxChipProgrammableYes, value);
            CheckBoxChipProgrammableNo = !value;
        }
    }
    private bool _checkBoxChipProgrammableYes;

    /// <summary>
    /// 
    /// </summary>
    public bool CheckBoxChipProgrammableNo
    {
        get => _checkBoxChipProgrammableNo;
        set => SetProperty(ref _checkBoxChipProgrammableNo, value);
    }
    private bool _checkBoxChipProgrammableNo;

    /// <summary>
    /// 
    /// </summary>
    public bool CheckBoxTestOnLockSuccess
    {
        get => _checkBoxTestOnLockSuccess;
        set => SetProperty(ref _checkBoxTestOnLockSuccess, value);
    }
    private bool _checkBoxTestOnLockSuccess;

    /// <summary>
    /// 
    /// </summary>
    public bool CheckBoxTestOnLockFailed
    {
        get => _checkBoxTestOnLockFailed;
        set => SetProperty(ref _checkBoxTestOnLockFailed, value);
    }
    private bool _checkBoxTestOnLockFailed;

    /// <summary>
    /// 
    /// </summary>
    public bool CheckBoxTestOnLockLimitedYes
    {
        get => _checkBoxTestOnLockLimitedYes;
        set => SetProperty(ref _checkBoxTestOnLockLimitedYes, value);
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
            SetProperty(ref _checkBoxTestOnLockLimitedYes, !value);
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
    public ICommand NavigateBackCommand => new RelayCommand(NavigateBackCommand_Executed);

    /// <summary>
    /// 
    /// </summary>
    public ICommand PostPageLoadedCommand => new AsyncRelayCommand(PostPageLoadedCommand_Executed);

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
            int amountOfFreeMemory = 0;
            string freeMemField = "N/A";
            TextBlockFreeMem = freeMemField;

            string chipType = "";
            InfoBarSupportedChipType = chipType;

            bool notEnoughFreeMemory = true;
            bool supported = false;
            bool programmable = false;

            using ReportReaderWriterService reportReader = new ReportReaderWriterService();
            using SettingsReaderWriter settings = new SettingsReaderWriter();

            await Task.Delay(1000);

            settings.ReadSettings();

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
                    settings.DefaultSettings.DefaultProjectOutputPath + "\\" + CheckProcessService.CurrentCardCheckProcess.JobNr + "-" + CheckProcessService.CurrentCardCheckProcess.ChipNumber + ".pdf",
                    CheckProcessService.CurrentCardCheckProcess.JobNr,
                    CheckProcessService.CurrentCardCheckProcess.ChipNumber,
                    (settings.DefaultSettings.AutoLoadProjectOnStart ?? false) ? "AUTORUN=1" : "AUTORUN=0"),

                UseShellExecute = false,
                WorkingDirectory = settings.DefaultSettings.DefaultProjectOutputPath
            };

            p.StartInfo = info;

            p.Exited += (sender, eventArgs) =>
            {
                reportReader.ReportTemplatePath = settings.DefaultSettings.DefaultProjectOutputPath + "\\" + CheckProcessService.CurrentCardCheckProcess.JobNr + "-" + CheckProcessService.CurrentCardCheckProcess.ChipNumber + ".pdf";

                supported = reportReader.GetReportField("CheckBox_isChipSuppYes") != null && reportReader.GetReportField("CheckBox_isChipSuppYes") == "Yes";
                programmable = reportReader.GetReportField("CheckBox_ChipCanUseYes") != null && reportReader.GetReportField("CheckBox_ChipCanUseYes") == "Yes";
                freeMemField = reportReader.GetReportField("TextBox_Detail_FreeMem_1") ?? "NA";
                chipType = reportReader.GetReportField("TextBox_ChipType") ?? "NA";
            };

            p.Start();

            await p.WaitForExitAsync();

            if (freeMemField != null)
            {
                if (int.TryParse(freeMemField.Split(' ')[0], out amountOfFreeMemory))
                {
                    TextBlockFreeMem = freeMemField;

                    if (amountOfFreeMemory >= 225) 
                    { 
                        notEnoughFreeMemory = false;
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
                scanChipTimer.Start();
            }
        }

        catch (Exception e)
        {
            LogWriter.CreateLogEntry(e);

            scanChipTimer.Stop();

            await App.MainRoot.MessageDialogAsync(
                "Fehler beim starten von RFIDGear.\n",
                string.Format("Bitte melde den folgenden Fehler an mich:\n{0}",e.Message));

            NextStepCanExecute = false;
            return;
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

            await readerService.ReadChipPublic();

            while (readerService.GenericChip != null)
            {
                await readerService.ReadChipPublic();
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

            await readerService.ReadChipPublic();

            while (readerService.GenericChip == null)
            {
                await readerService.ReadChipPublic();
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
    /// <param name="sender"></param>
    /// <param name="e"></param>
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
        catch(Exception ex)
        {
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
            scanChipTimer.Stop();

            await ExecuteRFIDGearCommand();
        }
        catch(Exception e)
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

            var info = new ProcessStartInfo
            {
                FileName = settings.DefaultSettings.DefaultProjectOutputPath + "\\" + CheckProcessService.CurrentCardCheckProcess.JobNr + "-" + CheckProcessService.CurrentCardCheckProcess.ChipNumber + ".pdf",
                Verb = "",     
                UseShellExecute = true
            };

            p.StartInfo = info;

            p.Start();
        }
        catch (Exception ex)
        {
            LogWriter.CreateLogEntry(ex);
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

            var info = new ProcessStartInfo
            {
                FileName = settings.DefaultSettings.DefaultProjectOutputPath + "\\",
                Verb = "",
                UseShellExecute = true
            };

            p.StartInfo = info;

            p.Start();
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

            string finalPath = settings.DefaultSettings.DefaultProjectOutputPath + "\\" + CheckProcessService.CurrentCardCheckProcess.JobNr + "-" + CheckProcessService.CurrentCardCheckProcess.ChipNumber + "_final.pdf";
            string semiFinalPath = settings.DefaultSettings.DefaultProjectOutputPath + "\\" + CheckProcessService.CurrentCardCheckProcess.JobNr + "-" + CheckProcessService.CurrentCardCheckProcess.ChipNumber + "_.pdf";
            string preFinalPath = settings.DefaultSettings.DefaultProjectOutputPath + "\\" + CheckProcessService.CurrentCardCheckProcess.JobNr + "-" + CheckProcessService.CurrentCardCheckProcess.ChipNumber + ".pdf";

            using ReportReaderWriterService reportReader = new ReportReaderWriterService();

            scanChipTimer.Stop();

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
            scanChipTimer.Stop();
            var window = (Application.Current as App)?.Window as MainWindow ?? new MainWindow();
            var navigation = window.Navigation;
            var step1Page = navigation.GetNavigationViewItems(typeof(Step1Page)).First();
            navigation.SetCurrentNavigationViewItem(step1Page);
        }
        catch (Exception ex)
        {
            LogWriter.CreateLogEntry(ex);
        }
    }
}

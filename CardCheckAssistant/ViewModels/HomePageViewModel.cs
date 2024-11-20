using CardCheckAssistant.Models;
using CardCheckAssistant.Services;
using CardCheckAssistant.Views;

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using CardCheckAssistant.Contracts.Services;
using CardCheckAssistant.Helpers;
using Windows.ApplicationModel.Store;
using Microsoft.UI.Windowing;
using CardCheckAssistant.Contracts.ViewModels;
using Windows.Foundation.Diagnostics;
using Elatec.NET;

namespace CardCheckAssistant.ViewModels;

public partial class HomePageViewModel : ObservableRecipient, INavigationAware
{
    private readonly EventLog eventLog = new("Application", ".", Assembly.GetEntryAssembly().GetName().Name);
    private readonly Microsoft.UI.Xaml.DispatcherTimer scanDBTimer;
    private SQLDBService dbService;
    private readonly bool isCreateEventLogSourceErr;

    private ObservableCollection<CardCheckProcess> cardCheckProcessesFromCache;
#if DEBUG
    private const string DBNAME = "OT_CardCheck_Test";
#else
    private const string DBNAME = "OT_CardCheck";
#endif

    /// <summary>
    /// 
    /// </summary>
    public HomePageViewModel()
    {
        try
        {
            if (!EventLog.SourceExists(Assembly.GetEntryAssembly()?.GetName()?.Name))
            {
                EventLog.CreateEventSource(new EventSourceCreationData(Assembly.GetEntryAssembly().GetName().Name, "Application"));
                isCreateEventLogSourceErr = false;
            }
        }
        catch
        {
            isCreateEventLogSourceErr = true;
        }

        eventLog.Source = Assembly.GetEntryAssembly()?.GetName()?.Name;

        DataGridItemCollection = new ObservableCollection<CardCheckProcess>();
        SetSortAscending = false;

        searchData = new ObservableCollection<CardCheckProcess>();

        scanDBTimer = new Microsoft.UI.Xaml.DispatcherTimer();
        scanDBTimer.Tick += OnTimedEvent;
        scanDBTimer.Interval = new TimeSpan(0, 0, 10);
        scanDBTimer.Stop();

        SelectedFilter = "InProgress";
        SelectedSort = "JobNumber";

        ButtonStartCheckContent = "Button_HomePage_StartCheck".GetLocalized();
        WelcomeScreenText = "TextBlock_HomePage_WelcomeScreenText".GetLocalized();

        NumberOfChecksText = "";

#if DEBUG

#endif

        // Select First "InProgress" Job... if any
        if (DataGridItemCollection != null && _dataGridItemCollection.Any())
        {
            if (DataGridItemCollection.Any(x => x.Status == "InProgress"))
            {
                SelectedCardCheckProcess = DataGridItemCollection.First(x => x.Status == "InProgress");
            }
        }
    }

    private void OpenReportWritable_Click(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void RemoveReportFromDB_Click(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    #region Properties

    /// <summary>
    /// 
    /// </summary>
    public string HomePageVersionString
    {
        get
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            var packageVersion = "";

            try
            {
                packageVersion = "";
            }
            catch { }

            return string.Format("CardCheckAssistant                  Version: {0}", fvi.FileVersion);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private string _buttonStartCheckContent;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private string _welcomeScreenText;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private string _numberOfChecksText;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private bool _homePageIsBusy;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private bool _startCheckCanExecute;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private bool _showAllJobs;

    /// <summary>
    /// 
    /// </summary>
    public CardCheckProcess? SelectedCardCheckProcess
    {
        get => _selectedCardCheckProcess;
        set
        {
            if (value == null)
            {
                StartCheckCanExecute = false;
                ButtonStartCheckContent = "Vorgang auswählen";
            }
            else
            {
                if (CheckProcessService.CurrentCardCheckProcess != null)
                {
                    CheckProcessService.CurrentCardCheckProcess.IsSelected = false;
                }

                CheckProcessService.CurrentCardCheckProcess = value;
                CheckProcessService.CurrentCardCheckProcess.IsSelected = true;

                switch (CheckProcessService.CurrentCardCheckProcess?.Status)
                {
                    case "InProgress":
                        StartCheckCanExecute = true;
                        ButtonStartCheckContent = "Prüfvorgang starten";
                        break;
                    case "CheckFinished":
                        StartCheckCanExecute = true;
                        ButtonStartCheckContent = "Berichtschreibschutz entfernen";
                        break;
                    default:
                        StartCheckCanExecute = false;
                        ButtonStartCheckContent = "Vorgang auswählen";
                        break;
                }
            }
            SetProperty(ref _selectedCardCheckProcess, value);
        }
    }
    private CardCheckProcess? _selectedCardCheckProcess;
    #endregion

    #region ObservableObjects

    [ObservableProperty]
    private ObservableCollection<CardCheckProcess> _dataGridItemCollection;

    #region Filter
    public ObservableCollection<CardCheckProcess> SortData(ObservableCollection<CardCheckProcess> sortData, string sortBy, bool ascending)
    {
        switch (sortBy)
        {
            case "Status":
                if (ascending)
                {
                    return new ObservableCollection<CardCheckProcess>(from item in sortData
                                                                      orderby item.Status ascending
                                                                      select item);
                }
                else
                {
                    return new ObservableCollection<CardCheckProcess>(from item in sortData
                                                                      orderby item.Status descending
                                                                      select item);
                }
            case "Editor":
                if (ascending)
                {
                    return new ObservableCollection<CardCheckProcess>(from item in sortData
                                                                      orderby item.EditorName ascending
                                                                      select item);
                }
                else
                {
                    return new ObservableCollection<CardCheckProcess>(from item in sortData
                                                                      orderby item.EditorName descending
                                                                      select item);
                }
            case "Created":
                if (ascending)
                {
                    return new ObservableCollection<CardCheckProcess>(from item in sortData
                                                                      orderby item.DateCreated ascending
                                                                      select item);
                }
                else
                {
                    return new ObservableCollection<CardCheckProcess>(from item in sortData
                                                                      orderby item.DateCreated descending
                                                                      select item);
                }
            case "JobNumber":
                if (ascending)
                {
                    return new ObservableCollection<CardCheckProcess>(from item in sortData
                                                                      orderby item.ID ascending
                                                                      select item);
                }
                else
                {
                    return new ObservableCollection<CardCheckProcess>(from item in sortData
                                                                      orderby item.ID descending
                                                                      select item);
                }

            default:
                break;
        }
        return new ObservableCollection<CardCheckProcess>();
    }

    public class GroupInfoCollection<T> : ObservableCollection<T>
    {
        public object Key
        {
            get; set;
        }

        public new IEnumerator<T> GetEnumerator()
        {
            return (IEnumerator<T>)base.GetEnumerator();
        }
    }

    public ObservableCollection<CardCheckProcess> FilterData(ObservableCollection<CardCheckProcess> filterData, string filterBy)
    {
        _dataGridItemCollection = cardCheckProcessesFromCache;

        switch (filterBy)
        {
            default:
            case "All":
                return new ObservableCollection<CardCheckProcess>(_dataGridItemCollection ?? new ObservableCollection<CardCheckProcess>());

            case "CheckFinished":
                return new ObservableCollection<CardCheckProcess>(from item in _dataGridItemCollection ?? new ObservableCollection<CardCheckProcess>()
                                                                  where item.Status == "CheckFinished"
                                                                  select item);
            case "InProgress":
                return new ObservableCollection<CardCheckProcess>(from item in _dataGridItemCollection ?? new ObservableCollection<CardCheckProcess>()
                                                                  where item.Status == "InProgress"
                                                                  select item);
        }
    }

    public ObservableCollection<CardCheckProcess> SearchData(string queryText)
    {
        if (!string.IsNullOrEmpty(queryText))
        {
            searchData = new ObservableCollection<CardCheckProcess>(from item in cardCheckProcessesFromCache
                                                                    where (
                                                                    item.JobNr.Contains(queryText, StringComparison.InvariantCultureIgnoreCase) ||
                                                                    item.CName.Contains(queryText, StringComparison.InvariantCultureIgnoreCase) ||
                                                                    item.DealerName.Contains(queryText, StringComparison.InvariantCultureIgnoreCase) ||
                                                                    item.SalesName.Contains(queryText, StringComparison.InvariantCultureIgnoreCase) ||
                                                                    item.EditorName.Contains(queryText, StringComparison.InvariantCultureIgnoreCase) ||
                                                                    item.DateCreated.Contains(queryText, StringComparison.InvariantCultureIgnoreCase))
                                                                    select item);
            return searchData;
        }

        else
        {
            return cardCheckProcessesFromCache;
        }
    }
    private ObservableCollection<CardCheckProcess> searchData;

    [ObservableProperty]
    private bool _openSelectedReportCommandIsRunning;

    public bool SetSortAscending
    {
        get => _setSortAscending;
        set
        {
            SetProperty(ref _setSortAscending, value);
            DataGridItemCollection = new ObservableCollection<CardCheckProcess>(SortData(_dataGridItemCollection, SelectedSort, value));
        }
    }
    private bool _setSortAscending;

    public string SelectedSort
    {
        get => _selectedSort;
        set
        {
            SetProperty(ref _selectedSort, value);
            DataGridItemCollection = new ObservableCollection<CardCheckProcess>(SortData(_dataGridItemCollection, value, SetSortAscending));
        }

    }
    private string _selectedSort;

    public string SelectedFilter
    {
        get => _selectedFilter;
        set
        {
            SetProperty(ref _selectedFilter, value);
            DataGridItemCollection = new ObservableCollection<CardCheckProcess>(SortData(FilterData(_dataGridItemCollection, value), SelectedSort, SetSortAscending));
        }

    }
    private string _selectedFilter;
    #endregion

    #endregion

    #region Commands
    public IAsyncRelayCommand OpenReportWritableCommand => new AsyncRelayCommand(OpenReportWritableCommand_Executed);

    public IAsyncRelayCommand OpenSelectedReportCommand => new AsyncRelayCommand(OpenSelectedReportCommand_Executed);

    public IAsyncRelayCommand NavigateToAboutCommand => new AsyncRelayCommand(NavigateToAboutCommand_Executed);

    public ICommand PostPageLoadedCommand => new AsyncRelayCommand(PostPageLoadedCommand_Executed);

    public ICommand NavigateCommand =>
        new RelayCommand(() =>
        {
            if (SelectedCardCheckProcess?.Status == "Created")
            {

            }
            else if (SelectedCardCheckProcess?.Status == "InProgress")
            {
                BeginCardCheck_Executed();
            }
        });

    private async Task OpenReportWritableCommand_Executed()
    {
        try
        {
            if (CheckProcessService.CurrentCardCheckProcess.Status == "CheckFinished" && CheckProcessService.CurrentCardCheckProcess.IsSelected == true)
            {
                using var settings = new SettingsReaderWriter();

                settings.ReadSettings();

                var p = new Process();

                await SQLDBService.Instance.GetCardCheckReportFromMSSQLAsync(CheckProcessService.CurrentCardCheckProcess.ID);

                var info = new ProcessStartInfo()
                {
                    FileName = settings.DefaultSettings.DefaultProjectOutputPath + "\\" + "downloadedReport.pdf",
                    Verb = "",
                    UseShellExecute = true
                };

                p.StartInfo = info;
                p.Start();

                await p.WaitForExitAsync();
            }
        }

        catch (Exception ex)
        {
            eventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
        }
    }

    private async Task OpenSelectedReportCommand_Executed()
    {
        try
        {
            if (CheckProcessService.CurrentCardCheckProcess.Status == "CheckFinished" && CheckProcessService.CurrentCardCheckProcess.IsSelected == true)
            {
                using var settings = new SettingsReaderWriter();

                settings.ReadSettings();

                var p = new Process();

                await SQLDBService.Instance.GetCardCheckReportFromMSSQLAsync(CheckProcessService.CurrentCardCheckProcess.ID);

                var info = new ProcessStartInfo()
                {
                    FileName = settings.DefaultSettings.DefaultProjectOutputPath + "\\" + "downloadedReport.pdf",
                    Verb = "",
                    UseShellExecute = true
                };

                p.StartInfo = info;
                p.Start();

                await p.WaitForExitAsync();
            }
        }

        catch (Exception ex)
        {
            eventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
        }
    }

    private async Task NavigateToAboutCommand_Executed()
    {
        await Windows.System.Launcher.LaunchUriAsync(new Uri("http://google.com"));
    }

    private async Task PostPageLoadedCommand_Executed()
    {
        try
        {
            if(isCreateEventLogSourceErr)
            {
                await App.MainRoot.MessageDialogAsync(
                    "Fehler.",
                    "Es konnte keine Lagdatei erzeugt werden.\n" +
                    "Bitte das Programm einmal mit Adminrechten starten...");
            }


            await CheckForUpdates();

            using var settings = new SettingsReaderWriter();
            dbService = new SQLDBService(
                settings.DefaultSettings.SelectedDBServerName ?? "",
                settings.DefaultSettings.SelectedDBName ?? "",
                settings.DefaultSettings.SelectedDBTableName ?? "",
                settings.DefaultSettings.SelectedDBUsername ?? "",
                settings.DefaultSettings.SelectedDBUserPwd ?? "");

            App.MainRoot.MessageDialogAsync(
                                "Datenbankverbindung",
                                "Aufträge werden geladen...", "Abbrechen", "connectWaitMsgDlg");

            HomePageIsBusy = true;
            // ensure attention of user
            await Task.Delay(2000);

            try
            {
                // Connect to DB Async
                cardCheckProcessesFromCache = SortData(await ReadCardChecks() ?? new ObservableCollection<CardCheckProcess>(), SelectedSort, SetSortAscending);

                if (cardCheckProcessesFromCache != null)
                {
                    DataGridItemCollection = FilterData(cardCheckProcessesFromCache, SelectedFilter);
                }
                else
                {
                    return;
                }

                ModalView.Dialogs.Where(x => x.Name == "connectWaitMsgDlg").Single().Hide();
            }

            // I expect the Delay to be not so sufficient on some machines
            catch (Exception e)
            {
                eventLog.WriteEntry(e.Message, EventLogEntryType.Error);
            }

            if (DataGridItemCollection == null)
            {
                await DBConnectFailed_Executed();
                WelcomeScreenText = "Verbindungsfehler";
                NumberOfChecksText = "Einstellungen überprüfen";
            }
            else if (DataGridItemCollection.Count == 0)
            {
                await NoJobFoundInDB_Executed();
                WelcomeScreenText = "keine Aufträge";
                NumberOfChecksText = "";
            }
            else
            {
                WelcomeScreenText = "Aufträge gefunden";
                NumberOfChecksText = string.Format("Zahl der neuen Aufträge: {0}", DataGridItemCollection.Where(x => x.Status == "InProgress").Count());
            }

            scanDBTimer.Start();

            HomePageIsBusy = false;
        }
        catch (Exception e)
        {
            eventLog.WriteEntry(e.Message, EventLogEntryType.Error);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private static async Task NoJobFoundInDB_Executed()
    {
        await App.MainRoot.MessageDialogAsync(
        "Keine Aufträge",
        "Die Verbindung mit der Datenbank war erfolgreich. Es befinden sich jedoch keine Aufträge darin. Wenn ein neuer Umschlag mit Karten eingetroffen ist, öffne bitte zuerst den Omnitracker und folge dort den Anweisungen.\n" +
        "\n" +
        "Kehre dann hierher zurück. Der CardCheckAssistant kann geöffnet bleiben und wird automatisch aktualisiert. Falls nicht: Sebastian Hotze hauen.\n" +
        "\n" +
        "Happy CardChecking ;-)");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private async Task DBConnectFailed_Executed()
    {
        await App.MainRoot.MessageDialogAsync(
        "Fehler in der Verbindung",
        "Es konnte keine Verbindung mit der Datenabnk hergestellt werden.\n" +
        "Bitte die Einstellungen überprüfen.");

        scanDBTimer.Stop();
    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    public void BeginCardCheck_Executed()
    {
        if (SelectedCardCheckProcess != null)
        {
            scanDBTimer.Stop();

            SelectedCardCheckProcess.Status = "InProgress";

            (App.MainRoot.XamlRoot.Content as ShellPage)?.ViewModel.NavigationService.NavigateTo(typeof(Step1PageViewModel).FullName ?? "");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void ContinueCardCheck_Executed()
    {
        scanDBTimer.Stop();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private async Task<ObservableCollection<CardCheckProcess>?> ReadCardChecks()
    {
        try
        {
            using var settings = new SettingsReaderWriter();
            // Connect to DB Async

            if (settings.DefaultSettings.CardCheckUseMSSQL ?? false)
            {
                return await SQLDBService.Instance.GetCardChecksFromMSSQLAsync();
            }
            else
            {
                return await SQLDBService.Instance.GetCardChecksFromSQLLiteAsync();
            }
        }
        catch (Exception ex)
        {
            eventLog.WriteEntry(ex.Message, EventLogEntryType.Error);

            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OnTimedEvent(object? _, object e)
    {
        try
        {
            var cardCheckProcessesFromDB = await ReadCardChecks() ?? new ObservableCollection<CardCheckProcess>();

            var selectedID = SelectedCardCheckProcess?.ID;

            var newJobs = false;

            if (DataGridItemCollection == null)
            {
                DataGridItemCollection = new ObservableCollection<CardCheckProcess>();
            }
            else
            {
                foreach (var itemFromDB in cardCheckProcessesFromDB)
                {
                    // There are new ID's, got from DB
                    if (!cardCheckProcessesFromCache.Any(x => x.ID == itemFromDB.ID))
                    {
                        newJobs = true;
                        cardCheckProcessesFromCache = new ObservableCollection<CardCheckProcess> (cardCheckProcessesFromDB);
                    }
                }

                if (newJobs)
                {
                    DataGridItemCollection = cardCheckProcessesFromDB;

                    App.GetService<IAppNotificationService>().Show(string.Format("AppNotificationSamplePayload".GetLocalized(), AppContext.BaseDirectory));

                    newJobs = false;
                }
            }

            try
            {
                SelectedCardCheckProcess = DataGridItemCollection.Where(y => y.ID == (selectedID ?? "")).Single();
            }
            catch
            {
                SelectedCardCheckProcess = null;
            }

            if (cardCheckProcessesFromCache != null)
            {
                if (cardCheckProcessesFromDB == null)
                {
                    await DBConnectFailed_Executed();
                    WelcomeScreenText = "Verbindungsfehler";
                    NumberOfChecksText = "Einstellungen überprüfen";
                }
                else if (cardCheckProcessesFromDB.Count == 0)
                {
                    await NoJobFoundInDB_Executed();
                    WelcomeScreenText = "keine Aufträge";
                    NumberOfChecksText = "";
                }
                else
                {
                    WelcomeScreenText = "Aufträge gefunden";
                    NumberOfChecksText = string.Format("Zahl der neuen Aufträge: {0}", cardCheckProcessesFromDB.Where(x => x.Status == "InProgress").Count());
                }
            }
        }

        //Detection Errors will not throw() because of multiple reasons for this to happen...
        catch
        {

        }
    }

    private async Task CheckForUpdates()
    {
        try
        {
            App.MainRoot.MessageDialogAsync(
                "Prüfe auf Updates",
                "Bitte warten...", "Abbrechen", "checkUpdatesWaitDlg");

            await Task.Delay(1000);

            var package = new PackageManager();
            var currentPackage = package.FindPackageForUser(string.Empty, Package.Current.Id.FullName);
            var result = await currentPackage.CheckUpdateAvailabilityAsync();

            switch (result.Availability)
            {
                case PackageUpdateAvailability.Available:

                    ModalView.Dialogs.Where(x => x.Name == "checkUpdatesWaitDlg").Single().Hide();

                    await App.MainRoot.MessageDialogAsync(
                        "Update wird installiert.",
                        "Es ist eine neue Version von CardCheckassistant verfügbar.\nSie wird nun heruntergalden und installiert...");

                    await InstallUpdate();
                    break;
                case PackageUpdateAvailability.Required:
                case PackageUpdateAvailability.NoUpdates:
                case PackageUpdateAvailability.Unknown:
                default:

                    ModalView.Dialogs.Where(x => x.Name == "checkUpdatesWaitDlg").Single().Hide();

                    break;
            }
        }
        catch (Exception e)
        {

            ModalView.Dialogs.Where(x => x.Name == "checkUpdatesWaitDlg").Single().Hide();

            eventLog.WriteEntry(e.Message, EventLogEntryType.Error);
        }

    }

    private static async Task InstallUpdate()
    {
        try
        {
            var pm = new PackageManager();
            var currentPackage = pm.FindPackageForUser(string.Empty, Package.Current.Id.FullName);
            var deploymentTask = await pm.UpdatePackageAsync(new Uri("https://github.com/c3rebro/CardCheckAssistant/releases/latest/download/CardCheckAssistant_x64.appinstaller"), null, DeploymentOptions.None);

            if (deploymentTask.ErrorText != null)
            {
                await App.MainRoot.MessageDialogAsync(
                    "Updatefehler",
                    string.Format("Bitte melde den folgenden Fehler an mich:\n{0}\n{1}", deploymentTask.ErrorText, deploymentTask.ExtendedErrorCode));
            }
            else
            {
                await App.MainRoot.MessageDialogAsync(
                    "Update Erfolgreich", "Bitte beende Deine Arbeit und starte die Anwendung neu.");
            }
        }
        catch
        {
            await App.MainRoot.MessageDialogAsync(
                "Fehler:",
                "Die Anwendung konnte nicht automatisch neu gestartet werden.\nBitte starte sie manuell neu.");
        }

    }

    /// <summary>
    /// INavigation Aware Event. Close Connection If Open
    /// </summary>
    /// <param name="parameter"></param>
    public async void OnNavigatedTo(object parameter)
    {
        await TWN4ReaderDevice.Instance[0].DisconnectAsync();
    }

    /// <summary>
    /// INavigation Aware Event. Close Connection If Open
    /// </summary>
    public async void OnNavigatedFrom()
    {
        // Run code when the app navigates away from this page
        scanDBTimer.Tick -= OnTimedEvent;
        await TWN4ReaderDevice.Instance[0].DisconnectAsync();
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

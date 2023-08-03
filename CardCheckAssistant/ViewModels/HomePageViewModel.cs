using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;
using CardCheckAssistant.Models;
using CardCheckAssistant.Services;
using System.Collections.Generic;
using CardCheckAssistant.Views;
using CardCheckAssistant.AppNotification;
using System.Security.Cryptography.X509Certificates;
using Log4CSharp;
using System.Reflection;
using System.Timers;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;

namespace CardCheckAssistant.ViewModels;

public class HomePageViewModel : ObservableObject, IDisposable
{
    private readonly DispatcherTimer scanDBTimer;
    private SQLDBService dbService;

    private FilterOptions currentFilter;

    private ObservableCollection<CardCheckProcess> cardCheckProcesses;
#if DEBUG
    private const string DBNAME = "OT_CardCheck_Test";
#else
    private const string DBNAME = "OT_CardCheck";
#endif

    public HomePageViewModel()
    {

        OpenSelectedReportCommand = new AsyncRelayCommand(OpenSelectedReportCommand_Executed);

        if (this != null)
        {

        }
        scanDBTimer = new DispatcherTimer();
        scanDBTimer.Tick += OnTimedEvent;
        scanDBTimer.Interval = new TimeSpan(0,0,10);
        scanDBTimer.Stop();

        currentFilter = FilterOptions.All;

        ButtonStartCheckContent = ResourceLoaderService.GetResource("buttonContentStartCheck");

        WelcomeScreenText = "Datenbankverbindung...";
        NumberOfChecksText = "";
#if DEBUG

#endif

        // Select First "InProgress" Job if any
        if (DataGridItemCollection != null && DataGridItemCollection.Any())
        {
            if (DataGridItemCollection.Any(x => x.Status == OrderStatus.InProgress))
            {
                SelectedCardCheckProcess = DataGridItemCollection.First(x => x.Status == OrderStatus.InProgress);
            }

        }
        else
        {

        }


        var window = (Application.Current as App)?.Window as MainWindow;
        var navigation = window.Navigation;

        // At AppLaunch: Disable all "In between" Steps.
        foreach (NavigationViewItem nVI in navigation.GetNavigationViewItems().Where(x => x.Content.ToString() != "Start").Where(x => x.Content.ToString().Contains("Schritt")))
        {
            nVI.IsEnabled = false;
        }
    }

    #region Properties
    public string ButtonStartCheckContent
    {
        get => _buttonStartCheckContent;
        set => SetProperty(ref _buttonStartCheckContent, value);
    }
    private string _buttonStartCheckContent;

    public string WelcomeScreenText
    {
        get => _welcomeScreenText;
        set => SetProperty(ref _welcomeScreenText, value);
    }
    private string _welcomeScreenText;

    public string NumberOfChecksText
    {
        get => _umberOfChecksText;
        set => SetProperty(ref _umberOfChecksText, value);
    }
    private string _umberOfChecksText;

    public bool HomePageIsBusy
    {
        get => _homePageIsBusy;
        set => SetProperty(ref _homePageIsBusy, value);
    }
    private bool _homePageIsBusy;

    public bool StartCheckCanExecute
    {
        get => _startCheckCanExecute;
        set => SetProperty(ref _startCheckCanExecute, value);
    }
    private bool _startCheckCanExecute;

    public bool ShowAllJobs
    {
        get => _showAllJobs;
        set 
        {
            SetProperty(ref _showAllJobs, value); 
        } 
    }
    private bool _showAllJobs;

    public CardCheckProcess SelectedCardCheckProcess
    {
        get => _selectedCardCheckProcess;
        set {
            if (value == null)
            {
                StartCheckCanExecute = false;
                ButtonStartCheckContent = "Vorgang auswählen";
            }
            else
            {
                if(CheckProcessService.CurrentCardCheckProcess != null)
                {
                    CheckProcessService.CurrentCardCheckProcess.IsSelected = false;
                }
                 
                CheckProcessService.CurrentCardCheckProcess = value;
                CheckProcessService.CurrentCardCheckProcess.IsSelected = true;

                switch (CheckProcessService.CurrentCardCheckProcess?.Status)
                {
                    case OrderStatus.InProgress:
                        StartCheckCanExecute = true;
                        ButtonStartCheckContent = "Prüfvorgang starten";
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
    private CardCheckProcess _selectedCardCheckProcess;
    #endregion

    #region ObservableObjects

    public ObservableCollection<CardCheckProcess> DataGridItemCollection
    {
        get => _dataGridItemCollection;
        set => SetProperty(ref _dataGridItemCollection, value);
    }
    private ObservableCollection<CardCheckProcess> _dataGridItemCollection;


    #region Filter

    public enum FilterOptions
    {
        All = -1,
        InProgress = 0,
        WaitForCustomer = 1,
        CheckFinisched = 2
    }

    public ObservableCollection<CardCheckProcess> SortData(string sortBy, bool ascending)
    {
        switch (sortBy)
        {
            case "Status":
                if (ascending)
                {
                    return new ObservableCollection<CardCheckProcess>(from item in _dataGridItemCollection
                                                                      orderby item.Status ascending
                                                                      select item);
                }
                else
                {
                    return new ObservableCollection<CardCheckProcess>(from item in _dataGridItemCollection
                                                                      orderby item.Status descending
                                                                      select item);
                }

            case "Editor":
                if (ascending)
                {
                    return new ObservableCollection<CardCheckProcess>(from item in _dataGridItemCollection
                                                                      orderby item.EditorName ascending
                                                                      select item);
                }
                else
                {
                    return new ObservableCollection<CardCheckProcess>(from item in _dataGridItemCollection
                                                                      orderby item.EditorName descending
                                                                      select item);
                }

            case "Date":
                if (ascending)
                {
                    return new ObservableCollection<CardCheckProcess>(from item in _dataGridItemCollection
                                                                      orderby item.Date ascending
                                                                      select item);
                }
                else
                {
                    return new ObservableCollection<CardCheckProcess>(from item in _dataGridItemCollection
                                                                      orderby item.Date descending
                                                                      select item);
                }

            case "JobNr":
                if (ascending)
                {
                    return new ObservableCollection<CardCheckProcess>(from item in _dataGridItemCollection
                                                                      orderby item.JobNr ascending
                                                                      select item);
                }
                else
                {
                    return new ObservableCollection<CardCheckProcess>(from item in _dataGridItemCollection
                                                                      orderby item.JobNr descending
                                                                      select item);
                }
        }

        return _dataGridItemCollection;
    }

    // Grouping implementation using LINQ

    /*
    public CollectionViewSource GroupData(string groupBy = "Range")
    {
        ObservableCollection<GroupInfoCollection<DataGridDataItem>> groups = new ObservableCollection<GroupInfoCollection<DataGridDataItem>>();
        var query = from item in _items
                    orderby item
                    group item by item.Range into g
                    select new { GroupName = g.Key, Items = g };
        if (groupBy == "Parent_Mountain")
        {
            query = from item in _items
                    orderby item
                    group item by item.Parent_mountain into g
                    select new { GroupName = g.Key, Items = g };
        }
        foreach (var g in query)
        {
            GroupInfoCollection<DataGridDataItem> info = new GroupInfoCollection<DataGridDataItem>();
            info.Key = g.GroupName;
            foreach (var item in g.Items)
            {
                info.Add(item);
            }

            groups.Add(info);
        }

        groupedItems = new CollectionViewSource();
        groupedItems.IsSourceGrouped = true;
        groupedItems.Source = groups;

        return groupedItems;
    }
    */

    public class GroupInfoCollection<T> : ObservableCollection<T>
    {
        public object Key { get; set; }

        public new IEnumerator<T> GetEnumerator()
        {
            return (IEnumerator<T>)base.GetEnumerator();
        }
    }

    public ObservableCollection<CardCheckProcess> FilterData(FilterOptions filterBy)
    {
        currentFilter = filterBy;

        _dataGridItemCollection = cardCheckProcesses;

        switch (currentFilter)
        {
            case FilterOptions.All:
                return new ObservableCollection<CardCheckProcess>(_dataGridItemCollection);

            case FilterOptions.WaitForCustomer:
                return new ObservableCollection<CardCheckProcess>(from item in _dataGridItemCollection
                                                                  where item.Status == OrderStatus.WaitForCustomer
                                                                  select item);

            case FilterOptions.CheckFinisched:
                return new ObservableCollection<CardCheckProcess>(from item in _dataGridItemCollection
                                                                  where item.Status == OrderStatus.CheckFinished
                                                                  select item);

            case FilterOptions.InProgress:
                return new ObservableCollection<CardCheckProcess>(from item in _dataGridItemCollection
                                                                  where item.Status == OrderStatus.InProgress
                                                                  select item);
        }

        return _dataGridItemCollection;
    }

    public ObservableCollection<CardCheckProcess> SearchData(string queryText)
    {
        if(!string.IsNullOrEmpty(queryText))
        {
            return new ObservableCollection<CardCheckProcess>(from item in FilterData(currentFilter)
                                                              where (
                                                              item.JobNr.Contains(queryText, StringComparison.InvariantCultureIgnoreCase) ||
                                                              item.CName.Contains(queryText, StringComparison.InvariantCultureIgnoreCase) ||
                                                              item.EditorName.Contains(queryText, StringComparison.InvariantCultureIgnoreCase) ||
                                                              item.Date.Contains(queryText, StringComparison.InvariantCultureIgnoreCase))
                                                              select item);
        }

        else 
        { 
            return cardCheckProcesses; 
        }
    }
    #endregion

    #endregion

    #region Commands

    public IAsyncRelayCommand OpenSelectedReportCommand { get; }

    public ICommand PostPageLoadedCommand => new AsyncRelayCommand(PostPageLoadedCommand_Executed);
    public ICommand InputStringCommand => new AsyncRelayCommand(InputString_Executed);

    public ICommand NavigateCommand => 
        new RelayCommand(() => 
        {
            if (SelectedCardCheckProcess.Status == OrderStatus.Created)
            {
                BeginCardCheck_Executed();
            }
            else if (SelectedCardCheckProcess.Status == OrderStatus.InProgress)
            {
                ContinueCardCheck_Executed();
            }
        });

    private async Task OpenSelectedReportCommand_Executed()
    {
        try
        {
            if (CheckProcessService.CurrentCardCheckProcess.Status == OrderStatus.CheckFinished && CheckProcessService.CurrentCardCheckProcess.IsSelected == true)
            {
                using SettingsReaderWriter settings = new SettingsReaderWriter();

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

        catch(Exception ex)
        {
            LogWriter.CreateLogEntry(ex, Assembly.GetExecutingAssembly().GetName().Name);
        }
    }

    private async Task PostPageLoadedCommand_Executed()
    {
        try
        {
            using SettingsReaderWriter settings = new SettingsReaderWriter();
            dbService = new SQLDBService(
                settings.DefaultSettings.SelectedDBServerName,
                settings.DefaultSettings.SelectedDBName,
                settings.DefaultSettings.SelectedDBUsername,
                settings.DefaultSettings.SelectedDBUserPwd);

            App.MainRoot.MessageDialogAsync(
                                "Datenbankverbindung",
                                "Aufträge werden geladen...", "Abbrechen", "connectWaitMsgDlg");

            HomePageIsBusy= true;
            // ensure attention of user
            await Task.Delay(2000);


            try
            {
                // Connect to DB Async
                cardCheckProcesses = await ReadCardChecks();

                if (cardCheckProcesses != null)
                {
                    /*
                    foreach (var cardCheck in cardCheckProcesses)
                    {
                        if (cardCheck.Status != OrderStatus.InProgress)
                        {
                            cardCheck.IsVisible = "Collapsed";
                        }
                        else
                        {
                            cardCheck.IsVisible = "Collapsed";
                        }
                    }
                    */

                    if(DataGridItemCollection == null)
                    {
                        DataGridItemCollection = new ObservableCollection<CardCheckProcess>(cardCheckProcesses.OrderBy(x => x.Status));
                    } 
                }

                ModalView.Dialogs.Where(x => x.Name == "connectWaitMsgDlg").Single().Hide();
                ModalView.Dialogs.Remove(ModalView.Dialogs.Where(x => x.Name == "connectWaitMsgDlg").Single());
            }

            //I expect the Delay to be not so sufficient on some machines
            catch (Exception e)
            {
                LogWriter.CreateLogEntry(e, Assembly.GetExecutingAssembly().GetName().Name);
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
                NumberOfChecksText = string.Format("Zahl der neuen Aufträge: {0}", DataGridItemCollection.Where(x => x.Status == OrderStatus.InProgress).Count());
            }

            scanDBTimer.Start();

            HomePageIsBusy = false;
        }
        catch (Exception e)
        {
            LogWriter.CreateLogEntry(e, Assembly.GetExecutingAssembly().GetName().Name);
        }
    }

    private async Task NoJobFoundInDB_Executed()
    {
        await App.MainRoot.MessageDialogAsync(
        "Keine Aufträge",
        "Die Verbindung mit der Datenbank war erfolgreich. Es befinden sich jedoch keine Aufträge darin. Wenn ein neuer Umschlag mit Karten eingetroffen ist, öffne bitte zuerst den Omnitracker und folge dort den Anweisungen.\n" +
        "\n" +
        "Kehre dann hierher zurück. Der CardCheckAssistant kann geöffnet bleiben und wird automatisch aktualisiert. Falls nicht: Sebastian Hotze hauen.\n" +
        "\n" +
        "Happy CardChecking ;-)");
    }

    private async Task DBConnectFailed_Executed()
    {
        await App.MainRoot.MessageDialogAsync(
        "Fehler in der Verbindung",
        "Es konnte keine Verbindung mit der Datenabnk hergestellt werden.\n" +
        "Bitte die Einstellungen überprüfen.");
    }
    #endregion

    public async void InputText_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("Opening Text Input Dialog.");
        var inputText = await App.MainRoot.InputTextDialogAsync(
                "What would Faramir say?",
                "“War must be, while we defend our lives against a destroyer who would devour all; but I do not love the bright sword for its sharpness, nor the arrow for its swiftness, nor the warrior for his glory. I love only that which they defend.”\n\nJ.R.R. Tolkien"
            );

            Debug.WriteLine($"Text Input Dialog was closed with {inputText}.");
    }

    private async Task InputString_Executed()
    {
        Debug.WriteLine("Opening String Input Dialog.");
        var inputString = await App.MainRoot.InputStringDialogAsync(
                "How can we help you?",
                "I need ammunition, not a ride.",
                "OK",
                "Forget it"
            );
        Debug.WriteLine($"String Input Dialog was closed with '{inputString}'.");
    }
    
    public void BeginCardCheck_Executed()
    {
        SelectedCardCheckProcess.Status = OrderStatus.InProgress;

        var window = (Application.Current as App)?.Window as MainWindow;
        var navigation = window.Navigation;
        var step1Page = navigation.GetNavigationViewItems(typeof(Step1Page)).First();
        navigation.SetCurrentNavigationViewItem(step1Page);
        step1Page.IsEnabled = true;
    }

    public void ContinueCardCheck_Executed()
    {
        scanDBTimer.Stop();

        var window = (Application.Current as App)?.Window as MainWindow;
        var navigation = window.Navigation;
        NavigationViewItem page = navigation.GetNavigationViewItems(typeof(Step1Page)).First();
        switch (SelectedCardCheckProcess.CurrentProcessNumber)
        {
            case 1:
                page = navigation.GetNavigationViewItems(typeof(Step1Page)).First();
                break;
            case 2:
                page = navigation.GetNavigationViewItems(typeof(Step2Page)).First();
                break;
            case 3:
                //page = navigation.GetNavigationViewItems(typeof(Step3Page)).First();
                break;
        }
        
        page.IsEnabled = true;
        navigation.SetCurrentNavigationViewItem(page);
    }

    private async Task<ObservableCollection<CardCheckProcess>> ReadCardChecks()
    {
        using (SettingsReaderWriter settings = new SettingsReaderWriter())
        {
            // Connect to DB Async

            if (settings.DefaultSettings.CardCheckUseMSSQL)
            {
                return await SQLDBService.Instance.GetCardChecksFromMSSQLAsync();
            }
            else
            {
                return await SQLDBService.Instance.GetCardChecksFromSQLLiteAsync();
            }
        }
    }

    private async void OnTimedEvent(object? sender, object e)
    {
        try
        {
            //scanDBTimer.Stop();
            //var t = await SQLDBService.Instance.GetCardCheckReportFromMSSQLAsync("KP-000038-1");
            
            var cardCheckProcessesFromDB = await ReadCardChecks();

            var selectedID = SelectedCardCheckProcess?.ID;

            if (DataGridItemCollection == null)
            {
                DataGridItemCollection = new ObservableCollection<CardCheckProcess>();
            }
            else
            {
                foreach(var item in cardCheckProcessesFromDB)
                {
                    if(!cardCheckProcesses.Where(x => x.ID == item.ID).Any())
                    {
                        cardCheckProcesses = new ObservableCollection<CardCheckProcess>(cardCheckProcessesFromDB.OrderBy(x => x.Status));
                        DataGridItemCollection = FilterData(currentFilter);
                    }
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

            if (cardCheckProcesses != null)
            {
                //Notify on new DB entry / on new ID
                foreach (var cc in cardCheckProcesses.Where(x => x.Status == OrderStatus.InProgress))
                {
                    if (!cardCheckProcessesFromDB.Where(y => y.Status == OrderStatus.InProgress).Select(x => x.ID).Contains(cc.ID))
                    {
                        ToastWithAvatar.Instance.ScenarioName = "Neue Aufträge gefunden...";
                        ToastWithAvatar.Instance.SendToast(string.Format("Es wurde ein neuer Auftrag gefunden:\n" +
                            "ID: {0}\n" +
                            "Job ID: {1}" +
                            "Kunde: {2}", cc.ID, cc.JobNr, cc.CName), "Assistent öffnen", "refreshDB", null);
                    }
                }

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
                    NumberOfChecksText = string.Format("Zahl der neuen Aufträge: {0}", cardCheckProcessesFromDB.Where(x => x.Status == OrderStatus.InProgress).Count());
                }

            }
        }
        catch 
        { 

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

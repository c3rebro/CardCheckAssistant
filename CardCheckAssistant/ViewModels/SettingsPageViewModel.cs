using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Reflection;
using System.Windows.Input;
using System.Linq;

using CardCheckAssistant.Contracts.Services;
using CardCheckAssistant.Services;
using CardCheckAssistant.Helpers;
using CardCheckAssistant.Models;
using CardCheckAssistant.DataAccessLayer;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using Windows.ApplicationModel;
using Windows.Storage.Pickers;
using Org.BouncyCastle.Crypto.Engines;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CardCheckAssistant.Views;
using System.Diagnostics;
using CardCheckAssistant.Contracts.ViewModels;
using Elatec.NET;

namespace CardCheckAssistant.ViewModels;

public partial class SettingsPageViewModel : ObservableRecipient, INavigationAware
{
    private readonly IThemeSelectorService _themeSelectorService;
    private ElementTheme _originalElementTheme;
    private DefaultSettings? _originalSettings;
    private readonly EventLog eventLog = new("Application", ".", Assembly.GetEntryAssembly().GetName().Name);

    [ObservableProperty]
    private ElementTheme _elementTheme;

    [ObservableProperty]
    private string _versionDescription;

    public ICommand SwitchThemeCommand
    {
        get;
    }

    public SettingsPageViewModel(IThemeSelectorService themeSelectorService)
    {
        try
        {
            _themeSelectorService = themeSelectorService;
            _originalElementTheme = _themeSelectorService.Theme;
            _elementTheme = _themeSelectorService.Theme;
            _versionDescription = GetVersionDescription();

            SwitchThemeCommand = new RelayCommand<ElementTheme>(
                async (param) =>
                {
                    if (ElementTheme != param)
                    {
                        ElementTheme = param;
                        await _themeSelectorService.SetThemeAsync(param);
                    }
                });

            LoadFromStoredSettings();
        }
        catch (Exception ex)
        {
            eventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
        }
    }

    #region ObservableObjects

    /// <summary>
    /// 
    /// </summary>
    public bool CardCheckUseSQLLite
    {
        get => _cardCheckUseSQLLite ?? false;
        set
        {
            SetProperty(ref _cardCheckUseSQLLite, value);
        }
    }
    private bool? _cardCheckUseSQLLite;

    /// <summary>
    /// 
    /// </summary>
    public string SelectedDBServerName
    {
        get => _selectedDBServerName ?? string.Empty;
        set
        {
            SetProperty(ref _selectedDBServerName, value);
        }
    }
    private string? _selectedDBServerName;

    /// <summary>
    /// 
    /// </summary>
    public string SelectedDBServerPort
    {
        get => _selectedDBServerPort ?? string.Empty;
        set
        {
            SetProperty(ref _selectedDBServerPort, value);
        }
    }
    private string? _selectedDBServerPort;

    /// <summary>
    /// 
    /// </summary>
    public string SelectedDBTableName
    {
        get => _selectedDBTableName ?? string.Empty;
        set
        {
            SetProperty(ref _selectedDBTableName, value);
        }
    }
    private string? _selectedDBTableName;

    /// <summary>
    /// 
    /// </summary>
    public string SelectedDBName
    {
        get => _selectedDBName ?? string.Empty;
        set
        {
            SetProperty(ref _selectedDBName, value);
        }
    }
    private string? _selectedDBName;

    /// <summary>
    /// 
    /// </summary>
    public string SelectedDBUsername
    {
        get => _selectedDBUsername ?? string.Empty;
        set
        {
            SetProperty(ref _selectedDBUsername, value);
        }
    }
    private string? _selectedDBUsername;

    /// <summary>
    /// 
    /// </summary>
    public string SelectedDBUserPwd
    {
        get => _selectedDBUserPwd ?? string.Empty;
        set
        {
            SetProperty(ref _selectedDBUserPwd, value);
        }
    }
    private string? _selectedDBUserPwd;

    /// <summary>
    /// 
    /// </summary>
    public string SelectedCustomProjectFolder
    {
        get => _selectedCustomProjectFolder ?? string.Empty;
        set
        {
            SetProperty(ref _selectedCustomProjectFolder, value);
        }
    }
    private string? _selectedCustomProjectFolder;

    /// <summary>
    /// 
    /// </summary>
    public string SelectedDefaultProject
    {
        get => _selectedDefaultProject ?? string.Empty;
        set
        {
            SetProperty(ref _selectedDefaultProject, value);
        }
    }
    private string? _selectedDefaultProject;

    /// <summary>
    /// 
    /// </summary>
    public string SelectedProjectFolder
    {
        get => _selectedProjectFolder ?? string.Empty;
        set
        {
            SetProperty(ref _selectedProjectFolder, value);
        }
    }
    private string? _selectedProjectFolder;

    /// <summary>
    /// 
    /// </summary>
    public string SelectedRFIDGearPath
    {
        get => _selectedRFIDGearPath ?? string.Empty;
        set
        {
            SetProperty(ref _selectedRFIDGearPath, value);
        }
    }
    private string? _selectedRFIDGearPath;

    /// <summary>
    /// 
    /// </summary>
    public bool RemoveTemporaryReportsIsEnabled
    {
        get => _removeTemporaryReportsIsEnabled ?? false;
        set
        {
            SetProperty(ref _removeTemporaryReportsIsEnabled, value);
        }
    }
    private bool? _removeTemporaryReportsIsEnabled;

    /// <summary>
    /// 
    /// </summary>
    public bool CreateSubdirectoryIsEnabled
    {
        get => _createSubdirectoryIsEnabled;
        set
        {
            SetProperty(ref _createSubdirectoryIsEnabled, value);
        }
    }
    private bool _createSubdirectoryIsEnabled;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private bool _isTextBoxCardCheckTextTemplateEnabled;

    /// <summary>
    /// 
    /// </summary>
    public bool RFiDGearIsAutoRunEnabled
    {
        get => _rFiDGearIsAutoRunEnabled ?? false;
        set
        {
            SetProperty(ref _rFiDGearIsAutoRunEnabled, value);
        }
    }
    private bool? _rFiDGearIsAutoRunEnabled;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _themeSource;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private string _textTemplateName;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<CardCheckTextTemplate> _textTemplates;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private CardCheckTextTemplate _selectedTextTemplate;

    /// <summary>
    /// 
    /// </summary>
    public int ReaderVolume
    {
        get => _readerVolume;
        set
        {
            SetProperty(ref _readerVolume, value);
        }
    }
    private int _readerVolume;

    #endregion

    #region Commands
    public ICommand DBConnectionTest => new AsyncRelayCommand(DBConnectionTest_Executed);

    public ICommand CreateNewTextTemplate => new AsyncRelayCommand(CreateNewTextTemplate_Executed);

    public ICommand DeleteTextTemplate => new AsyncRelayCommand(DeleteTextTemplate_Executed);

    public IAsyncRelayCommand NavigateBackCommand => new AsyncRelayCommand(NavigateBackCommand_Executed);

    public IAsyncRelayCommand CancelCommand => new AsyncRelayCommand(CancelCommand_Executed);

    public IAsyncRelayCommand ExportSettingsCommand => new AsyncRelayCommand(ExportSettings_Executed);

    public IAsyncRelayCommand ImportSettingsCommand => new AsyncRelayCommand(ImportSettings_Executed);

    public ICommand SelectRFIDGearExeCommand => new AsyncRelayCommand(SelectRFIDGearExe_Executed);

    public ICommand SelectProjectFolderCommand => new AsyncRelayCommand(SelectProjectFolder_Executed);

    public ICommand SelectRFIDGearCustomProjectCommand => new AsyncRelayCommand(SelectRFIDGearCustomProjectCommand_Executed);

    public ICommand SelectRFIDGearDefaultProjectCommand => new AsyncRelayCommand(SelectRFIDGearDefaultProjectCommand_Executed);

    public ICommand ReaderConnectionTestCommand => new AsyncRelayCommand(ReaderConnectionTestCommand_Executed);
    #endregion

    private void LoadFromStoredSettings()
    {
        using var settings = new SettingsReaderWriter();
        using var enc = new RijndaelEnc();

        _originalSettings = CloneSettings(settings.DefaultSettings);

        ApplySettingsToView(settings.DefaultSettings, enc);
    }

    private void ApplySettingsToView(DefaultSettings settings, RijndaelEnc enc)
    {
        IsTextBoxCardCheckTextTemplateEnabled = false;
        SelectedProjectFolder = settings.DefaultProjectOutputPath ?? string.Empty;
        SelectedCustomProjectFolder = settings.LastUsedCustomProjectPath ?? string.Empty;
        SelectedDefaultProject = settings.LastUsedDefaultProject ?? string.Empty;
        SelectedRFIDGearPath = settings.DefaultRFIDGearExePath ?? string.Empty;
        RFiDGearIsAutoRunEnabled = settings.AutoRunProjectOnStart == true;
        SelectedDBName = settings.SelectedDBName ?? string.Empty;
        SelectedDBTableName = settings.SelectedDBTableName ?? string.Empty;
        SelectedDBServerName = settings.SelectedDBServerName ?? string.Empty;
        SelectedDBServerPort = settings.SelectedDBServerPort ?? string.Empty;
        SelectedDBUsername = settings.SelectedDBUsername ?? string.Empty;
        CardCheckUseSQLLite = settings.CardCheckUseMSSQL == true;
        CreateSubdirectoryIsEnabled = settings.CreateSubdirectoryIsEnabled == true;
        RemoveTemporaryReportsIsEnabled = settings.RemoveTemporaryReportsIsEnabled == true;
        ReaderVolume = settings.ReaderVolume ?? 0;

        SelectedDBUserPwd = string.IsNullOrWhiteSpace(settings?.SelectedDBUserPwd)
            ? string.Empty
            : enc.Decrypt(settings!.SelectedDBUserPwd!);

        TextTemplates = CloneTemplates(settings.CardCheckTextTemplates);
        SelectedTextTemplate = TextTemplates.FirstOrDefault();
        IsTextBoxCardCheckTextTemplateEnabled = TextTemplates.Any() && !(TextTemplates.Count == 1 && TextTemplates.First().TemplateTextName == "N/A");
    }

    private void ApplyCurrentStateToSettings(DefaultSettings settings, RijndaelEnc enc)
    {
        settings.CardCheckUseMSSQL = CardCheckUseSQLLite;
        settings.SelectedDBServerName = SelectedDBServerName;
        settings.SelectedDBName = SelectedDBName;
        settings.SelectedDBTableName = SelectedDBTableName;
        settings.SelectedDBUsername = SelectedDBUsername;
        settings.SelectedDBUserPwd = enc.Encrypt(SelectedDBUserPwd ?? string.Empty);
        settings.SelectedDBServerPort = SelectedDBServerPort;

        settings.LastUsedCustomProjectPath = SelectedCustomProjectFolder;
        settings.LastUsedDefaultProject = SelectedDefaultProject;
        settings.DefaultRFIDGearExePath = SelectedRFIDGearPath;
        settings.DefaultProjectOutputPath = SelectedProjectFolder;
        settings.CreateSubdirectoryIsEnabled = CreateSubdirectoryIsEnabled;
        settings.RemoveTemporaryReportsIsEnabled = RemoveTemporaryReportsIsEnabled;
        settings.AutoRunProjectOnStart = RFiDGearIsAutoRunEnabled;
        settings.ReaderVolume = ReaderVolume;

        settings.CardCheckTextTemplates = CloneTemplates(TextTemplates);
    }

    private DefaultSettings CloneSettings(DefaultSettings settings)
    {
        return new DefaultSettings
        {
            CardCheckUseMSSQL = settings.CardCheckUseMSSQL,
            SelectedDBServerName = settings.SelectedDBServerName,
            SelectedDBName = settings.SelectedDBName,
            SelectedDBTableName = settings.SelectedDBTableName,
            SelectedDBUsername = settings.SelectedDBUsername,
            SelectedDBUserPwd = settings.SelectedDBUserPwd,
            SelectedDBServerPort = settings.SelectedDBServerPort,
            LastUsedCustomProjectPath = settings.LastUsedCustomProjectPath,
            LastUsedDefaultProject = settings.LastUsedDefaultProject,
            DefaultRFIDGearExePath = settings.DefaultRFIDGearExePath,
            DefaultProjectOutputPath = settings.DefaultProjectOutputPath,
            CreateSubdirectoryIsEnabled = settings.CreateSubdirectoryIsEnabled,
            RemoveTemporaryReportsIsEnabled = settings.RemoveTemporaryReportsIsEnabled,
            AutoRunProjectOnStart = settings.AutoRunProjectOnStart,
            ReaderVolume = settings.ReaderVolume,
            CardCheckTextTemplates = CloneTemplates(settings.CardCheckTextTemplates)
        };
    }

    private ObservableCollection<CardCheckTextTemplate> CloneTemplates(IEnumerable<CardCheckTextTemplate>? templates)
    {
        var items = (templates ?? Enumerable.Empty<CardCheckTextTemplate>())
            .Select(t => new CardCheckTextTemplate(t.TemplateTextName)
            {
                TemplateTextContent = t.TemplateTextContent
            })
            .ToList();

        if (!items.Any())
        {
            items.Add(new CardCheckTextTemplate("N/A"));
        }

        return new ObservableCollection<CardCheckTextTemplate>(items);
    }

    private async Task CreateNewTextTemplate_Executed()
    {
        try
        {
            if (TextTemplates.Any(x => x.TemplateTextName == "N/A") && !string.IsNullOrEmpty(TextTemplateName))
            {
                TextTemplates.Clear();
            }

            if (!string.IsNullOrEmpty(TextTemplateName))
            {
                var current = new CardCheckTextTemplate(TextTemplateName);
                TextTemplates.Add(current);
                TextTemplateName = string.Empty;

                SelectedTextTemplate = current;
                IsTextBoxCardCheckTextTemplateEnabled = true;

                OnPropertyChanged(nameof(TextTemplates));
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

    private async Task DeleteTextTemplate_Executed()
    {
        try
        {
            TextTemplates?.Remove(SelectedTextTemplate);

            if (TextTemplates?.FirstOrDefault() != null)
            {
                SelectedTextTemplate = TextTemplates?.FirstOrDefault();
            }
            else
            {
                IsTextBoxCardCheckTextTemplateEnabled = false;
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

    private async Task DBConnectionTest_Executed()
    {
        // Connect to DB Async with pending values
        if (CardCheckUseSQLLite)
        {
            using var dbService = new SQLDBService(
                SelectedDBServerName,
                SelectedDBName,
                SelectedDBTableName,
                SelectedDBUsername,
                SelectedDBUserPwd);
            await dbService.GetCardChecksFromMSSQLAsync();
        }
        else
        {
            using var dbService = new SQLDBService();

            await dbService.GetCardChecksFromSQLLiteAsync();
        }
    }

    private async Task SelectRFIDGearExe_Executed()
    {
        var window = App.MainWindow as MainWindow;

        var filePicker = new FileOpenPicker();

        // Get the current window's HWND by passing in the Window object
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

        // Associate the HWND with the file picker
        WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);

        filePicker.FileTypeFilter.Add("*");
        var file = await filePicker.PickSingleFileAsync();
        if (file != null && !string.IsNullOrEmpty(file.Path.ToString()))
        {
            SelectedRFIDGearPath = file?.Path.ToString() ?? "";
        }
    }

    private async Task SelectRFIDGearCustomProjectCommand_Executed()
    {
        var window = App.MainWindow as MainWindow;

        var filePicker = new FileOpenPicker();

        // Get the current window's HWND by passing in the Window object
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

        // Associate the HWND with the file picker
        WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);

        filePicker.FileTypeFilter.Add("*");
        var file = await filePicker.PickSingleFileAsync();
        if (file != null && !string.IsNullOrEmpty(file.Path.ToString()))
        {
            SelectedCustomProjectFolder = file?.Path.ToString();
        }

    }

    private async Task SelectRFIDGearDefaultProjectCommand_Executed()
    {
        var window = App.MainWindow as MainWindow;

        var filePicker = new FileOpenPicker();

        // Get the current window's HWND by passing in the Window object
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

        // Associate the HWND with the file picker
        WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);

        filePicker.FileTypeFilter.Add("*");
        var file = await filePicker.PickSingleFileAsync();
        if (file != null && !string.IsNullOrEmpty(file.Path.ToString()))
        {
            SelectedDefaultProject = file?.Path.ToString();
        }

    }

    private async Task SelectProjectFolder_Executed()
    {
        var window = App.MainWindow as MainWindow;

        var folderPicker = new FolderPicker();

        // Get the current window's HWND by passing in the Window object
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

        // Associate the HWND with the file picker
        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

        folderPicker.FileTypeFilter.Add("*");
        var file = await folderPicker.PickSingleFolderAsync();
        if (file != null && !string.IsNullOrEmpty(file.Path.ToString()))
        {
            SelectedProjectFolder = file.Path.ToString();
        }
    }

    private async Task ReaderConnectionTestCommand_Executed()
    {
    
    }

    /// <summary>
    /// INavigation Aware Event. Close Connection If Open
    /// </summary>
    /// <param name="parameter"></param>
    public async void OnNavigatedTo(object parameter)
    {
        // Run code when the app navigates to this page
        if (TWN4ReaderDevice.Instance?.Count > 0 && TWN4ReaderDevice.Instance[0] != null)
        {
            await TWN4ReaderDevice.Instance[0].DisconnectAsync();
        }
    }

    private async Task ExportSettings_Executed()
    {
        try
        {
            var window = App.MainWindow as MainWindow;

            var savePicker = new FileSavePicker();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

            savePicker.FileTypeChoices.Add("Einstellungsdatei", new List<string> { ".xml" });
            savePicker.SuggestedFileName = "settings";

            var file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                using var settings = new SettingsReaderWriter();
                using var enc = new RijndaelEnc();

                ApplyCurrentStateToSettings(settings.DefaultSettings, enc);

                var saveSucceeded = settings.SaveSettings(file.Path);
                if (!saveSucceeded)
                {
                    await App.MainRoot.MessageDialogAsync("Fehler", "Die Einstellungen konnten nicht exportiert werden.");
                }
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

    private async Task ImportSettings_Executed()
    {
        try
        {
            var window = App.MainWindow as MainWindow;

            var filePicker = new FileOpenPicker();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);

            filePicker.FileTypeFilter.Add(".xml");

            var file = await filePicker.PickSingleFileAsync();
            if (file != null)
            {
                using var settings = new SettingsReaderWriter();
                using var enc = new RijndaelEnc();

                var readFailed = settings.ReadSettings(file.Path);
                if (!readFailed)
                {
                    ApplySettingsToView(settings.DefaultSettings, enc);
                }
                else
                {
                    await App.MainRoot.MessageDialogAsync("Fehler", "Die ausgewählte Einstellungsdatei konnte nicht geladen werden.");
                }
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
    /// INavigation Aware Event. Close Connection If Open
    /// </summary>
    public async void OnNavigatedFrom()
    {
        // Run code when the app navigates away from this page
        if (TWN4ReaderDevice.Instance?.Count > 0 && TWN4ReaderDevice.Instance[0] != null)
        {
            await TWN4ReaderDevice.Instance[0].DisconnectAsync();
        }
    }

    private async Task NavigateBackCommand_Executed()
    {
        try
        {
            using var settings = new SettingsReaderWriter();
            using var enc = new RijndaelEnc();

            if (TWN4ReaderDevice.Instance?.Count > 0 && TWN4ReaderDevice.Instance[0] != null)
            {
                await TWN4ReaderDevice.Instance[0].DisconnectAsync();
            }

            ApplyCurrentStateToSettings(settings.DefaultSettings, enc);

            settings.SaveSettings();

            _originalSettings = CloneSettings(settings.DefaultSettings);
            _originalElementTheme = ElementTheme;
        }
        catch (Exception ex)
        {
            eventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
        }

        NavigateHome();
    }

    private async Task CancelCommand_Executed()
    {
        try
        {
            if (_originalSettings != null)
            {
                using var enc = new RijndaelEnc();
                ApplySettingsToView(_originalSettings, enc);
            }

            if (_originalElementTheme != ElementTheme)
            {
                await _themeSelectorService.SetThemeAsync(_originalElementTheme);
                ElementTheme = _originalElementTheme;
            }
        }
        catch (Exception ex)
        {
            eventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
        }

        NavigateHome();
    }

    private void NavigateHome()
    {
        (App.MainRoot.XamlRoot.Content as ShellPage)?.ViewModel.NavigationService.NavigateTo(typeof(HomePageViewModel).FullName ?? "");
    }

    private class RijndaelEnc : IDisposable
    {
        private bool _disposed;
        private readonly EventLog eventLog = new("Application", ".", Assembly.GetEntryAssembly().GetName().Name);

        private readonly byte[] IV = { 22, 85, 121, 60, 1, 77, 14, 69, 210, 10, 41, 22, 91, 6, 32, 4 };
        private readonly byte[] KEY = { 12, 122, 12, 72, 9, 1, 53, 72, 11, 94, 66, 84, 26, 110, 210, 44, 109, 10, 9, 100, 31, 201, 11, 23, 75, 91, 12, 83, 22, 19, 33, 3 };

        public RijndaelEnc()
        {
            
        }

        public string Encrypt(string source)
        {
            try
            {
                return BitConverter.ToString(EncryptStringToBytes(source, KEY, IV));
            }
            catch (Exception ex)
            {
                eventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
                return "";
            }
        }

        public string Decrypt(string source)
        {
            try
            {
                var arr = source.Split('-');
                var array = new byte[arr.Length];
                for (var i = 0; i < arr.Length; i++)
                {
                    array[i] = Convert.ToByte(arr[i], 16);
                }

                // Decrypt the bytes to a string.
                return DecryptStringFromBytes(array, KEY, IV);
            }
            catch (Exception ex)
            {
                eventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
                return "";
            }
        }

        private static byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
            {
                throw new ArgumentNullException("plainText");
            }

            if (Key == null || Key.Length <= 0)
            {
                throw new ArgumentNullException("Key");
            }

            if (IV == null || IV.Length <= 0)
            {
                throw new ArgumentNullException("IV");
            }

            byte[] encrypted;
            // Create an Rijndael object
            // with the specified key and IV.
            using (var rijAlg = Rijndael.Create())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                // Create an encryptor to perform the stream transform.
                var encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for encryption.
                using var msEncrypt = new MemoryStream();
                using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);

                using (var swEncrypt = new StreamWriter(csEncrypt))
                {
                    //Write all data to the stream.
                    swEncrypt.Write(plainText);
                }
                encrypted = msEncrypt.ToArray();
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        private static string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
            {
                throw new ArgumentNullException("cipherText");
            }

            if (Key == null || Key.Length <= 0)
            {
                throw new ArgumentNullException("Key");
            }

            if (IV == null || IV.Length <= 0)
            {
                throw new ArgumentNullException("IV");
            }

            // Declare the string used to hold
            // the decrypted text.
            var plaintext = "";

            // Create an Rijndael object
            // with the specified key and IV.
            using (var rijAlg = Rijndael.Create())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                // Create a decryptor to perform the stream transform.
                var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for decryption.
                using var msDecrypt = new MemoryStream(cipherText);
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(csDecrypt);

                // Read the decrypted bytes from the decrypting stream
                // and place them in a string.
                plaintext = srDecrypt.ReadToEnd();
            }

            return plaintext;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    try
                    {
                        // Dispose any managed objects
                        // ...
                    }

                    catch (Exception e)
                    {
                        eventLog.WriteEntry(e.Message, EventLogEntryType.Error);
                    }
                }

                // Now disposed of any unmanaged objects
                // ...

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }
}

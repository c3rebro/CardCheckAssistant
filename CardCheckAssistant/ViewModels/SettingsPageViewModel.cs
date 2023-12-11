using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Windows.Input;
using CardCheckAssistant.Models;
using CardCheckAssistant.Services;
using CardCheckAssistant.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Log4CSharp;
using Microsoft.UI.Xaml;
using Windows.Storage.Pickers;

namespace CardCheckAssistant.ViewModels;

public partial class SettingsPageViewModel : ObservableObject
{
    public SettingsPageViewModel()
    {
        try
        {            
            using var settings = new SettingsReaderWriter();
            using var enc = new RijndaelEnc();

            ThemeSource = new ObservableCollection<string>
            {
                "Light",
                "Dark",
                "Default"
            };

            IsTextBoxCardCheckTextTemplateEnabled = false;
            SelectedTheme = settings.DefaultSettings.DefaultTheme;
            SelectedProjectFolder = settings.DefaultSettings.DefaultProjectOutputPath;
            SelectedCustomProjectFolder = settings.DefaultSettings.LastUsedCustomProjectPath;
            SelectedRFIDGearPath = settings.DefaultSettings.DefaultRFIDGearExePath;
            RFiDGearIsAutoRunEnabled = settings.DefaultSettings.AutoLoadProjectOnStart;
            SelectedDBName = settings.DefaultSettings.SelectedDBName;
            SelectedDBServerName = settings.DefaultSettings.SelectedDBServerName;
            SelectedDBServerPort = settings.DefaultSettings.SelectedDBServerPort;
            SelectedDBUsername = settings.DefaultSettings.SelectedDBUsername;
            CardCheckUseSQLLite = settings.DefaultSettings.CardCheckUseMSSQL;
            CreateSubdirectoryIsEnabled = settings.DefaultSettings.CreateSubdirectoryIsEnabled;
            RemoveTemporaryReportsIsEnabled = settings.DefaultSettings.RemoveTemporaryReportsIsEnabled;

            SelectedDBUserPwd = enc.Decrypt(settings?.DefaultSettings?.SelectedDBUserPwd ?? "NoPWD");

            if (settings.DefaultSettings.CardCheckTextTemplates != null && settings.DefaultSettings.CardCheckTextTemplates.Any())
            {
                TextTemplates = new ObservableCollection<CardCheckTextTemplate>(settings.DefaultSettings.CardCheckTextTemplates);
                SelectedTextTemplate = TextTemplates?.FirstOrDefault();

                IsTextBoxCardCheckTextTemplateEnabled = true;
            }
            else
            {
                TextTemplates = new ObservableCollection<CardCheckTextTemplate>
                {
                    new ("N/A")
                };

                SelectedTextTemplate = TextTemplates?.FirstOrDefault();
            }
        }
        catch (Exception ex)
        {
            LogWriter.CreateLogEntry(ex);
        }
    }

    #region ObservableObjects

    public bool? CardCheckUseSQLLite
    {
        get => _cardCheckUseSQLLite;
        set
        {
            using var settings = new SettingsReaderWriter();
            settings.DefaultSettings.CardCheckUseMSSQL = value;
            settings.SaveSettings();
            SetProperty(ref _cardCheckUseSQLLite, value);
        }
    }
    private bool? _cardCheckUseSQLLite;

    public string? SelectedDBServerName
    {
        get => _selectedDBServerName;
        set
        {
            using var settings = new SettingsReaderWriter();
            settings.DefaultSettings.SelectedDBServerName = value;
            settings.SaveSettings();
            SetProperty(ref _selectedDBServerName, value);
        }
    }
    private string? _selectedDBServerName;

    public string? SelectedDBServerPort
    {
        get => _selectedDBServerPort;
        set
        {
            using var settings = new SettingsReaderWriter();
            settings.DefaultSettings.SelectedDBServerPort = value;
            settings.SaveSettings();
            SetProperty(ref _selectedDBServerPort, value);
        }
    }
    private string? _selectedDBServerPort;

    public string? SelectedDBName
    {
        get => _selectedDBName;
        set
        {
            using var settings = new SettingsReaderWriter();
            settings.DefaultSettings.SelectedDBName = value;
            settings.SaveSettings();
            SetProperty(ref _selectedDBName, value);
        }
    }
    private string? _selectedDBName;

    public string? SelectedDBUsername
    {
        get => _selectedDBUsername;
        set
        {
            using var settings = new SettingsReaderWriter();
            settings.DefaultSettings.SelectedDBUsername = value;
            settings.SaveSettings();
            SetProperty(ref _selectedDBUsername, value);
        }
    }
    private string? _selectedDBUsername;

    public string? SelectedDBUserPwd
    {
        get => _selectedDBUserPwd;
        set
        {
            using var settings = new SettingsReaderWriter();
            using var enc = new RijndaelEnc();
            settings.DefaultSettings.SelectedDBUserPwd = enc.Encrypt(value);
            settings.SaveSettings();
            SetProperty(ref _selectedDBUserPwd, value);
        }
    }
    private string? _selectedDBUserPwd;

    public string? SelectedCustomProjectFolder
    {
        get => _selectedCustomProjectFolder;
        set
        {
            using var settings = new SettingsReaderWriter();
            settings.DefaultSettings.LastUsedCustomProjectPath = value?.ToString();
            settings.SaveSettings();
            SetProperty(ref _selectedCustomProjectFolder, value);
        }
    }
    private string? _selectedCustomProjectFolder;

    public string? SelectedProjectFolder
    {
        get => _selectedProjectFolder;
        set 
        {
            using var settings = new SettingsReaderWriter();
            settings.DefaultSettings.DefaultProjectOutputPath = value.ToString();
            settings.SaveSettings();
            SetProperty(ref _selectedProjectFolder, value);
        }
    }
    private string? _selectedProjectFolder;

    public string? SelectedRFIDGearPath
    {
        get => _selectedRFIDGearPath;
        set
        {
            using var settings = new SettingsReaderWriter();
            settings.DefaultSettings.DefaultRFIDGearExePath = value ?? "";
            settings.SaveSettings();
            SetProperty(ref _selectedRFIDGearPath, value);
        }
    }
    private string? _selectedRFIDGearPath;

    public bool? RemoveTemporaryReportsIsEnabled
    {
        get => _removeTemporaryReportsIsEnabled;
        set
        {
            SetProperty(ref _removeTemporaryReportsIsEnabled, value);
            using SettingsReaderWriter settings = new SettingsReaderWriter();

            settings.DefaultSettings.RemoveTemporaryReportsIsEnabled = value;
            settings.SaveSettings();
        }
    }
    private bool? _removeTemporaryReportsIsEnabled;

    public bool? CreateSubdirectoryIsEnabled
    {
        get => _createSubdirectoryIsEnabled;
        set
        {
            SetProperty(ref _createSubdirectoryIsEnabled, value);
            using SettingsReaderWriter settings = new SettingsReaderWriter();

            settings.DefaultSettings.CreateSubdirectoryIsEnabled = value;
            settings.SaveSettings();
        }
    }
    private bool? _createSubdirectoryIsEnabled;

    [ObservableProperty]
    private bool? _isTextBoxCardCheckTextTemplateEnabled;
    
    public string? SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            SetProperty(ref _selectedTheme, value);
            var m_window = (Application.Current as App)?.Window as MainWindow ?? new MainWindow();
            m_window.SelectedTheme = value;
            
            using SettingsReaderWriter settings = new SettingsReaderWriter();

            settings.DefaultSettings.DefaultTheme = value;
            settings.SaveSettings();
        }
    }
    private string? _selectedTheme;

    public bool? RFiDGearIsAutoRunEnabled
    {
        get => _rFiDGearIsAutoRunEnabled;
        set
        {
            SetProperty(ref _rFiDGearIsAutoRunEnabled, value);
            using SettingsReaderWriter settings = new SettingsReaderWriter();

            settings.DefaultSettings.AutoLoadProjectOnStart = value;
            settings.SaveSettings();
        }
    }
    private bool? _rFiDGearIsAutoRunEnabled;

    public ObservableCollection<string>? ThemeSource
    {
        get => _themeSource;
        set
        {
            SetProperty(ref _themeSource, value);
        }
    }
    private ObservableCollection<string>? _themeSource;

    public string TextTemplateName
    {
        get => _textTemplateName;
        set
        {
            SetProperty(ref _textTemplateName, value);
        }
    }
    private string _textTemplateName;

    public ObservableCollection<CardCheckTextTemplate>? TextTemplates
    {
        get => _textTemplates;
        set
        {
            SetProperty(ref _textTemplates, value);
        }
    }
    private ObservableCollection<CardCheckTextTemplate>? _textTemplates;

    public CardCheckTextTemplate SelectedTextTemplate
    {
        get => _selectedTextTemplate;
        set
        {
            SetProperty(ref _selectedTextTemplate, value);
        }
    }
    private CardCheckTextTemplate _selectedTextTemplate;
    #endregion

    #region Commands
    public ICommand DBConnectionTest => new AsyncRelayCommand(DBConnectionTest_Executed);

    public ICommand CreateNewTextTemplate => new AsyncRelayCommand(CreateNewTextTemplate_Executed);

    public ICommand DeleteTextTemplate => new AsyncRelayCommand(DeleteTextTemplate_Executed);

    public ICommand NavigateBackCommand => new RelayCommand(NavigateBackCommand_Executed);

    public ICommand SelectRFIDGearExeCommand => new AsyncRelayCommand(SelectRFIDGearExe_Executed);
    
    public ICommand SelectProjectFolderCommand => new AsyncRelayCommand(SelectProjectFolder_Executed);

    public ICommand SelectRFIDGearCustomProjectCommand => new AsyncRelayCommand(SelectRFIDGearCustomProjectCommand_Executed);
    #endregion

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

            LogWriter.CreateLogEntry(ex);
        }
    }

    private async Task DeleteTextTemplate_Executed()
    {
        try
        {
            TextTemplates?.Remove(SelectedTextTemplate);

            if(TextTemplates?.FirstOrDefault() != null)
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

            LogWriter.CreateLogEntry(ex);
        }

    }

    private async Task DBConnectionTest_Executed()
    {
        using (SettingsReaderWriter settings = new SettingsReaderWriter())
        {
            // Connect to DB Async
            if (settings.DefaultSettings.CardCheckUseMSSQL ?? false)
            {
                using (SQLDBService dbService = new SQLDBService(
                    settings.DefaultSettings.SelectedDBServerName,
                    settings.DefaultSettings.SelectedDBName,
                    settings.DefaultSettings.SelectedDBUsername,
                    settings.DefaultSettings.SelectedDBUserPwd))
                {
                    await dbService.GetCardChecksFromMSSQLAsync();
                }
            }
            else
            {
                using (SQLDBService dbService = new SQLDBService(""))
                {
                    await dbService.GetCardChecksFromSQLLiteAsync();
                }
            }
        }
    }

    private async Task SelectRFIDGearExe_Executed()
    {
        var window = (Application.Current as App)?.Window as MainWindow ?? new MainWindow();

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
        var window = (Application.Current as App)?.Window as MainWindow ?? new MainWindow();

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
    private async Task SelectProjectFolder_Executed()
    {
        var window = (Application.Current as App)?.Window as MainWindow ?? new MainWindow();

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

    private void NavigateBackCommand_Executed()
    {
        try
        {
            using (SettingsReaderWriter settings = new SettingsReaderWriter())
            {
                settings.DefaultSettings.CardCheckTextTemplates = TextTemplates;

                settings.SaveSettings();
            }
        }
        catch 
        { 
        }

        var window = (Application.Current as App)?.Window as MainWindow ?? new MainWindow();
        var navigation = window.Navigation;
        var t = navigation.GetCurrentNavigationViewItem();
        var homePage = navigation.GetNavigationViewItems(typeof(HomePage)).First();
        
        navigation.SetCurrentNavigationViewItem(homePage);
        homePage.IsEnabled = true;
    }

    private class RijndaelEnc : IDisposable
    {
        private bool _disposed;
        private readonly byte[] IV = { 22, 85, 121, 60, 1, 77, 14, 69, 210, 10, 41, 22, 91, 6, 32, 4 };
        private readonly byte[] KEY = { 12, 122, 12, 72, 9, 1, 53, 72, 11, 94, 66, 84, 26, 110, 210, 44, 109, 10, 9, 100, 31, 201, 11, 23, 75, 91, 12, 83, 22, 19, 33, 3 };

        public RijndaelEnc() { }

        public string Encrypt(string source)
        {
            try
            {
                return BitConverter.ToString(EncryptStringToBytes(source, KEY, IV));
            }
            catch(Exception ex)
            {
                Log4CSharp.LogWriter.CreateLogEntry(ex);
                return "";
            }
        }

        public string Decrypt(string source)
        {
            try
            {
                String[] arr = source.Split('-');
                byte[] array = new byte[arr.Length];
                for (int i = 0; i < arr.Length; i++)
                {
                    array[i] = Convert.ToByte(arr[i], 16);
                }

                // Decrypt the bytes to a string.
                return DecryptStringFromBytes(array, KEY, IV);
            }
            catch(Exception ex)
            {
                Log4CSharp.LogWriter.CreateLogEntry(ex);
                return "";
            }
        }

        private byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
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
            using (Rijndael rijAlg = Rijndael.Create())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        private string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
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
            string plaintext = "";

            // Create an Rijndael object
            // with the specified key and IV.
            using (Rijndael rijAlg = Rijndael.Create())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
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
                        LogWriter.CreateLogEntry(e);
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
}
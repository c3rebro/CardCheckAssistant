/*
 * Created by SharpDevelop.
 * DateCreated: 12.10.2017
 * Time: 15:26
 *
 */

using CardCheckAssistant.Services;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Serialization;

namespace CardCheckAssistant.DataAccessLayer;

/// <summary>
///
/// </summary>
[XmlRoot("DefaultSettings", IsNullable = false)]
public class DefaultSettings : IDisposable
{
    private Version Version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version();

    public DefaultSettings()
    {
    }

    public DefaultSettings(bool init)
    {
        ManifestVersion = string.Format("{0}.{1}.{2}", Version?.Major, Version?.Minor, Version?.Build);

        if(init)
        {
            _defaultLanguage = "english";
            _autoCheckForUpdates = true;
            _autoLoadProjectOnStart = false;
            _cardCheckUseSQLLite = true;
            _lastUsedProjectPath = "";
            _lastUsedCustomProjectPath = "";
            _defaultTheme = "Light";
            _defaultRFIDGearExePath = "";
            _defaultProjectOutputPath = "";

            _selectedDBName = "db";
            _selectedDBServerName = "localhost";
            _selectedDBServerPort = "1433";
            _selectedDBUsername = "user";
            _selectedDBUserPwd = "3A-85-DC-5F-49-E4-A0-79-F9-6F-26-FE-DB-6C-56-CB"; //12345678
        }

    }

    #region Properties

    /// <summary>
    ///
    /// </summary>
    public string? ManifestVersion
    {
        get; set;
    }

    /// <summary>
    ///
    /// </summary>
    public bool? CardCheckUseMSSQL
    {
        get => _cardCheckUseSQLLite;
        set => _cardCheckUseSQLLite = value;
    }
    private bool? _cardCheckUseSQLLite;

    /// <summary>
    /// 
    /// </summary>
    public string? SelectedDBServerName
    {
        get => _selectedDBServerName;
        set => _selectedDBServerName = value;
    }
    private string? _selectedDBServerName;

    /// <summary>
    /// 
    /// </summary>
    public string? SelectedDBServerPort
    {
        get => _selectedDBServerPort;
        set => _selectedDBServerPort = value;
    }
    private string? _selectedDBServerPort;

    /// <summary>
    /// 
    /// </summary>
    public string? SelectedDBName
    {
        get => _selectedDBName;
        set => _selectedDBName = value;
    }
    private string? _selectedDBName;

    /// <summary>
    /// 
    /// </summary>
    public string? SelectedDBUsername
    {
        get => _selectedDBUsername;
        set => _selectedDBUsername = value;
    }
    private string? _selectedDBUsername;

    /// <summary>
    /// 
    /// </summary>
    public string? SelectedDBUserPwd
    {
        get => _selectedDBUserPwd;
        set => _selectedDBUserPwd = value;
    }
    private string? _selectedDBUserPwd;

    /// <summary>
    ///
    /// </summary>
    public string? DefaultTheme
    {
        get => _defaultTheme;
        set => _defaultTheme = value;
    }
    private string? _defaultTheme;

    /// <summary>
    ///
    /// </summary>
    public bool? AutoCheckForUpdates
    {
        get => _autoCheckForUpdates;
        set => _autoCheckForUpdates = value;
    }
    private bool? _autoCheckForUpdates;

    /// <summary>
    ///
    /// </summary>
    public string? DefaultLanguage
    {
        get => _defaultLanguage;
        set => _defaultLanguage = value;
    }
    private string? _defaultLanguage;

    /// <summary>
    ///
    /// </summary>
    public bool? AutoLoadProjectOnStart
    {
        get => _autoLoadProjectOnStart;
        set => _autoLoadProjectOnStart = value;
    }
    private bool? _autoLoadProjectOnStart;

    /// <summary>
    ///
    /// </summary>
    public string? LastUsedProjectPath
    {
        get => _lastUsedProjectPath;
        set => _lastUsedProjectPath = value;
    }
    private string? _lastUsedProjectPath;


    /// <summary>
    ///
    /// </summary>
    public string? LastUsedCustomProjectPath
    {
        get => _lastUsedCustomProjectPath;
        set => _lastUsedCustomProjectPath = value;
    }
    private string? _lastUsedCustomProjectPath;

    /// <summary>
    ///
    /// </summary>
    public string? DefaultRFIDGearExePath
    {
        get => _defaultRFIDGearExePath;
        set => _defaultRFIDGearExePath = value;
    }
    private string? _defaultRFIDGearExePath;

    /// <summary>
    ///
    /// </summary>
    public string? DefaultProjectOutputPath
    {
        get => _defaultProjectOutputPath;
        set => _defaultProjectOutputPath = value;
    }
    private string? _defaultProjectOutputPath;
    #endregion properties

    #region Extensions

    private bool _disposed;

    void IDisposable.Dispose()
    {
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose any managed objects
                // ...
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

    // Destructor
    ~DefaultSettings()
    {
        Dispose(false);
    }

    #endregion Extensions
}
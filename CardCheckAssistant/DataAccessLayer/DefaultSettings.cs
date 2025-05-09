/*
 * Created by SharpDevelop.
 * DateCreated: 12.10.2017
 * Time: 15:26
 *
 */

using CardCheckAssistant.Models;
using CardCheckAssistant.Services;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Xml.Serialization;

namespace CardCheckAssistant.DataAccessLayer;

/// <summary>
///
/// </summary>
[XmlRoot("DefaultSettings", IsNullable = false)]
public class DefaultSettings : IDisposable
{
    private readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version();

    public DefaultSettings()
    {
    }

    public DefaultSettings(bool init)
    {
        ManifestVersion = string.Format("{0}.{1}.{2}", Version?.Major, Version?.Minor, Version?.Build);

        if(init)
        {
            _defaultLanguage = "english";
            _defaultReportLanguage = "deutsch";
            _autoCheckForUpdates = true;
            _autoRunProjectOnStart = false;
            _cardCheckUseSQLLite = true;
            _lastUsedProjectPath = "";
            _lastUsedCustomProjectPath = "";
            _defaultTheme = "Light";
            _defaultRFIDGearExePath = "";
            _defaultProjectOutputPath = "";
            _readerVolume = 0;
            _cardCheckTextTemplates = new ObservableCollection<CardCheckTextTemplate>(new());

            _selectedDBName = "db";
            _selectedDBTableName = "db-table";
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
    public ObservableCollection<CardCheckTextTemplate>? CardCheckTextTemplates
    {
        get; set;
    }
    private ObservableCollection<CardCheckTextTemplate>? _cardCheckTextTemplates;

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
    public string? SelectedDBTableName
    {
        get => _selectedDBTableName;
        set => _selectedDBTableName = value;
    }
    private string? _selectedDBTableName;

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
    public string? DefaultReportLanguage
    {
        get => _defaultReportLanguage;
        set => _defaultReportLanguage = value;
    }
    private string? _defaultReportLanguage;

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
    public bool? AutoRunProjectOnStart
    {
        get => _autoRunProjectOnStart;
        set => _autoRunProjectOnStart = value;
    }
    private bool? _autoRunProjectOnStart;

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
    public bool? RemoveTemporaryReportsIsEnabled
    {
        get => _removeTemporaryReportsIsEnabled;
        set => _removeTemporaryReportsIsEnabled = value;
    }
    private bool? _removeTemporaryReportsIsEnabled;

    /// <summary>
    ///
    /// </summary>
    public bool? CreateSubdirectoryIsEnabled
    {
        get => _createSubdirectoryIsEnabled;
        set => _createSubdirectoryIsEnabled = value;
    }
    private bool? _createSubdirectoryIsEnabled;

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
    public string? LastUsedDefaultProject
    {
        get => _lastUsedDefaultProject;
        set => _lastUsedDefaultProject = value;
    }
    private string? _lastUsedDefaultProject;

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

    /// <summary>
    ///
    /// </summary>
    public int? ReaderVolume
    {
        get => _readerVolume;
        set => _readerVolume = value;
    }
    private int? _readerVolume;
    #endregion properties

    #region Extensions

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

    #endregion Extensions
}
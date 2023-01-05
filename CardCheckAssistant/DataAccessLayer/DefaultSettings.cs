/*
 * Created by SharpDevelop.
 * Date: 12.10.2017
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
/// Description of MifareClassicDefaultSpecification.
/// </summary>
[XmlRoot("DefaultSpecification", IsNullable = false)]
public class DefaultSettings : IDisposable
{
    private Version Version = Assembly.GetExecutingAssembly().GetName().Version;

    public DefaultSettings()
    {
    }

    public DefaultSettings(bool init)
    {
        ManifestVersion = string.Format("{0}.{1}.{2}", Version.Major, Version.Minor, Version.Build);

        _defaultLanguage = "english";
        defaultAutoPerformTasksEnabled = false;
        autoCheckForUpdates = true;
        _autoLoadProjectOnStart = false;
        _lastUsedProjectPath = "";
        _defaultTheme = "Light";
    }

    #region properties

    /// <summary>
    ///
    /// </summary>
    public string ManifestVersion
    {
        get; set;
    }

    /// <summary>
    ///
    /// </summary>
    public string DefaultTheme
    {
        get => _defaultTheme;
        set => _defaultTheme = value;
    }

    private string _defaultTheme;

    /// <summary>
    ///
    /// </summary>
    public bool AutoCheckForUpdates
    {
        get => autoCheckForUpdates;
        set => autoCheckForUpdates = value;
    }
    private bool autoCheckForUpdates;

    /// <summary>
    ///
    /// </summary>
    public string DefaultLanguage
    {
        get => _defaultLanguage;
        set => _defaultLanguage = value;
    }
    private string _defaultLanguage;

    /// <summary>
    ///
    /// </summary>
    public bool DefaultAutoPerformTasksEnabled
    {
        get => defaultAutoPerformTasksEnabled;
        set => defaultAutoPerformTasksEnabled = value;
    }

    private bool defaultAutoPerformTasksEnabled;

    /// <summary>
    ///
    /// </summary>
    public bool AutoLoadProjectOnStart
    {
        get => _autoLoadProjectOnStart;
        set => _autoLoadProjectOnStart = value;
    }
    private bool _autoLoadProjectOnStart;

    /// <summary>
    ///
    /// </summary>
    public string LastUsedProjectPath
    {
        get => _lastUsedProjectPath;
        set => _lastUsedProjectPath = value;
    }
    private string _lastUsedProjectPath;

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
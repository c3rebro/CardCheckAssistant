using CardCheckAssistant.DataAccessLayer;
using CardCheckAssistant.Services;

using Log4CSharp;

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CardCheckAssistant.Services;


/// <summary>
/// Description of Class1.
/// </summary>
public class SettingsReaderWriter : IDisposable
{
    #region fields
    private static readonly string FacilityName = Assembly.GetExecutingAssembly().GetName().Name;

    private readonly string _settingsFileFileName = "settings.xml";
    private readonly string _updateConfigFileFileName = "update.xml";
    private readonly string _updateURL = @"https://github.com/c3rebro/RFiDGear/releases/latest/download/update.xml";
    private readonly int _updateInterval = 900;
    private readonly string _securityToken = "D68EF3A7-E787-4CC4-B020-878BA649B4CD";
    private readonly string _payload = "update.zip";
    private readonly string _infoText = "Version Info\n\ngoes here! \n==>";
    private readonly string _baseUri = @"https://github.com/c3rebro/RFiDGear/releases/latest/download/";

    private readonly XmlWriter xmlWriter;
    private readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version;

    private readonly string appDataPath;

    private bool _disposed;

    public DefaultSettings DefaultSettings
    {
        get =>  defaultSettings ?? new DefaultSettings(true);

        set
        {
            defaultSettings = value;
            if (defaultSettings != null)
            {
                SaveSettings();
            }
        }
    }
    private DefaultSettings defaultSettings;

    #endregion fields

    public SettingsReaderWriter()
    {
        try
        {
            appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            appDataPath = Path.Combine(appDataPath, "CardCheckAssistant");

            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            var xmlSettings = new XmlWriterSettings();
            xmlSettings.Encoding = new UTF8Encoding(false);

            xmlWriter = XmlWriter.Create(Path.Combine(appDataPath, _updateConfigFileFileName), xmlSettings);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Manifest");
            xmlWriter.WriteAttributeString("version", string.Format("{0}.{1}.{2}", Version.Major, Version.Minor, Version.Build));

            xmlWriter.WriteEndElement();
            xmlWriter.Close();

            var doc = new XmlDocument();
            doc.Load(Path.Combine(appDataPath, _updateConfigFileFileName));

            if (doc.SelectSingleNode("//CheckInterval") == null)
            {
                var CheckIntervalElem = doc.CreateElement("CheckInterval");
                var RemoteConfigUriElem = doc.CreateElement("RemoteConfigUri");
                var SecurityTokenElem = doc.CreateElement("SecurityToken");
                var BaseUriElem = doc.CreateElement("BaseUri");
                var PayLoadElem = doc.CreateElement("Payload");
                var InfoTextElem = doc.CreateElement("VersionInfoText");

                doc.DocumentElement.AppendChild(CheckIntervalElem);
                doc.DocumentElement.AppendChild(RemoteConfigUriElem);
                doc.DocumentElement.AppendChild(SecurityTokenElem);
                doc.DocumentElement.AppendChild(BaseUriElem);
                doc.DocumentElement.AppendChild(PayLoadElem);
                doc.DocumentElement.AppendChild(InfoTextElem);

                CheckIntervalElem.InnerText = _updateInterval.ToString(CultureInfo.CurrentCulture);
                RemoteConfigUriElem.InnerText = _updateURL;
                SecurityTokenElem.InnerText = _securityToken;
                BaseUriElem.InnerText = _baseUri;
                PayLoadElem.InnerText = _payload;
                InfoTextElem.InnerText = _infoText;

                doc.Save(Path.Combine(appDataPath, _updateConfigFileFileName));
            }
        }
        catch (Exception ex)
        {
            LogWriter.CreateLogEntry(ex);
        }

        if (!File.Exists(Path.Combine(appDataPath, _settingsFileFileName)))
        {
            try
            {
                defaultSettings = new DefaultSettings(true);

                var serializer = new XmlSerializer(defaultSettings.GetType());

                var txtWriter = new StreamWriter(Path.Combine(appDataPath, _settingsFileFileName));

                serializer.Serialize(txtWriter, defaultSettings);

                txtWriter.Close();
            }
            catch (Exception ex)
            {
                LogWriter.CreateLogEntry(ex);
            }
        }
        else
        {
            try
            {
                ReadSettings();
            }
            catch (Exception ex)
            {
                LogWriter.CreateLogEntry(ex);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool ReadSettings()
    {
        return ReadSettings("");
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public bool ReadSettings(string _fileName)
    {
        TextReader reader;
        int verInfo;

        if (!string.IsNullOrWhiteSpace(_fileName) && !File.Exists(_fileName))
        {
            return false;
        }

        if (File.Exists(_fileName) || (string.IsNullOrWhiteSpace(_fileName) && File.Exists(Path.Combine(appDataPath, _settingsFileFileName))))
        {
            var doc = new XmlDocument();

            try
            {
                var serializer = new XmlSerializer(typeof(DefaultSettings));

                if (string.IsNullOrWhiteSpace(_fileName) && File.Exists(Path.Combine(appDataPath, _settingsFileFileName)))
                {
                    doc.Load(@Path.Combine(appDataPath, _settingsFileFileName));

                    var node = doc.SelectSingleNode("//ManifestVersion");
                    verInfo = Convert.ToInt32(node.InnerText.Replace(".", string.Empty));

                    reader = new StreamReader(Path.Combine(appDataPath, _settingsFileFileName));
                }
                else
                {
                    doc.Load(_fileName);

                    var node = doc.SelectSingleNode("//ManifestVersion");
                    verInfo = Convert.ToInt32(node.InnerText.Replace(".", string.Empty));

                    reader = new StreamReader(_fileName);
                }

                if (verInfo > Convert.ToInt32(string.Format("{0}{1}{2}", Version.Major, Version.Minor, Version.Build)))
                {
                    throw new Exception(
                        string.Format("database that was tried to open is newer ({0}) than this version of rfidgear ({1})"
                                      , verInfo, Convert.ToInt32(string.Format("{0}{1}{2}", Version.Major, Version.Minor, Version.Build))
                                     )
                    );
                }

                defaultSettings = (serializer.Deserialize(reader) as DefaultSettings);

                reader.Close();
            }
            catch (Exception e)
            {
                LogWriter.CreateLogEntry(string.Format("{0}: {1}; {2}", DateTime.Now, e.Message, e.InnerException != null ? e.InnerException.Message : ""));

                return true;
            }

            return false;
        }
        return true;
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public bool SaveSettings()
    {
        return SaveSettings("");
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public bool SaveSettings(string _path)
    {
        try
        {
            TextWriter textWriter;
            var serializer = new XmlSerializer(typeof(DefaultSettings));

            textWriter = new StreamWriter(!string.IsNullOrEmpty(_path) ? @_path : @Path.Combine(appDataPath, _settingsFileFileName), false);

            serializer.Serialize(textWriter, defaultSettings);

            textWriter.Close();

            return true;
        }
        catch (XmlException ex)
        {
            LogWriter.CreateLogEntry(ex);
            return false;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                try
                {
                    if (xmlWriter != null)
                    {
                        xmlWriter.Close();
                    }

                    defaultSettings = null;
                    // Dispose any managed objects
                    // ...
                }

                catch (Exception ex)
                {
                    LogWriter.CreateLogEntry(ex);
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
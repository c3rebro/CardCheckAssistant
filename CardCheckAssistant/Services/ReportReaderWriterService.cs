using System.Collections.ObjectModel;
using System.Reflection;
using System.Xml;
using GemBox.Pdf;
using Log4CSharp;

namespace CardCheckAssistant.Services;

/// <summary>
/// 
/// </summary>
public class ReportReaderWriterService : IDisposable
{
    #region fields
    private readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version;

    private const string reportTemplateTempFileName = "temptemplate.pdf";
    private readonly string appDataPath;
    public string ReportOutputPath { get; set; }
    public string ReportTemplatePath { get; set; }

    #endregion fields

   #region Public Methods

    /// <summary>
    /// 
    /// </summary>
    public ReportReaderWriterService()
    {
        try
        {
            // Set license key to use GemBox.Pdf in Free mode.
            ComponentInfo.SetLicense("FREE-LIMITED-KEY");

            appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            appDataPath = System.IO.Path.Combine(appDataPath, "CardCheckAssistant");

            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            if (File.Exists(System.IO.Path.Combine(appDataPath, reportTemplateTempFileName)))
            {
                File.Delete(System.IO.Path.Combine(appDataPath, reportTemplateTempFileName));
            }
        }
        catch (Exception e)
        {
            LogWriter.CreateLogEntry(e);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public ObservableCollection<string> GetReportFields()
    {
        try
        {
            var temp = new ObservableCollection<string>();

            using (var pdfDoc = PdfDocument.Load(ReportTemplatePath))
            {
                var form = pdfDoc.Form;

                try
                {
                    if (form != null)
                    {
                        foreach (var _form in form.Fields)
                        {
                            temp.Add(_form.Name);
                        }
                    }
                }
                catch (Exception e)
                {
                    LogWriter.CreateLogEntry(e);
                }
            }

            return temp;

        }
        catch (XmlException e)
        {
            LogWriter.CreateLogEntry(e);
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="isReadOnly"></param>
    public void SetReadOnlyOnAllFields(bool isReadOnly)
    {
        try
        {
            if (!String.IsNullOrWhiteSpace(ReportOutputPath))
            {
                using (var pdfDoc = PdfDocument.Load(ReportTemplatePath))
                {
                    try
                    {
                        if(pdfDoc != null)
                        {
                            ReportTemplatePath = System.IO.Path.Combine(appDataPath, reportTemplateTempFileName);

                            var form = pdfDoc.Form;

                            foreach (var _field in form.Fields)
                            {
                                pdfDoc.Form.Fields[_field.Name].ReadOnly = isReadOnly;
                            }

                            pdfDoc.Save(ReportOutputPath);
                            pdfDoc.Close();

                            File.Copy(ReportOutputPath, System.IO.Path.Combine(appDataPath, reportTemplateTempFileName), true);
                        }

                    }
                    catch (Exception e)
                    {
                        LogWriter.CreateLogEntry(e);
                    }
                }
            }
        }
        catch (XmlException e)
        {
            LogWriter.CreateLogEntry(e);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_field"></param>
    /// <param name="_value"></param>
    public void SetReportField(string _field, string _value)
    {
        try
        {
            if (!String.IsNullOrWhiteSpace(ReportOutputPath))
            {
                using (var pdfDoc = PdfDocument.Load(ReportTemplatePath))
                {
                    try
                    {
                        ReportTemplatePath = System.IO.Path.Combine(appDataPath, reportTemplateTempFileName);

                        if(pdfDoc.Form.Fields[_field] != null)
                        {
                            pdfDoc.Form.Fields[_field].Hidden = false;
                            pdfDoc.Form.Fields[_field].ReadOnly = false;
                            pdfDoc.Form.Fields[_field].Value = _value;
                        }


                        pdfDoc.Save(ReportOutputPath);
                        pdfDoc.Close();

                        File.Copy(ReportOutputPath, System.IO.Path.Combine(appDataPath, reportTemplateTempFileName), true);
                    }
                    catch (Exception e)
                    {
                        LogWriter.CreateLogEntry(e);
                    }
                }
            }
        }
        catch (XmlException e)
        {
            LogWriter.CreateLogEntry(e);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_field"></param>
    /// <param name="_value"></param>
    public void ConcatReportField(string _field, string _value)
    {
        try
        {
            if (!String.IsNullOrWhiteSpace(ReportOutputPath))
            {
                ReportTemplatePath = System.IO.Path.Combine(appDataPath, reportTemplateTempFileName);

                using (var pdfDoc = PdfDocument.Load(ReportTemplatePath))
                {
                    try
                    {
                        var form = pdfDoc.Form;

                        pdfDoc.Form.Fields[_field].Hidden = false;
                        pdfDoc.Form.Fields[_field].ReadOnly = false;
                        pdfDoc.Form.Fields[_field].Value = string.Format("{0}{1}", pdfDoc.Form.Fields[_field]?.Value, _value);

                        pdfDoc.Save(ReportOutputPath);
                        pdfDoc.Close();

                        File.Copy(ReportOutputPath, System.IO.Path.Combine(appDataPath, reportTemplateTempFileName), true);
                    }
                    catch (Exception e)
                    {
                        LogWriter.CreateLogEntry(e);
                    }
                }
            }
        }
        catch (XmlException e)
        {
            LogWriter.CreateLogEntry(e);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_field"></param>
    /// <returns></returns>
    public string GetReportField(string _field)
    {
        try
        {
            var result = "";

            if (!String.IsNullOrWhiteSpace(ReportTemplatePath) && File.Exists(ReportTemplatePath))
            {
                using (var pdfDoc = PdfDocument.Load(ReportTemplatePath))
                {
                    try
                    {
                        var form = pdfDoc.Form;

                        result = pdfDoc.Form.Fields[_field]?.Value as string ?? string.Empty;

                        pdfDoc.Close();
                    }
                    catch (Exception e)
                    {
                        LogWriter.CreateLogEntry(e);
                    }
                }

                    
            }
            return result ?? string.Empty;
        }
        catch (XmlException e)
        {
            LogWriter.CreateLogEntry(e);
        }
        catch (Exception e2)
        {
            LogWriter.CreateLogEntry(e2);
        }

        return string.Empty;
    }

    /// <summary>
    /// 
    /// </summary>
    public void DeleteDatabase()
    {
        try
        {
            File.Delete(System.IO.Path.Combine(ReportOutputPath));
        }
        catch (XmlException e)
        {
            LogWriter.CreateLogEntry(e);
        }
    }
    #endregion
 

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

                catch (XmlException e)
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
    private bool _disposed;
}
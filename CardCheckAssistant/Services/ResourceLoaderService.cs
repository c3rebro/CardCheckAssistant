using CardCheckAssistant.DataAccessLayer;
using Log4CSharp;

using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Resources;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using Windows.UI.Core;
using System.Reflection;

namespace CardCheckAssistant.Services
{
    /// <summary>
    /// Description of ResourceLoaderService.
    /// </summary>

    /// <summary>
    /// Enables Binding even if target is not part of visual or logical tree. Thanks to:
    /// https://www.thomaslevesque.com/2011/03/21/wpf-how-to-bind-to-data-when-the-datacontext-is-not-inherited/
    /// </summary>
    public class BindingProxy : Freezable
    {
        #region Overrides of Freezable

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

        #endregion Overrides of Freezable

        public object Data
        {
            get => (object)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));
    }

    /// <summary>
    ///
    /// </summary>
    public sealed class EnumerateExtension : MarkupExtension
    {
        private readonly CultureInfo cultureInfo;
        private readonly ResourceManager resManager;

        /// <summary>
        ///
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="type"></param>
        public EnumerateExtension(Type type)
        {
            Type = type;

            var settings = new SettingsReaderWriter();
            resManager = new ResourceManager("CardCheckAssistant.Resources.Manifest", System.Reflection.Assembly.GetExecutingAssembly());
            settings.ReadSettings();

            cultureInfo = (new DefaultSettings().DefaultLanguage == "german") ? new CultureInfo("de-DE") : new CultureInfo("en-US");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var names = Enum.GetNames(Type);
            var values = new string[names.Length];

            for (var i = 0; i < names.Length; i++)
            { values[i] = ResourceLoaderService.GetResource(string.Format("ENUM.{0}.{1}", Type.Name, names[i])); }

            return values;
        }
    }

    public class EnumDescriptionTypeConverter : EnumConverter
    {
        public EnumDescriptionTypeConverter(Type type) : base(type) 
        { 

        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if(destinationType == (typeof(string)))
            {
                if (value == null)
                {
                    FieldInfo fi = value.GetType().GetField(value.ToString());

                    if (fi != null)
                    {
                        var attrib = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

                        return attrib[0].Description;
                    }
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    /// <summary>
    ///
    /// </summary>
    public sealed class ResourceLoaderService : IValueConverter, IDisposable
    {
        private static readonly string FacilityName = "RFiDGear";

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            try
            {
                if (parameter is string)
                {
                    return ResourceLoaderService.GetResource((parameter as string));
                }
                else if (value != null && value.GetType() == typeof(ObservableCollection<string>))
                {
                    var collection = new ObservableCollection<string>();

                    foreach (var s in value as ObservableCollection<string>)
                    {
                        collection.Add(ResourceLoaderService.GetResource(string.Format("ENUM.{0}", s)));
                    }
                    return collection;
                }
                else if (value != null && !(value is string))
                {
                    var t = string.Format("ENUM.{0}.{1}", value.GetType().Name, Enum.GetName(value.GetType(), value));
                    return ResourceLoaderService.GetResource(string.Format("ENUM.{0}.{1}", value.GetType().Name, Enum.GetName(value.GetType(), value)));
                }
                else if (value is string)
                {
                    return ResourceLoaderService.GetResource(string.Format("ENUM.{0}.{1}", value.GetType().Name, value));
                }
                else
                {
                    return "Ressource not Found";
                }
            }
            catch (Exception e)
            {
                LogWriter.CreateLogEntry(e, FacilityName);

                throw new ArgumentOutOfRangeException(
                    string.Format("parameter:{0}\nvalue:{1}",
                                  parameter ?? "no param",
                                  value ?? "no value"));
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value != null)
            {
                var names = Enum.GetNames(parameter as Type);
                var values = new string[names.Length];

                for (var i = 0; i < names.Length; i++)
                {
                    values[i] = ResourceLoaderService.GetResource(string.Format("ENUM.{0}.{1}", targetType.Name, names[i]));
                    if ((string)value == values[i])
                    {
                        return names[i];
                    }
                }

                throw new ArgumentException(null, "value");
            }
            return null;
        }

        /// <summary>
        ///
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="resName"></param>
        /// <returns></returns>
        public static string GetResource(string resName)
        {
            try
            {
                using (var settings = new SettingsReaderWriter())
                {
                    settings.ReadSettings();

                    var ressource = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse().GetString(resName);
                    return ressource.Replace("%NEWLINE", "\n");
                }

            }
            catch (Exception e)
            {
                LogWriter.CreateLogEntry(e, FacilityName);
                return string.Empty;
            }
        }
    }
}
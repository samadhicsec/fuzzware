using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Fuzzware.Fuzzsaw.Common
{
    [ValueConversion(typeof(XmlNode[]), typeof(String))]
    public class XmlNodeArray_String : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// Convert an array of XmlNodes (actually XmlElements) to a string.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (null == value)
                return null;

            XmlNode[] NodeArray = (XmlNode[])value;
            if ((null == NodeArray) || (0 == NodeArray.Length))
                return null;

            StringBuilder Output = new StringBuilder();
            for (int i = 0; i < NodeArray.Length; i++)
            {
                if (NodeArray[i] is XmlElement)
                {
                    XPathNavigator Nav = (NodeArray[i] as XmlElement).CreateNavigator();
                    Output.Append(Nav.OuterXml);
                }
                else if (NodeArray[i] is XmlCharacterData)
                {
                    Output.Append((NodeArray[i] as XmlCharacterData).Value);
                }
                if (i < NodeArray.Length - 1)
                    Output.AppendLine();
            }

            return Output.ToString();
        }

        /// <summary>
        /// Converts a string into an array of XmlNodes
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (null == value)
                return null;

            try
            {
                XmlDocument oXmlDoc = new XmlDocument();
                oXmlDoc.LoadXml("<Root>" + (value as string) + "</Root>");
                XmlNode root = oXmlDoc.FirstChild;
                List<XmlNode> oXmlNodeList = new List<XmlNode>();
                if ((null != root) && (root.HasChildNodes))
                {
                    for (int i = 0; i < root.ChildNodes.Count; i++)
                    {
                        oXmlNodeList.Add(root.ChildNodes[i]);
                    }
                }
                return oXmlNodeList.ToArray();
            }
            catch { }
            return null;
        }

        #endregion
    }

    [ValueConversion(typeof(ObservableCollection<string>), typeof(String[]))]
    public class ObservableStringCollectionToStringArray : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// Convert an ObservableCollection&lt;string> to a string array.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (null == value)
                return new string[0];

            return (value as ObservableCollection<string>).ToArray<string>();
        }

        /// <summary>
        /// Converts a string array into an ObservableCollection&lt;string>
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ObservableCollection<string> ret = new ObservableCollection<string>();
            if (null == value)
                return ret;

            string[] array = value as string[];
            for (int i = 0; i < array.Length; i++)
                ret.Add(array[i]);

            return ret;
        }

        #endregion
    }

    /// <summary>
    /// If the input is false then Collapsed is returned, otherwise Visible
    /// </summary>
    [ValueConversion(typeof(Boolean), typeof(System.Windows.Visibility))]
    public class BooleanToVisability : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (null != value)
            {
                if ((Boolean)value == true)
                    return System.Windows.Visibility.Visible;
            }
            return System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        #endregion
    }

    /// <summary>
    /// If the input is null then Collapsed is returned, otherwise Visible
    /// </summary>
    [ValueConversion(typeof(object), typeof(System.Windows.Visibility))]
    public class ObjectToVisability : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (null != value)
                return System.Windows.Visibility.Visible;
            return System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        #endregion
    }

    /// <summary>
    /// Changes the file extension
    /// </summary>
    [ValueConversion(typeof(String), typeof(String))]
    public class ChangeFileExtension : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((null != value) && (null != parameter) && (value is String) && (parameter is string))
            {
                String FilePath = value as string;
                String NewExt = parameter as string;
                if (!String.IsNullOrEmpty(FilePath))
                {
                    if (!String.IsNullOrEmpty(System.IO.Path.GetDirectoryName(FilePath)) &&
                        (-1 != System.IO.Path.GetDirectoryName(FilePath).IndexOfAny(System.IO.Path.GetInvalidPathChars())))
                    {
                        HelperCommands.ShowError.Execute(new ErrorHelper(null, "The File path contains illegal path characters", true), App.Current.MainWindow);
                        return "";
                    }
                    if (!String.IsNullOrEmpty(System.IO.Path.GetFileName(FilePath)) &&
                        (-1 != System.IO.Path.GetFileName(FilePath).IndexOfAny(System.IO.Path.GetInvalidFileNameChars())))
                    {
                        HelperCommands.ShowError.Execute(new ErrorHelper(null, "The File name contains illegal file name characters", true), App.Current.MainWindow);
                        return "";
                    }
                    FilePath = System.IO.Path.ChangeExtension(FilePath, NewExt);
                    return FilePath;
                }
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        #endregion
    }

    /// <summary>
    /// Appends the parameter value to the value
    /// </summary>
    [ValueConversion(typeof(String), typeof(String))]
    public class AppendString : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((null != value) && (null != parameter) && (value is String) && (parameter is string))
            {
                return (value as string) + (parameter as string);
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        #endregion
    }

    /// <summary>
    /// Converts between the AnyType chosen type and an XmlQualifiedName
    /// </summary>
    [ValueConversion(typeof(String), typeof(XmlQualifiedName))]
    public class ChangeStringToXMLQualifiedName : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is XmlQualifiedName))
                return "String";

            XmlQualifiedName TypeName = value as XmlQualifiedName;

            switch (TypeName.Name)
            {
                case "int":
                    return "Integer";
                case "decimal":
                    return "Decimal";
                case "hexBinary":
                    return "Binary";
                default:
                    return "String";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is String))
                return null;

            String TypeName = value as string;

            switch (TypeName)
            {
                case "Integer":
                    return new XmlQualifiedName("int", "http://www.w3.org/2001/XMLSchema");
                case "Decimal":
                    return new XmlQualifiedName("decimal", "http://www.w3.org/2001/XMLSchema");
                case "Binary":
                    return new XmlQualifiedName("hexBinary", "http://www.w3.org/2001/XMLSchema");
                default:
                    return new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema");
            }
        }

        #endregion
    }

    /// <summary>
    /// Used to convert between source and destination, but ensures if the source gets wiped out
    /// this isn't passed to their destination, the destination just keeps its value.
    /// </summary>
    [ValueConversion(typeof(object), typeof(object))]
    public class DoNothingWithEmtpyStrings : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((null == value) || String.IsNullOrEmpty(value.ToString()))
                return Binding.DoNothing;

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((null == value) || String.IsNullOrEmpty(value.ToString()))
                return Binding.DoNothing;

            return value;
        }

        #endregion
    }

    /// <summary>
    /// Gets the Validation.Errors property of a FrameworkElement and returns its error content (if it has any)
    /// </summary>
    [ValueConversion(typeof(ReadOnlyObservableCollection<ValidationError>), typeof(object))]
    public class GetError : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (null == value)
                return null;

            if (value is ReadOnlyObservableCollection<ValidationError>)
            {
                ReadOnlyObservableCollection<ValidationError> oErrors = value as ReadOnlyObservableCollection<ValidationError>;
                if (oErrors.Count > 0)
                    return oErrors[0].ErrorContent;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        #endregion
    }
}

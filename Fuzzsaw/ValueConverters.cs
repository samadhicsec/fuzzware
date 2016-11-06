using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Windows.Data;

namespace Fuzzware.Fuzzsaw
{
    [ValueConversion(typeof(object), typeof(Boolean))]
    public class Object_Boolean : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (null != value)
                return true;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        #endregion
    }

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
            if (0 == NodeArray.Length)
                return null;

            StringBuilder Output = new StringBuilder();
            for (int i = 0; i < NodeArray.Length; i++)
            {
                XmlElement Ele = NodeArray[i] as XmlElement;
                if (null != Ele)
                {
                    XPathNavigator Nav = Ele.CreateNavigator();
                    Output.Append(Nav.OuterXml);
                }
                else
                {
                    XmlText TextEle = NodeArray[i] as XmlText;
                    Output.Append(TextEle.Value);
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

        #endregion
    }

    [ValueConversion(typeof(object[]), typeof(String[]))]
    public class XmlNodeArrayArray_StringArray : IValueConverter
    {

        #region IValueConverter Members

        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (null == value)
                return null;

            object[] oItems = value as object[];
            XmlNodeArray_String oConverter = new XmlNodeArray_String();
            // Convert each XmlNode[] to a string
            List<string> oStringItems = new List<string>();
            for (int i = 0; i < oItems.Length; i++)
            {
                String Node = oConverter.Convert(oItems[i], null, null, null) as string;
                oStringItems.Add(Node);
            }
            return oStringItems.ToArray();
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (null == value)
                return null;

            string[] oStringItems = value as string[];

            XmlNodeArray_String oConverter = new XmlNodeArray_String();
            List<XmlNode[]> oItems = new List<XmlNode[]>();
            for (int i = 0; i < oStringItems.Length; i++)
            {
                XmlNode[] Node = oConverter.ConvertBack(oStringItems[i], null, null, null) as XmlNode[];
                oItems.Add(Node);
            }
            return oItems.ToArray();
        }

        #endregion
    }

    [ValueConversion(typeof(object), typeof(Boolean))]
    public class ObjectOfType_Boolean : IValueConverter
    {

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((null != value) && (null != parameter))
            {
                if (value.GetType().Name.Equals((parameter as string), StringComparison.CurrentCultureIgnoreCase))
                    return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        #endregion
    }

    [ValueConversion(typeof(String), typeof(Boolean))]
    public class StringNotEmpty_Boolean : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is string))
                return false;
            if (String.IsNullOrEmpty((value as string)))
                return false;
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        #endregion
    }

    [ValueConversion(typeof(String), typeof(Boolean))]
    public class String_UInt : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (null == value)
                return null;

            String val = value as string;
            uint ret = 0;
            if (UInt32.TryParse(val, out ret))
                return ret;
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (null == value)
                return null;

            return value.ToString();
        }

        #endregion
    }

    [ValueConversion(typeof(String), typeof(Boolean))]
    public class ProcessType_Desc : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (null == value)
                return null;

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (null == value)
                return null;

            //System.Windows.Controls.StackPanel oStackPanel = parameter as System.Windows.Controls.StackPanel;
            uint ProcId = 0;
            if (UInt32.TryParse(value as string, out ProcId))
            {
                return ProcId;
            }
            else
            {
                return value as string;
            }
        }

        #endregion
    }

    [ValueConversion(typeof(object), typeof(System.Windows.Visibility))]
    public class ObjectOfType_Visability : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((null != value) && (null != parameter))
            {
                if (value.GetType().Name.Equals((parameter as string), StringComparison.CurrentCultureIgnoreCase))
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

}

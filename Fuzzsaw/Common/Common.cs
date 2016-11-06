using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Media;
using System.IO;
using System.Xml.Serialization;
using System.Reflection;

namespace Fuzzware.Fuzzsaw.Common
{
    public partial class Common
    {
        #region Common Attached Dependency Properties

        #region IsSelected
        /// <summary>
        /// The 'IsSelected' attached Dependency Property allows any UIElement to have a property stating whether or not it is selected
        /// </summary>
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.RegisterAttached("IsSelected", typeof(bool), typeof(Common),
                new FrameworkPropertyMetadata(false));

        public static Boolean GetIsSelected(UIElement target)
        {
            if (null == target)
                return false;
            return (Boolean)target.GetValue(IsSelectedProperty);
        }

        public static void SetIsSelected(UIElement target, Boolean value)
        {
            if (null == target)
                return;
            target.SetValue(IsSelectedProperty, value);
        }
        #endregion

        #region IsExpanded
        /// <summary>
        /// The 'IsExpanded' attached Dependency Property allows any UIElement to have a property stating whether or not it is expanded
        /// </summary>
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.RegisterAttached("IsExpanded", typeof(bool), typeof(Common),
                new FrameworkPropertyMetadata(false));

        public static Boolean GetIsExpanded(UIElement target)
        {
            if (null == target)
                return false;
            return (Boolean)target.GetValue(IsExpandedProperty);
        }

        public static void SetIsExpanded(UIElement target, Boolean value)
        {
            if (null == target)
                return;
            target.SetValue(IsExpandedProperty, value);
        }
        #endregion

        #region Command
        /// <summary>
        /// The 'Command' attached Dependency Property allows any DependencyObject to have a Command property that can be set to a command
        /// that will be invoked from code behind in response to some event
        /// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(Common));

        public static ICommand GetCommand(DependencyObject target)
        {
            if (null == target)
                return null;
            return (ICommand)target.GetValue(CommandProperty);
        }

        public static void SetCommand(DependencyObject target, ICommand value)
        {
            if (null == target)
                return;
            target.SetValue(CommandProperty, value);
        }
        #endregion

        #endregion

        //public void ShowInWorkingArea(object sender, MouseButtonEventArgs e)
        //{
        //    FrameworkElement oSender = sender as FrameworkElement;

        //    if (null == oSender)
        //        return;

        //    // Get the tag which contains the name of the UserControl to display
        //    String UserControlName = oSender.Tag as string;
        //    if (String.IsNullOrEmpty(UserControlName))
        //        return;

        //    ((MainWindow2)Application.Current.MainWindow).ShowInWorkingArea(UserControlName);
        //}

        /// <summary>
        /// Do our own reflection to get the desired type T
        /// </summary>
        public static T GetPropertyFromDataContext<T>(object DataContext, String PropertyName)
        {
            if (null == DataContext)
                return default(T);

            foreach (PropertyInfo pi in DataContext.GetType().GetProperties())
            {
                if (pi.Name.Equals(PropertyName) && (pi.PropertyType == typeof(T)))
                    return (T)pi.GetValue(DataContext, null);
            }
            return default(T);
        }

        public static DependencyObject GetDependencyObjectFromVisualTree(DependencyObject startObject, Type type)
        {
            //Walk the visual tree to get the parent(ItemsControl) 
            //of this control
            DependencyObject parent = startObject;
            while (parent != null)
            {
                if (type.IsInstanceOfType(parent))
                    break;
                else
                    parent = VisualTreeHelper.GetParent(parent);
            }

            return parent;
        }

        /// <summary>
        /// De-serialise a Configuration file into an object
        /// </summary>
        public static T DeserializeFile<T>(String filename)
        {
            T oConfig = default(T);
            // Open the XML Configuration file
            FileStream myFileStream = null;
            try
            {
                myFileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "An error occurred reading file '" + filename + "'", MessageBoxButton.OK);
                return default(T);
            }

            // Deserialise the XML file into the auto generated object
            XmlSerializer Serializer = new XmlSerializer(typeof(T));
            try
            {
                oConfig = (T)Serializer.Deserialize(myFileStream);
            }
            catch (Exception e2)
            {
                MessageBox.Show(e2.Message, "An error occurred deserializing the file '" + filename + "'", MessageBoxButton.OK);
            }
            myFileStream.Close();

            return oConfig;
        }

        /// <summary>
        /// Serialise a Configuration object to a temporary file and return the path of that file.
        /// </summary>
        public static String SerializeFile<T>(T Config, String DefaultNamespace)
        {
            // Get temp file name
            String tempfile = Path.GetTempFileName();

            // Create temp file
            FileStream myFileStream = null;
            try
            {
                myFileStream = new FileStream(tempfile, FileMode.Create, FileAccess.Write, FileShare.Write);
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "An error occurred creating file '" + tempfile + "' for writing", MessageBoxButton.OK);
                return null;
            }

            // Deserialise the XML file into the auto generated object
            XmlSerializer Serializer = new XmlSerializer(typeof(T), DefaultNamespace);
            try
            {
                Serializer.Serialize(myFileStream, Config);
            }
            catch (Exception e2)
            {
                MessageBox.Show(e2.Message, "An error occurred serializing to file '" + tempfile + "'", MessageBoxButton.OK);
                tempfile = null;
            }
            myFileStream.Close();

            return tempfile;
        }
    }

    [ValueConversion(typeof(object), typeof(object))]
    public class DummyValueConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        #endregion
    }

    public class UtilityHelper
    {
        /// <summary>
        /// Walk visual tree to find the first DependencyObject 
        /// of the specific type.
        /// </summary>
        public static DependencyObject GetDependencyObjectFromVisualTree(DependencyObject startObject, Type type)
        {
            //Walk the visual tree to get the parent(ItemsControl) 
            //of this control
            DependencyObject parent = startObject;
            while (parent != null)
            {
                if (type.IsInstanceOfType(parent))
                    break;
                else
                    parent = VisualTreeHelper.GetParent(parent);
            }

            return parent;
        }
    }
}

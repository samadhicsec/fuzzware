using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Fuzzware.Fuzzsaw
{
    /// <summary>
    /// Interaction logic for NewProject.xaml
    /// </summary>
    public partial class NewProjectWindow : Window
    {
        public NewProjectWindow()
        {
            InitializeComponent();
        }

        private void acceptButton_Click(object sender, RoutedEventArgs e)
        {
            ICommand AcceptCommand = Common.Common.GetPropertyFromDataContext<ICommand>(DataContext, "AcceptCommand");
            if (null != AcceptCommand)
                AcceptCommand.Execute(null);

            if ((bool)Common.Common.GetPropertyFromDataContext<bool>(DataContext, "Validated"))
            {
                DialogResult = true;

                Close();
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;

            Close();
        }
    }

    [ValueConversion(typeof(string), typeof(string))]
    public class Directory_Name : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (null == value)
                return "";
            if (!(value is string))
                return "";
            String Dir = (value as string);
            // If it ends in a '\' remove it
            if(Dir.EndsWith(Path.DirectorySeparatorChar.ToString()))
                Dir = Dir.Substring(Dir.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            // Get the directory name
            String Name = Dir.Substring(Dir.LastIndexOf(Path.DirectorySeparatorChar) + 1);

            return Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}

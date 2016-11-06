using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Fuzzware.Fuzzsaw.PropertiesWindows.View
{
    /// <summary>
    /// Interaction logic for ProjectPropertyWindow.xaml
    /// </summary>
    public partial class ProjectPropertiesWindow : Window
    {
        public ProjectPropertiesWindow()
        {
            InitializeComponent();
        }

        private void OK(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

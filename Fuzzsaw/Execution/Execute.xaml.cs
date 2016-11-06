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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Fuzzware.Fuzzsaw.Execution
{
    /// <summary>
    /// Interaction logic for Execute.xaml
    /// </summary>
    public partial class Execute : UserControl
    {
        #region Dependency Properties declaration and setup

        static readonly DependencyProperty ConfigProperty = DependencyProperty.Register("Config", typeof(ConfigView), typeof(Execute));
        public ConfigView Config
        {
            get { return (ConfigView)GetValue(ConfigProperty); }
            set { SetValue(ConfigProperty, value); }
        }

        #endregion

        public Execute()
        {
            InitializeComponent();
        }

        private void bExecute_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow2)Application.Current.MainWindow).ExecuteFuzzer();
        }
    }
}

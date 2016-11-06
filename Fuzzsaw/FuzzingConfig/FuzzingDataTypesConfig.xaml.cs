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
using Fuzzware.Fuzzsaw.Common;

namespace Fuzzware.Fuzzsaw.FuzzingConfig
{
    /// <summary>
    /// Interaction logic for FuzzingDataTypesConfig.xaml
    /// </summary>
    public partial class FuzzingDataTypesConfig : UserControl
    {
        public static DependencyProperty STFCViewProperty;

        static FuzzingDataTypesConfig()
        {
            STFCViewProperty = DependencyProperty.Register("STFCView", typeof(STFCView), typeof(FuzzingDataTypesConfig));
        }

        public FuzzingDataTypesConfig()
        {
            InitializeComponent();
        }

        public STFCView STFCView
        {
            get { return (STFCView)GetValue(STFCViewProperty); }
            set { SetValue(STFCViewProperty, value); }
        }

        private void bdAddCustomStringFuzzer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            STFCView.AddCustomStringFuzzer();
            e.Handled = true;
        }

        private void bdAddCustomIntegerFuzzer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            STFCView.AddCustomIntegerFuzzer();
            e.Handled = true;
        }

        private void bdAddCustomDecimalFuzzer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            STFCView.AddCustomDecimalFuzzer();
            e.Handled = true;
        }

        private void bdAddCustomByteFuzzer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            STFCView.AddCustomByteFuzzer();
            e.Handled = true;
        }

        /// <summary>
        /// Annoyingly there is a TreeViewItem in the VisualTree that causes lots of UserControls to be selected, we don't want
        /// this, so we use this method to change the setting.
        /// </summary>
        private void UpdateTreeViewItem(object sender, RoutedEventArgs e)
        {
            TreeViewItem oTreeViewItem = UtilityHelper.GetDependencyObjectFromVisualTree(sender as DependencyObject, typeof(TreeViewItem)) as TreeViewItem;

            if(null != oTreeViewItem)
                oTreeViewItem.Focusable = false;
        }
    }
}

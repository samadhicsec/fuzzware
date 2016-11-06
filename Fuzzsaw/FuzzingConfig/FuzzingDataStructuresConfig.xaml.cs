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
    /// Interaction logic for FuzzingDataStructuresConfig.xaml
    /// </summary>
    public partial class FuzzingDataStructuresConfig : UserControl
    {
        public static DependencyProperty CTFCViewProperty;

        static FuzzingDataStructuresConfig()
        {
            CTFCViewProperty = DependencyProperty.Register("CTFCView", typeof(CTFCView), typeof(FuzzingDataStructuresConfig));
        }

        public CTFCView CTFCView
        {
            get { return (CTFCView)GetValue(CTFCViewProperty); }
            set { SetValue(CTFCViewProperty, value); }
        }

        public FuzzingDataStructuresConfig()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Annoyingly there is a TreeViewItem in the VisualTree that causes lots of UserControls to be selected, we don't want
        /// this, so we use this method to change the setting.
        /// </summary>
        private void UpdateTreeViewItem(object sender, RoutedEventArgs e)
        {
            TreeViewItem oTreeViewItem = UtilityHelper.GetDependencyObjectFromVisualTree(sender as DependencyObject, typeof(TreeViewItem)) as TreeViewItem;

            if (null != oTreeViewItem)
                oTreeViewItem.Focusable = false;
        }
    }
}

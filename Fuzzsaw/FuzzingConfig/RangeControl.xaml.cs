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

namespace Fuzzware.Fuzzsaw.FuzzingConfig
{
    /// <summary>
    /// Interaction logic for RangeControl.xaml
    /// </summary>
    public partial class RangeControl : UserControl
    {
        public static DependencyProperty RangeDataProperty;

        static RangeControl()
        {
            RangeDataProperty = DependencyProperty.Register("RangeData", typeof(RangeDataView), typeof(RangeControl));
        }

        public RangeControl()
        {
            InitializeComponent();
        }

        public RangeDataView RangeData
        {
            get { return (RangeDataView)GetValue(RangeDataProperty); }
            set { SetValue(RangeDataProperty, value); }
        }
    }
}

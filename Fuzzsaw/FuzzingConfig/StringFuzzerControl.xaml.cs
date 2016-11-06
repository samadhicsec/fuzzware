using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for StringFuzzerControl.xaml
    /// </summary>
    public partial class StringFuzzerControl : UserControl
    {
        public static DependencyProperty StringFuzzerDataProperty;
        public static DependencyProperty IsDefaultProperty;
        public static DependencyProperty IsCustomProperty;

        static StringFuzzerControl()
        {
            StringFuzzerDataProperty = DependencyProperty.Register("StringFuzzerData", typeof(StringFuzzerView), typeof(StringFuzzerControl));
            IsDefaultProperty = DependencyProperty.Register("IsDefault", typeof(bool), typeof(StringFuzzerControl),
                new FrameworkPropertyMetadata(false));
            IsCustomProperty = DependencyProperty.Register("IsCustom", typeof(bool), typeof(StringFuzzerControl),
                new FrameworkPropertyMetadata(false));
        }

        public StringFuzzerView StringFuzzerData
        {
            get { return (StringFuzzerView)GetValue(StringFuzzerDataProperty); }
            set { SetValue(StringFuzzerDataProperty, value); }
        }

        public bool IsDefault
        {
            get { return (bool)GetValue(IsDefaultProperty); }
            set { SetValue(IsDefaultProperty, value); }
        }

        public bool IsCustom
        {
            get { return (bool)GetValue(IsCustomProperty); }
            set { SetValue(IsCustomProperty, value); }
        }

        public StringFuzzerControl()
        {
            InitializeComponent();
        }

        private void DeleteFuzzer_Click(object sender, RoutedEventArgs e)
        {
            if (IsDefault)
            {
                if (StringFuzzerData.IsConfigEnabled)
                    StringFuzzerData.IsConfigEnabled = false;
                else
                    StringFuzzerData.IsConfigEnabled = true;
            }
            else if (IsCustom)
            {
                // Search the visual tree for the list box this control is part of
                ItemsControl oItemsControl = UtilityHelper.GetDependencyObjectFromVisualTree(this, typeof(ItemsControl)) as ItemsControl;

                // Find the index of this StringFuzzerConfig in the ListBox ItemsSource
                ObservableCollection<StringFuzzerView> oItemsSource = oItemsControl.ItemsSource as ObservableCollection<StringFuzzerView>;
                for (int i = 0; i < oItemsSource.Count; i++)
                    if (oItemsSource[i] == StringFuzzerData)
                    {
                        if(MessageBoxResult.Yes == MessageBox.Show("Are you sure you want to delete this custom fuzzer?", "Confirm delete", MessageBoxButton.YesNo))
                            oItemsSource.RemoveAt(i);
                        break;
                    }
            }
            e.Handled = true;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Get the ListBoxItem that this control is a child of and make sure it is not focusable
            ListBoxItem oListBoxItem = UtilityHelper.GetDependencyObjectFromVisualTree(sender as DependencyObject, typeof(ListBoxItem)) as ListBoxItem;
            if (null != oListBoxItem)
                oListBoxItem.Focusable = false;
        }
    }
}

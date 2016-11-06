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
    /// Interaction logic for ByteFuzzerControl.xaml
    /// </summary>
    public partial class ByteFuzzerControl : UserControl
    {
        public static DependencyProperty ByteFuzzerViewProperty;
        public static DependencyProperty IsDefaultProperty;
        public static DependencyProperty IsCustomProperty;

        static ByteFuzzerControl()
        {
            ByteFuzzerViewProperty = DependencyProperty.Register("ByteFuzzerView", typeof(ByteFuzzerView), typeof(ByteFuzzerControl));
            IsDefaultProperty = DependencyProperty.Register("IsDefault", typeof(bool), typeof(ByteFuzzerControl),
                new FrameworkPropertyMetadata(false));
            IsCustomProperty = DependencyProperty.Register("IsCustom", typeof(bool), typeof(ByteFuzzerControl),
                new FrameworkPropertyMetadata(false));
        }

        public ByteFuzzerView  ByteFuzzerView
        {
            get { return (ByteFuzzerView)GetValue(ByteFuzzerViewProperty); }
            set { SetValue(ByteFuzzerViewProperty, value); }
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

        public ByteFuzzerControl()
        {
            InitializeComponent();
        }

        private void DeleteFuzzer_Click(object sender, RoutedEventArgs e)
        {
            if (IsDefault)
            {
                if (ByteFuzzerView.IsConfigEnabled)
                    ByteFuzzerView.IsConfigEnabled = false;
                else
                    ByteFuzzerView.IsConfigEnabled = true;
            }
            else if (IsCustom)
            {
                // Search the visual tree for the list box this control is part of
                ItemsControl oItemsControl = UtilityHelper.GetDependencyObjectFromVisualTree(this, typeof(ItemsControl)) as ItemsControl;

                // Find the index of this StringFuzzerConfig in the ListBox ItemsSource
                ObservableCollection<ByteFuzzerView> oItemsSource = oItemsControl.ItemsSource as ObservableCollection<ByteFuzzerView>;
                for (int i = 0; i < oItemsSource.Count; i++)
                    if (oItemsSource[i] == ByteFuzzerView)
                    {
                        if (MessageBoxResult.Yes == MessageBox.Show("Are you sure you want to delete this custom fuzzer?", "Confirm delete", MessageBoxButton.YesNo))
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

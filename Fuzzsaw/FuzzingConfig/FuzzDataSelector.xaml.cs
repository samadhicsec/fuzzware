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
    /// Interaction logic for FuzzDataSelector.xaml
    /// </summary>
    public partial class FuzzDataSelector : UserControl
    {
        public static DependencyProperty oSource;

        static FuzzDataSelector()
        {
            oSource = DependencyProperty.Register("Source", typeof(ObservableCollection<ObservableString>), typeof(FuzzDataSelector));
        }

        public FuzzDataSelector()
        {
            InitializeComponent();
        }

        #region EmptyValuesAllowed

        /// <summary>
        /// EmptyValuesAllowedProperty DependencyProperty
        /// </summary>
        public static DependencyProperty EmptyValuesAllowedProperty =
                DependencyProperty.Register(
                        "EmptyValuesAllowed",
                        typeof(bool),
                        typeof(FuzzDataSelector),
                        new FrameworkPropertyMetadata(false));


        /// <summary>
        /// Gets or sets the value of the EditBox
        /// </summary>
        public bool EmptyValuesAllowed
        {
            get { return (bool)GetValue(EmptyValuesAllowedProperty); }
            set { SetValue(EmptyValuesAllowedProperty, value); }
        }

        #endregion

        public ObservableCollection<ObservableString> Source
        {
            get { return (ObservableCollection<ObservableString>)GetValue(oSource); }
            set { SetValue(oSource, value); }
        }

        private void lbFuzzData_KeyDown(object sender, KeyEventArgs e)
        {
            // Handle deleting items
            if ((e.Key == Key.Delete) || (e.Key == Key.Back))
            {
                ObservableCollection<ObservableString> StrCollection = lbFuzzData.ItemsSource as ObservableCollection<ObservableString>;
                // Get reference to last item in list
                object LastItem = StrCollection[StrCollection.Count - 1];
                // Do our best to set the correct
                try
                {
                    int SelectedIndex = lbFuzzData.SelectedIndex;

                    while (lbFuzzData.SelectedItems.Count > 0)
                    {
                        if (lbFuzzData.SelectedItems[0] == LastItem)
                        {
                            lbFuzzData.SelectedItems.RemoveAt(0);
                            continue;
                        }
                        StrCollection.Remove(lbFuzzData.SelectedItems[0] as ObservableString);
                    }
                    // Try to highlight an item
                    if (SelectedIndex > StrCollection.Count - 1)
                        SelectedIndex = StrCollection.Count - 1;
                    lbFuzzData.SelectedIndex = SelectedIndex;
                    ListBoxItem oLBI = lbFuzzData.ItemContainerGenerator.ContainerFromIndex(SelectedIndex) as ListBoxItem;
                    if (null != oLBI)
                        oLBI.Focus();
                }
                catch { }
            }

            // Handle moving items
            if ((Keyboard.Modifiers == ModifierKeys.Shift) && (e.Key != Key.RightShift) && (e.Key != Key.LeftShift))
            {
                ObservableCollection<ObservableString> StrCollection = lbFuzzData.ItemsSource as ObservableCollection<ObservableString>;
                if (e.Key == Key.OemPlus)
                {
                    // Make sure only 1 item is selected and it is not the first or last
                    if ((lbFuzzData.SelectedItems.Count != 1) || (lbFuzzData.SelectedIndex == 0) || (lbFuzzData.SelectedIndex == StrCollection.Count - 1))
                        return;

                    StrCollection.Move(lbFuzzData.SelectedIndex, lbFuzzData.SelectedIndex - 1);
                }
                if (e.Key == Key.OemMinus)
                {
                    // Make sure only 1 item is selected and it is not the second-to-last or last
                    if ((lbFuzzData.SelectedItems.Count != 1) || (lbFuzzData.SelectedIndex == StrCollection.Count - 2) || (lbFuzzData.SelectedIndex == StrCollection.Count - 1))
                        return;

                    StrCollection.Move(lbFuzzData.SelectedIndex, lbFuzzData.SelectedIndex + 1);
                }

                // TODO currently after a shift the ListBoxItem does not seem to have focus, as the UP/DOWN keys take you to the first element
                //try
                //{
                //    lbFuzzData.Focus();
                //    ListBoxItem oLBI = lbFuzzData.ItemContainerGenerator.ContainerFromIndex(lbFuzzData.SelectedIndex) as ListBoxItem;
                //    if (null != oLBI)
                //        oLBI.Focus();
                //}
                //catch { }
            }
        }

    }
}

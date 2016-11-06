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
using Fuzzware.Fuzzsaw;
using Fuzzware.Fuzzsaw.Common;

namespace Fuzzware.Fuzzsaw.FuzzingConfig.View
{
    /// <summary>
    /// Interaction logic for FuzzDataSelectorView.xaml
    /// </summary>
    public partial class FuzzDataSelectorView : UserControl
    {
        public FuzzDataSelectorView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// KeyDown handler for DataSelector ListBox to allow delete and re-order
        /// </summary>
        private void lbDataCollection_KeyDown(object sender, KeyEventArgs e)
        {
            ListBox oListBox = lbDataCollection;
            // Handle deleting items
            if ((e.Key == Key.Delete) || (e.Key == Key.Back))
            {
                // Get the Delete Command
                ICommand DeleteCommand = (ICommand)oListBox.GetValue(ListDataCommands.DeleteDataMethodProperty);
                if (null == DeleteCommand)
                    return;
                // Get reference to last item in list
                object LastItem = oListBox.Items[oListBox.Items.Count - 1];
                // Do our best to set the correct
                try
                {
                    int SelectedIndex = oListBox.SelectedIndex;

                    while (oListBox.SelectedItems.Count > 0)
                    {
                        // We never delete the last item, so if it is selected, just remove it from the SelectedItems collection
                        if (oListBox.SelectedItems[0] == LastItem)
                        {
                            oListBox.SelectedItems.RemoveAt(0);
                            continue;
                        }
                        // Call the delete command
                        DeleteCommand.Execute(oListBox.SelectedItems[0]);
                    }
                    // Try to highlight an item
                    if (SelectedIndex > oListBox.Items.Count - 1)
                        SelectedIndex = oListBox.Items.Count - 1;
                    oListBox.SelectedIndex = SelectedIndex;
                    ListBoxItem oLBI = oListBox.ItemContainerGenerator.ContainerFromIndex(SelectedIndex) as ListBoxItem;
                    if (null != oLBI)
                        oLBI.Focus();
                }
                catch { }
            }

            // Handle moving items
            if ((Keyboard.Modifiers == ModifierKeys.Control) && (e.Key != Key.RightCtrl) && (e.Key != Key.LeftCtrl))
            {
                ObservableCollection<ObservableString> StrCollection = oListBox.ItemsSource as ObservableCollection<ObservableString>;
                if (e.Key == Key.OemPlus)
                {
                    ICommand PromoteCommand = (ICommand)oListBox.GetValue(ListDataCommands.PromoteDataMethodProperty);
                    if (null == PromoteCommand)
                        return;
                    // Make sure only 1 item is selected and it is not the first or last
                    if ((oListBox.SelectedItems.Count != 1) || (oListBox.SelectedIndex == 0) || (oListBox.SelectedIndex == StrCollection.Count - 1))
                        return;

                    PromoteCommand.Execute(oListBox.SelectedItems[0]);
                }
                if (e.Key == Key.OemMinus)
                {
                    ICommand DemoteCommand = (ICommand)oListBox.GetValue(ListDataCommands.DemoteDataMethodProperty);
                    if (null == DemoteCommand)
                        return;
                    // Make sure only 1 item is selected and it is not the second-to-last or last
                    if ((oListBox.SelectedItems.Count != 1) || (oListBox.SelectedIndex == StrCollection.Count - 2) || (oListBox.SelectedIndex == StrCollection.Count - 1))
                        return;

                    DemoteCommand.Execute(oListBox.SelectedItems[0]);
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

        /// <summary>
        /// When the initial methods list box has focus add an Edit Menu to the main menu
        /// </summary>
        private void lbDataCollection_GotFocus(object sender, RoutedEventArgs e)
        {
            // Get menu item
            ListBox oListBox = sender as ListBox;
            MenuItem oMenuItem = oListBox.FindResource("EditMenuResource") as MenuItem;
            oMenuItem.DataContext = oListBox;
            // Add it to the main menu
            HelperCommands.AddToMainMenu.Execute(oMenuItem, this);
        }

        /// <summary>
        /// When the initial methods list box loses focus remove the Edit Menu from the main menu
        /// </summary>
        private void lbDataCollection_LostFocus(object sender, RoutedEventArgs e)
        {
            // Get menu item
            ListBox oListBox = sender as ListBox;
            MenuItem oMenuItem = oListBox.FindResource("EditMenuResource") as MenuItem;
            // Remove it to the main menu
            HelperCommands.RemoveFromMainMenu.Execute(oMenuItem, this);
        }
    }
}

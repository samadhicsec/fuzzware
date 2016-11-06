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

namespace Fuzzware.Fuzzsaw.Output.View
{
    /// <summary>
    /// Interaction logic for WSDLOutput.xaml
    /// </summary>
    public partial class WSDLOutputView
    {
        #region Dependency Properties

        #region DeleteReuseParameter
        /// <summary>
        /// DeleteReuseParameter DependencyProperty
        /// </summary>
        public static DependencyProperty DeleteReuseParameterProperty = DependencyProperty.RegisterAttached("DeleteReuseParameter", typeof(ICommand), typeof(WSDLOutputView));

        public static ICommand GetDeleteReuseParameter(UIElement target)
        {
            if (null == target)
                return null;
            return (ICommand)target.GetValue(DeleteReuseParameterProperty);
        }

        public static void SetDeleteReuseParameter(UIElement target, ICommand value)
        {
            if (null == target)
                return;
            target.SetValue(DeleteReuseParameterProperty, value);
        }
        #endregion

        #endregion

        public WSDLOutputView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// KeyDown handler for Initial Methods ListBox to allow delete and re-order
        /// </summary>
        private void lbReuseParameters_KeyDown(object sender, KeyEventArgs e)
        {
            ListBox oListBox = lbReuseParameters;
            // Handle deleting items
            if ((e.Key == Key.Delete) || (e.Key == Key.Back))
            {
                // Get the Delete Command
                ICommand DeleteCommand = (ICommand)oListBox.GetValue(WSDLOutputView.DeleteReuseParameterProperty);
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
        }

        /// <summary>
        /// When the reuse parameters list box has focus add an Edit Menu to the main menu
        /// </summary>
        private void lbReuseParameters_GotFocus(object sender, RoutedEventArgs e)
        {
            // Get menu item
            ListBox oListBox = sender as ListBox;
            MenuItem oMenuItem = oListBox.FindResource("EditMenuResource") as MenuItem;
            oMenuItem.DataContext = oListBox;
            // Add it to the main menu
            HelperCommands.AddToMainMenu.Execute(oMenuItem, this);
        }

        /// <summary>
        /// When the reuse parameters list box loses focus remove the Edit Menu from the main menu
        /// </summary>
        private void lbReuseParameters_LostFocus(object sender, RoutedEventArgs e)
        {
            // Get menu item
            ListBox oListBox = sender as ListBox;
            MenuItem oMenuItem = oListBox.FindResource("EditMenuResource") as MenuItem;
            // Remove it to the main menu
            HelperCommands.RemoveFromMainMenu.Execute(oMenuItem, this);
        }
    }
}

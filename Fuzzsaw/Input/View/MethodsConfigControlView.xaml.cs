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
using System.Xml;
using Fuzzware.Fuzzsaw.Common;

namespace Fuzzware.Fuzzsaw.Input.View
{
    /// <summary>
    /// Interaction logic for MethodsConfigControlView.xaml
    /// </summary>
    public partial class MethodsConfigControlView : UserControl
    {
        #region Dependency Properties

        #region DeleteInitialMethod
        /// <summary>
        /// Delete Initial DependencyProperty
        /// </summary>
        public static DependencyProperty DeleteInitialMethodProperty =
                DependencyProperty.RegisterAttached(
                        "DeleteInitialMethod",
                        typeof(ICommand),
                        typeof(MethodsConfigControlView));

        public static ICommand GetDeleteInitialMethod(UIElement target)
        {
            if (null == target)
                return null;
            return (ICommand)target.GetValue(DeleteInitialMethodProperty);
        }

        public static void SetDeleteInitialMethod(UIElement target, ICommand value)
        {
            if (null == target)
                return;
            target.SetValue(DeleteInitialMethodProperty, value);
        }
        #endregion

        #region PromoteInitialMethod
        /// <summary>
        /// Promote Initial DependencyProperty
        /// </summary>
        public static DependencyProperty PromoteInitialMethodProperty =
                DependencyProperty.RegisterAttached(
                        "PromoteInitialMethod",
                        typeof(ICommand),
                        typeof(MethodsConfigControlView));

        public static ICommand GetPromoteInitialMethod(UIElement target)
        {
            if (null == target)
                return null;
            return (ICommand)target.GetValue(PromoteInitialMethodProperty);
        }

        public static void SetPromoteInitialMethod(UIElement target, ICommand value)
        {
            if (null == target)
                return;
            target.SetValue(PromoteInitialMethodProperty, value);
        }
        #endregion

        #region DemoteInitialMethod
        /// <summary>
        /// Demote Initial DependencyProperty
        /// </summary>
        public static DependencyProperty DemoteInitialMethodProperty =
                DependencyProperty.RegisterAttached(
                        "DemoteInitialMethod",
                        typeof(ICommand),
                        typeof(MethodsConfigControlView));

        public static ICommand GetDemoteInitialMethod(UIElement target)
        {
            if (null == target)
                return null;
            return (ICommand)target.GetValue(DemoteInitialMethodProperty);
        }

        public static void SetDemoteInitialMethod(UIElement target, ICommand value)
        {
            if (null == target)
                return;
            target.SetValue(DemoteInitialMethodProperty, value);
        }
        #endregion

        #region AddDefaultParamMethod
        /// <summary>
        /// AddDefaultParamMethod DependencyProperty
        /// </summary>
        public static DependencyProperty AddDefaultParamMethodProperty =
                DependencyProperty.RegisterAttached(
                        "AddDefaultParamMethod",
                        typeof(ICommand),
                        typeof(MethodsConfigControlView), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        public static ICommand GetAddDefaultParamMethod(UIElement target)
        {
            if (null == target)
                return null;
            return (ICommand)target.GetValue(AddDefaultParamMethodProperty);
        }

        public static void SetAddDefaultParamMethod(UIElement target, ICommand value)
        {
            if (null == target)
                return;
            target.SetValue(AddDefaultParamMethodProperty, value);
        }
        #endregion

        #region RemoveDefaultParamMethod
        /// <summary>
        /// RemoveDefaultParamMethod DependencyProperty
        /// </summary>
        public static DependencyProperty RemoveDefaultParamMethodProperty =
                DependencyProperty.RegisterAttached(
                        "RemoveDefaultParamMethod",
                        typeof(ICommand),
                        typeof(MethodsConfigControlView), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        public static ICommand GetRemoveDefaultParamMethod(UIElement target)
        {
            if (null == target)
                return null;
            return (ICommand)target.GetValue(RemoveDefaultParamMethodProperty);
        }

        public static void SetRemoveDefaultParamMethod(UIElement target, ICommand value)
        {
            if (null == target)
                return;
            target.SetValue(RemoveDefaultParamMethodProperty, value);
        }
        #endregion

        #endregion

        public MethodsConfigControlView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// KeyDown handler for Initial Methods ListBox to allow delete and re-order
        /// </summary>
        private void lbMethods_KeyDown(object sender, KeyEventArgs e)
        {
            ListBox oListBox = lbInitialMethods;
            // Handle deleting items
            if ((e.Key == Key.Delete) || (e.Key == Key.Back))
            {
                // Get the Delete Command
                ICommand DeleteCommand = (ICommand)oListBox.GetValue(MethodsConfigControlView.DeleteInitialMethodProperty);
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
                    ICommand PromoteCommand = (ICommand)oListBox.GetValue(MethodsConfigControlView.PromoteInitialMethodProperty);
                    if (null == PromoteCommand)
                        return;
                    // Make sure only 1 item is selected and it is not the first or last
                    if ((oListBox.SelectedItems.Count != 1) || (oListBox.SelectedIndex == 0) || (oListBox.SelectedIndex == StrCollection.Count - 1))
                        return;

                    PromoteCommand.Execute(oListBox.SelectedItems[0]);
                }
                if (e.Key == Key.OemMinus)
                {
                    ICommand DemoteCommand = (ICommand)oListBox.GetValue(MethodsConfigControlView.DemoteInitialMethodProperty);
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
        private void lbInitialMethods_GotFocus(object sender, RoutedEventArgs e)
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
        private void lbInitialMethods_LostFocus(object sender, RoutedEventArgs e)
        {
            // Get menu item
            ListBox oListBox = sender as ListBox;
            MenuItem oMenuItem = oListBox.FindResource("EditMenuResource") as MenuItem;
            // Remove it to the main menu
            HelperCommands.RemoveFromMainMenu.Execute(oMenuItem, this);
        }
    }
}

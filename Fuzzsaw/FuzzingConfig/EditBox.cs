using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows;
using System.Diagnostics;
using System.Windows.Documents;
using System.Windows.Media;
using Fuzzware.Fuzzsaw.Common;

namespace Fuzzware.Fuzzsaw.FuzzingConfig
{
    /// <summary>
    /// EditBox is a custom cotrol that can switch between two modes: 
    /// editing and normal. When it is in editing mode, the content is
    /// displayed in a TextBox that provides editing capbability. When 
    /// the EditBox is in normal, its content is displayed in a TextBlock
    /// that is not editable.
    /// 
    /// This control is designed to be used in with a GridView View.
    /// </summary>
    public class EditBox : Control
    {
        private EditBoxAdorner _adorner;

        //A TextBox in the visual tree
        private FrameworkElement _textBox;

        //Specifies whether an EditBox can switch to editing mode. 
        //Set to true if the ListViewItem that contains the EditBox is 
        //selected, when the mouse pointer moves over the EditBox
        private bool _canBeEdit = false;

        //Specifies whether an EditBox can switch to editing mode.
        //Set to true when the ListViewItem that contains the EditBox is 
        //selected when the mouse pointer moves over the EditBox.
        private bool _isMouseWithinScope = false;

        //The ListView control that contains the EditBox
        private ItemsControl _itemsControl;

        //The ListBoxItem control that contains the EditBox
        private ListBoxItem _listBoxItem;

        //The ListBox control that contains the EditBox
        private ListBox _listBox;

        // The current value of the EditBox when it goes into editing mode
        private Object _currentValue;

        /// <summary>
        /// Static constructor
        /// </summary>
        static EditBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EditBox), 
                new FrameworkPropertyMetadata(typeof(EditBox)));
        }

        #region Public Methods

        /// <summary>
        /// Called when the tree for the EditBox has been generated.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            TextBlock textBlock = GetTemplateChild("PART_TextBlockPart") 
                   as TextBlock;
            Debug.Assert(textBlock != null, "No TextBlock!");

            _textBox = new TextBox();
            _adorner = new EditBoxAdorner(textBlock, _textBox);
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(textBlock);
            layer.Add(_adorner);

            // So we can finish editing the TextBox
            _textBox.KeyDown += new KeyEventHandler(OnKeyDown);
            _textBox.LostKeyboardFocus += 
              new KeyboardFocusChangedEventHandler(OnTextBoxLostKeyboardFocus);

            //Receive notification of the event to handle the column 
            //resize. 
            HookTemplateParentResizeEvent();

            //Capture the resize event to  handle ListView resize cases.
            HookItemsControlEvents();

            _listBoxItem = GetDependencyObjectFromVisualTree(this, typeof(ListBoxItem)) as ListBoxItem;
            Debug.Assert(_listBoxItem != null, "No ListBoxItem found");
            // So we can use ENTER to edit the value
            _listBoxItem.KeyDown += new KeyEventHandler(OnKeyDown);

            _listBox = GetDependencyObjectFromVisualTree(this, typeof(ListBox)) as ListBox;

        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// If the ListViewItem that contains the EditBox is selected, 
        /// when the mouse pointer moves over the EditBox, the corresponding
        /// MouseEnter event is the first of two events (MouseUp is the second)
        /// that allow the EditBox to change to editing mode.
        /// </summary>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            if (!IsEditing && IsParentSelected)
            {
                _canBeEdit = true;
            }
        }

        /// <summary>
        /// If the MouseLeave event occurs for an EditBox control that
        /// is in normal mode, the mode cannot be changed to editing mode
        /// until a MouseEnter event followed by a MouseUp event occurs.
        /// </summary>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            _isMouseWithinScope = false;
            _canBeEdit = false;
        }

        /// <summary>
        /// An EditBox switches to editing mode when the MouseUp event occurs
        /// for that EditBox and the following conditions are satisfied:
        /// 1. A MouseEnter event for the EditBox occurred before the 
        /// MouseUp event.
        /// 2. The mouse did not leave the EditBox between the
        /// MouseEnter and MouseUp events.
        /// 3. The ListViewItem that contains the EditBox was selected
        /// when the MouseEnter event occurred.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.ChangedButton == MouseButton.Right || 
                e.ChangedButton == MouseButton.Middle)
                return;

            if (!IsEditing)
            {
                if (!e.Handled && (_canBeEdit || _isMouseWithinScope))
                {
                    IsEditing = true;
                }

                //If the first MouseUp event selects the parent ListViewItem,
                //then the second MouseUp event puts the EditBox in editing 
                //mode
                if (IsParentSelected)
                    _isMouseWithinScope = true;
            }
        }

        #endregion

        #region Public Properties

        #region Value

        /// <summary>
        /// ValueProperty DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
                DependencyProperty.Register(
                        "Value",
                        typeof(object),
                        typeof(EditBox),
                        new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the value of the EditBox
        /// </summary>
         public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        #endregion

        #region IsEditing

        /// <summary>
        /// IsEditingProperty DependencyProperty
        /// </summary>
        public static DependencyProperty IsEditingProperty =
                DependencyProperty.Register(
                        "IsEditing",
                        typeof(bool),
                        typeof(EditBox),
                        new FrameworkPropertyMetadata(false));

        /// <summary>
        /// Returns true if the EditBox control is in editing mode.
        /// </summary>
        public bool IsEditing
        {
            get { return (bool)GetValue(IsEditingProperty); }
            private set
            {
                EditingLogic(value);
                
                SetValue(IsEditingProperty, value);
                _adorner.UpdateVisibilty(value);
            }
        }

        /// <summary>
        /// Here is the logic we want
        ///  - If we move to editing mode, record the value of the item
        ///  - If we move to editing mode on the last item, also clear its value
        ///  - If we move from editing mode, if the new value is empty, and this is not allowed, restore its old value
        ///  - If we move to editing mode on the last item, if the new value is empty, restore its old value
        /// </summary>
        /// <param name="editing"></param>
        private void EditingLogic(bool editing)
        {
            bool bIsLastItem = (_listBox.Items.Count > 0) && (_listBox.SelectedIndex == (_listBox.Items.Count - 1));
            if (editing) // We are about to edit it, record its value
            {
                _currentValue = Value;
                if(bIsLastItem) // Clear the value of the last item, it is text indicating an item can be added
                    Value = "";
            }
            else if(null != _currentValue) // Restore its value if it is empty
            {
                String newValue = (_textBox as TextBox).Text;
                if (!bIsLastItem)
                {
                    if (String.IsNullOrEmpty(newValue) && !EmptyValuesAllowed)
                        Value = _currentValue;
                }
                else
                {
                    if (String.IsNullOrEmpty(newValue) || newValue.Equals(_currentValue))
                        Value = _currentValue;
                    else // Add a new last element if the last item is not empty
                        (_listBox.ItemsSource as ObservableCollection<ObservableString>).Add(new ObservableString(_currentValue as String));
                }
            }
        }

        #endregion

        #region IsParentSelected

        /// <summary>
        /// Gets whether the ListViewItem that contains the 
        /// EditBox is selected.
        /// </summary>
        private bool IsParentSelected
        {
            get
            {
                if (_listBoxItem == null)
                    return false;
                else
                    return _listBoxItem.IsSelected;
            }
        }

        #endregion

        #region EmptyValuesAllowed

        /// <summary>
        /// EmptyValuesAllowedProperty DependencyProperty
        /// </summary>
        public static DependencyProperty EmptyValuesAllowedProperty =
                DependencyProperty.Register(
                        "EmptyValuesAllowed",
                        typeof(bool),
                        typeof(EditBox),
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

        #endregion

        #region Private Methods

        /// <summary>
        /// When an EditBox is in editing mode, pressing the ENTER or F2
        /// keys switches the EditBox to normal mode.
        /// When an EditBox is not in editing mode, pressing the ENTER 
        /// key switches the EditBox to editing mode.
        /// </summary>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (IsEditing && (e.Key == Key.Enter || e.Key == Key.F2))
            {
                IsEditing = false;
                _canBeEdit = false;
                // If we don't set focus here, after editing if we use the UP/DOWN keys the listbox (for some reason)
                // the selected item is always the first item in the list.
                _listBoxItem.Focus();
            }
            else if (!IsEditing && (e.Key == Key.Enter))
            {
                IsEditing = true;
            }
        }

        /// <summary>
        /// If an EditBox loses focus while it is in editing mode, 
        /// the EditBox mode switches to normal mode.
        /// </summary>
        private void OnTextBoxLostKeyboardFocus(object sender, 
                                             KeyboardFocusChangedEventArgs e)
        {
            IsEditing = false;
        }

        /// <summary>
        /// Sets IsEditing to false when the ListViewItem that contains an
        /// EditBox changes its size
        /// </summary>
        private void OnCouldSwitchToNormalMode(object sender, 
                                               RoutedEventArgs e)
        {
            IsEditing = false;
        }

        /// <summary>
        /// Walk the visual tree to find the ItemsControl and 
        /// hook its some events on it.
        /// </summar
        private void HookItemsControlEvents()
        {
            _itemsControl = GetDependencyObjectFromVisualTree(this, 
                                typeof(ItemsControl)) as ItemsControl;
            if (_itemsControl != null)
            {
                //Handle the Resize/ScrollChange/MouseWheel 
                //events to determine whether to switch to Normal mode
                _itemsControl.SizeChanged += 
                    new SizeChangedEventHandler(OnCouldSwitchToNormalMode);
                _itemsControl.AddHandler(ScrollViewer.ScrollChangedEvent, 
                    new RoutedEventHandler(OnScrollViewerChanged));
                _itemsControl.AddHandler(ScrollViewer.MouseWheelEvent, 
                    new RoutedEventHandler(OnCouldSwitchToNormalMode), true);
            }
        }

        /// <summary>
        /// If an EditBox is in editing mode and the content of a ListView is
        /// scrolled, then the EditBox switches to normal mode.
        /// </summary>
        private void OnScrollViewerChanged(object sender, RoutedEventArgs args)
        {
            if (IsEditing && Mouse.PrimaryDevice.LeftButton == 
                                MouseButtonState.Pressed)
            {
                IsEditing = false;
            }
        }

        /// <summary>
        /// Walk visual tree to find the first DependencyObject 
        /// of the specific type.
        /// </summary>
        private DependencyObject GetDependencyObjectFromVisualTree(DependencyObject startObject, Type type)
        {
            //Walk the visual tree to get the parent(ItemsControl) 
            //of this control
            DependencyObject parent = startObject;
            while (parent != null)
            {
                if (type.IsInstanceOfType(parent))
                    break;
                else
                    parent = VisualTreeHelper.GetParent(parent);
            }

            return parent;
        }

        /// <summary>
        /// When the size of the column containing the EditBox changes
        /// and the EditBox is in editing mode, switch the mode to normal mode 
        /// </summary>
        private void HookTemplateParentResizeEvent()
        {
            FrameworkElement parent = TemplatedParent as FrameworkElement;
            if (parent != null)
            {
                parent.SizeChanged += 
                    new SizeChangedEventHandler(OnCouldSwitchToNormalMode);
            }
        }

        #endregion
    }

}

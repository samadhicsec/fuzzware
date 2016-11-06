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
using System.Windows.Data;
using Fuzzware.Fuzzsaw.Common;

namespace Fuzzware.Fuzzsaw.Input
{
    /// <summary>
    /// GenericAdornable is a custom cotrol that can switch between two modes: 
    /// editing and normal. When it is in editing mode, the content is
    /// displayed in a TextBox that provides editing capbability. When 
    /// the GenericAdornable is in normal, its content is displayed in a TextBlock
    /// that is not editable.
    /// 
    /// </summary>
    public class GenericAdornable : Control
    {
        private GenericAdorner m_oAdorner;

        //A FrameworkElement in the visual tree
        private FrameworkElement m_oAdornerElement;
        private string m_DataSourcePath;
        private DependencyProperty m_AdornerProperty;
        private IValueConverter m_ValueConverter;

        public static RoutedUICommand FinishedEditting = new RoutedUICommand();

        //Specifies whether an GenericAdornable can switch to editing mode. 
        //Set to true if the ListViewItem that contains the GenericAdornable is 
        //selected, when the mouse pointer moves over the GenericAdornable
        private bool _canBeEdit = false;

        //Specifies whether an GenericAdornable can switch to editing mode.
        //Set to true when the ListViewItem that contains the GenericAdornable is 
        //selected when the mouse pointer moves over the GenericAdornable.
        private bool _isMouseWithinScope = false;

        //The ListView control that contains the GenericAdornable
        private ItemsControl _itemsControl;

        //The ListBoxItem control that contains the GenericAdornable
        private ListBoxItem m_listBoxItem;

        //The ListBox control that contains the GenericAdornable
        //private ListBox _listBox;

        public static readonly RoutedEvent CreateAdornerEvent =
          EventManager.RegisterRoutedEvent("CreateAdorner", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(GenericAdornable));

        /// <summary>
        /// When ApplyTemplate gets called this event is raised  and SetAdornedElement
        /// should be called to set the adorner element
        /// </summary>
        public event RoutedEventHandler CreateAdorner
        {
            add { AddHandler(CreateAdornerEvent, value); }
            remove { RemoveHandler(CreateAdornerEvent, value); }
        }


        /// <summary>
        /// Static constructor
        /// </summary>
        static GenericAdornable()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GenericAdornable),
                new FrameworkPropertyMetadata(typeof(GenericAdornable)));
        }

        #region Public Methods

        /// <summary>
        /// Called when the parent element for the GenericAdornable has been generated.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UIElement oUIElement = Template.FindName("PART_UIElementPart", this) as UIElement;

            RaiseCreateAdornerEvent();
            
            m_oAdorner = new GenericAdorner(oUIElement, m_DataSourcePath, m_oAdornerElement, m_AdornerProperty, m_ValueConverter);
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(oUIElement);
            layer.Add(m_oAdorner);

            // So we can finish editing the TextBox
            m_oAdornerElement.KeyDown += new KeyEventHandler(OnKeyDown);
            
            if(m_oAdornerElement is TextBox)
                m_oAdornerElement.LostKeyboardFocus += new KeyboardFocusChangedEventHandler(OnAdornerLostKeyboardFocus);
            else
                m_oAdornerElement.MouseLeave += new MouseEventHandler(OnAdornerMouseLeave);

            //Receive notification of the event to handle the column resize. 
            HookTemplateParentResizeEvent();

            //Capture the resize event to  handle ListView resize cases.
            HookItemsControlEvents();

            m_listBoxItem = GetDependencyObjectFromVisualTree(this, typeof(ListBoxItem)) as ListBoxItem;
            // So we can use ENTER to edit the value
            if(null != m_listBoxItem)
                m_listBoxItem.KeyDown += new KeyEventHandler(OnKeyDown);

        }

        /// <summary>
        /// The public method to call in response to the CreateAdorner event to set the Adorner Element
        /// </summary>
        public FrameworkElement AdornerElement
        {
            get { return m_oAdornerElement; }
            set { m_oAdornerElement = value; }
        }

        /// <summary>
        /// Raises the CreateAdorner Event
        /// </summary>
        private void RaiseCreateAdornerEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(CreateAdornerEvent);
            newEventArgs.Source = this;
            RaiseEvent(newEventArgs);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// If the ListViewItem that contains the GenericAdornable is selected, 
        /// when the mouse pointer moves over the GenericAdornable, the corresponding
        /// MouseEnter event is the first of two events (MouseUp is the second)
        /// that allow the GenericAdornable to change to editing mode.
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
        /// If the MouseLeave event occurs for an GenericAdornable control that
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
        /// An GenericAdornable switches to editing mode when the MouseUp event occurs
        /// for that GenericAdornable and the following conditions are satisfied:
        /// 1. A MouseEnter event for the GenericAdornable occurred before the 
        /// MouseUp event.
        /// 2. The mouse did not leave the GenericAdornable between the
        /// MouseEnter and MouseUp events.
        /// 3. The ListViewItem that contains the GenericAdornable was selected
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

        #region DataSourcePath
        /// <summary>
        /// The AdornableElement Path that gets bound to the AdornerProperty of the AdornerElement 
        /// </summary>
        public String DataSourcePath
        {
            get { return m_DataSourcePath; }
            set { m_DataSourcePath = value; }
        }
        #endregion

        #region AdornerProperty
        /// <summary>
        /// The DependencyProperty of the AdornerElement to bind to the AdornableElement Path
        /// </summary>
        public DependencyProperty AdornerProperty
        {
            get { return m_AdornerProperty; }
            set { m_AdornerProperty = value; }
        }
        #endregion

        #region ValueConverter
        /// <summary>
        /// The IValueConverter of the AdornerElement to bind to the AdornableElement Path
        /// </summary>
        public IValueConverter ValueConverter
        {
            get { return m_ValueConverter; }
            set { m_ValueConverter = value; }
        }
        #endregion

        #region Value

        /// <summary>
        /// ValueProperty DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
                DependencyProperty.Register(
                        "Value",
                        typeof(object),
                        typeof(GenericAdornable),
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
                        typeof(GenericAdornable),
                        new FrameworkPropertyMetadata(false));

        /// <summary>
        /// Returns true if the GenericAdornable control is in editing mode.
        /// </summary>
        public bool IsEditing
        {
            get { return (bool)GetValue(IsEditingProperty); }
            private set
            {
                //EditingLogic(value);
                //Debug.WriteLine("IsEditing set to " + value);
                SetValue(IsEditingProperty, value);
                m_oAdorner.UpdateVisibilty(value);

                // If we finish editting issue command
                if (!value)
                    FinishedEditting.Execute(null, this);
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
            //bool bIsLastItem = (_listBox.Items.Count > 0) && (_listBox.SelectedIndex == (_listBox.Items.Count - 1));
            //if (editing) // We are about to edit it, record its value
            //{
            //    _currentValue = Value;
            //    if (bIsLastItem) // Clear the value of the last item, it is text indicating an item can be added
            //        Value = "";
            //}
            //else if (null != _currentValue) // Restore its value if it is empty
            //{
            //    String newValue = (m_oFrwkElement as TextBox).Text;
            //    if (!bIsLastItem)
            //    {
            //        if (String.IsNullOrEmpty(newValue) && !EmptyValuesAllowed)
            //            Value = _currentValue;
            //    }
            //    else
            //    {
            //        if (String.IsNullOrEmpty(newValue) || newValue.Equals(_currentValue))
            //            Value = _currentValue;
            //        else // Add a new last element if the last item is not empty
            //            (_listBox.ItemsSource as ObservableCollection<ObservableString>).Add(new ObservableString(_currentValue as String));
            //    }
            //}
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
                //if (_listBoxItem == null)
                //    return false;
                //else
                //    return _listBoxItem.IsSelected;
                if (m_listBoxItem != null)
                    return m_listBoxItem.IsSelected;
                return true;
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
                        typeof(GenericAdornable),
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
        /// When an GenericAdornable is in editing mode, pressing the ENTER or F2
        /// keys switches the GenericAdornable to normal mode.
        /// When an GenericAdornable is not in editing mode, pressing the ENTER 
        /// key switches the GenericAdornable to editing mode.
        /// </summary>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            //Debug.WriteLine("OnKeyDown called");
            if (IsEditing && (e.Key == Key.Enter || e.Key == Key.F2))
            {
                IsEditing = false;
                _canBeEdit = false;
                // If we don't set focus here, after editing if we use the UP/DOWN keys the listbox (for some reason)
                // the selected item is always the first item in the list.
                if(null != m_listBoxItem)
                    m_listBoxItem.Focus();
            }
            else if (!IsEditing && (e.Key == Key.Enter))
            {
                IsEditing = true;
            }
        }

        /// <summary>
        /// If an Adorner loses focus while it is in editing mode, 
        /// the Adornable mode switches to normal mode.
        /// </summary>
        private void OnAdornerLostKeyboardFocus(object sender,
                                             KeyboardFocusChangedEventArgs e)
        {
            //Debug.WriteLine("OnAdornerLostKeyboardFocus called");
            IsEditing = false;
        }

        /// <summary>
        /// On non-textbox adorner elements, when the mouse leaves the element
        /// editting mode is disabled
        /// </summary>
        private void OnAdornerMouseLeave(object sender,
                                             MouseEventArgs e)
        {
            //Debug.WriteLine("OnAdornerMouseLeave called");
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
        /// If an GenericAdornable is in editing mode and the content of a ListView is
        /// scrolled, then the GenericAdornable switches to normal mode.
        /// </summary>
        private void OnScrollViewerChanged(object sender, RoutedEventArgs args)
        {
            //Debug.WriteLine("OnScrollViewerChanged called");
            // TODO See if this check makes sense.  When the Adorner is a ComboBox the first time you open it
            // this event gets triggered and IsEditting gets set to false which makes the Adorner disappear.
            // We don't want this to happen but we do want actual scolling to have this effect.
            if ((bool)_itemsControl.GetValue(ScrollViewer.IsFocusedProperty))
            {
                if (IsEditing && Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed)
                {
                    IsEditing = false;
                }
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
        /// When the size of the column containing the GenericAdornable changes
        /// and the GenericAdornable is in editing mode, switch the mode to normal mode 
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

        /// <summary>
        /// Sets IsEditing to false when the ListViewItem that contains an
        /// GenericAdornable changes its size
        /// </summary>
        private void OnCouldSwitchToNormalMode(object sender, RoutedEventArgs e)
        {
            //Debug.WriteLine("OnCouldSwitchToNormalMode called");
            IsEditing = false;
        }

        #endregion
    }

}

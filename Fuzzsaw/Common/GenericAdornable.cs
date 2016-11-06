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
using System.ComponentModel;
using Fuzzware.Fuzzsaw.Common;

namespace Fuzzware.Fuzzsaw.Common
{
    /// <summary>
    /// GenericAdornable is a custom control that can switch between two modes: 
    /// editing and normal. When it is in editing mode, an Adorner is shown
    /// that provides editing capbability. When the GenericAdornable is in normal, 
    /// its content is displayed (through whatever Style has been set) and it
    /// is not editable.
    /// 
    /// </summary>
    public class GenericAdornable : Control, IDataErrorInfo
    {
        private GenericAdorner m_oAdorner;

        //A FrameworkElement in the visual tree
        private Style m_AdornerContentControlStyle;
        private Type m_AdornerFrwkElementType;
        
        private IValueConverter m_ValueConverter;

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
            if (null == oUIElement)
                return;

            // Create the Adorner
            m_oAdorner = new GenericAdorner(oUIElement, this);

            // Bind the ProxyProperty of the Adorner to the Value property
            Binding oBinding = new Binding();
            oBinding.Source = this;
            oBinding.Path = new PropertyPath("Value");
            oBinding.Mode = BindingMode.TwoWay;
            oBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            m_oAdorner.FrwkElement.SetBinding(GenericAdorner.ProxyPropertyProperty, oBinding);

            // Assign the Adorner to the Adorner layer
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(oUIElement);
            layer.Add(m_oAdorner);

            // So we can finish editing the TextBox
            m_oAdorner.FrwkElement.KeyDown += new KeyEventHandler(OnKeyDown);

            if (m_oAdorner.FrwkElement is TextBox)
                m_oAdorner.FrwkElement.LostKeyboardFocus += new KeyboardFocusChangedEventHandler(OnAdornerLostKeyboardFocus);
            else
                m_oAdorner.FrwkElement.MouseLeave += new MouseEventHandler(OnAdornerMouseLeave);

            //Receive notification of the event to handle the column resize. 
            HookTemplateParentResizeEvent();

            //Capture the resize event to  handle ListView resize cases.
            HookItemsControlEvents();

            m_listBoxItem = Common.GetDependencyObjectFromVisualTree(this, typeof(ListBoxItem)) as ListBoxItem;
            // So we can use ENTER to edit the value
            if (null != m_listBoxItem)
            {
                m_listBoxItem.KeyDown += new KeyEventHandler(OnKeyDown);
                m_listBoxItem.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            }

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
            //Debug.WriteLine("GenericAdornable: OnMouseEnter called");
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
            //Debug.WriteLine("GenericAdornable: OnMouseLeave called");
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
            //Debug.WriteLine("GenericAdornable: OnMouseUp called");
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

        #region AdornerContentControlStyle
        /// <summary>
        /// The Style that will get applied to the adorner content control
        /// </summary>
        public Style AdornerContentControlStyle
        {
            get { return m_AdornerContentControlStyle; }
            set { m_AdornerContentControlStyle = value; }
        }
        #endregion

        #region AdornerFrwkElementType
        /// <summary>
        /// The ancestor Type of FramewordElement to create as the adorner
        /// </summary>
        public Type AdornerFrwkElementType
        {
            get { return m_AdornerFrwkElementType; }
            set { m_AdornerFrwkElementType = value; }
        }
        #endregion

        #region AdornerDataContext
        public static readonly DependencyProperty AdornerDataContextProperty = DependencyProperty.Register("AdornerDataContext", typeof(object), typeof(GenericAdornable));
        /// <summary>
        /// The DataContext get applied to the adorner content control
        /// </summary>
        public object AdornerDataContext
        {
            get { return GetValue(AdornerDataContextProperty); }
            set { SetValue(AdornerDataContextProperty, value); }
        }
        #endregion

        #region StartedEditting
        public static readonly DependencyProperty StartedEdittingProperty = DependencyProperty.Register("StartedEditting", typeof(ICommand), typeof(GenericAdornable));
        /// <summary>
        /// The ICommand to execute when editting starts
        /// </summary>
        public ICommand StartedEditting
        {
            get { return (ICommand)GetValue(StartedEdittingProperty); }
            set { SetValue(StartedEdittingProperty, value); }
        }
        #endregion

        #region StartedEdittingParameter
        public static readonly DependencyProperty StartedEdittingParameterProperty = DependencyProperty.Register("StartedEdittingParameter", typeof(object), typeof(GenericAdornable));
        /// <summary>
        /// The Parameter to pass to the StartedEditting command
        /// </summary>
        public object StartedEdittingParameter
        {
            get { return GetValue(StartedEdittingParameterProperty); }
            set { SetValue(StartedEdittingParameterProperty, value); }
        }
        #endregion

        #region FinishedEditting
        public static readonly DependencyProperty FinishedEdittingProperty = DependencyProperty.Register("FinishedEditting", typeof(ICommand), typeof(GenericAdornable));
        /// <summary>
        /// The ICommand to execute once finished editting
        /// </summary>
        public ICommand FinishedEditting
        {
            get { return (ICommand)GetValue(FinishedEdittingProperty); }
            set { SetValue(FinishedEdittingProperty, value); }
        }
        #endregion

        #region FinishedEdittingParameter
        public static readonly DependencyProperty FinishedEdittingParameterProperty = DependencyProperty.Register("FinishedEdittingParameter", typeof(object), typeof(GenericAdornable));
        /// <summary>
        /// The Parameter to pass to the FinishedEditting command
        /// </summary>
        public object FinishedEdittingParameter
        {
            get { return GetValue(FinishedEdittingParameterProperty); }
            set { SetValue(FinishedEdittingParameterProperty, value); }
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
        /// Gets or sets the value of the GenericAdornable
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
                bool bCurrentValue = IsEditing;
                //Debug.WriteLine("IsEditing set to " + value);
                // If we are setting IsEditting = true and it was not already true, execute the StartedEditting Command if there is one
                if (value && (bCurrentValue != value) && (null != StartedEditting))
                    StartedEditting.Execute(StartedEdittingParameter);

                SetValue(IsEditingProperty, value);
                m_oAdorner.UpdateVisibilty(value);

                // If we are setting IsEditting = false and it was not already false, execute the FinishedEditting Command if there is one
                if ((!value) && (bCurrentValue != value) && (null != FinishedEditting))
                    FinishedEditting.Execute(FinishedEdittingParameter);
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

        #region ValidateValueMethod
        public static readonly DependencyProperty ValidateValueMethodProperty = DependencyProperty.Register("ValidateValueMethod", typeof(Func<object, string>), typeof(GenericAdornable));

        /// <summary>
        /// The Method to validate Value.  Need to set ValidatesOnDataErrors=True on the Value binding
        /// </summary>
        public Func<object, string> ValidateValueMethod
        {
            get { return (Func<object, string>)GetValue(ValidateValueMethodProperty); }
            set { SetValue(ValidateValueMethodProperty, value); }
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
                if (null != m_listBoxItem)
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
            //Debug.WriteLine("GenericAdornable: OnAdornerMouseLeave called");
            IsEditing = false;
        }

        /// <summary>
        /// Walk the visual tree to find the ItemsControl and 
        /// hook its some events on it.
        /// </summar
        private void HookItemsControlEvents()
        {
            _itemsControl = Common.GetDependencyObjectFromVisualTree(this,
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
            //Debug.WriteLine("GenericAdornable: OnScrollViewerChanged called");
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
        /// When the size of the column containing the GenericAdornable changes
        /// and the GenericAdornable is in editing mode, switch the mode to normal mode 
        /// </summary>
        private void HookTemplateParentResizeEvent()
        {
            FrameworkElement parent = TemplatedParent as FrameworkElement;
            if (parent != null)
            {
                // TODO: Determine if it si ok to not hook this event.  It is being called when the adorner is textbox after
                // every key press, so you can't enter more than one key at a time
                //parent.SizeChanged +=
                //    new SizeChangedEventHandler(OnCouldSwitchToNormalMode);
            }
        }

        /// <summary>
        /// Sets IsEditing to false when the ListViewItem that contains an
        /// GenericAdornable changes its size
        /// </summary>
        private void OnCouldSwitchToNormalMode(object sender, RoutedEventArgs e)
        {
            //Debug.WriteLine("GenericAdornable: OnCouldSwitchToNormalMode called");
            IsEditing = false;
        }

        #endregion

        #region IDataErrorInfo Members

        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public string this[string columnName]
        {
            get 
            {
                String error = null;
                if (columnName.Equals(ValueProperty.Name))
                {
                    if (null != ValidateValueMethod)
                        error = ValidateValueMethod(Value);
                }
                
                return error;
            }
        }

        #endregion
    }

}

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows;
using System.Diagnostics;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Data;

namespace Fuzzware.Fuzzsaw.Common
{
    /// <summary>
    /// An adorner class that provides editing capability 
    /// for an GenericAdornable control. The editable adorner resides in the 
    /// AdornerLayer. When the GenericAdornable is in editing mode, the adorner is given a size 
    /// it with desired size; otherwise, arrange it with size(0,0,0,0).
    /// </summary>
    public class GenericAdorner : Adorner
    {
        #region Private Variables
        // Visual children
        private VisualCollection _visualChildren;
        // The TextBox that this adorner covers.
        private FrameworkElement m_oFrwkElement;
        // Whether the EditBox is in editing mode which means the Adorner 
        // is visible.
        private bool _isVisible;
        // Canvas that contains the TextBox that provides the ability for it to 
        // display larger than the current size of the cell so that the entire
        // contents of the cell can be edited
        private Canvas _canvas;

        //Extra padding for the content when it is displayed in the TextBox
        private const double _extraWidth = 0;
        #endregion

        #region Attached Dependency Properties

        #region ProxyProperty
        /// <summary>
        /// The 'ProxyProperty' attached Dependency Property allows any UIElement to have a proxy property
        /// </summary>
        public static DependencyProperty ProxyPropertyProperty = DependencyProperty.RegisterAttached("ProxyProperty", typeof(object), typeof(GenericAdorner));
        public static object GetProxyProperty(DependencyObject target)
        {
            if (null == target)
                return null;
            return target.GetValue(ProxyPropertyProperty);
        }

        public static void SetIsSelected(DependencyObject target, object value)
        {
            if (null == target)
                return;
            target.SetValue(ProxyPropertyProperty, value);
        }
        #endregion

        #endregion

        /// <summary>
        /// Inialize the EditBoxAdorner.
        /// </summary>
        public GenericAdorner(UIElement adornedElement, GenericAdornable oGenericAdornable)
            : base(adornedElement)
        {
            m_oFrwkElement = (FrameworkElement)oGenericAdornable.AdornerFrwkElementType.GetConstructor(new Type[0]).Invoke(null);
            
            _visualChildren = new VisualCollection(this);

            BuildFrameworkElement(oGenericAdornable, oGenericAdornable.AdornerContentControlStyle);
        }

        /// <summary>
        /// The FrameworkElement displayed by this Adorner
        /// </summary>
        public FrameworkElement FrwkElement
        {
            get { return m_oFrwkElement; }
        }


        #region Public Methods

        /// <summary>
        /// Specifies whether a FrwkElement is visible 
        /// when the IsEditing property changes.
        /// </summary>
        /// <param name="isVisible"></param>
        public void UpdateVisibilty(bool isVisible)
        {
            _isVisible = isVisible;
            InvalidateMeasure();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Override to measure elements.
        /// </summary>
        protected override Size MeasureOverride(Size constraint)
        {
            //Debug.WriteLine("GenericAdorner: MeasureOverride called, constraint=" + constraint.ToString() + ", _isVisible=" + _isVisible);
            m_oFrwkElement.IsEnabled = _isVisible;
            //if in editing mode, measure the space the adorner element 
            //should cover.
            if (_isVisible)
            {
                AdornedElement.Measure(constraint);
                m_oFrwkElement.Measure(constraint);
                //m_oFrwkElement.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));

                //since the adorner is to cover the EditBox, it should return 
                //the AdornedElement.Width, the extra 15 is to make it more 
                //clear.
                //double dHeight = (AdornedElement.DesiredSize.Height > m_oFrwkElement.DesiredSize.Height) ? AdornedElement.DesiredSize.Height : m_oFrwkElement.DesiredSize.Height;
                //double dWidth = (AdornedElement.DesiredSize.Width > m_oFrwkElement.DesiredSize.Width) ? AdornedElement.DesiredSize.Width : m_oFrwkElement.DesiredSize.Width;
                //Debug.WriteLine("MeasureOverride, dWidth=" + dWidth + ", dHeight=" + dHeight);
                //return new Size(dWidth, dHeight);
                //Debug.WriteLine("MeasureOverride, AdornedElement.DesiredSize.Width=" + AdornedElement.DesiredSize.Width + ", m_oFrwkElement.DesiredSize.Height=" + m_oFrwkElement.DesiredSize.Height);
                //return new Size(AdornedElement.DesiredSize.Width + _extraWidth,
                //                m_oFrwkElement.DesiredSize.Height);
                return new Size(((FrameworkElement)AdornedElement).ActualWidth + _extraWidth,
                                m_oFrwkElement.DesiredSize.Height);
            }
            else  //if it is not in editable mode, no need to show anything.
                return new Size(0, 0);
        }

        /// <summary>
        /// Override function to arrange elements.
        /// </summary>
        protected override Size ArrangeOverride(Size finalSize)
        {
            //Debug.WriteLine("GenericAdorner: ArrangeOverride called, finalSize=" + finalSize.ToString() + ", _isVisible=" + _isVisible);
            if (_isVisible)
            {
                m_oFrwkElement.Arrange(new Rect(0, 0, finalSize.Width,
                                                finalSize.Height));
            }
            else // if is not is editable mode, no need to show elements.
            {
                m_oFrwkElement.Arrange(new Rect(0, 0, 0, 0));
            }
            return finalSize;
        }

        /// <summary>
        /// Override property to return infomation about visual tree.
        /// </summary>
        protected override int VisualChildrenCount
        {
            get { return _visualChildren.Count; }
        }

        /// <summary>
        /// Override function to return infomation about visual tree.
        /// </summary>
        protected override Visual GetVisualChild(int index)
        {
            return _visualChildren[index]; 
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Create the ContentControl and assign its Style and DataConext and add it to the visual collection
        /// </summary>
        private void BuildFrameworkElement(GenericAdornable oGenericAdornable, Style oAdornerContentControlStyle)
        {
            m_oFrwkElement.Style = oAdornerContentControlStyle;
            Binding oDataContextBinding = new Binding();
            oDataContextBinding.Source = oGenericAdornable;
            oDataContextBinding.Path = new PropertyPath("AdornerDataContext");
            m_oFrwkElement.SetBinding(FrameworkElement.DataContextProperty, oDataContextBinding);

            _canvas = new Canvas();
            _canvas.Children.Add(m_oFrwkElement);
            _visualChildren.Add(_canvas);

            // when layout finishes.
            m_oFrwkElement.LayoutUpdated += new EventHandler(OnLayoutUpdated);
        }

        /// <summary>
        /// When Layout finish, if in editable mode, update focus status 
        /// on Framework Element.
        /// </summary>
        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            if (_isVisible)
                m_oFrwkElement.Focus();
        }

        #endregion
    }

}

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

namespace Fuzzware.Fuzzsaw.Input
{
    /// <summary>
    /// An adorner class that contains a TextBox to provide editing capability 
    /// for an EditBox control. The editable TextBox resides in the 
    /// AdornerLayer. When the EditBox is in editing mode, the TextBox is given a size 
    /// it with desired size; otherwise, arrange it with size(0,0,0,0).
    /// </summary>
    internal sealed class GenericAdorner : Adorner
    {
        #region Private Variables
        //Visual children
        private VisualCollection _visualChildren;
        //The TextBox that this adorner covers.
        private FrameworkElement m_oFrwkElement;
        //Whether the EditBox is in editing mode which means the Adorner 
        //is visible.
        private bool _isVisible;
        //Canvas that contains the TextBox that provides the ability for it to 
        //display larger than the current size of the cell so that the entire
        //contents of the cell can be edited
        private Canvas _canvas;

        //Extra padding for the content when it is displayed in the TextBox
        private const double _extraWidth = 15;
        #endregion

        /// <summary>
        /// Inialize the EditBoxAdorner.
        /// </summary>
        public GenericAdorner(UIElement adornedElement, String AdornedElementPath,
                              FrameworkElement adorningElement, DependencyProperty AdorningElementProperty, IValueConverter ValueConverter)
            : base(adornedElement)
        {
            m_oFrwkElement = adorningElement;
            
            _visualChildren = new VisualCollection(this);

            BuildFrameworkElement(AdornedElementPath, AdorningElementProperty, ValueConverter);
        }

        #region Public Methods

        /// <summary>
        /// Specifies whether a TextBox is visible 
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
            //Debug.WriteLine("MeasureOverride called, constraint=" + constraint.ToString() + ", _isVisible=" + _isVisible);
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
                return new Size(AdornedElement.DesiredSize.Width + _extraWidth,
                                m_oFrwkElement.DesiredSize.Height);
            }
            else  //if it is not in editable mode, no need to show anything.
                return new Size(0, 0);
        }

        /// <summary>
        /// override function to arrange elements.
        /// </summary>
        protected override Size ArrangeOverride(Size finalSize)
        {
            //Debug.WriteLine("ArrangeOverride called, finalSize=" + finalSize.ToString() + ", _isVisible=" + _isVisible);
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
        /// override property to return infomation about visual tree.
        /// </summary>
        protected override int VisualChildrenCount
        {
            get { return _visualChildren.Count; }
        }

        /// <summary>
        /// override function to return infomation about visual tree.
        /// </summary>
        protected override Visual GetVisualChild(int index)
        {
            return _visualChildren[index]; 
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Inialize necessary properties and hook necessary events on TextBox, 
        /// then add it into tree.
        /// </summary>
        private void BuildFrameworkElement(String AdornedElementPath, DependencyProperty AdorningElementProperty, IValueConverter ValueConverter)
        {
            _canvas = new Canvas();
            _canvas.Children.Add(m_oFrwkElement);
            _visualChildren.Add(_canvas);

            if (null != AdorningElementProperty)
            {
                //Using a Path bind a property of the AdornedElement to a DepenedencyProprty of the AdorningElement.
                Binding binding = new Binding(AdornedElementPath);
                binding.Mode = BindingMode.TwoWay;
                binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                binding.Source = this.AdornedElement;
                binding.Converter = ValueConverter;

                m_oFrwkElement.SetBinding(AdorningElementProperty, binding);
            }
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

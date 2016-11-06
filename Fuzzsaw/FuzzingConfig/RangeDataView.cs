using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Fuzzware.Fuzzsaw.Common;

namespace Fuzzware.Fuzzsaw.FuzzingConfig
{
    public class RangeDataView : DependencyObject
    {
        public static DependencyProperty StartPositionProperty;
        public static DependencyProperty StepSizeProperty;
        public static DependencyProperty LengthProperty;

        static RangeDataView()
        {
            StartPositionProperty = DependencyProperty.Register("StartPosition", typeof(ObservableString), typeof(RangeDataView));
            StepSizeProperty = DependencyProperty.Register("StepSize", typeof(ObservableString), typeof(RangeDataView));
            LengthProperty = DependencyProperty.Register("Length", typeof(ObservableString), typeof(RangeDataView));
        }

        public RangeDataView()
        {
            StartPosition = new ObservableString("");
            StepSize = new ObservableString("");
            Length = new ObservableString("");
        }

        public ObservableString StartPosition
        {
            get { return (ObservableString)GetValue(StartPositionProperty); }
            set { SetValue(StartPositionProperty, value); }
        }

        public ObservableString StepSize
        {
            get { return (ObservableString)GetValue(StepSizeProperty); }
            set { SetValue(StepSizeProperty, value); }
        }

        public ObservableString Length
        {
            get { return (ObservableString)GetValue(LengthProperty); }
            set { SetValue(LengthProperty, value); }
        }

        public bool IsEmpty
        {
            get
            {
                if (String.IsNullOrEmpty(StartPosition.Value) &&
                    String.IsNullOrEmpty(StepSize.Value) &&
                    String.IsNullOrEmpty(Length.Value))
                    return true;
                return false;
            }
        }
    }
}

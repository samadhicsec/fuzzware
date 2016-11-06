using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using Fuzzware.Fuzzsaw.Common;

namespace Fuzzware.Fuzzsaw.FuzzingConfig.ViewModel
{
    public class RangeDataViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Dependency Properties

        static readonly DependencyProperty StartPositionProperty = DependencyProperty.Register("StartPosition", typeof(String), typeof(RangeDataViewModel));
        /// <summary>
        /// The start position for the range
        /// </summary>
        public String StartPosition
        {
            get { return (String)GetValue(StartPositionProperty); }
            set { SetValue(StartPositionProperty, value); }
        }

        static readonly DependencyProperty StepSizeProperty = DependencyProperty.Register("StepSize", typeof(String), typeof(RangeDataViewModel));
        /// <summary>
        /// The step size for the range
        /// </summary>
        public String StepSize
        {
            get { return (String)GetValue(StepSizeProperty); }
            set { SetValue(StepSizeProperty, value); }
        }

        static readonly DependencyProperty LengthProperty = DependencyProperty.Register("Length", typeof(String), typeof(RangeDataViewModel));
        /// <summary>
        /// The length for the range
        /// </summary>
        public String Length
        {
            get { return (String)GetValue(LengthProperty); }
            set { SetValue(LengthProperty, value); }
        }

        #endregion

        public RangeDataViewModel()
        {
            StartPosition = "";
            StepSize = "";
            Length = "";
        }

        public void Reset()
        {
            StartPosition = "";
            StepSize = "";
            Length = "";
        }

        public bool IsEmpty
        {
            get
            {
                if (String.IsNullOrEmpty(StartPosition) &&
                    String.IsNullOrEmpty(StepSize) &&
                    String.IsNullOrEmpty(Length))
                    return true;
                return false;
            }
        }

        #region IDataErrorInfo Members

        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public string this[string columnName]
        {
            get 
            {
                string error = null;
                ulong val = 0;
                if (columnName.Equals(StartPositionProperty.Name))
                {
                    if (!String.IsNullOrEmpty(StartPosition) && !UInt64.TryParse(StartPosition, out val))
                        error = "Start Position must be an integer greater than or equal to zero";
                }
                else if (columnName.Equals(StepSizeProperty.Name))
                {
                    if (!String.IsNullOrEmpty(StepSize))
                    {
                        if (!UInt64.TryParse(StepSize, out val))
                            error = "Step Size must be an integer greater than zero";
                        else
                            if (val == 0)
                                error = "StepSize must be greater than 0";
                    }
                }
                else if (columnName.Equals(LengthProperty.Name))
                {
                    if (!String.IsNullOrEmpty(Length))
                    {
                        if (!UInt64.TryParse(Length, out val))
                            error = "Length must be an integer greater than zero";
                        else
                            if (val == 0)
                                error = "Length must be greater than 0";
                    }
                }
                return error;
            }
        }

        #endregion
    }
}

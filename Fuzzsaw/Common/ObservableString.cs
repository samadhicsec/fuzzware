using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;

namespace Fuzzware.Fuzzsaw.Common
{
    /// <summary>
    /// For use in a collection that will be the source of a binding.  Ensures when the string gets updated so does the binding source.
    /// </summary>
    public class ObservableString : DependencyObject
    {
        static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(String), typeof(ObservableString));

        public ObservableString(String InitialValue)
        {
            Value = InitialValue;
        }

        public String Value
        {
            get { return (String)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        //public static implicit operator string(ObservableString o)
        //{
        //    return o.ToString();
        //}

        public override string ToString()
        {
            return Value;
        }
    }
}

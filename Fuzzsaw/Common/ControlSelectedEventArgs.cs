using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Fuzzware.Fuzzsaw.Common
{
    public class ControlSelectedEventArgs : EventArgs
    {
        public UIElement ControlSelected;

        public ControlSelectedEventArgs(UIElement ControlSelected)
        {
            this.ControlSelected = ControlSelected;
        }
    }
}

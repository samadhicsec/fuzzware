using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Fuzzware.Fuzzsaw.Common.ViewModel
{
    public class OnOffControlViewModel : ViewModelBase
    {
        #region Dependency Properties
        static readonly DependencyProperty OnProperty = DependencyProperty.Register("On", typeof(bool), typeof(OnOffControlViewModel));
        /// <summary>
        /// Whether it is on or off
        /// </summary>
        public bool On
        {
            get { return (bool)GetValue(OnProperty); }
            set { SetValue(OnProperty, value); }
        }
        #endregion
    }
}

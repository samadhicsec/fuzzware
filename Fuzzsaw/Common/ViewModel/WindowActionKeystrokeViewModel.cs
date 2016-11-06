using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using Fuzzware.Fuzzsaw.Common;

namespace Fuzzware.Fuzzsaw.Common.ViewModel
{
    public class WindowActionKeystrokeViewModel : ViewModelBase
    {
        #region Dependency Properties

        static readonly DependencyProperty KeysProperty = DependencyProperty.Register("Keys", typeof(string), typeof(WindowActionKeystrokeViewModel));
        public string Keys
        {
            get { return (string)GetValue(KeysProperty); }
            set { SetValue(KeysProperty, value); }
        }

        static readonly DependencyProperty DeleteProperty = DependencyProperty.Register("Delete", typeof(ICommand), typeof(WindowActionKeystrokeViewModel));
        public ICommand Delete
        {
            get { return (ICommand)GetValue(DeleteProperty); }
            set { SetValue(DeleteProperty, value); }
        }

        #endregion
    }
}

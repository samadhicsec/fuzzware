using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Fuzzware.Fuzzsaw.Input.View
{
    /// <summary>
    /// Interaction logic for XMLInputView.xaml
    /// </summary>
    public partial class XMLInputView : UserControl
    {
        #region Dependency Properties

        static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(string), typeof(XMLInputView));
        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        #endregion

        public XMLInputView()
        {
            InitializeComponent();
        }
    }
}

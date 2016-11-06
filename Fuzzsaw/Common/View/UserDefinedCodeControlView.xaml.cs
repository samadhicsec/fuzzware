using System;
using System.Collections.Generic;
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

namespace Fuzzware.Fuzzsaw.Common.View
{
    /// <summary>
    /// Interaction logic for UserDefinedCodeControlView.xaml
    /// </summary>
    public partial class UserDefinedCodeControlView : UserControl
    {
        static readonly DependencyProperty InterfaceNameProperty = DependencyProperty.Register("InterfaceName", typeof(string), typeof(UserDefinedCodeControlView));
        /// <summary>
        /// The interface this user defined code should implement
        /// </summary>
        public String InterfaceName
        {
            get { return (String)GetValue(InterfaceNameProperty); }
            set { SetValue(InterfaceNameProperty, value); }
        }

        public UserDefinedCodeControlView()
        {
            InitializeComponent();
        }
    }
}

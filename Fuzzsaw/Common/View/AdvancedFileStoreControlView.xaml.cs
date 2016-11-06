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
    /// Interaction logic for AdvancedFileStoreControlView.xaml
    /// </summary>
    public partial class AdvancedFileStoreControlView : UserControl
    {
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(AdvancedFileStoreControlView));
        /// <summary>
        /// The description to display
        /// </summary>
        public string Description
        {
            get { return (String)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public AdvancedFileStoreControlView()
        {
            InitializeComponent();
        }
    }
}

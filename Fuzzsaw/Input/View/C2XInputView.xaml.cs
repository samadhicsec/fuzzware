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
using Fuzzware.Fuzzsaw.Common;

namespace Fuzzware.Fuzzsaw.Input.View
{
    /// <summary>
    /// Interaction logic for C2XInput.xaml
    /// </summary>
    public partial class C2XInputView : UserControl
    {
        #region Dependency Properties

        static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(string), typeof(C2XInputView));
        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        #endregion

        public C2XInputView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Call the attached Common.Command
        /// </summary>
        private void ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            ComboBox oComboBox = sender as ComboBox;
            if (null == oComboBox)
                return;

            ICommand oCommand = oComboBox.GetValue(Common.Common.CommandProperty) as ICommand;
            if (null == oCommand)
                return;

            oCommand.Execute(null);
        }

        /// <summary>
        /// Invoke TestConversionCommand passing it a new TestC2XWindow window
        /// </summary>
        private void TestConversion_Click(object sender, RoutedEventArgs e)
        {
            ICommand TestConversionCommand = Common.Common.GetPropertyFromDataContext<ICommand>(DataContext, "TestConversionCommand");
            if (null != TestConversionCommand)
                TestConversionCommand.Execute(new TestC2XWindow());
        }
    }

    /// <summary>
    /// Template Selector for Node names or error message
    /// </summary>
    public class NodesDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement ResourceSource = Application.Current.MainWindow;
            if (container is FrameworkElement)
                ResourceSource = container as FrameworkElement;

            DataTemplate oDataTemplate = null;
            if (item is string)
            {
                oDataTemplate = ResourceSource.FindResource("ErrorNode") as DataTemplate;
            }
            else if (item is System.Xml.XmlQualifiedName)
            {
                oDataTemplate = ResourceSource.FindResource("XmlQualifiedNameNode") as DataTemplate;
            }
            return oDataTemplate;
        }
    }
}

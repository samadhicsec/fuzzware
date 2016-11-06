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
using System.Windows.Shapes;
using System.Reflection;

namespace Fuzzware.Fuzzsaw.Input.View
{
    /// <summary>
    /// Interaction logic for TestC2XWindow.xaml
    /// </summary>
    public partial class TestC2XWindow : Window
    {
        public TestC2XWindow()
        {
            InitializeComponent();

            tbConversionOutput.TextChanged += new TextChangedEventHandler(tbConversionOutput_TextChanged);
        }

        void tbConversionOutput_TextChanged(object sender, TextChangedEventArgs e)
        {
            tbConversionOutput.ScrollToEnd();
        }

        /// <summary>
        /// Do our own reflection to get the desired type T
        /// </summary>
        private T GetCommandFromDataContext<T>(String CommandName)
        {
            if (null == DataContext)
                return default(T);

            foreach (PropertyInfo pi in DataContext.GetType().GetProperties())
            {
                if (pi.Name.Equals(CommandName) && (pi.PropertyType == typeof(T)))
                    return (T)pi.GetValue(DataContext, null);
            }
            return default(T);
        }

        private void wC2XWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ICommand FinishTest = Common.Common.GetPropertyFromDataContext<ICommand>(DataContext, "FinishTestCommand");
            if (null != FinishTest)
                FinishTest.Execute(null);
        }
    }

    /// <summary>
    /// Set the height of the C2X conversion output textbox
    /// </summary>
    [ValueConversion(typeof(double), typeof(double))]
    public class C2XConversionOuputTextBoxHeight : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (null == value)
                return Binding.DoNothing;

            double ActualHeight = (double)value;

            if(0 == ActualHeight)
                return Binding.DoNothing;

            return ActualHeight - 15;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}

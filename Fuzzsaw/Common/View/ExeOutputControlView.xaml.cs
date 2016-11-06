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
using Fuzzware.Fuzzsaw.Common;

namespace Fuzzware.Fuzzsaw.Common.View
{
    /// <summary>
    /// Interaction logic for ExeOutputControlView.xaml
    /// </summary>
    public partial class ExeOutputControlView : UserControl
    {
        public ExeOutputControlView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// When the mouse is in the Border and the users presses a key
        /// </summary>
        private void WindowActionKeystroke(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.System)
                return;
            // Since the Modifier key are detected as key presses, we ignore these
            switch (e.Key)
            {
                case Key.LeftAlt:
                case Key.LeftCtrl:
                case Key.LeftShift:
                case Key.RightAlt:
                case Key.RightCtrl:
                case Key.RightShift:
                    return;
            }

            Border oBorder = sender as Border;
            TextBox oTextBox = e.OriginalSource as TextBox;
            // Make sure the Border initiated this event and not a TextBox
            if ((null != oBorder) && (oBorder.IsMouseOver) && (oTextBox == null))
            {
                ICommand AddCommand = (ICommand)oBorder.GetValue(Common.CommandProperty);
                if (null != AddCommand)
                    AddCommand.Execute(e);
            }
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border)
                Keyboard.Focus(sender as Border);
        }
    }
}

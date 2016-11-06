﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using Fuzzware.Fuzzsaw.Common;
using Fuzzware.Schemas.AutoGenerated;

namespace Fuzzware.Fuzzsaw.Common.ViewModel
{
    public class WindowActionsViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Dependency Properties

        static readonly DependencyProperty WindowTitleProperty = DependencyProperty.Register("WindowTitle", typeof(string), typeof(WindowActionsViewModel));
        /// <summary>
        /// The title of the window that is the target of the actions
        /// </summary>
        public string WindowTitle
        {
            get { return (string)GetValue(WindowTitleProperty); }
            set { SetValue(WindowTitleProperty, value); }
        }

        static readonly DependencyProperty KeystrokesProperty = DependencyProperty.Register("Keystrokes", typeof(ObservableCollection<WindowActionKeystrokeViewModel>), typeof(WindowActionsViewModel));
        /// <summary>
        /// A collection of Keystrokes
        /// </summary>
        public ObservableCollection<WindowActionKeystrokeViewModel> Keystrokes
        {
            get { return (ObservableCollection<WindowActionKeystrokeViewModel>)GetValue(KeystrokesProperty); }
            set { SetValue(KeystrokesProperty, value); }
        }

        static readonly DependencyProperty DeleteProperty = DependencyProperty.Register("Delete", typeof(ICommand), typeof(WindowActionsViewModel));
        /// <summary>
        /// The Command to delete a Window Action
        /// </summary>
        public ICommand Delete
        {
            get { return (ICommand)GetValue(DeleteProperty); }
            set { SetValue(DeleteProperty, value); }
        }

        #endregion

        #region Commands
        
        #region DeleteKeystroke
        RelayCommand<WindowActionKeystrokeViewModel> m_oDeleteKeystrokeCommand;

        public ICommand DeleteKeystrokeCommand
        {
            get
            {
                if (null == m_oDeleteKeystrokeCommand)
                    m_oDeleteKeystrokeCommand = new RelayCommand<WindowActionKeystrokeViewModel>(DeleteKeystrokeExecute);
                return m_oDeleteKeystrokeCommand;
            }
        }

        public void DeleteKeystrokeExecute(WindowActionKeystrokeViewModel oKeystroke)
        {
            Keystrokes.Remove(oKeystroke);
        }
        #endregion

        #region AddKeystroke
        RelayCommand<KeyEventArgs> m_oAddKeystrokeCommand;

        public ICommand AddKeystrokeCommand
        {
            get
            {
                if (null == m_oAddKeystrokeCommand)
                    m_oAddKeystrokeCommand = new RelayCommand<KeyEventArgs>(AddKeystrokeExecute);
                return m_oAddKeystrokeCommand;
            }
        }

        public void AddKeystrokeExecute(KeyEventArgs oKeystroke)
        {
            if (0 == Keystrokes.Count)
                Keystrokes.Add(KeystrokeCreator(""));

            // Get the last stored keystroke
            WindowActionKeystrokeViewModel oLastKeyStroke = Keystrokes[Keystrokes.Count - 1];
            bool bLastKeyIsSpecial = false;
            if (-1 != oLastKeyStroke.Keys.IndexOfAny(new char[] { '{', '^', '%' }))
                bLastKeyIsSpecial = true;
            // Get the current keystroke
            bool bNewKeyIsSpecial = false;
            String NewKey = KeystrokeToAdd(oKeystroke, out bNewKeyIsSpecial);
            if (null == NewKey)
                return;

            if (!bLastKeyIsSpecial && !bNewKeyIsSpecial)
            {
                oLastKeyStroke.Keys = oLastKeyStroke.Keys + NewKey;
                return;
            }
            else
            {
                // Add this Key
                if (String.IsNullOrEmpty(oLastKeyStroke.Keys))
                    Keystrokes[Keystrokes.Count - 1].Keys = NewKey;
                else
                    Keystrokes.Add(KeystrokeCreator(NewKey));
            }
        }

        private String KeystrokeToAdd(KeyEventArgs e, out bool bIsSpecial)
        {
            KeyConverter oKeyConverter = new KeyConverter();
            bIsSpecial = false;

            // Get Modifier
            String Modifier = "";
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Shift)
                Modifier += "+";
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                Modifier += "^";
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Alt)
                Modifier += "%";
            //if (!String.IsNullOrEmpty(Modifier))
            //    bIsSpecial = true;

            String KeyStr = null;
            // Conversion to something System.Windows.Forms.SendKeys can understand
            switch (e.Key)
            {
                case Key.Back:
                    KeyStr = "BACKSPACE";
                    break;
                case Key.Cancel:
                    if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                        KeyStr = "BREAK";
                    break;
                case Key.Delete:
                    KeyStr = "DELETE";
                    break;
                case Key.End:
                    KeyStr = "END";
                    break;
                case Key.Return:
                    KeyStr = "ENTER";
                    break;
                case Key.Escape:
                    KeyStr = "ESC";
                    break;
                case Key.Help:
                    KeyStr = "HELP";
                    break;
                case Key.Home:
                    KeyStr = "HOME";
                    break;
                case Key.Insert:
                    KeyStr = "INSERT";
                    break;
                case Key.Left:
                    KeyStr = "LEFT";
                    break;
                case Key.NumLock:
                    KeyStr = "NUMLOCK";
                    break;
                case Key.PageDown:
                    KeyStr = "PGDN";
                    break;
                case Key.PageUp:
                    KeyStr = "PGUP";
                    break;
                case Key.Right:
                    KeyStr = "RIGHT";
                    break;
                case Key.Scroll:
                    KeyStr = "SCROLLLOCK";
                    break;
                case Key.Tab:
                    KeyStr = "TAB";
                    break;
                case Key.Up:
                    KeyStr = "UP";
                    break;
                case Key.F1:
                    KeyStr = "F1";
                    break;
                case Key.F2:
                    KeyStr = "F2";
                    break;
                case Key.F3:
                    KeyStr = "F3";
                    break;
                case Key.F4:
                    KeyStr = "F4";
                    break;
                case Key.F5:
                    KeyStr = "F5";
                    break;
                case Key.F6:
                    KeyStr = "F6";
                    break;
                case Key.F7:
                    KeyStr = "F7";
                    break;
                case Key.F8:
                    KeyStr = "F8";
                    break;
                case Key.F9:
                    KeyStr = "F9";
                    break;
                case Key.F10:
                    KeyStr = "F10";
                    break;
                case Key.F11:
                    KeyStr = "F11";
                    break;
                case Key.F12:
                    KeyStr = "F12";
                    break;
                case Key.Add:
                    KeyStr = "ADD";
                    break;
                case Key.Subtract:
                    KeyStr = "SUBTRACT";
                    break;
                case Key.Multiply:
                    KeyStr = "MULTIPLY";
                    break;
                case Key.Divide:
                    KeyStr = "DIVIDE";
                    break;
            }
            if (null != KeyStr)
            {
                bIsSpecial = true;
                KeyStr = "{" + KeyStr + "}";
            }
            else
            {
                // At this stage we have already converted all the special characters that SendKeys understand.  
                //Change what other characters we can
                switch (e.Key)
                {
                    case Key.Space:
                        KeyStr = " ";
                        break;
                    case Key.OemComma:
                        KeyStr = ",";
                        break;
                    case Key.OemMinus:
                        KeyStr = "-";
                        break;
                    case Key.OemOpenBrackets:
                        KeyStr = "(";
                        break;
                    case Key.OemPeriod:
                        KeyStr = ".";
                        break;
                    case Key.OemPipe:
                        KeyStr = "|";
                        break;
                    case Key.OemPlus:
                        KeyStr = "{+}";
                        break;
                    case Key.OemQuestion:
                        KeyStr = "?";
                        break;
                    case Key.OemQuotes:
                        KeyStr = "'";
                        break;
                    case Key.OemSemicolon:
                        KeyStr = ";";
                        break;
                    case Key.OemTilde:
                        KeyStr = "~";
                        break;
                }
                if (null == KeyStr)
                {
                    KeyStr = oKeyConverter.ConvertToString(e.Key);
                    KeyStr = KeyStr.ToUpper();

                    //Now we only accept characters that are 1 in length
                    if (KeyStr.Length > 1)
                    {
                        return null;
                    }

                    // All chars are in uppercase.  If it could make a difference, check if Shift is pressed.
                    if (!KeyStr.ToLower().Equals(KeyStr))
                    {
                        // Remove the modifier
                        if (-1 != Modifier.IndexOf('+'))
                            Modifier = Modifier.Remove(Modifier.IndexOf('+'), 1);
                        // If shift is not down, make lowercase
                        if(e.KeyboardDevice.Modifiers != ModifierKeys.Shift)
                            KeyStr = KeyStr.ToLower();
                    }                    
                }
            }
            
            return Modifier + KeyStr;
        }
  
        #endregion
        
        #endregion

        public WindowActionsViewModel()
        {
            Keystrokes = new ObservableCollection<WindowActionKeystrokeViewModel>();
        }

        public WindowActionsViewModel(WindowAction oWindowAction) : this()
        {
            WindowTitle = oWindowAction.WindowTitle;
            for (int i = 0; (null != oWindowAction.KeyboardStrokes) && (i < oWindowAction.KeyboardStrokes.Length); i++)
                Keystrokes.Add(KeystrokeCreator(oWindowAction.KeyboardStrokes[i]));
        }

        private WindowActionKeystrokeViewModel KeystrokeCreator(String strKeystroke)
        {
            WindowActionKeystrokeViewModel Keys = new WindowActionKeystrokeViewModel();
            Keys.Keys = strKeystroke;
            Keys.Delete = DeleteKeystrokeCommand;
            return Keys;
        }

        #region IDataErrorInfo Members

        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public string this[string columnName]
        {
            get 
            { 
                if(columnName.Equals("WindowTitle"))
                {
                    if (String.IsNullOrEmpty(WindowTitle))
                        return "Window Title cannot be empty";
                }
                return null;
            }
        }

        #endregion
    }
}

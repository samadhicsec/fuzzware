using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Input;
using Fuzzware.Fuzzsaw.Common;

namespace Fuzzware.Fuzzsaw.Execution.ViewModel
{
    public class OnTerminateActionsViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Dependency Properties

        static readonly DependencyProperty ProcessProperty = DependencyProperty.Register("Process", typeof(string), typeof(OnTerminateActionsViewModel));
        /// <summary>
        /// The process (id or name) to monitor for termination
        /// </summary>
        public string Process
        {
            get { return (string)GetValue(ProcessProperty); }
            set { SetValue(ProcessProperty, value); }
        }

        static readonly DependencyProperty ActionsProperty = DependencyProperty.Register("Actions", typeof(ObservableCollection<ObservableString>), typeof(OnTerminateActionsViewModel));
        /// <summary>
        /// The Actions state
        /// </summary>
        public ObservableCollection<ObservableString> Actions
        {
            get { return (ObservableCollection<ObservableString>)GetValue(ActionsProperty); }
            set { SetValue(ActionsProperty, value); }
        }

        static readonly DependencyProperty PauseFuzzingProperty = DependencyProperty.Register("PauseFuzzing", typeof(bool), typeof(OnTerminateActionsViewModel));
        /// <summary>
        /// Whether or not to pause fuzzing on process termination
        /// </summary>
        public bool PauseFuzzing
        {
            get { return (bool)GetValue(PauseFuzzingProperty); }
            set { SetValue(PauseFuzzingProperty, value); }
        }

        static readonly DependencyProperty ResumeFuzzingProperty = DependencyProperty.Register("ResumeFuzzing", typeof(bool), typeof(OnTerminateActionsViewModel));
        /// <summary>
        /// Whether or not to Resume fuzzing on process termination
        /// </summary>
        public bool ResumeFuzzing
        {
            get { return (bool)GetValue(ResumeFuzzingProperty); }
            set { SetValue(ResumeFuzzingProperty, value); }
        }

        #endregion

        #region Commands

        #region AddAction
        RelayCommand m_oAddActionCommand;

        /// <summary>
        /// Add a Action
        /// </summary>
        public ICommand AddActionCommand
        {
            get
            {
                if (null == m_oAddActionCommand)
                    m_oAddActionCommand = new RelayCommand(AddActionExecute);
                return m_oAddActionCommand;
            }
        }

        public void AddActionExecute()
        {
            Actions.Add(new ObservableString(""));
        }
        #endregion

        #region RemoveAction
        RelayCommand<ObservableString> m_oRemoveActionCommand;

        /// <summary>
        /// Remove a Action from the list
        /// </summary>
        public ICommand RemoveActionCommand
        {
            get
            {
                if (null == m_oRemoveActionCommand)
                    m_oRemoveActionCommand = new RelayCommand<ObservableString>(RemoveActionExecute);
                return m_oRemoveActionCommand;
            }
        }

        public void RemoveActionExecute(ObservableString Action)
        {
            Actions.Remove(Action);
        }
        #endregion

        #endregion

        public OnTerminateActionsViewModel()
        {
            Actions = new ObservableCollection<ObservableString>();
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
                String error = null;
                if (columnName.Equals(ProcessProperty.Name))
                {
                    if (String.IsNullOrEmpty(Process))
                        error = "The process ID or name should not be empty";
                }
                return error;
            }
        }

        #endregion
    }
}

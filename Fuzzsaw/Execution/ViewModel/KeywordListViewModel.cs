using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Fuzzware.Fuzzsaw.Common;

namespace Fuzzware.Fuzzsaw.Execution.ViewModel
{
    public class KeywordListViewModel : ViewModelBase
    {
        #region Dependency Properties

        static readonly DependencyProperty KeywordsProperty = DependencyProperty.Register("Keywords", typeof(ObservableCollection<ObservableString>), typeof(KeywordListViewModel));
        /// <summary>
        /// The Keywords state
        /// </summary>
        public ObservableCollection<ObservableString> Keywords
        {
            get { return (ObservableCollection<ObservableString>)GetValue(KeywordsProperty); }
            set { SetValue(KeywordsProperty, value); }
        }

        #endregion

        #region Commands

        #region AddKeyword
        RelayCommand m_oAddKeywordCommand;

        /// <summary>
        /// Add a keyword
        /// </summary>
        public ICommand AddKeywordCommand
        {
            get
            {
                if (null == m_oAddKeywordCommand)
                    m_oAddKeywordCommand = new RelayCommand(AddKeywordExecute);
                return m_oAddKeywordCommand;
            }
        }

        public void AddKeywordExecute()
        {
            Keywords.Add(new ObservableString(""));
        }
        #endregion

        #region RemoveKeyword
        RelayCommand<ObservableString> m_oRemoveKeywordCommand;

        /// <summary>
        /// Remove a keyword from the list
        /// </summary>
        public ICommand RemoveKeywordCommand
        {
            get
            {
                if (null == m_oRemoveKeywordCommand)
                    m_oRemoveKeywordCommand = new RelayCommand<ObservableString>(RemoveKeywordExecute);
                return m_oRemoveKeywordCommand;
            }
        }

        public void RemoveKeywordExecute(ObservableString Keyword)
        {
            Keywords.Remove(Keyword);
        }
        #endregion

        #endregion

        public KeywordListViewModel()
        {
            Keywords = new ObservableCollection<ObservableString>();
        }
    }
}

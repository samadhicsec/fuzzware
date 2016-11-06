using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Fuzzware.Fuzzsaw.Common;

namespace Fuzzware.Fuzzsaw.Common.ViewModel
{
    public class FilesControlViewModel : BaseFileViewModel
    {
        #region Dependency Properties
        public static readonly DependencyProperty FilenamesProperty = DependencyProperty.Register("Filenames", typeof(ObservableCollection<string>), typeof(FileControlViewModel));
        /// <summary>
        /// The selected file
        /// </summary>
        public ObservableCollection<string> Filenames
        {
            get { return (ObservableCollection<string>)GetValue(FilenamesProperty); }
            set { SetValue(FilenamesProperty, value); }
        }
        #endregion

        #region Commands

        #region ChooseFiles
        RelayCommand m_oChooseFilesCommand;

        public ICommand ChooseFilesCommand
        {
            get
            {
                if (null == m_oChooseFilesCommand)
                    m_oChooseFilesCommand = new RelayCommand(ChooseFilesExecute);
                return m_oChooseFilesCommand;
            }
        }

        public void ChooseFilesExecute()
        {
            String[] oFiles = GetFilesUsingDialog(true);

            if (oFiles.Length > 0)
            {
                if (null == Filenames)
                    Filenames = new ObservableCollection<string>();
                for (int i = 0; i < oFiles.Length; i++)
                {
                    if (!Filenames.Contains(oFiles[i]))
                        Filenames.Add(oFiles[i]);
                }
            }
        }
        #endregion

        #region RemoveFiles
        RelayCommand<IList> m_oRemoveFilesCommand;

        public ICommand RemoveFilesCommand
        {
            get
            {
                if (null == m_oRemoveFilesCommand)
                    m_oRemoveFilesCommand = new RelayCommand<IList>(RemoveFilesExecute);
                return m_oRemoveFilesCommand;
            }
        }

        public void RemoveFilesExecute(IList oList)
        {
            if (oList.Count > 0)
            {
                // Make a copy of the selected items as otherwise removing objects changes the list
                object[] oSelectedItems = new object[oList.Count];
                oList.CopyTo(oSelectedItems, 0);
                for (int i = 0; i < oSelectedItems.Length; i++)
                {
                    Filenames.Remove((string)oSelectedItems[i]);
                }
            }
        }
        #endregion

        #endregion

        public FilesControlViewModel()
        {
            Filenames = new ObservableCollection<string>();
        }
    }
}

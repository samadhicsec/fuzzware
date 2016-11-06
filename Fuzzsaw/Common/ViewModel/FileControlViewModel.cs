using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Fuzzware.Fuzzsaw.Common;

namespace Fuzzware.Fuzzsaw.Common.ViewModel
{
    public class FileControlViewModel : BaseFileViewModel
    {
        #region Dependency Properties
        public static readonly DependencyProperty FilenameProperty = DependencyProperty.Register("Filename", typeof(string), typeof(FileControlViewModel));
        /// <summary>
        /// The selected file
        /// </summary>
        public string Filename
        {
            get { return (String)GetValue(FilenameProperty); }
            set { SetValue(FilenameProperty, value); }
        }
        #endregion

        #region Commands
        RelayCommand m_oChooseFileCommand;

        public ICommand ChooseFileCommand
        {
            get
            {
                if (null == m_oChooseFileCommand)
                    m_oChooseFileCommand = new RelayCommand(ChooseFileExecute);
                return m_oChooseFileCommand;
            }
        }

        public void ChooseFileExecute()
        {
            String[] oFiles = GetFilesUsingDialog(false);

            if (1 == oFiles.Length)
                Filename = oFiles[0];
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Fuzzware.Fuzzsaw.Common;

namespace Fuzzware.Fuzzsaw.Common.ViewModel
{
    /// <summary>
    /// The View Model for choosing a directory
    /// </summary>
    public class DirectoryControlViewModel : ViewModelBase
    {
        #region Dependency Properties
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(DirectoryControlViewModel));
        /// <summary>
        /// The title for the directory select dialog
        /// </summary>
        public string Title
        {
            get { return (String)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty UseRelativePathsProperty = DependencyProperty.Register("UseRelativePaths", typeof(bool), typeof(DirectoryControlViewModel));
        /// <summary>
        /// Whether or not to return relative paths or not
        /// </summary>
        public bool UseRelativePaths
        {
            get { return (bool)GetValue(UseRelativePathsProperty); }
            set { SetValue(UseRelativePathsProperty, value); }
        }

        public static readonly DependencyProperty DirectoryNameProperty = DependencyProperty.Register("DirectoryName", typeof(string), typeof(DirectoryControlViewModel));
        /// <summary>
        /// The name of the chosen directory
        /// </summary>
        public string DirectoryName
        {
            get { return (String)GetValue(DirectoryNameProperty); }
            set { SetValue(DirectoryNameProperty, value); }
        }

        #endregion

        #region Commands
        RelayCommand m_oChooseDirectoryCommand;

        public ICommand ChooseDirectoryCommand
        {
            get
            {
                if(null == m_oChooseDirectoryCommand)
                    m_oChooseDirectoryCommand = new RelayCommand(ChooseDirectoryExecute);
                return m_oChooseDirectoryCommand;
            }
        }

        public void ChooseDirectoryExecute()
        {
            String retDirName = FileAndPathHelper.GetFolderUsingDialog(Title, Fuzzsaw.ProjectsDirectory, UseRelativePaths);

            if (!String.IsNullOrEmpty(retDirName))
                DirectoryName = retDirName;
        }
        #endregion

        public DirectoryControlViewModel()
        {
            Title = "Select directory";
            UseRelativePaths = false;
        }
    }
}

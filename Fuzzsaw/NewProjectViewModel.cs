using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Fuzzware.Fuzzsaw.Common;
using Fuzzware.Fuzzsaw.Common.ViewModel;

namespace Fuzzware.Fuzzsaw
{
    public class NewProjectViewModel : ViewModelBase
    {
        #region Dependency Properties

        static readonly DependencyProperty ProjectDirectoryProperty = DependencyProperty.Register("ProjectDirectory", typeof(DirectoryControlViewModel), typeof(NewProjectViewModel));
        /// <summary>
        /// The new project directory
        /// </summary>
        public DirectoryControlViewModel ProjectDirectory
        {
            get { return (DirectoryControlViewModel)GetValue(ProjectDirectoryProperty); }
            set { SetValue(ProjectDirectoryProperty, value); }
        }

        //static readonly DependencyProperty ProjectNameProperty = DependencyProperty.Register("ProjectName", typeof(string), typeof(NewProjectViewModel));
        ///// <summary>
        ///// The new project name
        ///// </summary>
        //public string ProjectName
        //{
        //    get { return (string)GetValue(ProjectNameProperty); }
        //    set { SetValue(ProjectNameProperty, value); }
        //}

        static readonly DependencyProperty ValidatedProperty = DependencyProperty.Register("Validated", typeof(bool), typeof(NewProjectViewModel));
        /// <summary>
        /// Whether or not the new project information is valid
        /// </summary>
        public bool Validated
        {
            get { return (bool)GetValue(ValidatedProperty); }
            set { SetValue(ValidatedProperty, value); }
        }

        #endregion

        #region Commands

        #region Accept
        RelayCommand m_oAcceptCommand;

        /// <summary>
        /// Ok a New project
        /// </summary>
        public ICommand AcceptCommand
        {
            get
            {
                if (null == m_oAcceptCommand)
                    m_oAcceptCommand = new RelayCommand(AcceptExecute);
                return m_oAcceptCommand;
            }
        }

        public void AcceptExecute()
        {
            if (String.IsNullOrEmpty(ProjectDirectory.DirectoryName))
            {
                MessageBox.Show("Please specify the Project directory", "Project directory required", MessageBoxButton.OK);
                Validated = false;
                return;
            }
            if (-1 != ProjectDirectory.DirectoryName.IndexOfAny(Path.GetInvalidPathChars()))
            {
                MessageBox.Show("The Project directory contains invalid characters", "Project directory has invalid characters", MessageBoxButton.OK);
                Validated = false;
                return;
            }
            Validated = true;
        }
        #endregion

        #endregion

        public NewProjectViewModel()
        {
            ProjectDirectory = new DirectoryControlViewModel();
            ProjectDirectory.Title = "Choose the parent directory of the Project directory";
            ProjectDirectory.UseRelativePaths = false;
            ProjectDirectory.DirectoryName = Path.Combine(Fuzzsaw.ProjectsDirectory, "NewProject");
        }
    }
}

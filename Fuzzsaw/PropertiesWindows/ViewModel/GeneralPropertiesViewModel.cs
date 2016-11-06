using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Fuzzware.Fuzzsaw.Common;
using Fuzzware.Fuzzsaw.Common.ViewModel;

namespace Fuzzware.Fuzzsaw.PropertiesWindows.ViewModel
{
    public class GeneralPropertiesViewModel : ViewModelBase
    {
        #region Dependency Properties

        static readonly DependencyProperty ProjectsDirectoryProperty = DependencyProperty.Register("ProjectsDirectory", typeof(DirectoryControlViewModel), typeof(GeneralPropertiesViewModel));
        /// <summary>
        /// The Projects Directory
        /// </summary>
        public DirectoryControlViewModel ProjectsDirectory
        {
            get { return (DirectoryControlViewModel)GetValue(ProjectsDirectoryProperty); }
            set { SetValue(ProjectsDirectoryProperty, value); }
        }

        //static readonly DependencyProperty LogDirectoryProperty = DependencyProperty.Register("LogDirectory", typeof(string), typeof(GeneralPropertiesViewModel));
        ///// <summary>
        ///// The Log Directory
        ///// </summary>
        //public string LogDirectory
        //{
        //    get { return (string)GetValue(LogDirectoryProperty); }
        //    set { SetValue(LogDirectoryProperty, value); }
        //}

        #endregion

        public GeneralPropertiesViewModel()
        {
            ProjectsDirectory = new DirectoryControlViewModel();

            ProjectsDirectory.Title = "Choose the default Projects directory";
            ProjectsDirectory.UseRelativePaths = false;
            ProjectsDirectory.DirectoryName = Fuzzsaw.ProjectsDirectory;

            //LogDirectory = Fuzzsaw.LogDirectory;
        }

        public void Update()
        {
            if (String.IsNullOrEmpty(ProjectsDirectory.DirectoryName))
            {
                HelperCommands.ShowError.Execute(new ErrorHelper(this, "The default Projects directory cannot be empty.  Keeping current value.", true), App.Current.MainWindow);
                return;
            }

            if(!System.IO.Directory.Exists(ProjectsDirectory.DirectoryName))
            {
                HelperCommands.ShowError.Execute(new ErrorHelper(this, "The default Projects directory does not exist.  Keeping current value.", true), App.Current.MainWindow);
                return;
            }

            Fuzzsaw.ProjectsDirectory = ProjectsDirectory.DirectoryName;

            //// The default Log directory can be empty and since it could be relative to the Project directory we can't check if it exists
            //Fuzzsaw.LogDirectory = LogDirectory;
        }
    }
}

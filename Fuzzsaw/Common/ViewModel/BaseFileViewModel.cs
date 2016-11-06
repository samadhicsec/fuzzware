using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Fuzzware.Fuzzsaw.Common;

namespace Fuzzware.Fuzzsaw.Common.ViewModel
{
    public class BaseFileViewModel : ViewModelBase
    {
        #region Dependency Properties
        static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(BaseFileViewModel));
        /// <summary>
        /// The title for the file select dialog
        /// </summary>
        public string Title
        {
            get { return (String)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        static readonly DependencyProperty DefaultExtensionProperty = DependencyProperty.Register("DefaultExtension", typeof(string), typeof(BaseFileViewModel));
        /// <summary>
        /// The default extension for the file select dialog
        /// </summary>
        public string DefaultExtension
        {
            get { return (String)GetValue(DefaultExtensionProperty); }
            set { SetValue(DefaultExtensionProperty, value); }
        }

        static readonly DependencyProperty FilterProperty = DependencyProperty.Register("Filter", typeof(string), typeof(BaseFileViewModel));
        /// <summary>
        /// A filter of the type "FileDescription (.FileExtentsion)|*.FileExtentsion", for instance "Executable (.exe)|*.exe".
        /// </summary>
        public string Filter
        {
            get { return (String)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        static readonly DependencyProperty UseRelativePathsProperty = DependencyProperty.Register("UseRelativePaths", typeof(bool), typeof(BaseFileViewModel));
        /// <summary>
        /// Whether or not to return relative paths or not
        /// </summary>
        public bool UseRelativePaths
        {
            get { return (bool)GetValue(UseRelativePathsProperty); }
            set { SetValue(UseRelativePathsProperty, value); }
        }
        #endregion

        /// <summary>
        /// Call the OpenFileDialog.
        /// </summary>
        /// <param name="MultiSelect">Allow many files to be selected and returned</param>
        /// <returns>An array of files selected, or an empty array if none selected</returns>
        protected String[] GetFilesUsingDialog(bool MultiSelect)
        {
            String CurrentWorkingDir = Fuzzsaw.ProjectDirectory + "\\";
            Environment.CurrentDirectory = CurrentWorkingDir;

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Title = Title;
            dlg.DefaultExt = DefaultExtension;                                              // Default file extension
            dlg.Filter = Filter + (String.IsNullOrEmpty(Filter) ? "" : "|") + "All Files|*.*";  // Filter files by extension
            dlg.Multiselect = MultiSelect;                                                  // Allow more than one file to be selected

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // ShowDialog changes the current working directory
            Environment.CurrentDirectory = CurrentWorkingDir;

            // Process open file dialog box results
            if (result == true)
            {
                string[] Paths = dlg.FileNames;
                if (UseRelativePaths)
                {
                    // Edit return strings to be relative paths
                    for (int i = 0; i < dlg.FileNames.Length; i++)
                    {
                        // The CurrentWorkingDir needds to end in a '\', otherwise having CWD or 'c:\temp' and opening a file 'c:\temp.txt' results in a filename of '.txt'
                        if (Paths[i].StartsWith(CurrentWorkingDir))
                        {
                            Paths[i] = Paths[i].Substring(CurrentWorkingDir.Length);
                            if (Paths[i].StartsWith("" + System.IO.Path.DirectorySeparatorChar))
                                Paths[i] = Paths[i].Substring(1);
                        }
                    }
                }
                return Paths;
            }
            return new String[0];
        }
    }
}

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

namespace Fuzzware.Fuzzsaw.Common.Controls
{
    public abstract class CommonFileInput : UserControl
    {
        public static DependencyProperty TitleProperty;
        public static DependencyProperty DefaultExtensionProperty;
        public static DependencyProperty FilterProperty;
        public static DependencyProperty UseRelativePathsProperty;

        protected static void RegisterDependencyProperty(Type type)
        {
            TitleProperty = DependencyProperty.Register("Title", typeof(string), type, new FrameworkPropertyMetadata(""));
            DefaultExtensionProperty = DependencyProperty.Register("DefaultExtension", typeof(string), type, new FrameworkPropertyMetadata(""));
            FilterProperty = DependencyProperty.Register("Filter", typeof(string), type, new FrameworkPropertyMetadata(""));
            UseRelativePathsProperty = DependencyProperty.Register("UseRelativePaths", typeof(bool), type, new FrameworkPropertyMetadata(false));
        }

        /// <summary>
        /// The title for the file select dialog
        /// </summary>
        public string Title
        {
            get { return (String)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        /// <summary>
        /// The default extension for the file select dialog
        /// </summary>
        public string DefaultExtension
        {
            get { return (String)GetValue(DefaultExtensionProperty); }
            set { SetValue(DefaultExtensionProperty, value); }
        }

        /// <summary>
        /// A filter of the type "FileDescription (.FileExtentsion)|*.FileExtentsion", for instance "Executable (.exe)|*.exe".
        /// </summary>
        public string Filter
        {
            get { return (String)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        /// <summary>
        /// Whether or not to return relative paths or not
        /// </summary>
        public bool UseRelativePaths
        {
            get { return (bool)GetValue(UseRelativePathsProperty); }
            set { SetValue(UseRelativePathsProperty, value); }
        }

        /// <summary>
        /// Call the OpenFileDialog.
        /// </summary>
        /// <param name="MultiSelect">Allow many files to be slected and returned</param>
        /// <returns>An array of files selected, or an empty array if none selected</returns>
        protected String[] GetFilesUsingDialog(bool MultiSelect)
        {
            String CurrentWorkingDir = Fuzzsaw.ProjectDirectory;
            Environment.CurrentDirectory = CurrentWorkingDir;

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Title = Title;
            dlg.DefaultExt = DefaultExtension;                                              // Default file extension
            dlg.Filter = Filter + (String.IsNullOrEmpty(Filter)?"":"|") + "All Files|*.*";  // Filter files by extension
            dlg.Multiselect = MultiSelect;                                                  // Allow more than one file to be selected

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                string[] Paths = dlg.FileNames;
                if (UseRelativePaths)
                {
                    // Edit return strings to be relative paths
                    for (int i = 0; i < dlg.FileNames.Length; i++)
                    {
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

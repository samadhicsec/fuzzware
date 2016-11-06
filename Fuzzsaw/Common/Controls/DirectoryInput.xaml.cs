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
using Fuzzware.Fuzzsaw.Common;

namespace Fuzzware.Fuzzsaw.Common.Controls
{
    /// <summary>
    /// Interaction logic for DirectoryInput.xaml
    /// </summary>
    public partial class DirectoryInput : UserControl
    {
        public static DependencyProperty DirectoryNameProperty;
        public static DependencyProperty TitleProperty;
        public static DependencyProperty UseRelativePathsProperty;

        static DirectoryInput()
        {
            TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(DirectoryInput), new FrameworkPropertyMetadata(""));
            UseRelativePathsProperty = DependencyProperty.Register("UseRelativePaths", typeof(bool), typeof(DirectoryInput), new FrameworkPropertyMetadata(false));
            DirectoryNameProperty = DependencyProperty.Register("DirectoryName", typeof(string), typeof(DirectoryInput),
                new FrameworkPropertyMetadata(""));
        }

        /// <summary>
        /// The title for the directory select dialog
        /// </summary>
        public string Title
        {
            get { return (String)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        /// <summary>
        /// Whether or not to return relative paths or not
        /// </summary>
        public bool UseRelativePaths
        {
            get { return (bool)GetValue(UseRelativePathsProperty); }
            set { SetValue(UseRelativePathsProperty, value); }
        }

        public string DirectoryName
        {
            get { return (String)GetValue(DirectoryNameProperty); }
            set { SetValue(DirectoryNameProperty, value); }
        }

        public DirectoryInput()
        {
            InitializeComponent();
        }

        public void butBrowse_Click(Object sender, RoutedEventArgs e)
        {
            String retDirName = FileAndPathHelper.GetFolderUsingDialog(Title, Fuzzsaw.ProjectsDirectory, UseRelativePaths);

            if (!String.IsNullOrEmpty(retDirName))
                DirectoryName = retDirName;
        }
    }
}

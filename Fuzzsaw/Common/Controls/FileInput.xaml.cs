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
    /// <summary>
    /// Interaction logic for FileInput.xaml
    /// </summary>
    public partial class FileInput : CommonFileInput
    {
        public static DependencyProperty FilenameProperty;

        static FileInput()
        {
            RegisterDependencyProperty(typeof(FileInput));
            FilenameProperty = DependencyProperty.Register("Filename", typeof(string), typeof(FileInput), 
                new FrameworkPropertyMetadata(""));
        }

        public string Filename
        {
            get { return (String)GetValue(FilenameProperty); }
            set { SetValue(FilenameProperty, value); }
        }

        public FileInput()
        {
            InitializeComponent();
        }

        public void butBrowse_Click(Object sender, RoutedEventArgs e)
        {
            String[] oFiles = GetFilesUsingDialog(false);

            if (1 == oFiles.Length)
                Filename = oFiles[0];
        }

    }
}

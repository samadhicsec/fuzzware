using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for FilesInput.xaml
    /// </summary>
    public partial class FilesInput : CommonFileInput
    {
        public static DependencyProperty FilenamesProperty;

        static FilesInput()
        {
            RegisterDependencyProperty(typeof(FilesInput));
            FilenamesProperty = DependencyProperty.Register("Filenames", typeof(ObservableCollection<string>), typeof(FilesInput),
                new FrameworkPropertyMetadata(new ObservableCollection<string>()));
        }

        public ObservableCollection<string> Filenames
        {
            get { return (ObservableCollection<string>)GetValue(FilenamesProperty); }
            set { SetValue(FilenamesProperty, value); }
        }

        public FilesInput()
        {
            InitializeComponent();

            Filenames = new ObservableCollection<string>();
        }

        public void butBrowse_Click(Object sender, RoutedEventArgs e)
        {
            String[] oFiles = GetFilesUsingDialog(true);

            if (oFiles.Length > 0)
            {
                ObservableCollection<string> oFilesCollection = Filenames;
                if (null == oFilesCollection)
                    oFilesCollection = new ObservableCollection<string>();
                for (int i = 0; i < oFiles.Length; i++)
                {
                    if (!oFilesCollection.Contains(oFiles[i]))
                        oFilesCollection.Add(oFiles[i]);
                }
            }
        }

        private void butRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lbFilesInput.SelectedItems.Count > 0)
            {
                ObservableCollection<string> oFilesCollection = Filenames;
                string[] oSelectedItems = new string[lbFilesInput.SelectedItems.Count]; 
                lbFilesInput.SelectedItems.CopyTo(oSelectedItems, 0);
                for(int i = 0; i < oSelectedItems.Length; i++)
                {
                    oFilesCollection.Remove(oSelectedItems[i]);
                }
            }
        }
    }
}

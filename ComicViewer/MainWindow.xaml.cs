using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SharpCompress.Archive;

namespace ComicViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
           InitializeComponent();
        }


        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for comic book archives  
            dlg.Filter = "CBR files (*.cbr)|*.cbr|CBZ files (*.cbz)|*.cbz|All Files (*.*)|*.*";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and log to console (for now) 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                openArchive(filename);
            }
        }

        private void openArchive(String filename)
        {
            try
            {
                var archive = ArchiveFactory.Open(filename);
                foreach (IArchiveEntry entry in archive.Entries)
                {
                    if (!entry.IsDirectory)
                    {
                        Console.WriteLine(entry.Key);
                    }
                }
            }
            catch (InvalidOperationException ioe)
            {
                Console.WriteLine("Error opening file " + ioe.Message);
                MessageBox.Show("Error opening file. Please try another.");

            }
        }

       private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("File close.");
        }
       private void MenuItem_Click_3(object sender, RoutedEventArgs e)
       {
           Console.WriteLine("File save.");
       }
       
    }
}

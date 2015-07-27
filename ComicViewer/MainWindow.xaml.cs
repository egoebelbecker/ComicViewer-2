using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using SharpCompress.Archive;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace ComicViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        IArchive archive;
        int currentEntry = 0;
        int currentWidth = 0;
        int maxImageHeight = (int)System.Windows.SystemParameters.PrimaryScreenHeight - 75;

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
                openArchive(dlg.FileName);

                loadPage(0);
            }
        }

        private void loadPage(int index)
        {
            IArchiveEntry fileEntry = getEntry(index);
            Bitmap bitmap = loadAndResizeBitmap(fileEntry);
            Console.WriteLine("Image size " + bitmap.Width + "x" + bitmap.Height);
            displayImage(bitmap);
            currentEntry = index;
            currentWidth = bitmap.Width;
        }

        private void openArchive(String filename)
        {
            try
            {
                archive = ArchiveFactory.Open(filename);
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

        private IArchiveEntry getEntry(int index)
        {
            IArchiveEntry entry = archive.Entries.ElementAt(index);
            while (entry.IsDirectory) {
                entry = archive.Entries.ElementAt(++index);
            }
            return entry;
        }

        private void displayImage(Bitmap bitmap)
        {
            MemoryStream memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.EndInit();
            ImageViewer1.Source = bitmapImage;
            ImageViewer1.Height = bitmap.Height;
            ImageViewer1.Width = bitmap.Width;
        }

        private Bitmap loadAndResizeBitmap(IArchiveEntry fileEntry)
        {
            Bitmap bitmap = (Bitmap)Bitmap.FromStream(fileEntry.OpenEntryStream());

            if (bitmap.Height > System.Windows.SystemParameters.PrimaryScreenHeight)
            {
                System.Drawing.Size oldSize = new System.Drawing.Size(bitmap.Width, bitmap.Height);
                System.Drawing.Size newSize = getNewImageSize(oldSize);
                Bitmap newImage = ResizeImage(bitmap, newSize);
                bitmap = newImage;
            }
            return bitmap;
        }

        private System.Drawing.Size getNewImageSize(System.Drawing.Size oldSize)
        {
            Console.WriteLine("Old dimensions: " + oldSize.Width + "x" + oldSize.Height);

            float ratio;
            if (oldSize.Height > oldSize.Width)
            {
                ratio = (float)oldSize.Width / oldSize.Height;
            }
            else
            {
                ratio = (float)oldSize.Height / oldSize.Width;
            }

            Console.WriteLine("ratio: " + ratio);
            int newWidth = (int)(maxImageHeight * ratio);
            System.Drawing.Size newSize = new System.Drawing.Size(newWidth, maxImageHeight);
            Console.WriteLine("New dimensions: " + newSize.Width + "x" + newSize.Height);
            return newSize;
        }

        public static Bitmap ResizeImage(Image image, System.Drawing.Size newSize)
        {

            // Make a rectangle that is the new size
            Rectangle destRect = new Rectangle(0, 0, newSize.Width, newSize.Height);

            // Make a bitmap that is the new size
            Bitmap destImage = new Bitmap(newSize.Width, newSize.Height);

            // Set new image to the resolution of the original
            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            
            // Create a GDI holder and use it
            using (Graphics graphics = Graphics.FromImage(destImage))
            {

                // Set our quality options
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                // Resize original image into new one
                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }

        private void Image_Click(object sender, MouseButtonEventArgs e)
        {
            double azimuth = e.GetPosition(ImageViewer1).X;
            Console.WriteLine("Click: " + azimuth);
            
            if (currentWidth != 0) {

                if (azimuth > (currentWidth / 2)) 
                {
                    loadPage(++currentEntry);
                }
                else
                {
                    if (currentEntry != 0) { 
                        loadPage(--currentEntry);
                    }
                }
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

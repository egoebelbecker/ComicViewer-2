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
        IArchiveEntry currentEntry;
        int currentIndex = -1;
        int currentWidth = 0;

        int maxImageHeight = (int)System.Windows.SystemParameters.PrimaryScreenHeight - 75;

        enum Direction {
            Forward,
            Back
        }

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
                // Open document, reset page counter
                currentIndex = -1;
                openArchive(dlg.FileName);
                loadPage(Direction.Forward);
            }
        }

        private void loadPage(Direction direction)
        {

            if (direction == Direction.Forward)
            {
                currentEntry = getNextEntry();
            }
            else
            {
                currentEntry = getPreviousEntry();
            }

            Bitmap bitmap = loadAndResizeBitmap(currentEntry);
            displayImage(bitmap);
            currentWidth = bitmap.Width;
        }

        private IArchiveEntry getPreviousEntry()
        {
            Console.WriteLine("Getting index " + currentIndex);

            // Don't try to go back before the head of the collection
            if (currentIndex == 0)
            {
                return currentEntry;
            }
            else
            {
                IArchiveEntry entry = archive.Entries.ElementAt(--currentIndex);
                while (entry.IsDirectory)
                {
                    getPreviousEntry();
                }
                return entry;
            }
        }


        private IArchiveEntry getNextEntry()
        {
            Console.WriteLine("Getting index " + currentIndex);
            if (currentIndex == (archive.Entries.Count() - 1))
            {
                return currentEntry;
            }
            else
            {
                IArchiveEntry entry = archive.Entries.ElementAt(++currentIndex);
                while (entry.IsDirectory)
                {
                    getNextEntry();
                }
                return entry;
            }
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
            float ratio;
            if (oldSize.Height > oldSize.Width)
            {
                ratio = (float)oldSize.Width / oldSize.Height;
            }
            else
            {
                ratio = (float)oldSize.Height / oldSize.Width;
            }
            int newWidth = (int)(maxImageHeight * ratio);
            System.Drawing.Size newSize = new System.Drawing.Size(newWidth, maxImageHeight);
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
            if (currentWidth != 0)
            {
                if (azimuth > (currentWidth / 2))
                {
                    Console.WriteLine("incrementing from " + currentIndex);
                    loadPage(Direction.Forward);
                }
                else
                {
                    Console.WriteLine("decrementing from " + currentIndex);
                    loadPage(Direction.Back);
                }
            }
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("File close.");
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = archive.Entries.ElementAt(currentIndex).Key;

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and log to console (for now) 
            if (result == true)
            {
                archive.Entries.ElementAt(currentIndex).WriteToFile(dlg.FileName);
            }
        }
    }
}

using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Ilmasheva_pr34
{
    public partial class MainWindow : Window
    {
        private BitmapImage originalImage;
        private CroppedBitmap croppedBitmap;

        private bool isDragging = false;
        private bool isResizing = false;
        private Point clickPosition;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp";

            if (dialog.ShowDialog() == true)
            {
                originalImage = new BitmapImage(new Uri(dialog.FileName));

                MainImage.Source = originalImage;

                ImageCanvas.Width = originalImage.PixelWidth;
                ImageCanvas.Height = originalImage.PixelHeight;

                MainImage.Width = originalImage.PixelWidth;
                MainImage.Height = originalImage.PixelHeight;

                ResetFrame();
            }
        }

        private void CropFrame_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isResizing)
                return;

            isDragging = true;
            clickPosition = e.GetPosition(ImageCanvas);

            CropFrame.CaptureMouse();
        }

        private void CropFrame_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging)
                return;

            Point currentPosition = e.GetPosition(ImageCanvas);

            double left = Canvas.GetLeft(CropFrame);
            double top = Canvas.GetTop(CropFrame);

            double offsetX = currentPosition.X - clickPosition.X;
            double offsetY = currentPosition.Y - clickPosition.Y;

            Canvas.SetLeft(CropFrame, left + offsetX);
            Canvas.SetTop(CropFrame, top + offsetY);

            clickPosition = currentPosition;
        }

        private void CropFrame_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            CropFrame.ReleaseMouseCapture();
        }

        private void Resize_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isResizing = true;
            clickPosition = e.GetPosition(ImageCanvas);

            ((UIElement)sender).CaptureMouse();

            e.Handled = true;
        }

        private void Resize_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isResizing)
                return;

            Point position = e.GetPosition(ImageCanvas);

            double dx = position.X - clickPosition.X;
            double dy = position.Y - clickPosition.Y;

            CropFrame.Width += dx;
            CropFrame.Height += dy;

            if (CropFrame.Width < 50)
                CropFrame.Width = 50;

            if (CropFrame.Height < 50)
                CropFrame.Height = 50;

            clickPosition = position;
        }

        private void Resize_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isResizing = false;

            ((UIElement)sender).ReleaseMouseCapture();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            ResetFrame();
        }

        private void ResetFrame()
        {
            if (originalImage == null)
                return;

            CropFrame.Width = originalImage.PixelWidth / 2;
            CropFrame.Height = originalImage.PixelHeight / 2;

            Canvas.SetLeft(CropFrame, originalImage.PixelWidth / 4);
            Canvas.SetTop(CropFrame, originalImage.PixelHeight / 4);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (MainImage.Source == null)
                return;

            SaveFileDialog dialog = new SaveFileDialog();

            dialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg";

            if (dialog.ShowDialog() == true)
            {
                BitmapSource bitmap = (BitmapSource)MainImage.Source;
                BitmapEncoder encoder;

                if (System.IO.Path.GetExtension(dialog.FileName).ToLower() == ".jpg")
                {
                    encoder = new JpegBitmapEncoder();
                }
                else
                {
                    encoder = new PngBitmapEncoder();
                }

                encoder.Frames.Add(BitmapFrame.Create(bitmap));

                using (FileStream stream = new FileStream(dialog.FileName, FileMode.Create))
                {
                    encoder.Save(stream);
                }

                MessageBox.Show("Фото сохранено!");
            }
        }
    }
}

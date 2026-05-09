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
        // Изначальное изображение
        private BitmapImage originalImage;
        // Обрезанное изображение
        private CroppedBitmap croppedBitmap;
        // Переменная отвечающая за статус перемещения рамки
        private bool isDragging = false;
        // Переменная отвечающая за статус изменения размера рамки
        private bool isResizing = false;
        // Точка нажатия мыши
        private Point clickPosition;

        public MainWindow()
        {
            InitializeComponent();
        }

        // Метод загрузки изображения
        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            // Окно выбора файла
            OpenFileDialog dialog = new OpenFileDialog();

            // Разрешённые форматы
            dialog.Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp";

            // Если файл выбран то:
            if (dialog.ShowDialog() == true)
            {
                // Присваиваем переменной originalImage выбранное изображение
                originalImage = new BitmapImage(new Uri(dialog.FileName));

                // И отображаем его
                MainImage.Source = originalImage;

                // Присваиваем Canvas'у (блоку на котором находится наше изображение и Frame для обрезки фото) размеры загруженного изображения
                ImageCanvas.Width = originalImage.PixelWidth;
                ImageCanvas.Height = originalImage.PixelHeight;

                // Присваиваем блоку для изображения размеры загруженного изображения
                MainImage.Width = originalImage.PixelWidth;
                MainImage.Height = originalImage.PixelHeight;

                // Сбрасываем рамку
                ResetFrame();
            }
        }


        // --------------------------------------  ИЗМЕНЕНИЕ ПОЛОЖЕНИЯ РАМКИ ОБРЕЗКИ  --------------------------------------
        // Метод отвечающий за поведение при начале перемещения рамки обрезки (задержка левой кнопки мыши)
        private void CropFrame_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Если изменяем размер изображения, то возвращаем (рамка не двигается)
            if (isResizing)
                return;

            // Если нет, то включаем режим перемещения
            isDragging = true;
            // Получаем позицию курсора
            clickPosition = e.GetPosition(ImageCanvas);

            // И захватываем мышь
            CropFrame.CaptureMouse();
        }

        // Метод отвечающий за поведение при перемещении рамки обрезки
        private void CropFrame_MouseMove(object sender, MouseEventArgs e)
        {
            // Если режим переключения = false, то выходим
            if (!isDragging)
                return;

            // Получаем текущую позицию мыши
            Point currentPosition = e.GetPosition(ImageCanvas);

            // Текущие координаты рамки
            double left = Canvas.GetLeft(CropFrame);
            double top = Canvas.GetTop(CropFrame);

            // Смещение мыши
            double offsetX = currentPosition.X - clickPosition.X;
            double offsetY = currentPosition.Y - clickPosition.Y;

            // Перемещаем рамку
            Canvas.SetLeft(CropFrame, left + offsetX);
            Canvas.SetTop(CropFrame, top + offsetY);

            // Обновляем позицию мыши
            clickPosition = currentPosition;
        }

        // Метод отвечающий за поведение при завершении перемещения рамки обрезки (отпускаем левую кнопку мыши)
        private void CropFrame_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Выключаем режим перемещения
            isDragging = false;
            // Отключаем захват мыши
            CropFrame.ReleaseMouseCapture();
        }



        // --------------------------------------  ИЗМЕНЕНИЕ РАЗМЕРА РАМКИ ОБРЕЗКИ  --------------------------------------
        // Метод отвечающий за поведение при начале изменения размеров рамки обрезки (задержка левой кнопки мыши)
        private void Resize_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Включаем режим изменения размера
            isResizing = true;
            //Получаем позицию мыши
            clickPosition = e.GetPosition(ImageCanvas);

            // Захватываем мышь
            ((UIElement)sender).CaptureMouse();

            // Останавливаем обработку
            e.Handled = true;
        }

        // Изменение размера рамки
        private void Resize_MouseMove(object sender, MouseEventArgs e)
        {
            // Если режим выключен, то выходим
            if (!isResizing)
                return;

            // Получаем новую позицию мыши
            Point position = e.GetPosition(ImageCanvas);

            // Вычисляем разницу координат
            double dx = position.X - clickPosition.X;
            double dy = position.Y - clickPosition.Y;

            // Изменяем размеры рамки
            CropFrame.Width += dx;
            CropFrame.Height += dy;

            // Минимальные размеры рамки
            if (CropFrame.Width < 50)
                CropFrame.Width = 50;

            if (CropFrame.Height < 50)
                CropFrame.Height = 50;

            // Обновляем позицию мыши
            clickPosition = position;
        }

        // Завершение изменения размера
        private void Resize_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Выключаем режим изменения размера
            isResizing = false;

            // Отключаем захват мыши
            ((UIElement)sender).ReleaseMouseCapture();
        }



        // --------------------------------------  ОБРЕЗКА ИЗОБРАЖЕНИЯ  --------------------------------------
        private void BtnCrop_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем наличие изображения
            if (MainImage.Source == null)
                return;

            // Получаем источник изображения
            BitmapSource source = (BitmapSource)MainImage.Source;

            // Координаты рамки
            int cropX = (int)Canvas.GetLeft(CropFrame);
            int cropY = (int)Canvas.GetTop(CropFrame);

            // Размеры рамки
            int cropWidth = (int)CropFrame.Width;
            int cropHeight = (int)CropFrame.Height;

            // Проверяем вышли ли за границы изображения
            if (cropX + cropWidth > source.PixelWidth)
                cropWidth = source.PixelWidth - cropX;

            if (cropY + cropHeight > source.PixelHeight)
                cropHeight = source.PixelHeight - cropY;

            // Проверяем корректность размеров
            if (cropWidth <= 0 || cropHeight <= 0)
                return;

            // Создаём обрезанное изображение
            CroppedBitmap cropped = new CroppedBitmap(source, new Int32Rect(cropX, cropY, cropWidth, cropHeight));

            // Отображаем результат
            MainImage.Source = cropped;

            // Обновляем размеры изображения
            MainImage.Width = cropped.PixelWidth;
            MainImage.Height = cropped.PixelHeight;

            ImageCanvas.Width = cropped.PixelWidth;
            ImageCanvas.Height = cropped.PixelHeight;

            // Сохраняем новое изображение как начальное
            originalImage = BitmapToBitmapImage(cropped);

            // Сбрасываем рамку
            ResetFrame();
        }

        // Обработчик кнопки "Сбросить"
        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            ResetFrame();
        }

        // Метод установки рамки по центру
        private void ResetFrame()
        {
            // Проверяем есть ли изображение
            if (originalImage == null)
                return;

            // Размер рамки это половина изображения
            CropFrame.Width = originalImage.PixelWidth / 2;
            CropFrame.Height = originalImage.PixelHeight / 2;

            // Устанавливаем рамку по центру
            Canvas.SetLeft(CropFrame, originalImage.PixelWidth / 4);
            Canvas.SetTop(CropFrame, originalImage.PixelHeight / 4);
        }

        // Метод сохранения изображения
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем есть ли изображение
            if (MainImage.Source == null)
                return;

            // Открываем папку для сохранения изображения
            SaveFileDialog dialog = new SaveFileDialog();

            // Форматы сохранения
            dialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg";

            // Если пользователь выбрал место сохранения, то:
            if (dialog.ShowDialog() == true)
            {
                // Получаем изображение
                BitmapSource bitmap = (BitmapSource)MainImage.Source;
                BitmapEncoder encoder;

                // Выбор формата
                if (System.IO.Path.GetExtension(dialog.FileName).ToLower() == ".jpg")
                {
                    encoder = new JpegBitmapEncoder();
                }
                else
                {
                    encoder = new PngBitmapEncoder();
                }

                // Добавляем кадр изображения
                encoder.Frames.Add(BitmapFrame.Create(bitmap));

                // Сохраняем файл
                using (FileStream stream = new FileStream(dialog.FileName, FileMode.Create))
                {
                    encoder.Save(stream);
                }

                MessageBox.Show("Фото сохранено!");
            }
        }

        // Метод для конвертации BitmapSource в BitmapImage (нужен потому что после обрезки изображение возвращается в формате BitmapSource, а в проге используется формат BitmapImage)
        private BitmapImage BitmapToBitmapImage(BitmapSource bitmapSource)
        {
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

            using (MemoryStream stream = new MemoryStream())
            {
                encoder.Save(stream);

                BitmapImage image = new BitmapImage();

                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = new MemoryStream(stream.ToArray());
                image.EndInit();

                return image;
            }
        }
    }
}

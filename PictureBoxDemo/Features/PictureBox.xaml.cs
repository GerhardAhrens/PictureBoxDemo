namespace PictureBoxDemo
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media.Imaging;

    public sealed class ByteArrayToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not byte[] bytes) return null;

            var image = new BitmapImage();
            using var mem = new MemoryStream(bytes);

            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = mem;
            image.EndInit();

            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public partial class PictureBox : UserControl
    {
         public PictureBox()
        {
            this.InitializeComponent();
            WeakEventManager<Button, RoutedEventArgs>.AddHandler(this.BtnMoveDown, "Click", this.OnButtonAction);
            WeakEventManager<Button, RoutedEventArgs>.AddHandler(this.BtnMoveUp, "Click", this.OnButtonAction);
            WeakEventManager<Button, RoutedEventArgs>.AddHandler(this.BtnDelete, "Click", this.OnButtonAction);
            WeakEventManager<Button, RoutedEventArgs>.AddHandler(this.BtnPaste, "Click", this.OnButtonAction);
            WeakEventManager<Button, RoutedEventArgs>.AddHandler(this.BtnCopy, "Click", this.OnButtonAction);
        }

        // ===========================
        // ItemsSource
        // ===========================

        public ObservableCollection<byte[]> ItemsSource
        {
            get => (ObservableCollection<byte[]>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(ObservableCollection<byte[]>),
                typeof(PictureBox),
                new PropertyMetadata(null));


        // ===========================
        // SelectedPicture
        // ===========================

        public byte[] SelectedPicture
        {
            get => (byte[])GetValue(SelectedPictureProperty);
            set => SetValue(SelectedPictureProperty, value);
        }

        public static readonly DependencyProperty SelectedPictureProperty =
            DependencyProperty.Register(
                nameof(SelectedPicture),
                typeof(byte[]),
                typeof(PictureBox),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        private void OnButtonAction(object sender, RoutedEventArgs e)
        {
            Button btnAction = sender as Button;
            if (btnAction != null)
            {
                if (btnAction.Tag?.ToString().Contains("up", StringComparison.CurrentCultureIgnoreCase) == true)
                {
                    this.OnMove(-1);
                }
                else if (btnAction.Tag?.ToString().Contains("Down",StringComparison.CurrentCultureIgnoreCase) == true)
                {
                    this.OnMove(1);
                }
                else if (btnAction.Tag?.ToString().Contains("delete", StringComparison.CurrentCultureIgnoreCase) == true)
                {
                    this.Delete();
                }
                else if (btnAction.Tag?.ToString().Contains("paste", StringComparison.CurrentCultureIgnoreCase) == true)
                {
                    this.Paste();
                }
                else if (btnAction.Tag?.ToString().Contains("copy", StringComparison.CurrentCultureIgnoreCase) == true)
                {
                    this.Copy();
                }
            }
        }


        private void OnMove(int offset)
        {
            if (ItemsSource == null || SelectedPicture == null)
            {
                return;
            }

            int index = ItemsSource.IndexOf(SelectedPicture);
            int newIndex = index + offset;

            if (newIndex < 0 || newIndex >= ItemsSource.Count)
            {
                return;
            }

            ItemsSource.Move(index, newIndex);
        }

        private void Delete()
        {
            if (SelectedPicture == null)
            {
                return;
            }

            ItemsSource.Remove(SelectedPicture);
            SelectedPicture = ItemsSource.FirstOrDefault();
        }

        private void Paste()
        {
            if (Clipboard.ContainsImage() == false)
            {
                return;
            }

            var image = Clipboard.GetImage();
            ItemsSource.Add(ImageToBytes(image));
            SelectedPicture = ItemsSource.Last();
        }

        private void Copy()
        {
            if (SelectedPicture == null)
            {
                return;
            }

            Clipboard.SetImage(BytesToImage(SelectedPicture));
        }
        private static byte[] ImageToBytes(BitmapSource image)
        {
            if (image == null) return null;

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));

            using var stream = new MemoryStream();
            encoder.Save(stream);

            return stream.ToArray();
        }

        // byte[] → BitmapSource
        private static BitmapSource BytesToImage(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return null;
            }

            var bitmap = new BitmapImage();

            using var stream = new MemoryStream(bytes);

            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }
    }
}

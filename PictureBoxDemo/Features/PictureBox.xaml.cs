namespace PictureBoxDemo
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using Windows.ApplicationModel.Email.DataProvider;

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
        private static int movePos;

        public PictureBox()
        {
            this.InitializeComponent();
            WeakEventManager<UserControl, RoutedEventArgs>.AddHandler(this, "Loaded", this.OnLoaded);
            WeakEventManager<Button, RoutedEventArgs>.AddHandler(this.BtnMoveDown, "Click", this.OnButtonAction);
            WeakEventManager<Button, RoutedEventArgs>.AddHandler(this.BtnMoveUp, "Click", this.OnButtonAction);
            WeakEventManager<Button, RoutedEventArgs>.AddHandler(this.BtnDelete, "Click", this.OnButtonAction);
            WeakEventManager<Button, RoutedEventArgs>.AddHandler(this.BtnPaste, "Click", this.OnButtonAction);
            WeakEventManager<Button, RoutedEventArgs>.AddHandler(this.BtnCopy, "Click", this.OnButtonAction);
        }


        /// <summary>
        /// ItemsSource
        /// </summary>
        public ObservableCollection<byte[]> ItemsSource
        {
            get => (ObservableCollection<byte[]>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(ObservableCollection<byte[]>),
                typeof(PictureBox), new PropertyMetadata(null));

        /// <summary>
        /// SelectedPicture
        /// </summary>
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
                new FrameworkPropertyMetadata(null,FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// PictureInfo
        /// </summary>
        public string PictureInfo
        {
            get => (string)GetValue(PictureInfoProperty);
            set => SetValue(PictureInfoProperty, value);
        }

        public static readonly DependencyProperty PictureInfoProperty =
            DependencyProperty.Register(
                nameof(PictureInfo),
                typeof(string),
                typeof(PictureBox),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// Stretch
        /// </summary>
        public Stretch PictureStretch
        {
            get => (Stretch)GetValue(PictureStretchProperty);
            set => SetValue(PictureStretchProperty, value);
        }

        public static readonly DependencyProperty PictureStretchProperty =
            DependencyProperty.Register(
                nameof(PictureStretch),
                typeof(Stretch),
                typeof(PictureBox),
                new FrameworkPropertyMetadata(Stretch.Uniform, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (ItemsSource == null || SelectedPicture == null)
            {
                string base64Photo = Photo.EmptyPhoto;
                this.pictureBox.Source = Base64ToImageSource(base64Photo);
                this.BtnMoveDown.IsEnabled = false;
                this.BtnMoveUp.IsEnabled = false;
                this.BtnDelete.IsEnabled = false;
                this.BtnCopy.IsEnabled = false;
            }
            else
            {
                movePos = 0;
            }

            this.TbPictureInfo.Text = PictureInfo;
            this.pictureBox.Stretch = this.PictureStretch;
        }

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
            if (this.ItemsSource == null || this.ItemsSource.Count == 0  || SelectedPicture == null)
            {
                return;
            }

            this.BtnMoveDown.IsEnabled = true;
            this.BtnMoveUp.IsEnabled = true;

            movePos = movePos + offset;
            if (movePos >= ItemsSource.Count)
            {
                this.BtnMoveDown.IsEnabled = false;
                return;
            }

            if (movePos < 0)
            {
                this.BtnMoveUp.IsEnabled = false;

                return;
            }

            this.pictureBox.Source = BytesToImage(this.ItemsSource[movePos]);
            this.SelectedPicture = this.ItemsSource[movePos];

        }

        private void Delete()
        {
            if (SelectedPicture == null)
            {
                return;
            }

            this.ItemsSource.Remove(this.SelectedPicture);
            this.SelectedPicture = this.ItemsSource.FirstOrDefault();
            if (this.ItemsSource.Count == 0)
            {
                string base64Photo = Photo.EmptyPhoto;
                this.pictureBox.Source = Base64ToImageSource(base64Photo);
                this.BtnMoveDown.IsEnabled = false;
                this.BtnMoveUp.IsEnabled = false;
                this.BtnDelete.IsEnabled = false;
                this.BtnCopy.IsEnabled = false;
            }
        }

        private void Paste()
        {
            if (Clipboard.ContainsImage() == false)
            {
                return;
            }

            var image = Clipboard.GetImage();
            this.ItemsSource.Add(ImageToBytes(image));
            this.SelectedPicture = this.ItemsSource.Last();
            this.pictureBox.Source = BytesToImage(this.SelectedPicture);

            if (this.ItemsSource.Count > 0)
            {
                this.BtnMoveDown.IsEnabled = true;
                this.BtnMoveUp.IsEnabled = true;
                this.BtnDelete.IsEnabled = true;
                this.BtnCopy.IsEnabled = true;
            }
        }

        private void Copy()
        {
            if (SelectedPicture == null)
            {
                return;
            }

            Clipboard.SetImage(BytesToImage(SelectedPicture));
        }

        private static BitmapImage Base64ToImageSource(string base64String)
        {
            BitmapImage bi = new BitmapImage();

            bi.BeginInit();
            bi.StreamSource = new MemoryStream(System.Convert.FromBase64String(base64String));
            bi.EndInit();

            return bi;
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
        private static BitmapImage BytesToImage(byte[] bytes)
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

        private static class Photo
        {
            static Photo()
            {
                EmptyPhoto = "iVBORw0KGgoAAAANSUhEUgAAAgAAAAHGCAMAAAA1/8P8AAAABGdBTUEAALGPC/xhBQAAACBjSFJNAAB6JgAAgIQAAPoAAACA6AAAdTAAAOpgAAA6mAA"
                    +"AF3CculE8AAAAB3RJTUUH5gwEDjcuABSomAAAAvdQTFRFAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
                    +"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
                    +"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
                    +"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
                    +"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
                    +"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
                    +"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
                    +"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
                    +"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAlAzgpgAAAPx0Uk5TAAECAwQFBgcICQoLDA0ODxAREhMUFRYXGBkaGxweHyAhIiMkJSYnKCkqKywtLi8wMTIzNDU2Nzg5O"
                    +"js8PT4/QEFCQ0RFRkdISktMTU5PUFFSU1RVVldYWVpbXF1eX2BhYmNkZWZnaGlqa2xtbm9wcXJzdHV2d3h5ent8fX5/gIGCg4SFhoeIiYqLjI2Oj5CRkpOUlZaXmJmam5ydnp+g"
                    +"oaKjpKWmp6ipqqusra6vsLGys7S1tre4ubq7vL2+v8DBwsPExcbHyMrLzM3Oz9DR0tPU1dbX2Nna29zd3t/g4eLj5OXm5+jp6uvs7e7v8PHy8/T19vf4+fr7/P3+HLjZxQAAAA"
                    +"FiS0dE/DwOo38AABYQSURBVHja7Z13gBRFvsd/y7Isu8tKOJagcIIIwp3vIeLpcQ9QEE/hSBIUH+HwDpB8osQ9RBFBUJAcBAkiaRFFOQFBHxiAOyTnHASOKGmBQ6T/eLMLKOjWb3"
                    +"pmuqsrfD//dk9NTX2+VdOhqptIZ0o/rAClCQRE4n5HAfYnwERA9HaUoBdMBEPxTDUCkJkGF4EwzVGEqXARBBUdZagIGwGwSp0ArIIN+TR3FOJ/4UM2yYdVCsChZCXOigpbFIABjl"
                    +"L0V6BJxjjb7ElAyUtqBeBSycCbZFioGvYkYK6jGBlBt8iQ7GrYkoBqjnJUU8G/LQmI26xeADbHqeA/lICiFgSgnaMgbZTw7zi7zU9A6ikVA3AqVQn/NiRgmKMkQ9Xwb34CylxRMw"
                    +"CXy6jh3/gEfOooyj8U8W94Amo7ylJLEf9GJyBhp7oB2BmviP9QAu4wNQDPOwrTVRX/jrPP0ASknVU5AGcLquLf2ARMcJRmnDL+DU1AxatqB+BqBWX8m5mAVY7iLFfHv4kJaMT+3kZ"
                    +"ylgI1YCvxpKy2GOgijl0M888vBRojqxoj2YVCiar0f8cZYtoA8Hc1jsALnOTq0VuZ/m+cf34pUGd5FenILhQqjv7vE++pchUunp2QMg3+/eFBda7D81PSHlTBfz/zTgHXcr/3Y7l1+Z"
                    +"Cry1oF/L9gnv9WKt2LL3mZq03z4E5CzPWffIz7wW/Irs7rXG0OJ8O/5wzifvCxVLXyOAD+5Y65f5FfodbBLBSy1T99wB51BTAnP449Jp0L/0add0V+VloN/uVdeZkdTKVmSl8oZK1"
                    +"/6sD+4RYPplLF2RXK7eDfOwqyS4FeDqpaL8ldKGSvfxrBnnQnBlWtRPYpJcPg3ysqsEuBmgVXsae4el0pA/8esZz70YE+oYudofYp/HtDXfZXB/qMPn6O6hPw7wX8UqDJwVZuEjtFI"
                    +"QH+PeBFdvpNwM/p5depPC/R/0BT/fNN3EPpeHo0TdFq//wgG/yz+hPYicrj4d/fw6z6ih+ierFQyG7//InWchVquNzfGlrun7/UUkGFKvKXqRrDfyzwF1tHqVFJPxcKWe6f+rG3W"
                    +"wqqUUn+VlU6/EcPf8O1kyrV9G2h0ATL/YeZchGvSjX56SrT4T9agph0FRX+TFiz3j8/7fIjlao634eFQm78DzPZPz2r0lIgHn7Sekv4j4ZUdunFYLUqyy4UOpYM/1EwWK"
                    +"2lQDz8QqHX4D9yyrCj6rOqVZddKHS5JPxHzEeqLQWK5Yh1HvxHSi31lgJJO2eFf6J4dh7YTP2uWsXBf0R0UnEpEA9/3fo5+I8E/v6Koo+/8WahEPxnMVrNpUA8/L3r4fDvHn6OxV"
                    +"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";
            }

            public static string EmptyPhoto { get; }
        }
    }

}

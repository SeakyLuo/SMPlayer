using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace SMPlayer
{
    public static class ColorHelper
    {
        public static SolidColorBrush WhiteBrush = new SolidColorBrush(Colors.White);
        public static SolidColorBrush RedBrush = new SolidColorBrush(Colors.Red);
        public static SolidColorBrush WhiteSmokeBrush = new SolidColorBrush(Colors.WhiteSmoke);
        public static SolidColorBrush GrayBrush = new SolidColorBrush(Colors.Gray);
        public static SolidColorBrush BlackBrush = new SolidColorBrush(Colors.Black);
        public static SolidColorBrush TransparentBrush = new SolidColorBrush(Colors.Transparent);
        public static SolidColorBrush HighlightBrush
        {
            get => new SolidColorBrush(Models.Settings.settings.ThemeColor);
        }
        public static Brush MinimalTitleBarColor
        {
            get => Application.Current.Resources["MinimalTitleBarColor"] as Brush;
        }
        public static Brush MainNavigationViewBackground
        {
            get => Application.Current.Resources["MainNavigationViewBackground"] as Brush;
        }
        public static Color SystemColorHighlightColor
        {
            get => (Color) Application.Current.Resources["SystemColorHighlightColor"];
        }
        private struct Size
        {
            public uint Width, Height;
            public Size(uint Width, uint Height)
            {
                this.Width = Width;
                this.Height = Height;
            }
        }

        private const int MIN_VALUE = 10;
        private const int MAX_VALUE = 205;

        private static async Task<byte[]> GetPixelData(this BitmapDecoder decoder, uint x, uint y)
        {
            var data = await decoder.GetPixelDataAsync(BitmapPixelFormat.Bgra8,
                                                       BitmapAlphaMode.Straight,
                                                       new BitmapTransform()
                                                       {
                                                           Bounds = new BitmapBounds() { Width = 1, Height = 1, X = x, Y = y }
                                                       },
                                                       ExifOrientationMode.IgnoreExifOrientation,
                                                       ColorManagementMode.DoNotColorManage);
            return data.DetachPixelData();
        }

        public static async Task<Brush> GetMainColorAsync(this BitmapImage image)
        {
            RandomAccessStreamReference reference = RandomAccessStreamReference.CreateFromUri(image.UriSour‌​ce);
            return await GetThumbnailMainColorAsync(await reference.OpenReadAsync());
        }

        public static async Task<Brush> GetThumbnailMainColorAsync(IRandomAccessStream Thumbnail)
        {
            var decoder = await BitmapDecoder.CreateAsync(Thumbnail);
            uint width = decoder.PixelWidth, height = decoder.PixelHeight;
            uint divs = 16;
            byte[] bgra = new byte[] { 0, 0, 0, 255 };
            double distance = -1;
            for (uint i = 1; i < divs; i++)
            {
                for (uint j = 1; j < divs; j++)
                {
                    byte[] pixel = (await decoder.GetPixelData(width * i / divs, height * j / divs));
                    var bgr = pixel.SkipLast(1);
                    if (bgr.All(p => MIN_VALUE <= p && p <= MAX_VALUE) && bgr.Sum(p => Math.Pow(p - MIN_VALUE, 2)) > distance)
                        bgra = pixel;
                }
            }
            Color color = Color.FromArgb(bgra[3], bgra[2], bgra[1], bgra[0]);
            System.Diagnostics.Debug.WriteLine($"R: {bgra[2]} G: {bgra[1]} B: {bgra[0]}");
            return new AcrylicBrush()
            {
                BackgroundSource = AcrylicBackgroundSource.Backdrop,
                FallbackColor = color,
                TintOpacity = 0.75,
                TintColor = color
            };
        }
        public static async Task<Brush> GetThumbnailMainColorCommonAsync(IRandomAccessStream Thumbnail)
        {
            var decoder = await BitmapDecoder.CreateAsync(Thumbnail);
            uint width = decoder.PixelWidth, height = decoder.PixelHeight;
            Dictionary<byte, int>[] dict = new Dictionary<byte, int>[4];
            for (int n = 0; n < 4; n++) dict[n] = new Dictionary<byte, int>();
            for (uint i = 0; i < width - 1; i++)
            {
                for (uint j = 0; j < height - 1; j++)
                {
                    var bytes = await decoder.GetPixelData(i, j);
                    for (int n = 0; n < 4; n++)
                        dict[n][bytes[n]] = dict[n].GetValueOrDefault(bytes[n], 0) + 1;
                }
            }
            byte[] bgra = new byte[4];
            for (int n = 0; n < 4; n++)
                bgra[n] = dict[n].Aggregate((p1, p2) => p1.Value > p2.Value ? p1 : p2).Key;
            Color color = Color.FromArgb(bgra[3], bgra[2], bgra[1], bgra[0]);
            return new AcrylicBrush()
            {
                BackgroundSource = AcrylicBackgroundSource.Backdrop,
                FallbackColor = color,
                TintOpacity = 0.75,
                TintColor = color
            };
        }

        public static async Task<Brush> GetThumbnailMainColorAverageAsync(IRandomAccessStream Thumbnail)
        {
            var decoder = await BitmapDecoder.CreateAsync(Thumbnail);
            uint width = decoder.PixelWidth, height = decoder.PixelHeight;
            byte[] bgra = new byte[4];
            for (uint i = 0; i < width - 1; i++)
            {
                for (uint j = 0; j < height - 1; j++)
                {
                    var bytes = await decoder.GetPixelData(i, j);
                    for (int n = 0; n < 4; n++)
                        bgra[n] += bytes[n];
                }
            }
            for (int n = 0; n < 4; n++)
                bgra[n] = Convert.ToByte(bgra[n] / (width * height));
            Color color = Color.FromArgb(bgra[3], bgra[2], bgra[1], bgra[0]);
            return new AcrylicBrush()
            {
                BackgroundSource = AcrylicBackgroundSource.Backdrop,
                FallbackColor = color,
                TintOpacity = 0.75,
                TintColor = color
            };
        }
    }
}

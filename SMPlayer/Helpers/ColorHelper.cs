using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace SMPlayer
{
    public static class ColorHelper
    {
        private struct Size
        {
            public uint Width, Height;
            public Size(uint Width, uint Height)
            {
                this.Width = Width;
                this.Height = Height;
            }
        }

        public static async Task<Brush> GetThumbnailMainColor(StorageFile Thumbnail)
        {
            var decoder = await BitmapDecoder.CreateAsync(await Thumbnail.OpenAsync(FileAccessMode.Read));
            uint width = decoder.PixelWidth, height = decoder.PixelHeight;
            Size[] TargetPositions = { new Size(width / 4, height / 4), new Size(width * 3 / 4, height / 4), 
                                       new Size(width / 4, height * 3 / 4), new Size(width * 3 / 4, height * 3 / 4) };
            byte[] bgra = { 0, 0, 0, 255 };
            foreach (var size in TargetPositions)
            {
                bgra = await GetPixelData(decoder, size.Width, size.Height);
                byte b = bgra[0], g = bgra[1], r = bgra[2];
                if (b < 200 && g < 200 && r < 200) break;
            }
            Color color = Color.FromArgb(bgra[3], bgra[2], bgra[1], bgra[0]);
            return new AcrylicBrush()
            {
                BackgroundSource = AcrylicBackgroundSource.Backdrop,
                FallbackColor = color,
                TintOpacity = 0.75,
                TintColor = color
            };
        }
        public static async Task<Brush> GetThumbnailMainColorCommon(StorageFile Thumbnail)
        {
            var decoder = await BitmapDecoder.CreateAsync(await Thumbnail.OpenAsync(FileAccessMode.Read));
            uint width = decoder.PixelWidth, height = decoder.PixelHeight;
            Dictionary<byte, int>[] dict = new Dictionary<byte, int>[4];
            for (int n = 0; n < 4; n++) dict[n] = new Dictionary<byte, int>();
            for (uint i = 0; i < width - 1; i++)
            {
                for (uint j = 0; j < height - 1; j++)
                {
                    var bytes = await GetPixelData(decoder, i, j);
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

        private static async Task<byte[]> GetPixelData(BitmapDecoder decoder, uint x, uint y)
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

        public static async Task<Brush> GetThumbnailMainColorAverage(StorageFile Thumbnail)
        {
            var decoder = await BitmapDecoder.CreateAsync(await Thumbnail.OpenAsync(FileAccessMode.Read));
            uint width = decoder.PixelWidth, height = decoder.PixelHeight;
            byte[] bgra = new byte[4];
            for (uint i = 0; i < width - 1; i++)
            {
                for (uint j = 0; j < height - 1; j++)
                {
                    var bytes = await GetPixelData(decoder, i, j);
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

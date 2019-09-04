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
    class ColorHelper
    {
        public static async Task<Brush> GetThumbnailMainColor(StorageFile Thumbnail)
        {
            var decoder = await BitmapDecoder.CreateAsync(await Thumbnail.OpenAsync(FileAccessMode.Read));
            uint width = decoder.PixelWidth, height = decoder.PixelHeight;
            var data = await decoder.GetPixelDataAsync(BitmapPixelFormat.Bgra8,
                                                       BitmapAlphaMode.Straight,
                                                       new BitmapTransform()
                                                       {
                                                           Bounds = new BitmapBounds() { Width = 1, Height = 1, X = width / 4, Y = height / 4 }
                                                       },
                                                       ExifOrientationMode.IgnoreExifOrientation,
                                                       ColorManagementMode.DoNotColorManage);
            var bgra = data.DetachPixelData();
            Color color = Color.FromArgb(bgra[3], bgra[2], bgra[1], bgra[0]);
            return new AcrylicBrush()
            {
                BackgroundSource = AcrylicBackgroundSource.HostBackdrop,
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
                    var data = await decoder.GetPixelDataAsync(BitmapPixelFormat.Bgra8,
                                                               BitmapAlphaMode.Straight,
                                                               new BitmapTransform()
                                                               {
                                                                   Bounds = new BitmapBounds() { Width = 1, Height = 1, X = i, Y = j }
                                                               },
                                                               ExifOrientationMode.IgnoreExifOrientation,
                                                               ColorManagementMode.DoNotColorManage);
                    var bytes = data.DetachPixelData();
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
                BackgroundSource = AcrylicBackgroundSource.HostBackdrop,
                FallbackColor = color,
                TintOpacity = 0.75,
                TintColor = color
            };
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
                    var data = await decoder.GetPixelDataAsync(BitmapPixelFormat.Bgra8,
                                                               BitmapAlphaMode.Straight,
                                                               new BitmapTransform()
                                                               {
                                                                   Bounds = new BitmapBounds() { Width = 1, Height = 1, X = i, Y = j }
                                                               },
                                                               ExifOrientationMode.IgnoreExifOrientation,
                                                               ColorManagementMode.DoNotColorManage);
                    var bytes = data.DetachPixelData();
                    for (int n = 0; n < 4; n++)
                        bgra[n] += bytes[n];
                }
            }
            for (int n = 0; n < 4; n++)
                bgra[n] = Convert.ToByte(bgra[n] / (width * height));
            Color color = Color.FromArgb(bgra[3], bgra[2], bgra[1], bgra[0]);
            return new AcrylicBrush()
            {
                BackgroundSource = AcrylicBackgroundSource.HostBackdrop,
                FallbackColor = color,
                TintOpacity = 0.75,
                TintColor = color
            };
        }
    }
}

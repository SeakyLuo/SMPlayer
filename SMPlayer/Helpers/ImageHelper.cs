using SkiaSharp;
using SkiaSharp.QrCode.Image;
using SMPlayer.Models;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace SMPlayer
{
    public static class ImageHelper
    {
        public static bool NeedsLoading(FrameworkElement sender, EffectiveViewportChangedEventArgs args)
        {
            return args.BringIntoViewDistanceY < sender.ActualHeight * 1.5 && sender.DataContext != null;
        }

        private static readonly LoadingCache<string, BitmapImage> imageCache = new LoadingCache<string, BitmapImage>(30, TimeUnit.Minute)
        {
            MaxSize = 300
        };

        public static async Task<BitmapImage> LoadImage(string path)
        {
            BitmapImage image = imageCache.Get(path);
            if (image == null)
            {
                image = await Helper.GetThumbnailAsync(path);
                imageCache.PutIfNonNull(path, image);
            }
            return image;
        }

        public static async Task<BitmapImage> LoadImage(IMusicable music)
        {
            return await LoadImage(music.ToMusic().Path);
        }

        public static async Task<StorageItemThumbnail> LoadThumbnail(string path)
        {
            StorageItemThumbnail thumbnail = await Helper.GetStorageItemThumbnailAsync(path);
            return thumbnail.IsThumbnail() ? thumbnail : null;
        }

        public static void CacheImage(string path, BitmapImage item)
        {
            imageCache.Put(path, item);
        }

        public static void CacheImage(string path, StorageItemThumbnail item)
        {
            imageCache.Put(path, item.ToBitmapImage());
        }

        public static bool DeleteImageCache(string path)
        {
            return imageCache.Delete(path);
        }

        public static async Task<BitmapImage> GenQRCode(string str)
        {
            var qrCode = new QrCode(str, new Vector2Slim(512, 512), SKEncodedImageFormat.Png);
            string filename = Path.Combine(Helper.TempFolder.Path, $"QRCode_{Helper.TimeStamp}.png");
            using (var output = new FileStream(filename, FileMode.OpenOrCreate))
            {
                qrCode.GenerateImage(output);
            };
            StorageFile file = await StorageFile.GetFileFromPathAsync(filename);
            using (IRandomAccessStreamWithContentType ras = await file.OpenReadAsync())
            {
                BitmapImage dst = new BitmapImage();
                await dst.SetSourceAsync(ras);
                return dst;
            };
        }
    }
}

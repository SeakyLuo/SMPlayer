using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;

namespace SMPlayer
{
    public static class Helper
    {
        public static string DefaultAlbumCoverPath = "ms-appx:///Assets/music.png";
        public static BitmapImage DefaultAlbumCover = new BitmapImage(new Uri(DefaultAlbumCoverPath));
        public static async Task<BitmapImage> GetThumbnail(StorageFile file)
        {
            BitmapImage bitmapImage = DefaultAlbumCover;
            if (file != null)
            {
                using (var thumbnail = await file.GetThumbnailAsync(ThumbnailMode.MusicView, 300))
                {
                    if (thumbnail != null && thumbnail.Type == ThumbnailType.Image)
                    {
                        bitmapImage = new BitmapImage();
                        bitmapImage.SetSource(thumbnail);
                    }
                }
            }
            return bitmapImage;
        }
        public static void SetBackButtonVisibility(AppViewBackButtonVisibility visibility)
        {
            var currentView = SystemNavigationManager.GetForCurrentView();
            currentView.AppViewBackButtonVisibility = visibility;
        }
    }
}
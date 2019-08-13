using SMPlayer.Models;
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
        public static StorageFolder CurrentMusicFolder;
        public static Music CurrentMusic;
        public static int CurrentMusicIndex = -1;
        public static List<Music> CurrentPlayList;
        public static string DefaultAlbumCoverPath = "ms-appx:///Assets/music.png";
        public static BitmapImage DefaultAlbumCover = new BitmapImage(new Uri(DefaultAlbumCoverPath));
        public static string ThumbnailNotFoundPath = "ms-appx:///Assets/gray_music.png";
        public static BitmapImage ThumbnailNotFoundImage = new BitmapImage(new Uri(ThumbnailNotFoundPath));
        private static Random random = new Random();

        public static Music PrevMusic()
        {
            CurrentMusicIndex -= 1;
            if (CurrentMusicIndex < 0)
            {
                if (Settings.settings.Mode == PlayMode.Shuffle)
                {
                    CurrentMusic = null;
                    ShuffleCurrentPlayList();
                    CurrentMusicIndex = 0;
                }
                else
                {
                    CurrentMusicIndex += CurrentPlayList.Count;
                }
            }
            return CurrentPlayList[CurrentMusicIndex];
        }

        public static Music NextMusic()
        {
            CurrentMusicIndex += 1;
            if (CurrentMusicIndex >= CurrentPlayList.Count)
            {
                if (Settings.settings.Mode == PlayMode.Shuffle)
                {
                    CurrentMusic = null;
                    ShuffleCurrentPlayList();
                }
                CurrentMusicIndex = 0;
            }
            return CurrentPlayList[CurrentMusicIndex];
        }

        public static void ShuffleCurrentPlayList()
        {
            int n = CurrentPlayList.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                Music value = CurrentPlayList[k];
                CurrentPlayList[k] = CurrentPlayList[n];
                CurrentPlayList[n] = value;
            }
        }

        public static async Task<BitmapImage> GetThumbnail(string path, bool withDefault = true)
        {
            return await GetThumbnail(await StorageFile.GetFileFromPathAsync(path), withDefault);
        }
        public static async Task<BitmapImage> GetThumbnail(StorageFile file, bool withDefault = true)
        {
            BitmapImage bitmapImage = null;
            if (withDefault) bitmapImage = DefaultAlbumCover;
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
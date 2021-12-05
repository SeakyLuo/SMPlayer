using Microsoft.Toolkit.Uwp.Notifications;
using SMPlayer.Controls;
using SMPlayer.Helpers;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Globalization;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace SMPlayer
{
    public static class Helper
    {
        public const string LogoPath = "ms-appx:///Assets/monotone_no_bg.png";
        public static string TimeStamp { get => DateTime.Now.ToString("yyyyMMdd_HHmmss"); }
        public static string TimeStampInMills { get => DateTime.Now.ToString("yyyyMMdd_HHmmss.fff"); }

        public static StorageFolder CurrentFolder, ThumbnailFolder, TempFolder;
        public static StorageFolder LocalFolder { get => ApplicationData.Current.LocalFolder; }

        public static ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();
        public static ResourceLoader MessageResourceLoader = ResourceLoader.GetForCurrentView("Messages");
        public const string Language_CN = "zh-Hans-CN", Language_EN = "en-US";
        public static Language CurrentLanguage
        {
            get
            {
                if (currentLanguage != null) return currentLanguage;
                IReadOnlyList<string> languages = Windows.System.UserProfile.GlobalizationPreferences.Languages;
                string language = languages.FirstOrDefault(i => SupportedLanguages.Contains(i));
                return currentLanguage = new Language(language ?? Language_EN);
            }
        }
        private static readonly List<string> SupportedLanguages = new List<string>() { Language_EN, Language_CN };
        private static Language currentLanguage;
        public static ResourceLoader TextResourceLoader = ResourceLoader.GetForCurrentView("Texts");
        public static string AppVersion
        {
            get
            {
                Package package = Package.Current;
                PackageId packageId = package.Id;
                PackageVersion version = packageId.Version;
                return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
            }
        }

        private static List<Music> NotFoundHistory = new List<Music>();
        private static readonly Random random = new Random();

        public static int RandRange(int min, int max)
        {
            return random.Next(min, max);
        }

        public static int RandRange(int max)
        {
            return random.Next(max);
        }

        public static async Task Init()
        {
            var initDefaultImage = MusicImage.DefaultImage; // 静态资源只有在用到的时候才会被加载，所以做一个无用的声明强行用到
            TempFolder = await FileHelper.CreateFolder("Temp");
            await ClearBackups();
        }

        public static async Task ClearBackups(int maxBackups = 5)
        {
            await ClearTempFiles(maxBackups);
            await Log.ClearLogFiles(maxBackups);
        }

        private static async Task ClearTempFiles(int maxBackups = 5)
        {
            if (TempFolder == null) return;
            var files = await TempFolder.GetFilesAsync();
            await ClearBackup(files, SettingsHelper.JsonFilename, maxBackups);
            await ClearBackup(files, SettingsHelper.NewFilename, maxBackups);
            await ClearBackup(files, MusicPlayer.JsonFilename, maxBackups);
            await ClearBackup(files, AlbumsPage.JsonFilename, maxBackups);
        }

        public static async Task ClearBackup(IEnumerable<StorageFile> files, string prefix, int maxBackups)
        {
            var targets = files.Where(f => f.Name.StartsWith(prefix)).OrderBy(f => f.DateCreated);
            foreach (var file in targets.Take(targets.Count() - maxBackups))
                try { await file.DeleteAsync(); } catch (FileNotFoundException) { }
        }

        public static string ConvertBytes(ulong bytes)
        {
            ulong kb = bytes >> 10;
            return kb < 1024 ? kb + " KB" : Math.Round((double)kb / 1024, 2) + " MB";
        }
        public static IMainPageContainer GetMainPageContainer()
        {
            return (Window.Current?.Content as Frame)?.Content as IMainPageContainer;
        }
        public static void ShowNotificationRaw(string message, int duration = 2000)
        {
            GetMainPageContainer()?.ShowNotification(message, duration);
        }
        public static void ShowNotification(string message, int duration = 2000)
        {
            GetMainPageContainer()?.ShowNotification(LocalizeMessage(message), duration);
        }
        public static void ShowCancelableNotification(string message, Action cancel, int duration = 5000)
        {
            GetMainPageContainer()?.ShowUndoNotification(LocalizeMessage(message), cancel, duration);
        }
        public static void ShowCancelableNotificationRaw(string message, Action cancel, int duration = 5000)
        {
            GetMainPageContainer()?.ShowUndoNotification(message, cancel, duration);
        }
        public static void ShowMusicNotFoundNotification(string music, int duration = 5000)
        {
            GetMainPageContainer().ShowNotification(LocalizeMessage("MusicNotFound", music), duration);
        }
        public static void ShowPathNotFoundNotification(string path, int duration = 5000)
        {
            GetMainPageContainer().ShowNotification(LocalizeMessage("PathNotFound", FileHelper.GetParentPath(path)), duration);
        }
        public static void ShowAddMusicResultNotification(AddMusicResult result, Music target = null)
        {
            if (result.IsFailed)
            {
                IMainPageContainer container = GetMainPageContainer();
                int duration = 5000;
                if (result.FailCount > 1) container.ShowNotification(LocalizeMessage("MusicsNotFound", result.FailCount), duration);
                else
                {
                    if (target == null || !result.Failed.Contains(target)) target = result.Failed[0];
                    if (!NotFoundHistory.Contains(target))
                    {
                        NotFoundHistory.Add(target);
                        container.ShowNotification(LocalizeMessage("MusicNotFound", target.Name), duration);
                    }
                }
            }
        }
        public static void ShowMultiSelectCommandBar(MultiSelectCommandBarOption option)
        {
            GetMainPageContainer()?.ShowMultiSelectCommandBar(option);
        }

        public static string Localize(string resource)
        {
            return LocalizeHelper(resource, resourceLoader);
        }

        public static string LocalizeMessage(string resource, params object[] args)
        {
            return LocalizeWithLoader(MessageResourceLoader, resource, args);
        }

        public static string LocalizeText(string resource, params object[] args)
        {
            return LocalizeWithLoader(TextResourceLoader, resource, args);
        }

        private static string LocalizeWithLoader(ResourceLoader loader, string resource, params object[] args)
        {
            var str = LocalizeHelper(resource, loader);
            try
            {
                return string.Format(str, args);
            }
            catch (FormatException)
            {
                // No Format Args Provided
                return str;
            }
        }

        private static string LocalizeHelper(string resource, ResourceLoader loader)
        {
            if (string.IsNullOrEmpty(resource)) return resource;
            var str = loader.GetString(resource.Replace(":", "%3A"));
            return string.IsNullOrEmpty(str) ? resource : str;
        }

        public static string GetPlaylistName(string Name, int index)
        {
            return LocalizeMessage("PlaylistName", Name, index);
        }

        public static string GetVolumeIcon(double volume)
        {
            if (volume == 0) return "\uE992";
            if (volume < 34) return "\uE993";
            if (volume < 67) return "\uE994";
            return "\uE995";
        }

        public static async Task<BitmapImage> GetThumbnailAsync(string path)
        {
            using (var thumbnail = await GetStorageItemThumbnailAsync(path))
            {
                if (thumbnail.IsThumbnail())
                    return thumbnail.ToBitmapImage();
            }
            return null;
        }
        public static async Task<StorageItemThumbnail> GetStorageItemThumbnailAsync(Music music, uint size = 300)
        {
            return await GetStorageItemThumbnailAsync(music.Path, size);
        }
        public static async Task<StorageItemThumbnail> GetStorageItemThumbnailAsync(string path, uint size = 300)
        {
            try
            {
                var file = await StorageFile.GetFileFromPathAsync(path);
                return await file.GetThumbnailAsync(ThumbnailMode.MusicView, size);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (Exception)
            {
                // 拒绝访问
                return null;
            }
        }

        public static async Task<StorageFolder> GetThumbnailFolder()
        {
            if (ThumbnailFolder == null)
            {
                ThumbnailFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Thumbnails", CreationCollisionOption.OpenIfExists);
                foreach (var item in await ThumbnailFolder.GetFilesAsync())
                    await item.DeleteAsync();
            }
            return ThumbnailFolder;
        }

        public static bool IsFullyVisible(this FrameworkElement sender, FrameworkElement parent)
        {
            Rect rect = new Rect(0.0, 0.0, sender.ActualWidth, sender.ActualHeight);
            Rect bounds = parent.TransformToVisual(sender).TransformBounds(new Rect(0.0, 0.0, parent.ActualWidth, parent.ActualHeight));
            return rect.Contains(new Point(bounds.Left, bounds.Top)) && rect.Contains(new Point(bounds.Right, bounds.Bottom));
        }

        public static bool IsPartiallyVisible(this FrameworkElement sender, FrameworkElement parent)
        {
            Rect rect = new Rect(0.0, 0.0, sender.ActualWidth, sender.ActualHeight);
            Rect bounds = parent.TransformToVisual(sender).TransformBounds(new Rect(0.0, 0.0, parent.ActualWidth, parent.ActualHeight));
            return rect.Contains(new Point(bounds.Left, bounds.Top)) || rect.Contains(new Point(bounds.Right, bounds.Bottom));
        }

        public static bool ToBeVisible(this FrameworkElement sender, FrameworkElement parent)
        {
            return false;
            //Rect rect = new Rect(0.0, 0.0, sender.ActualWidth, sender.ActualHeight);
            //Rect bounds = parent.TransformToVisual(sender).TransformBounds(new Rect(0.0, 0.0, parent.ActualWidth, parent.ActualHeight));
            //return rect.Contains(new Point(bounds.Left, bounds.Top)) || rect.Contains(new Point(bounds.Right, bounds.Bottom));
        }
    }
    public enum ExecutionStatus
    {
        Running = 0, Done = 1, Ready = 2, Break = 3
    }

    public interface IMainPageContainer
    {
        void ShowNotification(string message, int duration = 2000);
        void ShowUndoNotification(string message, Action undo, int duration = 5000);
        void ShowLocalizedNotification(string message, int duration = 2000);
        void ShowMultiSelectCommandBar(MultiSelectCommandBarOption option = null);
        void HideMultiSelectCommandBar();
        void SetMultiSelectListener(IMultiSelectListener listener = null);
        MediaElement GetMediaElement();
        MultiSelectCommandBar GetMultiSelectCommandBar();
        MediaControl GetMediaControl();
    }
}
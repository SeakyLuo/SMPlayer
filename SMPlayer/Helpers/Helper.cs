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
        public const string StringConcatenationFlag = "+++++";
        public const string LogoPath = "ms-appx:///Assets/monotone_no_bg.png";
        public static string TimeStamp { get => DateTime.Now.ToString("yyyyMMdd_HHmmss"); }
        public static string TimeStampInMills { get => DateTime.Now.ToString("yyyyMMdd_HHmmss.fff"); }

        public static StorageFolder CurrentFolder, ThumbnailFolder, SecondaryTileFolder, TempFolder, LogFolder;
        public static StorageFolder LocalFolder { get => ApplicationData.Current.LocalFolder; }

        public static TileUpdater tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
        public static ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();
        public static ResourceLoader MessageResourceLoader = ResourceLoader.GetForCurrentView("Messages");
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

        private const string ExceptionLogFileNamePrefix = "ExceptionLog", LogFileNamePrefix = "Log";

        private static List<Music> NotFoundHistory = new List<Music>();
        private static Random random = new Random();

        public static void Log(string text)
        {
            Logger(LogFileNamePrefix, text);
        }

        public static void LogException(Exception e)
        {
            string text = "Exception: " + e.ToString() + "\nSource: " + e.Source + "\nStackTrace: " + e.StackTrace +
                          "\nTargetSite: " + e.TargetSite + "\nHelpLink: " + e.HelpLink + "\nHResult" + e.HResult;
            Logger(ExceptionLogFileNamePrefix, text);
        }

        private static async void Logger(string prefix, string text)
        {
            if (LogFolder == null) LogFolder = await CreateFolder("Logs");
            StorageFile log;
            string name = prefix + TimeStampInMills;
            int index = 0;
            while (true)
            {
                try
                {
                    log = await LogFolder.CreateFileAsync(name + ".txt");
                    break;
                }
                catch (Exception)
                {
                    // 当文件已存在时，无法创建该文件
                    name += "_" + index++;
                }
            }
            await FileIO.WriteTextAsync(log, text);
        }

        public static void Timer(Action action, string funcName = null)
        {
            DateTime start = DateTime.Now;
            action.Invoke();
            Debug.WriteLine("Time of " + (funcName ?? action.Method.Name) + ": " + (DateTime.Now - start).TotalMilliseconds);
        }

        public static int RandRange(int min, int max)
        {
            return random.Next(min, max);
        }

        public static int RandRange(int max)
        {
            return random.Next(max);
        }

        public static T RandItem<T>(this IEnumerable<T> list)
        {
            return list.ElementAt(RandRange(list.Count()));
        }

        public static IEnumerable<T> RandItems<T>(this IEnumerable<T> list, int count)
        {
            int listSize = list.Count();
            if (listSize < count) return list.Shuffle();
            HashSet<int> indices = new HashSet<int>();
            while (indices.Count < count)
            {
                indices.Add(RandRange(listSize));
            }
            return indices.Select(index => list.ElementAt(index));
        }
        public static async Task Init()
        {
            var initDefaultImage = MusicImage.DefaultImage; // 静态资源只有在用到的时候才会被加载，所以做一个无用的声明
            TempFolder = await CreateFolder("Temp");
            if (LogFolder == null) LogFolder = await CreateFolder("Logs");
            await ClearBackups();
        }

        private static async Task<StorageFolder> CreateFolder(string folderName)
        {
            return await ApplicationData.Current.LocalFolder.CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists);
        }

        public static async Task ClearBackups(int maxBackups = 3)
        {
            await ClearTempFiles(maxBackups);
            await ClearLogFiles(maxBackups);
        }

        private static async Task ClearTempFiles(int maxBackups = 3)
        {
            if (TempFolder == null) return;
            var files = await TempFolder.GetFilesAsync();
            await ClearBackup(files, Settings.JsonFilename, maxBackups);
            await ClearBackup(files, MediaHelper.JsonFilename, maxBackups);
            await ClearBackup(files, MusicLibraryPage.JsonFilename, maxBackups);
            await ClearBackup(files, AlbumsPage.JsonFilename, maxBackups);
        }

        private static async Task ClearLogFiles(int maxBackups = 3)
        {
            var files = await LogFolder.GetFilesAsync();
            await ClearBackup(files, LogFileNamePrefix, maxBackups);
            await ClearBackup(files, ExceptionLogFileNamePrefix, maxBackups);
        }

        private static async Task ClearBackup(IEnumerable<StorageFile> files, string prefix, int maxBackups)
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
        public static async Task<bool> FileNotExist(string path)
        {
            return !await FileExists(path);
        }
        public static async Task<bool> FileExists(string path)
        {
            try
            {
                await StorageFile.GetFileFromPathAsync(path);
                return true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static IMainPageContainer GetMainPageContainer() { return (Window.Current.Content as Frame).Content as IMainPageContainer; }
        public static void ShowNotificationWithoutLocalization(string message, int duration = 2000)
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
        public static void ShowCancelableNotificationWithoutLocalization(string message, Action cancel, int duration = 5000)
        {
            GetMainPageContainer()?.ShowUndoNotification(message, cancel, duration);
        }
        public static void ShowMusicNotFoundNotification(string music, int duration = 5000)
        {
            GetMainPageContainer().ShowNotification(LocalizeMessage("MusicNotFound", music), duration);
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
        public static string CurrentLanguage
        {
            get => new Windows.Globalization.Language(Windows.System.UserProfile.GlobalizationPreferences.Languages[0]).DisplayName;
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

        public static async Task UpdateTile(StorageItemThumbnail itemThumbnail, Music music)
        {
            if (music == null) return;
            string uri = MusicImage.DefaultImagePath;
            if (itemThumbnail != null)
            {
                try
                {
                    var file = await itemThumbnail.SaveAsync(await GetThumbnailFolder(), string.IsNullOrEmpty(music.Album) ? music.Name : music.Album, true);
                    uri = file.Path;
                }
                catch (Exception)
                {
                    
                }
            }
            var tileContent = new TileContent()
            {
                Visual = new TileVisual()
                {
                    TileSmall = new TileBinding()
                    {
                        Branding = TileBranding.None,
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage() { Source = uri }
                        }
                    },
                    TileMedium = new TileBinding()
                    {
                        Branding = TileBranding.Name,
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage() { Source = uri },
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = music.Name,
                                    HintStyle = AdaptiveTextStyle.Body,
                                    HintWrap = true
                                }
                            }
                        }
                    },
                    TileWide = new TileBinding()
                    {
                        Branding = TileBranding.Name,
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage() { Source = uri },
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = music.Name,
                                    HintStyle = AdaptiveTextStyle.Base,
                                    HintWrap = true
                                },
                                new AdaptiveText()
                                {
                                    Text = music.Artist,
                                    HintStyle = AdaptiveTextStyle.Caption,
                                    HintWrap = true
                                }
                            }
                        }
                    },
                    TileLarge = new TileBinding()
                    {
                        Branding = TileBranding.Name,
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage() { Source = uri },
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = music.Album,
                                    HintStyle = AdaptiveTextStyle.Caption
                                },
                                new AdaptiveText()
                                {
                                    Text = music.Name,
                                    HintStyle = AdaptiveTextStyle.Subtitle,
                                    HintWrap = true
                                },
                                new AdaptiveText()
                                {
                                    Text = music.Artist,
                                    HintStyle = AdaptiveTextStyle.Base
                                }
                            }
                        }
                    }
                }
            };
            try
            {
                // Create the tile notification
                var tileNotification = new TileNotification(tileContent.GetXml());

                // And send the notification to the primary tile
                tileUpdater.Update(tileNotification);
            }
            catch (Exception)
            {
                // ArgumentException: Value does not fall within the expected range.
                // 不知道为什么就变成了null
            }
        }

        public static void ResumeTile()
        {
            var tile = new TileBinding()
            {
                DisplayName = Windows.ApplicationModel.Package.Current.DisplayName,
                Branding = TileBranding.Name,
                Content = new TileBindingContentAdaptive()
                {
                    BackgroundImage = new TileBackgroundImage()
                    {
                        Source = MusicImage.DefaultImagePath
                    },
                }
            };
            var tileContent = new TileContent()
            {
                Visual = new TileVisual()
                {
                    TileMedium = tile,
                    TileWide = tile,
                    TileLarge = tile
                }
            };

            // Create the tile notification
            var tileNotification = new TileNotification(tileContent.GetXml());

            // And send the notification to the primary tile
            tileUpdater.Update(tileNotification);
        }
        public static async Task<StorageFolder> GetSecondaryTileFolder()
        {
            return SecondaryTileFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("SecondaryTiles", CreationCollisionOption.OpenIfExists);
        }
        public static async Task<bool> PinToStartAsync(Playlist playlist, bool isPlaylist)
        {
            var tilename = playlist.Name;
            var tileid = FormatTileId(playlist, isPlaylist);
            var filename = tileid + ".png";
            var uri = MusicImage.DefaultImagePath;
            if (playlist.DisplayItem.Source != null)
            {
                if (await (await GetSecondaryTileFolder()).Contains(filename))
                {
                    uri = "ms-appdata:///local/SecondaryTiles/" + WebUtility.UrlEncode(filename);
                }
                else
                {
                    var thumbnail = await ImageHelper.LoadThumbnail(playlist.DisplayItem.Source);
                    if (thumbnail.IsThumbnail())
                    {
                        await thumbnail.SaveAsync(SecondaryTileFolder, tileid);
                        uri = "ms-appdata:///local/SecondaryTiles/" + WebUtility.UrlEncode(filename);
                    }
                    else
                    {
                        uri = LogoPath;
                    }
                }
            }
            var tile = new SecondaryTile(tileid, tilename, isPlaylist.ToString(), new Uri(uri), TileSize.Default);
            tile.VisualElements.ShowNameOnSquare150x150Logo = tile.VisualElements.ShowNameOnSquare310x310Logo = tile.VisualElements.ShowNameOnWide310x150Logo = true;
            if (SecondaryTile.Exists(tileid)) await tile.RequestDeleteAsync();
            else await tile.RequestCreateAsync();
            return SecondaryTile.Exists(tileid);
        }
        public static string FormatTileId(Playlist playlist, bool isPlaylist)
        {
            var tilename = playlist.Name;
            return isPlaylist ? tilename : tilename + StringConcatenationFlag + playlist.Artist;
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
        MultiSelectCommandBar GetMultiSelectCommandBar();
    }
}
using Microsoft.Toolkit.Uwp.Notifications;
using SMPlayer.Controls;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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
        public const string ToastTaskName = "ToastBackgroundTask";
        public const string LogoPath = "ms-appx:///Assets/monotone_no_bg.png";
        public const string ToastTagPaused = "SMPlayerMediaToastTagPaused", ToastTagPlaying = "SMPlayerMediaToastTagPlaying", ToastGroup = "SMPlayerMediaToastGroup";
        public static string NoLyricsAvailable { get => LocalizeMessage("NoLyricsAvailable"); }
        public static string TimeStamp { get => DateTime.Now.ToString("yyyyMMdd_HHmmss"); }

        public static StorageFolder CurrentFolder, ThumbnailFolder, SecondaryTileFolder, TempFolder;
        public static StorageFolder LocalFolder { get => ApplicationData.Current.LocalFolder; }
        public static ToastNotification Toast;
        public static ToastNotifier toastNotifier = ToastNotificationManager.CreateToastNotifier();
        public static ToastAudio SlientToast = new ToastAudio() { Silent = true };
        public static TileUpdater tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
        public static ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();
        public static ResourceLoader MessageResourceLoader = ResourceLoader.GetForCurrentView("Messages");

        private static string Lyrics = "";
        private static List<Music> NotFoundHistory = new List<Music>();
        private static Random random = new Random();

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
            var initDefaultImage = MusicImage.DefaultImage;
            TempFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Temp", CreationCollisionOption.OpenIfExists);
            await ClearBackups();
        }

        public static async Task ClearBackups(int maxBackups = 3)
        {
            var files = await TempFolder.GetFilesAsync();
            var settings = files.Where(f => f.Name.StartsWith(Settings.JsonFilename)).OrderBy(f => f.DateCreated);
            var mediaHelper = files.Where(f => f.Name.StartsWith(MediaHelper.JsonFilename)).OrderBy(f => f.DateCreated);
            var musicLibrary = files.Where(f => f.Name.StartsWith(MusicLibraryPage.JsonFilename)).OrderBy(f => f.DateCreated);
            var albums = files.Where(f => f.Name.StartsWith(AlbumsPage.JsonFilename)).OrderBy(f => f.DateCreated);
            foreach (var file in settings.Take(settings.Count() - maxBackups))
                try { await file.DeleteAsync(); } catch (FileNotFoundException) { }
            foreach (var file in mediaHelper.Take(mediaHelper.Count() - maxBackups))
                try { await file.DeleteAsync(); } catch (FileNotFoundException) { }
            foreach (var file in musicLibrary.Take(musicLibrary.Count() - maxBackups))
                try { await file.DeleteAsync(); } catch (FileNotFoundException) { }
            foreach (var file in albums.Take(albums.Count() - maxBackups))
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
            var str = LocalizeHelper(resource, MessageResourceLoader);
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

        public static string GetPlaylistName(string Name, int index)
        {
            return LocalizeMessage("PlaylistName", Name, index);
        }

        private static string LocalizeHelper(string resource, ResourceLoader loader)
        {
            if (string.IsNullOrEmpty(resource)) return resource;
            var str = loader.GetString(resource.Replace(":", "%3A"));
            return string.IsNullOrEmpty(str) ? resource : str;
        }
        public static string GetVolumeIcon(double volume)
        {
            if (volume == 0) return "\uE992";
            if (volume < 34) return "\uE993";
            if (volume < 67) return "\uE994";
            return "\uE995";
        }
        public static string GetLyricByTime(double time)
        {
            if (string.IsNullOrEmpty(Lyrics)) return NoLyricsAvailable;
            return "";
        }

        public static async Task<BitmapImage> GetThumbnailAsync(Music music, bool withDefault = true)
        {
            return await GetThumbnailAsync(music.Path, withDefault);
        }
        public static async Task<BitmapImage> GetThumbnailAsync(string path, bool withDefault = true)
        {
            using (var thumbnail = await GetStorageItemThumbnailAsync(path))
            {
                if (thumbnail.IsThumbnail())
                    return thumbnail.GetBitmapImage();
            }
            return withDefault ? MusicImage.DefaultImage : null;
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
        private static async void ShowToast(Music music, MediaPlaybackState state)
        {
            if (Window.Current.Visible) return;
            ShowToast status = Settings.settings.Toast;
            if (status == Models.ShowToast.Never) return;
            ToastButton controlButton = null;
            string toastTag = "";
            switch (state)
            {
                case MediaPlaybackState.Paused:
                    controlButton = new ToastButton(LocalizeMessage("Play"), "Play") { ActivationType = ToastActivationType.Background };
                    toastTag = ToastTagPlaying;
                    break;
                case MediaPlaybackState.Playing:
                    controlButton = new ToastButton(LocalizeMessage("Pause"), "Pause") { ActivationType = ToastActivationType.Background };
                    toastTag = ToastTagPaused;
                    break;
            }
            var toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText() { Text = music.GetToastText() },
                            new AdaptiveProgressBar()
                            {
                                Value = new BindableProgressBarValue("MediaControlPosition"),
                                ValueStringOverride = MusicDurationConverter.ToTime(music.Duration),
                                Title = new BindableString("Lyrics"),
                                Status = new BindableString("MediaControlPositionTime")
                            }
                        }
                    }
                },
                Actions = new ToastActionsCustom()
                {
                    Buttons =
                    {
                        controlButton, new ToastButton(LocalizeMessage("Next"), "Next"){ ActivationType = ToastActivationType.Background }
                    },
                },
                ActivationType = ToastActivationType.Background,
                Launch = "Launch",
                Audio = SlientToast,
                Scenario = status == Models.ShowToast.Always || state == MediaPlaybackState.Paused ? ToastScenario.Reminder : ToastScenario.Default
            };

            // Create the toast notification
            Toast = new ToastNotification(toastContent.GetXml())
            {
                Tag = toastTag,
                Group = ToastGroup,
                Data = new NotificationData() { SequenceNumber = 0 },
                ExpiresOnReboot = true
            };
            if (status == Models.ShowToast.Always)
            {
                Lyrics = await music.GetLyricsAsync();
                Toast.Data.Values["Lyrics"] = GetLyricByTime(MediaHelper.Position);
            }
            else
            {
                Toast.ExpirationTime = DateTime.Now.AddSeconds(Math.Min(10, music.Duration));
                Toast.Data.Values["Lyrics"] = "";
            }
            Toast.Data.Values["MediaControlPosition"] = MediaHelper.Progress.ToString();
            Toast.Data.Values["MediaControlPositionTime"] = MusicDurationConverter.ToTime(MediaHelper.Position);
            try
            {
                toastNotifier.Show(Toast);
            }
            catch (Exception)
            {
                // 通知已发布
            }
        }

        public static void ShowPlayToast(Music music)
        {
            HideToast();
            ShowToast(music, MediaPlaybackState.Paused);
        }

        public static void ShowPauseToast(Music music)
        {
            HideToast();
            ShowToast(music, MediaPlaybackState.Playing);
        }
        public static void UpdateToast()
        {
            if (!MediaHelper.IsPlaying) return;
            // Create NotificationData and make sure the sequence number is incremented
            // since last update, or assign 0 for updating regardless of order
            var data = new NotificationData { SequenceNumber = 0 };
            data.Values["MediaControlPosition"] = MediaHelper.Progress.ToString();
            data.Values["MediaControlPositionTime"] = MusicDurationConverter.ToTime(MediaHelper.Position);
            data.Values["Lyrics"] = Settings.settings.Toast == Models.ShowToast.Always ? GetLyricByTime(MediaHelper.Position) : "";

            // Update the existing notification's data by using tag/group
            toastNotifier.Update(data, ToastTagPaused, ToastGroup);
        }

        public static void HideToast()
        {
            if (Toast != null)
            {
                try
                {
                    toastNotifier.Hide(Toast);
                }
                catch (Exception)
                {
                    // 通知已经隐藏。
                }
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
                    var thumbnail = await GetStorageItemThumbnailAsync(playlist.DisplayItem.Source.Path);
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
        MultiSelectCommandBar GetMultiSelectCommandBar();
    }
}
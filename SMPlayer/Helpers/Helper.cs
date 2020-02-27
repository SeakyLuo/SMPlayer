using Microsoft.Toolkit.Uwp.Notifications;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Graphics.Imaging;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace SMPlayer
{
    public static class Helper
    {
        public const string StringConcatenationFlag = "+++++";
        public const string ToastTaskName = "ToastBackgroundTask";
        public const string LogoPath = "ms-appx:///Assets/monotone_no_bg.png",
                            DefaultAlbumCoverPath = "ms-appx:///Assets/monotone_bg_wide.png",
                            ThumbnailNotFoundPath = "ms-appx:///Assets/colorful_bg_wide.png";
        public const string ToastTagPaused = "SMPlayerMediaToastTagPaused", ToastTagPlaying = "SMPlayerMediaToastTagPlaying", ToastGroup = "SMPlayerMediaToastGroup";
        public static string NoLyricsAvailable { get => LocalizeMessage("NoLyricsAvailable"); }

        public static StorageFolder CurrentFolder, ThumbnailFolder, SecondaryTileFolder;
        public static BitmapImage DefaultAlbumCover = new BitmapImage(new Uri(DefaultAlbumCoverPath));
        public static BitmapImage ThumbnailNotFoundImage = new BitmapImage(new Uri(ThumbnailNotFoundPath));
        public static ToastNotification Toast;
        public static ToastNotifier toastNotifier = ToastNotificationManager.CreateToastNotifier();
        public static ToastAudio SlientToast = new ToastAudio() { Silent = true };
        public static TileUpdater tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
        public static ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();
        public static ResourceLoader MessageResourceLoader = ResourceLoader.GetForCurrentView("Messages");

        private static string Lyrics = "";
        private static List<Music> NotFoundHistory = new List<Music>();

        public static string ConvertBytes(ulong bytes)
        {
            ulong kb = bytes >> 10;
            return kb < 1024 ? kb + " KB" : Math.Round((double)kb / 1024, 2) + " MB";
        }

        public static ObservableCollection<T> SetTo<T>(this ObservableCollection<T> dst, IEnumerable<T> src)
        {
            var temp = src.ToList();
            if (dst == null) dst = new ObservableCollection<T>();
            else dst.Clear();
            foreach (var item in temp) dst.Add(item);
            return dst;
        }

        public static NotificationContainer GetNotificationContainer() { return (Window.Current.Content as Frame).Content as NotificationContainer; }
        public static void ShowNotification(string message, int duration = 2000)
        {
            GetNotificationContainer()?.ShowNotification(LocalizeMessage(message), duration);
        }
        public static void ShowAddMusicResultNotification(string music)
        {
            GetNotificationContainer().ShowNotification(LocalizeMessage("MusicNotFound", music), 5000);
        }
        public static void ShowAddMusicResultNotification(AddMusicResult result, Music target = null)
        {
            if (result.IsFailed)
            {
                NotificationContainer container = GetNotificationContainer();
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
            return string.Format(str, args);
        }

        private static string LocalizeHelper(string resource, ResourceLoader loader)
        {
            if (string.IsNullOrEmpty(resource)) return resource;
            var str = loader.GetString(resource.Replace(":", "%3A"));
            return string.IsNullOrEmpty(str) ? resource : str;
        }
        public static bool IsMusicFile(this StorageFile file)
        {
            return file.FileType.EndsWith("mp3");
        }
        public static async Task<bool> Contains(this StorageFolder folder, string name)
        {
            try
            {
                return await folder.TryGetItemAsync(name) != null;
            }
            catch (ArgumentException)
            {
                // Value does not fall within the expected range.
                // e.g. ?.png
                return false;
            }
        }

        public static void SetToolTip(this DependencyObject obj, string tooltip, bool localize = true)
        {
            if (obj == null) return;
            ToolTipService.SetToolTip(obj, localize ? Localize(tooltip) : tooltip);
        }
        public static string GetVolumeIcon(double volume)
        {
            if (volume == 0) return "\uE992";
            if (volume < 34) return "\uE993";
            if (volume < 67) return "\uE994";
            return "\uE995";
        }
        public static bool SameAs(this IEnumerable<Music> list1, IEnumerable<Music> list2)
        {
            return list1.Count() == list2.Count() && list1.Zip(list2, (m1, m2) => m1.Equals(m2)).All(res => res);
        }

        public static string GetLyricByTime(double time)
        {
            if (string.IsNullOrEmpty(Lyrics)) return NoLyricsAvailable;
            return "";
        }

        public static async Task<BitmapImage> GetThumbnailAsync(Music music, bool withDefault = true)
        {
            using (var thumbnail = await GetStorageItemThumbnailAsync(music))
            {
                if (thumbnail.IsThumbnail())
                    return thumbnail.GetBitmapImage();
            }
            return withDefault ? DefaultAlbumCover : null;
        }

        public static bool IsThumbnail(this StorageItemThumbnail thumbnail)
        {
            return thumbnail != null && thumbnail.Type == ThumbnailType.Image;
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
        }
        public static BitmapImage GetBitmapImage(this StorageItemThumbnail thumbnail)
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.SetSource(thumbnail);
            return bitmapImage;
        }

        public static async Task<Brush> GetDisplayColor(this StorageItemThumbnail thumbnail)
        {
            return await ColorHelper.GetThumbnailMainColor(thumbnail.CloneStream());
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
            string uri = itemThumbnail == null ? DefaultAlbumCoverPath : (await itemThumbnail.SaveAsync(await GetThumbnailFolder(), string.IsNullOrEmpty(music.Album) ? music.Name : music.Album, true)).Path;
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
                        Source = DefaultAlbumCoverPath
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
            var uri = DefaultAlbumCoverPath;
            if (playlist.DisplayItem.Source != null)
            {
                if (!await (await GetSecondaryTileFolder()).Contains(filename))
                    await (await GetStorageItemThumbnailAsync(playlist.DisplayItem.Source.Path)).SaveAsync(SecondaryTileFolder, tileid);
                uri = "ms-appdata:///local/SecondaryTiles/" + WebUtility.UrlEncode(filename);
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

        public static async Task<StorageFile> SaveAsync(this StorageItemThumbnail thumbnail, StorageFolder folder, string name, bool encode = false)
        {
            using (var stream = thumbnail.CloneStream())
            {
                var decoder = await BitmapDecoder.CreateAsync(stream);
                var softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                var filename = name + ".png";
                if (encode) filename = WebUtility.UrlEncode(filename);
                bool notExists = !await folder.Contains(filename);
                var file = await folder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
                if (notExists)
                {
                    using (var filestream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, filestream);
                        encoder.SetSoftwareBitmap(softwareBitmap);
                        await encoder.FlushAsync();
                    }
                }
                return file;
            }
        }
    }
    public enum ExecutionStatus
    {
        Running = 0, Done = 1, Ready = 2, Break = 3
    }

    public interface NotificationContainer
    {
        void ShowNotification(string message, int duration = 1500);
    }
}
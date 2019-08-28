using Microsoft.Toolkit.Uwp.Notifications;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace SMPlayer
{
    public static class Helper
    {
        public const string ToastTaskName = "ToastBackgroundTask";
        public const string LogoPath = "ms-appx:///Asserts/monotone_no_bg.png";
        public const string DefaultAlbumCoverPath = "ms-appx:///Assets/monotone_bg_wide.png";
        public const string ThumbnailNotFoundPath = "ms-appx:///Assets/colorful_bg_wide.png";
        public const string ToastTag = "SMPlayerMediaToastTag";
        public const string ToastGroup = "SMPlayerMediaToastGroup";
        public const string NoLyricsAvailable = "No Lyrics Available";

        public static StorageFolder CurrentFolder, ThumbnailFolder;
        public static StorageFile Thumbnail;
        public static BitmapImage DefaultAlbumCover = new BitmapImage(new Uri(DefaultAlbumCoverPath));
        public static BitmapImage ThumbnailNotFoundImage = new BitmapImage(new Uri(ThumbnailNotFoundPath));
        public static ToastNotifier toastNotifier = ToastNotificationManager.CreateToastNotifier();
        public static ToastAudio SlientToast = new ToastAudio() { Silent = true };
        public static TileUpdater tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
        public static SolidColorBrush WhiteBrush = new SolidColorBrush(Colors.White);
        public static SolidColorBrush RedBrush = new SolidColorBrush(Colors.Red);
        public static SolidColorBrush WhiteSmokeBrush = new SolidColorBrush(Colors.WhiteSmoke);
        public static SolidColorBrush BlackBrush = new SolidColorBrush(Colors.Black);
        private static string Lyrics = "";
        public static SolidColorBrush GetHighlightBrush() { return new SolidColorBrush(Settings.settings.ThemeColor); }

        public static string GetVolumeIcon(double volume)
        {
            if (volume == 0) return "\uE992";
            if (volume < 34) return "\uE993";
            if (volume < 67) return "\uE994";
            return "\uE995";
        }

        public static bool SamePlayList(IEnumerable<Music> list1, IEnumerable<Music> list2)
        {
            return list1.Count() == list2.Count() && list1.Zip(list2, (m1, m2) => m1.Equals(m2)).All((res) => res);
        }

        public static string GetLyricByTime(double time)
        {
            if (string.IsNullOrEmpty(Lyrics)) return NoLyricsAvailable;
            return "";
        }

        public static async Task<BitmapImage> GetThumbnail(Music music, bool withDefault = true)
        {
            try
            {
                return await GetThumbnail(await CurrentFolder.GetFileAsync(music.GetShortPath()), withDefault);
            }
            catch (FileNotFoundException)
            {
                return withDefault ? DefaultAlbumCover : null;
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

        public static async Task<Brush> GetThumbnailMainColor()
        {
            var decoder = await BitmapDecoder.CreateAsync(await Thumbnail.OpenAsync(FileAccessMode.Read));
            uint width = decoder.PixelWidth, height = decoder.PixelHeight;
            var data = await decoder.GetPixelDataAsync(BitmapPixelFormat.Bgra8,
                                                       BitmapAlphaMode.Straight,
                                                       new BitmapTransform()
                                                       {
                                                           Bounds = new BitmapBounds() { Width = 1, Height = 1, X = width / 2, Y = width / 2 }
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
        public static void SetBackButtonVisibility(AppViewBackButtonVisibility visibility)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = visibility;
        }
        public static async void ShowToast(Music music)
        {
            ShowNotification status = Settings.settings.Notification;
            if (status == ShowNotification.Never) return;
            var toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = string.IsNullOrEmpty(music.Artist) ?
                                       string.IsNullOrEmpty(music.Album) ? music.Name : string.Format("{0} - {1}", music.Name, music.Album) :
                                       string.Format("{0} - {1}", music.Name, string.IsNullOrEmpty(music.Artist) ? music.Album : music.Artist)
                            },
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
                        new ToastButton("Pause", "Pause"){ ActivationType = ToastActivationType.Background },
                        new ToastButton("Next", "Next"){ ActivationType = ToastActivationType.Background }
                    },
                },
                ActivationType = ToastActivationType.Background,
                Launch = "Launch",
                Audio = SlientToast,
                Scenario = status == ShowNotification.Always ? ToastScenario.Reminder : ToastScenario.Default
            };

            // Create the toast notification
            var toast = new ToastNotification(toastContent.GetXml())
            {
                ExpirationTime = DateTime.Now.AddSeconds(music.Duration),
                Tag = ToastTag,
                Group = ToastGroup,
                Data = new NotificationData() { SequenceNumber = 0 }
            };
            toast.Data.Values["MediaControlPosition"] = "0";
            toast.Data.Values["MediaControlPositionTime"] = "0:00";
            toast.Data.Values["Lyrics"] = string.IsNullOrEmpty(Lyrics = await music.GetLyrics()) ? NoLyricsAvailable : "" ;

            toastNotifier.Show(toast);
        }
        public static void UpdateToast()
        {
            // Create NotificationData and make sure the sequence number is incremented
            // since last update, or assign 0 for updating regardless of order
            var data = new NotificationData { SequenceNumber = 0 };
            data.Values["MediaControlPosition"] = (MediaHelper.Position / 100).ToString();
            data.Values["MediaControlPositionTime"] = MusicDurationConverter.ToTime(MediaHelper.Position);
            data.Values["Lyrics"] = GetLyricByTime(MediaHelper.Position);

            // Update the existing notification's data by using tag/group
            toastNotifier.Update(data, ToastTag, ToastGroup);
        }

        public static async Task SaveThumbnail(UIElement image)
        {
            var _bitmap = new RenderTargetBitmap();
            await _bitmap.RenderAsync(image);
            var pixels = await _bitmap.GetPixelsAsync();
            while (true)
            {
                try
                {
                    Thumbnail = await ThumbnailFolder.CreateFileAsync($@"{Guid.NewGuid()}.png", CreationCollisionOption.ReplaceExisting);
                    break;
                }
                catch (FileLoadException)
                {
                    continue;
                }
            }
            using (IRandomAccessStream stream = await Thumbnail.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                byte[] bytes = pixels.ToArray();
                encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                                     BitmapAlphaMode.Ignore,
                                     (uint)_bitmap.PixelWidth,
                                     (uint)_bitmap.PixelHeight,
                                     200,
                                     200,
                                     bytes);

                await encoder.FlushAsync();
            }
        }

        public static void UpdateTile(Music music)
        {
            string uri = Thumbnail.Path;
            var tileContent = new TileContent()
            {
                Visual = new TileVisual()
                {
                    //TileSmall = new TileBinding()
                    //{
                    //    Branding = TileBranding.None,
                    //    Content = new TileBindingContentAdaptive()
                    //    {
                    //        BackgroundImage = new TileBackgroundImage()
                    //        {
                    //            Source = uri
                    //        }
                    //    }
                    //},
                    TileMedium = new TileBinding()
                    {
                        Branding = TileBranding.Name,
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = uri
                            },
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
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = uri
                            },
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
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = uri
                            },
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

            // Create the tile notification
            var tileNotification = new TileNotification(tileContent.GetXml());

            // And send the notification to the primary tile
            tileUpdater.Update(tileNotification);
        }

        public static void ResumeTile()
        {
            var tile = new TileBinding()
            {
                DisplayName = "SMPlayer",
                Branding = TileBranding.NameAndLogo,
                Content = new TileBindingContentAdaptive()
                {
                    BackgroundImage = new TileBackgroundImage()
                    {
                        Source = LogoPath
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
    }
}
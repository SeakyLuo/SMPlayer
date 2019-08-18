using Microsoft.Toolkit.Uwp.Notifications;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class NowPlayingPage : Page, MediaControlListener
    {

        private bool ShouldUpdate = true, SliderClicked = false;
        private static Dictionary<string, MusicControlListener> MusicControlListeners = new Dictionary<string, MusicControlListener>();
        public NowPlayingPage()
        {
            this.InitializeComponent();
            Helper.SetBackButtonVisibility(AppViewBackButtonVisibility.Visible);
            SystemNavigationManager.GetForCurrentView().BackRequested += (s, e) =>
            {
                if (Frame.CanGoBack) Frame.GoBack();
            };
            MediaControl.AddMediaControlListener(this as MediaControlListener);
            SetMusic(Settings.settings.LastMusic);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Settings settings = Settings.settings;
            VolumeButton.Content = GetVolumeIcon(settings.Volume);
            VolumeSlider.Value = settings.Volume * 100;
        }

        public async void SetMusic(Music music)
        {
            if (music == null) return;
            var thumbnail = await Helper.GetThumbnail(MediaControl.CurrentMusic);
            LargeAlbumCover.Source = thumbnail;
            AlbumCover.Source = thumbnail;
            TitleTextBlock.Text = music.Name;
            ArtistTextBlock.Text = music.Artist;
            MediaSlider.Maximum = music.Duration;
            RightTimeTextBlock.Text = MusicDurationConverter.ToTime(music.Duration);
            if (music.Favorite) LikeMusic(false);
            else DislikeMusic(false);
            if (Settings.settings.LastMusic != music)
            {
                Settings.settings.LastMusic = music;
                Settings.Save();
            }
            foreach (var listener in MusicControlListeners.Values)
                listener.MusicSet(music);
        }

        public void SetMusicAndPlay(Music music)
        {
            SetMusic(music);
            MediaControl.SetMusic(music);
            PlayMusic();
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            RepeatButton.IsChecked = false;
            RepeatOneButton.IsChecked = false;
            MediaControl.SetMode((bool)ShuffleButton.IsChecked ? PlayMode.Shuffle : PlayMode.Once);
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            ShuffleButton.IsChecked = false;
            RepeatOneButton.IsChecked = false;
            MediaControl.SetMode((bool)RepeatButton.IsChecked ? PlayMode.Repeat : PlayMode.Once);
        }

        private void RepeatOneButton_Click(object sender, RoutedEventArgs e)
        {
            ShuffleButton.IsChecked = false;
            RepeatButton.IsChecked = false;
            MediaControl.SetMode((bool)RepeatOneButton.IsChecked ? PlayMode.RepeatOne : PlayMode.Once);
        }

        public void PlayMusic()
        {
            if (MediaControl.CurrentMusic == null)
            {
                if (MusicLibraryPage.AllSongs.Count == 0) return;
                MediaControl.SetMusic(MediaControl.CurrentPlayList[0]);
            }
            PlayButtonIcon.Glyph = "\uE769";
            MediaControl.Play();
        }

        public void PauseMusic()
        {
            if (MediaControl.CurrentMusic == null) return;
            PlayButtonIcon.Glyph = "\uE768";
            MediaControl.Pause();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlayButtonIcon.Glyph == "\uE768") PlayMusic();
            else PauseMusic();
        }

        private void VolumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (VolumeButton.Content.ToString() == "\uE74F")
            {
                MediaControl.Player.IsMuted = false;
                VolumeButton.Content = GetVolumeIcon(VolumeSlider.Value);
            }
            else
            {
                MediaControl.Player.IsMuted = true;
                VolumeButton.Content = "\uE74F";
            }
        }

        public static string GetVolumeIcon(double volume)
        {
            if (volume == 0) return "\uE992";
            if (volume < 34) return "\uE993";
            if (volume < 67) return "\uE994";
            return "\uE995";
        }

        private void VolumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            MediaControl.Player.IsMuted = false;
            double volume = e.NewValue / 100;
            MediaControl.Player.Volume = volume;
            Settings.settings.Volume = volume;
            VolumeButton.Content = GetVolumeIcon(e.NewValue);
        }

        public void LikeMusic(bool isClick = true)
        {
            LikeButtonIcon.Glyph = "\uEB52";
            LikeButtonIcon.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
            if (isClick) SetMusicFavorite(true);
        }

        public void DislikeMusic(bool isClick = true)
        {
            LikeButtonIcon.Glyph = "\uEB51";
            LikeButtonIcon.Foreground = new SolidColorBrush(Windows.UI.Colors.Black);
            if (isClick) SetMusicFavorite(false);
        }

        public static void AddMusicControlListener(string name, MusicControlListener listener)
        {
            MusicControlListeners[name] = listener;
        }

        private void SetMusicFavorite(bool favorite)
        {
            Music before = new Music(MediaControl.CurrentMusic);
            MediaControl.CurrentMusic.Favorite = favorite;
            foreach (var listener in MusicControlListeners.Values)
                listener.MusicModified(before, MediaControl.CurrentMusic);
        }

        private void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            if (LikeButtonIcon.Glyph == "\uEB51") LikeMusic();
            else DislikeMusic();
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (MediaSlider.Value > 5)
            {
                MediaSlider.Value = 0;
                MediaControl.Position = 0;
            }
            else
            {
                MediaControl.PrevMusic();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            MediaControl.NextMusic();
        }

        private void MediaSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            int newValue = (int)MediaSlider.Value, diff = newValue - (int)MediaControl.Position;
            SliderClicked = diff != 1 && diff != 0;
            Debug.WriteLine("ValueChanged To " + MusicDurationConverter.ToTime(newValue));
            LeftTimeTextBlock.Text = MusicDurationConverter.ToTime(newValue);
        }

        private void MediaSlider_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            MediaControl.Position = MediaSlider.Value;
            ShouldUpdate = true;
            Debug.WriteLine("Completed");
        }

        private void MediaSlider_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            ShouldUpdate = false;
            Debug.WriteLine("Started");
        }
        private void MediaSlider_ManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        {
            Debug.WriteLine("Starting");
            if (SliderClicked)
            {
                MediaControl.Position = MediaSlider.Value;
            }
        }

        public void Tick()
        {
            if (ShouldUpdate)
            {
                MediaSlider.Value = MediaControl.Position;
            }
        }
        private void NotifyListeners(Music before, Music after)
        {
            foreach (var listener in MusicControlListeners.Values)
                listener.MusicModified(before, after);
        }

        public async void MusicSwitching(Music current, Music next)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                if (current == null) return;
                if (!Window.Current.Visible) ShowToast(next);
                SetMusic(next);
                Music after = new Music(current);
                after.PlayCount += 1;
                NotifyListeners(current, after);
            });
        }

        public async void MediaEnded()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                MediaControl.Timer.Stop();
                if (Settings.settings.Mode == PlayMode.Once)
                {
                    PlayButtonIcon.Glyph = "\uE768";
                    MediaSlider.Value = 0;
                }
            });
        }

        public void ShowToast(Music music)
        {
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
                                Value = new BindableProgressBarValue("MediaControl.Position"),
                                ValueStringOverride = MusicDurationConverter.ToTime(music.Duration),
                                Title = "Lyrics To Be Implemented",
                                Status = MusicDurationConverter.ToTime(MediaControl.Position)
                            }
                        }
                    }
                },
                Actions = new ToastActionsCustom()
                {
                    Buttons =
                    {
                        new ToastButton("Pause", "Pause"),
                        new ToastButton("Next", "Next")
                    },
                },
                Launch = "Launch",
                Audio = Helper.SlientToast,
            };

            // Create the toast notification
            var toast = new ToastNotification(toastContent.GetXml())
            {
                ExpirationTime = DateTime.Now.AddSeconds(music.Duration),
            };
            toast.Activated += Toast_Activated;
            Helper.ShowToast(toast);
        }

        private async void Toast_Activated(ToastNotification sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                switch ((args as ToastActivatedEventArgs).Arguments)
                {
                    case "Next":
                        MediaControl.NextMusic();
                        break;
                    case "Pause":
                        PauseMusic();
                        break;
                    default:
                        break;
                }
            });
        }
    }
}

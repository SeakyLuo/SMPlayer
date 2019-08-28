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
    public sealed partial class NowPlayingFullPage : Page
    {
        private bool ShouldUpdate = true, SliderClicked = false;
        private static Dictionary<string, MusicControlListener> MusicControlListeners = new Dictionary<string, MusicControlListener>();
        public NowPlayingFullPage()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().BackRequested += (s, e) =>
            {
                if (Frame.CanGoBack)
                    Frame.GoBack();
            };
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Settings settings = Settings.settings;
            VolumeButton.Content = GetVolumeIcon(settings.Volume);
            VolumeSlider.Value = settings.Volume * 100;
        }

        public async void SetMusic(Music music)
        {
            if (music == null) return;
            var thumbnail = await Helper.GetThumbnail(MediaHelper.CurrentMusic);
            LargeAlbumCover.Source = thumbnail;
            AlbumCover.Source = thumbnail;
            TitleTextBlock.Text = music.Name;
            ArtistTextBlock.Text = music.Artist;
            MediaSlider.Maximum = music.Duration;
            RightTimeTextBlock.Text = MusicDurationConverter.ToTime(music.Duration);
            if (music.Favorite) LikeMusic(false);
            else DislikeMusic(false);
            Settings.settings.LastMusic = music;
        }

        public void SetMusicAndPlay(Music music)
        {
            SetMusic(music);
            MediaHelper.MoveToMusic(music);
            PlayMusic();
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            RepeatButton.IsChecked = false;
            RepeatOneButton.IsChecked = false;
            MediaHelper.SetMode((bool)ShuffleButton.IsChecked ? PlayMode.Shuffle : PlayMode.Once);
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            ShuffleButton.IsChecked = false;
            RepeatOneButton.IsChecked = false;
            MediaHelper.SetMode((bool)RepeatButton.IsChecked ? PlayMode.Repeat : PlayMode.Once);
        }

        private void RepeatOneButton_Click(object sender, RoutedEventArgs e)
        {
            ShuffleButton.IsChecked = false;
            RepeatButton.IsChecked = false;
            MediaHelper.SetMode((bool)RepeatOneButton.IsChecked ? PlayMode.RepeatOne : PlayMode.Once);
        }

        public void PlayMusic()
        {
            if (MediaHelper.CurrentMusic == null)
            {
                if (MusicLibraryPage.AllSongs.Count == 0) return;
                MediaHelper.MoveToMusic(MediaHelper.CurrentPlayList[0]);
            }
            PlayButton.Content = "\uE769";
            MediaHelper.Play();
        }

        public void PauseMusic()
        {
            if (MediaHelper.CurrentMusic == null) return;
            PlayButton.Content = "\uE768";
            MediaHelper.Pause();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlayButton.Content.ToString() == "\uE768") PlayMusic();
            else PauseMusic();
        }

        private void VolumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (VolumeButton.Content.ToString() == "\uE74F")
            {
                MediaHelper.Player.IsMuted = false;
                VolumeButton.Content = GetVolumeIcon(VolumeSlider.Value);
            }
            else
            {
                MediaHelper.Player.IsMuted = true;
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
            MediaHelper.Player.IsMuted = false;
            double volume = e.NewValue / 100;
            MediaHelper.Player.Volume = volume;
            Settings.settings.Volume = volume;
            VolumeButton.Content = GetVolumeIcon(e.NewValue);
        }

        public void LikeMusic(bool isClick = true)
        {
            LikeButton.Content = "\uEB52";
            LikeButton.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
            if (isClick) SetMusicFavorite(true);
        }

        public void DislikeMusic(bool isClick = true)
        {
            LikeButton.Content = "\uEB51";
            LikeButton.Foreground = new SolidColorBrush(Windows.UI.Colors.Black);
            if (isClick) SetMusicFavorite(false);
        }

        public static void AddMusicControlListener(string name, MusicControlListener listener)
        {
            MusicControlListeners[name] = listener;
        }

        private void SetMusicFavorite(bool favorite)
        {
            Music before = new Music(MediaHelper.CurrentMusic);
            MediaHelper.CurrentMusic.Favorite = favorite;
            foreach (var listener in MusicControlListeners.Values)
                listener.MusicModified(before, MediaHelper.CurrentMusic);
        }

        private void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            if (LikeButton.Content.ToString() == "\uEB51") LikeMusic();
            else DislikeMusic();
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (MediaSlider.Value > 5)
            {
                MediaSlider.Value = 0;
                MediaHelper.Position = 0;
            }
            else
            {
                MediaHelper.PrevMusic();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            MediaHelper.NextMusic();
        }

        private void MediaSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            int newValue = (int)MediaSlider.Value, diff = newValue - (int)MediaHelper.Position;
            SliderClicked = diff != 1 && diff != 0;
            Debug.WriteLine("ValueChanged To " + MusicDurationConverter.ToTime(newValue));
            LeftTimeTextBlock.Text = MusicDurationConverter.ToTime(newValue);
        }

        private void MediaSlider_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            MediaHelper.Position = MediaSlider.Value;
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
                MediaHelper.Position = MediaSlider.Value;
            }
        }

        public void Tick()
        {
            if (ShouldUpdate)
            {
                MediaSlider.Value = MediaHelper.Position;
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
                if (!Window.Current.Visible) Helper.ShowToast(next);
                SetMusic(next);
                Music after = new Music(current);
                after.PlayCount += 1;
                NotifyListeners(current, after);
            });
        }

        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MiniPlayButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CarouselButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SavePlaylistButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MusicInfoButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ClearNowPlayingButton_Click(object sender, RoutedEventArgs e)
        {

        }

        public async void MediaEnded()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                MediaHelper.Timer.Stop();
                if (Settings.settings.Mode == PlayMode.Once)
                {
                    PlayButton.Content = "\uE768";
                    MediaSlider.Value = 0;
                }
            });
        }
    }
}

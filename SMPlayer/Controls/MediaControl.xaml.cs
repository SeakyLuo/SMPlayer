using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SMPlayer
{
    public sealed partial class MediaControl : UserControl, MusicSwitchingListener, MediaControlListener, AfterPathSetListener
    {
        public enum MediaControlMode
        {
            Main = 0, Full = 1
        }
        private MediaControlMode mode;
        public MediaControlMode Mode
        {
            get => mode;
            set
            {
                switch (value)
                {
                    case MediaControlMode.Main:
                        MainMediaControlGrid.Visibility = Visibility.Visible;
                        FullMediaControlGrid.Visibility = Visibility.Collapsed;
                        break;
                    case MediaControlMode.Full:
                        MainMediaControlGrid.Visibility = Visibility.Collapsed;
                        FullMediaControlGrid.Visibility = Visibility.Visible;
                        break;
                    default:
                        return;
                }
                mode = value;
            }
        }
        public Image AlbumCover
        {
            get
            {
                switch (mode)
                {
                    case MediaControlMode.Main:
                        return MainAlbumCover;
                    case MediaControlMode.Full:
                        return FullAlbumCover;
                    default:
                        return null;
                }
            }
        }
        public TextBlock TitleTextBlock
        {
            get
            {
                switch (mode)
                {
                    case MediaControlMode.Main:
                        return MainTitleTextBlock;
                    case MediaControlMode.Full:
                        return FullTitleTextBlock;
                    default:
                        return null;
                }
            }
        }
        public TextBlock ArtistTextBlock
        {
            get
            {
                switch (mode)
                {
                    case MediaControlMode.Main:
                        return MainArtistTextBlock;
                    case MediaControlMode.Full:
                        return FullArtistTextBlock;
                    default:
                        return null;
                }
            }
        }
        public Button PlayButton
        {
            get
            {
                switch (mode)
                {
                    case MediaControlMode.Main:
                        return MainPlayButton;
                    case MediaControlMode.Full:
                        return FullPlayButton;
                    default:
                        return null;
                }
            }
        }
        public TextBlock LeftTimeTextBlock
        {
            get
            {
                switch (mode)
                {
                    case MediaControlMode.Main:
                        return MainLeftTimeTextBlock;
                    case MediaControlMode.Full:
                        return FullLeftTimeTextBlock;
                    default:
                        return null;
                }
            }
        }
        public TextBlock RightTimeTextBlock
        {
            get
            {
                switch (mode)
                {
                    case MediaControlMode.Main:
                        return MainRightTimeTextBlock;
                    case MediaControlMode.Full:
                        return FullRightTimeTextBlock;
                    default:
                        return null;
                }
            }
        }
        public Slider MediaSlider
        {
            get
            {
                switch (mode)
                {
                    case MediaControlMode.Main:
                        return MainMediaSlider;
                    case MediaControlMode.Full:
                        return FullMediaSlider;
                    default:
                        return null;
                }
            }
        }
        public Button VolumeButton
        {
            get
            {
                switch (mode)
                {
                    case MediaControlMode.Main:
                        return MainVolumeButton;
                    case MediaControlMode.Full:
                        return FullVolumeButton;
                    default:
                        return null;
                }
            }
        }
        public Slider VolumeSlider
        {
            get
            {
                switch (mode)
                {
                    case MediaControlMode.Main:
                        return MainVolumeSlider;
                    case MediaControlMode.Full:
                        return FullVolumeSlider;
                    default:
                        return null;
                }
            }
        }
        public Button LikeButton
        {
            get
            {
                switch (mode)
                {
                    case MediaControlMode.Main:
                        return MainLikeButton;
                    case MediaControlMode.Full:
                        return FullLikeButton;
                    default:
                        return null;
                }
            }
        }
        public ToggleButton ShuffleButton
        {
            get
            {
                switch (mode)
                {
                    case MediaControlMode.Main:
                        return MainShuffleButton;
                    case MediaControlMode.Full:
                        return FullShuffleButton;
                    default:
                        return null;
                }
            }
        }
        public ToggleButton RepeatButton
        {
            get
            {
                switch (mode)
                {
                    case MediaControlMode.Main:
                        return MainRepeatButton;
                    case MediaControlMode.Full:
                        return FullRepeatButton;
                    default:
                        return null;
                }
            }
        }
        public ToggleButton RepeatOneButton
        {
            get
            {
                switch (mode)
                {
                    case MediaControlMode.Main:
                        return MainRepeatOneButton;
                    case MediaControlMode.Full:
                        return FullRepeatOneButton;
                    default:
                        return null;
                }
            }
        }
        private ToolTip MediaControlToolTip = new ToolTip();
        private ToolTip ShuffleToolTip = new ToolTip();
        private ToolTip RepeatOneToolTip = new ToolTip();
        private ToolTip RepeatToolTip = new ToolTip();

        private bool ShouldUpdate = true, SliderClicked = false;
        private static List<MusicControlListener> MusicControlListeners = new List<MusicControlListener>();
        private static List<MusicRequestListener> MusicRequestListeners = new List<MusicRequestListener>();

        public MediaControl()
        {
            this.InitializeComponent();
            MediaHelper.MusicSwitchingListeners.Add(this as MusicSwitchingListener);
            MediaHelper.MediaControlListeners.Add(this as MediaControlListener);
            SettingsPage.AddAfterPathSetListener(this as AfterPathSetListener);
        }

        public void Update()
        {
            SetMusic(MediaHelper.CurrentMusic);
            if (MediaHelper.IsPlaying) PlayMusic();
            else PauseMusic();

            Settings settings = Settings.settings;
            VolumeButton.Content = Helper.GetVolumeIcon(settings.Volume);
            VolumeSlider.Value = settings.Volume * 100;
            SetPlayMode(settings.Mode);
        }

        public async void SetMusic(Music music)
        {
            if (music == null) { AlbumCover.Source = Helper.DefaultAlbumCover; return; };
            AlbumCover.Source = await Helper.GetThumbnail(music);
            TitleTextBlock.Text = music.Name;
            ArtistTextBlock.Text = music.Artist;
            FullAlbumTextBlock.Text = music.Album;
            MediaSlider.Maximum = music.Duration;
            RightTimeTextBlock.Text = MusicDurationConverter.ToTime(music.Duration);
            if (music.Favorite) LikeMusic(false);
            else DislikeMusic(false);
            if (Mode != MediaControlMode.Full)
                MainMediaControlGrid.Background = await Helper.GetThumbnailMainColor(AlbumCover, true);
            Helper.UpdateTile(music);
            Settings.settings.LastMusic = music;
        }
        public void SetMusicAndPlay(Music music)
        {
            MediaHelper.MoveToMusic(music);
            PlayMusic();
        }
        public static void AddMusicControlListener(MusicControlListener listener)
        {
            MusicControlListeners.Add(listener);
        }

        public static void AddMusicRequestListener(MusicRequestListener listener)
        {
            MusicRequestListeners.Add(listener);
        }
        private void NotifyMusicModifiedListeners(Music before, Music after)
        {
            foreach (var listener in MusicControlListeners)
                listener.MusicModified(before, after);
        }

        public void SetPlayMode(PlayMode mode)
        {
            switch (mode)
            {
                case PlayMode.Once:
                    SetShuffle(false);
                    break;
                case PlayMode.Repeat:
                    RepeatButton.IsChecked = true;
                    SetRepeat(true);
                    break;
                case PlayMode.RepeatOne:
                    RepeatOneButton.IsChecked = true;
                    SetRepeatOne(true);
                    break;
                case PlayMode.Shuffle:
                    ShuffleButton.IsChecked = true;
                    SetShuffle(true);
                    break;
                default:
                    break;
            }
        }

        public void SetToolTip(DependencyObject control, string content)
        {
            MediaControlToolTip.Content = content;
            ToolTipService.SetToolTip(control, MediaControlToolTip);
        }

        public void SetMusicGridInfoTapped(Action<object, TappedRoutedEventArgs> action)
        {
            MainMusicInfoGrid.Tapped += (sender, e) => action(sender, e);
        }
        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            SetShuffle((bool)ShuffleButton.IsChecked);
        }
        private void SetToolTips()
        {
            ToolTipService.SetToolTip(ShuffleButton, ShuffleToolTip);
            ToolTipService.SetToolTip(RepeatButton, RepeatToolTip);
            ToolTipService.SetToolTip(RepeatOneButton, RepeatOneToolTip);
        }
        public void SetShuffle(bool isChecked)
        {
            RepeatButton.IsChecked = false;
            RepeatOneButton.IsChecked = false;
            ShuffleToolTip.Content = "Shuffle: " + (isChecked ? "Enabled" : "Disabled");
            RepeatToolTip.Content = "Repeat: Disabled";
            RepeatOneToolTip.Content = "Repeat One: Disabled";
            SetToolTips();
            MediaHelper.SetMode(isChecked ? PlayMode.Shuffle : PlayMode.Once);
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            SetRepeat((bool)RepeatButton.IsChecked);
        }
        public void SetRepeat(bool isChecked)
        {
            ShuffleButton.IsChecked = false;
            RepeatOneButton.IsChecked = false;
            ShuffleToolTip.Content = "Shuffle: Disabled";
            RepeatToolTip.Content = "Repeat: " + (isChecked ? "Enabled" : "Disabled");
            RepeatOneToolTip.Content = "Repeat One: Disabled";
            SetToolTips();
            MediaHelper.SetMode(isChecked ? PlayMode.Repeat : PlayMode.Once);
        }

        private void RepeatOneButton_Click(object sender, RoutedEventArgs e)
        {
            SetRepeatOne((bool)RepeatOneButton.IsChecked);
        }
        public void SetRepeatOne(bool isChecked)
        {
            ShuffleButton.IsChecked = false;
            RepeatButton.IsChecked = false;
            ShuffleToolTip.Content = "Shuffle: Disabled";
            RepeatToolTip.Content = "Repeat: Disabled";
            RepeatOneToolTip.Content = "Repeat One: " + (isChecked ? "Enabled" : "Disabled");
            SetToolTips();
            MediaHelper.SetMode(isChecked ? PlayMode.RepeatOne : PlayMode.Once);
        }

        public void PlayMusic()
        {
            PlayButton.Content = "\uE769";
            SetToolTip(PlayButton, "Pause");
        }

        public void PauseMusic()
        {
            PlayButton.Content = "\uE768";
            SetToolTip(PlayButton, "Play");
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            if (button.Content.ToString() == "\uE768")
            {
                if (MediaHelper.CurrentMusic == null)
                {
                    if (MediaHelper.CurrentPlaylist.Count == 0) return;
                    MediaHelper.MoveToMusic(MediaHelper.CurrentPlaylist[0]);
                }
                MediaHelper.Play();
            }
            else
            {
                MediaHelper.Pause();
            }
        }

        private void VolumeButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            if (button.Content.ToString() == "\uE74F")
            {
                SetToolTip(button, "Mute");
                MediaHelper.Player.IsMuted = false;
                button.Content = Helper.GetVolumeIcon(VolumeSlider.Value);
            }
            else
            {
                SetToolTip(button, "Undo Mute");
                MediaHelper.Player.IsMuted = true;
                button.Content = "\uE74F";
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            MediaHelper.Player.IsMuted = false;
            double volume = e.NewValue / 100;
            MediaHelper.Player.Volume = volume;
            string icon = Helper.GetVolumeIcon(e.NewValue);
            VolumeButton.Content = icon;
            Settings.settings.Volume = volume;
        }

        public void LikeMusic(bool isClick = true)
        {
            LikeButton.Content = "\uEB52";
            LikeButton.Foreground = Helper.RedBrush;
            ToolTipService.SetToolTip(LikeButton, "Undo Like");
            if (isClick) SetMusicFavorite(true);
        }

        public void DislikeMusic(bool isClick = true)
        {
            LikeButton.Content = "\uEB51";
            LikeButton.Foreground = Helper.WhiteBrush;
            ToolTipService.SetToolTip(LikeButton, "Like");
            if (isClick) SetMusicFavorite(false);
        }

        private void SetMusicFavorite(bool favorite)
        {
            Music before = MediaHelper.CurrentMusic.Copy();
            MediaHelper.CurrentMusic.Favorite = favorite;
            NotifyMusicModifiedListeners(before, MediaHelper.CurrentMusic);
        }

        private void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            if (button.Content.ToString() == "\uEB51") LikeMusic();
            else DislikeMusic();
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (MediaHelper.Position > 5)
            {
                MediaHelper.Position = 0;
                MediaSlider.Value = 0;
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
            //Debug.WriteLine("ValueChanged To " + MusicDurationConverter.ToTime(newValue));
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

        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ApplicationView.GetForCurrentView().IsFullScreenMode)
            {
                ApplicationView.GetForCurrentView().ExitFullScreenMode();
                FullScreenItem.Icon = new SymbolIcon(Symbol.FullScreen);
                FullScreenItem.Text = "Full Screen";
                FullScreenButton.Content = "\uE740";
                SetToolTip(FullScreenButton, "Full Screen");
            }
            else
            {
                if (ApplicationView.GetForCurrentView().TryEnterFullScreenMode())
                {
                    FullScreenItem.Icon = new SymbolIcon(Symbol.BackToWindow);
                    FullScreenItem.Text = "Exit Full Screen";
                    FullScreenButton.Content = "\uE73F";
                    SetToolTip(FullScreenButton, "Exit Full Screen");
                }
            }
        }

        private void MiniPlayButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CarouselButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MusicInfoButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var listener in MusicRequestListeners)
                listener.MusicInfoRequested(MediaHelper.CurrentMusic);
        }

        private void LyricsButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var listener in MusicRequestListeners)
                listener.LyricsRequested(MediaHelper.CurrentMusic);
        }
               
        private async void SavePlaylistItem_Click(object sender, RoutedEventArgs e)
        {
            var name = "Now Playing - " + DateTime.Now.ToString("yy/MM/dd");
            int index = Settings.settings.FindNextPlaylistNameIndex(name);
            var defaultName = index == 0 ? name : $"{name} ({index})";
            var listener = new VirtualRenameActionListener() { Data = PlaylistControl.NowPlayingPlaylist };
            var dialog = new RenameDialog(listener, TitleOption.NewPlaylist, defaultName);
            listener.Dialog = dialog;
            await dialog.ShowAsync();
        }

        private void ClearNowPlayingItem_Click(object sender, RoutedEventArgs e)
        {

        }
        public void Tick()
        {
            if (ShouldUpdate)
            {
                MediaSlider.Value = MediaHelper.Position;
            }
            if (!Window.Current.Visible) Helper.UpdateToast();
        }


        private void Played(Music music)
        {
            Music before = music.Copy();
            music.Played();
            NotifyMusicModifiedListeners(before, music);
        }

        public async void MusicSwitching(Music current, Music next, MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                if (reason == MediaPlaybackItemChangedReason.EndOfStream)
                    Played(current);
                next.IsPlaying = true;
                SetMusic(next);
                if (current != null && !Window.Current.Visible) Helper.ShowToast(next);
            });
        }

        public async void MediaEnded()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                MediaHelper.Timer.Stop();
                if (Settings.settings.Mode == PlayMode.Once)
                {
                    PlayButton.Content = "\uE768";
                    MediaSlider.Value = 0;
                }
                Played(MediaHelper.CurrentMusic);
            });
        }

        public void PathSet(string path)
        {
            PauseMusic();
            MediaSlider.Value = 0;
            AlbumCover.Source = Helper.DefaultAlbumCover;
            TitleTextBlock.Text = "";
            ArtistTextBlock.Text = "";
            RightTimeTextBlock.Text = "0:00";
        }

        public void Play()
        {
            PlayMusic();
        }

        public void Pause()
        {
            PauseMusic();
        }
    }

    public interface MusicControlListener
    {
        void MusicModified(Music before, Music after);
    }

    public interface MusicRequestListener
    {
        void MusicInfoRequested(Music music);
        void LyricsRequested(Music music);
    }

    public interface MediaControlContainer
    {
        void PauseMusic();
        void SetShuffle(bool isShuffle);
    }
}

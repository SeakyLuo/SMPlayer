﻿using SMPlayer.Dialogs;
using SMPlayer.Helpers;
using SMPlayer.Interfaces;
using SMPlayer.Models;
using SMPlayer.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Windows.Foundation;
using Windows.Media.Playback;
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using static SQLite.SQLite3;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SMPlayer
{
    public sealed partial class MediaControl : UserControl, IMusicPlayerEventListener, IMusicEventListener
    {
        public enum MediaControlMode
        {
            Main = 0, Full = 1, Mini = 2
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
                        MiniMediaControlGrid.Visibility = Visibility.Collapsed;
                        break;
                    case MediaControlMode.Full:
                        MainMediaControlGrid.Visibility = Visibility.Collapsed;
                        FullMediaControlGrid.Visibility = Visibility.Visible;
                        MiniMediaControlGrid.Visibility = Visibility.Collapsed;
                        break;
                    case MediaControlMode.Mini:
                        MainMediaControlGrid.Visibility = Visibility.Collapsed;
                        FullMediaControlGrid.Visibility = Visibility.Collapsed;
                        MiniMediaControlGrid.Visibility = Visibility.Visible;
                        break;
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
                    case MediaControlMode.Mini:
                        return MiniAlbumCover;
                    default:
                        return null;
                }
            }
        }
        public ScrollingTextBlock TitleTextBlock
        {
            get
            {
                switch (mode)
                {
                    case MediaControlMode.Main:
                        return MainTitleTextBlock;
                    case MediaControlMode.Full:
                        return FullTitleTextBlock;
                    case MediaControlMode.Mini:
                        return MiniTitleTextBlock;
                    default:
                        return null;
                }
            }
        }
        public ScrollingTextBlock ArtistTextBlock
        {
            get
            {
                switch (mode)
                {
                    case MediaControlMode.Main:
                        return MainArtistTextBlock;
                    case MediaControlMode.Full:
                        return FullArtistTextBlock;
                    case MediaControlMode.Mini:
                        return MiniArtistTextBlock;
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
                    case MediaControlMode.Mini:
                        return MiniPlayButton;
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
                    case MediaControlMode.Mini:
                        return MiniMediaSlider;
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
                    case MediaControlMode.Mini:
                        return MiniMoreVolumeButton;
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
                        return IsMinimalMain ? MainMoreVolumeSlider : MainVolumeSlider;
                    case MediaControlMode.Full:
                        return FullVolumeSlider;
                    case MediaControlMode.Mini:
                        return MiniMoreVolumeSlider;
                    default:
                        return null;
                }
            }
        }
        public ToggleButton LikeToggleButton
        {
            get
            {
                switch (mode)
                {
                    case MediaControlMode.Main:
                        return MainLikeToggleButton;
                    case MediaControlMode.Full:
                        return FullLikeToggleButton;
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
                    case MediaControlMode.Mini:
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
                    case MediaControlMode.Mini:
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
                    case MediaControlMode.Mini:
                        return MainRepeatOneButton;
                    case MediaControlMode.Full:
                        return FullRepeatOneButton;
                    default:
                        return null;
                }
            }
        }
        public Button VoiceAssistantButton
        {
            get
            {
                switch (mode)
                {
                    case MediaControlMode.Main:
                        return IsMinimalMain ? MainMediaControlVoiceAssistantButton : MainVoiceAssistantButton;
                    case MediaControlMode.Full:
                        return FullVoiceAssistantButton;
                    case MediaControlMode.Mini:
                        return MainVoiceAssistantButton;
                    default:
                        return null;
                }
            }
        }

        public Button MuteButton
        {
            get
            {
                switch (mode)
                {
                    case MediaControlMode.Main:
                    case MediaControlMode.Full:
                        return MainMoreVolumeButton;
                    case MediaControlMode.Mini:
                        return MiniMoreVolumeButton;
                    default:
                        return null;
                }
            }
        }

        public IconTextButton MoreShuffleButton
        {
            get
            {
                switch (mode)
                {
                    case MediaControlMode.Main:
                    case MediaControlMode.Full:
                        return MainMoreShuffleButton;
                    case MediaControlMode.Mini:
                        return MiniMoreShuffleButton;
                    default:
                        return null;
                }
            }
        }
        public IconTextButton MoreRepeatButton
        {
            get
            {
                switch (mode)
                {
                    case MediaControlMode.Main:
                    case MediaControlMode.Full:
                        return MainMoreRepeatButton;
                    case MediaControlMode.Mini:
                        return MiniMoreRepeatButton;
                    default:
                        return null;
                }
            }
        }
        public IconTextButton MoreRepeatOneButton
        {
            get
            {
                switch (mode)
                {
                    case MediaControlMode.Main:
                    case MediaControlMode.Full:
                        return MainMoreRepeatOneButton;
                    case MediaControlMode.Mini:
                        return MiniMoreRepeatOneButton;
                    default:
                        return null;
                }
            }
        }

        private MusicView CurrentMusic = null;
        private bool wasPlaying = false;
        private static List<IMusicRequestListener> MusicRequestListeners = new List<IMusicRequestListener>();
        private static bool inited = false;
        private bool IsMinimalMain { get => MainMediaControlMoreButton.Visibility == Visibility.Visible; }
        private double MinimalLayoutWidth { get => (double)Resources["MinimalLayoutWidth"]; }
        private DispatcherTimer SliderTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(500) };

        public MediaControl()
        {
            this.InitializeComponent();
            MusicPlayer.InitFinishedListeners.Add(() =>
            {
                AfterLoaded();
                inited = true;
                MainSliderProgressBar.Visibility = Visibility.Collapsed;
                MainMediaSlider.Visibility = Visibility.Visible;
            });
            MusicPlayer.AddMusicPlayerEventListener(this);
            MusicService.AddMusicEventListener(this);
            var left = new KeyboardAccelerator() { Key = Windows.System.VirtualKey.Left };
            left.Invoked += (sender, args) =>
            {
                if (MusicPlayer.CurrentMusic != null)
                    MusicPlayer.Position = Math.Max(MusicPlayer.Position - 5, 0);
            };
            KeyboardAccelerators.Add(left);
            var right = new KeyboardAccelerator() { Key = Windows.System.VirtualKey.Right };
            right.Invoked += (sender, args) =>
            {
                if (MusicPlayer.CurrentMusic != null)
                    MusicPlayer.Position = Math.Min(MusicPlayer.Position + 5, MusicPlayer.CurrentMusic.Duration);
            };
            KeyboardAccelerators.Add(right);
            KeyboardAcceleratorPlacementMode = KeyboardAcceleratorPlacementMode.Hidden;
            SliderTimer.Tick += (s, args) =>
            {
                MediaSlider.Value = MusicPlayer.Position;
            };
            SliderTimer.Start();
        }

        private void AfterLoaded()
        {
            UpdateMusic(MusicPlayer.CurrentMusic);

            if (MusicPlayer.IsPlaying) SetButtonPlaying();
            else SetMusicPause();

            double volume = Settings.settings.Volume * 100;
            VolumeButton.Content = Helper.GetVolumeIcon(volume);
            SetMuted(Settings.settings.IsMuted);
            VolumeSlider.Value = volume;
            SetPlayMode(Settings.settings.Mode);
            if (LeftTimeTextBlock != null) LeftTimeTextBlock.Text = MusicDurationConverter.ToTime(Settings.settings.MusicProgress);

            if (ApplicationView.GetForCurrentView().IsFullScreenMode) SetExitFullScreen();
            else SetFullScreen();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (inited)
            {
                AfterLoaded();
            }
        }

        public async void UpdateMusic(Music music)
        {
            if (music == null)
            {
                ClearMusic();
                return;
            }
            await Helper.RunInMainUIThread(Dispatcher, () =>
            {
                MusicView musicView = music.ToVO();
                MediaSlider.IsEnabled = true;
                TitleTextBlock.Text = music.Name;
                ArtistTextBlock.Text = string.IsNullOrWhiteSpace(music.Artist) ? Helper.LocalizeMessage("UnknownArtist") : music.Artist;

                MediaSlider.Maximum = music.Duration;
                MediaSlider.Value = MusicPlayer.Position;
                if (RightTimeTextBlock != null) RightTimeTextBlock.Text = MusicDurationConverter.ToTime(music.Duration);
                if (LikeToggleButton != null)
                {
                    LikeToggleButton.IsEnabled = true;
                    if (musicView.Favorite = PlaylistService.IsFavorite(music)) LikeMusic();
                    else DislikeMusic();
                }
                SetThumbnail(music);
                MainMusicInfoGrid.SetToolTip(music.Name);
                CurrentMusic = musicView;
            });
        }

        public async void SetThumbnail(Music music)
        {
            using (var thumbnail = await ImageHelper.LoadThumbnail(music.Path))
            {
                AlbumCover.Source = thumbnail.ToBitmapImage() ?? MusicImage.DefaultImage;
                switch (Mode)
                {
                    case MediaControlMode.Main:
                        MainMediaControlGrid.Background = thumbnail == null ? ColorHelper.HighlightBrush : await thumbnail.GetDisplayColor();
                        break;
                    case MediaControlMode.Full:
                        FullAlbumTextBlock.Text = music.Album;
                        break;
                    case MediaControlMode.Mini:
                        break;
                }
                try
                {
                    await TileHelper.UpdateTile(thumbnail, music);
                }
                catch (FileLoadException)
                {
                    // 正在使用此文件。请先关闭文件，然后再继续操作。
                }
                catch (PathTooLongException)
                {
                    // The specified file name or path is too long, or a component of the specified path is too long.
                }
                catch (FileNotFoundException)
                {
                    // System.IO.FileNotFoundException:“文件名、目录名或卷标语法不正确。 (Exception from HRESULT: 0x8007007B)”
                }
            }
        }

        public static void AddMusicRequestListener(IMusicRequestListener listener)
        {
            MusicRequestListeners.Add(listener);
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

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            SetShuffle((bool)ShuffleButton.IsChecked);
            if (MusicPlayer.ShuffleEnabled)
                MusicPlayer.ShuffleOthers();
        }
        private void MoreShuffleButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ShuffleButton.IsChecked = !ShuffleButton.IsChecked;
            ShuffleButton_Click(ShuffleButton, null);
        }

        public void SetShuffle(bool isChecked)
        {
            RepeatButton.IsChecked = false;
            RepeatOneButton.IsChecked = false;
            ShuffleButton.SetToolTip(MoreShuffleButton.Label = Helper.LocalizeMessage("Shuffle: " + (isChecked ? "Enabled" : "Disabled")), false);
            RepeatButton.SetToolTip(MoreRepeatButton.Label = Helper.LocalizeMessage("Repeat: Disabled"), false);
            RepeatOneButton.SetToolTip(MoreRepeatOneButton.Label = Helper.LocalizeMessage("Repeat One: Disabled"), false);
            MoreShuffleButton.IconBackground = isChecked ? ColorHelper.GrayBrush : ColorHelper.TransparentBrush;
            MoreRepeatButton.IconBackground = ColorHelper.TransparentBrush;
            MoreRepeatOneButton.IconBackground = ColorHelper.TransparentBrush;
            var mode = isChecked ? PlayMode.Shuffle : PlayMode.Once;
            if (mode != Settings.settings.Mode) MusicPlayer.PlayMode = mode;
        }
        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            SetRepeat((bool)RepeatButton.IsChecked);
        }
        private void MoreRepeatButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            RepeatButton.IsChecked = !RepeatButton.IsChecked;
            RepeatButton_Click(RepeatButton, null);
        }
        public void SetRepeat(bool isChecked)
        {
            ShuffleButton.IsChecked = false;
            RepeatOneButton.IsChecked = false;
            ShuffleButton.SetToolTip(MoreShuffleButton.Label = Helper.LocalizeMessage("Shuffle: Disabled"), false);
            RepeatButton.SetToolTip(MoreRepeatButton.Label = Helper.LocalizeMessage("Repeat: " + (isChecked ? "Enabled" : "Disabled")), false);
            RepeatOneButton.SetToolTip(MoreRepeatOneButton.Label = Helper.LocalizeMessage("Repeat One: Disabled"), false);
            MoreShuffleButton.IconBackground = ColorHelper.TransparentBrush;
            MoreRepeatButton.IconBackground = isChecked ? ColorHelper.GrayBrush : ColorHelper.TransparentBrush;
            MoreRepeatOneButton.IconBackground = ColorHelper.TransparentBrush;
            var mode = isChecked ? PlayMode.Repeat : PlayMode.Once;
            if (mode != Settings.settings.Mode) MusicPlayer.PlayMode = mode;
        }

        private void RepeatOneButton_Click(object sender, RoutedEventArgs e)
        {
            SetRepeatOne((bool)RepeatOneButton.IsChecked);
        }
        private void MoreRepeatOneButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            RepeatOneButton.IsChecked = !RepeatOneButton.IsChecked;
            RepeatOneButton_Click(RepeatOneButton, null);
        }
        public void SetRepeatOne(bool isChecked)
        {
            ShuffleButton.IsChecked = false;
            RepeatButton.IsChecked = false;
            ShuffleButton.SetToolTip(MoreShuffleButton.Label = Helper.LocalizeMessage("Shuffle: Disabled"), false);
            RepeatButton.SetToolTip(MoreRepeatButton.Label = Helper.LocalizeMessage("Repeat: Disabled"), false);
            RepeatOneButton.SetToolTip(MoreRepeatOneButton.Label = Helper.LocalizeMessage("Repeat One: " + (isChecked ? "Enabled" : "Disabled")), false);
            MoreShuffleButton.IconBackground = ColorHelper.TransparentBrush;
            MoreRepeatButton.IconBackground = ColorHelper.TransparentBrush;
            MoreRepeatOneButton.IconBackground = isChecked ? ColorHelper.GrayBrush : ColorHelper.TransparentBrush;
            var mode = isChecked ? PlayMode.RepeatOne : PlayMode.Once;
            if (mode != Settings.settings.Mode) MusicPlayer.PlayMode = mode;
        }

        public void SetButtonPlaying()
        {
            PlayButton.Content = "\uE769";
            PlayButton.SetToolTip("Pause");
        }

        public void SetMusicPause()
        {
            PlayButton.Content = "\uE768";
            PlayButton.SetToolTip("Play");
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (MusicPlayer.IsPlaying)
            {
                MusicPlayer.Pause();
            }
            else
            {
                MusicPlayer.Play();
            }
        }

        public void SetMuted(bool isMuted)
        {
            string tooltip = Helper.Localize(isMuted ? "Undo Mute" : "Mute");
            VolumeButton.Content = isMuted ? "\uE74F" : Helper.GetVolumeIcon(VolumeSlider.Value);
            MusicPlayer.Player.IsMuted = isMuted;
            VolumeButton.SetToolTip(tooltip, false);
            MuteButton.SetToolTip(tooltip, false);
        }

        private void VolumeButton_Click(object sender, RoutedEventArgs e)
        {
            SetMuted(Settings.settings.IsMuted = !Settings.settings.IsMuted);
        }

        private void VolumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!(sender as Slider).IsLoaded) return;
            double volume = e.NewValue;
            MusicPlayer.Player.IsMuted = false;
            Settings.settings.Volume = MusicPlayer.Player.Volume = volume / 100;
            string icon = Helper.GetVolumeIcon(volume);
            if (VolumeButton != null) VolumeButton.Content = icon;
        }

        public void SetVolume(int volume)
        {
            if (volume < 0)
            {
                VolumeSlider.Value = 0;
            }
            else if (volume > 100)
            {
                VolumeSlider.Value = 100;
            }
            else
            {
                VolumeSlider.Value = volume;
            }
        }

        public void LikeMusic()
        {
            ToggleButton likeToggleButton = LikeToggleButton;
            if (likeToggleButton == null)
            {
                return;
            }
            likeToggleButton.SetToolTip("UndoLikeMusicToolTip");
            likeToggleButton.IsChecked = true;
        }

        public void DislikeMusic()
        {
            ToggleButton likeToggleButton = LikeToggleButton;
            if (likeToggleButton == null)
            {
                return;
            }
            likeToggleButton.SetToolTip("LikeMusicToolTip");
            likeToggleButton.IsChecked = false;
        }

        private void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            if (button.IsChecked == true)
            {
                LikeMusic();
                MusicService.LikeMusic(MusicPlayer.CurrentMusic);
            }
            else
            {
                DislikeMusic();
                MusicService.DislikeMusic(MusicPlayer.CurrentMusic);
            }
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            MediaSlider.Value = 0;
            MusicPlayer.MovePrev();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            MediaSlider.Value = 0;
            MusicPlayer.MoveNext();
        }

        private void MediaSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            double newValue = e.NewValue;
            double position = MusicPlayer.Position;
            if (newValue == position || position > MediaSlider.Maximum)
            {
                return;
            }
            if (!MusicPlayer.IsPlaying)
            {
                MusicPlayer.Position = newValue;
            }
            if (LeftTimeTextBlock != null) LeftTimeTextBlock.Text = MusicDurationConverter.ToTime(newValue);
        }

        private void MediaSlider_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (wasPlaying)
            {
                MusicPlayer.Play();
            }
        }

        private void MediaSlider_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            wasPlaying = MusicPlayer.IsPlaying;
            MusicPlayer.Pause();
        }

        private void MainMusicInfoGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (MusicPlayer.CurrentMusic == null || MainPage.Instance == null) return;
            MainPage.Instance.Frame.Navigate(typeof(NowPlayingFullPage), null, new DrillInNavigationTransitionInfo());
        }

        private void SetFullScreen()
        {
            switch (mode)
            {
                case MediaControlMode.Main:
                    MainMoreFullScreenItem.Visibility = Visibility.Visible;
                    MainMoreExitFullScreenItem.Visibility = Visibility.Collapsed;
                    MainMediaControlMoreFullScreenItem.Visibility = Visibility.Visible;
                    MainMediaControlMoreExitFullScreenItem.Visibility = Visibility.Collapsed;
                    break;
                case MediaControlMode.Full:
                    FullScreenButton.Visibility = Visibility.Visible;
                    ExitFullScreenButton.Visibility = Visibility.Collapsed;
                    FullScreenItem.Visibility = Window.Current.Bounds.Width < MinimalLayoutWidth ? Visibility.Visible : Visibility.Collapsed;
                    ExitFullScreenItem.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void SetExitFullScreen()
        {
            switch (mode)
            {
                case MediaControlMode.Main:
                    MainMoreFullScreenItem.Visibility = Visibility.Collapsed;
                    MainMoreExitFullScreenItem.Visibility = Visibility.Visible;
                    MainMediaControlMoreFullScreenItem.Visibility = Visibility.Collapsed;
                    MainMediaControlMoreExitFullScreenItem.Visibility = Visibility.Visible;
                    break;
                case MediaControlMode.Full:
                    FullScreenButton.Visibility = Visibility.Collapsed;
                    ExitFullScreenButton.Visibility = Visibility.Visible;
                    FullScreenItem.Visibility = Visibility.Collapsed;
                    ExitFullScreenItem.Visibility = Window.Current.Bounds.Width < MinimalLayoutWidth ? Visibility.Visible : Visibility.Collapsed;
                    break;
            }
        }

        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationView applicationView = ApplicationView.GetForCurrentView();
            if (applicationView.TryEnterFullScreenMode())
            {
                SetExitFullScreen();
            }
        }

        private void ExitFullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationView applicationView = ApplicationView.GetForCurrentView();
            if (applicationView.IsFullScreenMode)
            {
                applicationView.ExitFullScreenMode();
                SetFullScreen();
            }
        }

        private void MoreFullScreenItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FullScreenButton_Click(sender, null);
        }
        private void MiniModeButton_Click(object sender, RoutedEventArgs e)
        {
            if (MiniModePage.IsMiniMode)
            {
                MiniModePage.ExitMiniMode();
            }
            else
            {
                MiniModePage.EnterMiniMode();
            }
        }

        private void MoreMiniModeItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            MiniModeButton_Click(sender, null);
        }

        private void PlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            List<Music> currentPlaylist = MusicPlayer.CurrentPlaylist.ToList();
            foreach (var listener in MusicRequestListeners)
                listener.PlaylistRequested(currentPlaylist);
        }

        private void MusicInfoButton_Click(object sender, RoutedEventArgs e)
        {
            Music currentMusic = MusicPlayer.CurrentMusic;
            foreach (var listener in MusicRequestListeners)
                listener.MusicInfoRequested(currentMusic);
        }

        private void LyricsButton_Click(object sender, RoutedEventArgs e)
        {
            Music currentMusic = MusicPlayer.CurrentMusic;
            foreach (var listener in MusicRequestListeners)
                listener.LyricsRequested(currentMusic);
        }

        private void AlbumArtButton_Click(object sender, RoutedEventArgs e)
        {
            Music currentMusic = MusicPlayer.CurrentMusic;
            foreach (var listener in MusicRequestListeners)
                listener.AlbumArtRequested(currentMusic);
        }

        private async void SavePlaylistItem_Click(object sender, RoutedEventArgs e)
        {
            var name = Helper.Localize("Now Playing") + " - " + DateTime.Now.ToString("yy/MM/dd");
            int index = PlaylistService.FindNextPlaylistNameIndex(name);
            var defaultName = index == 0 ? name : Helper.GetNextName(name, index);
            var dialog = new RenameDialog(RenameOption.Create, RenameTarget.Playlist, defaultName)
            {
                Validate = PlaylistService.ValidatePlaylistName,
                Confirmed = (newName) => PlaylistService.AddPlaylist(newName, MusicPlayer.CurrentPlaylist)
            };
            await dialog.ShowAsync();
        }

        private void ClearNowPlayingItem_Click(object sender, RoutedEventArgs e)
        {
            if (NowPlayingFullPage.Instance != null)
            {
                NowPlayingFullPage.Instance.GoBack();
                MusicPlayer.Clear();
            }
        }

        private void PlayAlbumItem_Click(object sender, RoutedEventArgs e)
        {
            MusicPlayer.ShuffleAndPlay(MusicService.SelectByAlbum(MusicPlayer.CurrentMusic.Album));
        }

        private void PlayArtistItem_Click(object sender, RoutedEventArgs e)
        {
            MusicPlayer.ShuffleAndPlay(MusicService.SelectByAlbum(MusicPlayer.CurrentMusic.Artist));
        }

        public void ClearMusic()
        {
            SetMusicPause();
            CurrentMusic = null;
            AlbumCover.Source = MusicImage.DefaultImage;
            MainMediaControlGrid.Background = ColorHelper.HighlightBrush;
            TitleTextBlock.Text = "";
            ArtistTextBlock.Text = "";
            FullAlbumTextBlock.Text = "";
            MainMusicInfoGrid.SetToolTip(null);
            if (RightTimeTextBlock != null) RightTimeTextBlock.Text = "0:00";
            if (LikeToggleButton != null)
            {
                LikeToggleButton.IsEnabled = false;
                LikeToggleButton.IsChecked = false;
            }
            MediaSlider.Value = 0;
            MediaSlider.IsEnabled = false;
        }

        private void MainMoreMenuFlyout_Opening(object sender, object e)
        {
            var flyout = sender as MenuFlyout;
            flyout.Items.PopToSize(3);
            if (MusicPlayer.CurrentMusic != null)
            {
                var helper = new MenuFlyoutHelper() { Data = MusicPlayer.CurrentMusic };
                var propertyItems = helper.GetMusicPropertiesMenuFlyout().Items;
                foreach (var item in propertyItems)
                    flyout.Items.AddToFirst(item);
                flyout.Items.AddToFirst(MenuFlyoutHelper.GetPreferItem(MusicPlayer.CurrentMusic.ToVO()));
                flyout.Items.AddToFirst(helper.GetAddToMenuFlyoutSubItem());
            }
            flyout.Items.AddToFirst(MenuFlyoutHelper.GetQuickPlayItem(showIcon: true));
        }

        private void MainMusicInfoGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (MusicPlayer.CurrentMusic == null) return;
            VisualStateManager.GoToState(this, "PointerOver", true);
            MainTitleTextBlock.StartScrolling();
            MainArtistTextBlock.StartScrolling();
        }

        private void MainMusicInfoGrid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (MusicPlayer.CurrentMusic == null) return;
            VisualStateManager.GoToState(this, "Normal", true);
        }

        private async void MainMediaControlMoreMusicInfoItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await new MusicDialog(MusicDialogOption.Properties, MusicPlayer.CurrentMusic).ShowAsync();
        }

        private async void MainMediaControlMoreLyricsItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await new MusicDialog(MusicDialogOption.Lyrics, MusicPlayer.CurrentMusic).ShowAsync();
        }

        private async void MainMediaControlMoreAlbumArtItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await new MusicDialog(MusicDialogOption.AlbumArt, MusicPlayer.CurrentMusic).ShowAsync();
        }

        private void FullMoreMenuFlyout_Opening(object sender, object e)
        {
            MenuFlyout flyout = sender as MenuFlyout;
            flyout.Items.RemoveAll(item => item.Name == MenuFlyoutHelper.ShuffleSubItemName);
            flyout.Items.Add(MenuFlyoutHelper.GetShuffleSubItem());
            flyout.Items.RemoveAll(item => item.Name == MenuFlyoutHelper.PreferItemName);
            flyout.Items.Add(MenuFlyoutHelper.GetPreferItem(MusicPlayer.CurrentMusic));
        }

        private void MiniMoreShufflePlayButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            MusicPlayer.QuickPlay();
            MiniMoreFlyout.Hide();
        }

        private async void VoiceAssistantButton_Click(object sender, RoutedEventArgs e)
        {
            string hint = VoiceAssistantHelper.GetRandomHint();

            Flyout flyout = new Flyout();
            StackPanel panel = new StackPanel();
            TextBlock textBlock = new TextBlock();
            Run hintTextBlockRun = new Run
            {
                Text = hint
            };
            textBlock.Inlines.Add(hintTextBlockRun);
            Hyperlink hyperlink = new Hyperlink()
            {
                UnderlineStyle = UnderlineStyle.None
            };
            hyperlink.Click += VoiceAssistantHelper_Click;
            hyperlink.Inlines.Add(new Run
            {
                Text = Helper.LocalizeText("GetHelp")
            });
            textBlock.Inlines.Add(hyperlink);
            panel.Children.Add(textBlock);
            ProgressBar progressBar = new ProgressBar
            {
                Style = (Style)Resources["VoiceAssistantProgressBarStyle"],
                Visibility = Visibility.Collapsed,
            };
            panel.Children.Add(progressBar);
            bool breakLoop = true, wasProcessing = false, waitForResponse = true;
            flyout.Closed += (s, a) => 
            {
                breakLoop = true;
                VoiceAssistantHelper.StateChangedListeners.Remove(OnVoiceAssistantStateChanged);
                VoiceAssistantHelper.StopRecognition();
            };
            flyout.Content = panel;
            flyout.ShowAt(sender as FrameworkElement);
            VoiceAssistantHelper.StateChangedListeners.Add(OnVoiceAssistantStateChanged);
            while (breakLoop)
            {
                SpeechRecognitionResult result = await VoiceAssistantHelper.Recognize();
                VoiceAssistantHelper.StopRecognition();
                if (result == null)
                {
                    break;
                }
                if (string.IsNullOrEmpty(result.Text))
                {
                    await Task.Delay(200);
                    continue;
                }
                textBlock.Text = result.Text;
                textBlock.Visibility = Visibility.Visible;
                progressBar.Visibility = Visibility.Collapsed;
                if (!await VoiceAssistantHelper.HandleCommand(result, () => { waitForResponse = false; }))
                {
                    while (waitForResponse)
                    {
                        await Task.Delay(100);
                    }
                    continue;
                }
                Timer timer = new Timer(5000);
                timer.Elapsed += async (s, args) =>
                {
                    await Dispatcher.RunIdleAsync(a =>
                    {
                        flyout.Hide();
                        timer.Stop();
                    });
                };
                timer.Start();
                break;
            }

            async void OnVoiceAssistantStateChanged(SpeechRecognizer r, VoiceAssistantEventArgs args)
            {
                await Helper.RunInMainUIThread(Dispatcher, () =>
                {
                    switch (args.State)
                    {
                        case SpeechRecognizerState.Capturing:
                            if (hintTextBlockRun.Text == hint)
                            {
                                break;
                            }
                            textBlock.Text = "正在听你说话…";
                            break;
                        case SpeechRecognizerState.Processing:
                            textBlock.Visibility = Visibility.Collapsed;
                            progressBar.Visibility = Visibility.Visible;
                            wasProcessing = true;
                            break;
                        case SpeechRecognizerState.Idle:
                            if (!wasProcessing)
                            {
                                textBlock.Visibility = Visibility.Visible;
                                progressBar.Visibility = Visibility.Collapsed;
                            }
                            wasProcessing = false;
                            break;
                    }
                });
            }
        }

        private async void VoiceAssistantHelper_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            await new VoiceAssistantHelpDialog().ShowAsync();
        }

        void IMusicEventListener.Execute(Music music, MusicEventArgs args)
        {
            switch (args.EventType)
            {
                case MusicEventType.Like:
                    if (CurrentMusic != null && CurrentMusic.Equals(music))
                    {
                        if (args.IsFavorite) LikeMusic();
                        else DislikeMusic();
                    }
                    break;
                case MusicEventType.Modify:
                    if (CurrentMusic != null && CurrentMusic.Equals(music))
                        UpdateMusic(args.ModifiedMusic);
                    break;
            }
        }

        async void IMusicPlayerEventListener.Execute(MusicPlayerEventArgs args)
        {
            switch (args.EventType)
            {
                case MusicPlayerEventType.Remove:
                    if (MusicPlayer.CurrentPlaylistCount == 0)
                    {
                        ClearMusic();
                    }
                    break;
                case MusicPlayerEventType.Clear:
                    ClearMusic();
                    break;
                case MusicPlayerEventType.MediaEnded:
                    ToastHelper.HideToast();
                    break;
                case MusicPlayerEventType.Switch:
                    await Helper.RunInMainUIThread(Dispatcher, async () =>
                    {
                        Music next = args.Music;
                        if (CurrentMusic == null || !CurrentMusic.Equals(next)) UpdateMusic(next);
                        if (MainTitleTextBlock.IsScrolling) MainTitleTextBlock.StopScrolling();
                        if (MainArtistTextBlock.IsScrolling) MainArtistTextBlock.StopScrolling();
                        // avoid showing toast on app launch
                        if ((args as MusicPlayerMusicSwitchEventArgs).Reason != MediaPlaybackItemChangedReason.InitialItem)
                        {
                            await ToastHelper.ShowToast(next, MusicPlayer.PlaybackState);
                        }
                    });
                    break;
                case MusicPlayerEventType.StateChanged:
                    MediaPlaybackState state = (args as MusicPlayerStateChangedEventArgs).State;
                    // For F3 Support
                    await Helper.RunInMainUIThread(Dispatcher, async () =>
                    {
                        switch (state)
                        {
                            case MediaPlaybackState.Playing:
                                SetButtonPlaying();
                                break;
                            case MediaPlaybackState.Paused:
                                SetMusicPause();
                                break;
                        }
                        await ToastHelper.ShowToast(MusicPlayer.CurrentMusic, state);
                    });
                    break;
            }
        }
    }

    public interface IMusicRequestListener
    {
        void PlaylistRequested(IEnumerable<Music> playlist);
        void MusicInfoRequested(Music music);
        void LyricsRequested(Music music);
        void AlbumArtRequested(Music music);
    }
}

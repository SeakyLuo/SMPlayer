using SMPlayer.Dialogs;
using SMPlayer.Helpers;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;
using Windows.Foundation;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SMPlayer
{
    public sealed partial class MediaControl : UserControl, ISwitchMusicListener, IMediaControlListener, ICurrentPlaylistChangedListener, IMusicEventListener, IMediaPlayerStateChangedListener
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

        public TextBlock VoiceAssistantTextBlock
        {
            get
            {
                switch (mode)
                {
                    case MediaControlMode.Main:
                        return IsMinimalMain ? MainMediaControlVoiceAssistantTextBlock : MainVoiceAssistantTextBlock;
                    case MediaControlMode.Full:
                        return FullMediaControlVoiceAssistantTextBlock;
                    case MediaControlMode.Mini:
                        return MiniVoiceAssistantButtonTextBlock;
                    default:
                        return null;
                }
            }
        }

        public ProgressBar VoiceAssistantProgressBar
        {
            get
            {
                switch (mode)
                {
                    case MediaControlMode.Main:
                        return IsMinimalMain ? MainMediaControlVoiceAssistantProgressBar : MainVoiceAssistantProgressBar;
                    case MediaControlMode.Full:
                        return FullMediaControlVoiceAssistantProgressBar;
                    case MediaControlMode.Mini:
                        return MiniVoiceAssistantButtonProgressBar;
                    default:
                        return null;
                }
            }
        }

        public Flyout VoiceAssistantButtonFlyout
        {
            get
            {
                switch (mode)
                {
                    case MediaControlMode.Main:
                        return IsMinimalMain ? MainMediaControlVoiceAssistantButtonFlyout : MainVoiceAssistantButtonFlyout;
                    case MediaControlMode.Full:
                        return FullVoiceAssistantButtonFlyout;
                    case MediaControlMode.Mini:
                        return MiniVoiceAssistantButtonFlyout;
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

        private Music CurrentMusic = null;
        private bool ShouldUpdate = true, SliderClicked = false;
        private static List<IMusicRequestListener> MusicRequestListeners = new List<IMusicRequestListener>();
        private static bool inited = false;
        private bool IsMinimalMain { get => MainMediaControlMoreButton.Visibility == Visibility.Visible; }
        private double MinimalLayoutWidth { get => (double) Resources["MinimalLayoutWidth"]; }
        private Timer VoiceAssistantFlyoutTimer = new Timer(5000);

        public MediaControl()
        {
            this.InitializeComponent();
            MusicPlayer.AddSwitchMusicListener(this);
            MusicPlayer.MediaControlListeners.Add(this);
            MusicPlayer.InitFinishedListeners.Add(() =>
            {
                AfterLoaded();
                inited = true;
                MainSliderProgressBar.Visibility = Visibility.Collapsed;
                MainMediaSlider.Visibility = Visibility.Visible;
            });
            MusicPlayer.MediaPlayerStateChangedListeners.Add(this);
            MusicPlayer.CurrentPlaylistChangedListeners.Add(this);
            Settings.AddMusicEventListener(this);
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

            VoiceAssistantHelper.StateChangedListeners.Add(async (sender, args) =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    switch (args.State)
                    {
                        case Windows.Media.SpeechRecognition.SpeechRecognizerState.Processing:
                            VoiceAssistantTextBlock.Visibility = Visibility.Collapsed;
                            VoiceAssistantProgressBar.Visibility = Visibility.Visible;
                            break;
                    }
                });
            });

            VoiceAssistantFlyoutTimer.Elapsed += async (s, args) =>
            {
                await Dispatcher.RunIdleAsync(a =>
                {
                    VoiceAssistantButtonFlyout.Hide();
                });
            };
        }

        private void AfterLoaded()
        {
            UpdateMusic(MusicPlayer.CurrentMusic);
            MediaSlider.Value = MusicPlayer.Position;

            if (MusicPlayer.IsPlaying) PlayMusic();
            else PauseMusic();

            double volume = Settings.settings.Volume * 100;
            VolumeButton.Content = Helper.GetVolumeIcon(volume);
            SetMuted(Settings.settings.IsMuted);
            VolumeSlider.Value = volume;
            SetPlayMode(Settings.settings.Mode);

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
            CurrentMusic = music;
            if (music == null)
            {
                ClearMusic();
                return;
            }
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                MediaSlider.IsEnabled = true;
                TitleTextBlock.Text = music.Name;
                ArtistTextBlock.Text = music.Artist;
                MediaSlider.Value = MusicPlayer.Progress;
                MediaSlider.Maximum = music.Duration;
                if (RightTimeTextBlock != null) RightTimeTextBlock.Text = MusicDurationConverter.ToTime(music.Duration);
                if (LikeToggleButton != null)
                {
                    LikeToggleButton.IsEnabled = true;
                    if (music.Favorite) LikeMusic(false);
                    else DislikeMusic(false);
                }
                SetThumbnail(music);
            });
        }

        public async void SetThumbnail(Music music)
        {
            using (var thumbnail = await ImageHelper.LoadThumbnail(music))
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

        public void SetMusic(Music music)
        {
            if (CurrentMusic == music) return;
            UpdateMusic(music);
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
            if (mode != Settings.settings.Mode) MusicPlayer.SetMode(mode);
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
            if (mode != Settings.settings.Mode) MusicPlayer.SetMode(mode);
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
            if (mode != Settings.settings.Mode) MusicPlayer.SetMode(mode);
        }

        public void PlayMusic()
        {
            PlayButton.Content = "\uE769";
            PlayButton.SetToolTip("Pause");
        }

        public void PauseMusic()
        {
            PlayButton.Content = "\uE768";
            PlayButton.SetToolTip("Play");
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            if (button.Content.ToString() == "\uE768")
            {
                if (MusicPlayer.CurrentMusic == null || MusicPlayer.CurrentPlaylist.IsEmpty())
                    return;
                MusicPlayer.Play();
            }
            else
            {
                MusicPlayer.Pause();
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

        public void LikeMusic(bool isClick = true)
        {
            LikeToggleButton.SetToolTip("UndoLikeMusicToolTip");
            if (isClick)
            {
                Settings.settings.LikeMusic(MusicPlayer.CurrentMusic);
            }
            else LikeToggleButton.IsChecked = true;
        }

        public void DislikeMusic(bool isClick = true)
        {
            LikeToggleButton.SetToolTip("LikeMusicToolTip");
            if (isClick)
            {
                Settings.settings.DislikeMusic(MusicPlayer.CurrentMusic);
            }
            else LikeToggleButton.IsChecked = false;
        }

        private void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            if (button.IsChecked == true) LikeMusic();
            else DislikeMusic();
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            MediaSlider.Value = 0;
            if (MusicPlayer.Position > 5)
                MusicPlayer.Position = 0;
            else
                MusicPlayer.MovePrev();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            MediaSlider.Value = 0;
            MusicPlayer.MoveNext();
        }

        private void MediaSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            int newValue = (int)e.NewValue, oldValue = (int)e.OldValue, diff = newValue - oldValue;
            SliderClicked = diff != 1 && diff != 0;
            if (LeftTimeTextBlock != null) LeftTimeTextBlock.Text = MusicDurationConverter.ToTime(newValue);
        }

        private void MediaSlider_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            MusicPlayer.Position = MediaSlider.Value;
            ShouldUpdate = true;
            Log.Info("MediaSlider_ManipulationCompleted");
        }

        private void MediaSlider_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            ShouldUpdate = false;
            Log.Info("MediaSlider_ManipulationStarted");
        }
        private void MediaSlider_ManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        {
            Log.Info("MediaSlider_ManipulationStarting");
            if (SliderClicked)
            {
                MusicPlayer.Position = MediaSlider.Value;
            }
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
            foreach (var listener in MusicRequestListeners)
                listener.PlaylistRequested(MusicPlayer.CurrentPlaylist);
        }

        private void MusicInfoButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var listener in MusicRequestListeners)
                listener.MusicInfoRequested(MusicPlayer.CurrentMusic);
        }

        private void LyricsButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var listener in MusicRequestListeners)
                listener.LyricsRequested(MusicPlayer.CurrentMusic);
        }

        private async void SavePlaylistItem_Click(object sender, RoutedEventArgs e)
        {
            var name = Helper.Localize("Now Playing") + " - " + DateTime.Now.ToString("yy/MM/dd");
            int index = Settings.settings.FindNextPlaylistNameIndex(name);
            var defaultName = index == 0 ? name : Helper.GetNextName(name, index);
            var dialog = new RenameDialog(RenameOption.Create, RenameTarget.Playlist, defaultName)
            {
                Validate = Settings.settings.ValidatePlaylistName,
                Confirmed = (newName) => Settings.settings.AddPlaylist(newName, MusicPlayer.CurrentPlaylist)
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
            MusicPlayer.SetMusicAndPlay(Settings.AllSongs.Where(m => m.Album == MusicPlayer.CurrentMusic.Album));
        }

        private void PlayArtistItem_Click(object sender, RoutedEventArgs e)
        {
            MusicPlayer.SetMusicAndPlay(Settings.AllSongs.Where(m => m.Artist == MusicPlayer.CurrentMusic.Artist));
        }

        public void Tick()
        {
            if (ShouldUpdate)
            {
                MediaSlider.Value = MusicPlayer.Position;
            }
        }

        public async void MusicSwitching(Music current, Music next, MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
            {
                if (reason == MediaPlaybackItemChangedReason.EndOfStream)
                {
                    Settings.settings.Played(current);
                }
                next.IsPlaying = true;
                SetMusic(next);
                if (MainTitleTextBlock.IsScrolling) MainTitleTextBlock.StopScrolling();
                if (MainArtistTextBlock.IsScrolling) MainArtistTextBlock.StopScrolling();
                // avoid showing toast on app launch
                if (reason != MediaPlaybackItemChangedReason.InitialItem)
                {
                    await ToastHelper.ShowToast(next, MusicPlayer.PlaybackState);
                }
            });
        }

        public void MediaEnded()
        {
            ToastHelper.HideToast();
        }

        public void ClearMusic()
        {
            PauseMusic();
            CurrentMusic = null;
            AlbumCover.Source = MusicImage.DefaultImage;
            MainMediaControlGrid.Background = ColorHelper.HighlightBrush;
            TitleTextBlock.Text = "";
            ArtistTextBlock.Text = "";
            FullAlbumTextBlock.Text = "";
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
            if (MusicPlayer.CurrentMusic == null)
            {
                if (flyout.Items[0].Name == MenuFlyoutHelper.AddToSubItemName)
                {
                    for (int i = 0; i < 3; i++) // HardCoded 3
                        flyout.Items.RemoveAt(0);
                }
            }
            else
            {
                var helper = new MenuFlyoutHelper() { Data = MusicPlayer.CurrentMusic };
                var addToItem = helper.GetAddToMenuFlyoutSubItem();
                var propertyItems = helper.GetMusicPropertiesMenuFlyout().Items;
                propertyItems.Insert(0, addToItem);
                if (flyout.Items[0].Name == MenuFlyoutHelper.AddToSubItemName)
                {
                    for (int i = 0; i < propertyItems.Count; i++)
                        flyout.Items[i] = propertyItems[i];
                }
                else
                {
                    foreach (var item in propertyItems.Reverse())
                        flyout.Items.Insert(0, item);
                }
                flyout.Items.Insert(1, MenuFlyoutHelper.GetPreferItem(MusicPlayer.CurrentMusic));
            }
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
            if (flyout.Items.Last().Name == MenuFlyoutHelper.ShuffleSubItemName)
                flyout.Items.RemoveAt(flyout.Items.Count - 1);
            flyout.Items.Add(MenuFlyoutHelper.GetShuffleSubItem());
            flyout.Items.Add(MenuFlyoutHelper.GetPreferItem(MusicPlayer.CurrentMusic));
        }

        private void MiniMoreShufflePlayButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            MusicPlayer.QuickPlay();
            MiniMoreFlyout.Hide();
        }

        private async void VoiceAssistantButton_Click(object sender, RoutedEventArgs e)
        {
            if (await VoiceAssistantHelper.Recognize() is Windows.Media.SpeechRecognition.SpeechRecognitionResult result)
            {
                VoiceAssistantTextBlock.Text = result.Text;
                VoiceAssistantProgressBar.Visibility = Visibility.Collapsed;
                VoiceAssistantTextBlock.Visibility = Visibility.Visible;
                await VoiceAssistantHelper.HandleCommand(result);
                VoiceAssistantFlyoutTimer.Start();
            } 
            else
            {
                VoiceAssistantButtonFlyout.Hide();
            }
        }

        public void MusicLiked(Music music, bool isFavorite)
        {
            if (music == MusicPlayer.CurrentMusic)
            {
                if (isFavorite) LikeMusic(false);
                else DislikeMusic(false);
            }
        }

        private void VoiceAssistantButtonFlyout_Closed(object sender, object e)
        {
            VoiceAssistantHelper.StopRecognition();
        }

        private void VoiceAssistantButtonFlyout_Opened(object sender, object e)
        {
            VoiceAssistantTextBlock.Text = VoiceAssistantHelper.GetRandomHint();
            VoiceAssistantTextBlock.Visibility = Visibility.Visible;
            VoiceAssistantProgressBar.Visibility = Visibility.Collapsed;
        }

        async void IMediaPlayerStateChangedListener.StateChanged(MediaPlaybackState state)
        {
            // For F3 Support
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                switch (state)
                {
                    case MediaPlaybackState.Playing:
                        PlayMusic();
                        break;
                    case MediaPlaybackState.Paused:
                        PauseMusic();
                        break;
                }
                await ToastHelper.ShowToast(MusicPlayer.CurrentMusic, state);
            });
        }

        void IMusicEventListener.Execute(Music music, MusicEventArgs args)
        {
            switch (args.EventType)
            {
                case MusicEventType.Modify:
                    if (CurrentMusic == music)
                        UpdateMusic(args.ModifiedMusic);
                    break;
            }
        }

        void ICurrentPlaylistChangedListener.AddMusic(Music music, int index) { }
        void ICurrentPlaylistChangedListener.RemoveMusic(Music music) { }
        void ICurrentPlaylistChangedListener.Cleared()
        {
            ClearMusic();
        }
    }

    public interface IMusicRequestListener
    {
        void PlaylistRequested(ICollection<Music> playlist);
        void MusicInfoRequested(Music music);
        void LyricsRequested(Music music);
    }
}

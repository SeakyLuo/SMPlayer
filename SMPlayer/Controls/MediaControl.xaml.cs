using SMPlayer.Dialogs;
using SMPlayer.Helpers;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Media.Playback;
using Windows.Media.SpeechRecognition;
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
    public sealed partial class MediaControl : UserControl, ISwitchMusicListener, IMediaControlListener, IRemoveMusicListener, ILikeMusicListener
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

        private Music CurrentMusic = null;
        private bool ShouldUpdate = true, SliderClicked = false;
        private static List<Action<Music, Music>> MusicModifiedListeners = new List<Action<Music, Music>>();
        private static List<IMusicRequestListener> MusicRequestListeners = new List<IMusicRequestListener>();
        private static bool inited = false;
        private SpeechRecognizer speechRecognizer = new SpeechRecognizer(Helper.CurrentLanguage);
        private volatile bool IsSpeeching = false;
        private bool IsMinimalMain { get => MainMediaControlMoreButton.Visibility == Visibility.Visible; }
        private double MinimalLayoutWidth { get => (double) Resources["MinimalLayoutWidth"]; }

        public MediaControl()
        {
            this.InitializeComponent();
            MediaHelper.SwitchMusicListeners.Add(this);
            MediaHelper.MediaControlListeners.Add(this);
            MediaHelper.RemoveMusicListeners.Add(this);
            MediaHelper.InitFinishedListeners.Add(() =>
            {
                AfterLoaded();
                inited = true;
                MainSliderProgressBar.Visibility = Visibility.Collapsed;
                MainMediaSlider.Visibility = Visibility.Visible;
            });
            Settings.LikeMusicListeners.Add(this);
            MediaHelper.Player.PlaybackSession.PlaybackStateChanged += async (sender, args) =>
            {
                // For F3 Support
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    switch (sender.PlaybackState)
                    {
                        case MediaPlaybackState.Playing:
                            PlayMusic();
                            break;
                        case MediaPlaybackState.Paused:
                            PauseMusic();
                            break;
                    }
                    await ToastHelper.ShowToast(MediaHelper.CurrentMusic, sender.PlaybackState);
                });
            };

            Controls.MusicInfoControl.MusicModifiedListeners.Add((before, after) =>
            {
                if (CurrentMusic == before)
                    UpdateMusic(after);
            });
            var left = new KeyboardAccelerator() { Key = Windows.System.VirtualKey.Left };
            left.Invoked += (sender, args) =>
            {
                if (MediaHelper.CurrentMusic != null)
                    MediaHelper.Position = Math.Max(MediaHelper.Position - 5, 0);
            };
            KeyboardAccelerators.Add(left);
            var right = new KeyboardAccelerator() { Key = Windows.System.VirtualKey.Right };
            right.Invoked += (sender, args) =>
            {
                if (MediaHelper.CurrentMusic != null)
                    MediaHelper.Position = Math.Min(MediaHelper.Position + 5, MediaHelper.CurrentMusic.Duration);
            };
            KeyboardAccelerators.Add(right);
            KeyboardAcceleratorPlacementMode = KeyboardAcceleratorPlacementMode.Hidden;
        }

        private void AfterLoaded()
        {
            UpdateMusic(MediaHelper.CurrentMusic);
            MediaSlider.Value = MediaHelper.Position;

            if (MediaHelper.IsPlaying) PlayMusic();
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

        public void UpdateMusic(Music music)
        {
            CurrentMusic = music;
            if (music == null)
            {
                ClearMusic();
                return;
            }
            MediaSlider.IsEnabled = true;
            TitleTextBlock.Text = music.Name;
            ArtistTextBlock.Text = music.Artist;
            MediaSlider.Value = MediaHelper.Progress;
            MediaSlider.Maximum = music.Duration;
            if (RightTimeTextBlock != null) RightTimeTextBlock.Text = MusicDurationConverter.ToTime(music.Duration);
            if (LikeToggleButton != null)
            {
                LikeToggleButton.IsEnabled = true;
                if (music.Favorite) LikeMusic(false);
                else DislikeMusic(false);
            }
            SetThumbnail(music);
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
        public static void AddMusicModifiedListener(Action<Music, Music> listener)
        {
            MusicModifiedListeners.Add(listener);
        }

        public static void AddMusicRequestListener(IMusicRequestListener listener)
        {
            MusicRequestListeners.Add(listener);
        }
        public static void NotifyMusicModifiedListeners(Music before, Music after)
        {
            foreach (var listener in MusicModifiedListeners)
                listener.Invoke(before, after);
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
            if (MediaHelper.ShuffleEnabled)
                MediaHelper.ShuffleOthers();
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
            ShuffleButton.SetToolTip(MoreShuffleButton.Label = Helper.LocalizeMessage("Shuffle: " + (isChecked ? "Enabled" : "Disabled")));
            RepeatButton.SetToolTip(MoreRepeatButton.Label = Helper.LocalizeMessage("Repeat: Disabled"));
            RepeatOneButton.SetToolTip(MoreRepeatOneButton.Label = Helper.LocalizeMessage("Repeat One: Disabled"));
            MoreShuffleButton.IconBackground = isChecked ? ColorHelper.GrayBrush : ColorHelper.TransparentBrush;
            MoreRepeatButton.IconBackground = ColorHelper.TransparentBrush;
            MoreRepeatOneButton.IconBackground = ColorHelper.TransparentBrush;
            var mode = isChecked ? PlayMode.Shuffle : PlayMode.Once;
            if (mode != Settings.settings.Mode) MediaHelper.SetMode(mode);
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
            ShuffleButton.SetToolTip(MoreShuffleButton.Label = Helper.LocalizeMessage("Shuffle: Disabled"));
            RepeatButton.SetToolTip(MoreRepeatButton.Label = Helper.LocalizeMessage("Repeat: " + (isChecked ? "Enabled" : "Disabled")));
            RepeatOneButton.SetToolTip(MoreRepeatOneButton.Label = Helper.LocalizeMessage("Repeat One: Disabled"));
            MoreShuffleButton.IconBackground = ColorHelper.TransparentBrush;
            MoreRepeatButton.IconBackground = isChecked ? ColorHelper.GrayBrush : ColorHelper.TransparentBrush;
            MoreRepeatOneButton.IconBackground = ColorHelper.TransparentBrush;
            var mode = isChecked ? PlayMode.Repeat : PlayMode.Once;
            if (mode != Settings.settings.Mode) MediaHelper.SetMode(mode);
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
            ShuffleButton.SetToolTip(MoreShuffleButton.Label = Helper.LocalizeMessage("Shuffle: Disabled"));
            RepeatButton.SetToolTip(MoreRepeatButton.Label = Helper.LocalizeMessage("Repeat: Disabled"));
            RepeatOneButton.SetToolTip(MoreRepeatOneButton.Label = Helper.LocalizeMessage("Repeat One: " + (isChecked ? "Enabled" : "Disabled")));
            MoreShuffleButton.IconBackground = ColorHelper.TransparentBrush;
            MoreRepeatButton.IconBackground = ColorHelper.TransparentBrush;
            MoreRepeatOneButton.IconBackground = isChecked ? ColorHelper.GrayBrush : ColorHelper.TransparentBrush;
            var mode = isChecked ? PlayMode.RepeatOne : PlayMode.Once;
            if (mode != Settings.settings.Mode) MediaHelper.SetMode(mode);
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
                if (MediaHelper.CurrentMusic == null || MediaHelper.CurrentPlaylist.Count == 0)
                    return;
                MediaHelper.Play();
            }
            else
            {
                MediaHelper.Pause();
            }
        }

        public void SetMuted(bool isMuted)
        {
            string tooltip = Helper.Localize(isMuted ? "Undo Mute" : "Mute");
            VolumeButton.Content = isMuted ? "\uE74F" : Helper.GetVolumeIcon(VolumeSlider.Value);
            MediaHelper.Player.IsMuted = isMuted;
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
            MediaHelper.Player.IsMuted = false;
            double volume = e.NewValue / 100;
            Settings.settings.Volume = MediaHelper.Player.Volume = volume;
            string icon = Helper.GetVolumeIcon(e.NewValue);
            if (VolumeButton != null) VolumeButton.Content = icon;
        }

        public void LikeMusic(bool isClick = true)
        {
            LikeToggleButton.SetToolTip("Undo Favorite");
            if (isClick)
            {
                SetMusicFavorite(true);
                Settings.settings.LikeMusic(MediaHelper.CurrentMusic);
            }
            else LikeToggleButton.IsChecked = true;
        }

        public void DislikeMusic(bool isClick = true)
        {
            LikeToggleButton.SetToolTip("Set As Favorite");
            if (isClick)
            {
                SetMusicFavorite(false);
                Settings.settings.DislikeMusic(MediaHelper.CurrentMusic);
            }
            else LikeToggleButton.IsChecked = false;
        }

        private void SetMusicFavorite(bool favorite)
        {
            Music before = MediaHelper.CurrentMusic.Copy();
            MediaHelper.CurrentMusic.Favorite = favorite;
            NotifyMusicModifiedListeners(before, MediaHelper.CurrentMusic);
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
            if (MediaHelper.Position > 5)
                MediaHelper.Position = 0;
            else
                MediaHelper.MovePrev();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            MediaSlider.Value = 0;
            MediaHelper.MoveNext();
        }

        private void MediaSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            int newValue = (int)e.NewValue, oldValue = (int)e.OldValue, diff = newValue - oldValue;
            SliderClicked = diff != 1 && diff != 0;
            //Debug.WriteLine("ValueChanged To " + MusicDurationConverter.ToTime(newValue));
            if (LeftTimeTextBlock != null) LeftTimeTextBlock.Text = MusicDurationConverter.ToTime(newValue);
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

        private void MainMusicInfoGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (MediaHelper.CurrentMusic == null || MainPage.Instance == null) return;
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
        private async void MiniModeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ApplicationView.GetForCurrentView().IsViewModeSupported(ApplicationViewMode.CompactOverlay))
            {
                Helper.ShowNotification("MiniModeFailed");
                return;
            }
            if (ApplicationView.GetForCurrentView().ViewMode == ApplicationViewMode.Default)
            {
                (Window.Current.Content as Frame).Navigate(typeof(MiniModePage));
                ApplicationView.GetForCurrentView().SetPreferredMinSize(MiniModePage.PageSize);
                await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay, MiniModePage.ViewModePreferences);
            }
            else
            {
                (Window.Current.Content as Frame).GoBack();
                ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size());
                await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);
            }
        }

        private void MoreMiniModeItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            MiniModeButton_Click(sender, null);
        }

        private void PlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var listener in MusicRequestListeners)
                listener.PlaylistRequested(MediaHelper.CurrentPlaylist);
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
            var name = Helper.Localize("Now Playing") + " - " + DateTime.Now.ToString("yy/MM/dd");
            int index = Settings.settings.FindNextPlaylistNameIndex(name);
            var defaultName = index == 0 ? name : Helper.GetPlaylistName(name, index);
            var listener = new VirtualRenameActionListener() { Data = MediaHelper.CurrentPlaylist };
            var dialog = new RenameDialog(listener, RenameOption.New, defaultName);
            listener.Dialog = dialog;
            await dialog.ShowAsync();
        }

        private void ClearNowPlayingItem_Click(object sender, RoutedEventArgs e)
        {
            if (NowPlayingFullPage.Instance != null)
            {
                NowPlayingFullPage.Instance.GoBack();
                MediaHelper.Clear();
            }
        }

        private void PlayAlbumItem_Click(object sender, RoutedEventArgs e)
        {
            MediaHelper.SetMusicAndPlay(MusicLibraryPage.AllSongs.ToList().FindAll(m => m.Album == MediaHelper.CurrentMusic.Album));
        }

        private void PlayArtistItem_Click(object sender, RoutedEventArgs e)
        {
            MediaHelper.SetMusicAndPlay(MusicLibraryPage.AllSongs.ToList().FindAll(m => m.Artist == MediaHelper.CurrentMusic.Artist));
        }
        public void Tick()
        {
            if (ShouldUpdate)
            {
                MediaSlider.Value = MediaHelper.Position;
            }
        }

        public async void MusicSwitching(Music current, Music next, MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
            {
                if (reason == MediaPlaybackItemChangedReason.EndOfStream)
                {
                    Music before = current.Copy();
                    current.Played();
                    MediaHelper.MusicModified(before, current);
                    NotifyMusicModifiedListeners(before, current);
                    MediaSlider.Value = 0;
                }
                Settings.settings.Played(current);
                next.IsPlaying = true;
                SetMusic(next);
                if (MainTitleTextBlock.IsScrolling) MainTitleTextBlock.StopScrolling();
                if (MainArtistTextBlock.IsScrolling) MainArtistTextBlock.StopScrolling();
                // avoid showing toast on app launch
                if (reason != MediaPlaybackItemChangedReason.InitialItem)
                {
                    await ToastHelper.ShowToast(next, MediaHelper.PlaybackState);
                }
            });
        }

        public async void MediaEnded()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (Settings.settings.Mode == PlayMode.Once)
                    PauseMusic();
                MediaSlider.Value = 0;
                ToastHelper.HideToast();
            });
        }

        public void ClearMusic()
        {
            PauseMusic();
            CurrentMusic = null;
            AlbumCover.Source = MusicImage.DefaultImage;
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
            if (MediaHelper.CurrentMusic == null)
            {
                if (flyout.Items[0].Name == MenuFlyoutHelper.AddToSubItemName)
                {
                    for (int i = 0; i < 3; i++) // HardCoded 3
                        flyout.Items.RemoveAt(0);
                }
            }
            else
            {
                var helper = new MenuFlyoutHelper() { Data = MediaHelper.CurrentMusic };
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
            }
        }

        private void MainMusicInfoGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (MediaHelper.CurrentMusic == null) return;
            VisualStateManager.GoToState(this, "PointerOver", true);
            MainTitleTextBlock.StartScrolling();
            MainArtistTextBlock.StartScrolling();
        }

        private void MainMusicInfoGrid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (MediaHelper.CurrentMusic == null) return;
            VisualStateManager.GoToState(this, "Normal", true);
        }

        private async void MainMediaControlMoreMusicInfoItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await new MusicDialog(MusicDialogOption.Properties, MediaHelper.CurrentMusic).ShowAsync();
        }

        private async void MainMediaControlMoreLyricsItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await new MusicDialog(MusicDialogOption.Lyrics, MediaHelper.CurrentMusic).ShowAsync();
        }

        private async void MainMediaControlMoreAlbumArtItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await new MusicDialog(MusicDialogOption.AlbumArt, MediaHelper.CurrentMusic).ShowAsync();
        }

        public void MusicRemoved(int index, Music music, IEnumerable<Music> newCollection)
        {
            if (newCollection.Count() == 0)
                ClearMusic();
        }

        private void FullMoreMenuFlyout_Opening(object sender, object e)
        {
            MenuFlyout flyout = sender as MenuFlyout;
            if (flyout.Items.Last().Name == MenuFlyoutHelper.ShuffleSubItemName)
                flyout.Items.RemoveAt(flyout.Items.Count - 1);
            flyout.Items.Add(MenuFlyoutHelper.GetShuffleSubItem());
        }

        private void MiniMoreShufflePlayButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            MediaHelper.SetPlaylistAndPlay(MusicLibraryPage.AllSongs.RandItems(100));
            MiniMoreFlyout.Hide();
        }

        private async void VoiceAssistantButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsSpeeching)
            {
                await speechRecognizer.StopRecognitionAsync();
                goto VoiceEnds;
            }
            IsSpeeching = true;
            await speechRecognizer.CompileConstraintsAsync();
            try
            {
                SpeechRecognitionResult speechRecognitionResult = await speechRecognizer.RecognizeWithUIAsync();
                if (!IsSpeeching)
                {
                    VoiceAssistantHelper.HandleCommand(speechRecognitionResult.Text);
                }
            }
            catch (Exception)
            {
                //privacyPolicyHResult
                //The speech privacy policy was not accepted prior to attempting a speech recognition.
                ContentDialog Dialog = new ContentDialog()
                {
                    Title = "The speech privacy policy was not accepted",
                    Content = "You need to turn on a button called 'Get to know me'...",
                    PrimaryButtonText = "Shut up",
                    SecondaryButtonText = "Shut up and show me the setting"
                };
                if (await Dialog.ShowAsync() == ContentDialogResult.Secondary)
                {
                    // https://stackoverflow.com/questions/42391526/exception-the-speech-privacy-policy-was-not-accepted-prior-to-attempting-a-spee
                    if (!await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-speechtyping")))
                    {
                        await new ContentDialog()
                        {
                            Title = "Oops! Something went wrong...",
                            Content = "The settings app could not be opened.",
                            PrimaryButtonText = "Shut your mouth up!"
                        }.ShowAsync();
                    }
                }
            }
        VoiceEnds:
            IsSpeeching = false;
            switch (mode)
            {
                case MediaControlMode.Main:
                    (IsMinimalMain ? MainMediaControlVoiceAssistantButtonFlyout : MainVoiceAssistantButtonFlyout).Hide();
                    break;
                case MediaControlMode.Full:
                    FullVoiceAssistantButtonFlyout.Hide();
                    break;
                case MediaControlMode.Mini:
                    break;
            }
        }

        public void MusicLiked(Music music, bool isFavorite)
        {
            if (music == MediaHelper.CurrentMusic)
            {
                if (isFavorite) LikeMusic(false);
                else DislikeMusic(false);
            }
        }
    }

    public interface IMusicRequestListener
    {
        void PlaylistRequested(ICollection<Music> playlist);
        void MusicInfoRequested(Music music);
        void LyricsRequested(Music music);
    }
}

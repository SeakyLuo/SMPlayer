﻿using SMPlayer.Dialogs;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
    public sealed partial class MediaControl : UserControl, SwitchMusicListener, MediaControlListener, AfterPathSetListener, RemoveMusicListener, LikeMusicListener
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
                    case MediaControlMode.Mini:
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
                        return MainMediaControlMoreButton.Visibility == Visibility.Visible ? MainMoreVolumeSlider : MainVolumeSlider;
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

        private bool ShouldUpdate = true, SliderClicked = false;
        private static List<Action<Music, Music>> MusicModifiedListeners = new List<Action<Music, Music>>();
        private static List<MusicRequestListener> MusicRequestListeners = new List<MusicRequestListener>();

        public MediaControl()
        {
            this.InitializeComponent();
            MediaHelper.SwitchMusicListeners.Add(this);
            MediaHelper.MediaControlListeners.Add(this);
            MediaHelper.RemoveMusicListeners.Add(this);
            SettingsPage.AddAfterPathSetListener(this);
            Settings.LikeMusicListeners.Add(this);
            MediaHelper.Player.PlaybackSession.PlaybackStateChanged += async (sender, args) =>
            {
                // For F3 Support
                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    if (sender.PlaybackState == MediaPlaybackState.Playing)
                    {
                        PlayMusic();
                        Helper.ShowPauseToast(MediaHelper.CurrentMusic);
                    }
                    else if (sender.PlaybackState == MediaPlaybackState.Paused)
                    {
                        PauseMusic();
                        Helper.ShowPlayToast(MediaHelper.CurrentMusic);
                    }
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
        private Music CurrentMusic = null;

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetMusic(MediaHelper.CurrentMusic);
            if (MediaHelper.IsPlaying) PlayMusic();
            else PauseMusic();
            if (ApplicationView.GetForCurrentView().IsFullScreenMode) SetExitFullScreen();
            else SetFullScreen();

            double volume = Settings.settings.Volume * 100;
            VolumeButton.Content = Helper.GetVolumeIcon(volume);
            SetMuted(Settings.settings.IsMuted);
            VolumeSlider.Value = volume;
            SetPlayMode(Settings.settings.Mode);
        }

        public async void UpdateMusic(Music music)
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
            MediaSlider.Maximum = music.Duration;
            if (RightTimeTextBlock != null) RightTimeTextBlock.Text = MusicDurationConverter.ToTime(music.Duration);
            if (LikeToggleButton != null)
            {
                LikeToggleButton.IsEnabled = true;
                if (music.Favorite) LikeMusic(false);
                else DislikeMusic(false);
            }
            using (var thumbnail = await Helper.GetStorageItemThumbnailAsync(music))
            {
                var isThumbnail = thumbnail.IsThumbnail();
                AlbumCover.Source = isThumbnail ? thumbnail.GetBitmapImage() : Helper.DefaultAlbumCover;
                switch (Mode)
                {
                    case MediaControlMode.Main:
                        MainMediaControlGrid.Background = isThumbnail ? await thumbnail.GetDisplayColor() : ColorHelper.HighlightBrush;
                        break;
                    case MediaControlMode.Full:
                        FullAlbumTextBlock.Text = music.Album;
                        break;
                    case MediaControlMode.Mini:
                        break;
                }
                try
                {
                    await Helper.UpdateTile(thumbnail, music);
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

        public static void AddMusicRequestListener(MusicRequestListener listener)
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
            ShuffleButton.SetToolTip(MoreShuffleButton.Label = Helper.Localize($"Shuffle: " + (isChecked ? "Enabled" : "Disabled")));
            RepeatButton.SetToolTip(MoreRepeatButton.Label = Helper.Localize("Repeat: Disabled"));
            RepeatOneButton.SetToolTip(MoreRepeatOneButton.Label = Helper.Localize("Repeat One: Disabled"));
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
            ShuffleButton.SetToolTip(MoreShuffleButton.Label = Helper.Localize("Shuffle: Disabled"));
            RepeatButton.SetToolTip(MoreRepeatButton.Label = Helper.Localize("Repeat: " + (isChecked ? "Enabled" : "Disabled")));
            RepeatOneButton.SetToolTip(MoreRepeatOneButton.Label = Helper.Localize("Repeat One: Disabled"));
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
            ShuffleButton.SetToolTip(MoreShuffleButton.Label = Helper.Localize("Shuffle: Disabled"));
            RepeatButton.SetToolTip(MoreRepeatButton.Label = Helper.Localize("Repeat: Disabled"));
            RepeatOneButton.SetToolTip(MoreRepeatOneButton.Label = Helper.Localize("Repeat One: " + (isChecked ? "Enabled" : "Disabled")));
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
            if (isMuted)
            {
                VolumeButton.Content = "\uE74F";
            }
            else
            {
                VolumeButton.Content = Helper.GetVolumeIcon(VolumeSlider.Value);
            }
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
            MediaHelper.Player.IsMuted = false;
            double volume = e.NewValue / 100;
            MediaHelper.Player.Volume = volume;
            string icon = Helper.GetVolumeIcon(e.NewValue);
            if (VolumeButton != null) VolumeButton.Content = icon;
            Settings.settings.Volume = volume;
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

        private void MainMusicInfoGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (MediaHelper.CurrentMusic == null || MainPage.Instance == null) return;
            MainPage.Instance.Frame.Navigate(typeof(NowPlayingFullPage), null, new DrillInNavigationTransitionInfo());
        }

        private static SymbolIcon FullScreenIcon = new SymbolIcon(Symbol.FullScreen);
        private static SymbolIcon BackToWindowIcon = new SymbolIcon(Symbol.BackToWindow);
        private void SetFullScreen()
        {
            string text = Helper.Localize("Full Screen"), tooltip = Helper.Localize("Enter Full Screen Mode");
            MainMediaControlMoreFullScreenItem.Icon = FullScreenIcon;
            MainMediaControlMoreFullScreenItem.Label = text;
            MainMediaControlMoreFullScreenItem.SetToolTip(tooltip, false);
            MainMoreFullScreenItem.Icon = FullScreenIcon;
            MainMoreFullScreenItem.Text = text;
            MainMoreFullScreenItem.SetToolTip(tooltip, false);
            FullScreenItem.Icon = new SymbolIcon(Symbol.FullScreen);
            FullScreenItem.Text = text;
            FullScreenItem.SetToolTip(tooltip, false);
            FullScreenButton.Content = "\uE740";
            FullScreenButton.SetToolTip(tooltip, false);
        }

        private void SetExitFullScreen()
        {
            string text = Helper.Localize("Exit Full Screen"), tooltip = Helper.Localize("Exit Full Screen Mode");
            MainMediaControlMoreFullScreenItem.Icon = BackToWindowIcon;
            MainMediaControlMoreFullScreenItem.Label = text;
            MainMediaControlMoreFullScreenItem.SetToolTip(tooltip, false);
            MainMoreFullScreenItem.Icon = BackToWindowIcon;
            MainMoreFullScreenItem.Text = text;
            MainMoreFullScreenItem.SetToolTip(tooltip, false);
            FullScreenItem.Icon = new SymbolIcon(Symbol.BackToWindow);
            FullScreenItem.Text = text;
            FullScreenItem.SetToolTip(tooltip, false);
            FullScreenButton.Content = "\uE73F";
            FullScreenButton.SetToolTip(tooltip, false);
        }

        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationView.GetForCurrentView().IsFullScreenMode)
            {
                ApplicationView.GetForCurrentView().ExitFullScreenMode();
                SetFullScreen();
            }
            else if (ApplicationView.GetForCurrentView().TryEnterFullScreenMode())
            {
                SetExitFullScreen();
            }
        }

        private void MoreFullScreenItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FullScreenButton_Click(sender, null);
        }
        private async void MiniModeButton_Click(object sender, RoutedEventArgs e)
        {
            if (MediaHelper.CurrentMusic == null || !ApplicationView.GetForCurrentView().IsViewModeSupported(ApplicationViewMode.CompactOverlay)) return;
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
        public void Tick()
        {
            if (ShouldUpdate)
            {
                MediaSlider.Value = MediaHelper.Position;
            }
            Helper.UpdateToast();
        }

        public async void MusicSwitching(Music current, Music next, MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                if (reason == MediaPlaybackItemChangedReason.EndOfStream)
                {
                    Music before = current.Copy();
                    current.Played();
                    NotifyMusicModifiedListeners(before, current);
                }
                Settings.settings.Played(current);
                next.IsPlaying = true;
                MediaSlider.Value = 0;
                SetMusic(next);
                if (MainTitleTextBlock.IsScrolling) MainTitleTextBlock.StopScrolling();
                if (MainArtistTextBlock.IsScrolling) MainArtistTextBlock.StopScrolling();
                Helper.HideToast();
                // Use current instead of next to avoid showing toast on app launch
                if (current != null) Helper.ShowPauseToast(next);
            });
        }

        public async void MediaEnded()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (Settings.settings.Mode == PlayMode.Once)
                    PlayButton.Content = "\uE768";
                MediaSlider.Value = 0;
                Helper.HideToast();
            });
        }

        public void ClearMusic()
        {
            PauseMusic();
            CurrentMusic = null;
            AlbumCover.Source = Helper.DefaultAlbumCover;
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

        public void PathSet(string path)
        {
            //if (await CurrentMusic.GetStorageFileAsync() == null)
            //    ClearMusic();
        }

        public void Play()
        {
            PlayMusic();
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

        public void Pause()
        {
            PauseMusic();
        }

        public void MusicRemoved(int index, Music music, ICollection<Music> newCollection)
        {
            if (newCollection.Count == 0)
                ClearMusic();
        }

        private void MiniTitleTextBlock_PointerEntered(object sender, PointerRoutedEventArgs e)
        {

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

    public interface MusicRequestListener
    {
        void PlaylistRequested(ICollection<Music> playlist);
        void MusicInfoRequested(Music music);
        void LyricsRequested(Music music);
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SMPlayer.Models;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.FileProperties;
using Windows.UI.Core;
using System.Diagnostics;
using Windows.Media.Playback;
using Windows.Media.Core;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;
using Windows.ApplicationModel.Activation;
using Windows.UI.Core.Preview;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page, MediaControlListener
    {
        public static MainPage Instance
        {
            // This will return null when your current page is not a MainPage instance!
            get { return (Window.Current.Content as Frame).Content as MainPage; }
        }
        private bool ShouldUpdate = true, SliderClicked = false, hasLoaded = false;
        private static List<MusicControlListener> MusicControlListeners = new List<MusicControlListener>();

        public MainPage()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().BackRequested += (s, e) =>
            {
                if (NaviFrame.CanGoBack) NaviFrame.GoBack();
            };
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += (s, e) =>
            {
                Settings.Save();
            };
            this.NavigationCacheMode = NavigationCacheMode.Required;
            MediaHelper.AddMediaControlListener(this as MediaControlListener);
            SetMusic(Settings.settings.LastMusic);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (hasLoaded) return;
            MusicLibraryItem.IsSelected = true;
            await MediaHelper.Init();
            Settings settings = Settings.settings;
            if (!string.IsNullOrEmpty(settings.RootPath))
                Helper.CurrentFolder = await StorageFolder.GetFolderFromPathAsync(settings.RootPath);
            VolumeButton.Content = GetVolumeIcon(settings.Volume);
            VolumeSlider.Value = settings.Volume * 100;
            MainNavigationView.IsPaneOpen = settings.IsNavigationCollapsed;
            switch (settings.Mode)
            {
                case PlayMode.Once:
                    break;
                case PlayMode.Repeat:
                    RepeatButton.IsChecked = true;
                    break;
                case PlayMode.RepeatOne:
                    RepeatOneButton.IsChecked = true;
                    break;
                case PlayMode.Shuffle:
                    ShuffleButton.IsChecked = true;
                    break;
                default:
                    break;
            }
            hasLoaded = true;
        }

        public async void SetMusic(Music music)
        {
            if (music == null) return;
            AlbumCover.Source = await Helper.GetThumbnail(music);
            TitleTextBlock.Text = music.Name;
            ArtistTextBlock.Text = music.Artist;
            MediaSlider.Maximum = music.Duration;
            RightTimeTextBlock.Text = MusicDurationConverter.ToTime(music.Duration);
            if (music.Favorite) LikeMusic(false);
            else DislikeMusic(false);
            Settings.settings.LastMusic = music;
            foreach (var listener in MusicControlListeners)
                listener.MusicSet(music);
        }

        public void SetMusicAndPlay(Music music)
        {
            SetMusic(music);
            MediaHelper.SetMusic(music);
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
                MediaHelper.SetMusic(MediaHelper.CurrentPlayList[0]);
            }
            PlayButtonIcon.Glyph = "\uE769";
            MediaHelper.Play();
        }

        public void PauseMusic()
        {
            if (MediaHelper.CurrentMusic == null) return;
            PlayButtonIcon.Glyph = "\uE768";
            MediaHelper.Pause();
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
                MediaHelper.Player.IsMuted = false;
                VolumeButton.Content = GetVolumeIcon(VolumeSlider.Value);
            }
            else
            {
                MediaHelper.Player.IsMuted = true;
                VolumeButton.Content = "\uE74F";
            }
        }

        private void MainNavigationView_PaneClosing(NavigationView sender, object args)
        {
            NaviSearchBarItem.Visibility = Visibility.Collapsed;
            NaviSearchItem.Visibility = Visibility.Visible;
        }

        private void Open_Navigation()
        {
            NaviSearchBarItem.Visibility = Visibility.Visible;
            NaviSearchItem.Visibility = Visibility.Collapsed;
        }

        private void MainNavigationView_PaneOpening(NavigationView sender, object args)
        {
            Open_Navigation();
        }

        private void NaviSearchBar_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (NaviSearchBar.Text.Length > 0)
            {
                NaviFrame.Navigate(typeof(SearchPage));
                Helper.SetBackButtonVisibility(AppViewBackButtonVisibility.Visible);
            }
        }
        private void MainNavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked) NaviFrame.Navigate(typeof(SettingsPage));
            else
            {
                var item = (NavigationViewItem)MainNavigationView.SelectedItem;
                switch (item.Name)
                {
                    case "NaviSearchItem":
                        Open_Navigation();
                        MainNavigationView.IsPaneOpen = true;
                        NaviSearchBar.Focus(FocusState.Programmatic);
                        break;
                    case "MusicLibraryItem":
                        NaviFrame.Navigate(typeof(MusicLibraryPage));
                        break;
                    case "AlbumsItem":
                        NaviFrame.Navigate(typeof(AlbumsPage));
                        break;
                    case "ArtistsItem":
                        NaviFrame.Navigate(typeof(ArtistsPage));
                        break;
                    case "NowPlayingItem":
                        NaviFrame.Navigate(typeof(NowPlayingPage));
                        break;
                    case "RecentItem":
                        NaviFrame.Navigate(typeof(RecentPage));
                        break;
                    case "LocalItem":
                        NaviFrame.Navigate(typeof(LocalPage));
                        break;
                    case "PlaylistsItem":
                        NaviFrame.Navigate(typeof(PlaylistsPage));
                        break;
                    case "MyFavoritesItem":
                        NaviFrame.Navigate(typeof(MyFavorites));
                        break;
                    default:
                        return;
                }
            }
        }
        private void NaviFrame_Navigated(object sender, NavigationEventArgs e)
        {
            Helper.SetBackButtonVisibility(NaviFrame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed);
            switch (NaviFrame.CurrentSourcePageType.Name)
            {
                case "MusicLibraryPage":
                    MainNavigationView.SelectedItem = MusicLibraryItem;
                    break;
                case "ArtistsPage":
                    MainNavigationView.SelectedItem = ArtistsItem;
                    break;
                case "AlbumsPage":
                    MainNavigationView.SelectedItem = AlbumsItem;
                    break;
                case "NowPlayingPage":
                    MainNavigationView.SelectedItem = NowPlayingItem;
                    break;
                case "RecentPage":
                    MainNavigationView.SelectedItem = RecentItem;
                    break;
                case "LocalPage":
                    MainNavigationView.SelectedItem = LocalItem;
                    break;
                case "PlaylistsPage":
                    MainNavigationView.SelectedItem = PlaylistsItem;
                    break;
                case "MyFavoritesPage":
                    MainNavigationView.SelectedItem = MyFavoritesItem;
                    break;
                default:
                    return;
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
        
        public static void AddMusicControlListener(MusicControlListener listener)
        {
            MusicControlListeners.Add(listener);
        }

        private void SetMusicFavorite(bool favorite)
        {
            Music before = new Music(MediaHelper.CurrentMusic);
            MediaHelper.CurrentMusic.Favorite = favorite;
            foreach (var listener in MusicControlListeners)
                listener.MusicModified(before, MediaHelper.CurrentMusic);
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

        public void Tick()
        {
            if (ShouldUpdate)
            {
                MediaSlider.Value = MediaHelper.Position;
            }
            if (!Window.Current.Visible) Helper.UpdateToast();
        }
        private void NotifyListeners(Music before, Music after)
        {
            foreach (var listener in MusicControlListeners)
                listener.MusicModified(before, after);
        }

        public async void MusicSwitching(Music current, Music next)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                if (current == null) return;
                next.IsPlaying = true;
                if (!Window.Current.Visible) Helper.ShowToast(next);
                SetMusic(next);
                Music before = new Music(current);
                current.Played();
                NotifyListeners(before, current);
            });
        }

        public async void MediaEnded()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                MediaHelper.Timer.Stop();
                if (Settings.settings.Mode == PlayMode.Once)
                {
                    PlayButtonIcon.Glyph = "\uE768";
                    MediaSlider.Value = 0;
                }
            });
        }
    }

    public interface MusicControlListener
    {
        void MusicModified(Music before, Music after);
        void MusicSet(Music music);
    }
}
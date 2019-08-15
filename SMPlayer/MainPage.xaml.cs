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
        private bool ShouldPlay = false;
        private bool ShouldUpdate = true;
        private bool SliderClicked = true;
        private static Dictionary<string, MusicControlListener> MusicControlListeners = new Dictionary<string, MusicControlListener>();

        public MainPage()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().BackRequested += (s, e) =>
            {
                if (MainFrame.CanGoBack) MainFrame.GoBack();
            };
            MediaControl.Init();
            MediaControl.AddMediaControlListener(this);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MusicLibraryItem.IsSelected = true;
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
            if (settings.LastMusic != null)
                SetMusic(settings.LastMusic, false);
        }

        public async void SetMusic(Music music, bool play = true)
        {
            if (music == null) return;
            ShouldPlay = play;
            StorageFile file;
            try
            {
                file = await Helper.CurrentFolder.GetFileAsync(music.GetShortPath());
            }
            catch (FileNotFoundException)
            {
                return;
            }
            AlbumCover.Source = await Helper.GetThumbnail(file);
            TitleTextBlock.Text = music.Name;
            ArtistTextBlock.Text = music.Artist;
            MediaSlider.Maximum = music.Duration;
            RightTimeTextBlock.Text = MusicDurationConverter.ToTime(music.Duration);
            if (music.Favorite) LikeMusic(false);
            else DislikeMusic(false);
            if (ShouldPlay)
            {
                MediaSlider.Value = 0;
                PlayMusic();
            }
            if (Settings.settings.LastMusic != music)
            {
                Settings.settings.LastMusic = music;
                Settings.Save();
            }
            foreach (var listener in MusicControlListeners.Values)
                listener.MusicSet(music);
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
                SetMusic(MediaControl.CurrentPlayList[0]);
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
                MainFrame.Navigate(typeof(SearchPage));
                Helper.SetBackButtonVisibility(AppViewBackButtonVisibility.Visible);
            }
        }

        private void MainNavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked) MainFrame.Navigate(typeof(SettingsPage));
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
                        MainFrame.Navigate(typeof(MusicLibraryPage));
                        break;
                    case "AlbumsItem":
                        MainFrame.Navigate(typeof(AlbumsPage));
                        break;
                    case "ArtistsItem":
                        MainFrame.Navigate(typeof(ArtistsPage));
                        break;
                    case "NowPlayingItem":
                        break;
                    case "RecentItem":
                        MainFrame.Navigate(typeof(RecentPage));
                        break;
                    case "LocalItem":
                        MainFrame.Navigate(typeof(LocalPage));
                        break;
                    case "PlaylistsItem":
                        MainFrame.Navigate(typeof(PlaylistsPage));
                        break;
                    case "MyFavoritesItem":
                        MainFrame.Navigate(typeof(MyFavorites));
                        break;
                    default:
                        return;
                }
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
                MediaControl.SetPosition(0);
            }
            else SetMusic(MediaControl.PrevMusic());
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            SetMusic(MediaControl.NextMusic());
        }

        private void MediaSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            LeftTimeTextBlock.Text = MusicDurationConverter.ToTime((int)MediaSlider.Value);
            //if (SliderClicked)
            //{
            //    MediaControl.SetPosition(MediaSlider.Value);
            //    SliderClicked = false;
            //}
        }

        private void MediaSlider_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            MediaControl.SetPosition(MediaSlider.Value);
            ShouldUpdate = true;
            Debug.WriteLine("Completed");
        }

        private void MediaSlider_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            ShouldUpdate = false;
            Debug.WriteLine("Started");
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            Helper.SetBackButtonVisibility(MainFrame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed);
            // need to cancel NowPlaying
            switch (MainFrame.CurrentSourcePageType.Name)
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

        public void Tick()
        {
            if (ShouldUpdate)
            {
                MediaSlider.Value = MediaControl.Player.PlaybackSession.Position.TotalSeconds;
            }
        }

        public void MediaOpened()
        {
            if (ShouldPlay) MediaControl.Player.Play();
        }

        public void MediaEnded(Music before, Music after)
        {
            NotifyListeners(before, after);
            switch (Settings.settings.Mode)
            {
                case PlayMode.Once:
                    PlayButtonIcon.Glyph = "\uE768";
                    MediaSlider.Value = 0;
                    break;
                case PlayMode.Repeat:
                    break;
                case PlayMode.RepeatOne:
                    break;
                case PlayMode.Shuffle:
                    break;
                default:
                    break;
            }
        }

        public void MediaFailed(MediaPlayerFailedEventArgs args)
        {
            Debug.WriteLine("MediaFailed: " + args.ErrorMessage);
            //var dialog = new Windows.UI.Popups.MessageDialog(args.ErrorMessage);
            //await dialog.ShowAsync();
        }

        private void NotifyListeners(Music before, Music after)
        {
            foreach (var listener in MusicControlListeners.Values)
                listener.MusicModified(before, after);
        }

        private void MediaSlider_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Debug.Write("Tapped");
            MediaControl.SetPosition(MediaSlider.Value);
        }
    }

    public interface MusicControlListener
    {
        void MusicModified(Music before, Music after);
        void MusicSet(Music music);
    }
}
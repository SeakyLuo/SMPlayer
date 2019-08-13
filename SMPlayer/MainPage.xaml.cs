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

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage Instance
        {
            // This will return null when your current page is not a MainPage instance!
            get { return (Window.Current.Content as Frame).Content as MainPage; }
        }
        public static List<Music> CurrentPlayList;
        public static DispatcherTimer MusicTimer;
        public static bool ShouldPlay = false;
        private static bool NotDragging = true;
        private Dictionary<string, MediaControlListener> MusicModificationListeners = new Dictionary<string, MediaControlListener>();

        public MainPage()
        {
            this.InitializeComponent();
            MusicTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            MusicTimer.Tick += MusicTimer_Tick;
            SystemNavigationManager.GetForCurrentView().BackRequested += (s, e) =>
            {
                if (MainFrame.CanGoBack) MainFrame.GoBack();
            };
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MusicLibraryItem.IsSelected = true;
            Settings settings = Settings.settings;
            if (!string.IsNullOrEmpty(settings.RootPath))
                Helper.CurrentMusicFolder = await StorageFolder.GetFolderFromPathAsync(settings.RootPath);
            MainPageMediaElement.Volume = settings.Volume;
            VolumeButton.Content = GetVolumeIcon(settings.Volume);
            VolumeSlider.Value = settings.Volume;
            MainNavigationView.IsPaneOpen = settings.IsNavigationCollapsed;
            switch (settings.Mode)
            {
                case PlayMode.Once:
                    break;
                case PlayMode.Repeat:
                    RepeatButton.IsChecked = true;
                    MainPageMediaElement.IsLooping = false;
                    break;
                case PlayMode.RepeatOne:
                    RepeatOneButton.IsChecked = true;
                    MainPageMediaElement.IsLooping = true;
                    break;
                case PlayMode.Shuffle:
                    ShuffleButton.IsChecked = true;
                    MainPageMediaElement.IsLooping = false;
                    break;
                default:
                    break;
            }
            if (settings.LastMusic != null)
            {
                SetMusic(settings.LastMusic, false);
                ResetPlaylist();
            }
        }

        public async void SetMusic(Music music, bool play = true)
        {
            if (music == null) return;
            ShouldPlay = play;
            StorageFile file;
            try
            {
                file = await Helper.CurrentMusicFolder.GetFileAsync(music.GetShortPath());
            }
            catch (FileNotFoundException e)
            {
                return;
            }
            Helper.CurrentMusic = music;
            MainPageMediaElement.SetSource(await file.OpenAsync(FileAccessMode.Read), file.ContentType);
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
                Play();
            }
            if (Settings.settings.LastMusic != music)
            {
                Settings.settings.LastMusic = music;
                Settings.Save();
            }
            foreach (var listener in MusicModificationListeners.Values)
                listener.MusicSet(music);
        }

        private void ResetPlaylist()
        {
            Helper.CurrentPlayList = MusicLibraryPage.AllSongs.ToList();
            if (Settings.settings.Mode == PlayMode.Shuffle) Helper.ShuffleCurrentPlayList();
            Helper.CurrentMusicIndex = Helper.CurrentPlayList.IndexOf(Helper.CurrentMusic);
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            RepeatButton.IsChecked = false;
            RepeatOneButton.IsChecked = false;
            MainPageMediaElement.IsLooping = false;
            Settings.settings.Mode = ShuffleButton.IsChecked == true ? PlayMode.Shuffle : PlayMode.Once;
            ResetPlaylist();
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            ShuffleButton.IsChecked = false;
            RepeatOneButton.IsChecked = false;
            Settings.settings.Mode = RepeatButton.IsChecked == true ? PlayMode.Repeat : PlayMode.Once;
            MainPageMediaElement.IsLooping = false;
            ResetPlaylist();
        }

        private void RepeatOneButton_Click(object sender, RoutedEventArgs e)
        {
            ShuffleButton.IsChecked = false;
            RepeatButton.IsChecked = false;
            Settings.settings.Mode = RepeatOneButton.IsChecked == true ? PlayMode.RepeatOne : PlayMode.Once;
            MainPageMediaElement.IsLooping = !MainPageMediaElement.IsLooping;
            ResetPlaylist();
        }

        public void Play()
        {
            if (Helper.CurrentMusic == null)
            {
                if (MusicLibraryPage.AllSongs.Count == 0) return;
                if (Settings.settings.Mode == PlayMode.Shuffle) SetMusic(MusicLibraryPage.AllSongs[0]);
                else SetMusic(MusicLibraryPage.AllSongs[0]);
            }
            PlayButtonIcon.Glyph = "\uE769";
            MainPageMediaElement.Play();
            MusicTimer.Start();
        }

        public void Pause()
        {
            if (Helper.CurrentMusic == null) return;
            PlayButtonIcon.Glyph = "\uE768";
            MainPageMediaElement.Pause();
            MusicTimer.Stop();
        }

        private void Play_Click()
        {
            if (PlayButtonIcon.Glyph == "\uE768") Play();
            else Pause();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            Play_Click();
        }

        private void VolumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (VolumeButton.Content.ToString() == "\uE74F")
            {
                MainPageMediaElement.IsMuted = false;
                VolumeButton.Content = GetVolumeIcon(VolumeSlider.Value);
            }
            else
            {
                MainPageMediaElement.IsMuted = true;
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

        public void LoadPage(Type page)
        {
            MainFrame.Navigate(page);
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
            MainPageMediaElement.IsMuted = false;
            MainPageMediaElement.Volume = e.NewValue / 100;
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
        
        public void AddMusicModificationListener(string name, MediaControlListener listener)
        {
            MusicModificationListeners[name] = listener;
        }

        private void SetMusicFavorite(bool favorite)
        {
            Music before = new Music(Helper.CurrentMusic);
            Helper.CurrentMusic.Favorite = favorite;
            foreach (var listener in MusicModificationListeners.Values)
                listener.MusicModified(before, Helper.CurrentMusic);
        }

        private void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            if (LikeButtonIcon.Glyph == "\uEB51") LikeMusic();
            else DislikeMusic();
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (MediaSlider.Value > 5) MediaSlider.Value = 0;
            else SetMusic(Helper.PrevMusic());
        }

        private void NextMusic()
        {
            SetMusic(Helper.NextMusic());
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            NextMusic();
        }

        private void MainPageMediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (ShouldPlay) MainPageMediaElement.Play();
        }

        private void MainPageMediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            Music before = new Music(Helper.CurrentMusic);
            Helper.CurrentMusic.PlayCount += 1;
            foreach (var listener in MusicModificationListeners.Values)
                listener.MusicModified(before, Helper.CurrentMusic);
            switch (Settings.settings.Mode)
            {
                case PlayMode.Once:
                    PlayButtonIcon.Glyph = "\uE768";
                    MainPageMediaElement.Stop();
                    MediaSlider.Value = 0;
                    MusicTimer.Stop();
                    break;
                case PlayMode.Repeat:
                    NextMusic();
                    break;
                case PlayMode.RepeatOne:
                    break;
                case PlayMode.Shuffle:
                    NextMusic();
                    break;
                default:
                    break;
            }
        }

        private async void MainPageMediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            var dialog = new Windows.UI.Popups.MessageDialog(e.ErrorMessage);
            await dialog.ShowAsync();
        }

        private void MusicTimer_Tick(object sender, object e)
        {
            if (NotDragging) MediaSlider.Value = MainPageMediaElement.Position.TotalSeconds;
        }

        private void MediaSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            LeftTimeTextBlock.Text = MusicDurationConverter.ToTime((int)MediaSlider.Value);
        }

        private void MediaSlider_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            MainPageMediaElement.Position = TimeSpan.FromSeconds(MediaSlider.Value);
            NotDragging = true;
        }

        private void MediaSlider_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            NotDragging = false;
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
    }

    public interface MediaControlListener
    {
        void MusicModified(Music before, Music after);
        void MusicSet(Music music);
    }
}
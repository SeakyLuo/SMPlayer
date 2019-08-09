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
        public static Music currentPlayingMusic;
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MusicLibraryItem.IsSelected = true;
            MediaPlayer.Volume = Settings.settings.Volume;
            switch (Settings.settings.Mode)
            {
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
        }

        public async void SetMusic(Music music)
        {
            currentPlayingMusic = music;
            MediaPlayer.Source = new Uri(music.Path);
            var file = await StorageFile.GetFileFromPathAsync(music.Path);
            using (var thumbnail = await file.GetThumbnailAsync(ThumbnailMode.MusicView, 300))
            {
                if (thumbnail != null && thumbnail.Type == ThumbnailType.Image)
                {
                    var bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(thumbnail);
                    AlbumCover.Source = bitmapImage;
                }
            }
            TitleTextBlock.Text = music.Name;
            ArtistTextBlock.Text = music.Artist;
            ProgressBar.Maximum = music.Duration;
            RightTimeTextBlock.Text = MusicDurationConverter.ToTime(music.Duration);
            if (music.Favorite) LikeMusic();
            else DislikeMusic();
            Play();
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            RepeatButton.IsChecked = false;
            RepeatOneButton.IsChecked = false;
            Settings.settings.Mode = ShuffleButton.IsChecked == true ? PlayMode.Shuffle : PlayMode.Once;
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            ShuffleButton.IsChecked = false;
            RepeatOneButton.IsChecked = false;
            Settings.settings.Mode = RepeatButton.IsChecked == true ? PlayMode.Shuffle : PlayMode.Once;
        }

        private void RepeatOneButton_Click(object sender, RoutedEventArgs e)
        {
            ShuffleButton.IsChecked = false;
            RepeatButton.IsChecked = false;
            Settings.settings.Mode = RepeatOneButton.IsChecked == true ? PlayMode.Shuffle : PlayMode.Once;
        }

        private void Play()
        {
            PlayButtonIcon.Glyph = "\uE769";
            MediaPlayer.Play();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlayButtonIcon.Glyph == "\uE768")
            {
                Play();
            }
            else
            {
                PlayButtonIcon.Glyph = "\uE768";
                MediaPlayer.Pause();
            }
        }

        private void VolumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (VolumnButtonIcon.Glyph == "\uE767")
            {
                MediaPlayer.IsMuted = true;
                VolumnButtonIcon.Glyph = "\uE74F";
            }
            else
            {
                MediaPlayer.IsMuted = false;
                VolumnButtonIcon.Glyph = "\uE767";
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
                MainNavigationView.IsBackButtonVisible = NavigationViewBackButtonVisible.Visible;
            }
        }

        private void MainNavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                MainFrame.Navigate(typeof(SettingsPage));
                return;
            }
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
                case "HistoryItem":
                    MainFrame.Navigate(typeof(HistoryPage));
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

        private void MainNavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
            }
            if (!MainFrame.CanGoBack)
            {
                MainNavigationView.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
            }
        }

        private void ProgressBar_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {

        }
        private void VolumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            MediaPlayer.Volume = e.NewValue;
        }

        private void LikeMusic()
        {
            LikeButtonIcon.Glyph = "\uEB52";
            LikeButtonIcon.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
            currentPlayingMusic.Favorite = true;
        }

        private void DislikeMusic()
        {
            LikeButtonIcon.Glyph = "\uEB51";
            LikeButtonIcon.Foreground = new SolidColorBrush(Windows.UI.Colors.Black);
            currentPlayingMusic.Favorite = false;
        }

        private void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            // Save to MusicManager.AllSongs
            if (LikeButtonIcon.Glyph == "\uEB51") LikeMusic();
            else DislikeMusic();
        }
    }
}

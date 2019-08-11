﻿using System;
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
        public static StorageFolder CurrentMusicFolder;
        public static Music CurrentMusic;
        public static int CurrentMusicIndex = -1;
        public static List<Music> CurrentPlayList;
        public static DispatcherTimer MusicTimer;
        public static bool ShouldPlay = false;
        public static BitmapImage DefaultAlbumCover = new BitmapImage(new Uri("ms-appx:///[project-name]/Assets/music.png"));
        private static Random random = new Random();
        private Dictionary<string, MusicModificationListener> MusicModificationListeners = new Dictionary<string, MusicModificationListener>();

        public MainPage()
        {
            this.InitializeComponent();
            MusicTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            MusicTimer.Tick += MusicTimer_Tick;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MusicLibraryItem.IsSelected = true;
            Settings settings = Settings.settings;
            if (!string.IsNullOrEmpty(settings.RootPath))
                CurrentMusicFolder = await StorageFolder.GetFolderFromPathAsync(settings.RootPath);
            MediaPlayer.Volume = settings.Volume;
            VolumeSlider.Value = settings.Volume;
            MainNavigationView.IsPaneOpen = settings.IsNavigationCollapsed;
            switch (settings.Mode)
            {
                case PlayMode.Once:
                    break;
                case PlayMode.Repeat:
                    RepeatButton.IsChecked = true;
                    MediaPlayer.IsLooping = false;
                    break;
                case PlayMode.RepeatOne:
                    RepeatOneButton.IsChecked = true;
                    MediaPlayer.IsLooping = true;
                    break;
                case PlayMode.Shuffle:
                    ShuffleButton.IsChecked = true;
                    MediaPlayer.IsLooping = false;
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
                file = await CurrentMusicFolder.GetFileAsync(music.GetShortPath());
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
                return;
            }
            CurrentMusic = music;
            MediaPlayer.SetSource(await file.OpenAsync(FileAccessMode.Read), file.ContentType);
            using (var thumbnail = await file.GetThumbnailAsync(ThumbnailMode.MusicView, 300))
            {
                BitmapImage bitmapImage;
                if (thumbnail != null && thumbnail.Type == ThumbnailType.Image)
                {
                    bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(thumbnail);
                }
                else
                {
                    bitmapImage = DefaultAlbumCover;
                }
                AlbumCover.Source = bitmapImage;
            }
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
        }

        private void ResetPlaylist()
        {
            CurrentPlayList = MusicLibraryPage.AllSongs.ToList();
            if (Settings.settings.Mode == PlayMode.Shuffle) ShuffleCurrentPlayList();
            CurrentMusicIndex = CurrentPlayList.IndexOf(CurrentMusic);
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            RepeatButton.IsChecked = false;
            RepeatOneButton.IsChecked = false;
            MediaPlayer.IsLooping = false;
            Settings.settings.Mode = ShuffleButton.IsChecked == true ? PlayMode.Shuffle : PlayMode.Once;
            ResetPlaylist();
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            ShuffleButton.IsChecked = false;
            RepeatOneButton.IsChecked = false;
            Settings.settings.Mode = RepeatButton.IsChecked == true ? PlayMode.Shuffle : PlayMode.Once;
            MediaPlayer.IsLooping = false;
            ResetPlaylist();
        }

        private void RepeatOneButton_Click(object sender, RoutedEventArgs e)
        {
            ShuffleButton.IsChecked = false;
            RepeatButton.IsChecked = false;
            Settings.settings.Mode = RepeatOneButton.IsChecked == true ? PlayMode.Shuffle : PlayMode.Once;
            MediaPlayer.IsLooping = !MediaPlayer.IsLooping;
            ResetPlaylist();
        }

        public void Play()
        {
            if (CurrentMusic == null)
            {
                if (MusicLibraryPage.AllSongs.Count == 0) return;
                if (Settings.settings.Mode == PlayMode.Shuffle) SetMusic(MusicLibraryPage.AllSongs[0]);
                else SetMusic(MusicLibraryPage.AllSongs[0]);
            }
            PlayButtonIcon.Glyph = "\uE769";
            MediaPlayer.Play();
            MusicTimer.Start();
        }

        public void Pause()
        {
            if (CurrentMusic == null) return;
            PlayButtonIcon.Glyph = "\uE768";
            MediaPlayer.Pause();
            MusicTimer.Stop();
        }

        public static void ShuffleCurrentPlayList()
        {
            int n = CurrentPlayList.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                Music value = CurrentPlayList[k];
                CurrentPlayList[k] = CurrentPlayList[n];
                CurrentPlayList[n] = value;
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlayButtonIcon.Glyph == "\uE768") Play();
            else Pause();
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

        private void VolumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            MediaPlayer.Volume = e.NewValue;
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
        
        public void AddMusicModificationListener(string name, MusicModificationListener listener)
        {
            MusicModificationListeners[name] = listener;
        }

        private void SetMusicFavorite(bool favorite)
        {
            Music before = new Music(CurrentMusic);
            CurrentMusic.Favorite = favorite;
            foreach (var listener in MusicModificationListeners.Values)
                listener.MusicModified(before, CurrentMusic);
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
            }
            else
            {
                CurrentMusicIndex -= 1;
                if (CurrentMusicIndex < 0 && Settings.settings.Mode == PlayMode.Shuffle)
                {
                    CurrentMusic = null;
                    ShuffleCurrentPlayList();
                    CurrentMusicIndex = 0;
                }
                else
                {
                    CurrentMusicIndex %= CurrentPlayList.Count;
                }
                SetMusic(CurrentPlayList[CurrentMusicIndex]);
            }
        }

        private void NextMusic()
        {
            CurrentMusicIndex += 1;
            if (CurrentMusicIndex >= CurrentPlayList.Count && Settings.settings.Mode == PlayMode.Shuffle)
            {
                CurrentMusic = null;
                ShuffleCurrentPlayList();
                CurrentMusicIndex = 0;
            }
            else
            {
                CurrentMusicIndex %= CurrentPlayList.Count;
            }
            SetMusic(CurrentPlayList[CurrentMusicIndex]);
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            NextMusic();
        }

        private void MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            CurrentMusic.PlayCount += 1;
            switch (Settings.settings.Mode)
            {
                case PlayMode.Once:
                    PlayButtonIcon.Glyph = "\uE768";
                    MediaPlayer.Stop();
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

        private void MediaPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            NaviSearchBar.Text = e.ErrorMessage;
        }

        private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (ShouldPlay)
                MediaPlayer.Play();
        }

        private void MusicTimer_Tick(object sender, object e)
        {
            MediaSlider.Value = MediaPlayer.Position.TotalSeconds;
        }

        private void MediaSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            MediaPlayer.Position = TimeSpan.FromSeconds(e.NewValue);
            LeftTimeTextBlock.Text = MusicDurationConverter.ToTime((int)e.NewValue);
        }
    }

    public interface MusicModificationListener
    {
        void MusicModified(Music before, Music after);
    }
}

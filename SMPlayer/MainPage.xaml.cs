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
using System.Threading.Tasks;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page, MediaControlListener, AfterPathSetListener
    {
        public static MainPage Instance
        {
            // This will return null when your current page is not a MainPage instance!
            get { return (Window.Current.Content as Frame).Content as MainPage; }
        }
        private bool ShouldUpdate = true, SliderClicked = false, WindowVisible = true;
        private static List<MusicControlListener> MusicControlListeners = new List<MusicControlListener>();

        public MainPage()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().BackRequested += (s, e) =>
            {
                if (NaviFrame.CanGoBack) NaviFrame.GoBack();
            };
            Window.Current.VisibilityChanged += async (s, e) =>
            {
                WindowVisible = e.Visible;
                if (e.Visible)
                {

                }
                else
                {
                    await Dispatcher.RunIdleAsync((args) =>
                    {
                        // 2333
                        System.Threading.Thread.Sleep(2333);
                        if (!WindowVisible) MusicLibraryPage.CheckLibrary();
                    });
                }
            };
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            MediaHelper.AddMediaControlListener(this as MediaControlListener);
            SettingsPage.AddAfterPathSetListener(this as AfterPathSetListener);
            // Settings
            Settings settings = Settings.settings;
            VolumeButton.Content = Helper.GetVolumeIcon(settings.Volume);
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
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SwitchPage(string.IsNullOrEmpty(Settings.settings.LastPage) ? "MusicLibraryItem" : Settings.settings.LastPage);
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
            await Helper.SaveThumbnail(AlbumCover);
            MediaControlGrid.Background = await Helper.GetThumbnailMainColor();
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
        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            SetShuffle((bool)ShuffleButton.IsChecked);
        }
        public void SetShuffle(bool isShuffle)
        {
            RepeatButton.IsChecked = false;
            RepeatOneButton.IsChecked = false;
            MediaHelper.SetMode(isShuffle ? PlayMode.Shuffle : PlayMode.Once);
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
            PlayButton.Content = "\uE769";
            MediaHelper.Play();
        }

        public void PauseMusic()
        {
            PlayButton.Content = "\uE768";
            MediaHelper.Pause();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlayButton.Content.ToString() == "\uE768")
            {
                if (MediaHelper.CurrentMusic == null)
                {
                    if (MediaHelper.CurrentPlayList.Count == 0) return;
                    MediaHelper.MoveToMusic(MediaHelper.CurrentPlayList[0]);
                }
                PlayMusic();
            }
            else PauseMusic();
        }

        private void VolumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (VolumeButton.Content.ToString() == "\uE74F")
            {
                MediaHelper.Player.IsMuted = false;
                VolumeButton.Content = Helper.GetVolumeIcon(VolumeSlider.Value);
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

        private void SwitchPage(string name)
        {
            switch (name)
            {
                case "MusicLibraryItem":
                    NaviFrame.Navigate(typeof(MusicLibraryPage));
                    MusicLibraryItem.IsSelected = true;
                    break;
                case "AlbumsItem":
                    NaviFrame.Navigate(typeof(AlbumsPage));
                    AlbumsItem.IsSelected = true;
                    break;
                case "ArtistsItem":
                    NaviFrame.Navigate(typeof(ArtistsPage));
                    ArtistsItem.IsSelected = true;
                    break;
                case "NowPlayingItem":
                    NaviFrame.Navigate(typeof(NowPlayingPage));
                    NowPlayingItem.IsSelected = true;
                    break;
                case "RecentItem":
                    NaviFrame.Navigate(typeof(RecentPage));
                    RecentItem.IsSelected = true;
                    break;
                case "LocalItem":
                    NaviFrame.Navigate(typeof(LocalPage));
                    LocalItem.IsSelected = true;
                    break;
                case "PlaylistsItem":
                    NaviFrame.Navigate(typeof(PlaylistsPage));
                    MusicLibraryItem.IsSelected = true;
                    break;
                case "MyFavoritesItem":
                    NaviFrame.Navigate(typeof(MyFavorites));
                    MyFavoritesItem.IsSelected = true;
                    break;
                default:
                    return;
            }
            Settings.settings.LastPage = name;
        }
        private void MainNavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked) NaviFrame.Navigate(typeof(SettingsPage));
            else
            {
                var item = (NavigationViewItem)args.InvokedItemContainer;
                if (item == null)
                {
                    // Search
                    Open_Navigation();
                    MainNavigationView.IsPaneOpen = true;
                    NaviSearchBar.Focus(FocusState.Programmatic);
                }
                else
                {
                    SwitchPage(item.Name);
                }
            }
        }
        private void NaviFrame_Navigated(object sender, NavigationEventArgs e)
        {
            Helper.SetBackButtonVisibility(NaviFrame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed);
        }

        private void VolumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            MediaHelper.Player.IsMuted = false;
            double volume = e.NewValue / 100;
            MediaHelper.Player.Volume = volume;
            VolumeButton.Content = Helper.GetVolumeIcon(e.NewValue);            
            Settings.settings.Volume = volume;
        }

        public void LikeMusic(bool isClick = true)
        {
            LikeButton.Content = "\uEB52";
            LikeButton.Foreground = Helper.RedBrush;
            if (isClick) SetMusicFavorite(true);
        }

        public void DislikeMusic(bool isClick = true)
        {
            LikeButton.Content = "\uEB51";
            LikeButton.Foreground = Helper.WhiteBrush;
            if (isClick) SetMusicFavorite(false);
        }

        private void SetMusicFavorite(bool favorite)
        {
            Music before = MediaHelper.CurrentMusic.Copy();
            MediaHelper.CurrentMusic.Favorite = favorite;
            NotifyListeners(before, MediaHelper.CurrentMusic);
        }

        private void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            if (LikeButton.Content.ToString() == "\uEB51") LikeMusic();
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

        private void Played(Music music)
        {
            Music before = music.Copy();
            music.Played();
            NotifyListeners(before, music);
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

        private void MusicInfoGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(NowPlayingFullPage));
        }

        public void PathSet(string path)
        {
            PauseMusic();
            MediaSlider.Value = 0;
            AlbumCover.Source = Helper.DefaultAlbumCover;
            TitleTextBlock.Text = "";
            ArtistTextBlock.Text = "";
        }


        public void ShuffleChanged(IEnumerable<Music> newPlayList, bool isShuffle) { return; }
    }

    public interface MusicControlListener
    {
        void MusicModified(Music before, Music after);
    }
}
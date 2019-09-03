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
using Windows.UI.Xaml.Media.Animation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page, MediaControlContainer
    {
        public static MainPage Instance
        {
            // This will return null when your current page is not a MainPage instance!
            get { return (Window.Current.Content as Frame).Content as MainPage; }
        }

        public MainPage()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().BackRequested += (s, e) =>
            {
                if (Frame.CanGoBack)
                {
                    Frame.GoBack(new DrillInNavigationTransitionInfo());
                    Helper.SetBackButtonVisible(NaviFrame.CanGoBack);
                }
                else if (NaviFrame.CanGoBack)
                {
                    NaviFrame.GoBack();
                }
            };

            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            MainNavigationView.IsPaneOpen = Settings.settings.IsNavigationCollapsed;
            MainMediaControl.SetMusicGridInfoTapped((sender, args) =>
            {
                Frame.Navigate(typeof(NowPlayingFullPage), null, new DrillInNavigationTransitionInfo());
                Helper.SetBackButtonVisible(true);
            });
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MainMediaControl.Update();
            if (NaviFrame.SourcePageType != null && NaviFrame.SourcePageType.Name.StartsWith(Settings.settings.LastPage)) return;
            SwitchPage(Settings.settings.LastPage);
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
                Helper.SetBackButtonVisible(true);
            }
        }

        private void SwitchPage(string name)
        {
            switch (name)
            {
                case "Albums":
                    NaviFrame.Navigate(typeof(AlbumsPage));
                    break;
                case "Artists":
                    NaviFrame.Navigate(typeof(ArtistsPage));
                    break;
                case "NowPlaying":
                    NaviFrame.Navigate(typeof(NowPlayingPage));
                    break;
                case "Recent":
                    NaviFrame.Navigate(typeof(RecentPage));
                    break;
                case "Local":
                    NaviFrame.Navigate(typeof(LocalPage));
                    break;
                case "Playlists":
                    NaviFrame.Navigate(typeof(PlaylistsPage));
                    break;
                case "MyFavorites":
                    NaviFrame.Navigate(typeof(MyFavoritesPage));
                    break;
                case "MusicLibrary":
                default:
                    NaviFrame.Navigate(typeof(MusicLibraryPage));
                    break;
            }
            Settings.settings.LastPage = name;
        }
        private void MainNavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                if (NaviFrame.SourcePageType.Name != "SettingsPage")
                    NaviFrame.Navigate(typeof(SettingsPage));
            }
            else
            {
                var item = (NavigationViewItem)args.InvokedItemContainer;
                if (item.Name == "NaviSearchItem")
                {
                    // Search
                    Open_Navigation();
                    MainNavigationView.IsPaneOpen = true;
                    NaviSearchBar.Focus(FocusState.Programmatic);
                }
                else
                {
                    SwitchPage(item.Name.Substring(0, item.Name.Length - 4));
                }
            }
        }

        private void NaviFrame_Navigated(object sender, NavigationEventArgs e)
        {
            Helper.SetBackButtonVisible(NaviFrame.CanGoBack);
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
                case "SettingsPage":
                    MainNavigationView.SelectedItem = MainNavigationView.SettingsItem;
                    break;
                default:
                    return;
            }
        }

        public void SetMusicAndPlay(Music music)
        {
            MainMediaControl.SetMusicAndPlay(music);
        }

        public void PauseMusic()
        {
            MainMediaControl.PauseMusic();
        }

        public void SetShuffle(bool isShuffle)
        {
            MainMediaControl.SetShuffle(isShuffle);
        }
    }
}
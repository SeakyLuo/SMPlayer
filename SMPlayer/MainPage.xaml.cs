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
            Window.Current.SizeChanged += Current_SizeChanged;

            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            MainNavigationView.IsPaneOpen = Settings.settings.IsNavigationCollapsed;
            if (MainNavigationView.IsPaneOpen) MainNavigationView_PaneOpening(null, null);
            else MainNavigationView_PaneClosing(null, null);

            var coreTitleBar = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar;
            // Register a handler for when the size of the overlaid caption control changes.
            // For example, when the app moves to a screen with a different DPI.
            coreTitleBar.LayoutMetricsChanged += (sender, args) => UpdateTitleBarLayout(sender);
            // Register a handler for when the title bar visibility changes.
            // For example, when the title bar is invoked in full screen mode.
            coreTitleBar.IsVisibleChanged += (sender, args) => AppTitleBar.Visibility = sender.IsVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            bool isMinimal = e.Size.Width < 720;
            HeaderGrid.Visibility = isMinimal && Settings.settings.LastPage == "NowPlaying" ? Visibility.Collapsed : Visibility.Visible;
            if (!MainNavigationView.IsPaneOpen)
                if (isMinimal) CloseMinimal();
                else CloseNormal();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            TitleBarHelper.SetMainTitleBar();
            Window.Current.SetTitleBar(AppTitleBar);
            UpdateTitleBarLayout(Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar);

            MainMediaControl.Update();
            if (NaviFrame.SourcePageType != null && NaviFrame.SourcePageType.Name.StartsWith(Settings.settings.LastPage)) return;
            SwitchPage(Settings.settings.LastPage);
        }

        private void UpdateTitleBarLayout(Windows.ApplicationModel.Core.CoreApplicationViewTitleBar coreTitleBar)
        {
            // Get the size of the caption controls area and back button 
            // (returned in logical pixels), and move your content around as necessary.
            LeftPaddingColumn.Width = new GridLength(coreTitleBar.SystemOverlayLeftInset);
            RightPaddingColumn.Width = new GridLength(coreTitleBar.SystemOverlayRightInset);
            BackButton.Margin = new Thickness(0, 0, coreTitleBar.SystemOverlayRightInset, 0);

            // Update title bar control size as needed to account for system size changes.
            AppTitleBar.Height = coreTitleBar.Height;
        }

        private void SetBackButtonVisible(bool visible)
        {
            if (visible)
            {
                AppTitle.Margin = new Thickness(40, 0, 0, 0);
                BackButton.Visibility = Visibility.Visible;
            }
            else
            {
                AppTitle.Margin = new Thickness(0, 0, 0, 0);
                BackButton.Visibility = Visibility.Collapsed;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NaviFrame.CanGoBack) NaviFrame.GoBack();
        }

        private void FakeTogglePaneButton_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).Visibility = Visibility.Collapsed;
            MainNavigationView.IsPaneOpen = true;
        }
        private void SearchBar_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string text = NaviSearchBar.Text.Trim();
            if (text.Length > 0)
            {
                NaviFrame.Navigate(typeof(SearchPage), text);
                SetBackButtonVisible(true);
            }
            else if (HeaderSearchBar.Visibility == Visibility.Visible)
            {
                HideHeaderSearchBar();
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
        public void SetHeaderText(string header)
        {
            MainNavigationViewHeader.Text = header;
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
                SwitchPage(item.Name.Substring(0, item.Name.Length - 4));
            }
        }

        private void NaviFrame_Navigated(object sender, NavigationEventArgs e)
        {
            SetBackButtonVisible(NaviFrame.CanGoBack);
            switch (NaviFrame.CurrentSourcePageType.Name)
            {
                case "MusicLibraryPage":
                    SetHeaderText("Music Library");
                    HeaderGrid.Visibility = Visibility.Visible;
                    MainNavigationView.SelectedItem = MusicLibraryItem;
                    break;
                case "ArtistsPage":
                    SetHeaderText("Artists");
                    HeaderGrid.Visibility = Visibility.Visible;
                    MainNavigationView.SelectedItem = ArtistsItem;
                    break;
                case "AlbumsPage":
                    SetHeaderText("Albums");
                    HeaderGrid.Visibility = Visibility.Visible;
                    MainNavigationView.SelectedItem = AlbumsItem;
                    break;
                case "NowPlayingPage":
                    SetHeaderText("NowPlaying");
                    HeaderGrid.Visibility = MainNavigationView.DisplayMode == NavigationViewDisplayMode.Minimal ? Visibility.Collapsed : Visibility.Visible;
                    MainNavigationView.SelectedItem = NowPlayingItem;
                    break;
                case "RecentPage":
                    SetHeaderText("Recent");
                    HeaderGrid.Visibility = Visibility.Visible;
                    MainNavigationView.SelectedItem = RecentItem;
                    break;
                case "LocalPage":
                    HeaderGrid.Visibility = Visibility.Visible;
                    MainNavigationView.SelectedItem = LocalItem;
                    break;
                case "PlaylistsPage":
                    HeaderGrid.Visibility = Visibility.Collapsed;
                    MainNavigationView.SelectedItem = PlaylistsItem;
                    break;
                case "MyFavoritesPage":
                    SetHeaderText("My Favorites");
                    HeaderGrid.Visibility = Visibility.Visible;
                    MainNavigationView.SelectedItem = MyFavoritesItem;
                    break;
                case "SearchPage":
                    SetHeaderText("Search Result");
                    HeaderGrid.Visibility = Visibility.Visible;
                    break;
                case "SettingsPage":
                    SetHeaderText("Settings");
                    HeaderGrid.Visibility = Visibility.Visible;
                    MainNavigationView.SelectedItem = MainNavigationView.SettingsItem;
                    break;
                default:
                    Debug.WriteLine("Navigate to " + NaviFrame.CurrentSourcePageType.Name);
                    break;
            }
        }

        public void PauseMusic()
        {
            MainMediaControl.PauseMusic();
        }

        public void ShowLoading(string text)
        {
            MainLoadingControl.Text = text;
            MainLoadingControl.IsLoading = true;
        }

        public void UpdateLoadingText(string text)
        {
            MainLoadingControl.Text = text;
        }

        public void StopLoading()
        {
            MainLoadingControl.IsLoading = false;
        }

        private void MainNavigationView_PaneOpening(NavigationView sender, object args)
        {
            VisualStateManager.GoToState(this, "Open", true);
        }

        private void MainNavigationView_PaneClosing(NavigationView sender, NavigationViewPaneClosingEventArgs args)
        {
            VisualStateManager.GoToState(this, "Close", true);
            if (MainNavigationView.DisplayMode == NavigationViewDisplayMode.Minimal)
                CloseMinimal();
            else
                CloseNormal();
        }

        private void HeaderSearchButton_Click(object sender, RoutedEventArgs e)
        {
            MainNavigationViewHeader.Visibility = Visibility.Collapsed;
            HeaderSearchButton.Visibility = Visibility.Collapsed;
            HeaderSearchBar.Visibility = Visibility.Visible;
        }

        private void HideHeaderSearchBar()
        {
            MainNavigationViewHeader.Visibility = Visibility.Visible;
            HeaderSearchButton.Visibility = Visibility.Visible;
            HeaderSearchBar.Visibility = Visibility.Collapsed;
        }

        private void CloseMinimal()
        {
            AppTitle.Visibility = Visibility.Visible;
            FakeTogglePaneButton.Visibility = Visibility.Visible;
            AppTitleBorder.Background = Application.Current.Resources["MinimalTitleBarColor"] as Brush;
        }

        private void CloseNormal()
        {
            AppTitle.Visibility = Visibility.Collapsed;
            FakeTogglePaneButton.Visibility = Visibility.Collapsed;
            AppTitleBorder.Background = Application.Current.Resources["MainNavigationViewBackground"] as Brush;
        }

        private void HeaderNaviSearchBar_LosingFocus(UIElement sender, LosingFocusEventArgs args)
        {
            Debug.WriteLine("HeaderNaviSearchBar_LosingFocus");
        }

        private void HeaderNaviSearchBar_FocusDisengaged(Control sender, FocusDisengagedEventArgs args)
        {
            Debug.WriteLine("HeaderNaviSearchBar_FocusDisengaged");

        }

        private void HeaderNaviSearchBar_NoFocusCandidateFound(UIElement sender, NoFocusCandidateFoundEventArgs args)
        {
            Debug.WriteLine("HeaderNaviSearchBar_NoFocusCandidateFound");
        }
    }
}
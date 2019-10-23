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
    public sealed partial class MainPage : Page, NotificationContainer
    {
        public static MainPage Instance
        {
            // This will return null when your current page is not a MainPage instance!
            get => (Window.Current.Content as Frame).Content as MainPage;
        }
        public bool IsMinimal { get => MainNavigationView.DisplayMode == NavigationViewDisplayMode.Minimal; }
        public Brush TitleBarBackground
        {
            get => AppTitleBarBackground;
            set => AppTitleBarBackground = AppTitleBar.Background = HeaderGrid.Background = FakeTogglePaneButton.Background = value;
        }
        private Brush AppTitleBarBackground;
        public Brush TitleBarForeground
        {
            get => AppTitleBarForeground;
            set => AppTitleBarForeground = AppTitle.Foreground = MainNavigationViewHeader.Foreground = HeaderSearchButton.Foreground = FakeTogglePaneButton.Foreground = BackButton.Foreground = value;
        }
        private Brush AppTitleBarForeground;
        public LoadingControl Loader { get => MainLoadingControl; }
        public bool IsTitleBarColorful
        {
            get
            {
                if (NaviFrame.CurrentSourcePageType == null) return false;
                var page = NaviFrame.CurrentSourcePageType.Name;
                return page == "AlbumPage" || page == "MyFavoritesPage";
            }
        }
        private static bool PageUnset = true;

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            MainNavigationView.IsPaneOpen = Settings.settings.IsNavigationCollapsed;
            if (MainNavigationView.IsPaneOpen) MainNavigationView_PaneOpening(null, null);
            else MainNavigationView_PaneClosing(null, null);

            Window.Current.SizeChanged += Current_SizeChanged;
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
            bool isTitleBarColorful = IsTitleBarColorful;
            var page = NaviFrame.CurrentSourcePageType.Name;
            bool collapsed = ((page == "NowPlayingPage" || page == "RecentPage") && isMinimal) ||
                             page == "PlaylistsPage" ||
                             (isTitleBarColorful && !isMinimal);
            AppTitleBorder.Background = isMinimal ? ColorHelper.TransparentBrush : ColorHelper.MainNavigationViewBackground;
            TitleBarForeground = isMinimal && isTitleBarColorful ? ColorHelper.WhiteBrush : ColorHelper.BlackBrush;
            if (!isTitleBarColorful) TitleBarBackground = isMinimal ? ColorHelper.MinimalTitleBarColor : ColorHelper.TransparentBrush;
            HeaderGrid.Visibility = collapsed ? Visibility.Collapsed : Visibility.Visible;
            if (!isMinimal) HideHeaderSearchBar(Visibility.Collapsed);
            if (page == "SearchPage" || page == "SearchResultPage") SetHeaderText(SearchPage.GetSearchHeader(SearchPage.History.Peek(), IsMinimal));
            if (!MainNavigationView.IsPaneOpen)
                if (isMinimal) PaneCloseMinimal();
                else PaneCloseNormal();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsTitleBarColorful) TitleBarHelper.SetDarkTitleBar();
            else TitleBarHelper.SetMainTitleBar();
            Window.Current.SetTitleBar(AppTitleBar);
            UpdateTitleBarLayout(Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar);

            if (PageUnset)
            {
                SwitchPage(Settings.settings.LastPage);
                foreach (var playlist in Settings.settings.Playlists)
                    await playlist.SetDisplayItemAsync();
                PageUnset = false;
            }
        }

        private void UpdateTitleBarLayout(Windows.ApplicationModel.Core.CoreApplicationViewTitleBar coreTitleBar)
        {
            // Update title bar control size as needed to account for system size changes.
            AppTitleBar.Height = coreTitleBar.Height;
        }

        private Brush beforeOpenTitleBarBackground;

        private void MainNavigationView_PaneOpening(NavigationView sender, object args)
        {
            beforeOpenTitleBarBackground = TitleBarBackground;
            VisualStateManager.GoToState(this, "Open", true);
        }

        private void MainNavigationView_PaneClosing(NavigationView sender, NavigationViewPaneClosingEventArgs args)
        {
            VisualStateManager.GoToState(this, "Close", true);
            if (MainNavigationView.DisplayMode == NavigationViewDisplayMode.Minimal)
                PaneCloseMinimal();
            else
                PaneCloseNormal();
        }

        private void PaneCloseMinimal()
        {
            AppTitle.Visibility = Visibility.Visible;
            FakeTogglePaneButton.Visibility = Visibility.Visible;
            bool isTitleBarColorful = IsTitleBarColorful;
            AppTitle.Foreground = BackButton.Foreground = isTitleBarColorful ? ColorHelper.WhiteBrush : ColorHelper.BlackBrush;
            AppTitleBorder.Background = ColorHelper.TransparentBrush;
            if (beforeOpenTitleBarBackground == null || !isTitleBarColorful)
            {
                SetFakeTogglePaneButtonBackground();
            }
            else
            {
                TitleBarBackground = beforeOpenTitleBarBackground;
                beforeOpenTitleBarBackground = null;
            }
        }

        private void PaneCloseNormal()
        {
            AppTitle.Visibility = Visibility.Collapsed;
            AppTitleBorder.Background = ColorHelper.MainNavigationViewBackground;
            FakeTogglePaneButton.Visibility = Visibility.Collapsed;
        }

        private void SetFakeTogglePaneButtonBackground()
        {
            FakeTogglePaneButton.Background = NaviFrame.CurrentSourcePageType.Name.StartsWith("Playlists") ? ColorHelper.TransparentBrush : TitleBarBackground;
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
            string text = sender.Text.Trim();
            if (text.Length > 0)
            {
                NaviFrame.Navigate(typeof(SearchPage), text);
                SetBackButtonVisible(true);
                if (MainNavigationView.DisplayMode != NavigationViewDisplayMode.Expanded)
                    MainNavigationView.IsPaneOpen = false;
            }
            if (HeaderSearchBar.Visibility == Visibility.Visible)
            {
                HideHeaderSearchBar();
            }
        }

        public void NavigateToPage(Type page, object parameter = null, NavigationTransitionInfo infoOverride = null)
        {
            if (NaviFrame.CurrentSourcePageType == page) return;
            NaviFrame.Navigate(page, parameter, infoOverride);
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
            MainNavigationViewHeader.Text = Helper.Localize(header);
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
            var page = NaviFrame.CurrentSourcePageType.Name;
            switch (page)
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
                case "AlbumPage":
                    SetHeaderText("Album");
                    HeaderGrid.Visibility = MainNavigationView.DisplayMode == NavigationViewDisplayMode.Minimal ? Visibility.Visible : Visibility.Collapsed;
                    MainNavigationView.SelectedItem = null;
                    break;
                case "AlbumsPage":
                    SetHeaderText("Albums");
                    HeaderGrid.Visibility = Visibility.Visible;
                    MainNavigationView.SelectedItem = AlbumsItem;
                    break;
                case "NowPlayingPage":
                    SetHeaderText("Now Playing");
                    HeaderGrid.Visibility = MainNavigationView.DisplayMode == NavigationViewDisplayMode.Minimal ? Visibility.Collapsed : Visibility.Visible;
                    MainNavigationView.SelectedItem = NowPlayingItem;
                    break;
                case "RecentPage":
                    SetHeaderText("Recent");
                    HeaderGrid.Visibility = MainNavigationView.DisplayMode == NavigationViewDisplayMode.Minimal ? Visibility.Collapsed : Visibility.Visible;
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
                    SetHeaderText("");
                    HeaderGrid.Visibility = MainNavigationView.DisplayMode == NavigationViewDisplayMode.Minimal ? Visibility.Visible : Visibility.Collapsed;
                    MainNavigationView.SelectedItem = MyFavoritesItem;
                    break;
                case "SearchPage":
                    HeaderGrid.Visibility = Visibility.Visible;
                    MainNavigationView.SelectedItem = null;
                    break;
                case "SearchResultPage":
                    HeaderGrid.Visibility = Visibility.Visible;
                    MainNavigationView.SelectedItem = null;
                    break;
                case "SettingsPage":
                    SetHeaderText("Settings");
                    HeaderGrid.Visibility = Visibility.Visible;
                    MainNavigationView.SelectedItem = MainNavigationView.SettingsItem;
                    break;
                default:
                    Debug.WriteLine("Navigate to " + NaviFrame.CurrentSourcePageType.Name);
                    MainNavigationView.SelectedItem = null;
                    break;
            }
            if (IsTitleBarColorful)
            {
                TitleBarHelper.SetDarkTitleBar();
            }
            else
            {
                TitleBarHelper.SetMainTitleBar();
                TitleBarBackground = IsMinimal ? ColorHelper.MinimalTitleBarColor : ColorHelper.TransparentBrush;
                TitleBarForeground = ColorHelper.BlackBrush;
                SetFakeTogglePaneButtonBackground();
            }
        }

        private void HeaderSearchButton_Click(object sender, RoutedEventArgs e)
        {
            MainNavigationViewHeader.Visibility = Visibility.Collapsed;
            HeaderSearchButton.Visibility = Visibility.Collapsed;
            HeaderSearchBar.Visibility = Visibility.Visible;
        }

        private void HideHeaderSearchBar(Visibility searchButtonVisibility = Visibility.Visible)
        {
            MainNavigationViewHeader.Visibility = Visibility.Visible;
            HeaderSearchButton.Visibility = searchButtonVisibility;
            HeaderSearchBar.Visibility = Visibility.Collapsed;
        }

        public void ShowNotification(string message, int duration = 1500)
        {
            ShowResultInAppNotification.Content = message;
            ShowResultInAppNotification.Show(duration);
        }

        public async void ShowAddMusicResultNotification(ICollection<Music> playlist, Music target)
        {
            Helper.ShowAddMusicResultNotification(await MediaHelper.SetMusicAndPlay(playlist, target), target);
        }
        public async void ShowAddMusicResultNotification(ICollection<Music> playlist)
        {
            Helper.ShowAddMusicResultNotification(await MediaHelper.ShuffleAndPlay(playlist));
        }
    }
}
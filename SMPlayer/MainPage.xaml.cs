using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.Toolkit.Uwp.UI.Controls.TextToolbarFormats;
using SMPlayer.Controls;
using SMPlayer.Dialogs;
using SMPlayer.Helpers;
using SMPlayer.Models;
using SMPlayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page, IMainPageContainer
    {
        public static MainPage Instance
        {
            // This will return null when your current page is not a MainPage instance!
            get => (Window.Current?.Content as Frame)?.Content as MainPage;
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
        public LoadingControl Loader => MainLoadingControl;
        public bool IsTitleBarColorful
        {
            get
            {
                var page = NaviFrame.CurrentSourcePageType;
                return page != null && (page == typeof(AlbumPage) || page == typeof(MyFavoritesPage));
            }
        }
        private static bool switchPage = true, firstLoaded = true;
        public Type CurrentPage => NaviFrame?.CurrentSourcePageType;
        public Frame NavigationFrame => NaviFrame;
        private InAppNotification Notification => BottomMultiSelectCommandBar.IsVisible ? Row2ShowResultInAppNotification : ShowResultInAppNotification;
        private InAppNotificationWithButton ButtonedNotification => BottomMultiSelectCommandBar.IsVisible ? Row2ButtonedInAppNotification : ButtonedInAppNotification;

        public static List<IWindowResizeListener> WindowResizeListeners = new List<IWindowResizeListener>();
        private static List<Func<Task>> MainPageLoadedAsyncListeners = new List<Func<Task>>();
        private static List<Action> MainPageLoadedListeners = new List<Action>();
        public static void AddMainPageLoadedListener(Func<Task> action) { MainPageLoadedAsyncListeners.Add(action); }
        public static void AddMainPageLoadedListener(Action action) { MainPageLoadedListeners.Add(action); }
 
        public MainPage()
        {
            this.InitializeComponent();
            try
            {
                this.NavigationCacheMode = NavigationCacheMode.Enabled;
                bool isPaneOpen = Settings.settings == null || Settings.settings.IsNavigationCollapsed;
                MainNavigationView.IsPaneOpen = isPaneOpen;
                if (isPaneOpen) MainNavigationView_PaneOpening(null, null);
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
            catch (Exception e)
            {
                Log.Warn($"MainPage Init failed {e}");
            }
        }

        private void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            bool isMinimal = e.Size.Width < 720;
            bool isTitleBarColorful = IsTitleBarColorful;
            if (CurrentPage == null) return;
            bool collapsed = (CurrentPage == typeof(NowPlayingPage) && isMinimal) ||
                              CurrentPage == typeof(PlaylistsPage) ||
                              CurrentPage == typeof(LocalPage) ||
                              (CurrentPage == typeof(RecentPage) && !isMinimal) || 
                              (isTitleBarColorful && !isMinimal);
            AppTitleBorder.Background = isMinimal ? ColorHelper.TransparentBrush : ColorHelper.MainNavigationViewBackground;
            TitleBarForeground = isMinimal && isTitleBarColorful ? ColorHelper.WhiteBrush : ColorHelper.BlackBrush;
            if (!isTitleBarColorful) TitleBarBackground = isMinimal ? ColorHelper.MinimalTitleBarColor : ColorHelper.TransparentBrush;
            HeaderGrid.Visibility = collapsed ? Visibility.Collapsed : Visibility.Visible;
            if (!isMinimal) HideHeaderSearchBar(Visibility.Collapsed);
            if (CurrentPage == typeof(SearchPage) || CurrentPage == typeof(SearchResultPage))
                SetHeaderTextRaw(SearchPage.GetSearchHeader(SearchPage.History.Peek(), IsMinimal));
            if (!MainNavigationView.IsPaneOpen)
                if (isMinimal) PaneCloseMinimal();
                else PaneCloseNormal();
            foreach (var listener in WindowResizeListeners)
            {
                listener.Resized(e);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // 如果从磁贴唤醒，就不需要切换
            if ("True".Equals(e.Parameter) || "False".Equals(e.Parameter))
            {
                switchPage = false;
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsTitleBarColorful) TitleBarHelper.SetDarkTitleBar();
            else TitleBarHelper.SetMainTitleBar();
            Window.Current.SetTitleBar(AppTitleBar);
            UpdateTitleBarLayout(Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar);

            if (firstLoaded)
            {
                foreach (var listener in MainPageLoadedListeners) listener.Invoke();
                foreach (var listener in MainPageLoadedAsyncListeners) await listener.Invoke();

                if (switchPage)
                {
                    SwitchPage(Settings.settings.LastPage);
                    switchPage = false;
                }
                if (!string.IsNullOrEmpty(Settings.settings.LastReleaseNotesVersion) &&
                    Settings.settings.LastReleaseNotesVersion != Helper.AppVersion)
                {
                    SettingsPage.ShowReleaseNotes();
                }
                firstLoaded = false;

                await Task.Run(() =>
                {
                    List<FolderFile> badFiles = StorageService.FindInvalidFiles();
                    if (badFiles.IsEmpty()) return;
                    FolderUpdateResult result = new FolderUpdateResult();
                    foreach (var item in badFiles)
                    {
                        StorageService.RemoveFile(item);
                        result.RemoveFile(item.Path);
                    }
                    ShowDetailNotification(Helper.LocalizeMessage("FindInvalidFiles", badFiles.Count),
                                           async () => { await new FolderUpdateResultDialog().ShowAsync(result); },
                                           10000);
                });
                await Task.Run(() =>
                {
                    List<FolderTree> badFolders = StorageService.FindNoParentFolders();
                    if (badFolders.IsEmpty()) return;
                    foreach (var item in badFolders)
                    {
                        FolderTree parent = StorageService.FindFolderInfo(item.ParentPath);
                        if (parent == null)
                        {
                            Log.Warn("Cannot find parent for {0}", item.Path);
                            continue;
                        }
                        item.ParentId = parent.Id;
                        StorageService.UpdateFolder(item);
                    }
                });
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
            FakeTogglePaneButton.Background = CurrentPage is Type page && page.Name.StartsWith("Playlists") ? ColorHelper.TransparentBrush : TitleBarBackground;
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
            try
            {
                if (NaviFrame.CanGoBack)
                {
                    NaviFrame.GoBack();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GoBack failed {ex}");
            }
        }

        private void FakeTogglePaneButton_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).Visibility = Visibility.Collapsed;
            MainNavigationView.IsPaneOpen = true;
        }

        private void SearchBar_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            Search(sender.Text);
        }

        public void SetSearchBarText(string text)
        {
            NaviSearchBar.Text = text;
        }

        public void Search(string keyword)
        {
            if (keyword.Length == 0)
            {
                if (HeaderSearchBar.Visibility == Visibility.Visible)
                {
                    HideHeaderSearchBar();
                }
                else
                {
                    Helper.ShowNotification("SearchEmpty");
                }
                return;
            }
            string trimmed = keyword.Trim();
            Search(new SearchKeyword()
            {
                Text = trimmed.Length == 0 ? keyword : trimmed,
            });
        }

        public void Search(SearchKeyword keyword)
        {
            SettingsService.Search(keyword.Text);
            NaviFrame.Navigate(typeof(SearchPage), keyword);
            SetBackButtonVisible(true);
            if (MainNavigationView.DisplayMode != NavigationViewDisplayMode.Expanded)
                MainNavigationView.IsPaneOpen = false;
            if (HeaderSearchBar.Visibility == Visibility.Visible)
            {
                HideHeaderSearchBar();
            }
        }

        private void SearchBar_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            string text = args.SelectedItem.ToString();
            SetSearchBarText(text);
            Search(text);
        }

        public void NavigateToPage(Type page, object parameter = null, NavigationTransitionInfo infoOverride = null)
        {
            if (NaviFrame.CurrentSourcePageType == page) return;
            NaviFrame.Navigate(page, parameter, infoOverride);
        }

        private void SwitchPage(string name)
        {
            // 不知道为啥下面的不行……
            //if (Type.GetType(name + "Page") is Type type)
            //{
            //    NaviFrame.Navigate(type);
            //}
            //else
            //{
            //    NaviFrame.Navigate(typeof(MusicLibraryPage));
            //}
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
                case "RemotePlay":
                    NaviFrame.Navigate(typeof(RemotePlayPage));
                    break;
                case "MusicLibrary":
                default:
                    NaviFrame.Navigate(typeof(MusicLibraryPage));
                    break;
            }
            Settings.settings.LastPage = name;
            App.Save();
        }
        public void SetHeaderText(string header, params object[] args)
        {
            MainNavigationViewHeader.Text = Helper.LocalizeText(header, args);
        }
        public void SetHeaderTextRaw(string header)
        {
            MainNavigationViewHeader.Text = header;
        }
        private void MainNavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            BottomMultiSelectCommandBar.Hide();
            if (args.IsSettingsInvoked)
            {
                Type settingsPageType = typeof(SettingsPage);
                if (NaviFrame.SourcePageType != settingsPageType)
                    NaviFrame.Navigate(settingsPageType);
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
            var page = NaviFrame.CurrentSourcePageType;
            if (page == typeof(MusicLibraryPage))
            {
                HeaderGrid.Visibility = Visibility.Visible;
                MainNavigationView.SelectedItem = MusicLibraryItem;
            }
            else if (page == typeof(ArtistsPage))
            {
                HeaderGrid.Visibility = Visibility.Visible;
                MainNavigationView.SelectedItem = ArtistsItem;
            }
            else if (page == typeof(AlbumPage))
            {
                HeaderGrid.Visibility = MainNavigationView.DisplayMode == NavigationViewDisplayMode.Minimal ? Visibility.Visible : Visibility.Collapsed;
                MainNavigationView.SelectedItem = null;
            }
            else if (page == typeof(AlbumsPage))
            {
                HeaderGrid.Visibility = Visibility.Visible;
                MainNavigationView.SelectedItem = AlbumsItem;
            }
            else if (page == typeof(NowPlayingPage))
            {
                SetHeaderText("NowPlaying");
                HeaderGrid.Visibility = MainNavigationView.DisplayMode == NavigationViewDisplayMode.Minimal ? Visibility.Collapsed : Visibility.Visible;
                MainNavigationView.SelectedItem = NowPlayingItem;
            }
            else if (page == typeof(RecentPage))
            {
                SetHeaderText("Recent");
                HeaderGrid.Visibility = MainNavigationView.DisplayMode == NavigationViewDisplayMode.Minimal ? Visibility.Visible : Visibility.Collapsed;
                MainNavigationView.SelectedItem = RecentItem;
            }
            else if (page == typeof(LocalPage))
            {
                SetHeaderText("");
                HeaderGrid.Visibility = Visibility.Collapsed;
                MainNavigationView.SelectedItem = LocalItem;
            }
            else if (page == typeof(PlaylistsPage))
            {
                HeaderGrid.Visibility = Visibility.Collapsed;
                MainNavigationView.SelectedItem = PlaylistsItem;
            }
            else if (page == typeof(MyFavoritesPage))
            {
                SetHeaderText("");
                HeaderGrid.Visibility = MainNavigationView.DisplayMode == NavigationViewDisplayMode.Minimal ? Visibility.Visible : Visibility.Collapsed;
                MainNavigationView.SelectedItem = MyFavoritesItem;
            }
            else if (page == typeof(SearchPage))
            {
                HeaderGrid.Visibility = Visibility.Visible;
                MainNavigationView.SelectedItem = null;
            }
            else if (page == typeof(SearchResultPage))
            {
                HeaderGrid.Visibility = Visibility.Visible;
                MainNavigationView.SelectedItem = null;
            }
            //else if (page == typeof(RemotePlayPage))
            //{
            //    SetHeaderText("RemotePlay");
            //    HeaderGrid.Visibility = Visibility.Visible;
            //    MainNavigationView.SelectedItem = RemotePlayItem;
            //}
            else if (page == typeof(SettingsPage))
            {
                SetHeaderText("Settings");
                HeaderGrid.Visibility = Visibility.Visible;
                MainNavigationView.SelectedItem = MainNavigationView.SettingsItem;
            }
            else
            {
                HeaderGrid.Visibility = Visibility.Collapsed;
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

        public void ShowNotification(string message, int duration = 2000)
        {
            Notification.Show(message, duration);
        }

        public void ShowDetailNotification(string message, Action showDetail, int duration = 5000)
        {
            ShowButtonedNotification(message, Helper.LocalizeText("Detail"), _ => showDetail.Invoke(), duration);
        }

        public void ShowButtonedNotification(string message, string buttonText, Action<InAppNotificationWithButton> buttonAction, int duration = 5000)
        {
            ButtonedNotification.Show(message, buttonText, buttonAction, duration);
        }

        public void ShowButtonedNotification(string message, string button1, Action<InAppNotificationWithButton> action1, string button2, Action<InAppNotificationWithButton> action2, int duration = 5000)
        {
            ButtonedNotification.Show(message, button1, action1, button2, action2, duration);
        }

        public void ShowMultiSelectCommandBar(MultiSelectCommandBarOption option = null)
        {
            //TestCommandBar.Show();
            BottomMultiSelectCommandBar.Show(option);
        }

        public MultiSelectCommandBar GetMultiSelectCommandBar()
        {
            return BottomMultiSelectCommandBar;
        }

        public void CancelMultiSelectCommandBar()
        {
            BottomMultiSelectCommandBar.Hide();
        }

        public void SetMultiSelectListener(IMultiSelectListener listener)
        {
            BottomMultiSelectCommandBar.MultiSelectListener = listener;
        }

        public MediaElement GetMediaElement()
        {
            return mediaElement;
        }

        public MediaControl GetMediaControl()
        {
            return MainMediaControl;
        }

    }

    public interface IWindowResizeListener
    {
        void Resized(WindowSizeChangedEventArgs e);
    }
}
using SMPlayer.Models;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MyFavoritesPage : Page, PlaylistScrollListener
    {
        private ScrollDirection direction;
        public MyFavoritesPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            MyFavoritesPlaylistControl.HeaderedPlaylist.ScrollListener = this;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SortByButton.Label = Helper.LocalizeMessage("Sort By " + MyFavoritesPlaylistControl.CurrentPlaylist.Criterion.ToStr());
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode != NavigationMode.Back)
                await MyFavoritesPlaylistControl.SetPlaylist(Settings.settings.MyFavorites);
            MainPage.Instance.TitleBarBackground = MyFavoritesPlaylistControl.HeaderBackground;
            MainPage.Instance.TitleBarForeground = MainPage.Instance.IsMinimal ? ColorHelper.WhiteBrush : ColorHelper.BlackBrush;
        }
        private void SortByButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var playlist = MyFavoritesPlaylistControl.CurrentPlaylist;
            MenuFlyoutHelper.SetPlaylistSortByMenu(sender, playlist);
        }

        public void Scrolled(double before, double after)
        {
            if (after > before + 3)
            {
                // scroll down
                if (direction != ScrollDirection.Down)
                {
                    direction = ScrollDirection.Down;
                    ShowFooterAnimation.Begin();
                }
            }
            else if (after < before - 3)
            {
                // scroll up
                if (direction != ScrollDirection.Up)
                {
                    direction = ScrollDirection.Up;
                    HideFooterAnimation.Begin();
                }
            }
            else
            {
                direction = ScrollDirection.None;
            }
        }
    }
}

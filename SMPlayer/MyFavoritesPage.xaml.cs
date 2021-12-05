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
    public sealed partial class MyFavoritesPage : Page, IMusicEventListener
    {
        public MyFavoritesPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            Settings.AddMusicEventListener(this);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode != NavigationMode.Back)
                await MyFavoritesPlaylistControl.SetPlaylist(Settings.settings.MyFavorites);
            MainPage.Instance.TitleBarBackground = MyFavoritesPlaylistControl.HeaderBackground;
            MainPage.Instance.TitleBarForeground = MainPage.Instance.IsMinimal ? ColorHelper.WhiteBrush : ColorHelper.BlackBrush;
        }

        void IMusicEventListener.Added(Music music)
        {
        }

        async void IMusicEventListener.Liked(Music music, bool isFavorite)
        {
            await MyFavoritesPlaylistControl.SetPlaylist(Settings.settings.MyFavorites);
        }

        void IMusicEventListener.Modified(Music before, Music after)
        {
        }

        void IMusicEventListener.Removed(Music music)
        {
        }
    }
}

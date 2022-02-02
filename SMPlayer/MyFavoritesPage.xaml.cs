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
                await MyFavoritesPlaylistControl.SetPlaylist(Settings.MyFavoritesPlaylist);
            MainPage.Instance.TitleBarBackground = MyFavoritesPlaylistControl.HeaderBackground;
            MainPage.Instance.TitleBarForeground = MainPage.Instance.IsMinimal ? ColorHelper.WhiteBrush : ColorHelper.BlackBrush;
        }

        async void IMusicEventListener.Execute(Music music, MusicEventArgs args)
        {
            switch (args.EventType)
            {
                case MusicEventType.Add:
                    break;
                case MusicEventType.Remove:
                    break;
                case MusicEventType.Like:
                    await MyFavoritesPlaylistControl.SetPlaylist(Settings.MyFavoritesPlaylist);
                    break;
                case MusicEventType.Modify:
                    break;
            }
        }
    }
}

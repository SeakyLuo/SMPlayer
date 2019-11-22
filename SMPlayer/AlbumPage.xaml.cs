using SMPlayer.Models;
using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class AlbumPage : Page
    {
        private NavigationMode navigationMode;
        private object parameter;
        public AlbumPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            navigationMode = e.NavigationMode;
            parameter = e.Parameter;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (navigationMode == NavigationMode.Back) return;
            Playlist playlist = null;
            if (parameter is AlbumView album)
            {
                playlist = album.ToPlaylist();
                if (string.IsNullOrEmpty(playlist.Name)) playlist.Name = Helper.LocalizeMessage("UnknownAlbum");
                AlbumPlaylistControl.SetPlaylistInfo(album.Artist);
            }
            else if (parameter is Playlist)
                playlist = (Playlist)parameter;
            else if (parameter is string albumText)
            {
                int index = albumText.IndexOf(Helper.StringConcatenationFlag);
                string albumName = albumText.Substring(0, index), albumArtist = albumText.Substring(index + Helper.StringConcatenationFlag.Length);
                playlist = new Playlist(albumName, MusicLibraryPage.AllSongs.Where(m => m.Album == albumName && m.Artist == albumArtist));
                AlbumPlaylistControl.SetPlaylistInfo(albumArtist);
            }
            await AlbumPlaylistControl.SetPlaylist(playlist);
            MainPage.Instance.TitleBarBackground = AlbumPlaylistControl.HeaderBackground;
            MainPage.Instance.TitleBarForeground = MainPage.Instance.IsMinimal ? ColorHelper.WhiteBrush : ColorHelper.BlackBrush;
        }
    }
}

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
                int index = albumText.IndexOf("+++");
                string albumName = albumText.Substring(0, index), albumArtist = albumText.Substring(index + 3);
                playlist = new Playlist(albumName, MusicLibraryPage.AllSongs.Where((m) => m.Album == albumName && m.Artist == albumArtist));
                AlbumPlaylistControl.SetPlaylistInfo(albumArtist);
            }
            await AlbumPlaylistControl.SetPlaylist(playlist);
            MainPage.Instance.TitleBarBackground = AlbumPlaylistControl.HeaderBackground;
            MainPage.Instance.TitleBarForeground = MainPage.Instance.IsMinimal ? ColorHelper.WhiteBrush : ColorHelper.BlackBrush;
        }
    }
}

using Microsoft.Toolkit.Uwp.UI.Controls.TextToolbarSymbols;
using SMPlayer.Helpers;
using SMPlayer.Models;
using SMPlayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public static AlbumPage Instance { get => MainPage.Instance.NavigationFrame.Content as AlbumPage; }
        private NavigationMode navigationMode;
        private object targetAlbum;
        public AlbumPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            navigationMode = e.NavigationMode;
            targetAlbum = e.Parameter;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (navigationMode == NavigationMode.Back) return;
            LoadAlbum(targetAlbum);
        }

        public async void LoadAlbum(object targetAlbum)
        {
            Playlist playlist = null;
            if (targetAlbum is AlbumView album)
            {
                playlist = album.ToPlaylist();
            }
            else if (targetAlbum is Playlist albumPlaylist)
            {
                playlist = albumPlaylist;
            }
            else if (targetAlbum is string albumText)
            {
                int index = albumText.IndexOf(TileHelper.StringConcatenationFlag);
                string albumName = index == -1 ? albumText : albumText.Substring(0, index);
                AlbumView albumView = new AlbumView(albumName, MusicService.SelectByAlbum(albumName), false);
                playlist = albumView.ToPlaylist();
            }
            await AlbumPlaylistControl.SetPlaylist(playlist);
            MainPage.Instance.TitleBarBackground = AlbumPlaylistControl.HeaderBackground;
            MainPage.Instance.TitleBarForeground = MainPage.Instance.IsMinimal ? ColorHelper.WhiteBrush : ColorHelper.BlackBrush;
        }
    }
}

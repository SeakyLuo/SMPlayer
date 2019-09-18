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
        public AlbumPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Playlist playlist = null;
            bool isAlbum = e.Parameter is AlbumView;
            if (isAlbum)
            {
                var album = e.Parameter as AlbumView;
                playlist = new Playlist(album.Name, album.Songs);
                AlbumPlaylistControl.SetPlaylistInfo(album.Artist);
            }
            else if (e.Parameter is Playlist)
                playlist = e.Parameter as Playlist;
            TitleBarHelper.SetDarkTitleBar();
            await AlbumPlaylistControl.SetMusicCollection(playlist);
            MainPage.Instance.SetTitleBarBackground(AlbumPlaylistControl.HeaderBackground);
        }
    }
}

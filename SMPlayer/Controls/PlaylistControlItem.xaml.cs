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

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SMPlayer.Controls
{
    public sealed partial class PlaylistControlItem : UserControl
    {
        public PlaylistControlItem()
        {
            this.InitializeComponent();
        }

        private void Album_Click(object sender, RoutedEventArgs e)
        {
            if (NowPlayingFullPage.Instance != null) NowPlayingFullPage.Instance.GoBack();
            var button = (sender as HyperlinkButton);
            var album = button.Content.ToString();
            var playlist = new Models.Playlist(album);
            foreach (var music in MusicLibraryPage.AllSongs)
                if (music.Album == album)
                    playlist.Add(music);
            MainPage.Instance.NavigateToPage(typeof(AlbumPage), playlist);
        }
        private void Artist_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

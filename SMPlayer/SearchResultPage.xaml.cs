using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SearchResultPage : Page
    {
        private string keyword;
        public ObservableCollection<Playlist> Artists = new ObservableCollection<Playlist>();
        public ObservableCollection<AlbumView> Albums = new ObservableCollection<AlbumView>();
        public ObservableCollection<Music> Songs = new ObservableCollection<Music>();
        public ObservableCollection<AlbumView> Playlists = new ObservableCollection<AlbumView>();
        public SearchResultPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        private void AlbumsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(AlbumPage), e.ClickedItem);
        }

        private void PlaylistGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(PlaylistsPage), e.ClickedItem);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.Back)
            {
                MainPage.Instance.SetHeaderText(SearchPage.GetSearchHeader(SearchPage.History.Peek(), MainPage.Instance.IsMinimal));
                return;
            }
            keyword = SearchPage.History.Peek();
            LoadingProgress.IsActive = true;
            Artists.Clear();
            Albums.Clear();
            Songs.Clear();
            Playlists.Clear();
            switch ((string)e.Parameter)
            {
                case "Artists":
                    foreach (var group in MusicLibraryPage.AllSongs.Where((m) => SearchPage.IsTargetArtist(m, keyword)).GroupBy((m) => m.Artist).OrderBy((g) => g.Key))
                        Artists.Add(new Playlist(group.Key, group));
                    break;
                case "Albums":
                    foreach (var group in MusicLibraryPage.AllSongs.Where((m) => SearchPage.IsTargetAlbum(m, keyword)).GroupBy((m) => m.Album).OrderBy((g) => g.Key))
                    {
                        Music music = group.ElementAt(0);
                        Albums.Add(new AlbumView(music.Album, music.Artist, group.OrderBy((m) => m.Name).ThenBy((m) => m.Artist)));
                    }
                    break;
                case "Songs":
                    foreach (var music in MusicLibraryPage.AllSongs.Where((m) => SearchPage.IsTargetMusic(m, keyword)).OrderBy((m) => m.Name).ThenBy((m) => m.Artist))
                        Songs.Add(music);
                    break;
                case "Playlists":
                    foreach (var playlist in Settings.settings.Playlists.Where((p) => SearchPage.IsTargetPlaylist(p, keyword)).OrderBy((p) => p.Name))
                        Playlists.Add(playlist.ToAlbumView());
                    break;
            }
            LoadingProgress.IsActive = false;
        }
    }
}

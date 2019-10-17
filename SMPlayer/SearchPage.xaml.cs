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
    public sealed partial class SearchPage : Page
    {
        public static Stack<string> History = new Stack<string>();
        public ObservableCollection<Playlist> Artists = new ObservableCollection<Playlist>();
        public ObservableCollection<AlbumView> Albums = new ObservableCollection<AlbumView>();
        public ObservableCollection<Music> Songs = new ObservableCollection<Music>();
        public ObservableCollection<AlbumView> Playlists = new ObservableCollection<AlbumView>();
        public const int ArtistLimit = 10, AlbumLimit = 10, SongLimit = 5, PlaylistLimit = 10;
        public SearchPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ArtistsViewAllButton.Visibility = SearchArtistView.GetFirstDescendantOfType<ScrollViewer>()?.ScrollableWidth == 0 ? Visibility.Collapsed : Visibility.Visible;
            AlbumsViewAllButton.Visibility = SearchAlbumView.GetFirstDescendantOfType<ScrollViewer>()?.ScrollableWidth == 0 ? Visibility.Collapsed : Visibility.Visible;
            PlaylistsViewAllButton.Visibility = SearchPlaylistView.GetFirstDescendantOfType<ScrollViewer>()?.ScrollableWidth == 0 ? Visibility.Collapsed : Visibility.Visible;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string keyword = e.Parameter as string;
            // User Search
            if (e.NavigationMode == NavigationMode.Back)
            {
                // Back to Search Page
                MainPage.Instance.SetHeaderText(GetSearchHeader(History.Peek(), MainPage.Instance.IsMinimal));
            }
            else
            {
                MainPage.Instance.SetHeaderText(GetSearchHeader(keyword, MainPage.Instance.IsMinimal));
                History.Push(keyword);
                SearchArtists(keyword);
                SearchAlbums(keyword);
                SearchSongs(keyword);
                SearchPlaylists(keyword);
                NoResultPanel.Visibility = Artists.Count == 0 && Albums.Count == 0 && Songs.Count == 0 && Playlists.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (e.NavigationMode == NavigationMode.Back)
                History.Pop();
        }

        public static bool IsTargetArtist(Music music, string keyword)
        {
            return music.Artist.ToLower().Contains(keyword);
        }
        public void SearchArtists(string keyword)
        {
            Artists.Clear();
            foreach (var group in MusicLibraryPage.AllSongs.Where((m) => IsTargetArtist(m, keyword)).GroupBy((m) => m.Artist))
            {
                Artists.Add(new Playlist(group.Key, group) { Artist = group.Key });
                if (Artists.Count == ArtistLimit) break;
            }
        }
        public static bool IsTargetAlbum(Music music, string keyword)
        {
            return music.Album.ToLower().Contains(keyword) || music.Artist.ToLower().Contains(keyword);
        }
        public void SearchAlbums(string keyword)
        {
            Albums.Clear();
            foreach (var group in MusicLibraryPage.AllSongs.Where((m) => IsTargetAlbum(m, keyword)).GroupBy((m) => m.Album))
            {
                Music music = group.ElementAt(0);
                Albums.Add(new AlbumView(music.Album, music.Artist, group.OrderBy((m) => m.Name).ThenBy((m) => m.Artist)));
                if (Albums.Count == AlbumLimit) break;
            }
        }
        public static bool IsTargetMusic(Music music, string keyword)
        {
            return music.Name.ToLower().Contains(keyword) || music.Album.ToLower().Contains(keyword) || music.Artist.ToLower().Contains(keyword);
        }

        public void SearchSongs(string keyword)
        {
            Songs.Clear();
            bool SongsViewAll = false;
            foreach (var music in MusicLibraryPage.AllSongs)
            {
                if (IsTargetMusic(music, keyword))
                {
                    if (SongsViewAll = Songs.Count == SongLimit) break;
                    Songs.Add(music);
                }
            }
            SongsViewAllButton.Visibility = SongsViewAll ? Visibility.Visible : Visibility.Collapsed;
        }
        public static bool IsTargetPlaylist(Playlist playlist, string keyword)
        {
            return playlist.Name.Contains(keyword) || playlist.Songs.Any((music) => IsTargetMusic(music, keyword));
        }

        public void SearchPlaylists(string keyword)
        {
            Playlists.Clear();
            foreach (var playlist in Settings.settings.Playlists)
            {
                if (IsTargetPlaylist(playlist, keyword))
                {
                    Playlists.Add(playlist.ToAlbumView());
                    if (Playlists.Count == PlaylistLimit) break;
                }
            }
        }

        public static string GetSearchHeader(string keyword, bool isMinimal)
        {
            string header = $"\"{keyword}\"";
            string localized = Helper.Localize("Search Result of");
            return isMinimal ? header : $"{localized} {header}";
        }

        private void SearchAlbumView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(AlbumPage), e.ClickedItem);
        }

        private void SearchPlaylistView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(PlaylistsPage), e.ClickedItem);
        }
        private void ArtistsViewAllButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(SearchResultPage), "Artists");
        }

        private void SearchArtistView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(ArtistsPage), e.ClickedItem);
        }

        private void AlbumsViewAllButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(SearchResultPage), "Albums");
        }
        private void SongsViewAllButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(SearchResultPage), "Songs");
        }

        private void PlaylistsViewAllButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(SearchResultPage), "Playlists");
        }
    }
}

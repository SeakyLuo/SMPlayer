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
        public bool ArtistsViewAll, AlbumsViewAll, SongsViewAll, PlaylistsViewAll;
        public const int ArtistLimit = 10, AlbumLimit = 10, SongLimit = 5, PlaylistLimit = 10;
        public SearchPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string text = e.Parameter as string;
            // User Search
            if (History.Count > 0 && text == History.Peek())
            {
                // Back to Search Page
                MainPage.Instance.SetHeaderText(GetSearchHeader(History.Peek(), MainPage.Instance.IsMinimal));
            }
            else
            {
                MainPage.Instance.SetHeaderText(GetSearchHeader(text, MainPage.Instance.IsMinimal));
                History.Push(text);
                SearchArtists(text);
                SearchAlbums(text);
                SearchSongs(text);
                SearchPlaylists(text);
                NoResultPanel.Visibility = Artists.Count == 0 && Albums.Count == 0 && Songs.Count == 0 && Playlists.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public static bool IsTargetArtist(Music music, string text)
        {
            return music.Artist.ToLower().Contains(text);
        }
        public void SearchArtists(string text)
        {
            Artists.Clear();
            ArtistsViewAll = false;
            foreach (var group in MusicLibraryPage.AllSongs.Where((m) => IsTargetArtist(m, text)).GroupBy((m) => m.Artist).OrderBy((g) => g.Key))
            {
                if (ArtistsViewAll = Artists.Count == ArtistLimit) break;
                Artists.Add(new Playlist(group.Key, group));
            }
            ArtistsViewAllButton.Visibility = ArtistsViewAll ? Visibility.Visible : Visibility.Collapsed;
        }
        public static bool IsTargetAlbum(Music music, string text)
        {
            return music.Album.ToLower().Contains(text) || music.Artist.ToLower().Contains(text);
        }
        public void SearchAlbums(string text)
        {
            Albums.Clear();
            AlbumsViewAll = false;
            foreach (var group in MusicLibraryPage.AllSongs.Where((m) => IsTargetAlbum(m, text)).GroupBy((m) => m.Album))
            {
                if (AlbumsViewAll = Albums.Count == AlbumLimit) break;
                Music music = group.ElementAt(0);
                Albums.Add(new AlbumView(music.Album, music.Artist, group.OrderBy((m) => m.Name).ThenBy((m) => m.Artist)));
            }
            AlbumsViewAllButton.Visibility = AlbumsViewAll ? Visibility.Visible : Visibility.Collapsed;
        }
        public static bool IsTargetMusic(Music music, string text)
        {
            return music.Name.ToLower().Contains(text) || music.Album.ToLower().Contains(text) || music.Artist.ToLower().Contains(text);
        }

        public void SearchSongs(string text)
        {
            Songs.Clear();
            SongsViewAll = false;
            foreach (var music in MusicLibraryPage.AllSongs)
            {
                if (IsTargetMusic(music, text))
                {
                    if (SongsViewAll = Songs.Count == SongLimit) break;
                    Songs.Add(music);
                }
            }
            SongsViewAllButton.Visibility = SongsViewAll ? Visibility.Visible : Visibility.Collapsed;
        }
        public static bool IsTargetPlaylist(Playlist playlist, string text)
        {
            return playlist.Name.Contains(text) || playlist.Songs.Any((music) => IsTargetMusic(music, text));
        }

        public void SearchPlaylists(string text)
        {
            Playlists.Clear();
            PlaylistsViewAll = false;
            foreach (var playlist in Settings.settings.Playlists)
            {
                if (IsTargetPlaylist(playlist, text))
                {
                    if (PlaylistsViewAll = Playlists.Count == PlaylistLimit) break;
                    Playlists.Add(playlist.ToAlbumView());
                }
            }
            PlaylistsViewAllButton.Visibility = ArtistsViewAll ? Visibility.Visible : Visibility.Collapsed;
        }

        public static string GetSearchHeader(string text, bool isMinimal)
        {
            string header = $"\"{text}\"";
            return isMinimal ? header : $"Search Result of {header}";
        }

        private void SearchArtistsView_ItemClick(object sender, ItemClickEventArgs e)
        {

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

        public static ICollection<Music> FindSongs(object data)
        {
            if (data is AlbumView album) return album.Songs;
            else if (data is Playlist playlist) return playlist.Songs;
            return null;
        }

        private void PlayAllButton_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as Button).DataContext;
            var songs = FindSongs(data);
            if (songs != null) MediaHelper.ShuffleAndPlay(songs);
        }
        private void AddToButton_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as Button).DataContext as AlbumView;
            var songs = FindSongs((sender as Button).DataContext);
            var helper = new MenuFlyoutHelper() { Data = songs, DefaultPlaylistName = data.Name };
            helper.GetAddToPlaylistsMenuFlyout().ShowAt(sender as FrameworkElement);
        }

        private void UserControl_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(sender as Control, "PointerOver", true);
        }

        private void UserControl_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(sender as Control, "Normal", true);
        }
    }
}

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
        public ObservableCollection<string> Artists = new ObservableCollection<string>();
        public ObservableCollection<AlbumView> Albums = new ObservableCollection<AlbumView>();
        public ObservableCollection<Music> Songs = new ObservableCollection<Music>();
        public ObservableCollection<Playlist> Playlists = new ObservableCollection<Playlist>();
        private bool ArtistsViewAll = false, AlbumsViewAll = false, SongsViewAll = false, PlaylistsViewAll = false;
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
            }
        }
        public static bool IsTargetArtist(Music music, string text)
        {
            return music.Name.ToLower().Contains(text) || music.Album.ToLower().Contains(text) || music.Artist.ToLower().Contains(text);
        }
        public void SearchArtists(string text)
        {
            Artists.Clear();
            foreach (var artist in MusicLibraryPage.AllSongs.Where((m) => IsTargetArtist(m, text)).Select((m) => m.Artist).ToHashSet().OrderBy((s) => s))
            {
                if (ArtistsViewAll = Artists.Count == ArtistLimit) break;
                Artists.Add(artist);
            }
        }
        public static bool IsTargetAlbum(Music music, string text)
        {
            return music.Album.ToLower().Contains(text) || music.Artist.ToLower().Contains(text);
        }
        public void SearchAlbums(string text)
        {
            Albums.Clear();
            foreach (var group in MusicLibraryPage.AllSongs.Where((m) => IsTargetAlbum(m, text)).GroupBy((m) => m.Album))
            {
                if (AlbumsViewAll = Albums.Count == AlbumLimit) break;
                Music music = group.ElementAt(0);
                Albums.Add(new AlbumView(music.Album, music.Artist, group.OrderBy((m) => m.Name).ThenBy((m) => m.Artist)));
            }
        }
        public static bool IsTargetMusic(Music music, string text)
        {
            return music.Name.ToLower().Contains(text) || music.Album.ToLower().Contains(text) || music.Artist.ToLower().Contains(text);
        }
        public void SearchSongs(string text)
        {
            Songs.Clear();
            foreach (var music in MusicLibraryPage.AllSongs)
            {
                if (IsTargetMusic(music, text))
                {
                    if (SongsViewAll = Songs.Count == SongLimit) break;
                    Songs.Add(music);
                }
            }
        }
        public static bool IsTargetPlaylist(Playlist playlist, string text)
        {
            return playlist.Name.Contains(text) || playlist.Songs.Any((music) => IsTargetMusic(music, text));
        }
        public async void SearchPlaylists(string text)
        {
            Playlists.Clear();
            foreach (var playlist in Settings.settings.Playlists)
            {
                if (IsTargetPlaylist(playlist, text))
                {
                    if (PlaylistsViewAll = Playlists.Count == PlaylistLimit) break;
                    playlist.DisplayItem = Songs.Count > 0 ? await Songs[0].GetMusicDisplayItemAsync() : MusicDisplayItem.DefaultItem;
                    Playlists.Add(playlist);
                }
            }
        }

        public static string GetSearchHeader(string text, bool isMinimal)
        {
            string header = $"\"{text}\"";
            return isMinimal ? header : $"Search Result of {header}";
        }

        private void ViewAllButton_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void SearchAlbumView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(AlbumPage), e.ClickedItem);
        }
    }
}

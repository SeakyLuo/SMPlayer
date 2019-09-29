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
        public List<string> Artists = new List<string>();
        public ObservableCollection<AlbumView> Albums = new ObservableCollection<AlbumView>();
        public ObservableCollection<Music> LimitedSongs = new ObservableCollection<Music>();
        public ObservableCollection<Music> Songs = new ObservableCollection<Music>();
        public List<Playlist> Playlists = new List<Playlist>();
        public SearchPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is string text)
            {
                // User Search
                MainPage.Instance.SetHeaderText(GetSearchHeader(text, MainPage.Instance.IsMinimal));
                History.Push(text);
                SearchArtists(text);
                SearchAlbums(text);
                SearchSongs(text);
                SearchPlaylists(text);
            }
            else
            {
                // Back to Search Page
                MainPage.Instance.SetHeaderText(GetSearchHeader(History.Pop(), MainPage.Instance.IsMinimal));
            }
        }

        public void SearchArtists(string text)
        {
            Artists = MusicLibraryPage.AllSongs.Where((m) => IsTargetArtist(m, text)).Select((m) => m.Artist).ToHashSet().OrderBy((s) => s).ToList();
        }

        public void SearchAlbums(string text)
        {
            Albums.Clear();
            foreach (var group in MusicLibraryPage.AllSongs.Where((m) => IsTargetAlbum(m, text)).GroupBy((m) => m.Album))
            {
                Music music = group.ElementAt(0);
                Albums.Add(new AlbumView(music.Album, music.Artist, group.OrderBy((m) => m.Name).ThenBy((m) => m.Artist)));
            }
        }

        public async void SearchSongs(string text)
        {
            LimitedSongs.Clear();
            foreach (var music in MusicLibraryPage.AllSongs)
            {
                if (IsTargetMusic(music, text))
                    LimitedSongs.Add(music);
                if (LimitedSongs.Count == 5)
                    break;
            }
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
            {
                MusicLibraryPage.AllSongs.Where((music) => IsTargetMusic(music, text)).ToList().ForEach((m) => Songs.Add(m));
            });
        }

        public void SearchPlaylists(string text)
        {
            Playlists = PlaylistsPage.Playlists.Where((playlist) => IsTargetPlaylist(playlist, text)).ToList();
        }

        public static bool IsTargetArtist(Music music, string text)
        {
            return music.Name.ToLower().Contains(text) || music.Album.ToLower().Contains(text) || music.Artist.ToLower().Contains(text);
        }
        public static bool IsTargetAlbum(Music music, string text)
        {
            return music.Album.ToLower().Contains(text) || music.Artist.ToLower().Contains(text);
        }
        public static bool IsTargetMusic(Music music, string text)
        {
            return music.Name.ToLower().Contains(text) || music.Album.ToLower().Contains(text) || music.Artist.ToLower().Contains(text);
        }

        public static bool IsTargetPlaylist(Playlist playlist, string text)
        {
            return playlist.Name.Contains(text) || playlist.Songs.Any((music) => IsTargetMusic(music, text));
        }

        public static string GetSearchHeader(string text, bool isMinimal)
        {
            string header = $"\"{text}\"";
            return isMinimal ? header : $"Search Result of {header}";
        }

        private void ViewAllButton_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }
    }
}

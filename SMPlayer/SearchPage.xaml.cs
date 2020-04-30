using SMPlayer.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using System;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SearchPage : Page
    {
        public static readonly SortBy[] ArtistsCriteria = new SortBy[] { SortBy.Default, SortBy.Name, SortBy.Album, SortBy.PlayCount, SortBy.Duration },
                                        AlbumsCriteria = new SortBy[] { SortBy.Default, SortBy.Name, SortBy.PlayCount, SortBy.Duration },
                                        SongsCriteria = new SortBy[] { SortBy.Default, SortBy.Title, SortBy.Album, SortBy.PlayCount, SortBy.Duration },
                                        PlaylistsCriteria = new SortBy[] { SortBy.Default, SortBy.Name, SortBy.PlayCount, SortBy.Duration };
        public static Stack<string> History = new Stack<string>();

        public ObservableCollection<Playlist> Artists = new ObservableCollection<Playlist>(), AllArtists = new ObservableCollection<Playlist>();
        public ObservableCollection<AlbumView> Albums = new ObservableCollection<AlbumView>(), AllAlbums = new ObservableCollection<AlbumView>();
        public ObservableCollection<Music> Songs = new ObservableCollection<Music>(), AllSongs = new ObservableCollection<Music>();
        public ObservableCollection<AlbumView> Playlists = new ObservableCollection<AlbumView>(), AllPlaylists = new ObservableCollection<AlbumView>();
        public const int ArtistLimit = 10, AlbumLimit = 5, SongLimit = 5, PlaylistLimit = 5;
        private string CurrentKeyword;
        public SearchPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SetArtistsDropdownContent();
            SetAlbumsDropdownContent();
            SetSongsDropdownContent();
            SetPlaylistsDropdownContent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string keyword = e.Parameter as string;
            MainPage.Instance.SetHeaderText(GetSearchHeader(keyword, MainPage.Instance.IsMinimal));
            switch (e.NavigationMode)
            {
                case NavigationMode.New:
                    History.Push(CurrentKeyword = keyword);
                    Search(keyword);
                    break;
                case NavigationMode.Back:
                    if (CurrentKeyword != keyword)
                        Search(keyword);
                    break;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (e.NavigationMode == NavigationMode.Back)
                History.Pop();
        }

        private async void Search(string keyword)
        {
            LoadingProgress.Visibility = Visibility.Visible;
            CurrentKeyword = keyword;
            AllArtists.Clear();
            AllAlbums.Clear();
            AllSongs.Clear();
            AllPlaylists.Clear();
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                string modifiedKeyowrd = keyword.ToLowerInvariant();
                SearchArtists(modifiedKeyowrd);
                SearchAlbums(modifiedKeyowrd);
                SearchSongs(modifiedKeyowrd);
                SearchPlaylists(modifiedKeyowrd);
                NoResultTextBlock.Visibility = Artists.Count == 0 && Albums.Count == 0 && Songs.Count == 0 && Playlists.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                LoadingProgress.Visibility = Visibility.Collapsed;
            });
        }

        public static bool IsTargetArtist(Music music, string keyword)
        {
            return IsTargetArtist(music.Artist, keyword);
        }
        public static bool IsTargetArtist(string artist, string keyword)
        {
            return artist.ToLowerInvariant().Contains(keyword);
        }
        public void SearchArtists(string keyword)
        {
            foreach (var group in MusicLibraryPage.AllSongs.Where(m => IsTargetArtist(m, keyword)).GroupBy(m => m.Artist))
            {
                AllArtists.Add(new Playlist(group.Key, group) { Artist = group.Key });
            }
            Artists.SetTo(AllArtists.Take(ArtistLimit));
            ArtistsViewAllButton.Visibility = AllArtists.Count > ArtistLimit ? Visibility.Visible : Visibility.Collapsed;
            ArtistsDropdown.Visibility = Artists.Count < 2 ? Visibility.Collapsed : Visibility.Visible;
        }
        public static bool IsTargetAlbum(Music music, string keyword)
        {
            return music.Album.ToLowerInvariant().Contains(keyword) || music.Artist.ToLower().Contains(keyword);
        }
        public void SearchAlbums(string keyword)
        {
            foreach (var group in MusicLibraryPage.AllSongs.Where(m => IsTargetAlbum(m, keyword)).GroupBy(m => m.Album))
            {
                Music music = group.ElementAt(0);
                AllAlbums.Add(new AlbumView(music.Album, music.Artist, group.OrderBy(m => m.Name).ThenBy(m => m.Artist), false));
            }
            Albums.SetTo(AllAlbums.Take(AlbumLimit));
            AlbumsViewAllButton.Visibility = AllAlbums.Count > AlbumLimit ? Visibility.Visible : Visibility.Collapsed;
            AlbumsDropdown.Visibility = Albums.Count < 2 ? Visibility.Collapsed : Visibility.Visible;
        }
        public static bool IsTargetMusic(Music music, string keyword)
        {
            return music.Name.ToLowerInvariant().Contains(keyword) ||
                   music.Album.ToLowerInvariant().Contains(keyword) ||
                   music.Artist.ToLowerInvariant().Contains(keyword);
        }

        public void SearchSongs(string keyword)
        {
            foreach (var music in MusicLibraryPage.AllSongs)
            {
                if (IsTargetMusic(music, keyword))
                {
                    AllSongs.Add(music);
                }
            }
            Songs.SetTo(AllSongs.Take(SongLimit));
            SongsViewAllButton.Visibility = AllSongs.Count > SongLimit ? Visibility.Visible : Visibility.Collapsed;
            SongsDropdown.Visibility = Songs.Count < 2 ? Visibility.Collapsed : Visibility.Visible;
        }
        public static bool IsTargetPlaylist(Playlist playlist, string keyword)
        {
            return playlist.Name.ToLowerInvariant().Contains(keyword) || playlist.Songs.Any(m => IsTargetMusic(m, keyword));
        }

        public void SearchPlaylists(string keyword)
        {
            var nowPlaying = MediaHelper.NowPlaying;
            if (IsTargetPlaylist(nowPlaying, keyword))
                AllPlaylists.Add(nowPlaying.ToAlbumView());
            if (IsTargetPlaylist(Settings.settings.MyFavorites, keyword))
                AllPlaylists.Add(Settings.settings.MyFavorites.ToAlbumView());
            foreach (var playlist in Settings.settings.Playlists)
            {
                if (IsTargetPlaylist(playlist, keyword))
                {
                    AllPlaylists.Add(playlist.ToAlbumView());
                }
            }
            Playlists.SetTo(AllPlaylists.Take(PlaylistLimit));
            PlaylistsViewAllButton.Visibility = AllPlaylists.Count > PlaylistLimit ? Visibility.Visible : Visibility.Collapsed;
            PlaylistsDropdown.Visibility = Playlists.Count < 2 ? Visibility.Collapsed : Visibility.Visible;
        }

        public static string GetSearchHeader(string keyword, bool isMinimal)
        {
            string header = Helper.LocalizeMessage("Quotations", keyword);
            return isMinimal ? header : Helper.LocalizeMessage("SearchResult", header);
        }
        private void SearchArtistView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(ArtistsPage), e.ClickedItem);
        }
        private void SearchAlbumView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(AlbumPage), e.ClickedItem);
        }
        private void SearchPlaylistView_ItemClick(object sender, ItemClickEventArgs e)
        {
            AlbumView playlist = (AlbumView)e.ClickedItem;
            if (playlist.Name == MenuFlyoutHelper.NowPlaying)
                Frame.Navigate(typeof(NowPlayingPage));
            else if (playlist.Name == MenuFlyoutHelper.MyFavorites)
                Frame.Navigate(typeof(MyFavoritesPage));
            else
                Frame.Navigate(typeof(PlaylistsPage), e.ClickedItem);
        }

        private void Album_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            (args.NewValue as AlbumView)?.SetCover();
        }
        private void SetArtistsDropdownContent()
        {
            ArtistsDropdown.Content = Helper.LocalizeMessage("Sort By " + Settings.settings.SearchArtistsCriterion.ToStr());
        }
        private void SetAlbumsDropdownContent()
        {
            AlbumsDropdown.Content = Helper.LocalizeMessage("Sort By " + Settings.settings.SearchAlbumsCriterion.ToStr());
        }
        private void SetSongsDropdownContent()
        {
            SongsDropdown.Content = Helper.LocalizeMessage("Sort By " + Settings.settings.SearchSongsCriterion.ToStr());
        }
        private void SetPlaylistsDropdownContent()
        {
            PlaylistsDropdown.Content = Helper.LocalizeMessage("Sort By " + Settings.settings.SearchPlaylistsCriterion.ToStr());
        }

        private void ArtistsDropdown_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutHelper.SetSearchSortByMenu(sender, Settings.settings.SearchArtistsCriterion, ArtistsCriteria,
                                                item =>
                                                {
                                                    Settings.settings.SearchArtistsCriterion = item;
                                                    SetArtistsDropdownContent();
                                                });
        }

        private void AlbumsDropdown_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutHelper.SetSearchSortByMenu(sender, Settings.settings.SearchAlbumsCriterion, AlbumsCriteria,
                                                item =>
                                                {
                                                    Settings.settings.SearchAlbumsCriterion = item;
                                                    SetAlbumsDropdownContent();
                                                });
        }

        private void SongsDropdown_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutHelper.SetSearchSortByMenu(sender, Settings.settings.SearchSongsCriterion, SongsCriteria,
                                                item =>
                                                {
                                                    Settings.settings.SearchSongsCriterion = item;
                                                    SetSongsDropdownContent();
                                                });
        }

        private void AddToButton_Click(object sender, RoutedEventArgs e)
        {
            new MenuFlyoutHelper()
            {
                Data = Songs,
                DefaultPlaylistName = Settings.settings.FindNextPlaylistName(CurrentKeyword)
            }.GetAddToMenuFlyout().ShowAt(sender as FrameworkElement);
            
        }

        private void PlaylistsDropdown_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutHelper.SetSearchSortByMenu(sender, Settings.settings.SearchPlaylistsCriterion, PlaylistsCriteria,
                                                item =>
                                                {
                                                    Settings.settings.SearchPlaylistsCriterion = item;
                                                    SetPlaylistsDropdownContent();
                                                });
        }


        private void ArtistsViewAllButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SearchResultPage), new SearchArgs()
            {
                Type = SearchType.Artists,
                Criterion = Settings.settings.SearchArtistsCriterion,
                Collection = AllArtists
            });
        }

        private void AlbumsViewAllButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SearchResultPage), new SearchArgs()
            {
                Type = SearchType.Albums,
                Criterion = Settings.settings.SearchAlbumsCriterion,
                Collection = AllAlbums
            });
        }

        private void SongsViewAllButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SearchResultPage), new SearchArgs()
            {
                Type = SearchType.Songs,
                Criterion = Settings.settings.SearchSongsCriterion,
                Collection = AllSongs
            });
        }

        private void PlaylistsViewAllButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SearchResultPage), new SearchArgs()
            {
                Type = SearchType.Playlists,
                Criterion = Settings.settings.SearchPlaylistsCriterion,
                Collection = AllPlaylists
            });
        }
    }

    public class SearchArgs
    {
        public SearchType Type;
        public SortBy Criterion;
        public object Collection;
    }
}

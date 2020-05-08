﻿using SMPlayer.Models;
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
using SMPlayer.Helpers;
using System.Threading.Tasks;

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
                                        SongsCriteria = new SortBy[] { SortBy.Default, SortBy.Title, SortBy.Artist, SortBy.Album, SortBy.PlayCount, SortBy.Duration },
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
            string modifiedKeyowrd = keyword.ToLowerInvariant();
            await SearchArtists(modifiedKeyowrd, Settings.settings.SearchArtistsCriterion);
            await SearchAlbums(modifiedKeyowrd, Settings.settings.SearchAlbumsCriterion);
            await SearchSongs(modifiedKeyowrd, Settings.settings.SearchSongsCriterion);
            await SearchPlaylists(modifiedKeyowrd, Settings.settings.SearchPlaylistsCriterion);
            NoResultTextBlock.Visibility = Artists.Count == 0 && Albums.Count == 0 && Songs.Count == 0 && Playlists.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            LoadingProgress.Visibility = Visibility.Collapsed;
        }

        public async Task SearchArtists(string keyword, SortBy criterion)
        {
            AllArtists.SetTo(await Task.Run(() => SearchHelper.SearchArtists(keyword, criterion)));
            Artists.SetTo(AllArtists.Take(ArtistLimit));
            ArtistsViewAllButton.Visibility = AllArtists.Count > ArtistLimit ? Visibility.Visible : Visibility.Collapsed;
            ArtistsDropdown.Visibility = Artists.Count < 2 ? Visibility.Collapsed : Visibility.Visible;
        }

        public async Task SearchAlbums(string keyword, SortBy criterion)
        {
            AllAlbums.SetTo(await Task.Run(() => SearchHelper.SearchAlbums(keyword, criterion)));
            Albums.SetTo(AllAlbums.Take(AlbumLimit));
            AlbumsViewAllButton.Visibility = AllAlbums.Count > AlbumLimit ? Visibility.Visible : Visibility.Collapsed;
            AlbumsDropdown.Visibility = Albums.Count < 2 ? Visibility.Collapsed : Visibility.Visible;
        }

        public async Task SearchSongs(string keyword, SortBy criterion)
        {
            AllSongs.SetTo(await Task.Run(() => SearchHelper.SearchSongs(keyword, criterion)));
            Songs.SetTo(AllSongs.Take(SongLimit));
            SongsViewAllButton.Visibility = AllSongs.Count > SongLimit ? Visibility.Visible : Visibility.Collapsed;
            SongsDropdown.Visibility = Songs.Count < 2 ? Visibility.Collapsed : Visibility.Visible;
        }

        public async Task SearchPlaylists(string keyword, SortBy criterion)
        {
            AllPlaylists.SetTo(await Task.Run(() => SearchHelper.SearchPlaylists(keyword, criterion)));
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
                                                async item =>
                                                {
                                                    Settings.settings.SearchArtistsCriterion = item;
                                                    SetArtistsDropdownContent();
                                                    LoadingProgress.Visibility = Visibility.Visible;
                                                    AllArtists.SetTo(await Task.Run(() => SearchHelper.SortArtists(AllArtists, CurrentKeyword, item)));
                                                    LoadingProgress.Visibility = Visibility.Collapsed;
                                                });
        }

        private void AlbumsDropdown_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutHelper.SetSearchSortByMenu(sender, Settings.settings.SearchAlbumsCriterion, AlbumsCriteria,
                                                async item =>
                                                {
                                                    Settings.settings.SearchAlbumsCriterion = item;
                                                    SetAlbumsDropdownContent();
                                                    LoadingProgress.Visibility = Visibility.Visible;
                                                    AllAlbums.SetTo(await Task.Run(() => SearchHelper.SortAlbums(AllAlbums, CurrentKeyword, item)));
                                                    LoadingProgress.Visibility = Visibility.Collapsed;
                                                });
        }

        private void SongsDropdown_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutHelper.SetSearchSortByMenu(sender, Settings.settings.SearchSongsCriterion, SongsCriteria,
                                                async item =>
                                                {
                                                    Settings.settings.SearchSongsCriterion = item;
                                                    SetSongsDropdownContent();
                                                    LoadingProgress.Visibility = Visibility.Visible;
                                                    AllSongs.SetTo(await Task.Run(() => SearchHelper.SortSongs(AllSongs, CurrentKeyword, item)));
                                                    LoadingProgress.Visibility = Visibility.Collapsed;
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
                                                async item =>
                                                {
                                                    Settings.settings.SearchPlaylistsCriterion = item;
                                                    SetPlaylistsDropdownContent();
                                                    LoadingProgress.Visibility = Visibility.Visible;
                                                    AllPlaylists.SetTo(await Task.Run(() => SearchHelper.SortPlaylists(AllPlaylists, CurrentKeyword, item)));
                                                    LoadingProgress.Visibility = Visibility.Collapsed;
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

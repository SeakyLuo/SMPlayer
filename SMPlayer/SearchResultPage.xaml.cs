using SMPlayer.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SearchResultPage : Page
    {
        public static Stack<string> History = new Stack<string>();
        public ObservableCollection<Playlist> Artists = new ObservableCollection<Playlist>();
        public ObservableCollection<AlbumView> Albums = new ObservableCollection<AlbumView>();
        public ObservableCollection<Music> Songs = new ObservableCollection<Music>();
        public ObservableCollection<AlbumView> Playlists = new ObservableCollection<AlbumView>();
        private SortBy[] Criteria;
        private SearchType searchType;
        private string CurrentKeyword;
        private SortBy SettingsCriterion
        {
            get
            {
                switch (searchType)
                {
                    case SearchType.Artists:
                        return Settings.settings.SearchArtistsCriterion;
                    case SearchType.Albums:
                        return Settings.settings.SearchAlbumsCriterion;
                    case SearchType.Songs:
                        return Settings.settings.SearchSongsCriterion;
                    case SearchType.Playlists:
                        return Settings.settings.SearchPlaylistsCriterion;
                    default:
                        return Settings.settings.SearchSongsCriterion;
                }
            }
            set
            {
                switch (searchType)
                {
                    case SearchType.Artists:
                        Settings.settings.SearchArtistsCriterion = value;
                        break;
                    case SearchType.Albums:
                        Settings.settings.SearchAlbumsCriterion = value;
                        break;
                    case SearchType.Songs:
                        Settings.settings.SearchSongsCriterion = value;
                        break;
                    case SearchType.Playlists:
                        Settings.settings.SearchPlaylistsCriterion = value;
                        break;
                }
            }
        }
        public SearchResultPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SearchArgs searchArgs = (SearchArgs)e.Parameter;
            searchType = searchArgs.Type;
            SortBy criterion = searchArgs.Criterion;
            string keyword = SearchPage.History.Peek();
            switch (searchType)
            {
                case SearchType.Artists:
                    Criteria = SearchPage.ArtistsCriteria;
                    break;
                case SearchType.Albums:
                    Criteria = SearchPage.AlbumsCriteria;
                    break;
                case SearchType.Songs:
                    Criteria = SearchPage.SongsCriteria;
                    break;
                case SearchType.Playlists:
                    Criteria = SearchPage.PlaylistsCriteria;
                    break;
            }
            SetSortByDropdownContent(criterion);
            MainPage.Instance.SetHeaderText(SearchPage.GetSearchHeader(keyword, MainPage.Instance.IsMinimal));
            switch (e.NavigationMode)
            {
                case NavigationMode.New:
                    History.Push(keyword);
                    Search(searchType, keyword);
                    break;
                case NavigationMode.Back:
                    if (CurrentKeyword != keyword)
                        Search(searchType, keyword);
                    break;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (e.NavigationMode == NavigationMode.Back)
                History.Pop();
        }

        private async void Search(SearchType searchType, string keyword)
        {
            LoadingProgress.IsActive = true;
            CurrentKeyword = keyword;
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Artists.Clear();
                Albums.Clear();
                Songs.Clear();
                Playlists.Clear();
                switch (searchType)
                {
                    case SearchType.Artists:
                        foreach (var group in MusicLibraryPage.AllSongs.Where(m => SearchPage.IsTargetArtist(m, keyword)).GroupBy(m => m.Artist).OrderBy(g => g.Key))
                            Artists.Add(new Playlist(group.Key, group) { Artist = group.Key });
                        break;
                    case SearchType.Albums:
                        foreach (var group in MusicLibraryPage.AllSongs.Where(m => SearchPage.IsTargetAlbum(m, keyword)).GroupBy(m => m.Album).OrderBy(g => g.Key))
                        {
                            Music music = group.ElementAt(0);
                            Albums.Add(new AlbumView(music.Album, music.Artist, group.OrderBy(m => m.Name).ThenBy(m => m.Artist)));
                        }
                        break;
                    case SearchType.Songs:
                        foreach (var music in MusicLibraryPage.AllSongs.Where(m => SearchPage.IsTargetMusic(m, keyword)).OrderBy(m => m.Name).ThenBy(m => m.Artist))
                            Songs.Add(music);
                        break;
                    case SearchType.Playlists:
                        var nowPlaying = MediaHelper.NowPlaying;
                        if (SearchPage.IsTargetPlaylist(nowPlaying, keyword))
                            Playlists.Add(nowPlaying.ToAlbumView());
                        if (SearchPage.IsTargetPlaylist(Settings.settings.MyFavorites, keyword))
                            Playlists.Add(Settings.settings.MyFavorites.ToAlbumView());
                        foreach (var playlist in Settings.settings.Playlists.Where(p => SearchPage.IsTargetPlaylist(p, keyword)).OrderBy(p => p.Name))
                            Playlists.Add(playlist.ToAlbumView());
                        break;
                }
                LoadingProgress.IsActive = false;
            });
        }

        private void SetSortByDropdownContent(SortBy criterion)
        {
            SortByDropdown.Content = Helper.LocalizeMessage("Sort By " + criterion.ToStr());
        }

        private void ArtistsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(ArtistsPage), e.ClickedItem);
        }

        private void AlbumsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(AlbumPage), e.ClickedItem);
        }

        private void PlaylistGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            AlbumView album = (AlbumView)e.ClickedItem;
            if (album.Name == MenuFlyoutHelper.NowPlaying)
                Frame.Navigate(typeof(NowPlayingPage));
            else if (album.Name == MenuFlyoutHelper.MyFavorites)
                Frame.Navigate(typeof(MyFavoritesPage));
            else
                Frame.Navigate(typeof(AlbumPage), album);
        }

        private void Album_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            (args.NewValue as AlbumView)?.SetCover();
        }

        private void SortByDropdown_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutHelper.SetSearchSortByMenu(sender, SettingsCriterion, Criteria,
                                                item =>
                                                {
                                                    SettingsCriterion = item;
                                                    SetSortByDropdownContent(item);
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
    }
}

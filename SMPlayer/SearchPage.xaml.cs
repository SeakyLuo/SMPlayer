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
                                        SongsCriteria = new SortBy[] { SortBy.Default, SortBy.Title, SortBy.Artist, SortBy.Album, SortBy.PlayCount, SortBy.Duration, SortBy.DateAdded },
                                        PlaylistsCriteria = new SortBy[] { SortBy.Default, SortBy.Name, SortBy.PlayCount, SortBy.Duration },
                                        FoldersCriteria = new SortBy[] { SortBy.Default, SortBy.Name };
        public static Stack<SearchKeyword> History = new Stack<SearchKeyword>();

        public ObservableCollection<Playlist> Artists = new ObservableCollection<Playlist>(), AllArtists = new ObservableCollection<Playlist>();
        public ObservableCollection<AlbumView> Albums = new ObservableCollection<AlbumView>(), AllAlbums = new ObservableCollection<AlbumView>();
        public ObservableCollection<Music> Songs = new ObservableCollection<Music>(), AllSongs = new ObservableCollection<Music>();
        public ObservableCollection<AlbumView> Playlists = new ObservableCollection<AlbumView>(), AllPlaylists = new ObservableCollection<AlbumView>();
        public ObservableCollection<GridFolderView> Folders = new ObservableCollection<GridFolderView>(), AllFolders = new ObservableCollection<GridFolderView>();
        public const int ArtistLimit = 10, AlbumLimit = 5, SongLimit = 5, PlaylistLimit = 5, FolderLimit = 5;
        private SearchKeyword CurrentKeyword;
        private volatile bool IsSearching = false;
        public SearchPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SearchKeyword keyword = e.Parameter as SearchKeyword;
            MainPage.Instance.SetHeaderTextWithoutLocalization(GetSearchHeader(keyword, MainPage.Instance.IsMinimal));
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

        private async void Search(SearchKeyword keyword)
        {
            if (IsSearching)
            {
                MainPage.Instance.ShowLocalizedNotification("ProcessingRequest");
                return;
            }
            IsSearching = true;
            LoadingProgress.Visibility = Visibility.Visible;
            NoResultTextBlock.Visibility = Visibility.Collapsed;
            CurrentKeyword = keyword;
            Artists.Clear();
            Albums.Clear();
            Songs.Clear();
            Playlists.Clear();
            Folders.Clear();
            string modifiedKeyowrd = keyword.Text.ToLowerInvariant();
            await SearchArtists(keyword.Songs, modifiedKeyowrd, Settings.settings.SearchArtistsCriterion);
            await SearchAlbums(keyword.Songs, modifiedKeyowrd, Settings.settings.SearchAlbumsCriterion);
            await SearchSongs(keyword.Songs, modifiedKeyowrd, Settings.settings.SearchSongsCriterion);
            await SearchPlaylists(keyword.Playlists, modifiedKeyowrd, Settings.settings.SearchPlaylistsCriterion);
            await SearchFolders(keyword.Tree, modifiedKeyowrd, Settings.settings.SearchFoldersCriterion);
            NoResultTextBlock.Visibility = Artists.Count == 0 && Albums.Count == 0 && Songs.Count == 0 && Playlists.Count == 0 && Folders.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            LoadingProgress.Visibility = Visibility.Collapsed;
            IsSearching = false;
        }

        public async Task SearchArtists(IEnumerable<Music> source, string keyword, SortBy criterion)
        {
            AllArtists.SetTo(await Task.Run(() => SearchHelper.SearchArtists(source, keyword, criterion)));
            Artists.SetTo(AllArtists.Take(ArtistLimit));
            ArtistsTextBlock.Text = Settings.settings.ShowCount ? Helper.LocalizeText("ArtistsWithCount", AllArtists.Count) : Helper.LocalizeText("Artists");
            ArtistsViewAllButton.Visibility = AllArtists.Count > ArtistLimit ? Visibility.Visible : Visibility.Collapsed;
            SortArtistsButton.Visibility = Artists.Count < 2 ? Visibility.Collapsed : Visibility.Visible;
        }

        public async Task SearchAlbums(IEnumerable<Music> source, string keyword, SortBy criterion)
        {
            AllAlbums.SetTo(await Task.Run(() => SearchHelper.SearchAlbums(source, keyword, criterion)));
            AlbumsTextBlock.Text = Settings.settings.ShowCount ? Helper.LocalizeText("AlbumsWithCount", AllAlbums.Count) : Helper.LocalizeText("Albums");
            AlbumsViewAllButton.Visibility = AllAlbums.Count > AlbumLimit ? Visibility.Visible : Visibility.Collapsed;
            SortAlbumsButton.Visibility = Albums.Count < 2 ? Visibility.Collapsed : Visibility.Visible;
        }

        public async Task SearchSongs(IEnumerable<Music> source, string keyword, SortBy criterion)
        {
            AllSongs.SetTo(await Task.Run(() => SearchHelper.SearchSongs(source, keyword, criterion)));
            Songs.SetTo(AllSongs.Take(SongLimit));
            SongsTextBlock.Text = Settings.settings.ShowCount ? Helper.LocalizeText("SongsWithCount", AllSongs.Count) : Helper.LocalizeText("Songs");
            SongsViewAllButton.Visibility = AllSongs.Count > SongLimit ? Visibility.Visible : Visibility.Collapsed;
            SortSongsButton.Visibility = Songs.Count < 2 ? Visibility.Collapsed : Visibility.Visible;
        }

        public async Task SearchPlaylists(IEnumerable<Playlist> source, string keyword, SortBy criterion)
        {
            AllPlaylists.SetTo(await Task.Run(() => SearchHelper.SearchPlaylists(source, keyword, criterion)));
            Playlists.SetTo(AllPlaylists.Take(PlaylistLimit));
            PlaylistsTextBlock.Text = Settings.settings.ShowCount ? Helper.LocalizeText("PlaylistsWithCount", AllPlaylists.Count) : Helper.LocalizeText("Playlists");
            PlaylistsViewAllButton.Visibility = AllPlaylists.Count > PlaylistLimit ? Visibility.Visible : Visibility.Collapsed;
            SortPlaylistsButton.Visibility = Playlists.Count < 2 ? Visibility.Collapsed : Visibility.Visible;
        }

        public async Task SearchFolders(FolderTree source, string keyword, SortBy criterion)
        {
            AllFolders.SetTo(await Task.Run(() => SearchHelper.SearchFolders(source, keyword, criterion)));
            Folders.SetTo(AllFolders.Take(FolderLimit));
            FoldersTextBlock.Text = Settings.settings.ShowCount ? Helper.LocalizeText("FoldersWithCount", AllFolders.Count) : Helper.LocalizeText("Folders");
            FoldersViewAllButton.Visibility = AllFolders.Count > FolderLimit ? Visibility.Visible : Visibility.Collapsed;
            SortFoldersButton.Visibility = AllFolders.Count < 2 ? Visibility.Collapsed : Visibility.Visible;
        }

        public static string GetSearchHeader(SearchKeyword keyword, bool isMinimal)
        {
            string header = Helper.LocalizeMessage("Quotations", keyword.Text);
            return isMinimal ? header :
                               keyword.Tree == Settings.settings.Tree ? Helper.LocalizeMessage("SearchResult", keyword.Text) :
                                                                        Helper.LocalizeMessage("SearchDirectoryResult", keyword.Text, keyword.Tree.Directory);
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
        private void SearchFolderView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(LocalPage), e.ClickedItem);
        }

        private async void Album_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue is AlbumView album)
                await album.SetThumbnailAsync();
        }

        private void SortSongsButton_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutHelper.SetSortByMenu(sender, Settings.settings.SearchSongsCriterion, SongsCriteria,
                                                   async item =>
                                                   {
                                                       Settings.settings.SearchSongsCriterion = item;
                                                       LoadingProgress.Visibility = Visibility.Visible;
                                                       AllSongs.SetTo(await Task.Run(() => SearchHelper.SortSongs(AllSongs, CurrentKeyword.Text, item).ToList()));
                                                       Songs.SetTo(AllSongs.Take(Songs.Count));
                                                       LoadingProgress.Visibility = Visibility.Collapsed;
                                                   });
        }

        private void SortFoldersButton_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutHelper.SetSortByMenu(sender, Settings.settings.SearchFoldersCriterion, FoldersCriteria,
                                                   async item =>
                                                   {
                                                       Settings.settings.SearchFoldersCriterion = item;
                                                       LoadingProgress.Visibility = Visibility.Visible;
                                                       AllFolders.SetTo(await Task.Run(() => SearchHelper.SortFolders(AllFolders, CurrentKeyword.Text, item).ToList()));
                                                       Folders.SetTo(AllFolders.Take(Folders.Count));
                                                       LoadingProgress.Visibility = Visibility.Collapsed;
                                                   });
        }

        private void SortPlaylistsButton_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutHelper.SetSortByMenu(sender, Settings.settings.SearchPlaylistsCriterion, PlaylistsCriteria,
                                                   async item =>
                                                   {
                                                       Settings.settings.SearchPlaylistsCriterion = item;
                                                       LoadingProgress.Visibility = Visibility.Visible;
                                                       int nowPlayingIndex = AllPlaylists.FindIndex(i => i.Name == MenuFlyoutHelper.NowPlaying);
                                                       if (nowPlayingIndex >= 0) AllPlaylists.RemoveAt(nowPlayingIndex);
                                                       int myFavoritesIndex = AllPlaylists.FindIndex(i => i.Name == MenuFlyoutHelper.MyFavorites);
                                                       if (myFavoritesIndex >= 0) AllPlaylists.RemoveAt(myFavoritesIndex);
                                                       AllPlaylists.SetTo(await Task.Run(() => SearchHelper.SortPlaylists(AllPlaylists, CurrentKeyword.Text, item).ToList()));
                                                       Playlists.SetTo(AllPlaylists.Take(Playlists.Count));
                                                       LoadingProgress.Visibility = Visibility.Collapsed;
                                                   });
        }

        private void SortArtistsButton_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutHelper.SetSortByMenu(sender, Settings.settings.SearchArtistsCriterion, ArtistsCriteria,
                                                   async item =>
                                                   {
                                                       Settings.settings.SearchArtistsCriterion = item;
                                                       LoadingProgress.Visibility = Visibility.Visible;
                                                       AllArtists.SetTo(await Task.Run(() => SearchHelper.SortArtists(AllArtists, CurrentKeyword.Text, item).ToList()));
                                                       Artists.SetTo(AllArtists.Take(Artists.Count));
                                                       LoadingProgress.Visibility = Visibility.Collapsed;
                                                   });
        }

        private void SortAlbumsButton_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutHelper.SetSortByMenu(sender, Settings.settings.SearchAlbumsCriterion, AlbumsCriteria,
                                                   async item =>
                                                   {  
                                                       Settings.settings.SearchAlbumsCriterion = item;
                                                       LoadingProgress.Visibility = Visibility.Visible;
                                                       AllAlbums.SetTo(await Task.Run(() => SearchHelper.SortAlbums(AllAlbums, CurrentKeyword.Text, item).ToList()));
                                                       Albums.SetTo(AllAlbums.Take(Albums.Count));
                                                       LoadingProgress.Visibility = Visibility.Collapsed;
                                                   });
        }

        private void ArtistsViewAllButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SearchResultPage), new SearchArgs
            {
                Type = SearchType.Artists,
                Criterion = Settings.settings.SearchArtistsCriterion,
                Collection = AllArtists,
                Summary = ArtistsTextBlock.Text
            });
        }

        private void AlbumsViewAllButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SearchResultPage), new SearchArgs
            {
                Type = SearchType.Albums,
                Criterion = Settings.settings.SearchAlbumsCriterion,
                Collection = AllAlbums,
                Summary = AlbumsTextBlock.Text
            });
        }

        private void SongsViewAllButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SearchResultPage), new SearchArgs
            {
                Type = SearchType.Songs,
                Criterion = Settings.settings.SearchSongsCriterion,
                Collection = AllSongs,
                Summary = SongsTextBlock.Text
            });
        }

        private void PlaylistsViewAllButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SearchResultPage), new SearchArgs
            {
                Type = SearchType.Playlists,
                Criterion = Settings.settings.SearchPlaylistsCriterion,
                Collection = AllPlaylists,
                Summary = PlaylistsTextBlock.Text
            });
        }

        private void FoldersViewAllButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SearchResultPage), new SearchArgs
            {
                Type = SearchType.Folders,
                Criterion = Settings.settings.SearchFoldersCriterion,
                Collection = AllFolders,
                Summary = FoldersTextBlock.Text
            });
        }
    }
    public class SearchKeyword
    {
        public string Text;
        public IEnumerable<Music> Songs = MusicLibraryPage.AllSongs;
        public IEnumerable<Playlist> Playlists = Settings.settings.Playlists;
        public FolderTree Tree = Settings.settings.Tree;
    }
    public class SearchArgs
    {
        public SearchType Type { get; set; }
        public SortBy Criterion { get; set; }
        public object Collection { get; set; }
        public string Summary { get; set; }
    }

}

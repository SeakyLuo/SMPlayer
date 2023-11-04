using SMPlayer.Models;
using SMPlayer.Models.VO;
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
using SMPlayer.Services;
using SMPlayer.Interfaces;

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

        public ObservableCollection<PlaylistView> Artists = new ObservableCollection<PlaylistView>();
        private List<MatchResult<Artist>> AllArtists = new List<MatchResult<Artist>>();
        public ObservableCollection<AlbumView> Albums = new ObservableCollection<AlbumView>();
        private List<MatchResult<Album>> AllAlbums = new List<MatchResult<Album>>();
        public ObservableCollection<MusicView> Songs = new ObservableCollection<MusicView>();
        private List<MatchResult<Music>> AllSongs = new List<MatchResult<Music>>();
        public ObservableCollection<AlbumView> Playlists = new ObservableCollection<AlbumView>();
        private List<MatchResult<Playlist>> AllPlaylists = new List<MatchResult<Playlist>>();
        public ObservableCollection<GridViewFolder> Folders = new ObservableCollection<GridViewFolder>();
        private List<MatchResult<FolderTree>> AllFolders = new List<MatchResult<FolderTree>>();
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
            MainPage.Instance.SetHeaderTextRaw(GetSearchHeader(keyword, MainPage.Instance.IsMinimal));
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
                Helper.ShowNotification("ProcessingRequest");
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
            await SearchPlaylists(AllSongs.Select(i => i.Entity), keyword.Playlists, modifiedKeyowrd, Settings.settings.SearchPlaylistsCriterion);
            await SearchFolders(AllSongs.Select(i => i.Entity), keyword.Folders, modifiedKeyowrd, Settings.settings.SearchFoldersCriterion);
            NoResultTextBlock.Visibility = Artists.Count == 0 && Albums.Count == 0 && Songs.Count == 0 && Playlists.Count == 0 && Folders.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            LoadingProgress.Visibility = Visibility.Collapsed;
            IsSearching = false;
        }

        public async Task SearchArtists(IEnumerable<Music> source, string keyword, SortBy criterion)
        {
            AllArtists = await Task.Run(() => SearchHelper.SearchArtists(source, keyword, criterion).ToList());
            SetArtists(AllArtists);
            ArtistsTextBlock.Text = Settings.settings.ShowCount ? Helper.LocalizeText("ArtistsWithCount", AllArtists.Count) : Helper.LocalizeText("Artists");
            ArtistsViewAllButton.Visibility = AllArtists.Count > ArtistLimit ? Visibility.Visible : Visibility.Collapsed;
            SortArtistsButton.Visibility = Artists.Count < 2 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void SetArtists(IEnumerable<MatchResult<Artist>> artists)
        {
            Artists.SetTo(artists.Take(ArtistLimit).Select(i => new PlaylistView(i.Entity)));
        }

        public async Task SearchAlbums(IEnumerable<Music> source, string keyword, SortBy criterion)
        {
            AllAlbums = await Task.Run(() => SearchHelper.SearchAlbums(source, keyword, criterion).ToList());
            SetAlbums(AllAlbums);
            AlbumsTextBlock.Text = Settings.settings.ShowCount ? Helper.LocalizeText("AlbumsWithCount", AllAlbums.Count) : Helper.LocalizeText("Albums");
            AlbumsViewAllButton.Visibility = AllAlbums.Count > AlbumLimit ? Visibility.Visible : Visibility.Collapsed;
            SortAlbumsButton.Visibility = Albums.Count < 2 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void SetAlbums(IEnumerable<MatchResult<Album>> albums)
        {
            Albums.SetTo(albums.Take(AlbumLimit).Select(i => i.Entity.ToVO()));
        }

        public async Task SearchSongs(IEnumerable<Music> source, string keyword, SortBy criterion)
        {
            AllSongs = await Task.Run(() => SearchHelper.SearchSongs(source, keyword, criterion).ToList());
            SetSongs(AllSongs);
            SongsTextBlock.Text = Settings.settings.ShowCount ? Helper.LocalizeText("SongsWithCount", AllSongs.Count) : Helper.LocalizeText("Songs");
            SongsViewAllButton.Visibility = AllSongs.Count > SongLimit ? Visibility.Visible : Visibility.Collapsed;
            SortSongsButton.Visibility = Songs.Count < 2 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void SetSongs(IEnumerable<MatchResult<Music>> list)
        {
            Songs.SetTo(list.Take(SongLimit).AsParallel().AsOrdered().Select(i => i.Entity.ToVO(isFavorite: PlaylistService.IsFavorite(i.Entity))));
        }

        public async Task SearchPlaylists(IEnumerable<Music> songs, IEnumerable<Playlist> source, string keyword, SortBy criterion)
        {
            AllPlaylists = await Task.Run(() => SearchHelper.SearchPlaylists(songs, source, keyword, criterion).ToList());
            SetPlaylists(AllPlaylists);
            PlaylistsTextBlock.Text = Settings.settings.ShowCount ? Helper.LocalizeText("PlaylistsWithCount", AllPlaylists.Count) : Helper.LocalizeText("Playlists");
            PlaylistsViewAllButton.Visibility = AllPlaylists.Count > PlaylistLimit ? Visibility.Visible : Visibility.Collapsed;
            SortPlaylistsButton.Visibility = Playlists.Count < 2 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void SetPlaylists(IEnumerable<MatchResult<Playlist>> list)
        {
            Playlists.SetTo(list.Take(PlaylistLimit).Select(i => i.Entity.ToVO().ToSearchAlbumView()));
        }

        public async Task SearchFolders(IEnumerable<Music> songs, IEnumerable<FolderTree> source, string keyword, SortBy criterion)
        {
            AllFolders = await Task.Run(() => SearchHelper.SearchFolders(songs, source, keyword, criterion).ToList());
            SetFolders(AllFolders);
            FoldersTextBlock.Text = Settings.settings.ShowCount ? Helper.LocalizeText("FoldersWithCount", AllFolders.Count) : Helper.LocalizeText("Folders");
            FoldersViewAllButton.Visibility = AllFolders.Count > FolderLimit ? Visibility.Visible : Visibility.Collapsed;
            SortFoldersButton.Visibility = AllFolders.Count < 2 ? Visibility.Collapsed : Visibility.Visible;
        }
        private void SetFolders(IEnumerable<MatchResult<FolderTree>> list)
        {
            Folders.SetTo(list.Take(FolderLimit).Select(i => new GridViewFolder(i.Entity)));
        }

        public static string GetSearchHeader(SearchKeyword keyword, bool isMinimal)
        {
            string header = Helper.LocalizeMessage("Quotations", keyword.Text);
            if (isMinimal) return header;
            return keyword.Folder == null || keyword.Folder == Settings.settings.Tree ? 
                Helper.LocalizeMessage("SearchResult", keyword.Text) : Helper.LocalizeMessage("SearchDirectoryResult", keyword.Text, keyword.Folder.Name);
        }
        private void SearchArtistView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(ArtistsPage), e.ClickedItem);
        }
        private void SearchAlbumView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(AlbumPage), e.ClickedItem);
        }

        private void OpenFolderMenuFlyout(object sender, object e)
        {
            var flyout = sender as MenuFlyout;
            GridViewFolder folder = flyout.Target.DataContext as GridViewFolder;
            MenuFlyoutHelper.SetSimpleFolderMenu(sender, folder.Source);
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
            try
            {
                Frame.Navigate(typeof(LocalPage), e.ClickedItem);
            }
            catch (Exception ex)
            {
                Log.Warn($"SearchFolderView_ItemClick failed {ex}");
                Helper.ShowNotification("OperationFailed");
            }
        }

        private async void Album_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue is AlbumView album)
                await album.SetThumbnailAsync();
        }

        private void SortSongsButton_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutHelper.SetSortByMenu(sender, Settings.settings.SearchSongsCriterion, SongsCriteria,
                                            item =>
                                            {
                                                LoadingProgress.Visibility = Visibility.Visible;
                                                Settings.settings.SearchSongsCriterion = item;
                                                SetSongs(SearchHelper.SortSongs(AllSongs, item));
                                                LoadingProgress.Visibility = Visibility.Collapsed;
                                            });
        }

        private void SortFoldersButton_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutHelper.SetSortByMenu(sender, Settings.settings.SearchFoldersCriterion, FoldersCriteria,
                                                   item =>
                                                   {
                                                       LoadingProgress.Visibility = Visibility.Visible;
                                                       Settings.settings.SearchFoldersCriterion = item;
                                                       SetFolders(SearchHelper.SortFolders(AllFolders, item));
                                                       LoadingProgress.Visibility = Visibility.Collapsed;
                                                   });
        }

        private void SortPlaylistsButton_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutHelper.SetSortByMenu(sender, Settings.settings.SearchPlaylistsCriterion, PlaylistsCriteria,
                                                   item =>
                                                   {
                                                       LoadingProgress.Visibility = Visibility.Visible;
                                                       Settings.settings.SearchPlaylistsCriterion = item;
                                                       SetPlaylists(SearchHelper.SortPlaylists(AllPlaylists, item));
                                                       LoadingProgress.Visibility = Visibility.Collapsed;
                                                   });
        }

        private void SortArtistsButton_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutHelper.SetSortByMenu(sender, Settings.settings.SearchArtistsCriterion, ArtistsCriteria,
                                                   item =>
                                                   {
                                                       LoadingProgress.Visibility = Visibility.Visible;
                                                       Settings.settings.SearchArtistsCriterion = item;
                                                       SetArtists(SearchHelper.SortArtists(AllArtists, item));
                                                       LoadingProgress.Visibility = Visibility.Collapsed;
                                                   });
        }

        private void SortAlbumsButton_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutHelper.SetSortByMenu(sender, Settings.settings.SearchAlbumsCriterion, AlbumsCriteria,
                                                   item =>
                                                   {  
                                                       LoadingProgress.Visibility = Visibility.Visible;
                                                       Settings.settings.SearchAlbumsCriterion = item;
                                                       SetAlbums(SearchHelper.SortAlbums(AllAlbums, item));
                                                       LoadingProgress.Visibility = Visibility.Collapsed;
                                                   });
        }

        private void ArtistsViewAllButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SearchResultPage), new SearchArgs
            {
                Type = EntityType.Artist,
                Criterion = Settings.settings.SearchArtistsCriterion,
                Collection = AllArtists,
                Summary = ArtistsTextBlock.Text
            });
        }

        private void AlbumsViewAllButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SearchResultPage), new SearchArgs
            {
                Type = EntityType.Album,
                Criterion = Settings.settings.SearchAlbumsCriterion,
                Collection = AllAlbums,
                Summary = AlbumsTextBlock.Text
            });
        }

        private void SongsViewAllButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SearchResultPage), new SearchArgs
            {
                Type = EntityType.Song,
                Criterion = Settings.settings.SearchSongsCriterion,
                Collection = AllSongs,
                Summary = SongsTextBlock.Text
            });
        }

        private void PlaylistsViewAllButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SearchResultPage), new SearchArgs
            {
                Type = EntityType.Playlist,
                Criterion = Settings.settings.SearchPlaylistsCriterion,
                Collection = AllPlaylists,
                Summary = PlaylistsTextBlock.Text
            });
        }

        private void FoldersViewAllButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SearchResultPage), new SearchArgs
            {
                Type = EntityType.Folder,
                Criterion = Settings.settings.SearchFoldersCriterion,
                Collection = AllFolders,
                Summary = FoldersTextBlock.Text
            });
        }
    }
    public class SearchKeyword
    {
        public string Text { get; set; }
        public FolderTree Folder { get; set; }
        public IEnumerable<Music> Songs { get; set; } = MusicService.AllSongs;
        public IEnumerable<Playlist> Playlists { get; set; } = PlaylistService.AllPlaylists;
        public IEnumerable<FolderTree> Folders { get; set; } = StorageService.AllFolders;
    }
    public class SearchArgs
    {
        public EntityType Type { get; set; }
        public SortBy Criterion { get; set; }
        public object Collection { get; set; }
        public string Summary { get; set; }
    }

}

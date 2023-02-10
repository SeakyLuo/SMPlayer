using SMPlayer.Models;
using SMPlayer.Models.VO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System;
using SMPlayer.Controls;
using SMPlayer.Interfaces;
using SMPlayer.Helpers;
using SMPlayer.Services;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SearchResultPage : Page, IMultiSelectListener
    {
        public static Stack<SearchKeyword> History = new Stack<SearchKeyword>();
        public ObservableCollection<PlaylistView> Artists = new ObservableCollection<PlaylistView>();
        public ObservableCollection<AlbumView> Albums = new ObservableCollection<AlbumView>();
        public ObservableCollection<MusicView> Songs = new ObservableCollection<MusicView>();
        public ObservableCollection<AlbumView> Playlists = new ObservableCollection<AlbumView>();
        public ObservableCollection<GridViewFolder> Folders = new ObservableCollection<GridViewFolder>();
        private object OriginalSearchResult;
        private SortBy[] Criteria;
        private EntityType searchType;
        private SearchKeyword CurrentKeyword;
        private SortBy SettingsCriterion
        {
            get
            {
                switch (searchType)
                {
                    case EntityType.Artist:
                        return Settings.settings.SearchArtistsCriterion;
                    case EntityType.Album:
                        return Settings.settings.SearchAlbumsCriterion;
                    case EntityType.Song:
                        return Settings.settings.SearchSongsCriterion;
                    case EntityType.Playlist:
                        return Settings.settings.SearchPlaylistsCriterion;
                    case EntityType.Folder:
                        return Settings.settings.SearchFoldersCriterion;
                    default:
                        return Settings.settings.SearchSongsCriterion;
                }
            }
            set
            {
                switch (searchType)
                {
                    case EntityType.Artist:
                        Settings.settings.SearchArtistsCriterion = value;
                        break;
                    case EntityType.Album:
                        Settings.settings.SearchAlbumsCriterion = value;
                        break;
                    case EntityType.Song:
                        Settings.settings.SearchSongsCriterion = value;
                        break;
                    case EntityType.Playlist:
                        Settings.settings.SearchPlaylistsCriterion = value;
                        break;
                    case EntityType.Folder:
                        Settings.settings.SearchFoldersCriterion = value;
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
            OriginalSearchResult = searchArgs.Collection;
            ResultTextBlock.Text = searchArgs.Summary;
            SearchKeyword keyword = SearchPage.History.Peek();
            switch (searchType)
            {
                case EntityType.Artist:
                    Criteria = SearchPage.ArtistsCriteria;
                    break;
                case EntityType.Album:
                    Criteria = SearchPage.AlbumsCriteria;
                    break;
                case EntityType.Song:
                    Criteria = SearchPage.SongsCriteria;
                    break;
                case EntityType.Playlist:
                    Criteria = SearchPage.PlaylistsCriteria;
                    break;
                case EntityType.Folder:
                    Criteria = SearchPage.FoldersCriteria;
                    break;
            }
            MainPage.Instance.SetHeaderTextRaw(SearchPage.GetSearchHeader(keyword, MainPage.Instance.IsMinimal));
            switch (e.NavigationMode)
            {
                case NavigationMode.New:
                    History.Push(keyword);
                    Search(searchType, keyword, OriginalSearchResult);
                    break;
                case NavigationMode.Back:
                    if (CurrentKeyword != keyword)
                        Search(searchType, keyword, OriginalSearchResult);
                    break;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (e.NavigationMode == NavigationMode.Back)
                History.Pop();
        }

        private void Search(EntityType searchType, SearchKeyword keyword, object list)
        {
            LoadingProgress.Visibility = Visibility.Visible;
            CurrentKeyword = keyword;
            Artists.Clear();
            Albums.Clear();
            Songs.Clear();
            Playlists.Clear();
            Folders.Clear();
            switch (searchType)
            {
                case EntityType.Artist:
                    SetArtists(list as List<MatchResult<Artist>>);
                    break;
                case EntityType.Album:
                    SetAlbums(list as List<MatchResult<Album>>);
                    break;
                case EntityType.Song:
                    SetSongs(list as List<MatchResult<Music>>);
                    break;
                case EntityType.Playlist:
                    SetPlaylists(list as List<MatchResult<Playlist>>);
                    break;
                case EntityType.Folder:
                    SetFolders(list as List<MatchResult<FolderTree>>);
                    break;
            }
            LoadingProgress.Visibility = Visibility.Collapsed;
        }

        private void SetArtists(IEnumerable<MatchResult<Artist>> list)
        {
            Artists.SetTo(list.Select(i => new PlaylistView(i.Entity)));
        }

        private void SetAlbums(IEnumerable<MatchResult<Album>> list)
        {
            Albums.SetTo(list.Select(i => i.Entity.ToVO()));
        }

        private void SetSongs(IEnumerable<MatchResult<Music>> list)
        {
            Songs.SetTo(list.AsParallel().AsOrdered().Select(i => i.Entity.ToVO(isFavorite: PlaylistService.IsFavorite(i.Entity))));
        }

        private void SetPlaylists(IEnumerable<MatchResult<Playlist>> list)
        {
            Playlists.SetTo(list.Select(i => i.Entity.ToVO().ToSearchAlbumView()));
        }

        private void SetFolders(IEnumerable<MatchResult<FolderTree>> list)
        {
            Folders.SetTo(list.Select(i => new GridViewFolder(i.Entity)));
        }

        private void ArtistsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (ArtistsGridView.SelectionMode != ListViewSelectionMode.None) return;
            Frame.Navigate(typeof(ArtistsPage), e.ClickedItem);
        }

        private void AlbumsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (AlbumsGridView.SelectionMode != ListViewSelectionMode.None) return;
            Frame.Navigate(typeof(AlbumPage), e.ClickedItem);
        }

        private void PlaylistGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (PlaylistsGridView.SelectionMode != ListViewSelectionMode.None) return;
            AlbumView album = (AlbumView)e.ClickedItem;
            if (album.Name == MenuFlyoutHelper.NowPlaying)
                Frame.Navigate(typeof(NowPlayingPage));
            else if (album.Name == MenuFlyoutHelper.MyFavorites)
                Frame.Navigate(typeof(MyFavoritesPage));
            else
                Frame.Navigate(typeof(AlbumPage), album);
        }
        private void FolderGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (FoldersGridView.SelectionMode != ListViewSelectionMode.None) return;
            Frame.Navigate(typeof(LocalPage), e.ClickedItem);
        }

        private void Album_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            (args.NewValue as AlbumView)?.SetThumbnailAsync();
        }

        private void MultiSelectAppButton_Click(object sender, RoutedEventArgs e)
        {
            AlbumsGridView.SelectionMode = ListViewSelectionMode.Multiple;
            ArtistsGridView.SelectionMode = ListViewSelectionMode.Multiple;
            SearchMusicView.SelectionMode = ListViewSelectionMode.Multiple;
            PlaylistsGridView.SelectionMode = ListViewSelectionMode.Multiple;
            FoldersGridView.SelectionMode = ListViewSelectionMode.Multiple;
            MainPage.Instance.ShowMultiSelectCommandBar(new MultiSelectCommandBarOption
            {
                ShowRemove = false
            });
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (searchType == EntityType.Song)
            {
                MainPage.Instance.SetMultiSelectListener(SearchMusicView);
            }
            else
            {
                MainPage.Instance.SetMultiSelectListener(this);
            }
        }

        private List<Music> GetSelectItems()
        {
            List<Music> list = new List<Music>();
            foreach (AlbumView item in AlbumsGridView.SelectedItems)
                list.AddRange(item.Songs.Select(i => i.FromVO()));
            foreach (MusicView item in SearchMusicView.SelectedItems)
                list.Add(item.FromVO());
            foreach (AlbumView item in PlaylistsGridView.SelectedItems)
                list.AddRange(item.Songs.Select(i => i.FromVO()));
            foreach (GridViewFolder item in FoldersGridView.SelectedItems)
                list.AddRange(item.Songs);
            foreach (PlaylistView item in ArtistsGridView.SelectedItems)
                list.AddRange(item.Songs.Select(i => i.FromVO()));
            return list;
        }

        private void SortButton_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutHelper.SetSortByMenu(sender, SettingsCriterion, Criteria, item =>
            {
                LoadingProgress.Visibility = Visibility.Visible;
                SettingsCriterion = item;
                switch (searchType)
                {
                    case EntityType.Artist:
                        SetArtists(SearchHelper.SortArtists(OriginalSearchResult as List<MatchResult<Artist>>, item));
                        break;
                    case EntityType.Album:
                        SetAlbums(SearchHelper.SortAlbums(OriginalSearchResult as List<MatchResult<Album>>, item));
                        break;
                    case EntityType.Song:
                        SetSongs(SearchHelper.SortSongs(OriginalSearchResult as List<MatchResult<Music>>, item));
                        break;
                    case EntityType.Playlist:
                        SetPlaylists(SearchHelper.SortPlaylists(OriginalSearchResult as List<MatchResult<Playlist>>, item));
                        break;
                    case EntityType.Folder:
                        SetFolders(SearchHelper.SortFolders(OriginalSearchResult as List<MatchResult<FolderTree>>, item));
                        break;
                }
                LoadingProgress.Visibility = Visibility.Collapsed;
            });
        }

        private void OpenFolderMenuFlyout(object sender, object e)
        {
            var flyout = sender as MenuFlyout;
            GridViewFolder folder = flyout.Target.DataContext as GridViewFolder;
            MenuFlyoutHelper.SetSimpleFolderMenu(sender, folder.Source);
        }

        void IMultiSelectListener.Execute(MultiSelectCommandBar commandBar, MultiSelectEventArgs args)
        {
            switch (args.Event)
            {
                case MultiSelectEvent.Cancel:
                    AlbumsGridView.SelectionMode = ListViewSelectionMode.None;
                    ArtistsGridView.SelectionMode = ListViewSelectionMode.None;
                    SearchMusicView.SelectionMode = ListViewSelectionMode.None;
                    PlaylistsGridView.SelectionMode = ListViewSelectionMode.None;
                    FoldersGridView.SelectionMode = ListViewSelectionMode.None;
                    break;
                case MultiSelectEvent.AddTo:
                    args.FlyoutHelper.DefaultPlaylistName = PlaylistService.FindNextPlaylistName(CurrentKeyword.Text);
                    args.FlyoutHelper.Data = GetSelectItems();
                    break;
                case MultiSelectEvent.Play:
                    MusicPlayer.SetMusicAndPlay(GetSelectItems());
                    break;
                case MultiSelectEvent.SelectAll:
                    AlbumsGridView.SelectAll();
                    SearchMusicView.SelectAll();
                    PlaylistsGridView.SelectAll();
                    FoldersGridView.SelectAll();
                    ArtistsGridView.SelectAll();
                    break;
                case MultiSelectEvent.ClearSelections:
                    AlbumsGridView.ClearSelections();
                    SearchMusicView.ClearSelections();
                    PlaylistsGridView.ClearSelections();
                    FoldersGridView.ClearSelections();
                    ArtistsGridView.ClearSelections();
                    break;
                case MultiSelectEvent.ReverseSelections:
                    AlbumsGridView.ReverseSelections();
                    SearchMusicView.ReverseSelections();
                    PlaylistsGridView.ReverseSelections();
                    FoldersGridView.ReverseSelections();
                    ArtistsGridView.ReverseSelections();
                    break;
            }
        }
    }
}

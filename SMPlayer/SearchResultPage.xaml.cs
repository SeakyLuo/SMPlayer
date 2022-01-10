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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SearchResultPage : Page, IMultiSelectListener
    {
        public static Stack<SearchKeyword> History = new Stack<SearchKeyword>();
        public ObservableCollection<Playlist> Artists = new ObservableCollection<Playlist>();
        public ObservableCollection<AlbumView> Albums = new ObservableCollection<AlbumView>();
        public ObservableCollection<Music> Songs = new ObservableCollection<Music>();
        public ObservableCollection<AlbumView> Playlists = new ObservableCollection<AlbumView>();
        public ObservableCollection<GridViewFolder> Folders = new ObservableCollection<GridViewFolder>();
        private SortBy[] Criteria;
        private SearchType searchType;
        private SearchKeyword CurrentKeyword;
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
                    case SearchType.Folders:
                        return Settings.settings.SearchFoldersCriterion;
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
                    case SearchType.Folders:
                        Settings.settings.SearchFoldersCriterion = value;
                        break;
                }
            }
        }
        public SearchResultPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            SearchMusicView.MultiSelectOption = new MultiSelectCommandBarOption
            {
                ShowRemove = false
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SearchArgs searchArgs = (SearchArgs)e.Parameter;
            searchType = searchArgs.Type;
            object list = searchArgs.Collection;
            ResultTextBlock.Text = searchArgs.Summary;
            SearchKeyword keyword = SearchPage.History.Peek();
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
                case SearchType.Folders:
                    Criteria = SearchPage.FoldersCriteria;
                    break;
            }
            MainPage.Instance.SetHeaderTextRaw(SearchPage.GetSearchHeader(keyword, MainPage.Instance.IsMinimal));
            switch (e.NavigationMode)
            {
                case NavigationMode.New:
                    History.Push(keyword);
                    Search(searchType, keyword, list);
                    break;
                case NavigationMode.Back:
                    if (CurrentKeyword != keyword)
                        Search(searchType, keyword, list);
                    break;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (e.NavigationMode == NavigationMode.Back)
                History.Pop();
        }

        private void Search(SearchType searchType, SearchKeyword keyword, object list)
        {
            LoadingProgress.IsActive = true;
            CurrentKeyword = keyword;
            Artists.Clear();
            Albums.Clear();
            Songs.Clear();
            Playlists.Clear();
            Folders.Clear();
            switch (searchType)
            {
                case SearchType.Artists:
                    Artists.SetTo(list as ObservableCollection<Playlist>);
                    break;
                case SearchType.Albums:
                    Albums.SetTo(list as ObservableCollection<AlbumView>);
                    break;
                case SearchType.Songs:
                    Songs.SetTo(list as ObservableCollection<Music>);
                    break;
                case SearchType.Playlists:
                    Playlists.SetTo(list as ObservableCollection<AlbumView>);
                    break;
                case SearchType.Folders:
                    Folders.SetTo(list as ObservableCollection<GridViewFolder>);
                    break;
            }
            LoadingProgress.IsActive = false;
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
            if (searchType == SearchType.Songs)
            {
                MainPage.Instance.SetMultiSelectListener(SearchMusicView);
            }
            else
            {
                MainPage.Instance.SetMultiSelectListener(this);
            }
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
                    args.FlyoutHelper.DefaultPlaylistName = Settings.settings.FindNextPlaylistName(CurrentKeyword.Text);
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

        private List<Music> GetSelectItems()
        {
            List<Music> list = new List<Music>();
            foreach (AlbumView item in AlbumsGridView.SelectedItems)
                list.AddRange(item.Songs);
            foreach (Music item in SearchMusicView.SelectedItems)
                list.Add(item);
            foreach (AlbumView item in PlaylistsGridView.SelectedItems)
                list.AddRange(item.Songs);
            foreach (GridViewFolder item in FoldersGridView.SelectedItems)
                list.AddRange(item.Songs);
            foreach (Playlist item in ArtistsGridView.SelectedItems)
                list.AddRange(item.Songs);
            return list;
        }

        private void SortButton_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutHelper.ShowSortByMenu(sender, SettingsCriterion, Criteria, item => SettingsCriterion = item);
        }
    }
}

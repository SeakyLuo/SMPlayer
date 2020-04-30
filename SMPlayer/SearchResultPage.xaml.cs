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
            object list = searchArgs.Collection;
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
            SetSortByDropdownContent(searchArgs.Criterion);
            MainPage.Instance.SetHeaderText(SearchPage.GetSearchHeader(keyword, MainPage.Instance.IsMinimal));
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

        private void Search(SearchType searchType, string keyword, object list)
        {
            LoadingProgress.IsActive = true;
            CurrentKeyword = keyword;
            Artists.Clear();
            Albums.Clear();
            Songs.Clear();
            Playlists.Clear();
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
            }
            LoadingProgress.IsActive = false;
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

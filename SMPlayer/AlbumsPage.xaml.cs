using SMPlayer.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using SMPlayer.Dialogs;
using SMPlayer.Controls;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Specialized;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class AlbumsPage : Page, IAfterSongsSetListener, IImageSavedListener, IMultiSelectListener
    {
        public const string JsonFilename = "AlbumInfo";
        private ObservableCollection<AlbumView> Albums = new ObservableCollection<AlbumView>();
        private bool IsProcessing = false;
        public static volatile List<AlbumInfo> AlbumInfoList;
        private static readonly SortBy[] SortByCriteria = { SortBy.Default, SortBy.Name, SortBy.Artist };
        public List<Music> SelectedSongs
        {
            get
            {
                List<Music> list = new List<Music>();
                foreach (AlbumView album in AlbumsGridView.SelectedItems)
                {
                    list.AddRange(album.Songs);
                }
                return list;
            }
        }

        public AlbumsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            MusicLibraryPage.AddAfterSongsSetListener(this);
            AlbumArtControl.ImageSavedListeners.Add(this);
        }

        public static async Task Init()
        {
            if (AlbumInfoList != null) return;
            var albums = await JsonFileHelper.ReadObjectAsync<List<AlbumInfo>>(JsonFilename);
            if (albums == null) AlbumInfoList = new List<AlbumInfo>();
            else AlbumInfoList = albums;
        }

        public static void Save()
        {
            if (AlbumInfoList?.Count == 0) return;
            JsonFileHelper.SaveAsync(JsonFilename, AlbumInfoList);
            JsonFileHelper.SaveAsync(Helper.TempFolder, JsonFilename + Helper.TimeStamp, AlbumInfoList);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MainPage.Instance.SetMultiSelectListener(this);
            SetHeader();
            if (Albums.Count == 0) Setup(MusicLibraryPage.AllSongs); // have not listened to changes
        }

        private async void Setup(IEnumerable<Music> songs)
        {
            if (IsProcessing) return;
            AlbumPageProgressRing.Visibility = Visibility.Visible;
            if (AlbumInfoList.Count > 0)
            {
                Albums.SetTo(AlbumInfoList.Select(a => a.ToAlbumView()));
                SetHeader();
            }
            else
            {
                await SetData(songs);
            }
            AlbumPageProgressRing.Visibility = Visibility.Collapsed;
        }

        private async Task SetData(IEnumerable<Music> songs)
        {
            IsProcessing = true;
            try
            {
                List<AlbumView> albums = new List<AlbumView>();
                await Task.Run(() =>
                {
                    foreach (var group in songs.GroupBy(m => m.Album))
                    {
                        foreach (var subgroup in group.GroupBy(m => m.Artist))
                        {
                            Music music = subgroup.ElementAt(0);
                            albums.Add(new AlbumView(music.Album, music.Artist, subgroup.OrderBy(m => m.Name), false));
                        }
                    }
                    albums = Sort(Settings.settings.AlbumsCriterion, albums);
                    BuildAlbumInfoList(albums);
                    Save();
                });
                Albums.SetTo(albums);
                SetHeader();
            }
            finally
            {
                IsProcessing = false;
            }
        }

        public void SetHeader()
        {
            if (Settings.settings.ShowCount)
            {
                MainPage.Instance?.SetHeaderText("AllAbumsWithCount", Albums.Count);
            }
            else
            {
                MainPage.Instance?.SetHeaderText("AllAbums");
            }
        }

        public async void SongsSet(ICollection<Music> songs)
        {
            if (MainPage.Instance.CurrentPage == typeof(AlbumsPage))
                Setup(songs);
            else
                await SetData(songs);
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (AlbumsGridView.SelectionMode != ListViewSelectionMode.None) return;
            AlbumView album = (AlbumView)e.ClickedItem;
            if (album.Songs == null)
                album.SetSongs(AlbumPage.SearchAlbumSongs(album.Name, album.Artist));
            Frame.Navigate(typeof(AlbumPage), e.ClickedItem);
        }

        private void MenuFlyout_Opening(object sender, object e)
        {
            MenuFlyout flyout = sender as MenuFlyout;
            AlbumView album = flyout.Target.DataContext as AlbumView;
            flyout.Items.Add(MenuFlyoutHelper.GetPreferItem(album));
            MenuFlyoutItem albumArtItem = new MenuFlyoutItem()
            {
                Icon = new SymbolIcon(Symbol.Pictures),
                Text = Helper.Localize("See Album Art")
            };
            albumArtItem.Click += async (s, args) =>
            {
                await new AlbumDialog(AlbumDialogOption.AlbumArt, album).ShowAsync();
            };
            flyout.Items.Add(albumArtItem);
        }

        public void SaveAlbum(AlbumView album, BitmapImage image)
        {
            if (Albums.FirstOrDefault(a => a.Equals(album)) is AlbumView albumView)
                albumView.Thumbnail = image ?? MusicImage.DefaultImage;
        }

        public void SaveMusic(Music music, BitmapImage image)
        {
            if (Albums.FirstOrDefault(a => a.Name.Equals(music.Album)) is AlbumView albumView && albumView.Songs.Count == 1)
                albumView.Thumbnail = image ?? MusicImage.DefaultImage;
        }

        private void DropShadowControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            AlbumView album = sender.DataContext as AlbumView;
            if (album == null || album.DontLoad || !sender.IsPartiallyVisible(AlbumsGridView)) return;
            LoadThumbnail(album);
        }

        private void DropShadowControl_EffectiveViewportChanged(FrameworkElement sender, EffectiveViewportChangedEventArgs args)
        {
            AlbumView album = sender.DataContext as AlbumView;
            if (album == null || album.DontLoad || !ImageHelper.NeedsLoading(sender, args)) return;
            LoadThumbnail(album);
        }

        private async void LoadThumbnail(AlbumView album)
        {
            string before = album.ThumbnailSource;
            if (album.Songs == null)
                album.SetSongs(AlbumPage.SearchAlbumSongs(album.Name, album.Artist));
            await album.SetThumbnailAsync();
            if (album.ThumbnailSource != before && AlbumInfoList.FirstOrDefault(a => a.Equals(album)) is AlbumInfo albumInfo)
                albumInfo.Thumbnail = album.ThumbnailSource;
        }

        private void MultiSelectButton_Click(object sender, RoutedEventArgs e)
        {
            AlbumsGridView.SelectionMode = ListViewSelectionMode.Multiple;
            MainPage.Instance.ShowMultiSelectCommandBar(new MultiSelectCommandBarOption
            {
                ShowRemove = false
            });
        }

        private void SortButton_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutHelper.SetSortByMenu(sender, Settings.settings.AlbumsCriterion, SortByCriteria,
                                           async criterion => 
                                           {
                                               AlbumPageProgressRing.Visibility = Visibility.Visible;
                                               Albums.SetTo(await Task.Run(() => Sort(criterion, Albums)));
                                               AlbumPageProgressRing.Visibility = Visibility.Collapsed;
                                           },
                                           async () =>
                                           {
                                               AlbumPageProgressRing.Visibility = Visibility.Visible;
                                               Albums.CopyAndSetTo(await Task.Run(() =>
                                               {
                                                   var albums = Albums.Reverse();
                                                   BuildAlbumInfoList(albums);
                                                   return albums;
                                               })); 
                                               AlbumPageProgressRing.Visibility = Visibility.Collapsed;
                                           });
        }

        private List<AlbumView> Sort(SortBy criterion, IEnumerable<AlbumView> albums)
        {
            Settings.settings.AlbumsCriterion = criterion;
            List<AlbumView> list;
            switch (criterion)
            {
                case SortBy.Name:
                    list = albums.OrderBy(a => a.Name).ToList();
                    break;
                case SortBy.Artist:
                    list = albums.OrderBy(a => a.Artist).ToList();
                    break;
                case SortBy.Default:
                default:
                    list = albums.OrderBy(a => a.Name).ThenBy(a => a.Artist).ToList();
                    break;
            }
            BuildAlbumInfoList(list);
            return list;
        }

        private void BuildAlbumInfoList(IEnumerable<AlbumView> albums)
        {
            AlbumInfoList = albums.Select(a => a.ToAlbumInfo()).ToList();
        }

        void IMultiSelectListener.Cancel(MultiSelectCommandBar commandBar)
        {
            AlbumsGridView.SelectionMode = ListViewSelectionMode.None;
        }

        void IMultiSelectListener.AddTo(MultiSelectCommandBar commandBar, MenuFlyoutHelper helper)
        {
            helper.Data = SelectedSongs;
        }

        void IMultiSelectListener.Play(MultiSelectCommandBar commandBar)
        {
            MediaHelper.SetMusicAndPlay(SelectedSongs);
        }

        void IMultiSelectListener.Remove(MultiSelectCommandBar commandBar) { }

        void IMultiSelectListener.SelectAll(MultiSelectCommandBar commandBar)
        {
            AlbumsGridView.SelectAll();
        }

        void IMultiSelectListener.ReverseSelections(MultiSelectCommandBar commandBar)
        {
            AlbumsGridView.ReverseSelections();
        }

        void IMultiSelectListener.ClearSelections(MultiSelectCommandBar commandBar)
        {
            AlbumsGridView.SelectedItems.Clear();
        }
    }
}

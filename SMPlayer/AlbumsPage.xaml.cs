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
using SMPlayer.Helpers;
using SMPlayer.Services;
using SMPlayer.Interfaces;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class AlbumsPage : Page, IImageSavedListener, IMultiSelectListener, IMusicEventListener, IStorageItemEventListener
    {
        public const string JsonFilename = "AlbumInfo";
        private ObservableCollection<AlbumView> Suggestions = new ObservableCollection<AlbumView>();
        private List<AlbumView> Albums = new List<AlbumView>();
        private bool IsProcessing = false, IsFolderUpdated = false, IsSearching = false;
        public List<MusicView> SelectedSongs => AlbumsGridView.SelectedItems.SelectMany(a => (a as AlbumView).Songs).ToList();

        public AlbumsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            AlbumArtControl.ImageSavedListeners.Add(this);
            MusicService.AddMusicEventListener(this);
            StorageService.AddStorageItemEventListener(this);
        }


        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MainPage.Instance.SetMultiSelectListener(this);
            SetHeader();
            await Setup();
        }

        private async Task Setup()
        {
            if (IsProcessing) return;
            if (!IsFolderUpdated && Albums.IsNotEmpty()) return;
            AlbumPageProgressRing.Visibility = Visibility.Visible;
            IsProcessing = true;
            // 加一个异步，主要是为了AlbumPageProgressRing能转起来
            await Helper.RunInMainUIThread(Dispatcher, () =>
            {
                foreach (var group in MusicService.AllSongs.GroupBy(m => m.Album))
                {
                    Albums.Add(new AlbumView(group.Key, group, false));
                }
                Albums = Sort(Settings.settings.AlbumsCriterion, Albums);
            });
            Suggestions.SetTo(Albums);
            SetHeader();
            AlbumsCommandBar.IsEnabled = Albums.IsNotEmpty();
            IsProcessing = false;
            IsFolderUpdated = false;
            AlbumPageProgressRing.Visibility = Visibility.Collapsed;
        }
        
        public void SetHeader()
        {
            if (Settings.settings.ShowCount)
            {
                MainPage.Instance?.SetHeaderText("AllAbumsWithCount", Suggestions.Count);
            }
            else
            {
                MainPage.Instance?.SetHeaderText("AllAbums");
            }
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (AlbumsGridView.SelectionMode != ListViewSelectionMode.None) return;
            AlbumView album = (AlbumView)e.ClickedItem;
            if (album.Songs == null)
                album.SetSongs(MusicService.SelectByAlbum(album.Name));
            Frame.Navigate(typeof(AlbumPage), e.ClickedItem);
        }

        public void SaveAlbum(AlbumView album, BitmapImage image)
        {
            if (Suggestions.FirstOrDefault(a => a.Equals(album)) is AlbumView albumView)
                albumView.Thumbnail = image ?? MusicImage.DefaultImage;
        }

        public void SaveMusic(MusicView music, BitmapImage image)
        {
            if (Suggestions.FirstOrDefault(a => a.Name.Equals(music.Album)) is AlbumView albumView && albumView.Songs.Count == 1)
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
            if (album.Songs == null)
                album.SetSongs(MusicService.SelectByAlbum(album.Name));
            await album.SetThumbnailAsync();
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
            SortBy[] SortByCriteria = { SortBy.Reverse, SortBy.Default, SortBy.Name, SortBy.Artist };
            MenuFlyoutHelper.SetSortByMenu(sender, Settings.settings.AlbumsCriterion, SortByCriteria,
                                           async criterion => 
                                           {
                                               AlbumPageProgressRing.Visibility = Visibility.Visible;
                                               if (criterion == SortBy.Reverse)
                                               {
                                                   Suggestions.SetTo(Suggestions.Reverse().ToList());
                                               }
                                               else
                                               {
                                                   Suggestions.SetTo(await Task.Run(() => Sort(criterion, Suggestions)));
                                               }
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
                default:
                    list = albums.OrderBy(a => a.Name).ThenBy(a => a.Artist).ToList();
                    break;
            }
            return list;
        }

        private async void AlbumSearchBar_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            string keyword = sender.Text.Trim();
            if (keyword.Length > 0)
            {
                if (IsSearching) return;
                AlbumPageProgressRing.Visibility = Visibility.Visible;
                IsSearching = true;
                Suggestions.SetTo(await Task.Run(() => SearchHelper.SearchAndSortByDefault(Albums, keyword)));
                IsSearching = false;
                AlbumPageProgressRing.Visibility = Visibility.Collapsed;
            }
            else
            {
                Suggestions.SetTo(Albums);
            }
            SetHeader();
        }

        void IMultiSelectListener.Execute(MultiSelectCommandBar commandBar, MultiSelectEventArgs args)
        {
            switch (args.Event)
            {
                case MultiSelectEvent.Cancel:
                    AlbumsGridView.SelectionMode = ListViewSelectionMode.None;
                    break;
                case MultiSelectEvent.AddTo:
                    args.FlyoutHelper.Data = SelectedSongs;
                    break;
                case MultiSelectEvent.Play:
                    MusicPlayer.SetMusicAndPlay(SelectedSongs);
                    break;
                case MultiSelectEvent.SelectAll:
                    AlbumsGridView.SelectAll();
                    Helper.GetMainPageContainer()?.GetMultiSelectCommandBar().CountSelections(AlbumsGridView.SelectedItems.Count);
                    break;
                case MultiSelectEvent.ReverseSelections:
                    AlbumsGridView.ReverseSelections();
                    Helper.GetMainPageContainer()?.GetMultiSelectCommandBar().CountSelections(AlbumsGridView.SelectedItems.Count);
                    break;
                case MultiSelectEvent.ClearSelections:
                    AlbumsGridView.ClearSelections();
                    break;
            }
        }

        void IMusicEventListener.Execute(Music music, MusicEventArgs args)
        {
            if (IsFolderUpdated) return;
            AlbumView album = Albums.FirstOrDefault(a => a.Name == music.Album);
            switch (args.EventType)
            {
                case MusicEventType.Add:
                    if (album != null)
                    {
                        album.AddMusic(music.ToVO());
                    }
                    else
                    {
                        album = new AlbumView(music.ToVO());
                        int index = Albums.FindSortedListInsertIndex(album, a =>
                        {
                            switch (Settings.settings.AlbumsCriterion)
                            {
                                case SortBy.Artist:
                                    return a.Artist;
                                default:
                                    return a.Name;
                            }
                        });
                        // 如果没有搜索内容才更新Suggestions
                        if (AlbumSearchBar.Text.Length == 0)
                        {
                            Suggestions.Insert(index, album);
                        }
                        Albums.Insert(index, album);
                        SetHeader();
                    }
                    break;
                case MusicEventType.Remove:
                    if (album != null)
                    {
                        album.RemoveMusic(music.ToVO());
                        if (album.Songs.IsEmpty())
                        {
                            Suggestions.Remove(album);
                            Albums.Remove(album);
                            SetHeader();
                        }
                    }
                    break;
            }
        }

        void IStorageItemEventListener.ExecuteFileEvent(FolderFile file, StorageItemEventArgs args)
        {
        }

        private void AlbumsGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListViewBase listViewBase = (sender as ListViewBase);
            if (listViewBase.SelectionMode == ListViewSelectionMode.Multiple)
            {
                Helper.GetMainPageContainer()?.GetMultiSelectCommandBar().CountSelections(listViewBase.SelectedItems.Count);
            }
        }

        void IStorageItemEventListener.ExecuteFolderEvent(FolderTree folder, StorageItemEventArgs args)
        {
            if (args.EventType == StorageItemEventType.BeforeReset)
            {
                IsFolderUpdated = true;
                Suggestions.Clear();
                Albums.Clear();
            }
        }
    }
}

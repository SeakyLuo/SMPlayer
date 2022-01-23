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
        private bool IsProcessing = false, IsFolderUpdated = false;
        public static List<AlbumInfo> AlbumInfoList;
        public List<Music> SelectedSongs => AlbumsGridView.SelectedItems.SelectMany(a => (a as AlbumView).Songs).ToList();

        public AlbumsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            AlbumArtControl.ImageSavedListeners.Add(this);
            Settings.AddMusicEventListener(this);
            Settings.AddStorageItemEventListener(this);
        }

        public static async Task Init()
        {
            if (AlbumInfoList != null) return;
            AlbumInfoList = await JsonFileHelper.ReadObjectAsync<List<AlbumInfo>>(JsonFilename) ?? new List<AlbumInfo>();
        }

        public static void Save()
        {
            if (AlbumInfoList.IsEmpty()) return;
            JsonFileHelper.Save(JsonFilename, AlbumInfoList);
            JsonFileHelper.SaveAsync(Helper.TempFolder, JsonFilename + Helper.TimeStamp, AlbumInfoList);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MainPage.Instance.SetMultiSelectListener(this);
            SetHeader();
            await Setup();
        }

        // TODO: album不区分artist
        private async Task Setup()
        {
            if (IsProcessing) return;
            if (!IsFolderUpdated && Suggestions.IsNotEmpty()) return;
            AlbumPageProgressRing.Visibility = Visibility.Visible;
            IsProcessing = true;
            if (AlbumInfoList.IsEmpty())
            {
                // 加一个异步，主要是为了AlbumPageProgressRing能转起来
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    foreach (var group in Settings.AllSongs.GroupBy(m => m.Album))
                    {
                        foreach (var subgroup in group.GroupBy(m => m.Artist))
                        {
                            Music music = subgroup.ElementAt(0);
                            Albums.Add(new AlbumView(music.Album, music.Artist, subgroup.OrderBy(m => m.Name), false));
                        }
                    }
                    Albums = Sort(Settings.settings.AlbumsCriterion, Albums);
                    BuildAlbumInfoList(Albums);
                    Save();
                });
                Suggestions.SetTo(Albums);
            }
            else
            {
                Suggestions.SetTo(Albums = AlbumInfoList.Select(a => a.ToAlbumView()).ToList());
            }
            SetHeader();
            MultiSelectButton.IsEnabled = Albums.IsNotEmpty();
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
            if (Suggestions.FirstOrDefault(a => a.Equals(album)) is AlbumView albumView)
                albumView.Thumbnail = image ?? MusicImage.DefaultImage;
        }

        public void SaveMusic(Music music, BitmapImage image)
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
            SortBy[] SortByCriteria = { SortBy.Reverse, SortBy.Default, SortBy.Name, SortBy.Artist };
            MenuFlyoutHelper.ShowSortByMenu(sender, Settings.settings.AlbumsCriterion, SortByCriteria,
                                           async criterion => 
                                           {
                                               AlbumPageProgressRing.Visibility = Visibility.Visible;
                                               if (criterion == SortBy.Reverse)
                                               {
                                                   Albums.Reverse();
                                                   Suggestions.SetTo(Albums);
                                               }
                                               else
                                               {
                                                   Suggestions.SetTo(Albums = await Task.Run(() => Sort(criterion, Suggestions)));
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

        private void AlbumSearchBar_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            string keyword = sender.Text.Trim();
            if (keyword.Length > 0)
            {
                IEnumerable<AlbumView> albums = SearchHelper.SortAlbums(Albums.Where(i => SearchHelper.IsTargetAlbum(i, keyword)), keyword, SortBy.Default);
                Suggestions.SetTo(albums);
            }
            else
            {
                Suggestions.SetTo(Albums);
            }
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
                    break;
                case MultiSelectEvent.ReverseSelections:
                    AlbumsGridView.ReverseSelections();
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
                        album.AddMusic(music);
                    }
                    else
                    {
                        album = new AlbumView(music);
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
                        Suggestions.Insert(index, album);
                        Albums.Insert(index, album);
                        AlbumInfoList.Insert(index, album.ToAlbumInfo());
                    }
                    break;
                case MusicEventType.Remove:
                    if (album != null)
                    {
                        album.RemoveMusic(music);
                        if (album.Songs.IsEmpty())
                        {
                            Suggestions.Remove(album);
                            Albums.Remove(album);
                            AlbumInfoList.Remove(album.ToAlbumInfo());
                        }
                    }
                    break;
            }
        }

        void IStorageItemEventListener.ExecuteFileEvent(FolderFile file, StorageItemEventArgs args)
        {
        }

        void IStorageItemEventListener.ExecuteFolderEvent(FolderTree folder, StorageItemEventArgs args)
        {
            if (args.EventType == StorageItemEventType.BeforeReset)
            {
                IsFolderUpdated = true;
                Suggestions.Clear();
                Albums.Clear();
                AlbumInfoList.Clear();
            }
        }
    }
}

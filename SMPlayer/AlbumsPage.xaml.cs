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
    public sealed partial class AlbumsPage : Page, AfterSongsSetListener, ImageSavedListener
    {
        public const string JsonFilename = "Albums";
        private ObservableCollection<AlbumView> Albums = new ObservableCollection<AlbumView>();
        private bool IsProcessing = false;
        private static volatile List<AlbumInfo> albumInfoList;

        public AlbumsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            MusicLibraryPage.AddAfterSongsSetListener(this);
            AlbumArtControl.ImageSavedListeners.Add(this);
        }

        public static async Task Init()
        {
            if (albumInfoList != null) return;
            var albums = JsonFileHelper.Convert<List<AlbumInfo>>(await JsonFileHelper.ReadAsync(JsonFilename));
            if (albums == null) albumInfoList = new List<AlbumInfo>();
            else albumInfoList = albums;
        }

        public static void Save()
        {
            if (albumInfoList.Count == 0) return;
            JsonFileHelper.SaveAsync(JsonFilename, albumInfoList);
            JsonFileHelper.SaveAsync(Helper.TempFolder, JsonFilename + Helper.TimeStamp, albumInfoList);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (Albums.Count == 0) Setup(MusicLibraryPage.AllSongs); // have not listened to changes
        }

        private async void Setup(ICollection<Music> songs)
        {
            if (IsProcessing) return;
            AlbumPageProgressRing.Visibility = Visibility.Visible;
            if (albumInfoList.Count > 0)
                SetData(albumInfoList);
            else
                await SetData(songs);
            AlbumPageProgressRing.Visibility = Visibility.Collapsed;
        }

        private async Task SetData(ICollection<Music> songs)
        {
            IsProcessing = true;
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
                albums = albums.OrderBy(a => a.Name).ThenBy(a => a.Artist).ToList();
                albumInfoList = albums.Select(a => a.ToAlbumInfo()).ToList();
                Save();
            });
            Albums.SetTo(albums);
            IsProcessing = false;
        }

        private void SetData(List<AlbumInfo> albums)
        {
            Albums.SetTo(albums.Select(a => a.ToAlbumView()));
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
            AlbumView album = (AlbumView)e.ClickedItem;
            if (album.Songs == null)
                album.SetSongs(AlbumPage.SearchAlbumSongs(album.Name, album.Artist));
            Frame.Navigate(typeof(AlbumPage), e.ClickedItem);
        }

        private async void DropShadowControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (sender.DataContext is AlbumView album)
            {
                if (!album.ThumbnailLoaded)
                {
                    string before = album.ThumbnailSource;
                    if (album.Songs == null)
                        album.SetSongs(AlbumPage.SearchAlbumSongs(album.Name, album.Artist));
                    await album.SetThumbnailAsync();
                    if (album.ThumbnailSource != before && albumInfoList.FirstOrDefault(a => a.Equals(album)) is AlbumInfo albumInfo)
                        albumInfo.Thumbnail = album.ThumbnailSource;
                }
            }
        }

        private void MenuFlyout_Opening(object sender, object e)
        {
            MenuFlyout flyout = sender as MenuFlyout;
            MenuFlyoutItem albumArtItem = new MenuFlyoutItem()
            {
                Icon = new SymbolIcon(Symbol.Pictures),
                Text = Helper.Localize("See Album Art")
            };
            albumArtItem.Click += async (s, args) =>
            {
                await new AlbumDialog(AlbumDialogOption.AlbumArt, flyout.Target.DataContext as AlbumView).ShowAsync();
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
    }
}

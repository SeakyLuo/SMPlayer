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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class AlbumsPage : Page, AfterSongsSetListener
    {
        private ObservableCollection<AlbumView> Albums = new ObservableCollection<AlbumView>();
        private bool IsProcessing = false;
        public AlbumsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            MusicLibraryPage.AddAfterSongsSetListener(this);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (Albums.Count == 0) Setup(MusicLibraryPage.AllSongs); // have not listened to changes
        }

        private async void Setup(ICollection<Music> songs)
        {
            if (IsProcessing) return;
            AlbumPageProgressRing.IsActive = true;
            await SetData(songs);
            AlbumPageProgressRing.IsActive = false;
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
            });
            Albums.SetTo(albums);
            IsProcessing = false;
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
            Frame.Navigate(typeof(AlbumPage), e.ClickedItem);
        }

        private void DropShadowControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            (sender.DataContext as AlbumView)?.SetCover();
        }
    }
}

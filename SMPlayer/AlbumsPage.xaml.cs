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
    public sealed partial class AlbumsPage : Page, AfterSongsSetListener
    {
        private ObservableCollection<AlbumView> Albums = new ObservableCollection<AlbumView>();
        private ExecutionStatus status = ExecutionStatus.Ready;
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
            if (status == ExecutionStatus.Running) return;
            AlbumPageProgressRing.IsActive = true;
            status = ExecutionStatus.Running;
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                SetData(songs);
                status = ExecutionStatus.Ready;
                AlbumPageProgressRing.IsActive = false;
            });
        }

        private void SetData(ICollection<Music> songs)
        {
            List<AlbumView> albums = new List<AlbumView>();
            foreach (var group in songs.GroupBy(m => m.Album))
            {
                foreach (var subgroup in group.GroupBy(m => m.Artist))
                {
                    Music music = subgroup.ElementAt(0);
                    albums.Add(new AlbumView(music.Album, music.Artist, subgroup.OrderBy(m => m.Name), false));
                }
            }
            Albums.SetTo(albums.OrderBy(a => a.Name).ThenBy(a => a.Artist));
        }

        public void SongsSet(ICollection<Music> songs)
        {
            if (MainPage.Instance.CurrentPage == typeof(AlbumsPage))
                Setup(songs);
            else
                SetData(songs);
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

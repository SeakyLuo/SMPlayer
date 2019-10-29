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
        private bool SetupStarted = false;
        private ExecutionStatus status = ExecutionStatus.Ready;
        public AlbumsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            MusicLibraryPage.AddAfterSongsSetListener(this);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (Albums.Count == 0) Setup(MusicLibraryPage.AllSongs); // Constructor not called
        }

        private void Setup(ICollection<Music> songs)
        {
            if (SetupStarted) return;
            if (status == ExecutionStatus.Done)
            {
                status = ExecutionStatus.Ready;
                return;
            }
            SetupStarted = true;
            AlbumPageProgressRing.IsActive = true;
            Albums.Clear();
            List<AlbumView> albums = new List<AlbumView>();
            foreach (var group in songs.GroupBy((m) => m.Album))
            {
                foreach (var subgroup in group.GroupBy((m) => m.Artist))
                {
                    Music music = subgroup.ElementAt(0);
                    albums.Add(new AlbumView(music.Album, music.Artist, subgroup.OrderBy((m) => m.Name)));
                }
            }
            foreach (var album in albums.OrderBy((a) => a.Name).ThenBy((a) => a.Artist)) Albums.Add(album);
            if (status == ExecutionStatus.Running) status = ExecutionStatus.Done;
            AlbumPageProgressRing.IsActive = false;
            SetupStarted = false;
        }

        public async void SongsSet(ICollection<Music> songs)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
             {
                 status = ExecutionStatus.Running;
                 Setup(songs);
             });
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(AlbumPage), e.ClickedItem);
        }

        private void DropShadowControl_Loaded(object sender, RoutedEventArgs e)
        {
            ((sender as FrameworkElement).DataContext as AlbumView)?.FindThumbnail();
        }
    }
}

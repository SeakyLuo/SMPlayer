using Microsoft.Toolkit.Uwp.UI.Controls;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
            MusicLibraryPage.AddAfterSongsSetListener(this as AfterSongsSetListener);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (Albums.Count == 0) Setup(); // Constructor not called
        }

        private void Setup()
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
            foreach (var group in MusicLibraryPage.AllSongs.GroupBy((m) => m.Album))
            {
                foreach (var subgroup in group.GroupBy((m) => m.Artist))
                {
                    Music music = subgroup.ElementAt(0);
                    albums.Add(new AlbumView(music.Album, music.Artist, group.OrderBy((m) => m.Name).ThenBy((m) => m.Artist)));
                }
            }
            foreach (var album in albums.OrderBy((a) => a.Name).ThenBy((a) => a.Artist)) Albums.Add(album);
            if (status == ExecutionStatus.Running) status = ExecutionStatus.Done;
            AlbumPageProgressRing.IsActive = false;
            SetupStarted = false;
        }

        public void SongsSet(ICollection<Music> songs)
        {
            status = ExecutionStatus.Running;
            Setup();
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(AlbumPage), e.ClickedItem);
        }
    }
}

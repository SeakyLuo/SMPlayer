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
    public sealed partial class AlbumsPage : Page, AfterPathSetListener
    {
        private ObservableCollection<GridAlbumView> Albums = new ObservableCollection<GridAlbumView>();
        private SortedDictionary<string, List<Music>> GroupedMusic = new SortedDictionary<string, List<Music>>();
        private bool SetupStarted = false;
        private int Notified = 0;
        public AlbumsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            SettingsPage.AddAfterPathSetListener(this);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Setup();
        }

        private void GridAlbumViewItem_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var shadow = (DropShadowPanel)sender;
            shadow.ShadowOpacity = 0;
        }

        private void GridAlbumViewItem_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var shadow = (DropShadowPanel)sender;
            shadow.ShadowOpacity = 1;
        }

        private async void Setup()
        {
            if (SetupStarted) return;
            if (Notified == 1)
            {
                Notified = 0;
                return;
            }
            SetupStarted = true;
            AlbumPageProgressRing.IsActive = true;
            AlbumPageProgressRing.Visibility = Visibility.Visible;
            Albums.Clear();
            List<GridAlbumView> gridAlbums = new List<GridAlbumView>();
            GroupedMusic.Clear();
            foreach (var group in MusicLibraryPage.AllSongs.GroupBy((m) => m.Album))
            {
                Music music = null;
                Windows.UI.Xaml.Media.Imaging.BitmapImage thumbnail = null;
                foreach (Music m in group)
                {
                    thumbnail = await Helper.GetThumbnail(m.Path, false);
                    if (thumbnail != null)
                    {
                        music = m;
                        break;
                    }
                }
                if (music == null)
                {
                    music = group.ElementAt(0);
                    thumbnail = Helper.DefaultAlbumCover;
                }
                var album = new GridAlbumView(music.Album, music.Artist, thumbnail);
                gridAlbums.Add(album);
                GroupedMusic.Add(music.Album, group.OrderBy((m) => m.Name).ThenBy((m) => m.Artist).ToList());
            }
            foreach (var album in gridAlbums.OrderBy((a) => a.Name).ThenBy((a) => a.Artist)) Albums.Add(album);
            if (Notified == 2) Notified = 1;
            AlbumPageProgressRing.Visibility = Visibility.Collapsed;
            AlbumPageProgressRing.IsActive = false;
            SetupStarted = false;
        }

        public void PathSet(string path)
        {
            Notified = 2;
            Setup();
        }
    }
}

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
    public sealed partial class AlbumsPage : Page
    {
        private ObservableCollection<GridAlbumView> Albums = new ObservableCollection<GridAlbumView>();
        private SortedDictionary<string, List<Music>> GroupedMusic = new SortedDictionary<string, List<Music>>();
        public AlbumsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Albums.Clear();
            GroupedMusic.Clear();
            foreach (var group in MusicLibraryPage.AllSongs.GroupBy((m) => m.Album))
            {
                Music music = null;
                Windows.UI.Xaml.Media.Imaging.BitmapImage thumbnail = null;
                foreach(Music m in group)
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
                Albums.Add(album);
                GroupedMusic.Add(music.Album, group.OrderBy((m) => m.Name).ThenBy((m) => m.Artist).ToList());
            }
            Albums = new ObservableCollection<GridAlbumView>(Albums.OrderBy((a) => a.Name).ThenBy((a) => a.Artist));
        }
    }
}

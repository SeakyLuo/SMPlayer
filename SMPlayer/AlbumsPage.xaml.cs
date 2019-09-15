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
        private NotifiedStatus Notified = NotifiedStatus.Ready;
        public AlbumsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            System.Diagnostics.Debug.WriteLine("Albums");
            MusicLibraryPage.AddAfterSongsSetListener(this as AfterSongsSetListener);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (Albums.Count == 0) Setup(); // Constructor not called
        }

        private void GridViewItem_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(sender as Control, "PointerOver", true);
        }

        private void GridViewItem_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(sender as Control, "Normal", true);
        }

        private async void Setup()
        {
            if (SetupStarted) return;
            if (Notified == NotifiedStatus.Finished)
            {
                Notified = NotifiedStatus.Ready;
                return;
            }
            SetupStarted = true;
            AlbumPageProgressRing.IsActive = true;
            AlbumPageProgressRing.Visibility = Visibility.Visible;
            Albums.Clear();
            List<AlbumView> albums = new List<AlbumView>();
            foreach (var group in MusicLibraryPage.AllSongs.GroupBy((m) => m.Album))
            {
                Music music = null;
                Windows.UI.Xaml.Media.Imaging.BitmapImage thumbnail = null;
                foreach (Music m in group)
                {
                    thumbnail = await Helper.GetThumbnailAsync(m.GetShortPath(), false);
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
                var album = new AlbumView(music.Album, music.Artist, thumbnail, group.OrderBy((m) => m.Name).ThenBy((m) => m.Artist));
                albums.Add(album);
            }
            foreach (var album in albums.OrderBy((a) => a.Name).ThenBy((a) => a.Artist)) Albums.Add(album);
            if (Notified == NotifiedStatus.Started) Notified = NotifiedStatus.Finished;
            AlbumPageProgressRing.Visibility = Visibility.Collapsed;
            AlbumPageProgressRing.IsActive = false;
            SetupStarted = false;
        }

        public void SongsSet(ICollection<Music> songs)
        {
            Notified = NotifiedStatus.Started;
            Setup();
        }

        private void MenuFlyout_Opening(object sender, object e)
        {
            MenuFlyoutHelper.SetPlaylistMenu(sender);
        }

        private void PlayAllButton_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as Button).DataContext as AlbumView;
            MediaHelper.ShuffleAndPlay(data.Songs);
        }
        private void AddToButton_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as Button).DataContext as AlbumView;
            var helper = new MenuFlyoutHelper() { Data = data.Songs };
            helper.GetAddToPlaylistsMenuFlyout(data.Name).ShowAt(sender as FrameworkElement);
        }
        private void GridViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(AlbumPage), (sender as FrameworkElement).DataContext);
        }
    }
}

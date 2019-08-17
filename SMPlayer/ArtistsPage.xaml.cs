using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
    public sealed partial class ArtistsPage : Page, AfterPathSetListener
    {
        private ObservableCollection<ArtistView> Artists = new ObservableCollection<ArtistView>();
        private bool SetupStarted = false;
        private int Notified = 0;
        public ArtistsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            SettingsPage.AddAfterPathSetListener(this);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Setup();
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
            ArtistsProgressRing.IsActive = true;
            Artists.Clear();
            List<ArtistView> artists = new List<ArtistView>();
            foreach (var group in MusicLibraryPage.AllSongs.GroupBy((m) => m.Artist))
            {
                List<AlbumView> albums = new List<AlbumView>();
                foreach (var songs in group.GroupBy((m) => m.Album))
                {
                    Windows.UI.Xaml.Media.Imaging.BitmapImage thumbnail = null;
                    foreach (var music in songs)
                    {
                        thumbnail = await Helper.GetThumbnail(music.Path, false);
                        if (thumbnail != null) break;
                    }
                    if (thumbnail == null) thumbnail = Helper.DefaultAlbumCover;
                    var album = new AlbumView(songs.Key, group.Key, thumbnail, songs.OrderBy((m) => m.Name).ToList());
                    albums.Add(album);
                }
                artists.Add(new ArtistView(group.Key, albums));
            }
            foreach (var artist in artists.OrderBy((a) => a.Name)) Artists.Add(artist);
            ArtistsCountTextBlock.Text = "All Artists: " + Artists.Count;
            if (Notified == 2) Notified = 1;
            ArtistsProgressRing.IsActive = false;
            SetupStarted = false;
        }

        public void PathSet(string path)
        {
            Notified = 2;
            Setup();
        }

        private void SongsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Music music = (Music)e.ClickedItem;
            MainPage.Instance.SetMusicAndPlay(music);
        }

        private void SongsListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            args.ItemContainer.Background = new SolidColorBrush(args.ItemIndex % 2 == 0 ? Colors.WhiteSmoke : Colors.White);
        }

        private void PlayItem_Click(object sender, RoutedEventArgs e)
        {
            Music music = (sender as MenuFlyoutItem).DataContext as Music;
            MainPage.Instance.SetMusicAndPlay(music);
        }
    }
}

using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
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
    public sealed partial class ArtistsPage : Page, AfterPathSetListener, MediaControlListener
    {
        private ObservableCollection<ArtistView> Artists = new ObservableCollection<ArtistView>();
        private bool SetupStarted = false;
        private int Notified = 0;

        public ArtistsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            SettingsPage.AddAfterPathSetListener(this as AfterPathSetListener);
            MediaHelper.AddMediaControlListener(this as MediaControlListener);
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
            ArtistsCountTextBlock.Text = "";
            ArtistProgressBar.Visibility = Visibility.Visible;
            Artists.Clear();
            List<ArtistView> artists = new List<ArtistView>();
            foreach (var group in MusicLibraryPage.AllSongs.GroupBy((m) => m.Artist))
            {
                ObservableCollection<AlbumView> albums = new ObservableCollection<AlbumView>();
                foreach (var songs in group.GroupBy((m) => m.Album))
                {
                    Windows.UI.Xaml.Media.Imaging.BitmapImage thumbnail = null;
                    foreach (var music in songs)
                    {
                        thumbnail = await Helper.GetThumbnail(music.Path, false);
                        if (thumbnail != null) break;
                    }
                    if (thumbnail == null) thumbnail = Helper.DefaultAlbumCover;
                    var album = new AlbumView(songs.Key, group.Key, thumbnail, songs.OrderBy((m) => m.Name));
                    albums.Add(album);
                }
                artists.Add(new ArtistView(group.Key, albums));
            }
            foreach (var artist in artists.OrderBy((a) => a.Name)) Artists.Add(artist);
            ArtistsCountTextBlock.Text = "All Artists: " + Artists.Count;
            FindMusicAndSetPlaying(MediaHelper.CurrentMusic, true);
            if (Notified == 2) Notified = 1;
            ArtistProgressBar.Visibility = Visibility.Collapsed;
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
            ListView listView = (ListView)sender;
            SetMusicAndPlay(music, listView.ItemsSource as ObservableCollection<Music>);
        }

        private void SongsListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            args.ItemContainer.Background = args.ItemIndex % 2 == 0 ? Helper.WhiteSmokeBrush : Helper.WhiteBrush;
        }

        private async void SetMusicAndPlay(Music music, IEnumerable<Music> playlist)
        {
            if (!music.Equals(MediaHelper.CurrentMusic))
            {
                FindMusicAndSetPlaying(MediaHelper.CurrentMusic, false);
                await MediaHelper.SetPlayList(playlist);
            }
            MainPage.Instance.SetMusicAndPlay(music);
        }

        private void PlayItem_Click(object sender, RoutedEventArgs e)
        {
            Music music = (sender as MenuFlyoutItem).DataContext as Music;
            SetMusicAndPlay(music, Artists.First((a) => a.Name == music.Artist).Albums.First((a) => a.Name == music.Album).Songs);
        }

        private void FindMusicAndSetPlaying(Music target, bool isPlaying)
        {
            if (target == null) return;
            var artist = Artists.FirstOrDefault((a) => a.Name == target.Artist);
            if (artist == null) return;
            var album = artist.Albums.FirstOrDefault((a) => a.Name == target.Album);
            if (album == null) return;
            var music = album.Songs.FirstOrDefault((m) => m.Equals(target));
            music.IsPlaying = isPlaying;
        }

        public void Tick()
        {
            return;
        }

        public async void MusicSwitching(Music current, Music next, Windows.Media.Playback.MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                foreach (var target in new Music[] { current, next })
                    FindMusicAndSetPlaying(target, next.Equals(target));
            });
        }

        public void MediaEnded() { return; }
        public void ShuffleChanged(IEnumerable<Music> newPlayList, bool isShuffle) { return; }

    }
}

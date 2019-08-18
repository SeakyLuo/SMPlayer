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
        private ListViewItem prevItem;
        private SolidColorBrush WhiteBrush = new SolidColorBrush(Colors.White);
        private SolidColorBrush WhiteSmokeBrush = new SolidColorBrush(Colors.WhiteSmoke);
        private SolidColorBrush BlackBrush = new SolidColorBrush(Colors.Black);

        public ArtistsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            SettingsPage.AddAfterPathSetListener(this as AfterPathSetListener);
            MediaControl.AddMediaControlListener(this as MediaControlListener);
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
            if (Notified == 2) Notified = 1;
            ArtistsProgressRing.IsActive = false;
            SetupStarted = false;
        }

        public void PathSet(string path)
        {
            Notified = 2;
            Setup();
        }

        private async void SongsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Music music = (Music)e.ClickedItem;
            //if (prevItem != null) SetColor(BlackBrush, Visibility.Collapsed);
            ListView listView = (ListView)sender;
            prevItem = (ListViewItem)listView.ContainerFromItem(music);
            //SetColor(new SolidColorBrush((Color)Resources["SystemColorHighlightColor"]), Visibility.Visible);
            await MediaControl.SetPlayList(listView.ItemsSource as ObservableCollection<Music>);
            MainPage.Instance.SetMusicAndPlay(music);
        }

        private void SetColor(Brush brush, Visibility visibility)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(prevItem); i++)
            {
                var child = VisualTreeHelper.GetChild(prevItem, i);
                if (child is TextBlock)
                    (child as TextBlock).Foreground = brush;
                else
                    (child as ListViewItemPresenter).Visibility = visibility;
            }

        }

        private void SongsListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            args.ItemContainer.Background = args.ItemIndex % 2 == 0 ? WhiteSmokeBrush : WhiteBrush;
            args.ItemContainer.Foreground = (Music)args.Item == MediaControl.CurrentMusic ? new SolidColorBrush((Color)Resources["SystemColorHighlightColor"]) : BlackBrush;
        }

        private async void PlayItem_Click(object sender, RoutedEventArgs e)
        {
            Music music = (sender as MenuFlyoutItem).DataContext as Music;
            await MediaControl.SetPlayList((e.OriginalSource as ListView).ItemsSource as List<Music>);
            MainPage.Instance.SetMusicAndPlay(music);
        }

        public void Tick()
        {
            return;
        }

        public async void MusicSwitching(Music current, Music next)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                //var item = ArtistMasterDetailsView.Items.FirstOrDefault((a) => (a as ArtistView).Name == next.Artist);
                //if (item == null) return;
                //var container = ArtistMasterDetailsView.ContainerFromItem(item);
            });
        }

        public void MediaEnded()
        {
            return;
        }
    }
}

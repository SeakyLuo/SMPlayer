using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ArtistsPage : Page, AfterSongsSetListener, SwitchMusicListener
    {
        private ObservableCollection<ArtistView> Artists = new ObservableCollection<ArtistView>();
        private bool SetupStarted = false;
        private ExecutionStatus status = ExecutionStatus.Ready;
        private object targetArtist;

        public ArtistsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            MusicLibraryPage.AddAfterSongsSetListener(this);
            MediaHelper.SwitchMusicListeners.Add(this);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (Artists.Count == 0) Setup();
            targetArtist = e.Parameter;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (targetArtist == null) return;
            ArtistView artist = null;
            if (targetArtist is string artistName)
            {
                artist = Artists.FirstOrDefault((a) => a.Name == artistName);
                if (artist.NotLoaded || !MusicLibraryPage.IsLibraryUnchangedAfterChecking)
                    artist.Load();
            }
            else if (targetArtist is Playlist playlist)
            {
                artist = Artists.FirstOrDefault((a) => a.Name == playlist.Artist);
                if (artist.NotLoaded || !MusicLibraryPage.IsLibraryUnchangedAfterChecking)
                    artist.CopyFrom(playlist);
            }
            ArtistMasterDetailsView.SelectedItem = artist;
            targetArtist = null;
            //(ArtistMasterDetailsView.ContainerFromItem(artist) as UIElement)?.StartBringIntoView();
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
            ArtistsCountTextBlock.Text = "";
            ArtistProgressBar.Visibility = Visibility.Visible;
            Artists.Clear();
            List<ArtistView> artists = new List<ArtistView>();
            foreach (var group in MusicLibraryPage.AllSongs.GroupBy((m) => m.Artist))
                artists.Add(new ArtistView(group.Key));
            foreach (var artist in artists.OrderBy((a) => a.Name))
                Artists.Add(artist);
            ArtistsCountTextBlock.Text = Helper.Localize("All Artists: ") + Artists.Count;
            FindMusicAndSetPlaying(MediaHelper.CurrentMusic);
            if (status == ExecutionStatus.Running) status = ExecutionStatus.Done;
            ArtistProgressBar.Visibility = Visibility.Collapsed;
            SetupStarted = false;
        }

        public void SongsSet(ICollection<Music> songs)
        {
            status = ExecutionStatus.Running;
            Setup();
        }

        private async void Artist_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var artist = (sender as FrameworkElement).DataContext as ArtistView;
                if (artist.NotLoaded || !MusicLibraryPage.IsLibraryUnchangedAfterChecking)
                    artist.Load();
            });
        }

        private void SongsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Music music = (Music)e.ClickedItem;
            ListView listView = (ListView)sender;
            MainPage.Instance.ShowAddMusicResultNotification(listView.ItemsSource as ObservableCollection<Music>, music);
        }

        private void SongsListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            args.ItemContainer.Background = args.ItemIndex % 2 == 0 ? ColorHelper.WhiteSmokeBrush : ColorHelper.WhiteBrush;
        }

        private void FindMusicAndSetPlaying(Music target)
        {
            foreach (var artist in Artists)
                foreach (var album in artist.Albums)
                    foreach (var music in album.Songs)
                        music.IsPlaying = music.Equals(target);
        }

        public async void MusicSwitching(Music current, Music next, Windows.Media.Playback.MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => FindMusicAndSetPlaying(next));
        }

        private void OpenPlaylistMenuFlyout(object sender, object e)
        {
            (((sender as MenuFlyout).Target as FrameworkElement).DataContext as ArtistView).Load();
            MenuFlyoutHelper.SetPlaylistMenu(sender);
        }
        private void OpenMusicMenuFlyout(object sender, object e)
        {
            MenuFlyoutHelper.SetMusicMenu(sender);
        }

        private void Artist_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            ((sender as StackPanel).Children[1] as ScrollingTextBlock).StartScrolling();
        }
    }
}

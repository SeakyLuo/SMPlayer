using SMPlayer.Helpers;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
        private List<string> SuggestionList = new List<string>();
        private ObservableCollection<string> Suggestions = new ObservableCollection<string>();
        private ExecutionStatus status = ExecutionStatus.Ready;
        private object targetArtist;

        public ArtistsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            MusicLibraryPage.AddAfterSongsSetListener(this);
            MediaHelper.SwitchMusicListeners.Add(this);
            Controls.MusicInfoControl.MusicModifiedListeners.Add((before, after) =>
            {
                var oldArtist = Artists.First(a => a.Name == before.Artist);
                if (oldArtist.Equals(ArtistMasterDetailsView.SelectedItem) || !oldArtist.NotLoaded) oldArtist.Load();
                if (SuggestionList.Contains(after.Artist))
                {
                    if (before.Artist != after.Artist)
                    {
                        var newArtist = Artists.First(a => a.Name == after.Artist);
                        if (newArtist.Equals(ArtistMasterDetailsView.SelectedItem))
                        {
                            newArtist.Load();
                            FindMusicAndSetPlaying(after);
                        }
                        else if (!newArtist.NotLoaded) newArtist.Load();
                    }
                }
                else
                {
                    SuggestionList.Add(after.Artist);
                    Artists.Add(new ArtistView(after));
                    Artists.SetTo(Artists.OrderBy(a => a.Name));
                }
            });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (Artists.Count == 0) Setup(MusicLibraryPage.AllSongs);
            targetArtist = e.Parameter;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (targetArtist == null) return;
            ArtistView artist = null;
            if (targetArtist is string artistName)
            {
                artist = FindAndLoadArtist(artistName);
            }
            else if (targetArtist is Playlist playlist)
            {
                artist = Artists.FirstOrDefault(a => a.Name == playlist.Artist);
                if (artist.NotLoaded || !MusicLibraryPage.IsLibraryUnchangedAfterChecking)
                    artist.CopyFrom(playlist);
            }
            ArtistMasterDetailsView.SelectedItem = artist;
            targetArtist = null;
            (ArtistMasterDetailsView.ContainerFromItem(artist) as UIElement)?.Locate();
        }

        private async void Setup(ICollection<Music> songs)
        {
            if (status == ExecutionStatus.Running) return;
            LoadingProgress.Visibility = Visibility.Visible;
            status = ExecutionStatus.Running;
            await Task.Run(() => SetData(songs));
            status = ExecutionStatus.Ready;
            LoadingProgress.Visibility = Visibility.Collapsed;
        }

        private void SetData(ICollection<Music> songs)
        {
            Artists.Clear();
            Suggestions.Clear();
            List<ArtistView> artists = new List<ArtistView>();
            foreach (var group in songs.GroupBy(m => m.Artist))
                artists.Add(new ArtistView(group.Key));
            foreach (var artist in artists.OrderBy(a => a.Name))
            {
                Artists.Add(artist);
                SuggestionList.Add(artist.Name);
            }
            Suggestions.SetTo(SuggestionList);
            ArtistSearchBox.PlaceholderText = Helper.Localize("All Artists") + Artists.Count;
            FindMusicAndSetPlaying(MediaHelper.CurrentMusic);
        }

        public void SongsSet(ICollection<Music> songs)
        {
            if (MainPage.Instance.CurrentPage == typeof(ArtistsPage))
                Setup(songs);
            else
                SetData(songs);
        }

        private async void Artist_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                var artist = (sender as FrameworkElement).DataContext as ArtistView;
                LoadArtist(artist);
            });
        }

        private void SongsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Music music = (Music)e.ClickedItem;
            ListView listView = (ListView)sender;
            MediaHelper.SetMusicAndPlay(listView.ItemsSource as ObservableCollection<Music>, music);
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

        private void ArtistMenuFlyout_Opening(object sender, object e)
        {
            var artistView = ((sender as MenuFlyout).Target as FrameworkElement).DataContext as ArtistView;
            if (artistView.NotLoaded) artistView.Load();
            MenuFlyoutHelper.SetPlaylistMenu(sender);
        }
        private void AlbumMenuFlyout_Opening(object sender, object e)
        {
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

        private void ArtistSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            Suggestions.SetTo(SuggestionList.Where(s => SearchHelper.IsTargetArtist(s, sender.Text)));
        }

        private void ArtistSearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            var artistName = (string)args.SelectedItem;
            ArtistMasterDetailsView.SelectedItem = FindAndLoadArtist(artistName);
            (ArtistMasterDetailsView.ContainerFromItem(artistName) as UIElement)?.Locate();
        }

        private ArtistView FindAndLoadArtist(string artistName)
        {
            var artist = Artists.First(a => a.Name == artistName);
            LoadArtist(artist);
            return artist;
        }

        private void LoadArtist(ArtistView artist)
        {
            if (artist.NotLoaded || !MusicLibraryPage.IsLibraryUnchangedAfterChecking)
                artist.Load();
        }

        private void ArtistSearchBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter && Suggestions.Count > 0)
            {
                string artist = Suggestions[0];
                ArtistSearchBox.Text = artist;
                ArtistMasterDetailsView.SelectedItem = FindAndLoadArtist(artist);
            }
        }
    }
}

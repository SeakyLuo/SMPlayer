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
        private bool IsProcessing = false;
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

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (targetArtist == null) return;
            if (IsProcessing)
            {
                await Task.Run(async () =>
                {
                    while (IsProcessing) ;
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, LocateArtist);
                });
            }
            else
            {
                LocateArtist();
            }
        }

        private void LocateArtist()
        {
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
            ScrollToArtist(artist.Name);
            targetArtist = null;
        }

        private void ScrollToArtist(string artist)
        {
            if (ArtistMasterDetailsView.GetFirstDescendantOfType<ScrollViewer>() is ScrollViewer scrollViewer)
            {
                int index = Artists.FindIndex(a => a.Name == artist);
                if (scrollViewer.IsLoaded)
                {
                    double itemHeight = scrollViewer.ExtentHeight / Artists.Count;
                    scrollViewer.ChangeView(null, itemHeight * index, null, false);
                }
                else
                {
                    scrollViewer.Loaded += (s, args) =>
                    {
                        double itemHeight = scrollViewer.ExtentHeight / Artists.Count;
                        scrollViewer.ChangeView(null, itemHeight * index, null, false);
                    };
                }
            }
        }

        private async void Setup(ICollection<Music> songs)
        {
            if (IsProcessing) return;
            LoadingProgress.Visibility = Visibility.Visible;
            await SetData(songs);
            LoadingProgress.Visibility = Visibility.Collapsed;
        }

        private async Task SetData(ICollection<Music> songs)
        {
            IsProcessing = true;
            List<ArtistView> artists = new List<ArtistView>();
            await Task.Run(() =>
            {
                foreach (var group in songs.GroupBy(m => m.Artist))
                    artists.Add(new ArtistView(group.Key));
                artists = artists.OrderBy(a => a.Name).ToList();
            });
            Artists.Clear();
            SuggestionList.Clear();
            Suggestions.Clear();
            foreach (var artist in artists)
            {
                Artists.Add(artist);
                SuggestionList.Add(artist.Name);
                Suggestions.Add(artist.Name);
            }
            FindMusicAndSetPlaying(MediaHelper.CurrentMusic);
            ArtistSearchBox.PlaceholderText = Helper.LocalizeMessage("AllArtists", Artists.Count);
            IsProcessing = false;
        }

        public async void SongsSet(ICollection<Music> songs)
        {
            if (MainPage.Instance.CurrentPage == typeof(ArtistsPage))
                Setup(songs);
            else
                await SetData(songs);
        }

        private void Artist_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var artist = (sender as FrameworkElement).DataContext as ArtistView;
            LoadArtist(artist);
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
            MenuFlyout flyout = sender as MenuFlyout;
            var artistView = flyout.Target.DataContext as ArtistView;
            if (artistView.NotLoaded) artistView.Load();
            MenuFlyoutHelper.SetPlaylistMenu(sender);
            var refreshArtist = new MenuFlyoutItem()
            {
                Text = Helper.Localize("RefreshArtist"),
                Icon = new SymbolIcon(Symbol.Refresh)
            };
            refreshArtist.Click += async (s, args) =>
            {
                if (artistView.IsLoading)
                {
                    MainPage.Instance.ShowNotification("ProcessingRequest");
                    return;
                }
                await artistView.LoadAsync();
            };
            flyout.Items.Add(refreshArtist);
        }
        private void AlbumMenuFlyout_Opening(object sender, object e)
        {
            MenuFlyoutHelper.SetPlaylistMenu(sender);
            MenuFlyout flyout = sender as MenuFlyout;
            var album = flyout.Target.DataContext as AlbumView;
            flyout.Items.Add(MenuFlyoutHelper.GetSeeAlbumFlyout(album.Songs[0]));
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
            ScrollToArtist(artistName);
        }

        private ArtistView FindAndLoadArtist(string artistName)
        {
            var artist = Artists.First(a => a.Name == artistName);
            LoadArtist(artist);
            return artist;
        }

        private async void LoadArtist(ArtistView artist)
        {
            if (artist.NotLoaded || !MusicLibraryPage.IsLibraryUnchangedAfterChecking)
                await artist.LoadAsync();
        }

        private void ArtistSearchBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter && Suggestions.Count > 0)
            {
                string artist = Suggestions[0];
                ArtistSearchBox.Text = artist;
                ArtistMasterDetailsView.SelectedItem = FindAndLoadArtist(artist);
                ScrollToArtist(artist);
            }
        }

        private void AlbumCover_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            (sender.DataContext as AlbumView)?.SetCover();
        }
    }
}

using SMPlayer.Controls;
using SMPlayer.Helpers;
using SMPlayer.Interfaces;
using SMPlayer.Models;
using SMPlayer.Services;
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
    public sealed partial class ArtistsPage : Page, IMusicEventListener, IMusicPlayerEventListener, IMenuFlyoutItemClickListener, IMultiSelectListener, IInitListener, IStorageItemEventListener
    {
        public static ArtistsPage Instance { get => MainPage.Instance.NavigationFrame.Content as ArtistsPage; }
        private ObservableCollection<ArtistView> Artists = new ObservableCollection<ArtistView>();
        private List<string> SuggestionList = new List<string>();
        private ObservableCollection<string> Suggestions = new ObservableCollection<string>();
        private bool IsProcessing = false, IsFolderUpdated = false;
        private object targetArtist;
        private List<ListView> listViews = new List<ListView>();
        private IInitListener initListener;
        private ArtistView SelectedArtist => ArtistMasterDetailsView.SelectedItem as ArtistView;
        private List<MusicView> SelectedItems
        {
            get
            {
                List<MusicView> playlist = new List<MusicView>();
                foreach (ListView listView in listViews)
                {
                    foreach (MusicView music in listView.SelectedItems)
                    {
                        playlist.Add(music);
                    }
                }
                return playlist;
            }
        }

        public ArtistsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            MusicService.AddMusicEventListener(this);
            StorageService.AddStorageItemEventListener(this);
            MusicPlayer.AddMusicPlayerEventListener(this);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (Artists.IsEmpty()) Setup(MusicService.AllSongs);
            targetArtist = e.Parameter;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SetHeader();
            MainPage.Instance.SetMultiSelectListener(this);
            if (targetArtist == null) return;
            if (IsProcessing)
            {
                initListener = this;
            }
            else
            {
                SelectArtist(targetArtist);
            }
        }

        public void SelectArtist(object targetArtist)
        {
            ArtistView artist = null;
            if (targetArtist is string artistName)
            {
                artist = FindAndLoadArtist(artistName);
            }
            else if (targetArtist is PlaylistView playlist)
            {
                artist = Artists.FirstOrDefault(a => a.Name == playlist.Name);
                if (artist.NotLoaded)
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

        private async void Setup(IEnumerable<Music> songs)
        {
            if (IsProcessing) return;
            LoadingProgress.Visibility = Visibility.Visible; IsProcessing = true;
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
            FindMusicAndSetPlaying(MusicPlayer.CurrentMusic);
            SetHeader();
            initListener?.Inited();
            IsProcessing = false;
            LoadingProgress.Visibility = Visibility.Collapsed;
        }

        public void SetHeader()
        {
            if (Settings.settings.ShowCount)
            {
                MainPage.Instance?.SetHeaderText("AllArtistsWithCount", Artists.Count);
            }
            else
            {
                MainPage.Instance?.SetHeaderText("AllArtists");
            }
        }

        private void Artist_Tapped(object sender, TappedRoutedEventArgs e)
        {
            listViews.Clear();
            MainPage.Instance.CancelMultiSelectCommandBar();
            var artist = (sender as FrameworkElement).DataContext as ArtistView;
            LoadArtist(artist);
        }

        private void SongsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ListView listView = (ListView)sender;
            if (listView.SelectionMode != ListViewSelectionMode.None) return;
            MusicView music = (MusicView)e.ClickedItem;
            MusicPlayer.SetMusicAndPlay(listView.ItemsSource as ObservableCollection<MusicView>, music);
        }

        private void SongsListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            args.ItemContainer.Background = ColorHelper.GetRowBackground(args.ItemIndex);
        }

        private void FindMusicAndSetPlaying(Music target)
        {
            foreach (var artist in Artists)
                foreach (var album in artist.Albums)
                    foreach (var music in album.Songs)
                        music.IsPlaying = music.Equals(target);
        }

        private void ArtistMenuFlyout_Opening(object sender, object e)
        {
            MenuFlyout flyout = sender as MenuFlyout;
            var artistView = flyout.Target.DataContext as ArtistView;
            if (artistView.NotLoaded) artistView.Load();
            MenuFlyoutHelper.SetPlaylistMenu(sender, this, null, new MenuFlyoutOption() { ShowMultiSelect = true, MultiSelectOption = new MultiSelectCommandBarOption() { ShowRemove = false } });
            flyout.Items.Add(MenuFlyoutHelper.GetPreferItem(artistView));
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
            var locateArtist = new MenuFlyoutItem()
            {
                Text = Helper.Localize("LocateArtist"),
                Icon = new SymbolIcon(Symbol.Target)
            };
            locateArtist.Click += (s, args) =>
            {
                ScrollToArtist(artistView.Name);
            };
            flyout.Items.Add(locateArtist);
        }
        private void AlbumMenuFlyout_Opening(object sender, object e)
        {
            MenuFlyoutHelper.SetPlaylistMenu(sender, this, null, new MenuFlyoutOption
            { 
                ShowSelect = true,
                MultiSelectOption = new MultiSelectCommandBarOption() { ShowRemove = false } 
            });
            MenuFlyout flyout = sender as MenuFlyout;
            var album = flyout.Target.DataContext as AlbumView;
            flyout.Items.Add(MenuFlyoutHelper.GetPreferItem(album));
            flyout.Items.Add(MenuFlyoutHelper.GetSeeAlbumFlyout(album.Songs[0].FromVO()));
        }
        private void OpenMusicMenuFlyout(object sender, object e)
        {
            MenuFlyoutHelper.SetMusicMenu(sender, this, null, new MenuFlyoutOption
            {
                ShowSelect = true,
                MultiSelectOption = new MultiSelectCommandBarOption() { ShowRemove = false }
            });
        }

        private void Artist_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            ((sender as StackPanel).Children[1] as ScrollingTextBlock).StartScrolling();
        }

        private void ArtistSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            Suggestions.SetTo(SearchHelper.SearchAndSortByDefault(SuggestionList.Select(i => new Artist() { Name = i }), sender.Text)
                                          .Select(i => i.Name));
        }

        private void ArtistSearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            ChooseSuggestion((string)args.SelectedItem);
        }

        private void ChooseSuggestion(string artist)
        {
            listViews.Clear();
            MainPage.Instance.CancelMultiSelectCommandBar();
            ArtistSearchBox.Text = artist;
            ArtistMasterDetailsView.SelectedItem = FindAndLoadArtist(artist);
            ScrollToArtist(artist);
        }

        private ArtistView FindAndLoadArtist(string artistName)
        {
            var artist = Artists.First(a => a.Name == artistName);
            LoadArtist(artist);
            return artist;
        }

        private async void LoadArtist(ArtistView artist)
        {
            if (artist.NotLoaded)
                await artist.LoadAsync();
        }

        private void ArtistSearchBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter && Suggestions.Count > 0)
            {
                ChooseSuggestion(Suggestions[0]);
            }
        }

        private async void AlbumCover_EffectiveViewportChanged(FrameworkElement sender, EffectiveViewportChangedEventArgs args)
        {
            if (ImageHelper.NeedsLoading(sender, args))
            {
                await (sender.DataContext as AlbumView)?.SetThumbnailAsync();
            }
        }
        void IMultiSelectListener.Execute(MultiSelectCommandBar commandBar, MultiSelectEventArgs args)
        {
            switch (args.Event)
            {
                case MultiSelectEvent.Cancel:
                    foreach (var listView in listViews)
                    {
                        listView.SelectionMode = ListViewSelectionMode.None;
                    }
                    break;
                case MultiSelectEvent.AddTo:
                    args.FlyoutHelper.Data = SelectedItems;
                    args.FlyoutHelper.DefaultPlaylistName = SelectedArtist.Name;
                    break;
                case MultiSelectEvent.Play:
                    List<MusicView> selectedItems = SelectedItems;
                    if (selectedItems.Count == 0) return;
                    MusicPlayer.SetMusicAndPlay(SelectedItems);
                    break;
                case MultiSelectEvent.SelectAll:
                    foreach (var listView in listViews)
                    {
                        try
                        {
                            listView.SelectAll();
                        }
                        catch (Exception)
                        {
                            // NotSupportException 没有加载完成导致部分ListView.SelectionMode还是None
                        }
                    }
                    Helper.GetMainPageContainer()?.GetMultiSelectCommandBar().CountSelections(SelectedItems.Count);
                    break;
                case MultiSelectEvent.ReverseSelections:
                    foreach (var listView in listViews)
                    {
                        try
                        {
                            listView.ReverseSelections();
                        }
                        catch (Exception)
                        {

                        }
                    }
                    Helper.GetMainPageContainer()?.GetMultiSelectCommandBar().CountSelections(SelectedItems.Count);
                    break;
                case MultiSelectEvent.ClearSelections:
                    foreach (var listView in listViews)
                    {
                        try
                        {
                            listView.ClearSelections();
                        }
                        catch (Exception e)
                        {
                            Log.Warn($"ArtistPage ClearSelection failed {e}");
                        }
                    }
                    break;
            }
        }
        void IMenuFlyoutItemClickListener.Execute(MenuFlyoutEventArgs args)
        {
            if (args.Event != MenuFlyoutEvent.Select)
            {
                return;
            }
            if (args.Data is MusicView music)
            {
                foreach (var listView in listViews)
                {
                    listView.SelectionMode = ListViewSelectionMode.Multiple;
                    if (listView.DataContext is AlbumView album && album.Contains(music))
                    {
                        listView.SelectedItems.Add(music);
                    }
                }
            }
            else
            {
                foreach (var listView in listViews)
                {
                    listView.SelectionMode = ListViewSelectionMode.Multiple;
                    if (listView.DataContext.Equals(args.Data))
                    {
                        listView.SelectAll();
                    }
                }
            }
        }

        private void SongsListView_Loaded(object sender, RoutedEventArgs e)
        {
            ListView listView = sender as ListView;
            listView.SelectionMode = MainPage.Instance.GetMultiSelectCommandBar().IsVisible ? ListViewSelectionMode.Multiple : ListViewSelectionMode.None;
            listViews.Add(listView);
        }

        void IInitListener.Inited()
        {
            SelectArtist(targetArtist);
        }

        async void IMusicEventListener.Execute(Music music, MusicEventArgs args)
        {
            if (IsFolderUpdated) return;
            ArtistView artist = Artists.FirstOrDefault(a => a.Name == music.Artist);
            await Helper.RunInMainUIThread(Dispatcher, () =>
            {
                try
                {
                    switch (args.EventType)
                    {
                        case MusicEventType.Add:
                            if (artist != null)
                            {
                                artist.AddMusic(music.ToVO());
                            }
                            else
                            {
                                artist = new ArtistView(music.ToVO());
                                Artists.InsertWithOrder(artist);
                            }
                            break;
                        case MusicEventType.Remove:
                            if (artist != null)
                            {
                                artist.RemoveMusic(music.ToVO());
                                if (artist.Songs.IsEmpty())
                                {
                                    Artists.Remove(artist);
                                }
                            }
                            break;
                        case MusicEventType.Modify:
                            Music before = music, after = args.ModifiedMusic;
                            if (artist.Equals(ArtistMasterDetailsView.SelectedItem) || !artist.NotLoaded) artist.Load();
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
                                int index = SuggestionList.FindSortedListInsertIndex(after.Artist);
                                SuggestionList.Insert(index, after.Artist);
                                Artists.Insert(index, new ArtistView(after.ToVO()));
                            }
                            if (before.Artist != after.Artist && artist.Songs.IsEmpty())
                            {
                                SuggestionList.Remove(artist.Name);
                                Artists.Remove(artist);
                            }
                            break;
                    }
                } 
                catch (Exception e)
                {
                    Log.Warn($"ArtistsPage IMusicEventListener.Execute failed {e}");
                }
            });
        }

        private void SongsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListViewBase listViewBase = (sender as ListViewBase);
            if (listViewBase.SelectionMode == ListViewSelectionMode.Multiple)
            {
                Helper.GetMainPageContainer()?.GetMultiSelectCommandBar().CountSelections(listViewBase.SelectedItems.Count);
            }
        }

        async void IMusicPlayerEventListener.Execute(MusicPlayerEventArgs args)
        {
            switch (args.EventType)
            {
                case MusicPlayerEventType.Switch:
                    await Helper.RunInMainUIThread(Dispatcher, () => FindMusicAndSetPlaying(args.Music));
                    break;
            }
        }

        void IStorageItemEventListener.ExecuteFileEvent(FolderFile file, StorageItemEventArgs args)
        {
        }

        void IStorageItemEventListener.ExecuteFolderEvent(FolderTree folder, StorageItemEventArgs args)
        {
            if (args.EventType == StorageItemEventType.BeforeReset)
            {
                IsFolderUpdated = true;
                Artists.Clear();
            }
        }
    }
}

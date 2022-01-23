using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.UI.Xaml.Controls;
using SMPlayer.Dialogs;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
    public sealed partial class PlaylistsPage : Page, IPlaylistEventListener
    {
        public static PlaylistsPage Instance { get => MainPage.Instance.NavigationFrame.Content as PlaylistsPage; }
        private ObservableCollection<Playlist> Playlists = new ObservableCollection<Playlist>();
        public Playlist CurrentPlaylist { get => PlaylistTabView.SelectedItem as Playlist; }
        private Playlist PreviousPlaylist;
        private HeaderedPlaylistControl PlaylistController;
        private RenameDialog dialog;
        public PlaylistsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            Playlists.SetTo(Settings.AllPlaylists);
            SelectPlaylist(Settings.settings.LastPlaylistId);
            Settings.AddPlaylistEventListener(this);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is Playlist playlist)
            {
                SelectPlaylist(playlist.Id);
            }
            else if (e.Parameter is string playlistName)
            {
                SelectPlaylist(playlistName);
            }
            else if (e.Parameter is AlbumView albumView)
            {
                SelectPlaylist(albumView.Name);
            }
            else if (e.Parameter is long id)
            {
                SelectPlaylist(id);
            }
        }

        private Playlist SelectPlaylistById(long id)
        {
            return Playlists.FirstOrDefault(p => p.Id == id);
        }

        private void SelectPlaylist(long id)
        {
            Playlist selected = SelectPlaylistById(id);
            PlaylistTabView.SelectedItem = selected;
        }

        private void SelectPlaylist(string target)
        {
            Playlist selected = Playlists.FirstOrDefault(p => p.Name == target);
            PlaylistTabView.SelectedItem = selected;
        }

        private async void LoadPlaylistSongs(Playlist playlist)
        {
            if (playlist == null) return;
            playlist.Songs.SetTo(Settings.FindPlaylistItems(playlist.Id));
            playlist.Sort();
            foreach (var music in playlist.Songs)
                music.IsPlaying = music.Equals(MusicPlayer.CurrentMusic);
            if (PlaylistController != null)
                await PlaylistController.SetPlaylist(playlist);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            BringSelectedTabIntoView();
            LoadPlaylistSongs(CurrentPlaylist);
        }

        private void PlaylistTabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tabview = sender as Microsoft.Toolkit.Uwp.UI.Controls.TabView;
            if (tabview.SelectedIndex == -1)
            {
                // If CurrentTab is deleted
                if (Playlists.IsNotEmpty()) tabview.SelectedIndex = Playlists.Count - 1;
                return;
            }
            var playlist = tabview.SelectedItem as Playlist;
            Settings.settings.LastPlaylistId = playlist.Id;
            LoadPlaylistSongs(playlist);
        }

        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            var playlist = (sender as MenuFlyoutItem).DataContext as Playlist;
            RemovePlaylist(playlist);
        }

        private async void RenameClick(object sender, RoutedEventArgs e)
        {
            var flyoutItem = sender as MenuFlyoutItem;
            var playlist = flyoutItem.DataContext as Playlist;
            dialog = new RenameDialog(RenameOption.Rename, RenameTarget.Playlist, playlist.Name)
            {
                Validate = Settings.settings.ValidatePlaylistName,
                Confirmed = (newName) => Settings.settings.RenamePlaylist(playlist, newName)
            };
            await dialog.ShowAsync();
        }

        private void DuplicateClick(object sender, RoutedEventArgs e)
        {
            var target = (sender as MenuFlyoutItem).DataContext as Playlist;
            int next = Settings.settings.FindNextPlaylistNameIndex(target.Name);
            string name = Helper.GetNextName(target.Name, next);
            var duplicate = target.Duplicate(name);
            Settings.settings.AddPlaylist(duplicate);
            Playlists.Add(duplicate);
            PlaylistTabView.SelectedItem = duplicate;
        }

        private void PlaylistTabView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            ObservableCollection<Playlist> playlists = PlaylistTabView.ItemsSource as ObservableCollection<Playlist>;
            for (int i = 0; i < playlists.Count; i++)
                playlists[i].Priority = i;
            Settings.settings.UpdatePlaylists(playlists);
        }

        private void PlaylistTabView_TabClosing(object sender, TabClosingEventArgs e)
        {
            var playlist = (Playlist)e.Item;
            e.Cancel = true;
            RemovePlaylist(playlist);
        }

        public void RemovePlaylist(Playlist playlist)
        {
            PlaylistController.DeletePlaylist(playlist);
        }

        private async void CreateNewPlaylist()
        {
            string name = Helper.Localize("Playlist");
            dialog = new RenameDialog(RenameOption.Create, RenameTarget.Playlist, Settings.settings.FindNextPlaylistName(name))
            {
                Validate = Settings.settings.ValidatePlaylistName,
                Confirmed = (newName) => Settings.settings.AddPlaylist(newName)
            };
            await dialog.ShowAsync();
        }

        private void OpenPlaylistsFlyout(object sender, object e)
        {
            SpinArrowAnimation.Begin();
            var flyout = sender as MenuFlyout;
            flyout.Items.Clear();
            var createNewPlaylist = new MenuFlyoutItem()
            {
                Text = Helper.LocalizeText("CreateNewPlaylist")
            };
            createNewPlaylist.Click += (s, args) =>
            {
                CreateNewPlaylist();
            };
            flyout.Items.Add(createNewPlaylist);
            flyout.Items.Add(new MenuFlyoutSeparator());
            foreach (var playlist in Playlists)
            {
                var item = new ToggleMenuFlyoutItem()
                {
                    Text = playlist.Name,
                    IsChecked = playlist.Equals(PlaylistTabView.SelectedItem)
                };
                item.Click += (s, args) =>
                {
                    PlaylistTabView.SelectedItem = playlist;
                    BringSelectedTabIntoView();
                };
                flyout.Items.Add(item);
            }
        }

        private void BringSelectedTabIntoView()
        {
            if (PlaylistTabView.SelectedIndex < 0) return;
            if (PlaylistTabView.GetFirstDescendantOfType<ScrollViewer>() is ScrollViewer scrollViewer)
            {
                if (scrollViewer.IsLoaded)
                {
                    double itemWidth = scrollViewer.ExtentWidth / PlaylistTabView.Items.Count;
                    scrollViewer.ChangeView(itemWidth * PlaylistTabView.SelectedIndex, null, null, false);
                }
                else
                {
                    scrollViewer.Loaded += (sender, args) =>
                    {
                        double itemWidth = scrollViewer.ActualWidth / PlaylistTabView.Items.Count;
                        scrollViewer.ChangeView(itemWidth * PlaylistTabView.SelectedIndex, null, null, false);
                    };
                }
            }
        }

        private void ClosePlaylistsFlyout(object sender, object e)
        {
            SpinArrowAnimation.Begin();
        }

        private async void HeaderedPlaylistControl_Loaded(object sender, RoutedEventArgs e)
        {
            PlaylistController = sender as HeaderedPlaylistControl;
            await PlaylistController.SetPlaylist(PlaylistTabView.SelectedItem as Playlist);
        }

        private void TabHeader_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (CurrentPlaylist.Equals(PreviousPlaylist))
            {
                PlaylistController.ScrollToTop();
            }
            PreviousPlaylist = CurrentPlaylist;
        }

        private void CreatePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            CreateNewPlaylist();
        }

        void IPlaylistEventListener.Added(Playlist playlist)
        {
            Playlists.Add(playlist);
            PlaylistTabView.SelectedItem = playlist;
            BringSelectedTabIntoView();
        }

        void IPlaylistEventListener.Renamed(Playlist playlist)
        {
            SelectPlaylistById(playlist.Id)?.CopyFrom(playlist);
        }

        void IPlaylistEventListener.Removed(Playlist playlist)
        {
            Playlists.Remove(playlist);
        }

        void IPlaylistEventListener.Sorted(Playlist playlist, SortBy criterion)
        {
            LoadPlaylistSongs(playlist);
        }
    }
}

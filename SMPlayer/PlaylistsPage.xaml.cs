using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.UI.Xaml.Controls;
using SMPlayer.Dialogs;
using SMPlayer.Models;
using SMPlayer.Services;
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
        private ObservableCollection<PlaylistView> Playlists = new ObservableCollection<PlaylistView>();
        public PlaylistView CurrentPlaylist { get => PlaylistTabView.SelectedItem as PlaylistView; }
        private PlaylistView PreviousPlaylist;
        private HeaderedPlaylistControl PlaylistController;
        private RenameDialog dialog;
        public PlaylistsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            Playlists.SetTo(PlaylistService.AllPlaylistViews);
            SelectPlaylist(Settings.settings.LastPlaylistId);
            Settings.AddPlaylistEventListener(this);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            NavigateToPlaylist(e.Parameter);
        }

        public void NavigateToPlaylist(object parameter)
        {
            if (parameter is PlaylistView playlist)
            {
                SelectPlaylist(playlist.Id);
            }
            else if (parameter is string playlistName)
            {
                SelectPlaylist(playlistName);
            }
            else if (parameter is AlbumView albumView)
            {
                SelectPlaylist(albumView.Name);
            }
            else if (parameter is long id)
            {
                SelectPlaylist(id);
            }
        }

        private PlaylistView SelectPlaylistById(long id)
        {
            return Playlists.FirstOrDefault(p => p.Id == id);
        }

        private void SelectPlaylist(long id)
        {
            PlaylistView selected = SelectPlaylistById(id);
            PlaylistTabView.SelectedItem = selected;
        }

        private void SelectPlaylist(string target)
        {
            PlaylistView selected = Playlists.FirstOrDefault(p => p.Name == target);
            PlaylistTabView.SelectedItem = selected;
        }

        private async void LoadPlaylistSongs(PlaylistView playlist)
        {
            if (playlist == null) return;
            playlist.Songs.SetTo(PlaylistService.FindPlaylistItemViews(playlist.Id));
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
            var playlist = tabview.SelectedItem as PlaylistView;
            Settings.settings.LastPlaylistId = playlist.Id;
            LoadPlaylistSongs(playlist);
        }

        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            var playlist = (sender as MenuFlyoutItem).DataContext as PlaylistView;
            RemovePlaylist(playlist);
        }

        private async void RenameClick(object sender, RoutedEventArgs e)
        {
            var flyoutItem = sender as MenuFlyoutItem;
            var playlist = flyoutItem.DataContext as PlaylistView;
            dialog = new RenameDialog(RenameOption.Rename, RenameTarget.Playlist, playlist.Name)
            {
                Validate = Settings.settings.ValidatePlaylistName,
                Confirmed = (newName) => Settings.settings.RenamePlaylist(playlist, newName)
            };
            await dialog.ShowAsync();
        }

        private void DuplicateClick(object sender, RoutedEventArgs e)
        {
            var target = (sender as MenuFlyoutItem).DataContext as PlaylistView;
            int next = Settings.settings.FindNextPlaylistNameIndex(target.Name);
            string name = Helper.GetNextName(target.Name, next);
            var duplicate = target.Duplicate(name);
            Settings.settings.AddPlaylist(duplicate);
            Playlists.Add(duplicate);
            PlaylistTabView.SelectedItem = duplicate;
        }

        private void PlaylistTabView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            ObservableCollection<PlaylistView> playlists = PlaylistTabView.ItemsSource as ObservableCollection<PlaylistView>;
            for (int i = 0; i < playlists.Count; i++)
                playlists[i].Priority = i;
            Settings.settings.UpdatePlaylists(playlists);
        }

        private void PlaylistTabView_TabClosing(object sender, TabClosingEventArgs e)
        {
            var playlist = (PlaylistView)e.Item;
            e.Cancel = true;
            RemovePlaylist(playlist);
        }

        public void RemovePlaylist(PlaylistView playlist)
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
            await PlaylistController.SetPlaylist(PlaylistTabView.SelectedItem as PlaylistView);
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

        void IPlaylistEventListener.Added(PlaylistView playlist)
        {
            Playlists.Add(playlist);
            PlaylistTabView.SelectedItem = playlist;
            BringSelectedTabIntoView();
        }

        void IPlaylistEventListener.Renamed(PlaylistView playlist)
        {
            SelectPlaylistById(playlist.Id)?.CopyFrom(playlist);
        }

        void IPlaylistEventListener.Removed(PlaylistView playlist)
        {
            Playlists.Remove(playlist);
        }

        void IPlaylistEventListener.Sorted(PlaylistView playlist, SortBy criterion)
        {
            LoadPlaylistSongs(playlist);
        }
    }
}

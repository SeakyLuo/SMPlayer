using Microsoft.Toolkit.Uwp.UI.Controls;
using SMPlayer.Dialogs;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public sealed partial class PlaylistsPage : Page, IRenameActionListener
    {
        public static ObservableCollection<Playlist> Playlists = new ObservableCollection<Playlist>();

        private HeaderedPlaylistControl PlaylistController;
        private RenameDialog dialog;
        public PlaylistsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            Playlists = new ObservableCollection<Playlist>(Settings.settings.Playlists);
            PlaylistTabView.ItemsSource = Playlists;
            SelectPlaylist(Settings.settings.LastPlaylist);
            Settings.PlaylistAddedListeners.Add(playlist => PlaylistTabView.SelectedItem = playlist);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is Playlist playlist)
            {
                SelectPlaylist(playlist.Name);
            }
            else if (e.Parameter is string playlistName)
            {
                SelectPlaylist(playlistName);
            }
            else if (e.Parameter is AlbumView albumView)
            {
                SelectPlaylist(albumView.Name);
            }
        }
        private void SelectPlaylist(string target)
        {
            PlaylistTabView.SelectedItem = Playlists.FirstOrDefault(p => p.Name == target);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            BringSelectedTabIntoView();
        }

        private async void PlaylistTabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tabview = sender as TabView;
            if (tabview.SelectedIndex == -1)
            {
                // If CurrentTab is deleted
                if (Playlists.Count > 0) tabview.SelectedIndex = Playlists.Count - 1;
                return;
            }
            var playlist = tabview.SelectedItem as Playlist;
            Settings.settings.LastPlaylist = playlist.Name;
            foreach (var music in playlist.Songs)
                music.IsPlaying = music.Equals(MediaHelper.CurrentMusic);
            if (PlaylistController != null)
                await PlaylistController.SetPlaylist(playlist);
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
            dialog = new RenameDialog(this, RenameOption.Rename, playlist.Name);
            await dialog.ShowAsync();
        }

        private void DuplicateClick(object sender, RoutedEventArgs e)
        {
            var target = (sender as MenuFlyoutItem).DataContext as Playlist;
            int next = Settings.settings.FindNextPlaylistNameIndex(target.Name);
            string name = Helper.GetPlaylistName(target.Name, next), prev = next == 1 ? target.Name : Helper.GetPlaylistName(target.Name, next - 1);
            var duplicate = target.Duplicate(name);
            int index = Settings.settings.Playlists.FindLastIndex(p => p.Name.StartsWith(prev)) + 1;
            Playlists.Insert(index, duplicate);
            Settings.settings.Playlists.Insert(index, duplicate);
            PlaylistTabView.SelectedIndex = index;
        }

        private async void NewPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            string name = Helper.Localize("Playlist");
            dialog = new RenameDialog(this, RenameOption.New, Settings.settings.FindNextPlaylistName(name));
            await dialog.ShowAsync();
        }

        private void PlaylistTabView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            Settings.settings.Playlists = (PlaylistTabView.ItemsSource as ObservableCollection<Playlist>).ToList();
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

        public bool Confirm(string oldName, string newName)
        {
            return ConfirmRenaming(dialog, oldName, newName);
        }
        public static bool ConfirmRenaming(RenameDialog dialog, string oldName, string newName)
        {
            return new VirtualRenameActionListener()
            {
                Dialog = dialog
            }.Confirm(oldName, newName);
        }

        private void OpenPlaylistsFlyout(object sender, object e)
        {
            SpinArrowAnimation.Begin();
            var flyout = sender as MenuFlyout;
            flyout.Items.Clear();
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
                    scrollViewer.Loaded += (sender, args) =>
                    {
                        double itemWidth = scrollViewer.ActualWidth / PlaylistTabView.Items.Count;
                        scrollViewer.ChangeView(itemWidth * PlaylistTabView.SelectedIndex, null, null, false);
                    };
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
    }
}

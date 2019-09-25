using Microsoft.Toolkit.Uwp.UI.Controls;
using SMPlayer.Dialogs;
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
using Windows.Media.Playback;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class PlaylistsPage : Page, RenameActionListener, PlaylistScrollListener, RemoveMusicListener
    {
        public static ObservableCollection<Playlist> Playlists = new ObservableCollection<Playlist>();
        private ObservableCollection<Playlist> playlists
        {
            get => Playlists;
            set => Playlists = value;
        }
        private Dialogs.RenameDialog dialog;
        private HeaderedPlaylistControl playlistControl;
        public PlaylistsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            Playlists = new ObservableCollection<Playlist>(Settings.settings.Playlists);
            SetFooterText();
            Playlists.CollectionChanged += (sender, e) => SetFooterText();
            PlaylistTabView.SelectedIndex = Settings.settings.Playlists.FindIndex((p) => p.Name == Settings.settings.LastPlaylist);
        }
        private static RemoveDialog DeleteDialog;

        public void SetFooterText()
        {
            if (Playlists.Count == 0)
            {
                ShowAllPlaylistButton.Label = "No Playlists";
                SortByButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                ShowAllPlaylistButton.Label = $"Playlists: {Playlists.Count}";
                SortByButton.Visibility = Visibility.Visible;
            }
        }

        private async void PlaylistTabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tabview = sender as TabView;
            if (tabview.SelectedIndex == -1)
            {
                // If CurrentTab is deleted
                if (playlists.Count > 0) tabview.SelectedIndex = playlists.Count - 1;
                return;
            }
            var playlist = tabview.SelectedItem as Playlist;
            Settings.settings.LastPlaylist = playlist.Name;
            foreach (var music in playlist.Songs)
                music.IsPlaying = music.Equals(MediaHelper.CurrentMusic);
            SortByButton.Label = "Sort By " + playlist.Criterion.ToStr();
            if (playlistControl != null)
                await playlistControl.SetMusicCollection(playlist);
        }

        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            var playlist = (sender as MenuFlyoutItem).DataContext as Playlist;
            DeletePlaylist(playlist);
        }

        private async void RenameClick(object sender, RoutedEventArgs e)
        {
            var flyoutItem = sender as MenuFlyoutItem;
            var playlist = flyoutItem.DataContext as Playlist;
            dialog = new Dialogs.RenameDialog(this as RenameActionListener, TitleOption.Rename, playlist.Name);
            await dialog.ShowAsync();
        }

        private void DuplicateClick(object sender, RoutedEventArgs e)
        {
            var target = (sender as MenuFlyoutItem).DataContext as Playlist;
            int next = Settings.settings.FindNextPlaylistNameIndex(target.Name);
            string name = $"{target.Name} {next}", prev = next == 1 ? target.Name : $"{target.Name} {next - 1}";
            var duplicate = target.Duplicate(name);
            int index = Settings.settings.Playlists.FindLastIndex((p) => p.Name.StartsWith(prev)) + 1;
            Playlists.Insert(index, duplicate);
            Settings.settings.Playlists.Insert(index, duplicate);
        }

        private async void NewPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            string name = "Playlist";
            dialog = new Dialogs.RenameDialog(this as RenameActionListener, TitleOption.NewPlaylist, Settings.settings.FindNextPlaylistName(name));
            await dialog.ShowAsync();
        }

        private void EditPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            if (EditPlaylistButton.Content.ToString() == "\uE70F")
            {
                EditPlaylistButton.Content = "\uE73E";
                PlaylistTabView.CanCloseTabs = true;
            }
            else
            {
                EditPlaylistButton.Content = "\uE70F";
                PlaylistTabView.CanCloseTabs = false;
            }
        }

        private void PlaylistTabView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            Settings.settings.Playlists = (PlaylistTabView.ItemsSource as ObservableCollection<Playlist>).ToList();
        }

        private void PlaylistTabView_TabClosing(object sender, TabClosingEventArgs e)
        {
            var playlist = (Playlist)e.Item;
            e.Cancel = true;
            DeletePlaylist(playlist);
        }

        public static async void DeletePlaylist(Playlist playlist)
        {
            if (DeleteDialog == null)
            {
                DeleteDialog = new RemoveDialog()
                {
                    Confirm = new UICommand("Confirm", new UICommandInvokedHandler((command) => RemovePlaylist(playlist)))
                };
            }
            if (DeleteDialog.IsChecked)
            {
                RemovePlaylist(playlist);
            }
            else
            {
                DeleteDialog.Message = $"Do you want to remove playlist {playlist.Name}?";
                await DeleteDialog.ShowAsync();
            }
        }

        private static void RemovePlaylist(Playlist playlist)
        {
            Playlists.Remove(playlist);
            Settings.settings.Playlists.Remove(playlist);
        }

        public bool Confirm(string OldName, string NewName)
        {
            return ConfirmRenaming(dialog, OldName, NewName);
        }
        public static bool ConfirmRenaming(Dialogs.RenameDialog dialog, string OldName, string NewName)
        {
            if (string.IsNullOrEmpty(NewName) || string.IsNullOrWhiteSpace(NewName))
            {
                dialog.ShowError(ErrorOption.EmptyOrWhiteSpace);
                return false;
            }
            if (Settings.settings.Playlists.FindIndex((p) => p.Name == NewName) != -1)
            {
                dialog.ShowError(ErrorOption.Used);
                return false;
            }
            switch (dialog.Option)
            {
                case TitleOption.NewPlaylist:
                    Playlist playlist = new Playlist(NewName);
                    PlaylistsPage.Playlists.Add(playlist);
                    Settings.settings.Playlists.Add(playlist);
                    break;
                case TitleOption.Rename:
                    if (OldName != NewName)
                    {
                        int index = Settings.settings.Playlists.FindIndex((p) => p.Name == OldName);
                        Settings.settings.Playlists[index].Name = NewName;
                        PlaylistsPage.Playlists[index].Name = NewName;
                    }
                    break;
            }
            return true;
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
                };
                flyout.Items.Add(item);
            }
        }

        private void ClosePlaylistsFlyout(object sender, object e)
        {
            SpinArrowAnimation.Begin();
        }

        private void SortByButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var playlist = PlaylistTabView.SelectedItem as Playlist;
            var flyout = new MenuFlyout();
            var reverseItem = new MenuFlyoutItem() { Text = "Reverse Playlist" };
            reverseItem.Click += (send, args) => playlist.Reverse();
            flyout.Items.Add(reverseItem);
            flyout.Items.Add(new MenuFlyoutSeparator());
            foreach (var criterion in Playlist.Criteria)
            {
                string sortby = "Sort By " + criterion.ToStr();
                var radioItem = new ToggleMenuFlyoutItem()
                {
                    Text = sortby,
                    IsChecked = playlist.Criterion == criterion
                };
                radioItem.Click += (send, args) =>
                {
                    playlist.SetCriterionAndSort(criterion);
                    (sender as IconTextButton).Label = sortby;
                };
                flyout.Items.Add(radioItem);
            }
        }

        private async void HeaderedPlaylistControl_Loaded(object sender, RoutedEventArgs e)
        {
            playlistControl = sender as HeaderedPlaylistControl;
            playlistControl.Playlist.ScrollListener = this as PlaylistScrollListener;
            playlistControl.Playlist.RemoveListeners.Add(this as RemoveMusicListener);
            await playlistControl.SetMusicCollection(PlaylistTabView.SelectedItem as Playlist);
        }

        private ScrollDirection direction;

        public void Scrolled(double before, double after)
        {
            if (after > before + 3)
            {
                // scroll down
                if (direction != ScrollDirection.Down)
                {
                    direction = ScrollDirection.Down;
                    ShowFooterAnimation.Begin();
                }
            }
            else if (after < before - 3)
            {
                // scroll up
                if (direction != ScrollDirection.Up)
                {
                    direction = ScrollDirection.Up;
                    HideFooterAnimation.Begin();
                }
            }
            else
            {
                direction = ScrollDirection.None;
            }
        }

        public void MusicRemoved(int index, Music music)
        {
            (PlaylistTabView.SelectedItem as Playlist).Songs.RemoveAt(index);
            //(PlaylistTabView.SelectedItem as Playlist).Songs.Remove(music);
        }
    }
}

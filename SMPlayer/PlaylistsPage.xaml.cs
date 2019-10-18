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
        private RenameDialog dialog;
        private static RemoveDialog DeleteDialog;
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is Playlist playlist)
            {
                PlaylistTabView.SelectedItem = playlist;
            }
            else if (e.Parameter is string playlistName)
            {
                PlaylistTabView.SelectedItem = Playlists.FirstOrDefault((p) => p.Name == playlistName);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            EditPlaylistButton.SetToolTip("Edit Playlists");
        }

        public void SetFooterText()
        {
            if (Playlists.Count == 0)
            {
                ShowAllPlaylistButton.Label = Helper.Localize("No Playlist");
                SortByButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                ShowAllPlaylistButton.Label = $"{Helper.Localize("Playlists:")} {Playlists.Count}";
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
            SortByButton.Label = Helper.Localize("Sort By " + playlist.Criterion.ToStr());
            if (playlistControl != null)
                await playlistControl.SetPlaylist(playlist);
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
            dialog = new RenameDialog(this, RenameOption.Rename, playlist.Name);
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
            string name = Helper.Localize("Playlist");
            dialog = new RenameDialog(this, RenameOption.New, Settings.settings.FindNextPlaylistName(name));
            await dialog.ShowAsync();
        }

        private void EditPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlaylistTabView.CanCloseTabs)
            {
                EditPlaylistButton.Content = "\uE70F";
                EditPlaylistButton.SetToolTip("Edit Playlists");
                PlaylistTabView.CanCloseTabs = false;
            }
            else
            {
                EditPlaylistButton.Content = "\uE73E";
                EditPlaylistButton.SetToolTip("Finish Editing");
                PlaylistTabView.CanCloseTabs = true;
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
                    Confirm = () => RemovePlaylist(playlist)
                };
            }
            if (DeleteDialog.IsChecked)
            {
                RemovePlaylist(playlist);
            }
            else
            {
                DeleteDialog.Message = string.Format(Helper.LocalizeMessage("RemovePlaylist"), playlist.Name);
                await DeleteDialog.ShowAsync();
            }
        }

        private static void RemovePlaylist(Playlist playlist)
        {
            Playlists.Remove(playlist);
            Settings.settings.Playlists.Remove(playlist);
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
            MenuFlyoutHelper.SetPlaylistSortByMenu(sender, playlist);
        }

        private async void HeaderedPlaylistControl_Loaded(object sender, RoutedEventArgs e)
        {
            playlistControl = sender as HeaderedPlaylistControl;
            playlistControl.HeaderedPlaylist.ScrollListener = this;
            playlistControl.HeaderedPlaylist.RemoveListeners.Add(this);
            await playlistControl.SetPlaylist(PlaylistTabView.SelectedItem as Playlist);
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
            (PlaylistTabView.SelectedItem as Playlist).Remove(index);
        }
    }
}

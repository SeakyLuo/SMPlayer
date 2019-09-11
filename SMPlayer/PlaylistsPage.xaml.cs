using Microsoft.Toolkit.Uwp.UI.Controls;
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
    public sealed partial class PlaylistsPage : Page, RenameActionListener, MusicSwitchingListener
    {
        public static ObservableCollection<Playlist> Playlists = new ObservableCollection<Playlist>();
        private ObservableCollection<Playlist> playlists
        {
            get => Playlists;
            set => Playlists = value;
        }
        private Dictionary<string, List<BitmapImage>> PlaylistThumbnailDict = new Dictionary<string, List<BitmapImage>>();
        private Random random = new Random();
        private RenameDialog dialog;
        private Image Thumbnail;
        private Grid PlaylistInfoGrid;
        public PlaylistsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            foreach (var list in Settings.settings.Playlists)
                Playlists.Add(list);
            SetFooterText();
            Playlists.CollectionChanged += (sender, e) => SetFooterText();
            PlaylistTabView.SelectedIndex = Settings.settings.Playlists.FindIndex((p) => p.Name == Settings.settings.LastPlaylist);
        }

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

        private void PlaylistTabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tabview = sender as TabView;
            var playlist = tabview.SelectedItem as Playlist;
            Settings.settings.LastPlaylist = playlist.Name;
            foreach (var music in playlist.Songs)
                music.IsPlaying = music.Equals(MediaHelper.CurrentMusic);
            SortByButton.Label = "Sort By " + playlist.Criterion.ToStr();
            if (!IsLoaded) return;
            SetPlaylistCover(playlist);
            SetGridBackground();
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
            dialog = new RenameDialog(this as RenameActionListener, TitleOption.Rename, playlist.Name);
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
            dialog = new RenameDialog(this as RenameActionListener, TitleOption.NewPlaylist, Settings.settings.FindNextPlaylistName(name));
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

        private async void DeletePlaylist(Playlist playlist)
        {
            // Create the message dialog and set its content
            var messageDialog = new MessageDialog(string.Format("Do you want to delete playlist {0}?", playlist.Name))
            {
                Title = "Warning",
                DefaultCommandIndex = 1, // Set the command that will be invoked by default
                CancelCommandIndex = 1 // Set the command to be invoked when escape is pressed
            };

            // Add commands and set their callbacks; both buttons use the same callback function instead of inline event handlers
            messageDialog.Commands.Add(new UICommand("Yes", new UICommandInvokedHandler((command) =>
            {
                Playlists.Remove(playlist);
                Settings.settings.Playlists.Remove(playlist);
            })));
            messageDialog.Commands.Add(new UICommand("No"));

            // Show the message dialog
            await messageDialog.ShowAsync();
        }

        public bool Confirm(string OldName, string NewName)
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
                    Playlists.Add(playlist);
                    Settings.settings.Playlists.Add(playlist);
                    break;
                case TitleOption.Rename:
                    if (OldName != NewName)
                    {
                        int index = Settings.settings.Playlists.FindIndex((p) => p.Name == OldName);
                        Settings.settings.Playlists[index].Name = NewName;
                        Playlists[index].Name = NewName;
                    }
                    break;
            }
            return true;
        }

        private void Shuffle_Click(object sender, RoutedEventArgs e)
        {
            var playlist = (sender as FrameworkElement).DataContext as Playlist;
            MediaHelper.ShuffleAndPlay(playlist.Songs);
        }
        private void AddTo_Click(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            MenuFlyoutHelper.SetAddToMenu(sender, (element.DataContext as Playlist).Name).ShowAt(element);
        }
        private async void Rename_Click(object sender, RoutedEventArgs e)
        {
            var playlist = (sender as FrameworkElement).DataContext as Playlist;
            dialog = new RenameDialog(this as RenameActionListener, TitleOption.Rename, playlist.Name);
            await dialog.ShowAsync();
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var playlist = (sender as FrameworkElement).DataContext as Playlist;
            DeletePlaylist(playlist);
        }
        private void PlaylistCover_Loaded(object sender, RoutedEventArgs e)
        {
            Thumbnail = sender as Image;
            SetPlaylistCover(PlaylistTabView.SelectedItem as Playlist);
        }

        private async void SetPlaylistCover(Playlist playlist)
        {
            if (!PlaylistThumbnailDict.TryGetValue(playlist.Name, out List<BitmapImage> thumbnails))
            {
                thumbnails = await playlist.GetThumbnails();
                PlaylistThumbnailDict[playlist.Name] = thumbnails;
            }
            Thumbnail.Source = thumbnails.Count == 0 ? Helper.DefaultAlbumCover : thumbnails[random.Next(thumbnails.Count)];
            if (PlaylistInfoGrid != null) SetGridBackground();
        }

        private void PlaylistInfoGrid_Loaded(object sender, RoutedEventArgs e)
        {
            PlaylistInfoGrid = sender as Grid;
            SetGridBackground();
        }

        private async void SetGridBackground()
        {
            PlaylistInfoGrid.Background = await Helper.GetThumbnailMainColor(Thumbnail, false);
        }

        public async void MusicSwitching(Music current, Music next, MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                var playlist = PlaylistTabView.SelectedItem as Playlist;
                MediaHelper.FindMusicAndSetPlaying(playlist.Songs, current, next);
            });
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
        private void OpenSortPlaylistsFlyout(object sender, object e)
        {
            var playlist = PlaylistTabView.SelectedItem as Playlist;
            var flyout = sender as MenuFlyout;
            flyout.Items.Clear();
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
                    SortByButton.Label = sortby;
                };
                flyout.Items.Add(radioItem);
            }
        }
    }
}

using Microsoft.Toolkit.Uwp.UI.Controls;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class PlaylistsPage : Page, RenameActionListener
    {
        public static ObservableCollection<Playlist> Playlists = new ObservableCollection<Playlist>();
        private Dictionary<string, List<BitmapImage>> PlaylistThumbnailDict = new Dictionary<string, List<BitmapImage>>();
        private Random random = new Random();
        private RenameDialog dialog;
        public PlaylistsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            PlaylistTabView.ItemsSource = Playlists;
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
                PlayListTabViewFooter.Text = "No Playlists";
            }
            else
            {
                PlayListTabViewFooter.Text = string.Format("Playlists: {0}", Playlists.Count);
            }
        }

        private void PlaylistTabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.settings.LastPlaylist = e.OriginalSource == null ? Playlists[0].Name : (e.OriginalSource as Playlist).Name;
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
            string name = string.Format("{0} {1}", target.Name, next), prev = next == 1 ? target.Name : string.Format("{0} {1}", target.Name, next - 1);
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
            Settings.settings.Playlists = (List<Playlist>)PlaylistTabView.ItemsSource;
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

        private async void PlaylistInfoGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var grid = sender as Grid;
            var playlist = grid.DataContext as Playlist;
            var thumbnail = grid.Children[0] as Image;
            if (!PlaylistThumbnailDict.TryGetValue(playlist.Name, out List<BitmapImage> thumbnails))
            {
                thumbnails = await playlist.GetThumbnails();
                PlaylistThumbnailDict[playlist.Name] = thumbnails;
            }
            thumbnail.Source = thumbnails.Count == 0 ? Helper.DefaultAlbumCover : thumbnails[random.Next(thumbnails.Count)];
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
        private void Shuffle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var playlist = (sender as NavigationViewItem).DataContext as Playlist;
            MediaHelper.ShuffleAndPlay(playlist.Songs);
        }
        private void AddTo_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var playlist = (sender as NavigationViewItem).DataContext as Playlist;
            var helper = new AddToMenuFlyout();
            helper.GetMenuFlyout().ShowAt(sender as FrameworkElement);
        }
        private async void Rename_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var playlist = (sender as NavigationViewItem).DataContext as Playlist;
            dialog = new RenameDialog(this as RenameActionListener, TitleOption.Rename, playlist.Name);
            await dialog.ShowAsync();
        }
        private void Delete_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var playlist = (sender as NavigationViewItem).DataContext as Playlist;
            DeletePlaylist(playlist);
        }
        private void More_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var playlist = (sender as NavigationViewItem).DataContext as Playlist;
            MenuFlyout flyout = new MenuFlyout();
            flyout.Items.Add(new MenuFlyoutItem()
            {
                Icon = new SymbolIcon(Symbol.Pin),
                Text = "Pin to Start"
            });
            flyout.Items.Add(new MenuFlyoutSeparator());
            foreach (var criterion in Playlist.Criteria)
            {
                var radioItem = new ToggleMenuFlyoutItem()
                {
                    Text = "Sort By " + criterion.ToStr(),
                    IsChecked = playlist.Criterion == criterion
                };
                radioItem.Click += (send, args) => playlist.Criterion = criterion;
                flyout.Items.Add(radioItem);
            }
            flyout.ShowAt(sender as FrameworkElement);
        }
    }
}

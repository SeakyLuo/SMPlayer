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
    public sealed partial class PlaylistsPage : Page
    {
        private ObservableCollection<Playlist> Playlists = new ObservableCollection<Playlist>();
        private Dictionary<string, List<BitmapImage>> PlaylistThumbnailDict = new Dictionary<string, List<BitmapImage>>();
        private Random random = new Random();
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
        private void RenameClick(object sender, RoutedEventArgs e)
        {
            var flyoutItem = sender as MenuFlyoutItem;
            var tab = (TabViewItem)PlaylistTabView.ContainerFromItem(flyoutItem.DataContext);
            var header = tab.HeaderTemplate.LoadContent();
            RenameHeader(header, true, true);
        }

        private void RenameHeader(DependencyObject header, bool isEdit, bool selectAll)
        {
            var NameTextBlock = (TextBlock)VisualTreeHelper.GetChild(header, 1);
            NameTextBlock.Visibility = isEdit ? Visibility.Visible : Visibility.Collapsed;
            TextBox NameTextBox = (TextBox)VisualTreeHelper.GetChild(header, 2);
            NameTextBox.Visibility = isEdit ? Visibility.Collapsed : Visibility.Visible;
            if (selectAll) NameTextBox.SelectAll();
        }

        private void DuplicateClick(object sender, RoutedEventArgs e)
        {
            var target = (sender as MenuFlyoutItem).DataContext as Playlist;
            int next = FindNextName(target.Name);
            string name = string.Format("{0} {1}", target.Name, next), prev = next == 1 ? target.Name : string.Format("{0} {1}", target.Name, next - 1);
            var duplicate = target.Duplicate(name);
            int index = Settings.settings.Playlists.FindLastIndex((p) => p.Name.StartsWith(prev)) + 1;
            Playlists.Insert(index, duplicate);
            Settings.settings.Playlists.Insert(index, duplicate);
        }

        private void CancelNewPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            NewPlaylistFlyout.Hide();
        }

        private void CreateNewPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NewPlaylistNameTextBox.Text;
            if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
            {
                NamingErrorTextBox.Text = "Playlist name cannot be empty or whitespaces!";
                NamingErrorTextBox.Visibility = Visibility.Visible;
                return;
            }
            if (Settings.settings.Playlists.FindIndex((p) => p.Name == name) != -1)
            {
                NamingErrorTextBox.Text = "This name has been used!";
                NamingErrorTextBox.Visibility = Visibility.Visible;
                return;
            }
            Playlist playlist = new Playlist(name);
            Playlists.Add(playlist);
            Settings.settings.Playlists.Add(playlist);
            NewPlaylistFlyout.Hide();
            NewPlaylistNameTextBox.Text = "";
        }

        private void EditPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            if (EditPlaylistButton.Content.ToString() == "\uE70F")
            {
                EditPlaylistButton.Content = "\uE73E";
                PlaylistTabView.CanCloseTabs = true;

                foreach (var playlist in Playlists)
                {
                    var tab = (TabViewItem)PlaylistTabView.ContainerFromItem(playlist);
                    var header = tab.HeaderTemplate.LoadContent();
                    RenameHeader(header, true, false);
                }
            }
            else
            {
                EditPlaylistButton.Content = "\uE70F";
                PlaylistTabView.CanCloseTabs = false;

                foreach (var playlist in Playlists)
                {
                    var tab = (TabViewItem)PlaylistTabView.ContainerFromItem(playlist);
                    var header = tab.HeaderTemplate.LoadContent();
                    RenameHeader(header, false, false);
                }
            }
        }

        private void PlaylistTabView_DropCompleted(UIElement sender, DropCompletedEventArgs args)
        {

        }

        private void PlaylistTabView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {

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

        private async void ShowNamingError()
        {
            var messageDialog = new MessageDialog("This name has been used!")
            {
                Title = "Error"
            };
            messageDialog.Commands.Add(new UICommand("Ok"));
            await messageDialog.ShowAsync();
        }

        private void NameTextBox_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {

        }

        private void NewPlaylistFlyout_Closed(object sender, object e)
        {
            NamingErrorTextBox.Visibility = Visibility.Collapsed;
        }

        private int FindNextName(string Name)
        {
            var siblings = Settings.settings.Playlists.FindAll((p) => p.Name.StartsWith(Name)).Select((p) => p.Name).ToHashSet();
            for (int i = 1; i <= siblings.Count; i++)
                if (!siblings.Contains(string.Format("{0} {1}", Name, i)))
                    return i;
            return 0;
        }

        private void NewPlaylistFlyout_Opening(object sender, object e)
        {
            string name = "Playlist";
            int index = FindNextName(name);
            NewPlaylistNameTextBox.Text = index == 0 ? name : string.Format("{0} {1}", name, index);
            NewPlaylistNameTextBox.SelectAll();
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
    }
}

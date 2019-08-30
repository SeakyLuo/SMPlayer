using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
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
        public PlaylistsPage()
        {
            this.InitializeComponent();
        }

        private void DeleteClick(object sender, RoutedEventArgs e)
        {

        }

        private void DuplicateClick(object sender, RoutedEventArgs e)
        {

        }

        private void CancelNewPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            Playlists.Add(new Playlist(NewPlaylistNameTextBox.Text));
            NewPlaylistFlyout.Hide();
        }

        private void CreateaNewPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            NewPlaylistFlyout.Hide();
        }

        private void EditPlaylistButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

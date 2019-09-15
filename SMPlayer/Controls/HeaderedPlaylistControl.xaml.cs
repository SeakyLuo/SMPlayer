using SMPlayer.Models;
using System;
using System.Collections.Generic;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SMPlayer
{
    public sealed partial class HeaderedPlaylistControl : UserControl, RenameActionListener
    {
        public Playlist MusicCollection { get; set; }
        public bool IsPlaylist { get; set; }
        private static Dictionary<string, List<BitmapImage>> PlaylistThumbnailDict = new Dictionary<string, List<BitmapImage>>();
        private static readonly Random random = new Random();
        private static RenameDialog dialog;
        public HeaderedPlaylistControl()
        {
            this.InitializeComponent();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetPlaylistArt(MusicCollection);
        }

        public async void SetPlaylistArt(Playlist playlist)
        {
            if (!PlaylistThumbnailDict.TryGetValue(playlist.Name, out List<BitmapImage> PlaylistCovers))
            {
                PlaylistCovers = await playlist.GetThumbnailsAsync();
                PlaylistThumbnailDict[playlist.Name] = PlaylistCovers;
            }
            PlaylistCover.Source = PlaylistCovers.Count == 0 ? Helper.DefaultAlbumCover : PlaylistCovers[random.Next(PlaylistCovers.Count)];
            PlaylistInfoGrid.Background = playlist.Songs.Count == 0 ? ColorHelper.HighlightBrush : await Helper.GetThumbnailMainColor(PlaylistCover, false);
        }

        private async void SetPlaylistCover(Playlist playlist)
        {
            if (!PlaylistThumbnailDict.TryGetValue(playlist.Name, out List<BitmapImage> PlaylistCovers))
            {
                PlaylistCovers = await playlist.GetThumbnailsAsync();
                PlaylistThumbnailDict[playlist.Name] = PlaylistCovers;
            }
            PlaylistCover.Source = PlaylistCovers.Count == 0 ? Helper.DefaultAlbumCover : PlaylistCovers[random.Next(PlaylistCovers.Count)];
        }

        private async void SetGridBackground()
        {
            PlaylistInfoGrid.Background = MusicCollection.Songs.Count == 0 ? ColorHelper.HighlightBrush : await Helper.GetThumbnailMainColor(PlaylistCover, false);
        }

        public bool Confirm(string OldName, string NewName)
        {
            return PlaylistsPage.ConfirmRenaming(dialog, OldName, NewName);
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
            PlaylistsPage.DeletePlaylist(playlist);
        }

        public async void MusicSwitching(Music current, Music next, Windows.Media.Playback.MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                MediaHelper.FindMusicAndSetPlaying(MusicCollection.Songs, current, next);
            });
        }
    }
}

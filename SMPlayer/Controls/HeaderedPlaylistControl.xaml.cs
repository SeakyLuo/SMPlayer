using SMPlayer.Dialogs;
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
        public Brush HeaderBackground
        {
            get => headerBackground;
            set => PlaylistInfoGrid.Background = headerBackground = value;
        }
        private Brush headerBackground = ColorHelper.HighlightBrush;
        public bool IsPlaylist { get; set; }
        private static Dictionary<string, List<MusicDisplayItem>> PlaylistDisplayDict = new Dictionary<string, List<MusicDisplayItem>>();
        private static readonly Random random = new Random();
        private static RenameDialog dialog;
        public PlaylistControl Playlist { get => HeaderedPlaylist; }
        public HeaderedPlaylistControl()
        {
            this.InitializeComponent();
        }

        public async System.Threading.Tasks.Task SetMusicCollection(Playlist playlist)
        {
            MusicCollection = playlist;
            HeaderedPlaylist.ItemsSource = playlist.Songs;
            PlaylistNameTextBlock.Text = playlist.Name;
            SetPlaylistInfo(SongCountConverter.ToStr(playlist.Songs));
            ShuffleButton.IsEnabled = playlist.Songs.Count != 0;
            AddToButton.IsEnabled = playlist.Songs.Count != 0;
            RenameButton.Visibility = IsPlaylist ? Visibility.Visible : Visibility.Collapsed;
            DeleteButton.Visibility = IsPlaylist ? Visibility.Visible : Visibility.Collapsed;
            if (!PlaylistDisplayDict.TryGetValue(playlist.Name, out List<MusicDisplayItem> MusicDisplayItems))
            {
                MusicDisplayItems = await playlist.GetMusicDisplayItems();
                if (MusicDisplayItems.Count == 0)
                    MusicDisplayItems = new List<MusicDisplayItem>() { new MusicDisplayItem(Helper.DefaultAlbumCover, ColorHelper.HighlightBrush) };
                PlaylistDisplayDict[playlist.Name] = MusicDisplayItems;
            }
            var item = MusicDisplayItems[random.Next(MusicDisplayItems.Count)];
            PlaylistCover.Source = item.Thumbnail;
            HeaderBackground = item.Color;
        }

        public void SetPlaylistInfo(string info)
        {
            if (string.IsNullOrEmpty(PlaylistInfoTextBlock.Text))
                PlaylistInfoTextBlock.Text = info;
        }

        public bool Confirm(string OldName, string NewName)
        {
            return PlaylistsPage.ConfirmRenaming(dialog, OldName, NewName);
        }

        private void Shuffle_Click(object sender, RoutedEventArgs e)
        {
            MediaHelper.ShuffleAndPlay(MusicCollection.Songs);
        }
        private void AddTo_Click(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            MenuFlyoutHelper.SetAddToMenu(sender, MusicCollection.Name).ShowAt(element);
        }
        private async void Rename_Click(object sender, RoutedEventArgs e)
        {
            dialog = new RenameDialog(this as RenameActionListener, TitleOption.Rename, MusicCollection.Name);
            await dialog.ShowAsync();
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            PlaylistsPage.DeletePlaylist(MusicCollection);
        }

        private void PinToStart_Click(object sender, RoutedEventArgs e)
        {

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

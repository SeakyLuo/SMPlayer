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

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SMPlayer
{
    public sealed partial class PlaylistControl : UserControl, MediaControlListener
    {
        public ObservableCollection<Music> Songs { get; set; }
        public PlaylistControl()
        {
            this.InitializeComponent();
        }
        private void SongsListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (RequestedTheme != ElementTheme.Dark)
                args.ItemContainer.Background = args.ItemIndex % 2 == 0 ? Helper.WhiteSmokeBrush : Helper.WhiteBrush;
        }

        private void SongsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Music music = (Music)e.ClickedItem;
            MainPage.Instance.SetMusicAndPlay(music);
        }
        public void Tick() { return; }

        private void FindMusicAndSetPlaying(Music target, bool isPlaying)
        {
            var music = Songs.FirstOrDefault((m) => m.Equals(target));
            if (music != null) music.IsPlaying = isPlaying;
        }

        public async void MusicSwitching(Music current, Music next, Windows.Media.Playback.MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                FindMusicAndSetPlaying(current, false);
                FindMusicAndSetPlaying(next, true);
            });
        }
        public void ShuffleChanged(IEnumerable<Music> newPlayList, bool isShuffle)
        {
            Songs.Clear();
            foreach (var music in newPlayList) Songs.Add(music);
        }
        public void MediaEnded() { return; }

        private void PlayItem_Click(object sender, RoutedEventArgs e)
        {
            Music music = (sender as MenuFlyoutItem).DataContext as Music;
            MainPage.Instance.SetMusicAndPlay(music);
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            Music music = (sender as MenuFlyoutItem).DataContext as Music;
            if (music.Equals(MediaHelper.CurrentMusic))
            {
                MediaHelper.NextMusic();
                Songs.Remove(music);
                MediaHelper.RemoveMusic(music);
            }
        }

        private void MoveToTopItem_Click(object sender, RoutedEventArgs e)
        {
            Music music = (sender as MenuFlyoutItem).DataContext as Music;
            Songs.Move(Songs.IndexOf(music), 0);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

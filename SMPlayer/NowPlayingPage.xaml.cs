using Microsoft.Toolkit.Uwp.Notifications;
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
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Notifications;
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
    public sealed partial class NowPlayingPage : Page, MediaControlListener
    {
        private ObservableCollection<Music> Songs = new ObservableCollection<Music>();
        public NowPlayingPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            MediaHelper.AddMediaControlListener(this as MediaControlListener);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //List<Music> playlist = await MediaHelper.GetRealPlayList();
            List<Music> playlist = MediaHelper.CurrentPlayList;
            if (Helper.SamePlayList(Songs, playlist)) return;
            Songs.Clear();
            foreach (var music in playlist)
                Songs.Add(music);
            FindMusicAndSetPlaying(MediaHelper.CurrentMusic, true);
        }

        private void SongsListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            args.ItemContainer.Background = args.ItemIndex % 2 == 0 ? Helper.WhiteSmokeBrush : Helper.WhiteBrush;
        }

        private void SongsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Music music = (Music)e.ClickedItem;
            MainPage.Instance.SetMusicAndPlay(music);
        }
        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            (Parent as MainPage).Frame.Navigate(typeof(NowPlayingFullPage));
        }
        private void NewListButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LyricsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MusicInfoButton_Click(object sender, RoutedEventArgs e)
        {

        }

        public void Tick() { return; }

        private void FindMusicAndSetPlaying(Music target, bool isPlaying)
        {
            var music = Songs.FirstOrDefault((m) => m.Equals(target));
            if (music != null) music.IsPlaying = isPlaying;
        }

        public async void MusicSwitchingAsync(Music current, Music next)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                FindMusicAndSetPlaying(current, false);
                FindMusicAndSetPlaying(next, true);
            });
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
                Songs.Remove(music);
                // TODO:
                // also need to remove from MediaHelper.
                MediaHelper.NextMusic();
            }
        }
    }
}

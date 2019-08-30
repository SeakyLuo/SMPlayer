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
        public static ObservableCollection<Music> Songs = new ObservableCollection<Music>();
        public ElementTheme Theme { get; set; }
        public static readonly DependencyProperty ThemeProperty = DependencyProperty.Register("Theme", typeof(ElementTheme), typeof(PlaylistControl), new PropertyMetadata(ElementTheme.Default));
        public bool AlternatingRowColor { get; set; }
        public PlaylistControl()
        {
            this.InitializeComponent();
            SongsListView.ItemsSource = Songs;
        }

        public static void SetPlaylist(IEnumerable<Music> playlist)
        {
            if (Helper.SamePlayList(Songs, playlist)) return;
            Songs.Clear();
            foreach (var music in playlist)
            {
                if (music.Equals(MediaHelper.CurrentMusic))
                    music.IsPlaying = true;
                Songs.Add(music);
            }
        }
        private void SongsListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            
            args.ItemContainer.Foreground = Theme == ElementTheme.Dark ? Helper.WhiteSmokeBrush : Helper.BlackBrush;
            if (AlternatingRowColor)
                args.ItemContainer.Background = args.ItemIndex % 2 == 0 ? Helper.WhiteSmokeBrush : Helper.WhiteBrush;
        }

        private void SongsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Music music = (Music)e.ClickedItem;
            ((Window.Current.Content as Frame).Content as MediaControlContainer).SetMusicAndPlay(music);
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
            ((Window.Current.Content as Frame).Content as MediaControlContainer).SetMusicAndPlay(music);
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

    public class PlaylistListItemContainerStyleSelector : StyleSelector
    {
        public Style NewStyle { get; set; }
        public Style OldStyle { get; set; }

        protected override Style SelectStyleCore(object item, DependencyObject container)
        {
            var obj = (string)item;
            if (obj.Equals("789"))
            {
                return NewStyle;
            }
            else
            {
                return OldStyle;
            }
        }
    }
}

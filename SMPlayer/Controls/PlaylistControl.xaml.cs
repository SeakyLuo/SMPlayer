﻿using SMPlayer.Models;
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
        public static ObservableCollection<Music> NowPlayingPlaylist = new ObservableCollection<Music>();
        private ObservableCollection<Music> CurrentPlaylist = new ObservableCollection<Music>();
        public ElementTheme Theme
        {
            get => SongsListView.RequestedTheme;
            set => SongsListView.RequestedTheme = value;
        }
        public static ElementTheme CurrentTheme;
        private static List<MusicRequestListener> MusicRequestListeners = new List<MusicRequestListener>();
        public bool AlternatingRowColor { get; set; }
        public bool AllowReorder
        {
            get => SongsListView.CanReorderItems;
            set
            {
                SongsListView.CanReorderItems = value;
                SongsListView.AllowDrop = value;
                SongsListView.CanDrag = value;
            }
        }
        public object Header
        {
            set => SongsListView.Header = value;
        }
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(object), typeof(PlaylistControl), new PropertyMetadata(null));
        public object ItemsSource
        {
            get => SongsListView.ItemsSource;
            set
            {
                CurrentPlaylist = new ObservableCollection<Music>(value as ICollection<Music>);
                SongsListView.ItemsSource = value;
            }
        }
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(object), typeof(PlaylistControl), new PropertyMetadata(null));

        public PlaylistControl()
        {
            this.InitializeComponent();
            MediaHelper.AddMediaControlListener(this as MediaControlListener);
            //CurrentTheme = Theme;
            if (ItemsSource == null) ItemsSource = NowPlayingPlaylist;
        }

        private void PlaylistController_Loading(FrameworkElement sender, object args)
        {
            //CurrentTheme = Theme;
        }

        private void PlaylistController_Loaded(object sender, RoutedEventArgs e)
        {
            CurrentTheme = Theme;
        }
        public static void AddMusic(object item)
        {
            if (item is Music)
                NowPlayingPlaylist.Add(item as Music);
            else if (item is ICollection<Music>)
                foreach (var music in item as ICollection<Music>) NowPlayingPlaylist.Add(music);
        }

        public static void AddMusicRequestListener(MusicRequestListener listener)
        {
            MusicRequestListeners.Add(listener);
        }

        public static void SetPlaylist(ICollection<Music> playlist)
        {
            if (Helper.SamePlayList(NowPlayingPlaylist, playlist)) return;
            NowPlayingPlaylist.Clear();
            foreach (var music in playlist)
            {
                music.IsPlaying = music.Equals(MediaHelper.CurrentMusic);
                NowPlayingPlaylist.Add(music);
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
            var music = CurrentPlaylist.FirstOrDefault((m) => m.Equals(target));
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
        public void ShuffleChanged(ICollection<Music> newPlayList, bool isShuffle)
        {
            CurrentPlaylist.Clear();
            foreach (var music in newPlayList) CurrentPlaylist.Add(music);
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
                CurrentPlaylist.Remove(music);
                MediaHelper.RemoveMusic(music);
            }
        }

        private void MoveToTopItem_Click(object sender, RoutedEventArgs e)
        {
            Music music = (sender as MenuFlyoutItem).DataContext as Music;
            CurrentPlaylist.Move(CurrentPlaylist.IndexOf(music), 0);
            MediaHelper.MoveMusic(music, 0);
        }

        private void SongsListView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {

        }
        private void OpenMusicMenuFlyout(object sender, object e)
        {
            MenuFlyoutHelper.InsertRemovableMusicMenu(sender);
        }
    }
}

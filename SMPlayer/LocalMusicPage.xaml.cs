﻿using SMPlayer.Controls;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LocalMusicPage : Page, ISwitchMusicListener, ILocalPageButtonListener, IRemoveMusicListener
    {
        public static FolderTree CurrentTree;
        private ObservableCollection<Music> Songs = new ObservableCollection<Music>();
        private string TreePath;
        public static bool ReverseRequested = false, SortByTitleRequested = false, SortByArtistRequested = false, SortByAlbumRequested = false;
        public static ILocalSetter setter;
        public LocalMusicPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            MediaHelper.SwitchMusicListeners.Add(this);
            LocalPage.MusicListener = this;
            LocalPlaylist.RemoveListeners.Add(this);
            LocalPlaylist.MultiSelectOption = new MultiSelectCommandBarOption() { ShowRemove = false };
            GridMusicView.RemoveListeners.Add(this);
            GridMusicView.MenuFlyoutOpeningOption = new MenuFlyoutOption()
            {
                MultiSelectOption = new MultiSelectCommandBarOption() { ShowRemove = false }
            };
            MusicInfoControl.MusicModifiedListeners.Add((before, after) =>
            {
                if (CurrentTree.FindMusic(before) is Music music)
                {
                    music.CopyFrom(after);
                    Songs.FirstOrDefault(m => m == before).CopyFrom(after);
                }
            });
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (CurrentTree.Path != TreePath) Setup(CurrentTree);
            ModeChanged(Settings.settings.LocalMusicGridView);
            if (ReverseRequested) Reverse();
            if (SortByTitleRequested) SortByTitle();
            if (SortByArtistRequested) SortByArtist();
            if (SortByAlbumRequested) SortByAlbum();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            CurrentTree = (FolderTree)e.Parameter;
        }

        public void UpdatePage(FolderTree tree)
        {
            if (tree.Equals(CurrentTree))
                Setup(tree);
        }
        private void Setup(FolderTree tree)
        {
            LoadingProgressBar.Visibility = Visibility.Visible;
            foreach (var music in tree.Files)
                music.IsPlaying = music.Equals(MediaHelper.CurrentMusic);
            try
            {
                if (GridMusicView.Visibility == Visibility.Visible)
                {
                    GridMusicView.Setup(tree.Files);
                    Songs.SetTo(tree.Files);
                }
                else
                {
                    Songs.SetTo(tree.Files);
                    GridMusicView.Setup(tree.Files);
                }
            }
            catch (InvalidOperationException)
            {
                // Loading while Set New Folder will cause this Exception
                System.Diagnostics.Debug.WriteLine("InvalidOperationException On Local Music Page");
            }
            TreePath = tree.Path;
            CurrentTree = tree;
            LoadingProgressBar.Visibility = Visibility.Collapsed;
        }

        public async void MusicSwitching(Music current, Music next, MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => MediaHelper.FindMusicAndSetPlaying(Songs, current, next));
        }

        public void ModeChanged(bool isGridView)
        {
            if (isGridView)
            {
                GridMusicView.Visibility = Visibility.Visible;
                LocalPlaylist.Visibility = Visibility.Collapsed;
                MainPage.Instance.SetMultiSelectListener(GridMusicView);
            }
            else
            {
                GridMusicView.Visibility = Visibility.Collapsed;
                LocalPlaylist.Visibility = Visibility.Visible;
                MainPage.Instance.SetMultiSelectListener(LocalPlaylist);
            }
        }
        public void Reverse()
        {
            Songs.SetTo(CurrentTree.Reverse());
            GridMusicView.Reverse();
            ReverseRequested = false;
            Settings.settings.Tree.FindTree(CurrentTree).CopyFrom(CurrentTree);
        }
        public void SortByTitle()
        {
            Songs.SetTo(CurrentTree.SortByTitle());
            GridMusicView.SortByTitle();
            SortByTitleRequested = false;
            CurrentTree.Criterion = SortBy.Title;
            Settings.settings.Tree.FindTree(CurrentTree).CopyFrom(CurrentTree);
        }
        public void SortByArtist()
        {
            Songs.SetTo(CurrentTree.SortByArtist());
            GridMusicView.SortByArtist();
            SortByAlbumRequested = false;
            CurrentTree.Criterion = SortBy.Artist;
            Settings.settings.Tree.FindTree(CurrentTree).CopyFrom(CurrentTree);
        }
        public void SortByAlbum()
        {
            Songs.SetTo(CurrentTree.SortByAlbum());
            GridMusicView.SortByAlbum();
            SortByAlbumRequested = false;
            CurrentTree.Criterion = SortBy.Album;
            Settings.settings.Tree.FindTree(CurrentTree).CopyFrom(CurrentTree);
        }

        public void MusicRemoved(int index, Music music, IEnumerable<Music> newCollection)
        {
            // It is actually delete music
            CurrentTree.Files.Remove(music);
            setter.SetNavText(CurrentTree.Info);
        }
    }
}

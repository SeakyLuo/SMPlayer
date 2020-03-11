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
    public sealed partial class LocalMusicPage : Page, SwitchMusicListener, LocalPageButtonListener
    {
        public static FolderTree CurrentTree;
        private ObservableCollection<Music> Songs = new ObservableCollection<Music>();
        private string TreePath;
        public static bool ReverseRequested = false, SortByTitleRequested = false, SortByArtistRequested = false, SortByAlbumRequested = false;
        public LocalMusicPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            MediaHelper.SwitchMusicListeners.Add(this);
            LocalPage.MusicViewModeChangedListener = this;
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
            }
            else
            {
                GridMusicView.Visibility = Visibility.Collapsed;
                LocalPlaylist.Visibility = Visibility.Visible;
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
            CurrentTree.Criteria = SortBy.Title;
            Settings.settings.Tree.FindTree(CurrentTree).CopyFrom(CurrentTree);
        }
        public void SortByArtist()
        {
            Songs.SetTo(CurrentTree.SortByArtist());
            GridMusicView.SortByArtist();
            SortByAlbumRequested = false;
            CurrentTree.Criteria = SortBy.Artist;
            Settings.settings.Tree.FindTree(CurrentTree).CopyFrom(CurrentTree);
        }
        public void SortByAlbum()
        {
            Songs.SetTo(CurrentTree.SortByAlbum());
            GridMusicView.SortByAlbum();
            SortByAlbumRequested = false;
            CurrentTree.Criteria = SortBy.Album;
            Settings.settings.Tree.FindTree(CurrentTree).CopyFrom(CurrentTree);
        }
    }
}

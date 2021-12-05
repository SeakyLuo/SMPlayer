using SMPlayer.Controls;
using SMPlayer.Helpers;
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
    public sealed partial class LocalMusicPage : Page, IMusicEventListener, ISwitchMusicListener, ILocalPageButtonListener
    {
        public static FolderTree CurrentTree;
        private readonly ObservableCollection<Music> Songs = new ObservableCollection<Music>();
        private string TreePath;
        public static bool ReverseRequested = false, SortByTitleRequested = false, SortByArtistRequested = false, SortByAlbumRequested = false;
        public static ILocalSetter setter;
        public LocalMusicPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            MusicPlayer.SwitchMusicListeners.Add(this);
            LocalPage.MusicListener = this;
            LocalPlaylist.MultiSelectOption = new MultiSelectCommandBarOption() { ShowRemove = false };
            GridMusicView.MenuFlyoutOpeningOption = new MenuFlyoutOption()
            {
                MultiSelectOption = new MultiSelectCommandBarOption() { ShowRemove = false }
            };
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
            try
            {
                if (GridMusicView.Visibility == Visibility.Visible)
                {
                    GridMusicView.Setup(tree.Songs);
                    Songs.SetTo(tree.Songs);
                }
                else
                {
                    Songs.SetTo(tree.Songs);
                    GridMusicView.Setup(tree.Songs);
                }
            }
            catch (InvalidOperationException e)
            {
                // Loading while Set New Folder will cause this Exception
                Log.Warn("InvalidOperationException On Local Music Page {0}", e);
            }
            TreePath = tree.Path;
            CurrentTree = tree;
            LoadingProgressBar.Visibility = Visibility.Collapsed;
        }

        public async void MusicSwitching(Music current, Music next, MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => MusicPlayer.SetMusicPlaying(Songs, next));
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

        void IMusicEventListener.Liked(Music music, bool isFavorite) { }
        void IMusicEventListener.Added(Music music) { }
        void IMusicEventListener.Removed(Music music)
        {
            CurrentTree.RemoveFile(music.Path);
            Songs.RemoveAll(i => i.Id == music.Id);
        }

        void IMusicEventListener.Modified(Music before, Music after)
        {
            if (CurrentTree.FindMusic(before) is Music music)
            {
                music.CopyFrom(after);
                Songs.FirstOrDefault(m => m == before).CopyFrom(after);
            }
        }
    }
}

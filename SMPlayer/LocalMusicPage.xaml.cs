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
    public sealed partial class LocalMusicPage : Page, SwitchMusicListener, ViewModeChangedListener
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
            Setup(CurrentTree);
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

        private async void Setup(FolderTree tree)
        {
            if (TreePath == tree.Path) return;
            LoadingProgressBar.Visibility = Visibility.Visible;
            foreach (var music in tree.Files)
                music.IsPlaying = music.Equals(MediaHelper.CurrentMusic);
            try
            {
                if (GridMusicView.Visibility == Visibility.Visible)
                {
                    await GridMusicView.Setup(tree.Files);
                    SetSongs(tree.Files);
                }
                else
                {
                    SetSongs(tree.Files);
                    await GridMusicView.Setup(tree.Files);
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

        private void SetSongs(ICollection<Music> songs)
        {
            Songs.Clear();
            foreach (var music in songs)
                Songs.Add(music);
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
            Songs.SetTo(Songs.Reverse());
            GridMusicView.Reverse();
            ReverseRequested = false;
        }
        public void SortByTitle()
        {
            Songs.SetTo(Songs.ToList().OrderBy((m) => m.Name));
            GridMusicView.SortByTitle();
            SortByTitleRequested = false;
        }
        public void SortByArtist()
        {
            Songs.SetTo(Songs.ToList().OrderBy((m) => m.Artist));
            GridMusicView.SortByArtist();
            SortByAlbumRequested = false;
        }
        public void SortByAlbum()
        {
            Songs.SetTo(Songs.ToList().OrderBy((m) => m.Album));
            GridMusicView.SortByAlbum();
            SortByAlbumRequested = false;
        }
    }
}

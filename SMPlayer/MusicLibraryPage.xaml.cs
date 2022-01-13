using Microsoft.Toolkit.Uwp.UI.Controls;
using Newtonsoft.Json.Linq;
using SMPlayer.Helpers;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MusicLibraryPage : Page, ISwitchMusicListener, IMusicEventListener, IMenuFlyoutHelperBuildListener
    {
        public ObservableCollection<Music> AllSongs = new ObservableCollection<Music>();
        public static bool IsLibraryUnchangedAfterChecking = true;

        public MusicLibraryPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            Settings.AddMusicEventListener(this);
            MusicPlayer.AddSwitchMusicListener(this);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SetHeader();
            if (string.IsNullOrEmpty(Settings.settings.RootPath)) return;
            SortAndSetAllSongs(Settings.AllSongs);
            MusicPlayer.SetMusicPlaying(AllSongs, MusicPlayer.CurrentMusic);
        }

        public void CheckLibrary()
        {
            if (Helper.CurrentFolder == null) return;
            UpdateHelper.RefreshFolder(Settings.settings.Tree, (folder) =>
            {
                IsLibraryUnchangedAfterChecking = true;
                SortAndSetAllSongs(Settings.AllSongs);
            });
        }

        private void MusicLibraryDataGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var music = (Music)MusicLibraryDataGrid.SelectedItem;
            if (music == null) return;
            MusicPlayer.AddMusicAndPlay(music);
        }

        private void MenuFlyout_Opening(object sender, object e)
        {
            if (MusicLibraryDataGrid.SelectedItems.Count > 1)
                MenuFlyoutHelper.SetPlaylistMenu(sender, null, this);
            else
                MenuFlyoutHelper.SetMusicMenu(sender, null, null, new MenuFlyoutOption() { ShowSelect = false });
        }

        public static IEnumerable<Music> SortPlaylist(IEnumerable<Music> playlist, SortBy criterion)
        {
            return playlist.OrderBy(SortByConverter.GetKeySelector(criterion));
        }
        public void SortAndSetAllSongs(IEnumerable<Music> list)
        {
            SetAllSongs(SortPlaylist(list, Settings.settings.MusicLibraryCriterion));
        }

        private void MusicLibraryDataGrid_Sorting(object sender, DataGridColumnEventArgs e)
        {
            string header = e.Column.Tag?.ToString();
            IEnumerable<Music> temp = SortPlaylist(AllSongs, Settings.settings.MusicLibraryCriterion = SortByConverter.FromStr(header));
            foreach (var column in MusicLibraryDataGrid.Columns)
                if (column.Tag?.ToString() != header)
                    column.SortDirection = null;
            if (e.Column.SortDirection == DataGridSortDirection.Ascending)
            {
                e.Column.SortDirection = DataGridSortDirection.Descending;
                temp = temp.Reverse();
            }
            else e.Column.SortDirection = DataGridSortDirection.Ascending;
            SetAllSongs(temp.ToList());
        }

        public void SetAllSongs(IEnumerable<Music> songs)
        {
            if (songs == null) return;
            AllSongs.Clear();
            var favSet = Settings.MyFavoritesPlaylist.Songs.Select(i => i.Id).ToHashSet();
            foreach (var item in songs)
            {
                Music music = item.Copy();
                music.Favorite = favSet.Contains(item.Id);
                AllSongs.Add(music);
            }
            SetHeader();
        }

        public async void MusicSwitching(Music current, Music next, Windows.Media.Playback.MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                MusicPlayer.SetMusicPlaying(AllSongs, next);
            });
        }

        public void OnBuild(MenuFlyoutHelper helper)
        {
            var list = new List<Music>();
            foreach (Music item in MusicLibraryDataGrid.SelectedItems)
            {
                list.Add(item);
            }
            helper.Data = list;
        }

        public void SetHeader()
        {
            if (MainPage.Instance?.CurrentPage != typeof(MusicLibraryPage))
            {
                return;
            }
            if (Settings.settings.ShowCount)
            {
                MainPage.Instance?.SetHeaderText("AllSongsWithCount", AllSongs.Count);
            }
            else
            {
                MainPage.Instance?.SetHeaderText("AllSongs");
            }
        }

        void IMusicEventListener.Liked(Music music, bool isFavorite)
        {
            if (AllSongs.FirstOrDefault(m => m == music) is Music target)
            {
                target.Favorite = isFavorite;
            }
        }

        void IMusicEventListener.Added(Music music)
        {
            //    var keySelector = SortByConverter.GetKeySelector(Settings.settings.MusicLibraryCriterion);
            //    for (int i = 0; i < AllSongs.Count; i++)
            //    {
            //        if (keySelector(music).CompareTo(keySelector(AllSongs[i])) <= 0)
            //        {
            //            AllSongs.Insert(i, music);
            //            goto AfterInsertion;
            //        }
            //    }
            //    AllSongs.Add(music);
            //    AfterInsertion:
            //    SetHeader();
        }

        void IMusicEventListener.Removed(Music music)
        {
            AllSongs.Remove(music);
            SetHeader();
        }

        void IMusicEventListener.Modified(Music before, Music after)
        {
            Log.Info("MusicLibraryPage Modified");
            AllSongs.FirstOrDefault(m => m == before)?.CopyFrom(after);
        }
    }
}

using Microsoft.Toolkit.Uwp.UI.Controls;
using Newtonsoft.Json.Linq;
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
    public sealed partial class MusicLibraryPage : Page, ISwitchMusicListener, IAfterSongsSetListener, ILikeMusicListener, IMenuFlyoutHelperBuildListener
    {
        public static ObservableCollection<Music> AllSongs = new ObservableCollection<Music>();
        public static int SongCount { get => AllSongs.Count; }
        public static bool IsLibraryUnchangedAfterChecking = true;

        private static List<IAfterSongsSetListener> listeners = new List<IAfterSongsSetListener>();

        public MusicLibraryPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            listeners.Add(this);
            SongsSet(AllSongs);
            MediaHelper.SwitchMusicListeners.Add(this);
            MediaControl.AddMusicModifiedListener(MusicModified);
            Controls.MusicInfoControl.MusicModifiedListeners.Add(MusicModified);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SetHeader();
            if (string.IsNullOrEmpty(Settings.settings.RootPath)) return;
            SortAndSetAllSongs(Settings.settings.MusicLibrary.Values);
            MediaHelper.FindMusicAndSetPlaying(AllSongs, null, MediaHelper.CurrentMusic);
        }

        public static async void CheckLibrary()
        {
            if (Helper.CurrentFolder == null) return;
            var data = new TreeUpdateData();
            await Settings.settings.Tree.CheckNewFile(data);
            if (IsLibraryUnchangedAfterChecking = data.More == 0 && data.Less == 0) return;
            SortAndSetAllSongs(Settings.settings.Tree.Flatten());
            NotifyListeners();
            App.Save();
        }

        public static void AddAfterSongsSetListener(IAfterSongsSetListener listener)
        {
            listeners.Add(listener);
        }

        private void MusicLibraryDataGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var music = (Music)MusicLibraryDataGrid.SelectedItem;
            if (music == null) return;
            MediaHelper.SetMusicAndPlay(music);
        }

        public static void MusicModified(Music before, Music after)
        {
            AllSongs.FirstOrDefault(m => m == before)?.CopyFrom(after);
            Settings.FindMusic(before)?.CopyFrom(after);
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
        public static void SortAndSetAllSongs(IEnumerable<Music> list)
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
            MusicLibraryDataGrid.ItemsSource = AllSongs;
        }

        public static void SetAllSongs(IEnumerable<Music> songs)
        {
            if (songs == null) return;
            AllSongs.Clear();
            var favSet = Settings.settings.MyFavorites.SongIds.ToHashSet();
            foreach (var item in songs)
            {
                item.Favorite = favSet.Contains(item.Id);
                AllSongs.Add(item);
            }
            SetHeader();
            NotifyListeners();
        }

        public static void AddMusic(Music music)
        {
            if (Settings.FindMusic(music) != null) return;
            var keySelector = SortByConverter.GetKeySelector(Settings.settings.MusicLibraryCriterion);
            for (int i = 0; i < SongCount; i++)
            {
                if (keySelector(music).CompareTo(keySelector(AllSongs[i])) <= 0)
                {
                    AllSongs.Insert(i, music);
                    goto AfterInsertion;
                }
            }
            AllSongs.Add(music);
            AfterInsertion:
            SetHeader();
            Settings.settings.AddMusic(music);
            NotifyListeners();
        }

        public static void RemoveMusic(Music music)
        {
            if (Settings.FindMusic(music) == null) return;
            AllSongs.Remove(music);
            SetHeader();
            Settings.settings.RemoveMusic(music);
            NotifyListeners();
        }

        private static void NotifyListeners()
        {
            foreach (var listener in listeners) listener.SongsSet(AllSongs);
        }

        public async void MusicSwitching(Music current, Music next, Windows.Media.Playback.MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => MediaHelper.FindMusicAndSetPlaying(AllSongs, current, next));
        }

        public void SongsSet(ICollection<Music> songs)
        {
            MusicLibraryDataGrid.ItemsSource = songs;
        }

        public void MusicLiked(Music music, bool isFavorite)
        {
            var target = AllSongs.FirstOrDefault(m => m == music);
            if (target != null) target.Favorite = isFavorite;
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

        public static void SetHeader()
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

        public static List<Music> GetMostPlayed(int limit)
        {
            List<Music> list = new List<Music>();
            foreach (var group in AllSongs.GroupBy(m => m.PlayCount).OrderByDescending(g => g.Key))
            {
                if (list.Count > limit) break;
                list.AddRange(group);
            }
            return list;
        }

        public static List<Music> GetLeastPlayed(int limit)
        {
            List<Music> list = new List<Music>();
            foreach (var group in AllSongs.GroupBy(m => m.PlayCount).OrderBy(g => g.Key))
            {
                if (list.Count > limit) break;
                list.AddRange(group);
            }
            return list;
        }
    }

    public interface IAfterSongsSetListener
    {
        void SongsSet(ICollection<Music> songs);
    }
}

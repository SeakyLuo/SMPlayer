using Microsoft.Toolkit.Uwp.UI.Controls;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public const string JsonFilename = "MusicLibrary";
        public static ObservableCollection<Music> AllSongs = new ObservableCollection<Music>();
        public static int SongCount { get => AllSongs.Count; }
        public static HashSet<Music> AllSongsSet;
        public static bool IsLibraryUnchangedAfterChecking = true;

        private static List<IAfterSongsSetListener> listeners = new List<IAfterSongsSetListener>();

        public MusicLibraryPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            listeners.Add(this);
            SongsSet(AllSongs);
            MediaHelper.SwitchMusicListeners.Add(this);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Settings.settings.RootPath)) return;
            SetHeader();
            MediaHelper.FindMusicAndSetPlaying(AllSongs, null, MediaHelper.CurrentMusic);
        }
        public static async Task Init()
        {
            var songs = JsonFileHelper.Convert<List<Music>>(await JsonFileHelper.ReadAsync(JsonFilename));
            // 如果初始化了设置但是音乐库没有音乐
            if (songs?.Count == 0 && Settings.Inited && (songs = Settings.settings.Tree.Flatten()).Count > 0)
                SortAndSetAllSongs(songs);
            else
                SetAllSongs(songs);
            MediaControl.AddMusicModifiedListener(MusicModified);
            Controls.MusicInfoControl.MusicModifiedListeners.Add(MusicModified);
        }

        public static void Save()
        {
            JsonFileHelper.SaveAsync(JsonFilename, AllSongs);
            JsonFileHelper.SaveAsync(Helper.TempFolder, JsonFilename + Helper.TimeStamp, AllSongs);
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
            MediaHelper.SetMusicAndPlay(AllSongs, music);
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
            Save();
            MusicLibraryDataGrid.ItemsSource = AllSongs;
        }

        public static void SetAllSongs(IEnumerable<Music> songs)
        {
            if (songs == null) return;
            AllSongs.Clear();
            var favSet = Settings.settings.MyFavorites.Songs.ToHashSet();
            foreach (var item in songs)
            {
                item.Favorite = favSet.Contains(item);
                AllSongs.Add(item);
            }
            AllSongsSet = AllSongs.ToHashSet();
            SetHeader();
            NotifyListeners();
        }
        public static void AddMusic(Music music)
        {
            if (!AllSongsSet.Add(music)) return;
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
            if (!AllSongsSet.Remove(music)) return;
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
            if (Settings.settings.ShowCount)
            {
                MainPage.Instance?.SetHeaderText("AllSongsWithCount", AllSongs.Count);
            }
            else
            {
                MainPage.Instance?.SetHeaderText("AllSongs");
            }
        }
    }

    public interface IAfterSongsSetListener
    {
        void SongsSet(ICollection<Music> songs);
    }
}

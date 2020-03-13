using Microsoft.Toolkit.Uwp.UI.Controls;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
    public sealed partial class MusicLibraryPage : Page, SwitchMusicListener, AfterSongsSetListener, LikeMusicListener
    {
        private const string FILENAME = "MusicLibrary.json";
        public static ObservableCollection<Music> AllSongs = new ObservableCollection<Music>();
        public static HashSet<Music> AllSongsSet;
        public static bool IsLibraryUnchangedAfterChecking = true;

        private static bool libraryReset = false;
        private static List<AfterSongsSetListener> listeners = new List<AfterSongsSetListener>();

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
            MediaHelper.FindMusicAndSetPlaying(AllSongs, null, MediaHelper.CurrentMusic);
        }

        public static async Task Init()
        {
            SetAllSongs(JsonFileHelper.Convert<ObservableCollection<Music>>(await JsonFileHelper.ReadAsync(FILENAME)));
            MediaControl.AddMusicModifiedListener(MusicModified);
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

        public static void Save()
        {
            JsonFileHelper.SaveAsync(FILENAME, AllSongs);
        }

        public static void AddAfterSongsSetListener(AfterSongsSetListener listener)
        {
            listeners.Add(listener);
            if (libraryReset) listener.SongsSet(AllSongs);
        }

        private void MusicLibraryDataGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var music = (Music)MusicLibraryDataGrid.SelectedItem;
            if (music == null) return;
            MediaHelper.SetMusicAndPlay(AllSongs, music);
        }

        public static void MusicModified(Music before, Music after)
        {
            AllSongs.FirstOrDefault(m => m.Equals(before))?.CopyFrom(after);
        }

        private void MenuFlyout_Opening(object sender, object e)
        {
            if (MusicLibraryDataGrid.SelectedItems.Count > 1)
            {
                MenuFlyoutHelper.SetPlaylistMenu(sender);
            }
            else
            {
                MenuFlyoutHelper.SetMusicMenu(sender);
            }
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
            libraryReset = true;
            AllSongs.Clear();
            var favSet = Settings.settings.MyFavorites.Songs.ToHashSet();
            foreach (var item in songs)
            {
                item.Favorite = favSet.Contains(item);
                AllSongs.Add(item);
            }
            AllSongsSet = AllSongs.ToHashSet();
            NotifyListeners();
        }
        public static void AddMusic(Music music)
        {
            if (!AllSongsSet.Add(music)) return;
            libraryReset = true;
            var keySelector = SortByConverter.GetKeySelector(Settings.settings.MusicLibraryCriterion);
            for (int i = 0; i < AllSongs.Count; i++)
            {
                if (keySelector(music).CompareTo(keySelector(AllSongs[i])) <= 0)
                {
                    AllSongs.Insert(i, music);
                    goto AfterInsertion;
                }
            }
            AllSongs.Add(music);
            AfterInsertion:
            Settings.settings.AddMusic(music);
            NotifyListeners();
        }
        public static void RemoveMusic(Music music)
        {
            if (!AllSongsSet.Remove(music)) return;
            libraryReset = true;
            AllSongs.Remove(music);
            Settings.settings.RemoveMusic(music);
            NotifyListeners();
        }

        private static void NotifyListeners()
        {
            foreach (var listener in listeners) listener.SongsSet(AllSongs);
        }

        public static List<Music> ConvertMusicPathToCollection(ICollection<string> paths, bool isFavorite = false)
        {
            List<Music> collection = new List<Music>();
            foreach (var path in paths)
            {
                if (MusicFromPath(path) is Music music)
                {
                    if (isFavorite) music.Favorite = true;
                    collection.Add(music);
                }
            }
            return collection;
        }

        public static Music MusicFromPath(string path)
        {
            return AllSongs.FirstOrDefault(m => m.Path == path);
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
    }

    public interface AfterSongsSetListener
    {
        void SongsSet(ICollection<Music> songs);
    }
}

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

        private bool libraryChecked = false;
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

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Settings.settings.RootPath)) return;
            MediaHelper.FindMusicAndSetPlaying(AllSongs, null, MediaHelper.CurrentMusic);
            if (!libraryChecked)
                await Dispatcher.RunIdleAsync((args) => { libraryChecked = true; CheckLibrary(); });
        }

        public static async Task Init()
        {
            SetAllSongs(JsonFileHelper.Convert<ObservableCollection<Music>>(await JsonFileHelper.ReadAsync(FILENAME)));
            MediaControl.AddMusicControlListener(MusicModified);
        }

        public static async void CheckLibrary()
        {
            if (Helper.CurrentFolder == null) return;
            var tree = new FolderTree();
            await tree.Init(Helper.CurrentFolder);
            var newLibrary = tree.Flatten();
            if (IsLibraryUnchangedAfterChecking = AllSongs.SameAs(newLibrary)) return;
            tree.MergeFrom(Settings.settings.Tree);
            Settings.settings.Tree = tree;
            SetAllSongs(newLibrary);
            SettingsPage.NotifyLibraryChange(tree.Path);
            Save();
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
            var music = AllSongs.First(m => m == before);
            music.CopyFrom(after);
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

        private void MusicLibraryDataGrid_Sorting(object sender, DataGridColumnEventArgs e)
        {
            string header = e.Column.Tag?.ToString();
            IEnumerable<Music> temp = null;
            switch (header)
            {
                case "Name":
                    temp = AllSongs.OrderBy((music) => music.Name);
                    break;
                case "Album":
                    temp = AllSongs.OrderBy((music) => music.Album);
                    break;
                case "Artist":
                    temp = AllSongs.OrderBy((music) => music.Artist);
                    break;
                case "Duration":
                    temp = AllSongs.OrderBy((music) => music.Duration);
                    break;
                case "PlayCount":
                    temp = AllSongs.OrderBy((music) => music.PlayCount);
                    break;
                default:
                    return;
            }
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

        public static void SetAllSongs(ICollection<Music> songs)
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
            foreach (var listener in listeners) listener.SongsSet(AllSongs);
            Save();
        }

        public static List<Music> ConvertMusicPathToCollection(ICollection<string> paths, bool isFavorite = false)
        {
            List<Music> collection = new List<Music>();
            foreach (var path in paths)
            {
                if (AllSongs.FirstOrDefault((m) => m.Path == path) is Music music)
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
            var target = AllSongs.FirstOrDefault((m) => m == music);
            if (target != null) target.Favorite = isFavorite;
        }
    }

    public interface AfterSongsSetListener
    {
        void SongsSet(ICollection<Music> songs);
    }
}

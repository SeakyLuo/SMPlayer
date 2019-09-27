using System;
using System.Collections.Generic;
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
using System.Collections.ObjectModel;
using SMPlayer.Models;
using Windows.Storage;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.UI.Core;
using System.Threading.Tasks;
using System.Diagnostics;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MusicLibraryPage : Page, MusicControlListener, SwitchMusicListener, AfterSongsSetListener
    {
        private const string FILENAME = "MusicLibrary.json";
        public static ObservableCollection<Music> AllSongs = new ObservableCollection<Music>();
        private static HashSet<Music> MusicSet = new HashSet<Music>();
        private bool libraryChecked = false;
        private static bool libraryReset = false;
        private static List<AfterSongsSetListener> listeners = new List<AfterSongsSetListener>();

        public MusicLibraryPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            listeners.Add(this as AfterSongsSetListener);
            SongsSet(AllSongs);
            MediaControl.AddMusicControlListener(this as MusicControlListener);
            MediaHelper.SwitchMusicListeners.Add(this as SwitchMusicListener);
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
        }

        public static async void CheckLibrary()
        {
            if (Helper.CurrentFolder == null) return;
            var tree = new FolderTree();
            await tree.Init(Helper.CurrentFolder);
            var newLibrary = tree.Flatten();
            if (Helper.SamePlaylist(AllSongs, newLibrary)) return;
            tree.Update(Settings.settings.Tree);
            Settings.settings.Tree = tree;
            SetAllSongs(newLibrary);
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

        public void MusicModified(Music before, Music after)
        {
            var music = AllSongs.First((m) => m.Equals(before));
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
            string header = e.Column.Header.ToString();
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
                case "Play Count":
                    temp = AllSongs.OrderBy((music) => music.PlayCount);
                    break;
                default:
                    return;
            }
            foreach (var column in MusicLibraryDataGrid.Columns)
                if (column.Header.ToString() != header)
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
            foreach (var item in songs) AllSongs.Add(item);
            MusicSet = AllSongs.ToHashSet();
            foreach (var listener in listeners) listener.SongsSet(AllSongs);
        }

        public static List<Music> ConvertMusicPathToCollection(ICollection<string> paths)
        {
            List<Music> collection = new List<Music>();
            foreach (var path in paths)
                if (MusicSet.FirstOrDefault((m) => m.Path == path) is Music music)
                    collection.Add(music);
            return collection;
        }

        public async void MusicSwitching(Music current, Music next, Windows.Media.Playback.MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => MediaHelper.FindMusicAndSetPlaying(AllSongs, current, next));
        }

        public void SongsSet(ICollection<Music> songs)
        {
            MusicLibraryDataGrid.ItemsSource = songs;
        }
    }

    public interface AfterSongsSetListener
    {
        void SongsSet(ICollection<Music> songs);
    }
}

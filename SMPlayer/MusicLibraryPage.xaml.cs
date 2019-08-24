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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MusicLibraryPage : Page, MusicControlListener, MediaControlListener
    {
        private const string FILENAME = "MusicLibrary.json";
        public static ObservableCollection<Music> AllSongs = new ObservableCollection<Music>();

        public MusicLibraryPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            MainPage.AddMusicControlListener(this as MusicControlListener);
            MediaHelper.AddMediaControlListener(this as MediaControlListener);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Settings.settings.RootPath)) return;
            Update();
            FindMusicAndSetPlaying(MediaHelper.CurrentMusic, true);
            await Dispatcher.TryRunAsync(CoreDispatcherPriority.Low, () => { });
            await Dispatcher.RunIdleAsync((args) => { CheckLibrary(); });
        }

        public static async Task Init()
        {
            AllSongs = JsonFileHelper.Convert<ObservableCollection<Music>>(await JsonFileHelper.ReadAsync(FILENAME));
        }

        public static async void CheckLibrary()
        {
            if (Helper.CurrentFolder == null) return;
            var tree = new FolderTree();
            await tree.Init(Helper.CurrentFolder);
            var newLibrary = tree.Flatten();
            if (Helper.SamePlayList(AllSongs, newLibrary)) return;
            tree.Update(Settings.settings.Tree);
            Settings.settings.Tree = tree;
            if (Helper.SamePlayList(AllSongs, MediaHelper.CurrentPlayList))
                await MediaHelper.SetPlayList(newLibrary);
            SetAllSongs(newLibrary);
            Save();
        }

        public static void Save()
        {
            JsonFileHelper.SaveAsync(FILENAME, AllSongs);
        }

        private void Update()
        {
            MusicLibraryDataGrid.ItemsSource = AllSongs;
        }
        private void PlayItem_Click(object sender, RoutedEventArgs e)
        {
            var music = (sender as MenuFlyoutItem).DataContext as Music;
            MediaHelper.MoveToMusic(music);
            PlayMusic(music);
        }

        private void MusicLibraryDataGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var music = (Music)MusicLibraryDataGrid.SelectedItem;
            PlayMusic(music);
        }

        private async void PlayMusic(Music music)
        {
            await MediaHelper.SetPlayList(AllSongs.ToList());
            MainPage.Instance.SetMusicAndPlay(music);
        }

        public void MusicModified(Music before, Music after)
        {
            var music = AllSongs.First((m) => m.Equals(before));
            music.CopyFrom(after);
        }

        private void AddToMyFavorites_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MusicInfoItem_Click(object sender, RoutedEventArgs e)
        {
            var list = AllSongs;
        }

        private void MusicLibraryDataGrid_Sorting(object sender, DataGridColumnEventArgs e)
        {
            string header = e.Column.Header.ToString();
            IOrderedEnumerable<Music> temp;
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
            SetAllSongs(temp.ToList());
            foreach (var column in MusicLibraryDataGrid.Columns)
                if (column.Header.ToString() != header)
                    column.SortDirection = null;
            if (e.Column.SortDirection == DataGridSortDirection.Ascending)
            {
                e.Column.SortDirection = DataGridSortDirection.Descending;
                AllSongs.Reverse();
            }
            else e.Column.SortDirection = DataGridSortDirection.Ascending;
            Update();
            Save();
        }

        public static void SetAllSongs(IEnumerable<Music> songs)
        {
            if (AllSongs == null) AllSongs = new ObservableCollection<Music>();
            else AllSongs.Clear();
            foreach (var item in songs) AllSongs.Add(item);
        }

        public void Tick() { return; }

        private void FindMusicAndSetPlaying(Music target, bool isPlaying)
        {
            if (target == null) return;
            var music = AllSongs.FirstOrDefault((m) => m.Equals(target));
            if (music != null) music.IsPlaying = isPlaying;
        }

        public async void MusicSwitchingAsync(Music current, Music next)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                FindMusicAndSetPlaying(current, false);
                FindMusicAndSetPlaying(next, true);
            });
        }

        public void MediaEnded() { return; }

    }
}

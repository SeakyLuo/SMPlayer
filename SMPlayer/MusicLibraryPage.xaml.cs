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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MusicLibraryPage : Page, MediaControlListener
    {
        private static readonly string FILENAME = "MusicLibrary.json";
        public static List<Music> AllSongs = new List<Music>();

        public MusicLibraryPage()
        {
            this.InitializeComponent();
            MainPage.Instance.AddMusicModificationListener("MusicLibraryPage", this);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Settings.settings.RootPath)) return;
            if (AllSongs.Count == 0)
            {
                MusicLibraryProgressRing.IsActive = true;
                MusicLibraryProgressRing.Visibility = Visibility.Visible;
                await Settings.SetTreeFolder(await StorageFolder.GetFolderFromPathAsync(Settings.settings.RootPath));
                MusicLibraryDataGrid.ItemsSource = AllSongs;
                MusicLibraryProgressRing.IsActive = false;
                MusicLibraryProgressRing.Visibility = Visibility.Collapsed;
            }
            else
            {
                MusicLibraryDataGrid.ItemsSource = AllSongs;
            }
        }

        public static async void Init()
        {
            AllSongs = JsonFileHelper.Convert<List<Music>>(await JsonFileHelper.ReadAsync(FILENAME));
        }

        public static void Save()
        {
            JsonFileHelper.SaveAsync(FILENAME, AllSongs);
        }
        private void PlayItem_Click(object sender, RoutedEventArgs e)
        {
            var music = (sender as MenuFlyoutItem).DataContext as Music;
            MainPage.Instance.SetMusic(music);
        }

        public void MusicModified(Music before, Music after)
        {
            int index = AllSongs.IndexOf(before);
            if (index < 0) return;
            AllSongs[index] = after;
            Save();
        }
        public void MusicSet(Music music)
        {
            int index = AllSongs.IndexOf(music);
        }

        private void MusicLibraryDataGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var music = (Music)MusicLibraryDataGrid.SelectedItem;
            MainPage.Instance.SetMusic(music);
        }

        private void AddToMyFavorites_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MusicLibraryDataGrid_Sorting(object sender, DataGridColumnEventArgs e)
        {
            string header = e.Column.Header.ToString();
            switch (header)
            {
                case "Name":
                    AllSongs = AllSongs.OrderBy((music) => music.Name).ToList();
                    break;
                case "Album":
                    AllSongs = AllSongs.OrderBy((music) => music.Album).ToList();
                    break;
                case "Artist":
                    AllSongs = AllSongs.OrderBy((music) => music.Artist).ToList();
                    break;
                case "Duration":
                    AllSongs = AllSongs.OrderBy((music) => music.Duration).ToList();
                    break;
                case "Play Count":
                    AllSongs = AllSongs.OrderBy((music) => music.PlayCount).ToList();
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
                AllSongs.Reverse();
            }
            else e.Column.SortDirection = DataGridSortDirection.Ascending;
            MusicLibraryDataGrid.ItemsSource = AllSongs;
            Save();
        }
    }
}

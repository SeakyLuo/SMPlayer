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
    public sealed partial class MusicLibraryPage : Page, MusicModificationListener
    {
        private static readonly string FILENAME = "MusicLibrary.json";
        public static List<Music> AllSongs = new List<Music>();

        public MusicLibraryPage()
        {
            this.InitializeComponent();
            MusicLibraryDataGrid.ItemsSource = AllSongs;
            MainPage.Instance.AddMusicModificationListener("MusicLibraryPage", this);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Settings.settings.RootPath)) return;
            if (AllSongs.Count == 0)
            {
                MusicLibraryProgressRing.IsActive = true;
                MusicLibraryProgressRing.Visibility = Visibility.Visible;
                Settings.SetTreeFolder(await StorageFolder.GetFolderFromPathAsync(Settings.settings.RootPath), AfterTreeFirstlySet);
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

        private void AfterTreeFirstlySet()
        {
            MusicLibraryDataGrid.ItemsSource = AllSongs;
            MusicLibraryProgressRing.IsActive = false;
            MusicLibraryProgressRing.Visibility = Visibility.Collapsed;
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

        private void MusicLibraryDataGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var music = (Music)MusicLibraryDataGrid.SelectedItem;
            MainPage.Instance.SetMusic(music);
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

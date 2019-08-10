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
        private static ObservableCollection<Music> songs = new ObservableCollection<Music>();
        private static List<Music> copy;

        public MusicLibraryPage()
        {
            this.InitializeComponent();
            //MusicLibraryDataGrid.ItemsSource = songs;
            MainPage.Instance.AddMusicModificationListener("MusicLibraryPage", this);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Settings.settings.RootPath)) return;
            if (songs.Count == 0)
            {
                MusicLibraryProgressRing.IsActive = true;
                MusicLibraryProgressRing.Visibility = Visibility.Visible;
                Settings.SetTreeFolder(await StorageFolder.GetFolderFromPathAsync(Settings.settings.RootPath), AfterTreeFirstlySet);
            }
            else
            {
                Settings.SetTreeFolder(await StorageFolder.GetFolderFromPathAsync(Settings.settings.RootPath), ConvertCollection);
            }
        }

        private void AfterTreeFirstlySet()
        {
            ConvertCollection();
            MusicLibraryDataGrid.ItemsSource = songs;
            MusicLibraryProgressRing.IsActive = false;
            MusicLibraryProgressRing.Visibility = Visibility.Collapsed;
        }

        private void ConvertCollection()
        {
            songs.Clear();
            foreach (var item in MusicManager.AllSongs)
                songs.Add(item);
        }

        private void PlayItem_Click(object sender, RoutedEventArgs e)
        {
            var music = (sender as MenuFlyoutItem).DataContext as Music;
            MainPage.Instance.SetMusic(music);
        }

        public void MusicModified(Music before, Music after)
        {
            MusicManager.AllSongs[MusicManager.AllSongs.IndexOf(before)] = after;
            copy = songs.ToList();
            copy[copy.IndexOf(before)] = after;
            songs.Clear();
            foreach (var item in copy)
                songs.Add(item);
            //MusicLibraryDataGrid.ItemsSource = songs;
        }

        private void MusicLibraryDataGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var music = (Music)MusicLibraryDataGrid.SelectedItem;
            MainPage.Instance.SetMusic(music);
        }
    }
}

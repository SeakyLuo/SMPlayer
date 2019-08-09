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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MusicLibraryPage : Page, MusicModificationListener
    {
        private ObservableCollection<Music> songs = new ObservableCollection<Music>();

        public MusicLibraryPage()
        {
            this.InitializeComponent();
            MusicLibraryDataGrid.ItemsSource = songs;
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
            var menu = (MenuFlyoutItem)sender;
            var music = (Music)menu.DataContext;
            MainPage.Instance.SetMusic(music);
            MainPage.Instance.AddMusicModificationListener(this);
        }

        public void FavoriteChangeListener(Music music, bool favorite)
        {
            int index = MusicManager.AllSongs.IndexOf(music);
            MusicManager.AllSongs[index].Favorite = favorite;
            songs[index].Favorite = favorite;
        }
    }
}

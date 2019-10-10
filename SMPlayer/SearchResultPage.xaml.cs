using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SearchResultPage : Page
    {
        private string Keyword;
        public ObservableCollection<Playlist> Artists = new ObservableCollection<Playlist>();
        public ObservableCollection<AlbumView> Albums = new ObservableCollection<AlbumView>();
        public ObservableCollection<Music> Songs = new ObservableCollection<Music>();
        public ObservableCollection<AlbumView> Playlists = new ObservableCollection<AlbumView>();
        public SearchResultPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string text = (string)e.Parameter;
            if (text == Keyword) return;
            LoadingProgressBar.Visibility = Visibility.Visible;
            Keyword = text;
            switch (Keyword)
            {
                case "Artists":
                    Artists.Clear();
                    foreach (var group in MusicLibraryPage.AllSongs.Where((m) => SearchPage.IsTargetArtist(m, text)).GroupBy((m) => m.Artist).OrderBy((g) => g.Key))
                        Artists.Add(new Playlist(group.Key, group));
                    break;
                case "Albums":
                    Albums.Clear();
                    foreach (var group in MusicLibraryPage.AllSongs.Where((m) => SearchPage.IsTargetAlbum(m, text)).GroupBy((m) => m.Album))
                    {
                        Music music = group.ElementAt(0);
                        Albums.Add(new AlbumView(music.Album, music.Artist, group.OrderBy((m) => m.Name).ThenBy((m) => m.Artist)));
                    }
                    break;
                case "Songs":
                    Songs.Clear();
                    foreach (var music in MusicLibraryPage.AllSongs)
                        if (SearchPage.IsTargetMusic(music, text))
                            Songs.Add(music);
                    break;
                case "Playlists":
                    Playlists.Clear();
                    foreach (var playlist in Settings.settings.Playlists)
                        if (SearchPage.IsTargetPlaylist(playlist, text))
                            Playlists.Add(playlist.ToAlbumView());
                    break;
            }
            LoadingProgressBar.Visibility = Visibility.Collapsed;
        }
    }
}

﻿using SMPlayer.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using System;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SearchPage : Page
    {
        public static Stack<string> History = new Stack<string>();
        public ObservableCollection<Playlist> Artists = new ObservableCollection<Playlist>();
        public ObservableCollection<AlbumView> Albums = new ObservableCollection<AlbumView>();
        public ObservableCollection<Music> Songs = new ObservableCollection<Music>();
        public ObservableCollection<AlbumView> Playlists = new ObservableCollection<AlbumView>();
        public const int ArtistLimit = 10, AlbumLimit = 10, SongLimit = 5, PlaylistLimit = 10;
        public SearchPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string keyword = e.Parameter as string;
            switch (e.NavigationMode)
            {
                case NavigationMode.New:
                    History.Push(keyword);
                    Search(keyword);
                    break;
                case NavigationMode.Back:
                    if (History.Pop() != keyword)
                        Search(keyword);
                    break;
            }
        }

        private async void Search(string keyword)
        {
            MainPage.Instance.SetHeaderText(GetSearchHeader(keyword, MainPage.Instance.IsMinimal));
            LoadingProgress.Visibility = Visibility.Visible;
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                SearchArtists(keyword);
                SearchAlbums(keyword);
                SearchSongs(keyword);
                SearchPlaylists(keyword);
                NoResultTextBlock.Visibility = Artists.Count == 0 && Albums.Count == 0 && Songs.Count == 0 && Playlists.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                LoadingProgress.Visibility = Visibility.Collapsed;
            });
        }

        public static bool IsTargetArtist(Music music, string keyword)
        {
            return IsTargetArtist(music.Artist, keyword);
        }
        public static bool IsTargetArtist(string artist, string keyword)
        {
            return artist.ToLower().Contains(keyword);
        }
        public void SearchArtists(string keyword)
        {
            Artists.Clear();
            bool viewAll = false;
            foreach (var group in MusicLibraryPage.AllSongs.Where(m => IsTargetArtist(m, keyword)).GroupBy(m => m.Artist))
            {
                if (viewAll = Artists.Count == ArtistLimit) break;
                Artists.Add(new Playlist(group.Key, group) { Artist = group.Key });
            }
            ArtistsViewAllButton.Visibility = viewAll ? Visibility.Visible : Visibility.Collapsed;
        }
        public static bool IsTargetAlbum(Music music, string keyword)
        {
            return music.Album.ToLower().Contains(keyword) || music.Artist.ToLower().Contains(keyword);
        }
        public void SearchAlbums(string keyword)
        {
            Albums.Clear();
            bool viewAll = false;
            foreach (var group in MusicLibraryPage.AllSongs.Where(m => IsTargetAlbum(m, keyword)).GroupBy(m => m.Album))
            {
                if (viewAll = Albums.Count == AlbumLimit) break;
                Music music = group.ElementAt(0);
                Albums.Add(new AlbumView(music.Album, music.Artist, group.OrderBy(m => m.Name).ThenBy(m => m.Artist), false));
            }
            AlbumsViewAllButton.Visibility = viewAll ? Visibility.Visible : Visibility.Collapsed;
        }
        public static bool IsTargetMusic(Music music, string keyword)
        {
            return music.Name.ToLower().Contains(keyword) || music.Album.ToLower().Contains(keyword) || music.Artist.ToLower().Contains(keyword);
        }

        public void SearchSongs(string keyword)
        {
            Songs.Clear();
            bool viewAll = false;
            foreach (var music in MusicLibraryPage.AllSongs)
            {
                if (IsTargetMusic(music, keyword))
                {
                    if (viewAll = Songs.Count == SongLimit) break;
                    Songs.Add(music);
                }
            }
            SongsViewAllButton.Visibility = viewAll ? Visibility.Visible : Visibility.Collapsed;
        }
        public static bool IsTargetPlaylist(Playlist playlist, string keyword)
        {
            return playlist.Name.Contains(keyword) || playlist.Songs.Any((music) => IsTargetMusic(music, keyword));
        }

        public void SearchPlaylists(string keyword)
        {
            Playlists.Clear();
            bool viewAll = false;
            foreach (var playlist in Settings.settings.Playlists)
            {
                if (IsTargetPlaylist(playlist, keyword))
                {
                    if (viewAll = Playlists.Count == PlaylistLimit) break;
                    Playlists.Add(playlist.ToAlbumView());
                }
            }
            PlaylistsViewAllButton.Visibility = viewAll ? Visibility.Visible : Visibility.Collapsed;
        }

        public static string GetSearchHeader(string keyword, bool isMinimal)
        {
            string header = Helper.LocalizeMessage("Quotations", keyword);
            return isMinimal ? header : Helper.LocalizeMessage("SearchResult", header);
        }
        private void SearchArtistView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(ArtistsPage), e.ClickedItem);
        }
        private void SearchAlbumView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(AlbumPage), e.ClickedItem);
        }
        private void SearchPlaylistView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(PlaylistsPage), e.ClickedItem);
        }
        private void ArtistsViewAllButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(SearchResultPage), SearchType.Artists);
        }

        private void Album_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            (args.NewValue as AlbumView)?.SetCover();
        }

        private void AlbumsViewAllButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(SearchResultPage), SearchType.Albums);
        }
        private void SongsViewAllButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(SearchResultPage), SearchType.Songs);
        }
        private void PlaylistsViewAllButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(SearchResultPage), SearchType.Playlists);
        }
    }

    public enum SearchType { Artists = 0, Albums = 1, Songs = 2, Playlists = 3 }
}

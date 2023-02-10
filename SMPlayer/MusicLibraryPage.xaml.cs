using Microsoft.Toolkit.Uwp.UI.Controls;
using Newtonsoft.Json.Linq;
using SMPlayer.Helpers;
using SMPlayer.Interfaces;
using SMPlayer.Models;
using SMPlayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
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
    public sealed partial class MusicLibraryPage : Page, IMusicPlayerEventListener, IMusicEventListener, IMenuFlyoutHelperBuildListener
    {
        public ObservableCollection<MusicView> AllSongs = new ObservableCollection<MusicView>();

        public MusicLibraryPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            MusicService.AddMusicEventListener(this);
            MusicPlayer.AddMusicPlayerEventListener(this);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SetHeader();
            if (string.IsNullOrEmpty(Settings.settings.RootPath)) return;
            LoadingProgress.Visibility = Visibility.Visible;
            await Helper.RunInMainUIThread(Dispatcher, () =>
            {
                if (AllSongs.IsEmpty())
                {
                    SortAndSetAllSongs(MusicService.AllSongs.Select(i => i.ToVO()));
                }
                MusicPlayer.SetMusicPlaying(AllSongs, MusicPlayer.CurrentMusic);
                LoadingProgress.Visibility = Visibility.Collapsed;
            });
        }

        private void MusicLibraryDataGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            MusicPlayer.AddNextAndPlay((MusicView) MusicLibraryDataGrid.SelectedItem);
        }

        private void MenuFlyout_Opening(object sender, object e)
        {
            if (MusicLibraryDataGrid.SelectedItems.Count > 1)
                MenuFlyoutHelper.SetPlaylistMenu(sender, null, this);
            else
                MenuFlyoutHelper.SetMusicMenu(sender, null, null, new MenuFlyoutOption() { ShowSelect = false });
        }

        public static IEnumerable<MusicView> SortPlaylist(IEnumerable<MusicView> playlist, SortBy criterion)
        {
            return playlist.OrderBy(i => i.FromVO().GetComparable(criterion));
        }
        public void SortAndSetAllSongs(IEnumerable<MusicView> list)
        {
            SetAllSongs(SortPlaylist(list, Settings.settings.MusicLibraryCriterion));
        }

        private void MusicLibraryDataGrid_Sorting(object sender, DataGridColumnEventArgs e)
        {
            string header = e.Column.Tag?.ToString();
            IEnumerable<MusicView> temp = SortPlaylist(AllSongs, Settings.settings.MusicLibraryCriterion = SortByConverter.FromStr(header));
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
        }

        public void SetAllSongs(IEnumerable<MusicView> songs)
        {
            if (songs == null) return;
            AllSongs.Clear();
            var favSet = PlaylistService.MyFavorites.Songs.Select(i => i.Id).ToHashSet();
            foreach (var item in songs)
            {
                MusicView music = item.Copy();
                music.Favorite = favSet.Contains(item.Id);
                AllSongs.Add(music);
            }
            SetHeader();
        }

        public void OnBuild(MenuFlyoutHelper helper)
        {
            var list = new List<MusicView>();
            foreach (MusicView item in MusicLibraryDataGrid.SelectedItems)
            {
                list.Add(item);
            }
            helper.Data = list;
        }

        public void SetHeader()
        {
            if (MainPage.Instance?.CurrentPage != typeof(MusicLibraryPage))
            {
                return;
            }
            if (Settings.settings.ShowCount)
            {
                MainPage.Instance?.SetHeaderText("AllSongsWithCount", AllSongs.Count);
            }
            else
            {
                MainPage.Instance?.SetHeaderText("AllSongs");
            }
        }

        void IMusicEventListener.Execute(Music music, MusicEventArgs args)
        {
            switch (args.EventType)
            {
                case MusicEventType.Add:
                    AllSongs.InsertWithOrder(music.ToVO());
                    break;
                case MusicEventType.Remove:
                    AllSongs.Remove(music.ToVO());
                    SetHeader();
                    break;
                case MusicEventType.Like:
                    if (AllSongs.FirstOrDefault(m => m.Equals(music)) is MusicView target)
                    {
                        target.Favorite = args.IsFavorite;
                    }
                    break;
                case MusicEventType.Modify:
                    AllSongs.FirstOrDefault(m => m.Equals(music))?.CopyFrom(args.ModifiedMusic.ToVO());
                    break;
            }
        }

        async void IMusicPlayerEventListener.Execute(MusicPlayerEventArgs args)
        {
            switch (args.EventType)
            {
                case MusicPlayerEventType.Switch:
                    await Helper.RunInMainUIThread(Dispatcher, () => MusicPlayer.SetMusicPlaying(AllSongs, args.Music));
                    break;
            }
        }
    }
}

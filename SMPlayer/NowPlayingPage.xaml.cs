using SMPlayer.Controls;
using SMPlayer.Helpers;
using SMPlayer.Interfaces;
using SMPlayer.Models;
using SMPlayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class NowPlayingPage : Page, IMusicPlayerEventListener, IMultiSelectListener
    {
        private string DefaultNewPlaylistName
        {
            get
            {
                var name = Constants.NowPlaying + " - " + DateTime.Now.ToString("yy/MM/dd");
                int index = PlaylistService.FindNextPlaylistNameIndex(name);
                return index == 0 ? name : Helper.GetNextName(name, index);
            }
        }
        private List<MusicView> SelectedItems => NowPlayingPlaylistControl.SelectedItems;
        public NowPlayingPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            MusicPlayer.AddMusicPlayerEventListener(this);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SetEnabled();
            NowPlayingPlaylistControl.ScrollToCurrentMusic();
            MainPage.Instance.SetMultiSelectListener(this);
        }

        private void SetEnabled()
        {
            LocateCurrentButton.Visibility = SaveToButton.Visibility = ClearButton.Visibility = PlayModeButton.Visibility = MultiSelectAppButton.Visibility =
                                            MusicPlayer.CurrentPlaylist.IsEmpty() ? Visibility.Collapsed : Visibility.Visible;
            RandomPlayButton.IsEnabled = MusicService.AllSongs.IsNotEmpty();
            QuickPlayButton.IsEnabled = MusicService.AllSongs.IsNotEmpty();
        }

        private void SaveToButton_Click(object sender, RoutedEventArgs e)
        {
            var helper = new MenuFlyoutHelper
            {
                Data = MusicPlayer.CurrentPlaylist,
                DefaultPlaylistName = DefaultNewPlaylistName,
                CurrentPlaylistName = Constants.NowPlaying
            };
            helper.GetAddToMenuFlyout().ShowAt(sender as FrameworkElement);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            MusicPlayer.Clear();
            SetEnabled();
        }

        private void LocateCurrentButton_Click(object sender, RoutedEventArgs e)
        {
            NowPlayingPlaylistControl.ScrollToCurrentMusic(true);
        }

        private void ShuffleMenuFlyout_Opening(object sender, object e)
        {
            (sender as MenuFlyout).SetTo(MenuFlyoutHelper.GetShuffleMenu(100, SetEnabled));
        }

        private void PlayModeButton_Click(object sender, RoutedEventArgs e)
        {
            (Window.Current.Content as Frame).Navigate(typeof(NowPlayingFullPage));
        }

        private void MultiSelectAppButton_Click(object sender, RoutedEventArgs e)
        {
            NowPlayingPlaylistControl.SelectionMode = ListViewSelectionMode.Multiple;
            MainPage.Instance.ShowMultiSelectCommandBar(new MultiSelectCommandBarOption());
        }

        private void QuickPlayButton_Click(object sender, RoutedEventArgs e)
        {
            MusicPlayer.QuickPlay(100);
        }

        async void IMusicPlayerEventListener.Execute(MusicPlayerEventArgs args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
            {
                if (MainPage.Instance?.CurrentPage == typeof(NowPlayingPage))
                {
                    SetEnabled();
                }
            });
        }

        void IMultiSelectListener.Execute(MultiSelectCommandBar commandBar, MultiSelectEventArgs args)
        {
            switch (args.Event)
            {
                case MultiSelectEvent.Cancel:
                    NowPlayingPlaylistControl.SelectionMode = ListViewSelectionMode.None;
                    break;
                case MultiSelectEvent.AddTo:
                    args.FlyoutHelper.Data = SelectedItems;
                    args.FlyoutHelper.DefaultPlaylistName = DefaultNewPlaylistName;
                    args.FlyoutHelper.CurrentPlaylistName = Constants.NowPlaying;
                    break;
                case MultiSelectEvent.Play:
                    MusicPlayer.SetMusicAndPlay(SelectedItems);
                    break;
                case MultiSelectEvent.Remove:
                    foreach (var item in SelectedItems.OrderByDescending(i => i.Index))
                        MusicPlayer.RemoveMusic(item.Index);
                    break;
                case MultiSelectEvent.SelectAll:
                    NowPlayingPlaylistControl.SelectAll();
                    break;
                case MultiSelectEvent.ReverseSelections:
                    NowPlayingPlaylistControl.ReverseSelections();
                    Helper.GetMainPageContainer()?.GetMultiSelectCommandBar().CountSelections(SelectedItems.Count);
                    break;
                case MultiSelectEvent.ClearSelections:
                    NowPlayingPlaylistControl.ClearSelections();
                    Helper.GetMainPageContainer()?.GetMultiSelectCommandBar().CountSelections(SelectedItems.Count);
                    break;
            }
        }

    }
}

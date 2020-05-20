using SMPlayer.Models;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class NowPlayingPage : Page
    {
        public NowPlayingPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SetEnabled();
            NowPlayingPlaylistControl.ScrollToCurrentMusic();
        }

        private void SetEnabled()
        {
            LocateCurrentButton.IsEnabled = SaveToButton.IsEnabled = ClearButton.IsEnabled = FullScreenButton.IsEnabled = 
                                            MediaHelper.CurrentPlaylist.Count != 0;
            RandomPlayButton.IsEnabled = MusicLibraryPage.SongCount != 0;
        }
        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Instance?.Frame.Navigate(typeof(NowPlayingFullPage));
        }
        private void SaveToButton_Click(object sender, RoutedEventArgs e)
        {
            var name = Helper.Localize("Now Playing") + " - " + DateTime.Now.ToString("yy/MM/dd");
            int index = Settings.settings.FindNextPlaylistNameIndex(name);
            var defaultName = index == 0 ? name : Helper.GetPlaylistName(name, index);
            var helper = new MenuFlyoutHelper() { Data = MediaHelper.CurrentPlaylist, DefaultPlaylistName = defaultName };
            helper.GetAddToMenuFlyout(MenuFlyoutHelper.NowPlaying).ShowAt(sender as FrameworkElement);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            MediaHelper.Clear();
            SetEnabled();
        }

        private void LocateCurrentButton_Click(object sender, RoutedEventArgs e)
        {
            NowPlayingPlaylistControl.ScrollToCurrentMusic(true);
        }

        private async void RandomPlayButton_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Instance.Loader.ShowIndeterminant("ProcessRequest");
            await Task.Run(() =>
            {
                MediaHelper.ShuffleAndPlay(MusicLibraryPage.AllSongs);
            });
            MainPage.Instance.Loader.Hide();
        }

        private void ShuffleMenuFlyout_Opening(object sender, object e)
        {
            (sender as MenuFlyout).SetTo(MenuFlyoutHelper.GetShuffleMenu());
        }
    }
}

using SMPlayer.Models;
using System;
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
    public sealed partial class NowPlayingPage : Page, ISwitchMusicListener
    {
        public NowPlayingPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            MediaHelper.SwitchMusicListeners.Add(this);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SetEnabled();
            NowPlayingPlaylistControl.ScrollToCurrentMusic();
        }

        private void SetEnabled()
        {
            LocateCurrentButton.Visibility = SaveToButton.Visibility = ClearButton.Visibility = PlayModeButton.Visibility = 
                                            MediaHelper.CurrentPlaylist.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
            RandomPlayButton.Visibility = Settings.settings.MusicLibrary.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void SaveToButton_Click(object sender, RoutedEventArgs e)
        {
            var name = Helper.Localize("Now Playing") + " - " + DateTime.Now.ToString("yy/MM/dd");
            int index = Settings.settings.FindNextPlaylistNameIndex(name);
            var defaultName = index == 0 ? name : Helper.GetPlaylistName(name, index);
            var helper = new MenuFlyoutHelper
            {
                Data = MediaHelper.CurrentPlaylist,
                DefaultPlaylistName = defaultName,
                CurrentPlaylistName = MenuFlyoutHelper.NowPlaying
            };
            helper.GetAddToMenuFlyout().ShowAt(sender as FrameworkElement);
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

        private void ShuffleMenuFlyout_Opening(object sender, object e)
        {
            (sender as MenuFlyout).SetTo(MenuFlyoutHelper.GetShuffleMenu(100, SetEnabled));
        }

        private void PlayModeButton_Click(object sender, RoutedEventArgs e)
        {
            (Window.Current.Content as Frame).Navigate(typeof(NowPlayingFullPage));
        }

        private void PreferenceSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Instance.NavigateToPage(typeof(PreferenceSettingsPage));
        }

        async void ISwitchMusicListener.MusicSwitching(Music current, Music next, MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
            {
                if (MainPage.Instance is MainPage main && main.CurrentPage == typeof(NowPlayingPage))
                {
                    SetEnabled();
                }
            });
        }
    }
}

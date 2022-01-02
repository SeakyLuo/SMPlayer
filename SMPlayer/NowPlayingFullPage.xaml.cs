using SMPlayer.Controls;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class NowPlayingFullPage : Page, IMainPageContainer, IMusicRequestListener
    {
        public static NowPlayingFullPage Instance { get => (Window.Current.Content as Frame).Content as NowPlayingFullPage; }
        public NowPlayingFullPage()
        {
            this.InitializeComponent();
            MediaControl.AddMusicRequestListener(this);

            var coreTitleBar = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar;
            // Register a handler for when the size of the overlaid caption control changes.
            // For example, when the app moves to a screen with a different DPI.
            coreTitleBar.LayoutMetricsChanged += (sender, args) => UpdateTitleBarLayout(sender);
            // Register a handler for when the title bar visibility changes.
            // For example, when the title bar is invoked in full screen mode.
            coreTitleBar.IsVisibleChanged += (sender, args) => AppTitleBar.Visibility = sender.IsVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            TitleBarHelper.SetDarkTitleBar();
            Window.Current.SetTitleBar(AppTitleBar);
            UpdateTitleBarLayout(Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar);

            SetMusic(MusicPlayer.CurrentMusic);
            FullPlaylistControl.ScrollToCurrentMusic();
        }
        private void UpdateTitleBarLayout(Windows.ApplicationModel.Core.CoreApplicationViewTitleBar coreTitleBar)
        {
            // Update title bar control size as needed to account for system size changes.
            AppTitleBar.Height = coreTitleBar.Height;
        }

        public void SetMusic(Music music)
        {
            MusicInfoController.SetMusicInfo(music);
            MusicLyricsController.SetLyrics(music);
        }

        public void ShowNotification(string message, int duration = 2000)
        {
            ShowResultInAppNotification.Content = message;
            ShowResultInAppNotification.Show(duration);
        }

        private Action undo;

        public void ShowUndoableNotification(string message, Action undo, int duration = 5000)
        {
            UndoInAppNotification.Content = message;
            this.undo = undo;
            UndoInAppNotification.Show(duration);
        }

        public void ShowLocalizedNotification(string message, int duration = 2000)
        {
            ShowNotification(Helper.LocalizeMessage(message), duration);
        }

        public void PlaylistRequested(ICollection<Music> playlist)
        {
            PlaylistBladeItem.StartBringIntoView(new BringIntoViewOptions() { AnimationDesired = true });
            FullPlaylistControl.ScrollToCurrentMusic();
        }
        public void MusicInfoRequested(Music music)
        {
            MusicInfoController.SetMusicInfo(music);
            MusicPropertyBladeItem.StartBringIntoView(new BringIntoViewOptions() { AnimationDesired = true });
        }

        public void LyricsRequested(Music music)
        {
            MusicLyricsController.SetLyrics(music);
            LyricsBladeItem.StartBringIntoView(new BringIntoViewOptions() { AnimationDesired = true });
        }

        public void GoBack()
        {
            if (Frame.CanGoBack) Frame.GoBack();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            GoBack();
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            undo.Invoke();
            UndoInAppNotification.Dismiss();
        }

        public void ShowMultiSelectCommandBar(MultiSelectCommandBarOption option = null)
        {
            BottomMultiSelectCommandBar.Show(option);
        }

        public MultiSelectCommandBar GetMultiSelectCommandBar()
        {
            return BottomMultiSelectCommandBar;
        }

        public void HideMultiSelectCommandBar()
        {
            BottomMultiSelectCommandBar.Hide();
        }

        public void SetMultiSelectListener(IMultiSelectListener listener)
        {
            BottomMultiSelectCommandBar.MultiSelectListener = listener;
        }

        public MediaElement GetMediaElement()
        {
            return mediaElement;
        }

        public MediaControl GetMediaControl()
        {
            return FullMediaControl;
        }
    }
}

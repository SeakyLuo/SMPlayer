using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SMPlayer.Controls
{
    public sealed partial class MusicLyricsControl : UserControl, SwitchMusicListener
    {
        public bool AllowMusicSwitching { get; set; }
        public bool ShowHeader { get; set; }
        private string Lyrics = "";
        private Music CurrentMusic;
        public MusicLyricsControl()
        {
            this.InitializeComponent();
            MediaHelper.SwitchMusicListeners.Add(this);
        }
        private void ResetLyricsButton_Click(object sender, RoutedEventArgs e)
        {
            LyricsTextBox.Text = Lyrics;
            Helper.GetMediaControlContainer().ShowNotification(Helper.LocalizeMessage("LyricsReset"));
        }

        private async void SaveLyricsButton_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                SavingLyricsProgress.Visibility = Visibility.Visible;
                LyricsTextBox.IsEnabled = false;
                var music = await CurrentMusic.GetStorageFileAsync();
                using (var file = TagLib.File.Create(new MusicFileAbstraction(music), TagLib.ReadStyle.Average))
                {
                    Lyrics = LyricsTextBox.Text;
                    file.Tag.Lyrics = Lyrics;
                    file.Save();
                }
                SavingLyricsProgress.Visibility = Visibility.Collapsed;
                LyricsTextBox.IsEnabled = true;
                Helper.GetMediaControlContainer().ShowNotification(Helper.LocalizeMessage("LyricsUpdated"));
            });
        }

        private async void SearchLyricsButton_Click(object sender, RoutedEventArgs e)
        {
            if (await Windows.System.Launcher.LaunchUriAsync(new Uri("https://cn.bing.com/search?q=lryics+" + $"{CurrentMusic.Name}+{CurrentMusic.Artist}")))
            {

            }
            else
            {
                string notification = Helper.LocalizeMessage("FailToOpenBrowser");
                MainPage.Instance?.ShowNotification(notification);
                NowPlayingFullPage.Instance?.ShowNotification(notification);
            }
        }
        public async void SetLyrics(Music music)
        {
            if (music == null) return;
            CurrentMusic = music;
            var lyrics = await music.GetLyricsAsync();
            LyricsTextBox.Text = string.IsNullOrEmpty(lyrics) ? "" : lyrics;
            Lyrics = LyricsTextBox.Text;
        }

        public async void MusicSwitching(Music current, Music next, MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
            {
                if (!AllowMusicSwitching) return;
                if (Lyrics != LyricsTextBox.Text) return;
                SetLyrics(next);
            });
        }
    }
}

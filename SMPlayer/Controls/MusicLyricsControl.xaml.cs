using SMPlayer.Helpers;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SMPlayer.Controls
{
    public sealed partial class MusicLyricsControl : UserControl, ISwitchMusicListener
    {
        public bool AllowMusicSwitching { get; set; }
        public bool ShowHeader { get; set; }
        public Windows.UI.Xaml.Media.Brush ProgressBarColor
        {
            get => SaveProgress.Foreground;
            set => SaveProgress.Foreground = value;
        }
        private string Lyrics = "";
        private MusicView CurrentMusic;
        public bool IsProcessing { get; private set; } = false;
        public MusicLyricsControl()
        {
            this.InitializeComponent();
            MusicPlayer.AddSwitchMusicListener(this);
        }
        private void ResetLyricsButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing)
            {
                Helper.ShowNotification("ProcessingRequest");
                return;
            }
            SaveProgress.Visibility = Visibility.Visible;
            LyricsTextBox.IsEnabled = false;
            LyricsTextBox.Text = Lyrics;
            SaveProgress.Visibility = Visibility.Collapsed;
            LyricsTextBox.IsEnabled = true;
            Helper.ShowNotification("LyricsReset");
        }

        public void ScrollToTop()
        {
            LyricsTextBox.GetFirstDescendantOfType<ScrollViewer>()?.ChangeView(0, 0, null);
        }

        private async void SaveLyricsButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing)
            {
                Helper.ShowNotification("ProcessingRequest");
                return;
            }
            IsProcessing = true;
            string lyrics = LyricsTextBox.Text;
            if (Lyrics != lyrics)
            {
                SaveProgress.Visibility = Visibility.Visible;
                LyricsTextBox.IsEnabled = false;
                await Task.Run(async () =>
                {
                    await CurrentMusic.SaveLyricsAsync(lyrics);
                    Lyrics = lyrics;
                });
                SaveProgress.Visibility = Visibility.Collapsed;
                LyricsTextBox.IsEnabled = true;
            }
            IsProcessing = false;
            Helper.ShowNotificationRaw(Helper.LocalizeMessage("LyricsUpdated", CurrentMusic.Name));
        }
        private async void SearchLyricsButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing)
            {
                Helper.ShowNotification("ProcessingRequest");
                return;
            }
            SaveProgress.Visibility = Visibility.Visible;
            LyricsTextBox.IsEnabled = false;
            string notification = "";
            try
            {
                string lyrics = "";
                await Task.Run(async () => lyrics = await LyricsHelper.SearchLyrics(CurrentMusic));
                if (lyrics == "") throw new Exception("LyricsNotFound");
                LyricsTextBox.Text = lyrics;
                notification = Helper.LocalizeMessage("SearchLyricsSuccessful");
                ScrollToTop();
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine("MusicLyricsControl: " + exception.Message);
                string uri = Helper.LocalizeMessage("SearchLyricsUrl", Helper.LocalizeMessage("Lyrics"), CurrentMusic.Name, CurrentMusic.Artist);
                if (await Windows.System.Launcher.LaunchUriAsync(new Uri(uri)))
                    notification = Helper.LocalizeMessage("OpenBrowserSuccessful");
                else
                    notification = Helper.LocalizeMessage("SearchLyricsFailed");
            }
            finally
            {
                LyricsTextBox.IsEnabled = true;
                SaveProgress.Visibility = Visibility.Collapsed;
                Helper.ShowNotification(notification);
            }
        }

        public async void SetLyrics(MusicView music)
        {
            if (music == null) return;
            IsProcessing = true;
            CurrentMusic = music;
            try
            {
                var lyrics = await music.GetLyricsAsync();
                LyricsTextBox.Text = string.IsNullOrEmpty(lyrics) ? "" : lyrics;
            }
            catch (IOException)
            {
                Helper.ShowNotification("GetLyricsFailed");
            }
            Lyrics = LyricsTextBox.Text;
            IsProcessing = false;
        }

        public async void MusicSwitching(MusicView current, MusicView next, MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
            {
                if (!AllowMusicSwitching) return;
                if (Lyrics != LyricsTextBox.Text) return;
                ScrollToTop();
                SetLyrics(next);
            });
        }

        private async void ImportLyricsButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.Desktop
            };
            picker.FileTypeFilter.Add(".lrc");
            picker.FileTypeFilter.Add(".txt");
            picker.FileTypeFilter.AddRange(MusicHelper.SupportedFileTypes);
            StorageFile file = await picker.PickSingleFileAsync();
            if (file == null) return;
            IsProcessing = true;
            SaveProgress.Visibility = Visibility.Visible;
            string lyrics;
            try
            {
                lyrics = file.IsMusicFile() ? file.GetLyrics() : await FileIO.ReadTextAsync(file);
                LyricsTextBox.Text = string.IsNullOrEmpty(lyrics) ? "" : lyrics;
            }
            catch (ArgumentOutOfRangeException)
            {
                // 在多字节的目标代码页中，没有此 Unicode 字符可以映射到的字符。
                Helper.ShowNotification("ImportLyricsFailed");
            }
            SaveProgress.Visibility = Visibility.Collapsed;
            IsProcessing = false;
        }
    }
}

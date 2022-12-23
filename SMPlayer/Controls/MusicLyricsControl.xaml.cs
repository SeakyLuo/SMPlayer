using SMPlayer.Helpers;
using SMPlayer.Interfaces;
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
    public sealed partial class MusicLyricsControl : UserControl, IMusicPlayerEventListener
    {
        public bool AllowMusicSwitching { get; set; }
        public bool ShowHeader { get; set; }
        public Windows.UI.Xaml.Media.Brush ProgressBarColor
        {
            get => SaveProgress.Foreground;
            set => SaveProgress.Foreground = value;
        }
        private string Lyrics = "";
        private Music CurrentMusic;
        public bool IsProcessing { get; private set; } = false;
        public MusicLyricsControl()
        {
            this.InitializeComponent();
            MusicPlayer.AddMusicPlayerEventListener(this);
        }
        private void ResetLyricsButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing)
            {
                Helper.ShowNotification("ProcessingRequest");
                return;
            }
            SaveProgress.Visibility = Visibility.Visible;
            SetControlEnablility(false);
            LyricsTextBox.Text = Lyrics;
            SaveProgress.Visibility = Visibility.Collapsed;
            SetControlEnablility(true);
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
            try
            {
                string lyrics = LyricsTextBox.Text;
                if (Lyrics != lyrics)
                {
                    SaveProgress.Visibility = Visibility.Visible;
                    SetControlEnablility(false);
                    await Task.Run(async () =>
                    {
                        await CurrentMusic.SaveLyricsAsync(lyrics);
                        Lyrics = lyrics;
                    });
                }
                Helper.ShowNotificationRaw(Helper.LocalizeMessage("LyricsUpdated", CurrentMusic.Name));
            }
            catch (Exception ex)
            {
                Helper.ShowNotificationRaw(Helper.LocalizeMessage("UpdateFailed", ex.Message), 5000);
            }
            finally
            {
                SaveProgress.Visibility = Visibility.Collapsed;
                SetControlEnablility(true);
                IsProcessing = false;
            }
        }
        private async void SearchLyricsButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing)
            {
                Helper.ShowNotification("ProcessingRequest");
                return;
            }
            SaveProgress.Visibility = Visibility.Visible;
            SetControlEnablility(false);
            string notification = "";
            try
            {
                string lyrics = "";
                await Task.Run(async () => lyrics = await LyricsHelper.SearchLyrics(CurrentMusic));
                if (string.IsNullOrEmpty(lyrics))
                {
                    notification = await OpenBrowser(CurrentMusic);
                }
                else
                {
                    LyricsTextBox.Text = lyrics;
                    notification = Helper.LocalizeMessage("SearchLyricsSuccessful");
                    ScrollToTop();
                }
            }
            catch (Exception ex)
            {
                Log.Warn($"search lryics failed {ex}");
                notification = await OpenBrowser(CurrentMusic);
            }
            SetControlEnablility(true);
            SaveProgress.Visibility = Visibility.Collapsed;
            Helper.ShowNotification(notification);
        }

        private void SetControlEnablility(bool isEnabled)
        {
            LyricsTextBox.IsEnabled 
                = SearchLyricsButton.IsEnabled = ImportLyricsButton.IsEnabled 
                = SaveLyricsButton.IsEnabled = ResetLyricsButton.IsEnabled
                = isEnabled;
        }

        private async Task<string> OpenBrowser(Music music)
        {
            string uri = Helper.LocalizeMessage("SearchLyricsUrl", Helper.LocalizeMessage("Lyrics"), music.Name, music.Artist);
            if (await Windows.System.Launcher.LaunchUriAsync(new Uri(uri)))
                return Helper.LocalizeMessage("OpenBrowserSuccessful");
            else
                return Helper.LocalizeMessage("SearchLyricsFailed");
        }

        public async void SetLyrics(Music music)
        {
            if (music == null) return;
            IsProcessing = true;
            CurrentMusic = music;
            try
            {
                var lyrics = await music.GetLyricsAsync();
                LyricsTextBox.Text = string.IsNullOrEmpty(lyrics) ? "" : lyrics;
            }
            catch (IOException e)
            {
                Log.Warn($"GetLyrics Failed {e}");
                Helper.ShowNotification("GetLyricsFailed");
            }
            Lyrics = LyricsTextBox.Text;
            IsProcessing = false;
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
            SetControlEnablility(false);
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
            SetControlEnablility(true);
            IsProcessing = false;
        }

        async void IMusicPlayerEventListener.Execute(MusicPlayerEventArgs args)
        {
            switch (args.EventType)
            {
                case MusicPlayerEventType.Switch:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        if (!AllowMusicSwitching) return;
                        if (Lyrics != LyricsTextBox.Text) return;
                        ScrollToTop();
                        SetLyrics(args.Music);
                    });
                    break;
            }
        }
    }
}

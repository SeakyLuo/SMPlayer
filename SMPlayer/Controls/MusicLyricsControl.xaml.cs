using SMPlayer.Helpers;
using SMPlayer.Interfaces;
using SMPlayer.Models;
using SMPlayer.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
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
        private string SaveLyricsSnapshotLyrics;
        private Music SaveLyricsSnapshotMusic;
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
            await SaveLyrics(CurrentMusic, LyricsTextBox.Text, false);
        }

        // 歌词控件展示的歌不是当前播放的
        // 或者下一首播放的是当前播放的（待定）
        private static bool ShouldSaveLyricsImmediately(Music music)
        {
            return MusicPlayer.CurrentMusic != music;
            //return MusicPlayer.CurrentMusic != music || MusicPlayer.NextMusic == music;
        }

        private async Task SaveLyrics(Music music, string lyrics, bool SaveLyricsImmediately, bool RefreshLatestLyrics = false)
        {
            if (IsProcessing)
            {
                Helper.ShowNotification("ProcessingRequest");
                return;
            }
            IsProcessing = true;
            bool isLyricsSaved = false;
            try
            {
                if (Lyrics == lyrics)
                {
                    Helper.ShowNotification("NothingChanged");
                }
                else
                {
                    if (SaveLyricsImmediately || Settings.settings.SaveLyricsImmediately || ShouldSaveLyricsImmediately(music))
                    {
                        SaveProgress.Visibility = Visibility.Visible;
                        SetControlEnablility(false);
                        await Task.Run(async () =>
                        {
                            await music.SaveLyricsAsync(lyrics);
                            isLyricsSaved = true;
                        });
                    }
                    else
                    {
                        SaveLyricsSnapshotLyrics = lyrics;
                        SaveLyricsSnapshotMusic = music;
                        Helper.ShowButtonedNotificationRaw(Helper.LocalizeMessage("SaveLyricsLater", music.Name),
                            Helper.LocalizeText("SaveImmediately"),
                            async n => await SaveLyrics(music, lyrics, true));
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.ShowNotificationRaw(Helper.LocalizeMessage("UpdateFailed", ex.Message), 5000);
            }
            finally
            {
                if (isLyricsSaved)
                {
                    Lyrics = lyrics;
                    SaveLyricsSnapshotLyrics = null;
                    SaveLyricsSnapshotMusic = null;
                    if (RefreshLatestLyrics)
                    {
                        Music currentMusic = MusicPlayer.CurrentMusic;
                        Helper.ShowNotificationRaw(Helper.LocalizeMessage("LyricsUpdatedAndRefreshed", music.Name, currentMusic.Name), 5000);
                        SetLyrics(currentMusic);
                    }
                    else
                    {
                        Helper.ShowNotificationRaw(Helper.LocalizeMessage("LyricsUpdated", music.Name));
                    }
                }
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
                    string before = LyricsTextBox.Text;
                    LyricsTextBox.Text = lyrics;
                    if (before == LyricsTextBox.Text)
                    {
                        // TextBox会把\n变成\r。。。所以检测赋值前后的变化
                        notification = "NothingChanged";
                    }
                    else
                    {
                        notification = Helper.LocalizeMessage("SearchLyricsSuccessful");
                        ScrollToTop();
                    }
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
            ScrollToTop();
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

        private void SwitchMusic(MusicPlayerEventArgs args)
        {
            if (!AllowMusicSwitching || args.Music == CurrentMusic)
            {
                return;
            }
            string lyrics = LyricsTextBox.Text;
            // 如果歌词发生变化，且用户没点击保存或者点了保存后又有更新，则通知用户保存
            if (Lyrics != lyrics && (SaveLyricsSnapshotMusic == null || lyrics != SaveLyricsSnapshotLyrics))
            {
                Helper.ShowButtonedNotificationRaw(Helper.LocalizeMessage("PendingSaveLyrics", CurrentMusic.Name),
                    Helper.LocalizeText("SaveImmediately"),
                    async n =>
                    {
                        await SaveLyrics(CurrentMusic, lyrics, true, true);
                        n.Dismiss();
                    },
                    Helper.LocalizeText("DiscardChanges"),
                    n =>
                    {
                        SetLyrics(MusicPlayer.CurrentMusic);
                        n.Dismiss();
                    });
                return;
            }
            SetLyrics(args.Music);
        }

        private async Task SaveLyricsAndClearSnapshot()
        {
            if (SaveLyricsSnapshotMusic == null)
            {
                return;
            }
            // 异步执行，避免阻塞主线程UI
            await Task.Run(async () =>
            {
                // 备份一下，省的其他地方处理太快，在睡着后把这个改了
                Music music = SaveLyricsSnapshotMusic;
                string lryics = SaveLyricsSnapshotLyrics;
                // 睡一会，避免源文件的句柄在保存歌词的时候关闭了
                Thread.Sleep(3000);
                await music.SaveLyricsAsync(lryics);
                if (music == SaveLyricsSnapshotMusic)
                {
                    SaveLyricsSnapshotLyrics = null;
                    SaveLyricsSnapshotMusic = null;
                }
            });
        }

        async void IMusicPlayerEventListener.Execute(MusicPlayerEventArgs args)
        {
            switch (args.EventType)
            {
                case MusicPlayerEventType.Switch:
                    await Helper.RunInMainUIThread(Dispatcher, async () =>
                    {
                        SwitchMusic(args);
                        await SaveLyricsAndClearSnapshot();
                    });
                    break;
            }
        }
    }
}

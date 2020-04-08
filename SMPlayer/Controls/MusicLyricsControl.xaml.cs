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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SMPlayer.Controls
{
    public sealed partial class MusicLyricsControl : UserControl, SwitchMusicListener
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
        public MusicLyricsControl()
        {
            this.InitializeComponent();
            MediaHelper.SwitchMusicListeners.Add(this);
        }
        private void ResetLyricsButton_Click(object sender, RoutedEventArgs e)
        {
            SaveProgress.Visibility = Visibility.Visible;
            LyricsTextBox.IsEnabled = false;
            LyricsTextBox.Text = Lyrics;
            SaveProgress.Visibility = Visibility.Collapsed;
            LyricsTextBox.IsEnabled = true;
            Helper.ShowNotification("LyricsReset");
        }

        public void ScrollToTop()
        {
            var scrollViewers = LyricsTextBox.GetDescendantsOfType<ScrollViewer>();
            if (scrollViewers != null && scrollViewers.Count() > 0) scrollViewers.ElementAt(0).ChangeView(0, 0, null);
            TextBlockScroller.ChangeView(0, 0, null);
        }

        private async void SaveLyricsButton_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                SaveProgress.Visibility = Visibility.Visible;
                LyricsTextBox.IsEnabled = false;
                var music = await CurrentMusic.GetStorageFileAsync();
                using (var file = TagLib.File.Create(new MusicFileAbstraction(music), TagLib.ReadStyle.Average))
                {
                    Lyrics = LyricsTextBox.Text;
                    file.Tag.Lyrics = Lyrics;
                    file.Save();
                }
                SaveProgress.Visibility = Visibility.Collapsed;
                LyricsTextBox.IsEnabled = true;
                Helper.ShowNotification("LyricsUpdated");
            });
        }

        private async void SearchLyricsButton_Click(object sender, RoutedEventArgs e)
        {
            SaveProgress.Visibility = Visibility.Visible;
            LyricsTextBox.IsEnabled = false;
            string notification = "";
            try
            {
                string lyrics = await SearchLyrics(CurrentMusic.Name + " " + CurrentMusic.Artist);
                if (lyrics == "") lyrics = await SearchLyrics(CurrentMusic.Name);
                if (lyrics == "") throw new Exception("LyricsNotFound");
                LyricsTextBox.Text = lyrics;
                notification = Helper.LocalizeMessage("SearchLyricsSuccessful");
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception);
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
                MainPage.Instance?.ShowNotification(notification);
                NowPlayingFullPage.Instance?.ShowNotification(notification);
            }
        }
        public static async Task<string> SearchLyrics(string keyword)
        {
            string uri = $"https://c.y.qq.com/soso/fcgi-bin/client_search_cp?ct=24&qqmusic_ver=1298&new_json=1&remoteplace=sizer.yqq.lyric_next&searchid=63514736641951294&aggr=1&cr=1&catZhida=1&lossless=0&sem=1&t=7&p=1&n=1&w={Uri.EscapeUriString(keyword)}&g_tk=1714057807&loginUin=0&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.json&needNewCode=0";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:22.0) Gecko/20100101 Firefox/22.0");
                var response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                Windows.Data.Json.JsonObject json = Windows.Data.Json.JsonObject.Parse(content);
                var list = json.GetNamedObject("data").GetNamedObject("lyric").GetNamedArray("list");
                if (list.Count == 0) return "";
                var lyrics = list.GetObjectAt(0).GetNamedString("content");
                lyrics = string.Join("\n", new List<string>(lyrics.Replace("\\n", "\n").Replace("<em>", "").Replace("</em>", "")
                                                                  .Split("\n")).ConvertAll(line => line.Trim()));
                return lyrics; 
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
                ScrollToTop();
                SetLyrics(next);
            });
        }
    }
}

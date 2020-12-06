using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace SMPlayer.Helpers
{
    public class LyricsHelper
    {
        private static Music CurrentMusic;

        private static string Lyrics = "";

        private static string CurrentLine;

        private static string DisplayLine = "";

        private static int CurrentIndex = -1;

        private static string[] LyricsList;

        private static bool IsLrc = false;

        public static void ClearLyrics()
        {
            CurrentMusic = null;
            Lyrics = "";
            CurrentLine = null;
            CurrentIndex = -1;
            LyricsList = null;
            IsLrc = false;
        }

        public static void SetLyrics()
        {
            SetLyrics(MediaHelper.CurrentMusic);
        }

        public static async void SetLyrics(Music music)
        {
            if (CurrentMusic == music && !string.IsNullOrEmpty(Lyrics))
                return;
            if (music == null)
            {
                ClearLyrics();
            }
            else
            {
                string lyrics = await music.GetLrcLyricsAsync();
                if (string.IsNullOrEmpty(lyrics))
                {
                    SetLyrics(music, await music.GetLyricsAsync());
                    IsLrc = (bool)(LyricsList?.Take(4).All(l => l.StartsWith("[")));
                }
                else
                {
                    SetLyrics(music, lyrics);
                    IsLrc = true;
                }
            }
        }

        public static void SetLyrics(Music music, string lyrics)
        {
            CurrentMusic = music;
            Lyrics = lyrics;
            CurrentIndex = 0;
            CurrentLine = null;
            DisplayLine = "";
            LyricsList = lyrics?.Split('\n', '\r');
        }

        public static string GetLyrics()
        {
            if (string.IsNullOrEmpty(Lyrics)) return "";
            try
            {
                string lyric = IsLrc ? GetLrcLyrics() : null;
                if (lyric == null)
                {
                    CurrentIndex = Math.Min(LyricsList.Length - 1, (int)(LyricsList.Length * MediaHelper.Progress));
                    lyric = CurrentLine = LyricsList[CurrentIndex];
                }
                if (!string.IsNullOrWhiteSpace(lyric))
                {
                    DisplayLine = lyric;
                }
            }
            catch (Exception e)
            {
                Helper.LogException(e);
            }
            return DisplayLine;
        }

        private static string GetLrcLyrics()
        {
            if (CurrentLine == null) return FindFirstLrcLyrics();
            string time = ToTime(MediaHelper.Position);
            if (CurrentLine.Contains(time)) return DisplayLine;
            int index = LyricsList.FindIndex(l => l.Contains(time));
            if (index == -1) return CurrentLine.StartsWith("[") ? DisplayLine : null;
            CurrentIndex = index;
            CurrentLine = LyricsList[CurrentIndex];
            return TrimTag(CurrentLine);
        }

        private static string FindFirstLrcLyrics()
        {
            double position = MediaHelper.Position;
            while (position >= 0)
            {
                position -= 0.1;
                string time = ToTime(position);
                if (LyricsList.FirstOrDefault(l => l.Contains(time)) is string lyric)
                {
                    CurrentLine = lyric;
                    return TrimTag(CurrentLine);
                }
            }
            return null;
        }

        private static string ToTime(double position)
        {
            return TimeSpan.FromSeconds(position).ToString(@"\[mm\:ss\.f");
        }

        private static string TrimTag(string lyric)
        {
            return lyric.Substring(lyric.LastIndexOf("]") + 1);
        }

        public static async Task<string> SearchLyrics(Music music)
        {
            return await ImproveSearch(music, SearchLyrics);
        }

        public static async Task<string> SearchLyrics(string keyword)
        {
            string uri = $"https://c.y.qq.com/soso/fcgi-bin/client_search_cp?new_json=1&catZhida=1&t=7&p=1&n=1&w={Uri.EscapeUriString(keyword)}&format=json";
            try
            {
                JsonObject json = await GetQQMusicResponse(uri);
                var list = json.GetNamedObject("data").GetNamedObject("lyric").GetNamedArray("list");
                if (list.Count == 0) return "";
                var lyrics = list.GetObjectAt(0).GetNamedString("content");
                lyrics = string.Join("\n", new List<string>(lyrics.Replace("\\n", "\n").Replace("<em>", "").Replace("</em>", "")
                                                                  .Split("\n")).ConvertAll(line => line.Trim()));
                return lyrics;
            }
            catch (Exception)
            {
                return "";
            }
        }

        private static Uri QQMusicLyricsReferer = new Uri("https://y.qq.com/portal/player.html");

        public static async Task<string> SearchLrcLyrics(Music music)
        {
            string songmid = await GetSongMid(music);
            if (string.IsNullOrEmpty(songmid)) return "";
            string uri = $"https://c.y.qq.com/lyric/fcgi-bin/fcg_query_lyric_new.fcg?songmid={songmid}&format=json&nobase64=1";
            try
            {
                JsonObject json = await GetQQMusicResponse(uri, client => client.DefaultRequestHeaders.Referrer = QQMusicLyricsReferer);
                var lyrics = json.GetNamedString("lyric");
                return string.IsNullOrEmpty(lyrics) ? "" : lyrics.Replace("\\n", "\n");
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static async Task<string> GetSongMid(Music music)
        {
            return await ImproveSearch(music, GetSongMid);
        }

        private static async Task<string> GetSongMid(string keyword)
        {
            string uri = $"https://c.y.qq.com/soso/fcgi-bin/client_search_cp?p=1&n=1&w={keyword}&format=json";
            try
            {
                JsonObject json = await GetQQMusicResponse(uri);
                var list = json.GetNamedObject("data").GetNamedObject("song").GetNamedArray("list");
                return list.Count == 0 ? "" : list.GetObjectAt(0).GetNamedString("songmid");
            }
            catch (Exception)
            {
                return "";
            }
        }

        private static async Task<string> ImproveSearch(Music music, Func<string, Task<string>> search)
        {
            string ret = await search.Invoke(music.Name + " " + music.Artist);
            if (string.IsNullOrEmpty(ret))
            {
                string musicName = music.Name.RemoveBraces('(', ')').RemoveBraces('（', '）').
                                              RemoveBraces('《', '》').RemoveBraces('<', '>').
                                              RemoveBraces('[', ']').RemoveBraces('【', '】');
                ret = await search.Invoke(musicName + " " + music.Artist);
                if (ret == "") ret = await search.Invoke(musicName);
            }
            return ret;
        }

        private static async Task<JsonObject> GetQQMusicResponse(string uri, Action<HttpClient> httpClientBuilder = null)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:22.0) Gecko/20100101 Firefox/22.0");
                httpClientBuilder?.Invoke(client);
                var response = await client.GetAsync(new Uri(uri));
                response.EnsureSuccessStatusCode();
                string content = await response.Content.ReadAsStringAsync();
                return JsonObject.Parse(content);
            }
        }
    }
}
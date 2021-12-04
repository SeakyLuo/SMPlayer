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
    public static class LyricsHelper
    {
        private static Music CurrentMusic;

        private static string CurrentLyrics = "";

        private static string CurrentLine;

        private static string DisplayLine = "";

        private static string[] LyricsList;

        private static bool IsLrc = false;

        public static void ClearLyrics()
        {
            CurrentMusic = null;
            CurrentLyrics = "";
            CurrentLine = null;
            LyricsList = null;
            IsLrc = false;
        }

        public static async Task SetLyrics()
        {
            try
            {
                await SetLyrics(MusicPlayer.CurrentMusic);
            }
            catch (ArgumentException e)
            {
                // Value cannot be null. ?????????
                Helper.LogException(e);
            }
            catch (Exception e)
            {
                Helper.LogException(e);
            }
        }

        private static async Task SetLyrics(Music music)
        {
            if (CurrentMusic == music && !string.IsNullOrEmpty(CurrentLyrics))
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
                    lyrics = await music.GetLyricsAsync();
                    lock (CurrentLyrics)
                    {
                        SetLyrics(music, lyrics);
                        IsLrc = LyricsList != null && LyricsList.Take(4).All(l => l.StartsWith("["));
                    }
                }
                else
                {
                    lock (CurrentLyrics)
                    {
                        SetLyrics(music, lyrics);
                        IsLrc = true;
                    }
                }
            }
        }

        private static void SetLyrics(Music music, string lyrics)
        {
            CurrentMusic = music;
            CurrentLyrics = lyrics;
            CurrentLine = null;
            DisplayLine = "";
            LyricsList = lyrics?.Split('\n', '\r');
        }

        public static string GetLyrics()
        {
            if (string.IsNullOrEmpty(CurrentLyrics)) return "";
            try
            {
                string lyric = IsLrc ? GetLrcLyrics() : null;
                if (lyric == null)
                {
                    int index = Math.Min(LyricsList.Length - 1, (int)(LyricsList.Length * MusicPlayer.Progress));
                    lyric = CurrentLine = LyricsList[index];
                }
                if (!string.IsNullOrWhiteSpace(lyric))
                {
                    DisplayLine = TrimTag(lyric);
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
            double position = MusicPlayer.Position;
            string time = ToTime(position);
            if (CurrentLine != null && CurrentLine.Contains(time)) return DisplayLine;
            if (LyricsList.Length == 1)
            {
                CurrentLine = LyricsList[0];
                return TrimTag(CurrentLine);
            }
            while (position >= 0)
            {
                if (LyricsList.FirstOrDefault(l => l.Contains(time)) is string lyric)
                {
                    CurrentLine = lyric;
                    return TrimTag(CurrentLine);
                }
                position -= 0.1;
                time = ToTime(position);
            }
            return CurrentLine != null && CurrentLine.StartsWith("[") ? DisplayLine : null;
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
                return string.Join("\n", new List<string>(System.Web.HttpUtility.HtmlDecode(TrimLyrics(lyrics)).Split("\n")).ConvertAll(line => line.Trim()));
            }
            catch (Exception)
            {
                return "";
            }
        }

        private static string TrimLyrics(string lyrics)
        {
            return lyrics.Replace("<em>", "").Replace("</em>", "").Replace("\\n", "\n");
        }

        public static async Task<string> SearchLrcLyrics(Music music)
        {
            string songmid = await GetSongMid(music);
            if (string.IsNullOrEmpty(songmid)) return "";
            string uri = $"https://c.y.qq.com/lyric/fcgi-bin/fcg_query_lyric_new.fcg?songmid={songmid}&format=json&nobase64=1";
            try
            {
                JsonObject json = await GetQQMusicResponse(uri);
                var lyrics = json.GetNamedString("lyric");
                return string.IsNullOrEmpty(lyrics) ? "" : System.Web.HttpUtility.HtmlDecode(lyrics);
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
            string uri = $"https://c.y.qq.com/soso/fcgi-bin/client_search_cp?p=1&n=1&w={Uri.EscapeUriString(keyword)}&format=json";
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

        private static Uri QQMusicLyricsReferer = new Uri("https://y.qq.com/portal/player.html");

        private static async Task<JsonObject> GetQQMusicResponse(string uri)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:22.0) Gecko/20100101 Firefox/22.0");
                client.DefaultRequestHeaders.Referrer = QQMusicLyricsReferer;
                var response = await client.GetAsync(new Uri(uri));
                response.EnsureSuccessStatusCode();
                string content = await response.Content.ReadAsStringAsync();
                return JsonObject.Parse(content);
            }
        }
    }
}
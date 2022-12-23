using SMPlayer.Models;
using SMPlayer.Services;
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
                Log.Warn($"SetLyrics.ArgumentException {e}");
            }
            catch (Exception e)
            {
                Log.Warn($"SetLyrics.Exception {e}");
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
                    SetLyrics(music, lyrics);
                    IsLrc = LyricsList != null && LyricsList.Take(4).All(l => l.StartsWith("["));
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
                Log.Warn($"GetLyrics Exception {e}");
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
            string lyrics = await GetLrcLyrics(music);
            if (string.IsNullOrEmpty(lyrics) || lyrics.Contains(NoLyricsText)) return "";
            return lyrics.Split('\n').Select(i => TrimTag(i))
                         .Where(i => !string.IsNullOrWhiteSpace(i))
                         .Join("\n");
        }

        private static string TrimLyrics(string lyrics)
        {
            return lyrics.Replace("<em>", "").Replace("</em>", "").Replace("\\n", "\n");
        }

        private const string NoLyricsText = "此歌曲为没有填词的纯音乐，请您欣赏";

        public static async Task<string> SearchLrcLyrics(Music music)
        {
            string lyrics = await GetLrcLyrics(music);
            if (lyrics.Contains(NoLyricsText))
            {
                return Helper.LocalizeMessage("NoMatchingLyrics");
            }
            return lyrics;
        }

        private static async Task<string> GetLrcLyrics(Music music)
        {
            return await ImproveSearch(music, async (keyword, artist) =>
            {
                string songmid = await GetSongMid(keyword, artist);
                if (string.IsNullOrEmpty(songmid)) return "";
                string uri = $"https://c.y.qq.com/lyric/fcgi-bin/fcg_query_lyric_new.fcg?songmid={songmid}&format=json&nobase64=1";
                try
                {
                    JsonObject json = await GetQQMusicResponse(uri);
                    if (json == null || !json.ContainsKey("lyric")) return "";
                    var lyrics = json.GetNamedString("lyric");
                    if (string.IsNullOrEmpty(lyrics)) return "";
                    return System.Web.HttpUtility.HtmlDecode(lyrics);
                }
                catch (Exception e)
                {
                    Log.Warn($"SearchLrcLyrics Exception {e}");
                    return "";
                }
            });
        }

        public static async Task<string> GetSongMid(Music music)
        {
            return await ImproveSearch(music, GetSongMid);
        }

        private static async Task<string> GetSongMid(string keyword, string artist)
        {
            string uri = $"https://c.y.qq.com/splcloud/fcgi-bin/smartbox_new.fcg?cv=4747474&ct=24&format=json&inCharset=utf-8&outCharset=utf-8&notice=0&platform=yqq.json&needNewCode=1&g_tk_new_20200303=1383304704&g_tk=1383304704&hostUin=0&is_xml=0&key={Uri.EscapeUriString(keyword)}";
            try
            {
                JsonObject json = await GetQQMusicResponse(uri);
                var data = json.GetNamedObject("data");
                if (data == null)
                {
                    Log.Info($"[GetSongMid] data is null, keyword {keyword} artist {artist}");
                    return "";
                }
                var song = data.GetNamedObject("song");
                if (song == null)
                {
                    Log.Info($"[GetSongMid] song is null, keyword {keyword} artist {artist}");
                    return "";
                }
                var list = song.GetNamedArray("itemlist");
                if (list.IsEmpty())
                {
                    Log.Info($"[GetSongMid] itemlist is empty, keyword {keyword} artist {artist}");
                    return "";
                }
                uint index = 0;
                if (!string.IsNullOrEmpty(artist))
                {
                    int points = 0;
                    for (uint i = 0; i < list.Count; i++)
                    {
                        var item = list.GetObjectAt(i);
                        string singer = item.GetNamedString("singer");
                        if (singer == null) continue;
                        int eval = SearchHelper.EvaluateString(artist, singer);
                        if (eval > points) index = i;
                    }
                }
                return list.GetObjectAt(index).GetNamedString("mid");
            }
            catch (Exception e)
            {
                Log.Warn($"get song mid failed for keyword {keyword} artist {artist}, Exception {e}");
                return "";
            }
        }

        private static async Task<string> ImproveSearch(Music music, Func<string, string, Task<string>> search)
        {
            string ret = await search.Invoke(music.Name + " " + music.Artist, music.Artist);
            if (!string.IsNullOrEmpty(ret)) return ret;
            ret = await search.Invoke(music.Name + "-" + music.Album, music.Artist);
            if (!string.IsNullOrEmpty(ret)) return ret;
            ret = await search.Invoke(music.Name, music.Artist);
            if (!string.IsNullOrEmpty(ret)) return ret;

            string simpleName = RemoveBraces(music.Name);
            string simpleArtist = RemoveBraces(music.Artist);
            string simpleAlbum = RemoveBraces(music.Album);
            bool diffName = simpleName != music.Name, diffArtist = simpleArtist != music.Artist, diffAlbum = simpleAlbum != music.Album;
            if (diffName) ret = await search.Invoke(simpleName + " " + music.Artist, music.Artist);
            if (!string.IsNullOrEmpty(ret)) return ret;
            if (diffArtist) ret = await search.Invoke(music.Name + " " + simpleArtist, simpleArtist);
            if (!string.IsNullOrEmpty(ret)) return ret;
            if (diffName || diffArtist) ret = await search.Invoke(simpleName + " " + simpleArtist, simpleArtist);
            if (!string.IsNullOrEmpty(ret)) return ret;
            if (diffName || diffArtist || diffAlbum) ret = await search.Invoke(simpleName + "-" + simpleAlbum, simpleArtist);
            if (!string.IsNullOrEmpty(ret)) return ret;
            if (diffName) ret = await search.Invoke(simpleName, music.Artist);
            if (!string.IsNullOrEmpty(ret)) return ret;
            if (diffName || diffArtist) ret = await search.Invoke(simpleName, simpleArtist);
            if (!string.IsNullOrEmpty(ret)) return ret;
            return "";
        }

        private static string RemoveBraces(string text)
        {
            return text.RemoveBraces('(', ')').RemoveBraces('（', '）')
                       .RemoveBraces('《', '》').RemoveBraces('<', '>')
                       .RemoveBraces('[', ']').RemoveBraces('【', '】');
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
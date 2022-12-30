using SMPlayer.Models;
using SMPlayer.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI.Xaml.Media;

namespace SMPlayer.Helpers
{
    public static class LyricsHelper
    {
        private static Music CurrentMusic;

        private static string CurrentLyrics = "";

        private static string CurrentLine;

        private static string DisplayLine = "";

        private static List<string> LyricsList;
        public static LyricsSource Source { get; private set; }

        private static bool IsLrc = false;
        private static long LyricOffset = 0; // millseconds
        private static readonly string QQMusicSearchUrl = "https://c.y.qq.com/splcloud/fcgi-bin/smartbox_new.fcg?cv=4747474&ct=24&format=json&inCharset=utf-8&outCharset=utf-8&notice=0&platform=yqq.json&needNewCode=1&g_tk_new_20200303=1383304704&g_tk=1383304704&hostUin=0&is_xml=0&key={0}";

        private static readonly Regex TiRegex = new Regex("\\[ti:(.*)\\]"),
            ArRegex = new Regex("\\[ar:(.*)\\]"),
            AlRegex = new Regex("\\[al:(.*)\\]"),
            ByRegex = new Regex("\\[by:(.*)\\]"),
            OffsetRegex = new Regex("\\[offset:(.*)\\]"),
            TimeRegex = new Regex("\\[(.+):(.+)\\.(.+)\\].+");

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
                return;
            }
            string lyrics = await music.GetLrcLyricsAsync();
            if (string.IsNullOrEmpty(lyrics))
            {
                lyrics = await SearchLrcLyrics(music);
                if (string.IsNullOrEmpty(lyrics))
                {
                    lyrics = await music.GetLyricsAsync();
                    Source = LyricsSource.Music;
                }
                else
                {
                    Source = LyricsSource.Internet;
                }
            }
            else
            {
                Source = LyricsSource.LrcFile;
            }
            SetLyrics(music, lyrics, Source);
        }

        public static void SetLyrics(Music music, string lyrics, LyricsSource source)
        {
            CurrentMusic = music;
            CurrentLyrics = lyrics;
            CurrentLine = null;
            DisplayLine = "";
            Source = source;
            LyricsList = GenLyricsList(lyrics);
            IsLrc = LyricsList != null && LyricsList.Take(4).All(l => l.StartsWith("["));
        }

        // 改写，方便展示
        private static List<string> GenLyricsList(string lyrics)
        {
            if (string.IsNullOrWhiteSpace(lyrics)) return new List<string>();
            var lines = lyrics.Split('\n', '\r').Where(i => !string.IsNullOrWhiteSpace(i)).ToList();
            if (lines.IsEmpty()) return new List<string>();
            try
            {
                if (GetFromRegex(TiRegex, lines[0]) is string title)
                {
                    lines[0] = $"[00:00.00]{title}";

                    string artist = GetFromRegex(ArRegex, lines[1]);
                    string album = GetFromRegex(AlRegex, lines[2]);
                    string by = GetFromRegex(ByRegex, lines[3]);
                    long.TryParse(GetFromRegex(OffsetRegex, lines[4]), out LyricOffset);
                    long tagTime = (GetMillisecondsOfLine(lines[5]) + LyricOffset) / 4;

                    if (!string.IsNullOrEmpty(artist))
                    {
                        lines[1] = $"[{MillisecondsToString(tagTime)}]{Helper.LocalizeText("LrcLyricsAr")}{artist}";
                    }
                    if (!string.IsNullOrEmpty(album))
                    {
                        lines[2] = $"[{MillisecondsToString(tagTime * 2)}]{Helper.LocalizeText("LrcLyricsAl")}{album}";
                    }
                    if (!string.IsNullOrEmpty(by))
                    {
                        lines[3] = $"[{MillisecondsToString(tagTime * 3)}]{Helper.LocalizeText("LrcLyricsBy")}{by}";
                    }
                }
                else
                {
                    LyricOffset = 0;
                }
            }
            catch (Exception e)
            {
                Log.Warn($"GenLyricsList failed {e}");
                LyricOffset = 0;
            }
            return lines;
        }

        private static long GetMillisecondsOfLine(string line)
        {
            MatchCollection matchCollection = TimeRegex.Matches(line);
            GroupCollection groups = matchCollection[0].Groups;
            return long.Parse(groups[1].Value) * 60000 + long.Parse(groups[2].Value) * 1000 + long.Parse(groups[3].Value);
        }

        private static string MillisecondsToString(long time)
        {
            long ms = time % 1000;
            long seconds = time / 1000;
            long minutes = seconds / 60;
            return $"{minutes:00}:{seconds % 60:00}.{ms}";
        }

        private static string GetFromRegex(Regex regex, string str)
        {
            MatchCollection mc = regex.Matches(str);
            try
            {
                if (mc.IsNotEmpty())
                {
                    return mc[0].Groups[1].Value;
                }
            }
            catch (Exception e)
            {
                Log.Warn($"GetFromRegex Exception {e}");
            }
            return null;
        }

        public static string GetCurrentLyricsLine()
        {
            if (string.IsNullOrEmpty(CurrentLyrics)) return "";
            try
            {
                string lyric = IsLrc ? GetLrcLyrics() : null;
                if (lyric == null)
                {
                    int index = Math.Min(LyricsList.Count - 1, (int)(LyricsList.Count * MusicPlayer.Progress));
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
            double position = MusicPlayer.Position + LyricOffset;
            string time = ToTime(position);
            if (CurrentLine != null && CurrentLine.Contains(time)) return DisplayLine;
            if (LyricsList.Count == 1)
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
                return "";
                //return Helper.LocalizeMessage("NoMatchingLyrics");
            }
            return lyrics;
        }

        private static async Task<string> GetLrcLyrics(Music music)
        {
            return await ImproveSearch(music, async (keyword, title, artist, album) =>
            {
                string songmid = await GetSongMid(keyword, title, artist, album);
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

        private static async Task<string> GetSongMid(string keyword, string title, string artist, string album)
        {
            try
            {
                if (string.IsNullOrEmpty(album))
                {
                    return await SearchByKeyword(keyword, title, artist, album);
                }
                string searchAlbumUrl = string.Format(QQMusicSearchUrl, Uri.EscapeUriString(album));
                JsonObject albumResponse = await GetQQMusicResponse(searchAlbumUrl);
                var albumData = albumResponse.GetNamedObject("data");
                if (albumData == null)
                {
                    Log.Info($"[SearchByAlbum] data is null, title {title} artist {artist} album {album}");
                    return await SearchByKeyword(keyword, title, artist, album);
                }
                var albumJson = albumData.GetNamedObject("album");
                if (albumJson == null)
                {
                    Log.Info($"[SearchByAlbum] album is null, title {title} artist {artist} album {album}");
                    return await SearchByKeyword(keyword, title, artist, album);
                }
                else
                {
                    var albumList = albumJson.GetNamedArray("itemlist");
                    if (albumList.IsEmpty())
                    {
                        Log.Info($"[SearchByAlbum] itemlist is empty, title {title} artist {artist} album {album}");
                        return await SearchByKeyword(keyword, title, artist, album);
                    }
                    uint albumIndex = FindNearestItem(albumList, "singer", artist);
                    string albumMid = albumList.GetObjectAt(albumIndex).GetNamedString("mid");
                    string albumDetailUrl = $"https://c.y.qq.com/v8/fcg-bin/musicmall.fcg?cv=4747474&ct=24&format=json&inCharset=utf-8&outCharset=utf-8&notice=0&platform=yqq.json&needNewCode=1&g_tk_new_20200303=1383304704&g_tk=1383304704&cmd=get_album_buy_page&albummid={albumMid}";
                    JsonObject albumDetailResponse = await GetQQMusicResponse(albumDetailUrl);
                    JsonObject albumDetailData = albumDetailResponse.GetNamedObject("data");
                    if (albumDetailData == null)
                    {
                        Log.Info($"[GetAlbumDetail] data is null, title {title} artist {artist} album {album}");
                        return await SearchByKeyword(keyword, title, artist, album);
                    }
                    JsonArray albumSongs = albumDetailData.GetNamedArray("songlist");
                    if (albumSongs.IsEmpty())
                    {
                        Log.Info($"[GetAlbumDetail] itemlist is empty, title {title} artist {artist} album {album}");
                        return await SearchByKeyword(keyword, title, artist, album);
                    }
                    uint songIndex = FindNearestItem(albumSongs, "songname", title);
                    return albumSongs.GetObjectAt(songIndex).GetNamedString("songmid");
                }
            }
            catch (Exception e)
            {
                Log.Warn($"get song mid failed for title {title} artist {artist} album {album}, Exception {e}");
                return await SearchByKeyword(keyword, title, artist, album);
            }
        }

        private static async Task<string> SearchByKeyword(string keyword, string title, string artist, string album)
        {
            JsonObject response = await GetQQMusicResponse(string.Format(QQMusicSearchUrl, Uri.EscapeUriString(keyword)));
            var data = response.GetNamedObject("data");
            if (data == null)
            {
                Log.Info($"[SearchByTitle] data is null, keyword {keyword} title {title} artist {artist} album {album}");
                return "";
            }
            var song = data.GetNamedObject("song");
            if (song == null)
            {
                Log.Info($"[SearchByTitle] song is null, keyword {keyword} title {title} artist {artist} album {album}");
                return "";
            }
            var list = song.GetNamedArray("itemlist");
            if (list.IsEmpty())
            {
                Log.Info($"[SearchByTitle] itemlist is empty, keyword {keyword} title {title} artist {artist} album {album}");
                return "";
            }
            uint index = FindNearestItem(list, "singer", artist);
            return list.GetObjectAt(index).GetNamedString("mid");
        }

        private static uint FindNearestItem(JsonArray list, string field, string target)
        {
            uint index = 0;
            if (string.IsNullOrEmpty(target))
            {
                return index;
            }
            int points = 0;
            for (uint i = 0; i < list.Count; i++)
            {
                var item = list.GetObjectAt(i);
                string str = item.GetNamedString(field);
                if (str == null) continue;
                int eval = SearchHelper.EvaluateString(target, str);
                if (eval == 100)
                {
                    index = i;
                    break;
                }
                if (eval > points)
                {
                    points = eval;
                    index = i;
                }
            }
            return index;
        }

        private static async Task<string> ImproveSearch(Music music, Func<string, string, string, string, Task<string>> search)
        {
            string ret = await search.Invoke(music.Name + " " + music.Artist, music.Name, music.Artist, music.Album);
            if (!string.IsNullOrEmpty(ret)) return ret;
            ret = await search.Invoke(music.Name, music.Name, music.Artist, music.Album);
            if (!string.IsNullOrEmpty(ret)) return ret;

            string simpleName = RemoveBraces(music.Name);
            string simpleArtist = RemoveBraces(music.Artist);
            string simpleAlbum = RemoveBraces(music.Album);
            bool diffName = simpleName != music.Name, diffArtist = simpleArtist != music.Artist, diffAlbum = simpleAlbum != music.Album;
            if (diffName) ret = await search.Invoke(simpleName + " " + music.Artist, simpleName, music.Artist, music.Album);
            if (!string.IsNullOrEmpty(ret)) return ret;
            if (diffArtist) ret = await search.Invoke(music.Name + " " + simpleArtist, music.Name, simpleArtist, music.Album);
            if (!string.IsNullOrEmpty(ret)) return ret;
            if (diffName || diffArtist) ret = await search.Invoke(simpleName + " " + simpleArtist, simpleName, simpleArtist, music.Album);
            if (!string.IsNullOrEmpty(ret)) return ret;
            if (diffName || diffArtist || diffAlbum) ret = await search.Invoke(simpleName, simpleName, simpleArtist, simpleAlbum);
            if (!string.IsNullOrEmpty(ret)) return ret;
            if (diffName) ret = await search.Invoke(simpleName, simpleName, music.Artist, music.Album);
            if (!string.IsNullOrEmpty(ret)) return ret;
            if (diffName || diffArtist) ret = await search.Invoke(simpleName, simpleName, simpleArtist, music.Album);
            if (!string.IsNullOrEmpty(ret)) return ret;
            return "";
        }

        private static string RemoveBraces(string text)
        {
            return text.RemoveBraces('(', ')').RemoveBraces('（', '）')
                       .RemoveBraces('<', '>')
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
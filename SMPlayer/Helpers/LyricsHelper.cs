using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            string lyric = IsLrc ? GetLrcLyrics() : null;
            if (lyric == null)
            {
                CurrentIndex = (int)(LyricsList.Length * MediaHelper.Progress);
                lyric = CurrentLine = LyricsList[CurrentIndex];
            }
            if (!string.IsNullOrWhiteSpace(lyric))
            {
                DisplayLine = lyric;
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
    }
}
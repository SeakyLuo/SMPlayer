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

        private static string CurrentLine = "";

        private static string DisplayLine = "";

        private static int CurrentIndex = -1;

        private static string[] LyricsList;

        public static void ClearLyrics()
        {
            CurrentMusic = null;
            Lyrics = "";
            CurrentLine = "";
            CurrentIndex = -1;
            LyricsList = null;
        }

        public static void SetLyrics()
        {
            SetLyrics(MediaHelper.CurrentMusic);
        }

        public static async void SetLyrics(Music music)
        {
            if (CurrentMusic == music && !string.IsNullOrEmpty(Lyrics))
                return;
            SetLyrics(music, await music?.GetLyricsAsync());
        }

        public static void SetLyrics(Music music, string lyrics)
        {
            CurrentMusic = music;
            Lyrics = lyrics;
            CurrentIndex = 0;
            CurrentLine = "";
            DisplayLine = "";
            LyricsList = lyrics?.Split('\n', '\r');
        }

        public static string GetLyrics()
        {
            if (string.IsNullOrEmpty(Lyrics)) return "";
            string lyric = GetLrcLyrics();
            //Debug.WriteLine(lyric);
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
            string time = TimeSpan.FromSeconds(MediaHelper.Position).ToString(@"\[mm\:ss\.f");
            if (CurrentLine.StartsWith(time)) return DisplayLine;
            int index = LyricsList.TakeLast(LyricsList.Length - CurrentIndex).FindIndex(l => l.StartsWith(time));
            if (index == -1) return CurrentLine.StartsWith("[") ? DisplayLine : null;
            CurrentIndex += index;
            CurrentLine = LyricsList[CurrentIndex];
            return CurrentLine.Substring(CurrentLine.IndexOf("]") + 1);
        }
    }
}
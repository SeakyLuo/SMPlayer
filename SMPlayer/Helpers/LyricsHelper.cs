using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Helpers
{
    public class LyricsHelper
    {
        private static Music CurrentMusic;

        private static string Lyrics = "";

        private static string[] LyricsList;

        public static void ClearLyrics()
        {
            CurrentMusic = null;
            Lyrics = null;
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
            LyricsList = lyrics?.Split("\n");
        }

        public static string GetLyrics()
        {
            if (string.IsNullOrEmpty(Lyrics)) return "";
            int index = (int)(LyricsList.Length * MediaHelper.Progress);
            //int timer = LyricsList.FindIndex(l => l.StartsWith(MediaHelper.Position.ToString()));
            //if (timer > 0)
            //{
            //    index = timer;
            //}
            string lyrics = LyricsList[index];
            return lyrics;
        }
    }
}
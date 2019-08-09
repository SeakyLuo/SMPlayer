using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Windows.Storage;

namespace SMPlayer.Models
{
    public class MusicManager
    {
        private static readonly string filename = "MusicLibrary.json";
        public static SortedSet<Music> AllSongs = new SortedSet<Music>();
        public static SortedSet<Music> SortedSongs = new SortedSet<Music>();

        public static async void Init()
        {
            AllSongs = JsonFileHelper.Convert<SortedSet<Music>>(await JsonFileHelper.ReadAsync(filename));
        }

        public static void Save()
        {
            JsonFileHelper.SaveAsync(filename, AllSongs);
        }

        public static void Sort(Func<Music, string> lambda, IEnumerable<Music> list)
        {
            SortedSongs.Clear();
            foreach (var music in AllSongs)
                SortedSongs.Add(music);
            SortedSongs.OrderBy(lambda);
        }
    }
}

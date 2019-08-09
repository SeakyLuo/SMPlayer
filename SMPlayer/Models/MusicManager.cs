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
        public static List<Music> AllSongs = new List<Music>();

        public static async void Init()
        {
            AllSongs = JsonFileHelper.Convert<List<Music>>(await JsonFileHelper.ReadAsync(filename));
        }

        public static void Save()
        {
            JsonFileHelper.SaveAsync(filename, AllSongs);
        }
    }
}

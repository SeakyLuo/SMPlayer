using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using SMPlayer.Models;

namespace SMPlayer
{
    class Music
    {
        private string Path { get; set; }
        private string Name { get; set; }
        private string Artist { get; set; }
        private string Album { get; set; }
        private int Duration { get; set; } 

        public static async Task<MusicProperties> GetMusicProperties(string path)
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(path);
            return await file.Properties.GetMusicPropertiesAsync();
        }

        public static async Task<Music> GetMusic(string path)
        {
            return new Music(path, await GetMusicProperties(path));
        }

        public Music(string path, MusicProperties properties)
        {
            Path = path;
            Name = properties.Title;
            Artist = properties.Artist;
            Album = properties.Album;
            Duration = (int)properties.Duration.TotalSeconds;
        }
    }
}

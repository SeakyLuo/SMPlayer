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
    [Serializable]
    public class Music : IComparable<Music>
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public int Duration { get; set; }
        public bool Favorite { get; set; }
        public int PlayedTimes { get; set; }
        public Music() { }
        public Music(Music obj)
        {
            Path = obj.Path;
            Name = obj.Name;
            Artist = obj.Artist;
            Album = obj.Album;
            Duration = obj.Duration;
            Favorite = obj.Favorite;
            PlayedTimes = obj.PlayedTimes;
        }

        public Music(string path, MusicProperties properties)
        {
            Path = path;
            Name = properties.Title;
            Artist = properties.Artist;
            Album = properties.Album;
            Duration = (int)properties.Duration.TotalSeconds;
            Favorite = false;
            PlayedTimes = 0;
        }

        public string GetShortPath()
        {
            return Path.Substring(Settings.settings.RootPath.Length + 1); // Plus one due to "/"
        }

        public static async Task<MusicProperties> GetMusicProperties(string path)
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(path);
            return await file.Properties.GetMusicPropertiesAsync();
        }

        public static async Task<Music> GetMusic(string path)
        {
            return new Music(path, await GetMusicProperties(path));
        }

        int IComparable<Music>.CompareTo(Music other)
        {
            return (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(other.Name)) ? 0 : Name.CompareTo(other.Name);
        }

        public override bool Equals(object obj)
        {
            return Path == (obj as Music).Path;
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }

        public override string ToString()
        {
            return Path;
        }
    }
}

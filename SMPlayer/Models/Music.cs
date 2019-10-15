using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using SMPlayer.Models;
using Windows.Media.Core;
using System.ComponentModel;
using System.IO;
using Windows.Media.Playback;

namespace SMPlayer.Models
{
    [Serializable]
    public class Music : IComparable<Music>, INotifyPropertyChanged
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public int Duration { get; set; }

        private bool isFavorite = false;

        public bool Favorite
        {
            get => isFavorite;
            set
            {
                if (isFavorite != value)
                {
                    isFavorite = value;
                    OnPropertyChanged();
                }
            }
        }

        private int playCount = 0;
        public int PlayCount
        {
            get => playCount;
            set
            {
                playCount = value;
                OnPropertyChanged();
            }
        }

        private bool isPlaying = false;

        [Newtonsoft.Json.JsonIgnore]
        public bool IsPlaying
        {
            get => isPlaying;
            set
            {
                if (isPlaying != value)
                {
                    isPlaying = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public Music() { }
        public Music(Music obj)
        {
            if (obj == null) return;
            CopyFrom(obj);
        }

        public Music(string path, MusicProperties properties)
        {
            Path = path;
            Name = properties.Title;
            Artist = properties.Artist;
            Album = properties.Album;
            Duration = (int)properties.Duration.TotalSeconds;
            Favorite = false;
            PlayCount = 0;
            IsPlaying = false;
        }

        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public Music Copy()
        {
            return new Music(this);
        }

        public void CopyFrom(Music obj)
        {
            Path = obj.Path;
            Name = obj.Name;
            Artist = obj.Artist;
            Album = obj.Album;
            Duration = obj.Duration;
            Favorite = obj.Favorite;
            PlayCount = obj.PlayCount;
            IsPlaying = obj.IsPlaying;
        }

        public void Played()
        {
            if (IsPlaying)
            {
                PlayCount++;
                IsPlaying = false;
            }
        }

        public string GetShortPath()
        {
            return Path.Substring(Settings.settings.RootPath.Length + 1); // Plus one due to "/"
        }

        public async Task<MusicProperties> GetMusicPropertiesAsync()
        {
            var file = await GetStorageFileAsync();
            return await file.Properties.GetMusicPropertiesAsync();
        }

        public static async Task<Music> GetMusicAsync(StorageFile file)
        {
            return new Music(file.Path, await file.Properties.GetMusicPropertiesAsync());
        }

        public static async Task<Music> GetMusicAsync(string source)
        {
            return await GetMusicAsync(await StorageFile.GetFileFromPathAsync(source));
        }

        public static async Task<Music> GetMusicAsync(Uri source)
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(source);
            return new Music(source.AbsolutePath, await file.Properties.GetMusicPropertiesAsync());
        }

        public async Task<StorageFile> GetStorageFileAsync()
        {
            return await StorageFile.GetFileFromPathAsync(Path);
        }

        public async Task<string> GetLyricsAsync()
        {
            var file = await StorageFile.GetFileFromPathAsync(Path);
            using (var tagFile = TagLib.File.Create(new MusicFileAbstraction(file), TagLib.ReadStyle.Average))
            {
                return tagFile.Tag.Lyrics;
            }
        }

        public async Task<MusicDisplayItem> GetMusicDisplayItemAsync()
        {
            var thumbnail = await Helper.GetStorageItemThumbnailAsync(this);
            return thumbnail.IsThumbnail() ? new MusicDisplayItem(thumbnail, await thumbnail.GetDisplayColor(), this) : MusicDisplayItem.DefaultItem;
        }


        public async Task<MediaPlaybackItem> GetMediaPlaybackItemAsync()
        {
            var file = await GetStorageFileAsync();
            var source = MediaSource.CreateFromStorageFile(file);
            source.CustomProperties.Add("Source", this);
            return new MediaPlaybackItem(source);
        }

        int IComparable<Music>.CompareTo(Music other)
        {
            return string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(other.Name) ? 0 : Name.CompareTo(other.Name);
        }

        public static bool operator == (Music music1, Music music2)
        {
            return music1 is null ? music2 is null : music1.Equals(music2);
        }
        public static bool operator != (Music music1, Music music2)
        {
            return !(music1 == music2);
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is Music && Path == (obj as Music).Path;
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

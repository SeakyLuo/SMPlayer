﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using SMPlayer.Models;
using Id3;
using Windows.Media.Core;
using System.ComponentModel;

namespace SMPlayer
{
    [Serializable]
    public class Music : IComparable<Music>, INotifyPropertyChanged
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public int Duration { get; set; }
        public bool Favorite { get; set; }
        public int PlayCount { get; set; }

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
            IsPlaying = false;
        }

        public void Played()
        {
            PlayCount += 1;
        }

        public string GetShortPath()
        {
            return Path.Substring(Settings.settings.RootPath.Length + 1); // Plus one due to "/"
        }

        public static async Task<Music> GetMusic(string source)
        {
            var file = await StorageFile.GetFileFromPathAsync(source);
            return new Music(source, await file.Properties.GetMusicPropertiesAsync());
        }

        public static async Task<Music> GetMusic(Uri source)
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(source);
            return new Music(source.AbsolutePath, await file.Properties.GetMusicPropertiesAsync());
        }

        public string GetLyrics()
        {
            //await Launcher.LaunchUriAsync(new Uri(“ms - settings:appsfeatures - app”));
            return "";
            //System.UnauthorizedAccessException
            //using (var mp3 = new Mp3(Path))
            //{
            //    return mp3.GetTag(Id3TagFamily.Version2X).Lyrics.ToString();
            //}
        }

        int IComparable<Music>.CompareTo(Music other)
        {
            return string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(other.Name) ? 0 : Name.CompareTo(other.Name);
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

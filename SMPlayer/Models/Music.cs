using SMPlayer.Helpers;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;

namespace SMPlayer.Models
{
    [Serializable]
    public class Music : IComparable<Music>, INotifyPropertyChanged, IMusicable, IPreferable
    {
        public long Id { get; set; }
        public string Path { get; set; }
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }
        private string name;
        public string Artist
        {
            get => artist;
            set
            {
                artist = value;
                OnPropertyChanged();
            }
        }
        private string artist;
        public string Album
        {
            get => album;
            set 
            {
                album = value;
                OnPropertyChanged();
            }
        }
        private string album;
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

        public DateTimeOffset DateAdded { get; set; }

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
        [Newtonsoft.Json.JsonIgnore]
        public int Index = -1;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public Music() { }
        public Music(Music obj)
        {
            if (obj == null) return;
            CopyFrom(obj);
        }
        public Music(StorageFile file, MusicProperties properties)
        {
            Path = file.Path;
            Name = properties.Title;
            Artist = properties.Artist;
            Album = properties.Album;
            Duration = (int)properties.Duration.TotalSeconds;
            Favorite = false;
            PlayCount = 0;
            IsPlaying = false;
            DateAdded = file.DateCreated;
        }
        //public Music(string path, MusicProperties properties, TagLib.Tag tag)
        //{
        //    Path = path;
        //    Name = tag.Title;
        //    Artist = tag.JoinedPerformers;
        //    Album = tag.Album;
        //    Duration = (int)properties.Duration.TotalSeconds;
        //    Favorite = false;
        //    PlayCount = 0;
        //    IsPlaying = false;
        //}

        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public Music Copy()
        {
            return new Music(this);
        }

        public Music CopyFrom(Music src)
        {
            Id = src.Id;
            Path = src.Path;
            CopyMusicProperties(src);
            Favorite = src.Favorite;
            PlayCount = src.PlayCount;
            IsPlaying = src.IsPlaying;
            Index = src.Index;
            return src;
        }

        public bool CopyMusicProperties(Music src)
        {
            if (Name == src.Name && Artist == src.Artist && Album == src.Album && Duration == src.Duration && DateAdded == src.DateAdded)
                return false;
            Name = src.Name;
            Artist = src.Artist;
            Album = src.Album;
            Duration = src.Duration;
            DateAdded = src.DateAdded;
            return true;
        }

        public void Played()
        {
            if (IsPlaying)
            {
                PlayCount++;
                IsPlaying = false;
            }
        }

        public static string GetFileFolder(string path)
        {
            //return Path.Substring(Settings.settings.RootPath.Length + 1); // Plus one due to "\"
            return path.Substring(path.LastIndexOf('\\') + 1);
        }

        public async Task<MusicProperties> GetMusicPropertiesAsync()
        {
            var file = await GetStorageFileAsync();
            return await file.Properties.GetMusicPropertiesAsync();
        }

        public static async Task<Music> LoadFromPathAsync(string path)
        {
            return await LoadFromFileAsync(await StorageFile.GetFileFromPathAsync(path));
        }

        public static async Task<Music> LoadFromFileAsync(StorageFile file)
        {
            return new Music(file, await file.Properties.GetMusicPropertiesAsync());
            //using (var tagFile = TagLib.File.Create(new MusicFileAbstraction(file), TagLib.ReadStyle.Average))
            //{
            //    return new Music(file.Path, await file.Properties.GetMusicPropertiesAsync(), tagFile.Tag);
            //}
        }

        public async Task<StorageFile> GetStorageFileAsync()
        {
            try
            {
                return await StorageFile.GetFileFromPathAsync(Path);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        public async Task<TagLib.File> GetTagFileAsync()
        {
            return TagLib.File.Create(new MusicFileAbstraction(await GetStorageFileAsync()), TagLib.ReadStyle.Average);
        }

        public async Task<string> GetLyricsAsync()
        {
            var file = await GetStorageFileAsync();
            return file.GetLyrics();
        }

        public async Task<string> GetLrcLyricsAsync()
        {
            try
            {
                var file = await StorageFile.GetFileFromPathAsync(Path.Substring(0, Path.LastIndexOf(".")) + ".lrc");
                return await FileIO.ReadTextAsync(file);
            }
            catch (Exception)
            {
                return await LyricsHelper.SearchLrcLyrics(this);
            }
        }

        public async Task<bool> SaveLyricsAsync(string lyrics)
        {
            var music = await GetStorageFileAsync();
            try
            {
                using (var file = TagLib.File.Create(new MusicFileAbstraction(music), TagLib.ReadStyle.Average))
                {
                    file.Tag.Lyrics = lyrics;
                    file.Save();
                }
                return true;
            }
            catch (Exception exception)
            {
                Helper.Print($"Exception ({exception.Message}) when saving lyrics for {Name}");
                return false;
            }
        }

        public async Task<MusicDisplayItem> GetMusicDisplayItemAsync()
        {
            var thumbnail = await ImageHelper.LoadThumbnail(Path);
            if (thumbnail.IsThumbnail())
            {
                ImageHelper.CacheImage(Path, thumbnail);
                Brush color = await thumbnail.GetDisplayColor();
                return new MusicDisplayItem(color, this);
            }
            return MusicDisplayItem.DefaultItem;
        }

        public MediaPlaybackItem GetMediaPlaybackItem()
        {
            var source = MediaSource.CreateFromStreamReference(new MusicStream(Path), "audio/mpeg");
            source.CustomProperties.Add("Source", this);
            return new MediaPlaybackItem(source);
        }

        public string MoveToFolder(string newPath)
        {
            return Path = FileHelper.MoveToPath(Path, newPath);
        }

        public string GetToastText()
        {
            return string.IsNullOrEmpty(Artist) ? string.IsNullOrEmpty(Album) ? Name : string.Format("{0} - {1}", Name, Album) :
                                                  string.Format("{0} - {1}", Name, string.IsNullOrEmpty(Artist) ? Album : Artist);
        }

        int IComparable<Music>.CompareTo(Music other)
        {
            int result = Name.CompareTo(other.Name);
            if (result != 0) result = Artist.CompareTo(other.Artist);
            if (result != 0) result = Album.CompareTo(other.Album);
            if (result != 0) result = Path.CompareTo(other.Path);
            return result;
        }

        public bool IsDifferent(Music music)
        {
            return !Equals(music) || Index != music.Index;
        }

        public static bool operator ==(Music music1, Music music2)
        {
            return music1 is null ? music2 is null : music1.Equals(music2);
        }
        public static bool operator !=(Music music1, Music music2)
        {
            return !(music1 == music2);
        }

        public override bool Equals(object obj)
        {
            return obj is Music music && Id == music.Id;
        }

        public bool IndexedEquals(Music music)
        {
            return this == music && Index == music.Index;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return Path;
        }

        Music IMusicable.ToMusic()
        {
            return this;
        }

        PreferenceItem IPreferable.AsPreferenceItem()
        {
            return new PreferenceItem(Id.ToString(), Name);
        }

        PreferenceItemView IPreferable.AsPreferenceItemView()
        {
            return new PreferenceItemView(Id.ToString(), Name, Path, PreferType.Song);
        }
    }

    public class MusicStream : IRandomAccessStreamReference
    {
        private readonly string path;

        public MusicStream(string path)
        {
            this.path = path;
        }

        public IAsyncOperation<IRandomAccessStreamWithContentType> OpenReadAsync() => Open().AsAsyncOperation();

        // private async helper task that is necessary if you need to use await.
        private async Task<IRandomAccessStreamWithContentType> Open()
        {
            //return await (await StorageFile.GetFileFromPathAsync(path)).OpenReadAsync();
            try
            {
                return await (await StorageFile.GetFileFromPathAsync(path)).OpenReadAsync();
            }
            catch (FileNotFoundException)
            {
                Helper.ShowMusicNotFoundNotification(path);
                return await (await StorageFile.GetFileFromPathAsync(path)).OpenReadAsync();
            }
        }
    }

    public interface IMusicable
    {
        Music ToMusic();
    }
}

using SMPlayer.Helpers;
using SMPlayer.Interfaces;
using SMPlayer.Models.VO;
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
    public class MusicView : IComparable, INotifyPropertyChanged, IMusicable, IPreferable, IFolderFile, ISortable
    {
        public long Id { get; set; }
        public string Path { get; set; }
        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged("Name");
                }
            }
        }
        private string name;
        public string Artist
        {
            get => artist;
            set
            {
                if (artist != value)
                {
                    artist = value;
                    OnPropertyChanged("Artist");
                }
            }
        }
        private string artist;
        public string Album
        {
            get => album;
            set
            {
                if (album != value)
                {
                    album = value;
                    OnPropertyChanged("Album");
                }
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
                    OnPropertyChanged("Favorite");
                }
            }
        }

        private int playCount = 0;
        public int PlayCount
        {
            get => playCount;
            set
            {
                if (playCount != value)
                {
                    playCount = value;
                    OnPropertyChanged("PlayCount");
                }
            }
        }

        public DateTimeOffset DateAdded { get; set; }

        private bool isPlaying = false;
        public bool IsPlaying
        {
            get => isPlaying;
            set
            {
                if (isPlaying != value)
                {
                    isPlaying = value;
                    OnPropertyChanged("IsPlaying");
                }
            }
        }
        public int Index { get; set; } = -1;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public MusicView() { }
        public MusicView(MusicView obj)
        {
            if (obj == null) return;
            CopyFrom(obj, false);
        }

        public MusicView(StorageFile file, MusicProperties properties)
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

        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public MusicView Copy()
        {
            return new MusicView(this);
        }

        public MusicView CopyFrom(MusicView src, bool notifyPropertyChange = true)
        {
            Id = src.Id;
            Path = src.Path;
            Index = src.Index;
            if (notifyPropertyChange)
            {
                Name = src.Name;
                Artist = src.Artist;
                Album = src.Album;
                Favorite = src.Favorite;
                PlayCount = src.PlayCount;
                IsPlaying = src.IsPlaying;
            }
            else
            {
                name = src.Name;
                artist = src.Artist;
                album = src.Album;
                isFavorite = src.Favorite;
                playCount = src.PlayCount;
                isPlaying = src.IsPlaying;
            }
            Duration = src.Duration;
            DateAdded = src.DateAdded;
            return src;
        }

        public void Played()
        {
            playCount++;
            isPlaying = false;
        }

        public void SetPlaying(bool IsPlaying)
        {
            if (this.IsPlaying != IsPlaying)
            {
                this.IsPlaying = IsPlaying;
                OnPropertyChanged("IsPlaying");
            }
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

        public bool IsPlayingMusic(Music music)
        {
            return Equals(music) && (Index == -1 || Index == MusicPlayer.CurrentIndex);
        }

        int IComparable.CompareTo(object obj)
        {
            MusicView m = obj as MusicView;
            int result = Name.CompareTo(m.Name);
            if (result != 0) result = Artist.CompareTo(m.Artist);
            if (result != 0) result = Album.CompareTo(m.Album);
            if (result != 0) result = Path.CompareTo(m.Path);
            return result;
        }

        public static bool operator ==(MusicView music1, MusicView music2)
        {
            return music1 is null ? music2 is null : music1.Equals(music2);
        }
        public static bool operator !=(MusicView music1, MusicView music2)
        {
            return !(music1 == music2);
        }

        public override bool Equals(object obj)
        {
            return obj is IMusicable && Id == (obj as IMusicable).ToMusic().Id;
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
            return VOConverter.FromVO(this);
        }

        PreferenceItem IPreferable.AsPreferenceItem()
        {
            return new PreferenceItem(Id.ToString(), Name, EntityType.Song);
        }

        public FolderFile ToFolderFile()
        {
            return new FolderFile
            {
                FileId = Id,
                FileType = FileType.Music,
                Path = Path,
                Source = this
            };
        }

        IComparable ISortable.GetComparable(SortBy criterion)
        {
            return VOConverter.FromVO(this).GetComparable(criterion);
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

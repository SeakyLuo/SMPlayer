using SMPlayer.Helpers;
using SMPlayer.Interfaces;
using SMPlayer.Models.DAO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media;

namespace SMPlayer.Models
{
    public class Music : ISearchEvaluator, IMusicable, IPreferable, IFolderFile, ISortable
    {
        public long Id { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public int Duration { get; set; }
        public int PlayCount { get; set; }
        public DateTimeOffset DateAdded { get; set; }
        public ActiveState State { get; set; } = ActiveState.Active;

        public Music() { }

        public Music(Music src)
        {
            CopyFrom(src);
        }

        public Music(StorageFile file, MusicProperties properties)
        {
            Path = file.Path;
            // Settings.settings可能为null，从日志上分析应该是通过文件启动播放器导致还没来得及加载
            Name = string.IsNullOrWhiteSpace(properties.Title) || (Settings.settings != null && Settings.settings.UseFilenameNotMusicName) ? 
                   file.DisplayName : properties.Title;
            Artist = properties.Artist;
            Album = properties.Album;
            Duration = (int)properties.Duration.TotalSeconds;
            PlayCount = 0;
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

        public Music Copy()
        {
            return new Music(this);
        }

        public Music CopyFrom(Music src)
        {
            Id = src.Id;
            Path = src.Path;
            Name = src.Name;
            Artist = src.Artist;
            Album = src.Album;
            Duration = src.Duration;
            PlayCount = src.PlayCount;
            DateAdded = src.DateAdded;
            State = src.State;
            return src;
        }

        public void Played()
        {
            PlayCount++;
        }

        public MediaPlaybackItem GetMediaPlaybackItem()
        {
            var source = MediaSource.CreateFromStreamReference(new MusicStream(Path), "audio/mpeg");
            source.CustomProperties["Source"] = this;
            return new MediaPlaybackItem(source);
        }

        public async Task<MusicProperties> GetMusicPropertiesAsync()
        {
            var file = await GetStorageFileAsync();
            return await file.Properties.GetMusicPropertiesAsync();
        }

        public static async Task<Music> LoadFromPathAsync(string path)
        {
            return await LoadFromFileAsync(await StorageHelper.LoadFileAsync(path));
        }

        public static async Task<Music> LoadFromFileAsync(StorageFile file)
        {
            if (file == null) return null;
            MusicProperties properties = await file.Properties?.GetMusicPropertiesAsync();
            if (properties == null) return null;
            try
            {
                return new Music(file, properties);
            }
            catch (Exception ex)
            {
                Log.Warn($"create music file failed {ex}");
                return null;
            }
        }

        public async Task<StorageFile> GetStorageFileAsync() => await StorageHelper.LoadFileAsync(Path);
  
        public string RenameFolder(string oldPath, string newPath)
        {
            return Path = Path.Replace(oldPath, newPath);
        }

        public string MoveToFolder(string newPath)
        {
            return Path = StorageHelper.MoveToPath(Path, newPath);
        }

        public string GetToastText()
        {
            return string.IsNullOrEmpty(Artist) ? string.IsNullOrEmpty(Album) ? Name : string.Format("{0} - {1}", Name, Album) :
                                                  string.Format("{0} - {1}", Name, string.IsNullOrEmpty(Artist) ? Album : Artist);
        }

        public double Evaluate(string keyword)
        {
            return SearchHelper.EvaluateString(Name, keyword, -5);
        }

        public double Match(string keyword)
        {
            int basePoints = new List<int> { SearchHelper.EvaluateString(Name, keyword), SearchHelper.EvaluateString(Artist, keyword) - 10,
                                             SearchHelper.EvaluateString(Album, keyword) - 20, 0}.Max();
            return basePoints == 0 ? 0 : basePoints + Math.Min(PlayCount / 10, 10);
        }

        public Music ToMusic()
        {
            return this;
        }

        public override bool Equals(object obj)
        {
            if (obj is Music music)
            {
                if (Id == 0 || music.Id == 0)
                {
                    return Path == music.Path;
                }
                else
                {
                    return Id == music.Id;
                }
            }
            return false;
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        PreferenceItem IPreferable.AsPreferenceItem()
        {
            return new PreferenceItem(Id.ToString(), Name, EntityType.Song);
        }

        public static bool operator ==(Music music1, Music music2)
        {
            return music1 is null ? music2 is null : music1.Equals(music2);
        }
        public static bool operator !=(Music music1, Music music2)
        {
            return !(music1 == music2);
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

        public IComparable GetComparable(SortBy criterion)
        {
            switch (criterion)
            {
                case SortBy.Name:
                case SortBy.Title:
                    return Name;
                case SortBy.Artist:
                    return Artist;
                case SortBy.Album:
                    return Album;
                case SortBy.Duration:
                    return Duration;
                case SortBy.PlayCount:
                    return PlayCount;
                case SortBy.DateAdded:
                    return DateAdded;
                default:
                    return Id;
            }
        }
    }
}

using SMPlayer.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SMPlayer.Models
{
    [Serializable]
    public class Playlist : INotifyPropertyChanged
    {
        public static SortBy[] Criteria = new SortBy[] { SortBy.Title, SortBy.Artist, SortBy.Album, SortBy.Duration, SortBy.PlayCount, SortBy.DateAdded };
        private string name;
        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged();
                }
            }
        }
        public SortBy Criterion { get; set; } = SortBy.Title;

        [Newtonsoft.Json.JsonIgnore]
        public MusicDisplayItem DisplayItem { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public string Artist { get; set; }

        public ObservableCollection<Music> Songs { get; set; }

        public int Count { get => Songs.Count; }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public Playlist() { }

        public Playlist(string Name)
        {
            this.Name = Name;
            this.Songs = new ObservableCollection<Music>();
        }

        public Playlist(string Name, Music music)
        {
            this.Name = Name;
            this.Songs = new ObservableCollection<Music>() { music };
        }

        public Playlist(string Name, IEnumerable<Music> Songs)
        {
            this.Name = Name;
            this.Songs = new ObservableCollection<Music>(Songs);
        }

        public Playlist Duplicate(string NewName)
        {
            return new Playlist(NewName, Songs);
        }

        public void Add(object item)
        {
            if (item is IMusicable musicable)
            {
                Music music = musicable.ToMusic();
                if (Songs.Contains(music))
                {
                    return;
                }
                else
                {

                    Songs.Add(music);
                }
            }
            else if (item is IEnumerable<IMusicable> songs)
            {
                var set = Songs.Select(m => m.Path).ToHashSet();
                bool neverAdded = true;
                foreach (var song in songs)
                {
                    Music music = song.ToMusic();
                    if (!set.Contains(music.Path))
                    {
                        Songs.Add(music);
                        neverAdded = false;
                    }
                }
                if (neverAdded) return;
            }
            else
            {
                return;
            }
            Sort();
        }

        public void Remove(object item)
        {
            if (item is Music targetMusic)
            {
                Songs.Remove(targetMusic);
            }
            else if (item is IMusicable musicable)
            {
                Songs.Remove(musicable.ToMusic());
            }
            else if (item is IEnumerable<Music> songs)
            {
                foreach (var music in songs)
                    Songs.Remove(music);
            }
            else if (item is IEnumerable<IMusicable> musicables)
            {
                foreach (var music in musicables)
                    Songs.Remove(music.ToMusic());
            }
            else if (item is int index)
            {
                Songs.RemoveAt(index);
            }
            else return;
            Sort();
        }

        public bool Contains(Music music)
        {
            return Songs.Contains(music);
        }

        public void Clear()
        {
            Songs.Clear();
        }

        public async Task SetDisplayItemAsync()
        {
            if (DisplayItem != null && !DisplayItem.IsDefault) return;
            foreach (var song in Songs)
            {
                DisplayItem = await song.GetMusicDisplayItemAsync();
                if (!DisplayItem.IsDefault) return;
            }
            DisplayItem = MusicDisplayItem.DefaultItem;
        }

        public async Task<List<MusicDisplayItem>> GetAllDisplayItemsAsync()
        {
            var result = new List<MusicDisplayItem>();
            foreach (var group in Songs.GroupBy(m => m.Album))
            {
                foreach (var song in group)
                {
                    var item = await song.GetMusicDisplayItemAsync();
                    if (!item.IsDefault)
                    {
                        result.Add(item);
                        break;
                    }
                }
            }
            return result;
        }

        public AlbumView ToAlbumView()
        {
            return new AlbumView(Name, Artist)
            {
                Songs = Songs,
                ThumbnailSource = DisplayItem?.Source.Path,
            };
        }

        public AlbumView ToSearchAlbumView()
        {
            return new AlbumView(Name, SongCountConverter.GetSongCount(Count))
            {
                Songs = Songs,
                ThumbnailSource = DisplayItem?.Source.Path,
            };
        }

        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public void SetCriterionAndSort(SortBy criterion)
        {
            if (Criterion == criterion)
            {
                Reverse();
            }
            else
            {
                Criterion = criterion;
                Sort();
            }
        }

        public void Sort()
        {
            List<Music> list;
            switch (Criterion)
            {
                case SortBy.Title:
                    list = Songs.OrderBy(m => m.Name).ToList();
                    break;
                case SortBy.Artist:
                    list = Songs.OrderBy(m => m.Artist).ToList();
                    break;
                case SortBy.Album:
                    list = Songs.OrderBy(m => m.Album).ToList();
                    break;
                case SortBy.Duration:
                    list = Songs.OrderBy(m => m.Duration).ToList();
                    break;
                case SortBy.PlayCount:
                    list = Songs.OrderBy(m => m.PlayCount).ToList();
                    break;
                case SortBy.DateAdded:
                    list = Songs.OrderBy(m => m.DateAdded).ToList();
                    break;
                default:
                    return;
            }
            for (int i = 0; i < Count; i++) Songs[i] = list[i];
            OnPropertyChanged();
        }

        public void Reverse()
        {
            var list = Songs.Reverse().ToList();
            for (int i = 0; i < Count; i++) Songs[i] = list[i];
            OnPropertyChanged();
        }
    }
}


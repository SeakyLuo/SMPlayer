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
    public class Playlist : INotifyPropertyChanged, IPreferable
    {
        public static SortBy[] Criteria = new SortBy[] { SortBy.Title, SortBy.Artist, SortBy.Album, SortBy.Duration, SortBy.PlayCount, SortBy.DateAdded };
        public long Id { get; set; }
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
        public MusicDisplayItem DisplayItem { get; set; }
        public string Artist { get; set; }
        public ObservableCollection<Music> Songs { get; set; } = new ObservableCollection<Music>();
        public int Priority { get; set; }
        public int Count { get => Songs.Count; }
        public bool IsMyFavorite { get => Name == Constants.MyFavorites; }
        public bool IsEmpty { get => Songs.IsEmpty(); }

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
            this.Songs = Songs.IsEmpty() ? new ObservableCollection<Music>() : new ObservableCollection<Music>(Songs);
        }

        public Playlist Duplicate(string newName)
        {
            return new Playlist(newName, Songs);
        }

        public void Add(object item)
        {
            if (item is IMusicable musicable)
            {
                Music music = musicable.ToMusic();
                if (Contains(music))
                {
                    return;
                }
                Songs.Add(music);
            }
            else if (item is IEnumerable<IMusicable> songs)
            {
                var set = Songs.Select(i => i.Id).ToHashSet();
                bool neverAdded = true;
                foreach (var song in songs)
                {
                    Music music = song.ToMusic();
                    if (!set.Contains(music.Id))
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

        public void Remove(IEnumerable<IMusicable> musicables)
        {
            foreach (var music in musicables)
                Songs.Remove(music.ToMusic());
            Sort();
        }

        public void Remove(Music music)
        {
            Songs.Remove(music);
            Sort();
        }

        public void Remove(int index)
        {
            Songs.RemoveAt(index);
            Sort();
        }

        public void RemoveAll(Func<Music, bool> predicate)
        {
            Songs.RemoveAll(i => predicate.Invoke(Settings.FindMusic(i)));
        }

        public bool Contains(Music music)
        {
            return Songs.Contains(music);
        }

        public void Clear()
        {
            Songs.Clear();
        }

        public async Task LoadDisplayItemAsync()
        {
            if (DisplayItem != null && !DisplayItem.IsDefault) return;
            foreach (var song in Songs)
            {
                DisplayItem = await song.GetMusicDisplayItemAsync();
                if (!DisplayItem.IsDefault) return;
            }
            DisplayItem = MusicDisplayItem.DefaultItem;
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
                ThumbnailSource = DisplayItem?.Source?.Path,
            };
        }

        public void CopyFrom(Playlist playlist)
        {
            Name = playlist.Name;
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
            IOrderedEnumerable<Music> list;
            switch (Criterion)
            {
                case SortBy.Title:
                    list = Songs.OrderBy(m => m.Name);
                    break;
                case SortBy.Artist:
                    list = Songs.OrderBy(m => m.Artist);
                    break;
                case SortBy.Album:
                    list = Songs.OrderBy(m => m.Album);
                    break;
                case SortBy.Duration:
                    list = Songs.OrderBy(m => m.Duration);
                    break;
                case SortBy.PlayCount:
                    list = Songs.OrderBy(m => m.PlayCount);
                    break;
                case SortBy.DateAdded:
                    list = Songs.OrderBy(m => m.DateAdded);
                    break;
                default:
                    return;
            }
            Songs.SetTo(list);
        }

        public void Reverse()
        {
            Songs.Reverse();
        }

        PreferenceItem IPreferable.AsPreferenceItem()
        {
            return new PreferenceItem(Id.ToString(), Name);
        }

        PreferenceItemView IPreferable.AsPreferenceItemView()
        {
            return new PreferenceItemView(Id.ToString(), Name, Name, PreferType.Playlist);
        }
    }
}


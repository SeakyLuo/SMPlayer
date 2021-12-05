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
        public ObservableCollection<Music> Songs
        {
            get => new ObservableCollection<Music>(Settings.FindMusicList(SongIds));
        }
        public List<long> SongIds { get; set; }
        public int Count { get => SongIds.Count; }
        public bool IsMyFavorite { get => Name == Constants.MyFavorites; }
        public bool IsEmpty { get => SongIds.Count == 0; }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public Playlist() { }

        public Playlist(string Name)
        {
            this.Name = Name;
            this.SongIds = new List<long>();
        }

        public Playlist(string Name, Music music)
        {
            this.Name = Name;
            this.SongIds = new List<long>() { music.Id };
        }

        public Playlist(string Name, IEnumerable<Music> Songs)
        {
            this.Name = Name;
            this.SongIds = Songs.Select(i => i.Id).ToList();
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
                if (Contains(music))
                {
                    return;
                }
                SongIds.Add(music.Id);
            }
            else if (item is IEnumerable<IMusicable> songs)
            {
                var set = SongIds.ToHashSet();
                bool neverAdded = true;
                foreach (var song in songs)
                {
                    Music music = song.ToMusic();
                    if (!set.Contains(music.Id))
                    {
                        SongIds.Add(music.Id);
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
                SongIds.Remove(music.ToMusic().Id);
            Sort();
        }

        public void Remove(Music music)
        {
            SongIds.Remove(music.Id);
            Sort();
        }

        public void Remove(int index)
        {
            SongIds.RemoveAt(index);
            Sort();
        }

        public void RemoveAll(Func<Music, bool> predicate)
        {
            SongIds.RemoveAll(i => predicate.Invoke(Settings.FindMusic(i)));
        }

        public bool Contains(Music music)
        {
            return SongIds.Contains(music.Id);
        }

        public void Clear()
        {
            SongIds.Clear();
        }

        public async Task LoadDisplayItemAsync()
        {
            if (DisplayItem != null && !DisplayItem.IsDefault) return;
            foreach (var songId in SongIds)
            {
                DisplayItem = await Settings.FindMusic(songId).GetMusicDisplayItemAsync();
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
            IOrderedEnumerable<long> list;
            switch (Criterion)
            {
                case SortBy.Title:
                    list = SongIds.OrderBy(m => Settings.FindMusic(m).Name);
                    break;
                case SortBy.Artist:
                    list = SongIds.OrderBy(m => Settings.FindMusic(m).Artist);
                    break;
                case SortBy.Album:
                    list = SongIds.OrderBy(m => Settings.FindMusic(m).Album);
                    break;
                case SortBy.Duration:
                    list = SongIds.OrderBy(m => Settings.FindMusic(m).Duration);
                    break;
                case SortBy.PlayCount:
                    list = SongIds.OrderBy(m => Settings.FindMusic(m).PlayCount);
                    break;
                case SortBy.DateAdded:
                    list = SongIds.OrderBy(m => Settings.FindMusic(m).DateAdded);
                    break;
                default:
                    return;
            }
            SongIds = list.ToList();
            OnPropertyChanged("Songs");
        }

        public void Reverse()
        {
            SongIds.Reverse();
            OnPropertyChanged("Songs");
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


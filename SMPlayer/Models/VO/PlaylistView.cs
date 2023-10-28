using SMPlayer.Helpers;
using SMPlayer.Models.VO;
using SMPlayer.Services;
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
    public class PlaylistView : INotifyPropertyChanged, IPreferable
    {
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
        public ObservableCollection<MusicView> Songs { get; set; } = new ObservableCollection<MusicView>();
        public int Priority { get; set; }
        public int Count { get => Songs.Count; }
        public EntityType EntityType { get; set; } = EntityType.Playlist;


        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public PlaylistView() { }

        public PlaylistView(string Name)
        {
            this.Name = Name;
            this.Songs = new ObservableCollection<MusicView>();
        }

        public PlaylistView(Artist artist)
        {
            this.Name = artist.Name;
            this.Songs = new ObservableCollection<MusicView>(artist.Songs.Select(i => i.ToVO()));
        }

        public PlaylistView(string Name, MusicView music)
        {
            this.Name = Name;
            this.Songs = new ObservableCollection<MusicView>() { music };
        }

        public PlaylistView(string Name, IEnumerable<MusicView> Songs)
        {
            this.Name = Name;
            this.Songs = Songs.IsEmpty() ? new ObservableCollection<MusicView>() : new ObservableCollection<MusicView>(Songs);
        }

        public PlaylistView Duplicate(string newName)
        {
            return new PlaylistView(newName, Songs);
        }

        public void Add(object item)
        {
            if (item is IMusicable musicable)
            {
                MusicView music = musicable.ToMusic().ToVO();
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
                    MusicView music = song.ToMusic().ToVO();
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
                Songs.Remove(music.ToMusic().ToVO());
        }


        public bool Contains(MusicView music)
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
                ThumbnailSource = DisplayItem?.Path,
                EntityType = EntityType.Playlist,
                OriginalItemId = Id,
            };
        }

        public AlbumView ToSearchAlbumView(EntityType? entityType = null)
        {
            return new AlbumView(Name, SongCountConverter.GetSongCount(Count))
            {
                Songs = Songs,
                ThumbnailSource = DisplayItem?.Path,
                EntityType = entityType ?? EntityType.Playlist,
                OriginalItemId = Id,
            };
        }

        public ArtistView ToArtistView()
        {
            return new ArtistView(Name);
        }

        public void CopyFrom(PlaylistView playlist)
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
            IOrderedEnumerable<MusicView> list;
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
            Songs.SetTo(list.ToList());
        }

        public void Reverse()
        {
            Songs.SetTo(Songs.Reverse().ToList());
        }

        PreferenceItem IPreferable.AsPreferenceItem()
        {
            return new PreferenceItem(Id.ToString(), Name,
                                      Settings.settings.MyFavoritesId == Id ? EntityType.MyFavorites : EntityType.Playlist);
        }
    }
}


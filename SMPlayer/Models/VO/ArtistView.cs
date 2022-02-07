using SMPlayer.Models.VO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace SMPlayer.Models
{
    public class ArtistView : INotifyPropertyChanged, IPreferable, IComparable
    {
        public string Name { get; set; }
        public ObservableCollection<AlbumView> Albums { get; set; } = new ObservableCollection<AlbumView>();
        public bool IsLoading { get; private set; } = false;
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public bool NotLoaded
        {
            get => notLoaded;
            set
            {
                notLoaded = value;
                if (!notLoaded)
                    ArtistInfo = $"{Helper.LocalizeMessage("Albums:")}{Albums.Count} • {Helper.LocalizeMessage("Songs:")}{Songs.Count}";
                OnPropertyChanged();
            }
        }
        private bool notLoaded = true;
        public string ArtistInfo
        {
            get => info;
            set
            {
                info = value;
                OnPropertyChanged();
            }
        }
        private string info = "";
        public List<MusicView> Songs { get => Albums.SelectMany(i => i.Songs).ToList(); }
        public ArtistView(string Name)
        {
            this.Name = Name;
        }
        public ArtistView(MusicView music)
        {
            Name = music.Artist;
            Albums.Add(new AlbumView(music, true));
            Songs.Add(music);
            NotLoaded = false;
        }

        public void Load()
        {
            if (IsLoading) return;
            NotLoaded = true;
            IsLoading = true;
            CopySongs(Settings.AllSongs.Where(m => m.Artist == Name));
            NotLoaded = false;
            IsLoading = false;
        }
        public async Task LoadAsync()
        {
            if (IsLoading) return;
            NotLoaded = true;
            IsLoading = true;
            List<AlbumView> albums = new List<AlbumView>();
            await Task.Run(() =>
            {
                foreach (var group in Settings.AllSongs.Where(m => m.Artist == Name).GroupBy(m => m.Album).OrderBy(g => g.Key))
                    albums.Add(new AlbumView(group.Key, Name, group.OrderBy(m => m.Name), false));
                IsLoading = false;
            });
            Albums.SetTo(albums);
            NotLoaded = false;
        }

        public void AddMusic(MusicView music)
        {
            if (Albums.FirstOrDefault(i => i.Name == music.Album) is AlbumView album)
            {
                album.AddMusic(music);
            }
            else
            {
                Albums.Add(new AlbumView(music));
            }
        }

        public void RemoveMusic(MusicView music)
        {
            if (Albums.FirstOrDefault(i => i.Name == music.Album) is AlbumView album)
            {
                album.RemoveMusic(music);
                if (album.Songs.IsEmpty()) Albums.Remove(album);
            }
        }

        public void CopySongs(IEnumerable<MusicView> songs)
        {
            Albums.Clear();
            foreach (var group in songs.GroupBy(m => m.Album).OrderBy(g => g.Key))
                Albums.Add(new AlbumView(group.Key, Name, group.OrderBy(m => m.Name), false));
        }

        public void CopyFrom(PlaylistView playlist)
        {
            NotLoaded = true;
            Name = playlist.Name;
            CopySongs(playlist.Songs);
            NotLoaded = false;
        }
        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            return obj is ArtistView artist && Name == artist.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        PreferenceItem IPreferable.AsPreferenceItem()
        {
            return new PreferenceItem(Name, Name, EntityType.Artist);
        }

        public int CompareTo(object obj)
        {
            ArtistView artist = obj as ArtistView;
            return Name.CompareTo(artist.Name);
        }
    }

}

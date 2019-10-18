using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models
{
    public class ArtistView : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public ObservableCollection<AlbumView> Albums { get; set; }
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public bool NotLoaded
        {
            get => notLoaded;
            set
            {
                notLoaded = value;
                if (!notLoaded)
                    ArtistInfo = $"{Helper.Localize("Albums:")} {Albums.Count} • {Helper.Localize("Songs:")} {Songs.Count}";
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
        public List<Music> Songs { get; set; }
        public ArtistView(string Name)
        {
            this.Name = Name;
            this.Albums = new ObservableCollection<AlbumView>();
        }

        public ArtistView(string Name, ICollection<Music> Songs)
        {
            this.Name = Name;
            this.Songs = Songs.ToList();
            Albums = new ObservableCollection<AlbumView>();
            foreach (var group in Songs.GroupBy((m) => m.Album).OrderBy((g) => g.Key))
                Albums.Add(new AlbumView(group.Key, Name, group));
            NotLoaded = false;
        }

        public void Load()
        {
            NotLoaded = true;
            Songs = MusicLibraryPage.AllSongs.Where((m) => m.Artist == Name).ToList();
            var groups = Songs.GroupBy((m) => m.Album).OrderBy((g) => g.Key);
            foreach (var group in groups)
                Albums.Add(new AlbumView(group.Key, group.Key, group.OrderBy((m) => m.Name)));
            NotLoaded = false;
        }

        public void CopyFrom(Playlist playlist)
        {
            NotLoaded = true;
            Name = playlist.Artist;
            Songs = playlist.Songs.ToList();
            Albums = new ObservableCollection<AlbumView>();
            foreach (var group in Songs.GroupBy((m) => m.Album).OrderBy((g) => g.Key))
                Albums.Add(new AlbumView(group.Key, Name, group));
            NotLoaded = false;
        }
        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is ArtistView && Name == (obj as ArtistView).Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }

}

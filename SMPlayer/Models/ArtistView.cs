using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace SMPlayer.Models
{
    public class ArtistView : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public ObservableCollection<AlbumView> Albums { get; set; } = new ObservableCollection<AlbumView>();
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public bool NotLoaded
        {
            get => notLoaded;
            set
            {
                notLoaded = value;
                if (!notLoaded)
                    ArtistInfo = $"{Helper.Localize("Albums:")} {Albums.Count} • {Helper.LocalizeMessage("Songs:")} {Songs.Count}";
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
        }

        public ArtistView(string Name, ICollection<Music> Songs)
        {
            this.Name = Name;
            CopySongs(Songs);
            NotLoaded = false;
        }
        private bool loading = false;
        public void Load()
        {
            if (loading) return;
            NotLoaded = true;
            loading = true;
            CopySongs(MusicLibraryPage.AllSongs.Where(m => m.Artist == Name));
            NotLoaded = false;
            loading = false;
        }
        public void CopySongs(IEnumerable<Music> songs)
        {
            foreach (var group in (Songs = songs.ToList()).GroupBy(m => m.Album).OrderBy(g => g.Key))
                if (Albums.FirstOrDefault(a => a.Name == group.Key) == null)
                    Albums.Add(new AlbumView(group.Key, Name, group.OrderBy(m => m.Name)));
        }

        public void CopyFrom(Playlist playlist)
        {
            NotLoaded = true;
            Name = playlist.Artist;
            CopySongs(playlist.Songs.ToList());
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

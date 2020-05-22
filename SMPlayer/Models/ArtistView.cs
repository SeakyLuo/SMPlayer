using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace SMPlayer.Models
{
    public class ArtistView : INotifyPropertyChanged
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
        public List<Music> Songs { get; set; } = new List<Music>();
        public ArtistView(string Name)
        {
            this.Name = Name;
        }
        public ArtistView(Music music)
        {
            Name = music.Artist;
            Albums.Add(new AlbumView(music, true));
            Songs.Add(music);
            NotLoaded = false;
        }
        public ArtistView(string Name, ICollection<Music> Songs)
        {
            this.Name = Name;
            CopySongs(Songs);
            NotLoaded = false;
        }
        public void Load()
        {
            if (IsLoading) return;
            NotLoaded = true;
            IsLoading = true;
            CopySongs(MusicLibraryPage.AllSongs.Where(m => m.Artist == Name));
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
                Songs = MusicLibraryPage.AllSongs.Where(m => m.Artist == Name).ToList();
                foreach (var group in Songs.GroupBy(m => m.Album).OrderBy(g => g.Key))
                    albums.Add(new AlbumView(group.Key, Name, group.OrderBy(m => m.Name), false));
                IsLoading = false;
            });
            Albums.SetTo(albums);
            NotLoaded = false;
        }
        public void CopySongs(IEnumerable<Music> songs)
        {
            Albums.Clear();
            foreach (var group in (Songs = songs.ToList()).GroupBy(m => m.Album).OrderBy(g => g.Key))
                Albums.Add(new AlbumView(group.Key, Name, group.OrderBy(m => m.Name), false));
        }

        public void CopyFrom(Playlist playlist)
        {
            NotLoaded = true;
            Name = playlist.Artist;
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
    }

}

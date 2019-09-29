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
        public bool IsUnloaded
        {
            get => isUnloaded;
            set
            {
                isUnloaded = value;
                OnPropertyChanged();
            }
        }
        private bool isUnloaded = true;
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
        public List<Music> Songs
        {
            get
            {
                List<Music> list = new List<Music>();
                foreach (var album in Albums)
                    list.AddRange(album.Songs.ToList());
                return list;
            }
        }
        public ArtistView(string Name)
        {
            this.Name = Name;
            this.Albums = new ObservableCollection<AlbumView>();
        }

        public ArtistView(string Name, ObservableCollection<AlbumView> Albums)
        {
            this.Name = Name;
            this.Albums = Albums;
        }

        public void Load()
        {
            IsUnloaded = true;
            var songs = MusicLibraryPage.AllSongs.Where((m) => m.Artist == Name);
            var groups = songs.GroupBy((m) => m.Album);
            ArtistInfo = $"Albums: {groups.Count()} • Songs: {songs.Count()}";
            foreach (var group in groups)
                Albums.Add(new AlbumView(group.Key, group.Key, group.OrderBy((m) => m.Name)));
            IsUnloaded = false;
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

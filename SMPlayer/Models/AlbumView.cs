using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.UI.Xaml.Media.Imaging;
using System.Linq;

namespace SMPlayer.Models
{
    public class AlbumView : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Artist { get; set; }
        public ObservableCollection<Music> Songs { get; set; } = new ObservableCollection<Music>();
        public BitmapImage Cover
        {
            get => thumbnail;
            set
            {
                thumbnail = value;
                CoverLoaded = value != null;
                OnPropertyChanged();
            }
        }
        private BitmapImage thumbnail = Helper.DefaultAlbumCover;
        public bool CoverLoaded { get; private set; } = false;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public AlbumView() { }
        public AlbumView(Music music, bool setCover = true)
        {
            Name = music.Album;
            Artist = music.Artist;
            Songs.Add(music);
            if (setCover) SetCover();
        }
        public AlbumView(string name, string artist)
        {
            Name = name;
            Artist = artist;
        }
        public AlbumView(string name, string artist, IEnumerable<Music> songs, bool setCover = true)
        {
            Name = name;
            Artist = artist;
            Songs.SetTo(songs);
            if (setCover) SetCover();
        }
        public async void SetCover()
        {
            if (!CoverLoaded) Cover = await GetAlbumCoverAsync(Songs);
        }
        public static async System.Threading.Tasks.Task<BitmapImage> GetAlbumCoverAsync(ICollection<Music> songs)
        {
            BitmapImage cover;
            foreach (var music in songs)
                if ((cover = await Helper.GetThumbnailAsync(music, false)) != null)
                    return cover;
            return Helper.DefaultAlbumCover;
        }
        public void AddMusic(Music music)
        {
            Songs.Add(music);
            Songs.SetTo(Songs.OrderBy(m => m.Name));
        }
        public Playlist ToPlaylist()
        {
            return new Playlist(Name, Songs)
            {
                Artist = Artist
            };
        }
        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            return obj is AlbumView album && Name == album.Name && Artist == album.Artist;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
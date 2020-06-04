using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.UI.Xaml.Media.Imaging;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace SMPlayer.Models
{
    public class AlbumView : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Artist { get; set; }
        public ObservableCollection<Music> Songs { get; set; } = new ObservableCollection<Music>();
        public BitmapImage Thumbnail
        {
            get => thumbnail;
            set
            {
                thumbnail = value;
                ThumbnailLoaded = value != null;
                OnPropertyChanged();
            }
        }
        private BitmapImage thumbnail = Helper.DefaultAlbumCover;
        public bool ThumbnailLoaded { get; private set; } = false;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public AlbumView() { }
        public AlbumView(Music music, bool setThumbnail = true)
        {
            Name = music.Album;
            Artist = music.Artist;
            Songs.Add(music);
            if (setThumbnail) SetThumbnail();
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
            if (setCover) SetThumbnail();
        }
        public async void SetThumbnail()
        {
            if (ThumbnailLoaded) return;
            Thumbnail = await GetAlbumCoverAsync(Songs);
        }
        public static async Task<BitmapImage> GetAlbumCoverAsync(ICollection<Music> songs)
        {
            foreach (var music in songs)
                if (await Helper.GetThumbnailAsync(music, false) is BitmapImage image)
                    return image;
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
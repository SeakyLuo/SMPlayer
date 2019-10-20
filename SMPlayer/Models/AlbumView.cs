using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;

namespace SMPlayer.Models
{
    public class AlbumView : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Artist { get; set; }
        public ObservableCollection<Music> Songs { get; set; }
        public BitmapImage Cover
        {
            get => thumbnail;
            set
            {
                if (value == null) return;
                thumbnail = value;
                OnPropertyChanged();;
            }
        }
        private BitmapImage thumbnail = null;
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public AlbumView() { }

        public AlbumView(string name, string artist, IEnumerable<Music> songs)
        {
            Name = name;
            Artist = artist;
            Songs = new ObservableCollection<Music>(songs);
            FindThumbnail();
        }
        public async void FindThumbnail()
        {
            foreach (var music in Songs)
                if ((Cover = await Helper.GetThumbnailAsync(music, false)) != null)
                    break;
            if (Cover == null) Cover = Helper.DefaultAlbumCover;
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
            return Name == (obj as AlbumView).Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace SMPlayer.Models
{
    [Serializable]
    public class Playlist : INotifyPropertyChanged
    {
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

        public ObservableCollection<Music> Songs { get; set; }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public Playlist() { }

        public Playlist(string Name)
        {
            this.Name = Name;
            this.Songs = new ObservableCollection<Music>();
        }

        public Playlist(string Name, IEnumerable<Music> Songs)
        {
            this.Name = Name;
            this.Songs = new ObservableCollection<Music>(Songs);
        }

        public Playlist Duplicate(string NewName)
        {
            return new Playlist(NewName, Songs);
        }

        public void Add(Music music)
        {
            Songs.Add(music);
            OnPropertyChanged();
        }

        public void Add(IEnumerable<Music> playlist)
        {
            foreach (var music in playlist) Songs.Add(music);
            OnPropertyChanged();
        }

        public async Task<List<BitmapImage>> GetThumbnails()
        {
            List<BitmapImage> Thumbnails = new List<BitmapImage>();
            foreach (var group in Songs.GroupBy((m) => m.Album))
                Thumbnails.Add(await Helper.GetThumbnail(group.ElementAt(0).Path, true));
            return Thumbnails;
        }
        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}


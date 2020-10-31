using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace SMPlayer.Models
{
    public class GridMusicView : INotifyPropertyChanged, IMusicable
    {
        public string Name { get; set; }
        public string Artist { get; set; }
        public BitmapImage Thumbnail
        {
            get => thumbnail;
            set
            {
                if (value == null) return;
                thumbnail = value;
                thumbnailLoaded = true;
                OnPropertyChanged();
            }
        }
        private BitmapImage thumbnail = MusicImage.DefaultImage;
        private bool thumbnailLoaded = false;
        public Music Source
        {
            get => source;
            set
            {
                source = value;
                Name = value.Name;
                Artist = value.Artist;
                OnPropertyChanged();
            }
        }
        private Music source;
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public GridMusicView(Music music)
        {
            Source = music;
        }
        public async void SetThumbnail()
        {
            if (thumbnailLoaded) return;
            Thumbnail = await ImageHelper.LoadImage(Source);
        }
        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is GridMusicView && Source.Path == (obj as GridMusicView).Source.Path;
        }

        public override int GetHashCode()
        {
            return Source.Path.GetHashCode();
        }

        Music IMusicable.ToMusic()
        {
            return source;
        }
    }
}

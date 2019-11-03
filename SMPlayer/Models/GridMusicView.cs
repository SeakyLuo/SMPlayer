using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace SMPlayer.Models
{
    public class GridMusicView : INotifyPropertyChanged
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
        private BitmapImage thumbnail = Helper.DefaultAlbumCover;
        private bool thumbnailLoaded = false;
        public Music Source { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public GridMusicView(Music music)
        {
            Name = music.Name;
            Artist = music.Artist;
            Source = music;
        }
        public async void SetThumbnail()
        {
            if (!thumbnailLoaded) Thumbnail = await Helper.GetThumbnailAsync(Source);
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
    }
}

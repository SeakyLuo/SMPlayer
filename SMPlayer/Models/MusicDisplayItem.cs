using SMPlayer.Helpers;
using System.Threading.Tasks;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace SMPlayer.Models
{
    public class MusicDisplayItem
    {
        public Music Source { get; private set; }
        public string Path { get => Source.Path; }
        public Brush Color { get; private set; }
        public bool IsDefault { get; private set; }

        public static MusicDisplayItem DefaultItem = new MusicDisplayItem(ColorHelper.HighlightBrush);

        public MusicDisplayItem(Brush color, Music music)
        {
            Color = color;
            Source = music;
            IsDefault = false;
        }

        private MusicDisplayItem(Brush color)
        {
            Color = color;
            IsDefault = true;
        }

        public static bool IsNullOrEmpty(MusicDisplayItem item)
        {
            return item == null || item.Color == null;
        }

        public async Task<BitmapImage> GetThumbnailAsync()
        {
            return Source == null ? MusicImage.DefaultImage : await ImageHelper.LoadImage(Source);
        }
    }
}

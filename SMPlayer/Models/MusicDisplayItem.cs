using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace SMPlayer.Models
{
    public class MusicDisplayItem
    {
        public BitmapImage Thumbnail { get; private set; }
        public Brush Color { get; private set; }
        public bool IsDefault { get; private set; }
        public Music Source { get; private set; }

        public static MusicDisplayItem DefaultItem = new MusicDisplayItem(MusicImage.DefaultImage, ColorHelper.HighlightBrush);

        public MusicDisplayItem(StorageItemThumbnail thumbnail, Brush color, Music music)
        {
            Thumbnail = thumbnail.GetBitmapImage();
            Color = color;
            Source = music;
            IsDefault = false;
        }

        public MusicDisplayItem(BitmapImage bitmap, Brush color, Music music)
        {
            Thumbnail = bitmap;
            Color = color;
            Source = music;
            IsDefault = false;
        }

        private MusicDisplayItem(BitmapImage bitmap, Brush color)
        {
            Thumbnail = bitmap;
            Color = color;
            IsDefault = true;
        }

        public static bool IsNullOrEmpty(MusicDisplayItem item)
        {
            return item == null || item.Thumbnail == null || item.Color == null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace SMPlayer.Models
{
    public class MusicImage
    {
        public const string DefaultImagePath = "ms-appx:///Assets/monotone_bg_wide.png",
                            NotFoundImagePath = "ms-appx:///Assets/colorful_bg_wide.png";
        public static volatile BitmapImage DefaultImage = new BitmapImage(new Uri(DefaultImagePath));
        public static volatile BitmapImage NotFound = new BitmapImage(new Uri(NotFoundImagePath));

        public static MusicImage Default { get => new MusicImage("", DefaultImage);  }
        public string Path;
        public BitmapImage Image;

        public MusicImage(string path, BitmapImage image)
        {
            Path = path;
            Image = image;
        }
    }
}

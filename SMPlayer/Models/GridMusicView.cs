using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace SMPlayer.Models
{
    public class GridMusicView
    {
        public string Name { get; set; }
        public string Artist { get; set; }
        public BitmapImage Thumbnail { get; set; }
    }
}

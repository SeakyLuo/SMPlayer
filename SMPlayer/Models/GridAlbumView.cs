using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace SMPlayer.Models
{
    class GridAlbumView
    {
        public string Name { get; set; }
        public string Artist { get; set; }
        public BitmapImage Cover { get; set; }
        public GridAlbumView(string Name, string Artist, BitmapImage Cover)
        {
            this.Name = Name;
            this.Artist = Artist;
            this.Cover = Cover;
        }
    }
}
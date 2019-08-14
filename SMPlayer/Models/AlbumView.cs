using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace SMPlayer.Models
{
    class AlbumView
    {
        public string Name { get; set; }
        public string Artist { get; set; }
        public BitmapImage Cover { get; set; }

        public List<Music> Songs { get; set; }
        public AlbumView(string Name, string Artist, BitmapImage Cover, List<Music> Songs)
        {
            this.Name = string.IsNullOrEmpty(Name) ? "Unknown Album" : Name;
            this.Artist = string.IsNullOrEmpty(Artist) ? "Unknown Artist" : Artist;
            this.Cover = Cover;
            this.Songs = Songs;
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
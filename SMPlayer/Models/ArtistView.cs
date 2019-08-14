using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace SMPlayer.Models
{
    class ArtistView
    {
        public string Name { get; set; }
        public List<AlbumView> Albums { get; set; }

        public ArtistView(string Name, List<AlbumView> Albums)
        {
            this.Name = Name;
            this.Albums = Albums;
        }

        public override bool Equals(object obj)
        {
            return Name == (obj as ArtistView).Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }

}

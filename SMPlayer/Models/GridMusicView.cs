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

        public GridMusicView() { }

        public async Task Init(Music music)
        {
            Name = music.Name;
            Artist = music.Artist;
            Thumbnail = await Helper.GetThumbnail(music.Path);
        }

        public override bool Equals(object obj)
        {
            return Name == (obj as GridFolderView).Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}

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
        public Music Source { get; private set; }

        public GridMusicView() { }

        public async Task Init(Music music)
        {
            Name = music.Name;
            Artist = string.IsNullOrEmpty(music.Artist) ? "Unknown Artist" : music.Artist;
            Thumbnail = await Helper.GetThumbnail(music.Path);
            Source = music;
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is GridMusicView && Name == (obj as GridFolderView).Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}

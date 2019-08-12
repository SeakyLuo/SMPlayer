using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace SMPlayer.Models
{
    public class GridFolderView
    {
        public string Name;
        public string MusicCount;
        public BitmapImage First, Second, Third, Fourth;
        public GridFolderView() { }

        public async void Init(FolderTree tree)
        {
            List<BitmapImage> thumbnails = new List<BitmapImage>();
            var folder = await StorageFolder.GetFolderFromPathAsync(tree.Path);
            Name = folder.DisplayName;
            MusicCount = "Songs: " + tree.Files.Count;
            BitmapImage thumbnail;
            foreach (var music in tree.Files)
            {
                var file = await StorageFile.GetFileFromPathAsync(music.Path);
                thumbnail = await Helper.GetThumbnail(file);
                if (thumbnail != null && !thumbnail.Equals(Helper.DefaultAlbumCover))
                    thumbnails.Add(thumbnail);
                if (thumbnails.Count == 4) break;
            }
            for (int i = 0; i < 4 - thumbnails.Count; i++)
                thumbnails.Add(Helper.DefaultAlbumCover);
            First = thumbnails[0];
            Second = thumbnails[1];
            Third = thumbnails[2];
            Fourth = thumbnails[3];
        }

        public async void SwitchThumbnail()
        {
            await Helper.GetThumbnail(null);
        }
    }
}

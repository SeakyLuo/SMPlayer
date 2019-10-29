using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace SMPlayer.Models
{
    class GridFolderView
    {
        public string Name { get; set; }
        public string FolderInfo { get; set; }
        public BitmapImage First { get; set; }
        public BitmapImage Second { get; set; }
        public BitmapImage Third { get; set; }
        public BitmapImage Fourth { get; set; }
        public BitmapImage LargeThumbnail { get; set; }
        public FolderTree Tree { get; private set; }
        public List<Music> Songs
        {
            get => songs ?? (songs = Tree.Flatten());
        }
        private List<Music> songs;
        public GridFolderView() { }

        public async Task Init(FolderTree tree)
        {
            Tree = tree;
            List<BitmapImage> thumbnails = new List<BitmapImage>(4);
            var folder = await StorageFolder.GetFolderFromPathAsync(tree.Path);
            Name = folder.DisplayName;
            var songs = tree.Flatten();
            FolderInfo = Helper.LocalizeMessage("Songs:") + songs.Count;
            if (tree.Trees.Count > 0) FolderInfo = Helper.LocalizeMessage("Folders:") + tree.Trees.Count + " • " + FolderInfo;
            BitmapImage thumbnail;
            foreach (var music in songs.OrderBy((m) => m.Name))
            {
                thumbnail = await Helper.GetThumbnailAsync(music, false);
                if (thumbnail != null) thumbnails.Add(thumbnail);
                if (thumbnails.Count == 4) break;
            }
            int count = thumbnails.Count;
            if (count == 0) LargeThumbnail = Helper.ThumbnailNotFoundImage;
            else if (count == 1) LargeThumbnail = thumbnails[0];
            else
            {
                for (int i = 0; i < 4 - count; i++)
                    thumbnails.Add(Helper.ThumbnailNotFoundImage);
                First = thumbnails[0];
                Second = thumbnails[1];
                Third = thumbnails[2];
                Fourth = thumbnails[3];
            }
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is GridFolderView && Name == (obj as GridFolderView).Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}

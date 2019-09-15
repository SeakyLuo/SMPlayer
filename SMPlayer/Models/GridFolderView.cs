﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace SMPlayer.Models
{
    class GridFolderView
    {
        public string Name { get; set; }
        public string MusicCount { get; set; }
        public BitmapImage First { get; set; }
        public BitmapImage Second { get; set; }
        public BitmapImage Third { get; set; }
        public BitmapImage Fourth { get; set; }
        public BitmapImage LargeThumbnail { get; set; }
        public List<Music> Songs
        {
            get
            {
                if (songs == null)
                    songs = Tree.Flatten();
                return songs;
            }
        }
        private List<Music> songs;
        private FolderTree Tree;
        public GridFolderView() { }

        public async Task Init(FolderTree tree)
        {
            Tree = tree;
            List<BitmapImage> thumbnails = new List<BitmapImage>(4);
            var folder = await StorageFolder.GetFolderFromPathAsync(tree.Path);
            Name = folder.DisplayName;
            MusicCount = "Songs: " + tree.Files.Count;
            BitmapImage thumbnail;
            foreach (var music in tree.Files.OrderBy((m) => m.Name))
            {
                thumbnail = await Helper.GetThumbnailAsync(music.Path, false);
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

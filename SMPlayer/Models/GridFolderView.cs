using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;

namespace SMPlayer.Models
{
    public class GridFolderView : INotifyPropertyChanged
    {
        public string Name { get => Tree.Directory; }
        public string FolderInfo { get => Tree.Info.ToString(); }
        public BitmapImage First
        {
            get => first;
            set
            {
                first = value;
                OnPropertyChanged();
            }
        }
        public BitmapImage Second
        {
            get => second;
            set
            {
                second = value;
                OnPropertyChanged();
            }
        }
        public BitmapImage Third
        {
            get => third;
            set
            {
                third = value;
                OnPropertyChanged();
            }
        }
        public BitmapImage Fourth
        {
            get => fourth;
            set
            {
                fourth = value;
                OnPropertyChanged();
            }
        }
        public BitmapImage LargeThumbnail
        {
            get => large;
            set
            {
                large = value;
                OnPropertyChanged();
            }
        }
        private BitmapImage first, second, third, fourth, large = MusicImage.NotFound;
        public FolderTree Tree
        {
            get => folderTree;
            set
            {
                folderTree.CopyFrom(value);
                songs = value.Flatten();
                OnPropertyChanged();
            }
        }
        private FolderTree folderTree;
        public List<Music> Songs { get => songs ?? (songs = Tree.Flatten()); }
        private List<Music> songs;
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public bool ThumbnailLoaded { get; private set; } = false;
        public bool IsThumbnailLoading { get; private set; } = false;

        public GridFolderView(FolderTree tree)
        {
            folderTree = tree;
        }

        public async Task SetThumbnailAsync()
        {
            if (IsThumbnailLoading) return;
            IsThumbnailLoading = true;
            List<BitmapImage> thumbnails = new List<BitmapImage>(4);
            async Task<bool> addThumbnail(List<Music> src)
            {
                foreach (var group in src.GroupBy(m => m.Album))
                {
                    foreach (var music in group)
                    {
                        if (await ImageHelper.LoadImage(music) is BitmapImage thumbnail)
                        {
                            thumbnails.Add(thumbnail);
                            break;
                        }
                    }
                    if (thumbnails.Count == 4)
                        return true;
                }
                return false;
            }
            if (!await addThumbnail(Tree.Files))
                foreach (var tree in Tree.Trees)
                    if (await addThumbnail(tree.Flatten()))
                        break;
            int count = thumbnails.Count;
            if (count == 0) LargeThumbnail = MusicImage.NotFound;
            else if (count <= 2) LargeThumbnail = thumbnails[0];
            else
            {
                for (int i = 0; i < 4 - count; i++)
                    thumbnails.Add(MusicImage.NotFound);
                First = thumbnails[0];
                Second = thumbnails[1];
                Third = thumbnails[2];
                Fourth = thumbnails[3];
                LargeThumbnail = null;
            }
            IsThumbnailLoading = false;
            ThumbnailLoaded = true;
        }

        public void Rename(string newPath)
        {
            Tree.Rename(newPath);
            OnPropertyChanged("Name");
        }


        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            return obj is GridFolderView item && Name == item.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace SMPlayer.Models
{
    class GridFolderView : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string FolderInfo { get; set; }
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

        private BitmapImage first, second, third, fourth, large;
        public FolderTree Tree { get; private set; }
        public List<Music> Songs
        {
            get => songs ?? (songs = Tree.Flatten());
        }
        private List<Music> songs;
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private bool thumbnaiLoaded = false;
        public GridFolderView(FolderTree tree)
        {
            Tree = tree;
            Name = tree.Directory;
            FolderInfo = tree.Info.Info;
        }

        public async void SetThumbnail()
        {
            if (thumbnaiLoaded) return;
            List<BitmapImage> thumbnails = new List<BitmapImage>(4);
            BitmapImage thumbnail;
            foreach (var group in Tree.Flatten().GroupBy((m) => m.Album))
            {
                foreach (var music in group)
                {
                    thumbnail = await Helper.GetThumbnailAsync(music, false);
                    if (thumbnail != null)
                    {
                        thumbnails.Add(thumbnail);
                        break;
                    }
                }
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
            thumbnaiLoaded = true;
        }
        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
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

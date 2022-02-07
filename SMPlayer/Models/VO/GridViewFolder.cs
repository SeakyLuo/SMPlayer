using SMPlayer.Models.VO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;

namespace SMPlayer.Models.VO
{
    public class GridViewFolder : GridViewStorageItem, IComparable
    {
        public override long Id => Source.Id;
        public override string Name => Source.Name;
        public override string Info => Source.Info.ToString();
        public override string PlayButtonToolTip => Helper.LocalizeText("GridViewMusicPlayInfo", Name);
        public override bool IsPlaying
        {
            get => false;
            set { }
        }
        public override string TypeIcon => "ms-appx:///Assets/folder.png";
        public override bool ShowTypeIcon => true;

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
        private BitmapImage first, second, third, fourth;
        public FolderTree Source
        {
            get => source;
            set
            {
                source = value;
                OnPropertyChanged("Info");
            }
        }
        private FolderTree source { get; set; }
        public List<MusicView> Songs { get => Source.Flatten(); }
        private bool HasNoThumbnail
        {
            get => (thumbnail == null || thumbnail == MusicImage.NotFound) && first == null;
        }

        public GridViewFolder(FolderTree folder)
        {
            Path = folder.Path;
            Type = StorageType.Folder;
            Source = folder;
            thumbnail = MusicImage.NotFound;
        }

        public override async Task LoadThumbnailAsync()
        {
            if (IsThumbnailLoading) return;
            IsThumbnailLoading = true;
            List<BitmapImage> thumbnails = new List<BitmapImage>(4);
            if (!await AddThumbnail(thumbnails, Source.Songs))
                foreach (var tree in Source.Trees)
                    if (await AddThumbnail(thumbnails, tree.Flatten()))
                        break;
            int count = thumbnails.Count;
            if (count == 0) Thumbnail = MusicImage.NotFound;
            else if (count <= 2) Thumbnail = thumbnails[0];
            else
            {
                for (int i = 0; i < 4 - count; i++)
                    thumbnails.Add(MusicImage.NotFound);
                First = thumbnails[0];
                Second = thumbnails[1];
                Third = thumbnails[2];
                Fourth = thumbnails[3];
                Thumbnail = null;
            }
            IsThumbnailLoaded = true;
            IsThumbnailLoading = false;
        }

        private async Task<bool> AddThumbnail(List<BitmapImage> thumbnails, List<MusicView> src)
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

        public void Rename(string newPath)
        {
            Source.Rename(newPath);
            OnPropertyChanged("Name");
        }

        public async void AddFile(FolderFile file)
        {
            Source.Files.Add(file);
            if (HasNoThumbnail)
            {
                await LoadThumbnailAsync();
            }
            OnPropertyChanged("Info");
        }

        public async void AddFolder(FolderTree folder)
        {
            Source.Trees.Add(folder);
            if (HasNoThumbnail)
            {
                await LoadThumbnailAsync();
            }
            OnPropertyChanged("Info");
        }

        public override bool Equals(object obj)
        {
            return obj is GridViewFolder folder && Id == folder.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        int IComparable.CompareTo(object other)
        {
            return Name.CompareTo((other as GridViewFolder).Name);
        }
    }
}

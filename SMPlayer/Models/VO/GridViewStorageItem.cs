using SMPlayer.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace SMPlayer.Models.VO
{
    public abstract class GridViewStorageItem : ILocalStorageItem, INotifyPropertyChanged
    {
        public string Path { get; set; }
        public StorageType Type { get; protected set; }
        public abstract long Id { get; }
        public abstract string Name { get; }
        public abstract string Info { get; }
        public abstract string PlayButtonToolTip { get; }
        public abstract bool IsPlaying { get; set; }
        public abstract string TypeIcon { get; }
        public abstract bool ShowTypeIcon { get; }
        public BitmapImage Thumbnail
        {
            get => thumbnail;
            set
            {
                thumbnail = value;
                OnPropertyChanged("Thumbnail");
            }
        }
        protected BitmapImage thumbnail;

        protected bool IsThumbnailLoading = false;
        public bool IsThumbnailLoaded { get; protected set; } = false;
        public abstract Task LoadThumbnailAsync();

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            return obj is GridViewStorageItem i && Path == i.Path;
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }

        public StorageItem AsStorageItem()
        {
            return Type == StorageType.Folder ? new FolderTree { Path = Path, Id = Id } :
                                                (StorageItem) new FolderFile { Path = Path, };
        }
    }

    public enum StorageType
    {
        Folder, File
    }
}

using SMPlayer.Models.VO;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace SMPlayer.Models.VO
{
    public class GridViewMusic : GridViewStorageItem, IMusicable
    {
        public override long Id => Source.Id;
        public override string Name => Source.Name;
        public override string Info => Source.Artist;
        public override string PlayButtonToolTip => Helper.LocalizeText("GridViewMusicPlayInfo", Name);
        public override bool IsPlaying
        {
            get => Source.IsPlaying;
            set
            {
                Source.IsPlaying = value;
                OnPropertyChanged("IsPlaying");
            }
        }
        public override string TypeIcon => "";
        public string Artist { get => Source.Artist; }
        public Music Source
        {
            get => source;
            set
            {
                source = value;
                OnPropertyChanged("Name");
                OnPropertyChanged("Info");
                OnPropertyChanged("PlayButtonToolTip");
                OnPropertyChanged("Artist");
            }
        }
        private Music source;

        public GridViewMusic(Music music)
        {
            Path = music.Path;
            Type = StorageType.File;
            Thumbnail = MusicImage.DefaultImage;
            Source = music;
        }
        public override async Task LoadThumbnailAsync()
        {
            if (IsThumbnailLoading) return;
            IsThumbnailLoading = true;
            if (await ImageHelper.LoadImage(Source) is BitmapImage thumbnail)
                Thumbnail = thumbnail;
            IsThumbnailLoaded = true;
            IsThumbnailLoading = false;
        }

        Music IMusicable.ToMusic()
        {
            return source;
        }
    }
}

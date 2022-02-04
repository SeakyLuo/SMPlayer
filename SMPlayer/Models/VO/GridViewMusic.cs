using SMPlayer.Models.VO;
using SMPlayer.Services;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace SMPlayer.Models.VO
{
    public class GridViewMusic : GridViewStorageItem, IMusicable
    {
        public override long Id => id;
        private long id;
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
        public override string TypeIcon => "Assets/colorful_no_bg.png";
        public string Artist { get => Source.Artist; }
        public string Album { get => Source.Album; }
        private FolderFile SourceFile;
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
        public override bool ShowTypeIcon => false;
        
        public GridViewMusic(FolderFile file)
        {
            id = file.Id;
            Path = file.Path;
            Type = StorageType.File;
            SourceFile = file;
            Thumbnail = MusicImage.DefaultImage;
            Music music = MusicService.FindMusic(file.FileId);
            Source = music;
            IsPlaying = MusicPlayer.CurrentMusic == music;
        }

        public GridViewMusic(Music music)
        {
            Path = music.Path;
            Type = StorageType.File;
            Thumbnail = MusicImage.DefaultImage;
            Source = music;
            IsPlaying = MusicPlayer.CurrentMusic == music;
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

        public FolderFile ToFolderFile()
        {
            return SourceFile;
        }

        Music IMusicable.ToMusic()
        {
            return source;
        }
    }
}

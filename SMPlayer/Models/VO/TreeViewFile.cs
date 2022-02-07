using SMPlayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models.VO
{
    public class TreeViewFile : TreeViewStorageItem, IFolderFile
    {
        public bool IsPlaying
        {
            get => isPlaying;
            set
            {
                isPlaying = value;
                OnPropertyChanged();
            }
        }
        private bool isPlaying;
        public string Icon { get; set; }
        public string Creator { get; set; }
        public string Collection { get; set; } // 从属的集合
        public long FileId { get; set; }
        public FileType FileType { get; set; }

        public TreeViewFile() { }

        public TreeViewFile(FolderFile source)
        {
            Id = source.Id;
            FileId = source.FileId;
            Path = source.Path;
            FileType = source.FileType;
            if (!source.IsMusicFile())
            {
                return;
            }
            MusicView music = MusicService.FindMusic(source.FileId);
            Icon = "Assets/colorful_no_bg.png";
            Creator = music.Artist;
            Collection = music.Album;
        }

        public FolderFile ToFolderFile()
        {
            return new FolderFile
            {
                Id = Id,
                FileId = FileId,
                FileType = FileType,
                Path = Path,
            };
        }
    }
}

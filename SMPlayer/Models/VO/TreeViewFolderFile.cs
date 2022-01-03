using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models.VO
{
    public class TreeViewFolderFile : IFolderFile
    {
        public long Id { get; set; }
        public string Icon { get; set; }
        public string Name { get; set; }
        public string Creator { get; set; }
        public string Collection { get; set; } // 从属的集合
        public long FileId { get; set; }
        public string Path { get; set; }
        public FileType FileType { get; set; }

        public TreeViewFolderFile() { }

        public TreeViewFolderFile(FolderFile source)
        {
            Id = source.Id;
            FileId = source.FileId;
            Path = source.Path;
            Name = System.IO.Path.GetFileNameWithoutExtension(Path);
            FileType = source.FileType;
            if (!source.IsMusicFile())
            {
                return;
            }
            Music music = Settings.FindMusic(source.FileId);
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

using SMPlayer.Helpers;
using SMPlayer.Models.VO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models
{
    public class FolderFile : StorageItem
    {
        public long Id { get; set; }
        public long FileId { get; set; } // 文件ID
        public FileType FileType { get; set; }
        public IFolderFile Source { get; set; }
        public long ParentId { get; set; }
        public string NameWithExtension { get => System.IO.Path.GetFileName(Path); }

        public FolderFile() { }

        public FolderFile(Music music)
        {
            FileId = music.Id;
            FileType = FileType.Music;
            Path = music.Path;
        }

        public FolderFile Copy()
        {
            return new FolderFile()
            {
                Id = Id,
                FileId = FileId,
                FileType = FileType,
                Path = Path,
                Source = Source,
                ParentId = ParentId,
            };
        }

        public void RenameFolder(string oldPath, string newPath)
        {
            Path = Path.Replace(oldPath, newPath);
        }

        public void MoveToFolder(string newPath)
        {
            Path = FileHelper.MoveToPath(Path, newPath);
        }

        public bool IsMusicFile()
        {
            return FileType.IsMusic();
        }

        public override bool Equals(object obj)
        {
            return obj is FolderFile file && Path == file.Path;
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }
    }

    public enum FileType
    {
        Music
    }

    public interface IFolderFile
    {
        FolderFile ToFolderFile();
    }
}

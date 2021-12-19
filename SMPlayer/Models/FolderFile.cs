using SMPlayer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models
{
    public class FolderFile
    {
        public long Id { get; set; }
        // 文件ID
        public long FileId { get; set; }
        public FileType FileType { get; set; }
        public string Path { get; set; }
        public IFolderFile src { get; set; }
        public long ParentId { get; set; }

        public FolderFile() { }

        public FolderFile(Music music)
        {
            FileId = music.Id;
            FileType = FileType.Music;
            Path = music.Path;
        }

        public Music ToMusic()
        {
            return Settings.FindMusic(FileId);
        }

        public void RenameFolder(string oldPath, string newPath)
        {
            Path = Path.Replace(oldPath, newPath);
        }

        public void MoveToFolder(string newPath)
        {
            Path = FileHelper.MoveToPath(Path, newPath);
        }

        public void CopyFrom(FolderFile file)
        {
            FileId = file.FileId;
            FileType = file.FileType;
            Path = file.Path;
        }

        public bool IsMusicFile()
        {
            return FileType == FileType.Music;
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

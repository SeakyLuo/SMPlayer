using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models
{
    public class FolderFile
    {
        // 文件ID
        public long Id { get; set; }
        public FileType Type { get; set; }
        public string Path { get; set; }

        public FolderFile() { }

        public FolderFile(Music music)
        {
            Id = music.Id;
            Type = FileType.Music;
            Path = music.Path;
        }

        public Music ToMusic()
        {
            return Settings.settings.SelectMusicById(Id);
        }

        public void RenameFolder(string oldPath, string newPath)
        {
            Path = Path.Replace(oldPath, newPath);
        }

        public void CopyFrom(FolderFile file)
        {
            Id = file.Id;
            Type = file.Type;
            Path = file.Path;
        }

        public bool IsMusicFile()
        {
            return Type == FileType.Music;
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
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models.DAO
{
    public class FileDAO
    {
        // 文件ID
        public long Id { get; set; }
        public FileType Type { get; set; }
        public string Path { get; set; }
    }

    public enum FileType {
        Music
    }
}

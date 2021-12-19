using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models.DAO
{
    [Table("File")]
    public class FileDAO
    {
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }
        [Indexed]
        public string Path { get; set; }
        [Indexed]
        public long ParentId { get; set; }
        public long FileId { get; set; } // 对应的文件ID，非该记录ID
        public FileType FileType { get; set; }
        public ActiveState State { get; set; }
    }
}

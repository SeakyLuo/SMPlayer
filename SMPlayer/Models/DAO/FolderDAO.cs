using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models.DAO
{
    [Table("Folder")]
    public class FolderDAO
    {
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; } // 文件夹ID
        [Indexed]
        public string Path { get; set; }
        public SortBy Criterion { get; set; } = SortBy.Title;
        [Indexed]
        public long ParentId { get; set; }
        public ActiveState State { get; set; }
        [Ignore]
        public List<FolderDAO> Folders { get; set; }
        [Ignore]
        public List<FileDAO> Files { get; set; }
    }
}

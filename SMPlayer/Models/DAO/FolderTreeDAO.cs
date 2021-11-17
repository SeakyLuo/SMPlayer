using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models.DAO
{
    public class FolderTreeDAO
    {
        // 文件夹ID
        public long Id { get; set; }
        public string Path { get; set; } = "";
        public SortBy Criterion { get; set; } = SortBy.Title;
        public List<FolderTreeDAO> Trees { get; set; } = new List<FolderTreeDAO>();
        public List<FileDAO> Files { get; set; } = new List<FileDAO>();

    }
}

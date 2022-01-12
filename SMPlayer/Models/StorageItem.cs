using SMPlayer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models
{
    public class StorageItem
    {
        public string Path { get; set; } = "";
        public string ParentPath { get => FileHelper.GetParentPath(Path); }
        public string Name { get => System.IO.Path.GetFileNameWithoutExtension(Path); }
    }
}

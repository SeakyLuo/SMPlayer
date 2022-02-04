using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models.VO
{
    public class TreeViewFolder : TreeViewStorageItem
    {
        public FolderTree Source { get; set; }

        public TreeViewFolder(FolderTree folder)
        {
            this.Source = folder;
            Id = folder.Id;
            Path = folder.Path;
        }

        public void Rename(string newPath)
        {
            Source.Rename(Source.Path, newPath);
            Path = Source.Path;
            OnPropertyChanged("Path");
            OnPropertyChanged("Name");
        }

        public List<Music> Flatten()
        {
            return Source.Flatten();
        }
    }
}

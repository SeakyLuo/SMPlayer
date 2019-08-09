using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SMPlayer.Models
{
    [Serializable]
    public class FolderTree
    {
        public List<FolderTree> folders { get; set; }
        public SortedSet<Music> musics { get; set; }

        public FolderTree()
        {
            folders = new List<FolderTree>();
            musics = new SortedSet<Music>();
        }
        public FolderTree(StorageFolder folder, Action afterInitiation = null)
        {
            folders = new List<FolderTree>();
            musics = new SortedSet<Music>();
            Init(folder);
            afterInitiation?.Invoke();
        }

        private async void Init(StorageFolder folder)
        {
            foreach (var subFolder in await folder.GetFoldersAsync())
                folders.Add(new FolderTree(subFolder));
            foreach (var file in await folder.GetFilesAsync())
                if (file.Name.EndsWith("mp3"))
                    musics.Add(await Music.GetMusic(file.Path));
        }

        public SortedSet<Music> Flatten()
        {
            SortedSet<Music> list = new SortedSet<Music>();
            foreach (var branch in folders)
                list.Concat(branch.Flatten());
            foreach (var music in musics)
                list.Add(music);
            return list;
        }
    }
}

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
        public List<FolderTree> folders = new List<FolderTree>();
        public List<Music> musics = new List<Music>();

        public FolderTree() { }
        public FolderTree(StorageFolder folder, Action afterInitiation = null)
        {
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

        public List<Music> Flatten()
        {
            List<Music> list = new List<Music>();
            foreach (var branch in folders)
                list.Concat(branch.Flatten());
            foreach (var music in musics)
                list.Add(music);
            return list;
        }
    }
}

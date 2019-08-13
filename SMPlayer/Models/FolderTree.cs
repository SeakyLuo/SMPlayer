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
        public List<FolderTree> Trees = new List<FolderTree>();
        public List<Music> Files = new List<Music>();
        public string Path = "";

        public FolderTree() { }
        public FolderTree(FolderTree tree)
        {
            CopyFrom(tree);
        }

        public void CopyFrom(FolderTree tree)
        {
            Trees = tree.Trees;
            Files = tree.Files;
            Path = tree.Path;
        }

        public async Task Init(StorageFolder folder)
        {
            if (!string.IsNullOrEmpty(Path) && folder.Path.StartsWith(Path))
            {
                // New folder is a Subfolder of the current folder
                FolderTree tree;
                do
                {
                    tree = Trees.Find((t) => folder.Path.StartsWith(t.Path));
                } while (tree.Path != folder.Path);
                CopyFrom(tree);
            }
            else if (Path.StartsWith(folder.Path))
            {
                // New folder is a Subfolder of the current folder
                FolderTree newTree = new FolderTree();
                foreach (var subFolder in await folder.GetFoldersAsync())
                {
                    var tree = subFolder.Path == Path ? new FolderTree() : new FolderTree(this);
                    await tree.Init(subFolder);
                    newTree.Trees.Add(tree);
                }
                foreach (var file in await folder.GetFilesAsync())
                    if (file.Name.EndsWith("mp3"))
                        newTree.Files.Add(await Music.GetMusic(file.Path));
                CopyFrom(newTree);
            }
            else
            {
                // No hierarchy between folders
                Trees.Clear();
                foreach (var subFolder in await folder.GetFoldersAsync())
                {
                    var tree = new FolderTree();
                    await tree.Init(subFolder);
                    Trees.Add(tree);
                }
                Files.Clear();
                foreach (var file in await folder.GetFilesAsync())
                    if (file.Name.EndsWith("mp3"))
                        Files.Add(await Music.GetMusic(file.Path));
            }
            Path = folder.Path;
        }

        public List<Music> Flatten()
        {
            List<Music> list = new List<Music>();
            foreach (var branch in Trees)
                list.AddRange(branch.Flatten());
            foreach (var music in Files)
                list.Add(music);
            return list;
        }

        public string GetFolderName()
        {
            return Path.Substring(Path.LastIndexOf("\\" + 1));
        }
    }
}

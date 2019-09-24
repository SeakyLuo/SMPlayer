﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SMPlayer.Models
{
    [Serializable]
    public class FolderTree : INotifyPropertyChanged
    {
        public List<FolderTree> Trees = new List<FolderTree>();
        public List<Music> Files = new List<Music>();
        public string Path = "";
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public TreeInfo Info { get => new TreeInfo(Directory, Trees.Count, Files.Count); }
        public bool IsEmpty { get => Files.Count == 0 && Trees.All((tree) => tree.IsEmpty); }
        public string Directory { get => Path.Substring(Path.LastIndexOf("\\") + 1); }

        public FolderTree() { }
        public FolderTree(FolderTree tree)
        {
            CopyFrom(tree);
        }

        public void CopyFrom(FolderTree tree)
        {
            Trees = tree.Trees.ToList();
            Files = tree.Files.ToList();
            Path = tree.Path;
            OnPropertyChanged();
        }

        public static async Task<int> CountMusicAsync(StorageFolder folder)
        {
            int count = (await folder.GetFilesAsync()).Count((f) => IsMusicFile(f));
            foreach (var sub in await folder.GetFoldersAsync())
                count += await CountMusicAsync(sub);
            return count;
        }

        public async Task Init(StorageFolder folder, TreeInitProgressListener listener = null)
        {
            await Init(folder, listener, new TreeInitProgressIndicator() { Max = listener == null ? 0 : await CountMusicAsync(folder) });
        }

        private async Task Init(StorageFolder folder, TreeInitProgressListener listener, TreeInitProgressIndicator indicator)
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
                listener.Update(folder.DisplayName, "", 0, 0);
            }
            else if (Path.StartsWith(folder.Path))
            {
                // Current folder is a Subfolder of the new folder
                FolderTree newTree = new FolderTree();
                var folders = await folder.GetFoldersAsync();
                for (int i = 0; i < folders.Count; i++)
                {
                    var subFolder = folders[i];
                    FolderTree tree;
                    if (subFolder.Path == Path)
                    {
                        tree = new FolderTree();
                        await tree.Init(subFolder, listener, indicator);
                    }
                    else
                    {
                        tree = new FolderTree(this);
                    }
                    newTree.Trees.Add(tree);
                }
                foreach (var file in await folder.GetFilesAsync())
                {
                    if (IsMusicFile(file))
                    {
                        Music music = await Music.GetMusicAsync(file);
                        listener?.Update(folder.DisplayName, music.Name, indicator.Update(), indicator.Max);
                        newTree.Files.Add(music);
                    }
                }
                CopyFrom(newTree);
            }
            else
            {
                // No hierarchy between folders
                Trees.Clear();
                foreach (var subFolder in await folder.GetFoldersAsync())
                {
                    var tree = new FolderTree();
                    await tree.Init(subFolder, listener, indicator);
                    Trees.Add(tree);
                }
                Files.Clear();
                foreach (var file in await folder.GetFilesAsync())
                {
                    if (IsMusicFile(file))
                    {
                        Music music = await Music.GetMusicAsync(file);
                        listener?.Update(folder.DisplayName, music.Name, indicator.Update(), indicator.Max);
                        Files.Add(music);
                    }
                }
            }
            Path = folder.Path;
        }

        public bool Contains(Music music)
        {
            return Files.Contains(music) || Trees.Any((tree) => tree.Contains(music));
        }

        public void Update(FolderTree tree)
        {
            // Merge to this tree
            foreach (var folder in tree.Trees)
                Trees.FirstOrDefault((f) => f.Equals(folder))?.Update(folder);
            var set = Files.ToHashSet();
            foreach (var file in tree.Files)
                if (set.Contains(file))
                    Files.First((f) => f.Equals(file)).CopyFrom(file);
            OnPropertyChanged();
        }

        public List<Music> Flatten()
        {
            List<Music> list = new List<Music>();
            foreach (var branch in Trees)
                list.AddRange(branch.Flatten());
            list.AddRange(Files);
            return list;
        }

        public static bool IsMusicFile(StorageFile file)
        {
            return file.FileType.Contains("mp3");
        }

        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is FolderTree && Path == (obj as FolderTree).Path;
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }
    }
    public struct TreeInfo
    {
        public string Directory;
        public int Folders;
        public int Songs;
        public TreeInfo(string Directory, int Folders, int Songs)
        {
            this.Directory = Directory;
            this.Folders = Folders;
            this.Songs = Songs;
        }
    }

    class TreeInitProgressIndicator
    {
        public int Progress = 0;
        public int Max = 0;
        public int Update() { return ++Progress; }
    }

    public interface TreeInitProgressListener
    {
        void Update(string folder, string file, int progress, int max);
    }
}

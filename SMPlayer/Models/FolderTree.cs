using SMPlayer.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace SMPlayer.Models
{
    [Serializable]
    public class FolderTree : IComparable, IPreferable
    {
        public long Id { get; set; }
        public List<FolderTree> Trees { get; set; } = new List<FolderTree>();
        public List<FolderFile> Files { get; set; } = new List<FolderFile>();
        public List<Music> Songs { get => Settings.FindMusicList(Files.Select(i => i.FileId)); }
        public string Path { get; set; } = "";
        public SortBy Criterion { get; set; } = SortBy.Title;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public TreeInfo Info { get => new TreeInfo(Name, Trees.Count, Files.Count); }
        public bool IsEmpty { get => Files.Count == 0 && Trees.All(tree => tree.IsEmpty); }
        public bool IsNotEmpty { get => !IsEmpty; }
        public int FileCount { get => Trees.Sum(t => t.FileCount) + Files.Count; }
        public string Name { get => FileHelper.GetDisplayName(Path); }
        public string ParentPath { get => FileHelper.GetParentPath(Path); }
        public long ParentId { get; set; }

        public FolderTree() { }
        public FolderTree(FolderTree tree)
        {
            CopyFrom(tree);
        }

        public void CopyFrom(FolderTree tree)
        {
            Id = tree.Id;
            Trees = tree.Trees.ToList();
            Files = tree.Files.ToList();
            Path = tree.Path;
            Criterion = tree.Criterion;
        }

        public void AddTree(FolderTree tree)
        {
            Trees.Add(tree);
        }

        public void AddFile(FolderFile file)
        {
            Files.Add(file);
        }

        public bool Contains(string path)
        {
            return FindFile(path) != null;
        }

        public List<Music> Flatten()
        {
            if (IsEmpty)
            {
                CopyFrom(Settings.FindFolder(Id));
            }
            List<Music> list = new List<Music>();
            foreach (var branch in Trees)
                list.AddRange(branch.Flatten());
            list.AddRange(Songs);
            return list;
        }
        public List<FolderTree> GetAllTrees()
        {
            List<FolderTree> list = new List<FolderTree> { this };
            foreach (var tree in Trees)
                list.AddRange(tree.GetAllTrees());
            return list;
        }
        public IEnumerable<Music> SortByTitle()
        {
            Criterion = SortBy.Title;
            var list = Files.Select(f => new { File = f, Music = Settings.FindMusic(f.FileId) })
                            .OrderBy(i => i.Music.Name).ToList();
            Files = list.Select(i => i.File).ToList();
            return list.Select(i => i.Music);
        }
        public IEnumerable<Music> SortByArtist()
        {
            Criterion = SortBy.Artist;
            var list = Files.Select(f => new { File = f, Music = Settings.FindMusic(f.FileId) })
                            .OrderBy(i => i.Music.Artist).ToList();
            Files = list.Select(i => i.File).ToList();
            return list.Select(i => i.Music);
        }
        public IEnumerable<Music> SortByAlbum()
        {
            Criterion = SortBy.Album;
            var list = Files.Select(f => new { File = f, Music = Settings.FindMusic(f.FileId) })
                            .OrderBy(i => i.Music.Album).ToList();
            Files = list.Select(i => i.File).ToList();
            return list.Select(i => i.Music);
        }
        public List<Music> Reverse()
        {
            Files.Reverse();
            return Songs;
        }
        public void Clear()
        {
            foreach (var tree in Trees)
                tree.Clear();
            Trees.Clear();
            Files.Clear();
        }

        public FolderTree FindTree(string path)
        {
            if (Path.Equals(path)) return this;
            foreach (var tree in Trees)
            {
                if (path.Equals(tree.Path))
                    return tree;
                if (path.StartsWith(tree.Path))
                    return tree.FindTree(path);
            }
            return null;
        }
        public FolderTree FindTree(long id)
        {
            if (Id == id) return this;
            foreach (var tree in Trees)
            {
                if (tree.FindTree(id) is FolderTree target)
                    return target;
            }
            return null;
        }

        public FolderFile FindFile(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            return Files.FirstOrDefault(m => m.Path == path) ?? Trees.FirstOrDefault(tree => path.StartsWith(tree.Path))?.FindFile(path);
        }

        public async Task<StorageFolder> GetStorageFolderAsync()
        {
            return await FileHelper.LoadFolderAsync(Path);
        }

        public void Rename(string newPath)
        {
            Rename(Path, newPath);
        }

        public void Rename(string oldPath, string newPath)
        {
            foreach (var tree in Trees)
            {
                tree.Rename(oldPath, newPath);
            }
            foreach (var file in Files)
            {
                file.RenameFolder(oldPath, newPath);
            }
            Path = Path.Replace(oldPath, newPath);
        }

        public bool RemoveBranch(string path)
        {
            return Trees.RemoveAll(f => f.Path == path) > 0 ||
                   (Trees.FirstOrDefault(t => path.StartsWith(t.Path)) is FolderTree tree && tree.RemoveBranch(path));
        }

        public void MoveBranch(FolderTree branch, string newPath)
        {
            FolderTree tree = FindTree(branch.Id);
            RemoveBranch(branch.Path);
            tree.Rename(tree.Path, System.IO.Path.Combine(newPath, Name));
            FindTree(newPath)?.AddTree(tree);
        }

        public bool RemoveFile(string path)
        {
            return Files.RemoveAll(f => f.Path == path) > 0 ||
                   (Trees.FirstOrDefault(t => path.StartsWith(t.Path)) is FolderTree tree && tree.RemoveFile(path));
        }

        public void MoveFile(FolderFile src, string newPath)
        {
            FolderFile file = FindFile(src.Path);
            RemoveFile(src.Path);
            file.MoveToFolder(newPath);
            if (FindTree(newPath) is FolderTree newParent)
            {
                if (newParent.Contains(src.Path))
                {
                    newParent.RemoveFile(src.Path);
                }
                newParent.AddFile(file);
            }
        }

        public override bool Equals(object obj)
        {
            return obj is FolderTree tree && Path == tree.Path;
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }
        public override string ToString()
        {
            return Path;
        }

        public int CompareTo(object obj)
        {
            return ToString().CompareTo(obj.ToString());
        }

        PreferenceItem IPreferable.AsPreferenceItem()
        {
            return new PreferenceItem(Id.ToString(), Name);
        }

        PreferenceItemView IPreferable.AsPreferenceItemView()
        {
            return new PreferenceItemView(Id.ToString(), Name, Path, PreferType.Folder);
        }
    }

    public struct TreeInfo
    {
        public string Directory { get; set; }
        public int Folders { get; set; }
        public int Songs { get; set; }
        public TreeInfo(string Directory, int Folders, int Songs)
        {
            this.Directory = Directory;
            this.Folders = Folders;
            this.Songs = Songs;
        }
        public override string ToString()
        {
            string info = Helper.LocalizeMessage("Songs:") + Songs;
            if (Folders > 0) info = Helper.LocalizeMessage("Folders:") + Folders + " • " + info;
            return info;
        }
    }
}
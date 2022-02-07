﻿using SMPlayer.Helpers;
using SMPlayer.Interfaces;
using SMPlayer.Models.DAO;
using SMPlayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace SMPlayer.Models
{
    [Serializable]
    public class FolderTree : StorageItem, IPreferable, ISearchEvaluator
    {
        public List<FolderTree> Trees { get; set; } = new List<FolderTree>();
        public List<FolderFile> Files { get; set; } = new List<FolderFile>();
        public List<MusicView> Songs { get => MusicService.FindMusicList(Files.Select(i => i.FileId)); }
        public SortBy Criterion { get; set; } = SortBy.Title;
        public string Info => GetFolderInfo(Trees.Count, Files.Count);
        public bool IsEmpty { get => Files.IsEmpty() && Trees.All(tree => tree.IsEmpty); }
        public bool IsNotEmpty { get => !IsEmpty; }
        public int FileCount { get => Trees.Sum(t => t.FileCount) + Files.Count; }
        public long ParentId { get; set; }
        public ActiveState State { get; set; } = ActiveState.Active;

        public FolderTree() { }

        public FolderTree(string path)
        {
            Path = path;
        }

        public FolderTree(FolderTree src)
        {
            CopyFrom(src);
        }

        public void CopyFrom(FolderTree tree)
        {
            Id = tree.Id;
            Trees = tree.Trees.ToList();
            Files = tree.Files.ToList();
            ParentId = tree.ParentId;
            Path = tree.Path;
            Criterion = tree.Criterion;
        }

        public void AddBranch(FolderTree tree)
        {
            Trees.Add(tree);
            Trees.Sort((t1, t2) => t1.Name.CompareTo(t2.Name));
        }

        public void AddFile(FolderFile file)
        {
            Files.Add(file);
            SortFiles();
        }

        public bool ContainsFile(string path)
        {
            return FindFile(path) != null;
        }

        public List<MusicView> Flatten()
        {
            if (IsEmpty)
            {
                CopyFrom(StorageService.FindFolder(Id));
            }
            List<MusicView> list = new List<MusicView>();
            foreach (var branch in Trees)
                list.AddRange(branch.Flatten());
            list.AddRange(Songs);
            return list;
        }

        public void SortFiles(SortBy? criterion = null)
        {
            if (criterion != null) Criterion = (SortBy)criterion;
            Files = Files.Select(f => new { File = f, Music = MusicService.FindMusic(f.FileId) })
                         .Where(i => i.Music != null)
                         .OrderBy(i =>
                         {
                             switch (Criterion)
                             {
                                 case SortBy.Artist:
                                     return i.Music.Artist;
                                 case SortBy.Album:
                                     return i.Music.Album;
                                 default:
                                     return i.Music.Name;
                             }
                         }).Select(i => i.File).ToList();
        }

        public List<MusicView> Reverse()
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

        public FolderTree FindFolder(string path)
        {
            if (Path.Equals(path)) return this;
            foreach (var tree in Trees)
            {
                if (path.Equals(tree.Path))
                    return tree;
                if (path.StartsWith(tree.Path))
                    return tree.FindFolder(path);
            }
            return null;
        }
        public FolderTree FindFolder(long id)
        {
            if (Id == id) return this;
            foreach (var tree in Trees)
            {
                if (tree.FindFolder(id) is FolderTree target)
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
            return await StorageHelper.LoadFolderAsync(Path);
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

        public void MoveToFolder(FolderTree target)
        {
            Rename(ParentPath, target.Path);
            ParentId = target.Id;
        }

        public bool RemoveBranch(string path)
        {
            return Trees.RemoveAll(f => f.Path == path) > 0 ||
                   (Trees.FirstOrDefault(t => path.StartsWith(t.Path)) is FolderTree tree && tree.RemoveBranch(path));
        }

        public void MoveBranch(FolderTree branch, FolderTree newParent)
        {
            RemoveBranch(branch.Path);
            FindFolder(branch.Id)?.MoveToFolder(newParent);
            FindFolder(newParent.Path)?.AddBranch(branch);
        }

        public bool RemoveFile(string path)
        {
            return Files.RemoveAll(f => f.Path == path) > 0 ||
                   (Trees.FirstOrDefault(t => path.StartsWith(t.Path)) is FolderTree tree && tree.RemoveFile(path));
        }

        public bool IsParentOf(string path)
        {
            return StorageHelper.IsParentDirectory(path, Path);
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

        PreferenceItem IPreferable.AsPreferenceItem()
        {
            return new PreferenceItem(Id.ToString(), Name, EntityType.Folder);
        }

        public static string GetFolderInfo(int folders, int files)
        {
            string info = Helper.LocalizeMessage("Songs:") + files;
            if (folders > 0) info = Helper.LocalizeMessage("Folders:") + folders + " • " + info;
            return info;
        }
        public double Evaluate(string keyword)
        {
            return SearchHelper.EvaluateString(Name, keyword, -3);
        }

        public double Match(string keyword)
        {
            return SearchHelper.EvaluateString(Name, keyword);
        }
    }
}
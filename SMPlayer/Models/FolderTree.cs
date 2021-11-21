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
    public class FolderTree : INotifyPropertyChanged, IComparable, IPreferable
    {
        public long Id { get; set; }
        public List<FolderTree> Trees { get; set; } = new List<FolderTree>();
        public List<FolderFile> Files { get; set; } = new List<FolderFile>();
        [Newtonsoft.Json.JsonIgnore]
        public List<Music> Songs { get => Settings.settings.SelectMusicByIds(Files.Select(i => i.Id)); }
        public string Path { get; set; } = "";
        public SortBy Criterion { get; set; } = SortBy.Title;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        [Newtonsoft.Json.JsonIgnore]
        public TreeInfo Info { get => new TreeInfo(Directory, Trees.Count, Files.Count); }
        [Newtonsoft.Json.JsonIgnore]
        public bool IsEmpty { get => Files.Count == 0 && Trees.All(tree => tree.IsEmpty); }
        [Newtonsoft.Json.JsonIgnore]
        public bool IsNotEmpty { get => !IsEmpty; }
        [Newtonsoft.Json.JsonIgnore]
        public int FileCount { get => Trees.Sum(t => t.FileCount) + Files.Count; }
        [Newtonsoft.Json.JsonIgnore]
        public string Directory { get => Path.Substring(Path.LastIndexOf("\\") + 1); }
        [Newtonsoft.Json.JsonIgnore]
        public string ParentPath
        {
            get
            {
                int index = Path.LastIndexOf("\\");
                return index == -1 ? "" : Path.Substring(0, index);
            }
        }

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
            OnPropertyChanged();
        }

        public bool Contains(string path)
        {
            return FindFile(path) != null;
        }

        public bool RemoveMusic(Music music)
        {
            return Files.RemoveAll(f =>f.Path == music.Path) > 0 ||
                   (Trees.FirstOrDefault(t => music.Path.StartsWith(t.Path)) is FolderTree tree && tree.RemoveMusic(music));
        }

        public List<Music> Flatten()
        {
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
        public void Sort()
        {
            Trees = Trees.OrderBy(t => t.Directory).ToList();
            switch (Criterion)
            {
                case SortBy.Title:
                    SortByTitle();
                    break;
                case SortBy.Album:
                    SortByAlbum();
                    break;
                case SortBy.Artist:
                    SortByArtist();
                    break;
            }
        }
        public List<Music> SortByTitle()
        {
            Criterion = SortBy.Title;
            List<Music> songs = Songs.OrderBy(m => m.Name).ToList();
            Files = songs.Select(i => new FolderFile(i)).ToList();
            return songs;
        }
        public List<Music> SortByArtist()
        {
            Criterion = SortBy.Artist;
            List<Music> songs = Songs.OrderBy(m => m.Artist).ToList();
            Files = songs.Select(i => new FolderFile(i)).ToList();
            return songs;
        }
        public List<Music> SortByAlbum()
        {
            Criterion = SortBy.Album;
            List<Music> songs = Songs.OrderBy(m => m.Album).ToList();
            Files = songs.Select(i => new FolderFile(i)).ToList();
            return songs;
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
        public FolderTree FindTree(FolderTree target)
        {
            //return Trees.FirstOrDefault(tree => tree.Equals(target)) ?? Trees.FirstOrDefault(tree => target.Path.StartsWith(tree.Path))?.FindTree(target);
            return FindTree(target.Id);
        }
        public FolderTree FindTree(string path)
        {
            //return Trees.FirstOrDefault(tree => tree.Equals(target)) ?? Trees.FirstOrDefault(tree => target.Path.StartsWith(tree.Path))?.FindTree(target);
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
        public FolderTree FindTree(Music music)
        {
            string path = music.Path.Substring(0, music.Path.LastIndexOf('\\'));
            return FindTree(path);
        }
        public Music FindMusic(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            return FindFile(path) is FolderFile file ? Settings.settings.SelectMusicById(file.Id) : null;
        }
        public Music FindMusic(Music target)
        {
            return FindMusic(target.Path);
        }
        public FolderFile FindFile(FolderFile file)
        {
            return FindFile(file?.Path);
        }
        public FolderFile FindFile(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            return Files.FirstOrDefault(m => m.Path == path) ?? Trees.FirstOrDefault(tree => path.StartsWith(tree.Path))?.FindFile(path);
        }
        //public static bool operator == (FolderTree tree1, FolderTree tree2)
        //{
        //    if (!tree1.Equals(tree2)) return false;
        //    return tree1.Files.Zip(tree2.Files, (m1, m2) => m1 == m2).All(v => v) && tree1.Trees.Zip(tree2.Trees, (t1, t2) => t1 == t2).All(v => v);
        //}
        //public static bool operator !=(FolderTree tree1, FolderTree tree2)
        //{
        //    return !(tree1 == tree2);
        //}
        public async Task<StorageFolder> GetStorageFolderAsync()
        {
            try
            {
                return await StorageFolder.GetFolderFromPathAsync(Path);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        public void Rename(string newPath)
        {
            Rename(Path, newPath);
            OnPropertyChanged("Directory");
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

        public void MoveBranch(FolderTree branch, string newPath)
        {
            FolderTree tree = FindTree(branch);
            FindTree(branch.ParentPath).Trees.Remove(branch);
            tree.Rename(tree.Path, newPath + "\\" + Directory);
            FindTree(newPath)?.Trees.Add(tree);
        }

        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
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
            return new PreferenceItem(Id.ToString(), Directory);
        }

        PreferenceItemView IPreferable.AsPreferenceItemView()
        {
            return new PreferenceItemView(Id.ToString(), Directory, Path, PreferType.Folder);
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
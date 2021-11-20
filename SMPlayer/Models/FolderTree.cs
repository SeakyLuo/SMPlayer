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
        public List<FolderFile> Files { get; set; }
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

        private static volatile ExecutionStatus LoadingStatus = ExecutionStatus.Ready;

        public FolderTree() { }
        public FolderTree(FolderTree tree)
        {
            CopyFrom(tree);
        }

        public void CopyFrom(FolderTree tree)
        {
            Id = Id;
            Trees = tree.Trees.ToList();
            Files = tree.Files.ToList();
            Path = tree.Path;
            Criterion = tree.Criterion;
            OnPropertyChanged();
        }

        public static async Task<int> CountFilesAsync(StorageFolder folder)
        {
            int count = (await folder.GetFilesAsync()).Count(f => f.IsMusicFile());
            foreach (var sub in await folder.GetFoldersAsync())
                count += await CountFilesAsync(sub);
            return count;
        }

        public void PauseLoading()
        {
            LoadingStatus = ExecutionStatus.Break;
        }
        public async Task<bool> CheckNewFile(TreeUpdateData data = null)
        {
            StorageFolder folder;
            try
            {
                folder = await GetStorageFolderAsync();
            }
            catch (UnauthorizedAccessException)
            {
                if (data != null)
                    data.Message = Helper.LocalizeMessage("UnauthorizedAccessException", Path);
                return false;
            }
            if (folder == null)
            {
                if (data != null)
                    data.Message = Helper.LocalizeMessage("FolderNotFound", Path);
                return false;
            }
            return await CheckNewFile(folder, data);
        }
        private void AddToMusicLibrary()
        {
            Id = Settings.settings.IdGenerator.GenerateTreeId();
            foreach (var music in Songs)
                Settings.settings.AddMusic(music);
            foreach (var tree in Trees)
                tree.AddToMusicLibrary();
        }
        private async Task<bool> CheckNewFile(StorageFolder folder, TreeUpdateData data = null)
        {
            LoadingStatus = ExecutionStatus.Running;
            var pathSet = new HashSet<string>(); // folder path set
            foreach (var subFolder in await folder.GetFoldersAsync())
            {
                if (LoadingStatus == ExecutionStatus.Break) return false;
                if (Trees.FirstOrDefault(t => t.Path == subFolder.Path) is FolderTree tree)
                {
                    await tree.CheckNewFile(subFolder, data);
                }
                else
                {
                    tree = new FolderTree();
                    await tree.Init(subFolder);
                    if (!tree.IsEmpty)
                    {
                        Trees.Add(tree);
                        data.More += tree.FileCount;
                        tree.AddToMusicLibrary();
                    }
                }
                pathSet.Add(subFolder.Name);
            }
            foreach (var tree in Trees.FindAll(t => !pathSet.Contains(t.Directory)))
            {
                data.Less += tree.FileCount;
                Trees.Remove(tree);
                tree.Clear();
            }
            pathSet = Files.Select(m => m.Path).ToHashSet(); // file path set
            var newList = new List<FolderFile>();
            var newSet = new HashSet<string>();
            foreach (var file in await folder.GetFilesAsync())
            {
                if (LoadingStatus == ExecutionStatus.Break) return false;
                if (!file.IsMusicFile()) continue;
                newSet.Add(file.Path);
                if (!pathSet.Contains(file.Path))
                {
                    Music music = await Music.LoadFromFileAsync(file);
                    newList.Add(new FolderFile(music));
                    data.More++;
                }
            }
            int before = Files.Count;
            foreach (var music in Files.FindAll(m => !newSet.Contains(m.Path)))
            {
                if (LoadingStatus == ExecutionStatus.Break) return false;
                Files.Remove(music);
            }
            data.Less += before - Files.Count;
            foreach (var music in newList)
            {
                Files.Add(music);
            }
            Sort();
            LoadingStatus = ExecutionStatus.Ready;
            return true;
        }
        public async Task<bool> Init(StorageFolder folder, Action<string, string, int, int> listener = null)
        {
            return await Init(folder, listener, new TreeOperationIndicator() { Max = listener == null ? 0 : await CountFilesAsync(folder) });
        }
        private async Task<bool> Init(StorageFolder folder, Action<string, string, int, int> updater, TreeOperationIndicator indicator)
        {
            LoadingStatus = ExecutionStatus.Running;
            Id = Settings.settings.IdGenerator.GenerateTreeId();
            var samePath = folder.Path == Path;
            if (string.IsNullOrEmpty(Path))
            {
                // New folder tree
                foreach (var subFolder in await folder.GetFoldersAsync())
                {
                    if (LoadingStatus == ExecutionStatus.Break) return false;
                    var tree = new FolderTree();
                    await tree.Init(subFolder, updater, indicator);
                    if (!tree.IsEmpty) Trees.Add(tree);
                }
                foreach (var file in await folder.GetFilesAsync())
                {
                    if (LoadingStatus == ExecutionStatus.Break) return false;
                    if (file.IsMusicFile())
                    {
                        Music music = await Music.LoadFromFileAsync(file);
                        updater?.Invoke(folder.DisplayName, music.Name, indicator.Update(), indicator.Max);
                        Files.Add(new FolderFile(music));
                    }
                }
            }
            else if (!samePath && folder.Path.StartsWith(Path))
            {
                // New folder is a Subfolder of the current folder
                FolderTree tree = FindTree(folder.Path);
                CopyFrom(tree);
                updater?.Invoke(folder.DisplayName, "", 0, 0);
            }
            else if (!samePath && Path.StartsWith(folder.Path))
            {
                // Current folder is a Subfolder of the new folder
                FolderTree newTree = new FolderTree();
                var folders = await folder.GetFoldersAsync();
                foreach (var subFolder in folders)
                {
                    if (LoadingStatus == ExecutionStatus.Break) return false;
                    FolderTree tree;
                    if (subFolder.Path == Path)
                    {
                        tree = new FolderTree(this);
                    }
                    else
                    {
                        tree = new FolderTree();
                        await tree.Init(subFolder, updater, indicator);
                    }
                    if (tree.Files.Count != 0) newTree.Trees.Add(tree);
                }
                foreach (var file in await folder.GetFilesAsync())
                {
                    if (LoadingStatus == ExecutionStatus.Break) return false;
                    if (file.IsMusicFile())
                    {
                        Music music = await Music.LoadFromFileAsync(file);
                        updater?.Invoke(folder.DisplayName, music.Name, indicator.Update(), indicator.Max);
                        newTree.Files.Add(new FolderFile(music));
                    }
                }
                CopyFrom(newTree);
            }
            else
            {
                // No hierarchy between folders
                // or update folder
                var trees = new List<FolderTree>();
                foreach (var subFolder in await folder.GetFoldersAsync())
                {
                    if (LoadingStatus == ExecutionStatus.Break) return false;
                    var source = Trees.FirstOrDefault(t => t.Path == subFolder.Path);
                    var tree = new FolderTree()
                    {
                        Criterion = source?.Criterion ?? SortBy.Title
                    };
                    await tree.Init(subFolder, updater, indicator);
                    if (!tree.IsEmpty) trees.Add(tree);
                }
                Clear();
                Trees = trees;
                foreach (var file in await folder.GetFilesAsync())
                {
                    if (LoadingStatus == ExecutionStatus.Break) return false;
                    if (file.IsMusicFile())
                    {
                        Music music = await Music.LoadFromFileAsync(file);
                        if (samePath && Settings.FindMusic(music) is Music oldItem)
                        {
                            music.Id = oldItem.Id;
                            music.PlayCount = oldItem.PlayCount;
                            music.Favorite = oldItem.Favorite;
                        }
                        updater?.Invoke(folder.DisplayName, music.Name, indicator.Update(), indicator.Max);
                        Files.Add(new FolderFile(music));
                    }
                }
            }
            Path = folder.Path;
            Sort();
            LoadingStatus = ExecutionStatus.Ready;
            return true;
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

        public FolderTree MergeFrom(FolderTree tree)
        {
            // Merge to this tree
            foreach (var folder in tree.Trees)
                Trees.FirstOrDefault(f => f.Equals(folder))?.MergeFrom(folder);
            var set = Files.ToHashSet();
            foreach (var file in tree.Files)
                if (set.Contains(file))
                    Files.First(f => f.Equals(file)).CopyFrom(file);
            Criterion = tree.Criterion;
            Sort();
            OnPropertyChanged();
            return this;
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
            return FindFile(path) is FolderFile file ? Settings.settings.SelectMusicById(file.Id) : null;
        }
        private FolderFile FindFile(string path)
        {
            return Files.FirstOrDefault(m => m.Path == path) ?? Trees.FirstOrDefault(tree => path.StartsWith(tree.Path))?.FindFile(path);
        }
        public Music FindMusic(Music target)
        {
            return FindMusic(target.Path);
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

    public class TreeOperationIndicator
    {
        public int Progress { get; set; } = 0;
        public int Max { get; set; } = 0;
        public int Update() { return ++Progress; }
    }

    public class TreeUpdateData
    {
        public int More { get; set; } = 0;
        public int Less { get; set; } = 0;
        public string Message { get; set; }
    }
}
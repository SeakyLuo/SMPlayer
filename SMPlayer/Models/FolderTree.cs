using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace SMPlayer.Models
{
    [Serializable]
    public class FolderTree : INotifyPropertyChanged, IComparable
    {
        public List<FolderTree> Trees = new List<FolderTree>();
        public List<Music> Files = new List<Music>();
        public string Path = "";
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public TreeInfo Info { get => new TreeInfo(Directory, Trees.Count, Files.Count); }
        public bool IsEmpty { get => Files.Count == 0 && Trees.All((tree) => tree.IsEmpty); }
        public string Directory { get => Path.Substring(Path.LastIndexOf("\\") + 1); }
        public SortBy Criteria { get; set; } = SortBy.Title;

        public static ExecutionStatus LoadingStatus { get; private set; } = ExecutionStatus.Ready;

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
            Criteria = tree.Criteria;
            OnPropertyChanged();
        }

        public static async Task<int> CountMusicAsync(StorageFolder folder)
        {
            int count = (await folder.GetFilesAsync()).Count(f => f.IsMusicFile());
            foreach (var sub in await folder.GetFoldersAsync())
                count += await CountMusicAsync(sub);
            return count;
        }

        public void PauseLoading()
        {
            LoadingStatus = ExecutionStatus.Break;
        }
        public async Task<bool> CheckNewFile(TreeUpdateData data = null)
        {
            // Use Progress for music added, Max for music removed.
            LoadingStatus = ExecutionStatus.Running;
            foreach (var tree in Trees)
                await tree.CheckNewFile(data);
            if (LoadingStatus == ExecutionStatus.Break) return false;
            var pathSet = Files.Select(m => m.Path).ToHashSet();
            var newList = new List<Music>();
            var newSet = new HashSet<string>();
            foreach (var file in await (await GetStorageFolder()).GetFilesAsync())
            {
                if (LoadingStatus == ExecutionStatus.Break) return false;
                if (!file.IsMusicFile()) continue;
                newSet.Add(file.Path);
                if (!pathSet.Contains(file.Path))
                {
                    Music music = await Music.GetMusicAsync(file);
                    newList.Add(music);
                    data.More++;
                    MusicLibraryPage.AllSongs.Add(music); // Temporary
                    Settings.settings.AddMusic(music);
                }
            }
            int before = Files.Count;
            foreach (var music in Files.FindAll(m => !newSet.Contains(m.Path)))
            {
                if (LoadingStatus == ExecutionStatus.Break) return false;
                MusicLibraryPage.AllSongs.Remove(music);
                Files.Remove(music);
                Settings.settings.DeleteMusic(music);
            }
            data.Less += before - Files.Count;
            Files.AddRange(newList);
            Sort();
            LoadingStatus = ExecutionStatus.Ready;
            return true;
        }
        public async Task<bool> Init(StorageFolder folder, TreeOperationProgressListener listener = null)
        {
            return await Init(folder, listener, new TreeOperationProgressIndicator() { Max = listener == null ? 0 : await CountMusicAsync(folder) });
        }
        private async Task<bool> Init(StorageFolder folder, TreeOperationProgressListener listener, TreeOperationProgressIndicator indicator)
        {
            LoadingStatus = ExecutionStatus.Running;
            var samePath = folder.Path == Path;
            if (!string.IsNullOrEmpty(Path) && !samePath && folder.Path.StartsWith(Path))
            {
                // New folder is a Subfolder of the current folder
                FolderTree tree = FindTree(folder.Path);
                CopyFrom(tree);
                listener.Update(folder.DisplayName, "", 0, 0);
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
                        await tree.Init(subFolder, listener, indicator);
                    }
                    if (tree.Files.Count != 0) newTree.Trees.Add(tree);
                }
                foreach (var file in await folder.GetFilesAsync())
                {
                    if (LoadingStatus == ExecutionStatus.Break) return false;
                    if (file.IsMusicFile())
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
                // or same folder
                var trees = new List<FolderTree>();
                foreach (var tree in Trees)
                {
                    tree.Clear();
                    trees.Add(tree);
                }
                Trees.Clear();
                foreach (var subFolder in await folder.GetFoldersAsync())
                {
                    if (LoadingStatus == ExecutionStatus.Break) return false;
                    var source = trees.FirstOrDefault(t => t.Path == subFolder.Path);
                    var tree = new FolderTree()
                    {
                        Criteria = source == null ? SortBy.Title : source.Criteria
                    };
                    await tree.Init(subFolder, listener, indicator);
                    if (tree.Files.Count != 0) Trees.Add(tree);
                }
                Files.Clear();
                foreach (var file in await folder.GetFilesAsync())
                {
                    if (LoadingStatus == ExecutionStatus.Break) return false;
                    if (file.IsMusicFile())
                    {
                        Music music = await Music.GetMusicAsync(file);
                        if (samePath)
                        {
                            if (MusicLibraryPage.AllSongsSet.FirstOrDefault(m => m == music) is Music oldItem)
                            {
                                music.PlayCount = oldItem.PlayCount;
                                music.Favorite = oldItem.Favorite;
                            }
                        }
                        listener?.Update(folder.DisplayName, music.Name, indicator.Update(), indicator.Max);
                        Files.Add(music);
                    }
                }
            }
            Path = folder.Path;
            Sort();
            LoadingStatus = ExecutionStatus.Ready;
            return true;
        }

        public bool Contains(Music music)
        {
            return Files.Contains(music) || Trees.Any(tree => tree.Contains(music));
        }

        public bool RemoveMusic(Music music)
        {
            return Files.Remove(music) || (Trees.FirstOrDefault(t => music.Path.StartsWith(t.Path)) is FolderTree tree && tree.RemoveMusic(music));
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
            Criteria = tree.Criteria;
            Sort();
            OnPropertyChanged();
            return this;
        }
        public List<Music> Flatten()
        {
            List<Music> list = new List<Music>();
            foreach (var branch in Trees)
                list.AddRange(branch.Flatten());
            list.AddRange(Files);
            return list;
        }
        public void Sort()
        {
            switch (Criteria)
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
            Criteria = SortBy.Title;
            return Files = Files.OrderBy(m => m.Name).ToList();
        }
        public List<Music> SortByArtist()
        {
            Criteria = SortBy.Artist;
            return Files = Files.OrderBy(m => m.Artist).ToList();
        }
        public List<Music> SortByAlbum()
        {
            Criteria = SortBy.Album;
            return Files = Files.OrderBy(m => m.Album).ToList();
        }
        public List<Music> Reverse()
        {
            Files.Reverse();
            return Files;
        }
        public void Clear()
        {
            Trees.Clear();
            Files.Clear();
        }
        public FolderTree FindTree(FolderTree target)
        {
            //return Trees.FirstOrDefault(tree => tree.Equals(target)) ?? Trees.FirstOrDefault(tree => target.Path.StartsWith(tree.Path))?.FindTree(target);
            return FindTree(target.Path);
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
        public Music FindMusic(Music target)
        {
            return Files.FirstOrDefault(m => m == target) ?? Trees.FirstOrDefault(tree => target.Path.StartsWith(tree.Path))?.FindMusic(target);
        }
        public async Task<StorageFolder> GetStorageFolder() { return await StorageFolder.GetFolderFromPathAsync(Path);  }

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

    public class TreeOperationProgressIndicator
    {
        public int Progress { get; set; } = 0;
        public int Max { get; set; } = 0;
        public int Update() { return ++Progress; }
    }

    public class TreeUpdateData
    {
        public int More { get; set; } = 0;
        public int Less { get; set; } = 0;
    }

    public interface TreeOperationProgressListener
    {
        void Update(string folder, string file, int progress, int max);
    }
}

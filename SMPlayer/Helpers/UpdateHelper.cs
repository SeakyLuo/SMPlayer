using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SMPlayer.Helpers
{
    public static class UpdateHelper
    {
        private const string JsonFileName = "UpdateLogger";
        public static UpdateLog Log;
        private static readonly List<IAfterPathSetListener> listeners = new List<IAfterPathSetListener>();
        private static TreeUpdateListener treeUpdateListener = new TreeUpdateListener();
        private static volatile ExecutionStatus LoadingStatus = ExecutionStatus.Ready;

        public static async Task Init()
        {
            MainPage.MainPageLoadedListeners.Add(() =>
            {
                MainPage.Instance.Loader.BreakLoadingListeners.Add(() => PauseLoading());
            });
            Log = await JsonFileHelper.ReadObjectAsync<UpdateLog>(JsonFileName) ?? new UpdateLog();
        }

        public static void Save()
        {
            JsonFileHelper.SaveAsync(JsonFileName, Log);
        }

        public static async Task Update()
        {
            Log.DateAdded = await UpdateMusicLibrary(Helper.LocalizeMessage("Updating"));
            Save();
        }

        public static void UpdateIds()
        {
            UpdateTreeFileId(Settings.settings.Tree);
            foreach (var playlist in Settings.settings.Playlists)
            {
                UpdatePlaylistId(playlist);
            }
            UpdatePlaylistId(Settings.settings.MyFavorites);
            Log.NewSettings = true;
            Save();
        }

        private static void UpdatePlaylistId(Playlist playlist)
        {
            playlist.Id = Settings.settings.IdGenerator.GeneratePlaylistId();
            foreach (var music in playlist.Songs)
            {
                if (Settings.FindMusic(music) is Music m)
                {
                    music.Id = m.Id;
                }
            }
        }

        private static void UpdateTreeFileId(FolderTree tree)
        {
            foreach (var branch in tree.Trees)
            {
                UpdateTreeFileId(branch);
            }
            foreach (var file in tree.Files)
            {
                file.Id = Settings.settings.IdGenerator.GenerateMusicId();
            }
        }

        public static void AddAfterPathSetListener(IAfterPathSetListener listener)
        {
            listeners.Add(listener);
        }

        public static void NotifyLibraryChange(string path) { foreach (var listener in listeners) listener.PathSet(path); }

        public static void PauseLoading()
        {
            LoadingStatus = ExecutionStatus.Break;
        }

        public static async Task<bool> UpdateMusicLibrary(string message = null)
        {
            return Helper.CurrentFolder == null || await UpdateMusicLibrary(Helper.CurrentFolder, message);
        }

        public static async Task<bool> UpdateMusicLibrary(StorageFolder folder, string message = null)
        {
            MainPage.Instance.Loader.ShowDeterminant(message ?? "LoadMusicLibrary", true);
            treeUpdateListener.Message = message;
            FolderTree tree = new FolderTree();
            TreeOperationIndicator indicator = new TreeOperationIndicator()
            {
                Max = treeUpdateListener == null ? 0 : await folder.CountFilesAsync()
            };
            if (!await Init(tree, folder, treeUpdateListener, indicator))
            {
                return false;
            }
            MainPage.Instance.Loader.SetLocalizedText(message ?? "UpdateMusicLibrary");
            Helper.CurrentFolder = folder;

            Settings.settings.Tree = Settings.settings.Tree.IsEmpty ? tree : MergeTree(Settings.settings.Tree, tree);
            Settings.settings.RootPath = folder.Path;

            MainPage.Instance.Loader.Progress = 0;
            MainPage.Instance.Loader.Max = listeners.Count;
            for (int i = 0; i < listeners.Count;)
            {
                var listener = listeners[i];
                listener.PathSet(folder.Path);
                MainPage.Instance.Loader.Progress = ++i;
            }
            MediaHelper.RemoveBadMusic();
            App.Save();
            MainPage.Instance.Loader.Hide();
            return true;
        }

        private static async Task<bool> Init(FolderTree tree, StorageFolder folder, ITreeUpdateListener listener, TreeOperationIndicator indicator)
        {
            LoadingStatus = ExecutionStatus.Running;
            var samePath = folder.Path == tree.Path;
            if (string.IsNullOrEmpty(tree.Path))
            {
                // New folder tree
                foreach (var subFolder in await folder.GetFoldersAsync())
                {
                    if (LoadingStatus == ExecutionStatus.Break) return false;
                    var branch = new FolderTree();
                    await Init(branch, subFolder, listener, indicator);
                    if (branch.IsNotEmpty) tree.Trees.Add(branch);
                }
                foreach (var file in await folder.GetFilesAsync())
                {
                    if (LoadingStatus == ExecutionStatus.Break) return false;
                    if (file.IsMusicFile())
                    {
                        Music music = await Music.LoadFromFileAsync(file);
                        Settings.settings.AddMusic(music);
                        listener?.OnUpdate(folder.DisplayName, music.Name, indicator.Update(), indicator.Max);
                        tree.Files.Add(new FolderFile(music));
                    }
                }
            }
            else if (!samePath && folder.Path.StartsWith(tree.Path))
            {
                // New folder is a Subfolder of the current folder
                FolderTree branch = tree.FindTree(folder.Path);
                tree.CopyFrom(branch);
                listener?.OnUpdate(folder.DisplayName, "", 0, 0);
            }
            else if (!samePath && tree.Path.StartsWith(folder.Path))
            {
                // Current folder is a Subfolder of the new folder
                FolderTree newTree = new FolderTree();
                var folders = await folder.GetFoldersAsync();
                foreach (var subFolder in folders)
                {
                    if (LoadingStatus == ExecutionStatus.Break) return false;
                    FolderTree branch;
                    if (subFolder.Path == tree.Path)
                    {
                        branch = new FolderTree(tree);
                    }
                    else
                    {
                        branch = new FolderTree();
                        await Init(branch, subFolder, listener, indicator);
                    }
                    if (tree.IsNotEmpty) newTree.Trees.Add(tree);
                }
                foreach (var file in await folder.GetFilesAsync())
                {
                    if (LoadingStatus == ExecutionStatus.Break) return false;
                    if (file.IsMusicFile())
                    {
                        Music music = await Music.LoadFromFileAsync(file);
                        Settings.settings.AddMusic(music);
                        listener?.OnUpdate(folder.DisplayName, music.Name, indicator.Update(), indicator.Max);
                        newTree.Files.Add(new FolderFile(music));
                    }
                }
                tree.CopyFrom(newTree);
            }
            else
            {
                // No hierarchy between folders
                // or update folder
                var trees = new List<FolderTree>();
                foreach (var subFolder in await folder.GetFoldersAsync())
                {
                    if (LoadingStatus == ExecutionStatus.Break) return false;
                    var source = tree.Trees.FirstOrDefault(t => t.Path == subFolder.Path);
                    var branch = new FolderTree()
                    {
                        Criterion = source?.Criterion ?? SortBy.Title
                    };
                    await Init(branch, subFolder, listener, indicator);
                    if (branch.IsNotEmpty) trees.Add(branch);
                }
                tree.Clear();
                tree.Trees = trees;
                foreach (var file in await folder.GetFilesAsync())
                {
                    if (LoadingStatus == ExecutionStatus.Break) return false;
                    if (file.IsMusicFile())
                    {
                        Music music = await Music.LoadFromFileAsync(file);
                        Settings.settings.AddMusic(music);
                        listener?.OnUpdate(folder.DisplayName, music.Name, indicator.Update(), indicator.Max);
                        tree.Files.Add(new FolderFile(music));
                    }
                }
            }
            tree.Path = folder.Path;
            tree.Sort();
            LoadingStatus = ExecutionStatus.Ready;
            return true;
        }

        /**
         * Merge source to target
         * 注意会把老树中存在、新树中不存在的项目加到新树中，不然老信息会丢失，而用户无感知这样不太好
         */
        private static FolderTree MergeTree(FolderTree source, FolderTree target)
        {
            foreach (var branch in target.Trees)
            {
                if (source.FindTree(branch) is FolderTree tree)
                {
                    // 如果有能合并的老树
                    MergeTree(branch, tree);
                }
                else
                {
                    // 没有的话这就是棵新树
                    branch.Id = Settings.settings.IdGenerator.GenerateTreeId();
                }
            }
            foreach (var item in source.Trees)
            {
                if (target.FindTree(item) == null)
                {
                    target.Trees.Add(item);
                }
            }
            foreach (var item in source.Files)
            {
                if (target.FindFile(item) == null)
                {
                    target.Files.Add(item);
                }
            }
            target.Id = source.Id;
            target.Path = source.Path;
            target.Criterion = source.Criterion;
            target.Sort();
            return target;
        }

        public static async void CheckNewMusic(FolderTree tree, Action<FolderTree> afterTreeUpdated = null)
        {
            MainPage.Instance?.Loader.ShowIndeterminant("ProcessRequest");
            var data = new TreeUpdateData();
            StorageFolder folder;
            try
            {
                folder = await tree.GetStorageFolderAsync();
                if (folder == null)
                {
                    data.Message = Helper.LocalizeMessage("FolderNotFound", tree.Path);
                    ExitChecking(data);
                    return;
                }
            }
            catch (UnauthorizedAccessException)
            {
                data.Message = Helper.LocalizeMessage("UnauthorizedAccessException", tree.Path);
                ExitChecking(data);
                return;
            }
            if (!await CheckNewFile(tree, folder, data))
            {
                ExitChecking(data);
                return;
            }
            if (data.More != 0 || data.Less != 0)
            {
                Settings.settings.Tree.FindTree(tree).CopyFrom(tree);
                NotifyLibraryChange(tree.Path);
                afterTreeUpdated?.Invoke(tree);
                App.Save();
            }
            MainPage.Instance?.Loader.Hide();
            string message;
            if (data.More == 0 && data.Less == 0)
                message = Helper.LocalizeMessage("CheckNewMusicResultNoChange");
            else if (data.More == 0)
                message = Helper.LocalizeMessage("CheckNewMusicResultRemoved", data.Less);
            else if (data.Less == 0)
                message = Helper.LocalizeMessage("CheckNewMusicResultAdded", data.More);
            else
                message = Helper.LocalizeMessage("CheckNewMusicResultChange", data.More, data.Less);
            Helper.ShowNotificationRaw(message);
        }

        private static TreeUpdateData ExitChecking(TreeUpdateData data)
        {
            if (!string.IsNullOrEmpty(data.Message))
                MainPage.Instance.ShowNotification(data.Message);
            MainPage.Instance?.Loader.Hide();
            return data;
        }

        /**
         * 同MergeTree，这里不做删除
         */ 
        private static async Task<bool> CheckNewFile(FolderTree tree, StorageFolder folder, TreeUpdateData data)
        {
            LoadingStatus = ExecutionStatus.Running;
            var pathSet = new HashSet<string>(); // folder path set
            foreach (var subFolder in await folder.GetFoldersAsync())
            {
                if (LoadingStatus == ExecutionStatus.Break) return false;
                if (tree.Trees.FirstOrDefault(t => t.Path == subFolder.Path) is FolderTree branch)
                {
                    await CheckNewFile(branch, subFolder, data);
                }
                else
                {
                    branch = new FolderTree();
                    if (!await Init(branch, subFolder, null, null)) return false;
                    if (branch.IsNotEmpty)
                    {
                        branch.Id = Settings.settings.IdGenerator.GenerateTreeId();
                        tree.Trees.Add(branch);
                        data.More += branch.FileCount;
                    }
                }
                pathSet.Add(subFolder.Name);
            }
            pathSet = tree.Files.Select(m => m.Path).ToHashSet(); // file path set
            var newFolderFilePathSet = new HashSet<string>();
            foreach (var file in await folder.GetFilesAsync())
            {
                if (LoadingStatus == ExecutionStatus.Break) return false;
                if (!file.IsMusicFile()) continue;
                newFolderFilePathSet.Add(file.Path);
                if (!pathSet.Contains(file.Path))
                {
                    Music music = await Music.LoadFromFileAsync(file);
                    tree.Files.Add(new FolderFile(music));
                    data.More++;
                    Settings.settings.AddMusic(music);
                }
            }
            foreach (var file in tree.Files.FindAll(m => !newFolderFilePathSet.Contains(m.Path)))
            {
                if (LoadingStatus == ExecutionStatus.Break) return false;
                tree.Files.Remove(file);
                data.Less++;
                Settings.settings.RemoveFile(file);
            }
            tree.Sort();
            LoadingStatus = ExecutionStatus.Ready;
            return true;
        }
    }

    public interface IAfterPathSetListener
    {
        void PathSet(string path);
    }

    public interface ITreeUpdateListener
    {
        void OnUpdate(string FolderName, string Filename, int Progress, int Max);
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

    public class UpdateLog
    {
        public string LastReleaseNotesVersion { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public bool ShowReleaseNotesDialog { get => LastReleaseNotesVersion != Helper.AppVersion; }
        public bool DateAdded { get; set; } = false;
        public bool NewSettings { get; set; } = false;
        [Newtonsoft.Json.JsonIgnore]
        public bool AllUpdated { get => DateAdded && NewSettings; }
    }

    class TreeUpdateListener : ITreeUpdateListener
    {
        public string Message { get; set; }

        void ITreeUpdateListener.OnUpdate(string FolderName, string Filename, int Progress, int Max)
        {
            bool isDeterminant = Max != 0;
            if (MainPage.Instance.Loader.IsDeterminant != isDeterminant)
                MainPage.Instance.Loader.IsDeterminant = isDeterminant;
            if (isDeterminant)
            {
                MainPage.Instance.Loader.Max = Max;
                MainPage.Instance.Loader.Progress = Progress;
                MainPage.Instance.Loader.Text = Message ?? Filename;
            }
        }
    }
}

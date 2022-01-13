using SMPlayer.Models;
using SQLite;
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
        private static readonly List<IAfterPathSetListener> listeners = new List<IAfterPathSetListener>();
        private static TreeUpdateListener treeUpdateListener = new TreeUpdateListener();
        private static volatile ExecutionStatus LoadingStatus = ExecutionStatus.Ready;

        public static void AddAfterPathSetListener(IAfterPathSetListener listener)
        {
            listeners.Add(listener);
        }

        public static void NotifyLibraryChange(string path) { foreach (var listener in listeners) listener.PathSet(path); }

        public static async Task<bool> UpdateMusicLibrary(string message = null)
        {
            return Helper.CurrentFolder == null || await UpdateMusicLibrary(Helper.CurrentFolder, message);
        }

        public static async Task<bool> UpdateMusicLibrary(StorageFolder folder, string loadingMessage = null)
        {
            MainPage.Instance.Loader.ShowDeterminant(loadingMessage ?? "LoadMusicLibrary", true);
            treeUpdateListener.Message = loadingMessage;
            TreeOperationIndicator indicator = new TreeOperationIndicator()
            {
                Max = treeUpdateListener == null ? 0 : await folder.CountFilesAsync()
            };
            if (!await SQLHelper.RunAsync(async c => await LoadFolder(c, folder, treeUpdateListener, indicator)))
            {
                return false;
            }
            Helper.CurrentFolder = folder;
            Settings.settings.Tree = Settings.FindFolderInfo(folder.Path);
            Settings.settings.RootPath = folder.Path;

            if (loadingMessage != null)
                MainPage.Instance.Loader.SetMessage(loadingMessage);
            MainPage.Instance.Loader.Progress = 0;
            MainPage.Instance.Loader.Max = listeners.Count;
            for (int i = 0; i < listeners.Count; i++)
            {
                var listener = listeners[i];
                listener.PathSet(folder.Path);
                MainPage.Instance.Loader.Progress = i + 1;
            }
            MusicPlayer.RemoveBadMusic();
            App.Save();
            MainPage.Instance.Loader.Hide();
            return true;
        }

        private static async Task<bool> LoadFolder(SQLiteConnection c, StorageFolder folder, ITreeUpdateListener listener, TreeOperationIndicator indicator, TreeUpdateData updateData = null)
        {
            LoadingStatus = ExecutionStatus.Running;
            FolderTree tree = new FolderTree();
            await LoadFolder(c, folder, tree, listener, indicator);
            listener?.OnUpdate(Helper.LocalizeMessage("UpdateMusicLibrary"), indicator.Update(), indicator.Max);
            ResetFolderData(c, tree, updateData);
            LoadingStatus = ExecutionStatus.Done;
            return true;
        }

        private static async Task<bool> LoadFolder(SQLiteConnection c, StorageFolder folder, FolderTree tree, ITreeUpdateListener listener, TreeOperationIndicator indicator)
        {
            foreach (var subFolder in await folder.GetFoldersAsync())
            {
                if (LoadingStatus == ExecutionStatus.Break) return false;
                var branch = new FolderTree();
                await LoadFolder(c, subFolder, branch, listener, indicator);
                if (branch.IsNotEmpty)
                {
                    tree.Trees.Add(branch);
                }
            }
            foreach (var file in await folder.GetFilesAsync())
            {
                if (LoadingStatus == ExecutionStatus.Break) return false;
                if (file.IsMusicFile())
                {
                    Music music = await Music.LoadFromFileAsync(file);
                    listener?.OnUpdate(music.Name, indicator.Update(), indicator.Max);
                    tree.Files.Add(music.ToFolderFile());
                }
            }
            if (tree.IsEmpty)
            {
                return true;
            }
            tree.Path = folder.Path;
            return true;
        }

        /**
         * 重置文件夹数据库
         */
        private static void ResetFolderData(SQLiteConnection c, FolderTree folder, TreeUpdateData updateData)
        {
            FolderTree existing = c.SelectFolderInfoByPath(folder.Path);
            if (existing == null)
            {
                c.InsertFolder(folder);
            }
            else
            {
                folder.Id = existing.Id;
            }
            foreach (var item in folder.Trees)
            {
                item.ParentId = folder.Id;
                ResetFolderData(c, item, updateData);
            }
            HashSet<string> currentFilePathSet = folder.Files.Select(f => f.Path).ToHashSet();
            Dictionary<string, FolderFile> existingFileDict = c.SelectSubFiles(folder).ToDictionary(i => i.Path);
            foreach (var entry in existingFileDict)
            {
                if (!currentFilePathSet.Contains(entry.Key))
                {
                    Settings.settings.RemoveFile(entry.Value);
                    if (updateData != null) updateData.Removed++;
                }
            }
            foreach (var item in folder.Files)
            {
                item.ParentId = folder.Id;
                existingFileDict.TryGetValue(item.Path, out FolderFile existingFile);
                if (existingFile != null)
                {
                    if (item.FileType == FileType.Music)
                    {
                        Music music = (Music)item.Source;
                        music.Id = existingFile.FileId;
                        Settings.settings.AddMusic(c, music);
                    }
                }
                else
                {
                    if (item.FileType == FileType.Music)
                    {
                        Music music = (Music)item.Source;
                        Settings.settings.AddMusic(c, music);
                        item.FileId = music.Id;
                    }
                    if (updateData != null) updateData.Added++;
                    c.InsertFile(item);
                }
            }
        }

        public static async void RefreshFolder(FolderTree tree, Action<FolderTree> afterTreeUpdated = null)
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
            if (!await SQLHelper.RunAsync(async c => await LoadFolder(c, folder, null, null, data)))
            {
                ExitChecking(data);
                return;
            }
            if (data.Added != 0 || data.Removed != 0)
            {
                NotifyLibraryChange(tree.Path);
                afterTreeUpdated?.Invoke(tree);
                App.Save();
            }
            MainPage.Instance?.Loader.Hide();
            string message;
            if (data.Added == 0 && data.Removed == 0)
                message = Helper.LocalizeMessage("CheckNewMusicResultNoChange");
            else if (data.Added == 0)
                message = Helper.LocalizeMessage("CheckNewMusicResultRemoved", data.Removed);
            else if (data.Removed == 0)
                message = Helper.LocalizeMessage("CheckNewMusicResultAdded", data.Added);
            else
                message = Helper.LocalizeMessage("CheckNewMusicResultChange", data.Added, data.Removed);
            Helper.ShowNotificationRaw(message);
        }

        private static TreeUpdateData ExitChecking(TreeUpdateData data)
        {
            if (!string.IsNullOrEmpty(data.Message))
                MainPage.Instance.ShowNotification(data.Message);
            MainPage.Instance?.Loader.Hide();
            return data;
        }
    }

    public interface IAfterPathSetListener
    {
        void PathSet(string path);
    }

    public interface ITreeUpdateListener
    {
        void OnUpdate(string Filename, int Progress, int Max);
    }

    public class TreeOperationIndicator
    {
        public int Progress { get; set; } = 0;
        public int Max { get; set; } = 0;
        public int Update() { return ++Progress; }
    }

    public class TreeUpdateData
    {
        public int Added { get; set; } = 0;
        public int Removed { get; set; } = 0;
        public string Message { get; set; }
    }

    public class UpdateLog
    {
        public string LastReleaseNotesVersion { get; set; }
    }

    class TreeUpdateListener : ITreeUpdateListener
    {
        public string Message { get; set; }

        void ITreeUpdateListener.OnUpdate(string message, int progress, int max)
        {
            bool isDeterminant = max != 0;
            if (MainPage.Instance.Loader.IsDeterminant != isDeterminant)
                MainPage.Instance.Loader.IsDeterminant = isDeterminant;
            if (isDeterminant)
            {
                MainPage.Instance.Loader.Max = max;
                MainPage.Instance.Loader.Progress = progress;
                MainPage.Instance.Loader.SetRawMessage(Message ?? message);
            }
        }
    }
}

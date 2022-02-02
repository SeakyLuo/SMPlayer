using SMPlayer.Dialogs;
using SMPlayer.Models;
using SMPlayer.Models.DAO;
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
        private static volatile ExecutionStatus LoadingStatus = ExecutionStatus.Ready;

        public static async Task<bool> UpdateMusicLibrary(StorageFolder folder)
        {
            if (folder == null) return true;
            MainPage.Instance.Loader.ShowDeterminant("LoadMusicLibrary", true);
            MainPage.Instance.Loader.Max = await folder.CountFilesAsync();
            MainPage.Instance.Loader.BreakLoadingListener = () =>
            {
                LoadingStatus = ExecutionStatus.Break;
            };
            LoadingStatus = ExecutionStatus.Running;
            FolderTree tree = Settings.FindFolder(folder.Path) ?? new FolderTree(folder.Path);
            NotifyFolderEvent(Settings.settings.Tree, StorageItemEventType.BeforeReset);
            bool unbroken = await LoadFolder(folder, tree);
            MainPage.Instance.Loader.AllowBreak = false;
            await MainPage.Instance.Loader.ResetAsync("UpdateMusicLibrary");
            await ResetFolderData(tree);
            if (Settings.settings.Tree.Id != 0 && !Settings.settings.Tree.Equals(tree))
            {
                await MainPage.Instance.Loader.ResetAsync("ClearExpiredData");
                ClearOldTree(Settings.Root, tree);
            }
            Helper.CurrentFolder = folder;
            Settings.settings.Tree = tree;
            Settings.settings.RootPath = folder.Path;
            await MainPage.Instance.Loader.ResetAsync("ResyncData", Settings.StorageItemEventListeners.Count);
            for (int i = 0; i < Settings.StorageItemEventListeners.Count; i++)
            {
                var listener = Settings.StorageItemEventListeners[i];
                listener.ExecuteFolderEvent(Settings.settings.Tree, new StorageItemEventArgs(StorageItemEventType.Reset));
                await MainPage.Instance.Loader.IncrementAsync();
            }
            MusicPlayer.RemoveBadMusic();
            App.Save();
            LoadingStatus = ExecutionStatus.Done;
            MainPage.Instance.Loader.Hide();
            return true;
        }

        private static async Task<bool> LoadFolder(StorageFolder folder, FolderTree tree)
        {
            HashSet<string> existingFolders = tree.Trees.Select(i => i.Path).ToHashSet();
            HashSet<string> newFolders = new HashSet<string>();
            List<FolderTree> newBranches = new List<FolderTree>();
            foreach (var subFolder in await folder.GetFoldersAsync())
            {
                if (LoadingStatus == ExecutionStatus.Break) return false;
                var branch = Settings.FindFolder(subFolder.Path) ?? new FolderTree(subFolder.Path);
                await LoadFolder(subFolder, branch);
                newFolders.Add(subFolder.Path);
                if (branch.IsNotEmpty)
                {
                    newBranches.Add(branch);
                }
            }
            foreach (var branch in tree.Trees)
            {
                if (!newFolders.Contains(branch.Path))
                {
                    FolderTree folderTree = Settings.FindFullFolder(branch.Id);
                    folderTree.State = ActiveState.Inactive;
                    newBranches.Add(folderTree);
                }
            }
            tree.Trees = newBranches;
            HashSet<string> existingFiles = tree.Files.Select(i => i.Path).ToHashSet();
            HashSet<string> newFiles = new HashSet<string>();
            foreach (var file in await folder.GetFilesAsync())
            {
                if (LoadingStatus == ExecutionStatus.Break) return false;
                if (file.IsMusicFile())
                {
                    newFiles.Add(file.Path);
                    if (!existingFiles.Contains(file.Path))
                    {
                        Music music = await Music.LoadFromFileAsync(file);
                        tree.Files.Add(music.ToFolderFile());
                        MainPage.Instance.Loader.Increment(file.Name);
                    }
                }
            }
            foreach (var file in tree.Files)
            {
                if (!newFiles.Contains(file.Path))
                {
                    file.State = ActiveState.Inactive;
                }
            }
            return true;
        }

        /**
         * 重置文件夹数据库
         */
        private static async Task ResetFolderData(FolderTree folder, FolderUpdateResult result = null)
        {
            if (folder.State.IsInactive())
            {
                RemoveFolder(folder, result);
                return;
            }
            SQLHelper.Run(c =>
            {
                FolderTree existing = c.SelectAnyFolderInfo(folder.Path);
                if (existing == null)
                {
                    c.InsertFolder(folder);
                    result?.AddFolder(folder.Path);
                }
                else
                {
                    folder.Id = existing.Id;
                    folder.Criterion = existing.Criterion;
                    c.Update(folder.ToDAO());
                }
            });
            foreach (var item in folder.Trees)
            {
                item.ParentId = folder.Id;
                await ResetFolderData(item, result);
            }
            folder.Trees.RemoveAll(i => i.State.IsInactive() || i.IsEmpty);
            foreach (var item in folder.Files)
            {
                if (item.Id == 0)
                {
                    item.ParentId = folder.Id;
                    await Settings.settings.AddFile(item);
                    result?.AddFile(item.Path);
                    Log.Debug("file is added, path {0}", item.Path);
                    await MainPage.Instance.Loader.IncrementAsync();
                }
                else
                {
                    if (item.State.IsInactive())
                    {
                        result?.RemoveFile(item.Path);
                        Settings.settings.RemoveFile(item);
                        Log.Info("file is deleted, path {0}", item.Path);
                    }
                }
            }
            folder.Files.RemoveAll(i => i.State.IsInactive());
            if (folder.ParentId > 0 && folder.IsEmpty)
            {
                RemoveFolder(folder, result);
            }
        }

        private static void RemoveFolder(FolderTree folder, FolderUpdateResult result = null)
        {
            Log.Info("folder is deleted, path {0}", folder.Path);
            FolderDAO folderDAO = folder.ToDAO();
            folderDAO.State = ActiveState.Inactive;
            SQLHelper.Run(c => c.Update(folderDAO));
            result?.RemoveFolder(folder.Path);
            foreach (var item in folder.Trees)
            {
                RemoveFolder(Settings.FindFolder(item.Id), result);
            }
            foreach (var item in folder.Files)
            {
                RemoveFile(item, result);
            }
        }

        private static void RemoveFile(FolderFile item, FolderUpdateResult result = null)
        {
            result?.RemoveFile(item.Path);
            Settings.settings.RemoveFile(item);
            Log.Info("file is deleted, path {0}", item.Path);
        }

        private static void ClearOldTree(FolderTree oldRoot, FolderTree newRoot)
        {
            // 如果oldRoot是newRoot的子文件夹，可以不删
            if (oldRoot.Path.Contains(newRoot.Path))
            {
                return;
            }
            // 如果newRoot是oldRoot的子文件夹，删除其他分支的数据
            if (newRoot.Path.Contains(oldRoot.Path))
            {
                foreach (var item in oldRoot.Trees)
                {
                    FolderTree branch = Settings.FindFolder(item.Id);
                    if (newRoot.Path.Contains(item.Path))
                    {
                        ClearOldTree(branch, newRoot);
                    }
                    else
                    {
                        RemoveFolder(branch);
                    }
                }
                foreach (var item in oldRoot.Files)
                {
                    RemoveFile(item);
                }
                return;
            }
            // 两者没有关系，清除全部数据
            foreach (var item in oldRoot.Trees)
            {
                ClearOldTree(Settings.FindFolder(item.Id), newRoot);
            }
        }

        public static async void RefreshFolder(FolderTree tree)
        {
            MainPage.Instance?.Loader.ShowIndeterminant("ProcessRequest");
            StorageFolder storageFolder;
            try
            {
                storageFolder = await tree.GetStorageFolderAsync();
                if (storageFolder == null)
                {
                    ExitChecking(Helper.LocalizeMessage("FolderNotFound", tree.Path));
                    return;
                }
            }
            catch (UnauthorizedAccessException)
            {
                ExitChecking(Helper.LocalizeMessage("UnauthorizedAccessException", tree.Path));
                return;
            }
            LoadingStatus = ExecutionStatus.Running;
            FolderTree folderTree = Settings.FindFolder(tree.Id);
            bool unbroken = await LoadFolder(storageFolder, folderTree);
            if (!unbroken)
            {
                ExitChecking("");
                return;
            }
            var result = new FolderUpdateResult(folderTree.Path);
            await ResetFolderData(folderTree, result);
            if (result.FilesAdded.IsNotEmpty() || result.FilesRemoved.IsNotEmpty())
            {
                NotifyFolderEvent(folderTree, StorageItemEventType.Update);
            }
            result.Finish();
            LoadingStatus = ExecutionStatus.Done;
            if (result.HasChange)
            {
                MainPage.Instance?.ShowButtonedNotification(result.ToDisplayMessage(),
                                                            Helper.LocalizeText("Detail"),
                                                            async () => { await new FolderUpdateResultDialog().ShowAsync(result); },
                                                            5000);
            }
            else
            {
                MainPage.Instance?.ShowLocalizedNotification("CheckNewMusicResultNoChange");
            }
            MainPage.Instance?.Loader.Hide();
        }

        private static void ExitChecking(string message)
        {
            LoadingStatus = ExecutionStatus.Done;
            MainPage.Instance?.Loader.Hide();
            if (!string.IsNullOrEmpty(message))
                MainPage.Instance.ShowNotification(message);
        }

        public static void NotifyFolderEvent(FolderTree folder, StorageItemEventType type)
        {
            StorageItemEventArgs args = new StorageItemEventArgs(type);
            foreach (var listener in Settings.StorageItemEventListeners)
                listener.ExecuteFolderEvent(folder, args);
        }
    }

    public class UpdateLog
    {
        public string LastReleaseNotesVersion { get; set; }
    }
}

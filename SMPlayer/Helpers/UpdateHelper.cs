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

        public static void NotifyLibraryChange(FolderTree folder)
        {
            foreach (var listener in Settings.StorageItemEventListeners)
                listener.ExecuteFolderEvent(folder, new StorageItemEventArgs(StorageItemEventType.Update));
        }

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
            FolderTree tree = Settings.FindFullFolder(folder.Path) ?? new FolderTree();
            bool unbroken = await LoadFolder(folder, tree);
            LoadingStatus = ExecutionStatus.Done;
            await MainPage.Instance.Loader.ResetAsync("UpdateMusicLibrary");
            await ResetFolderData(tree);
            Helper.CurrentFolder = folder;
            Settings.settings.Tree = Settings.FindFolderInfo(folder.Path);
            Settings.settings.RootPath = folder.Path;
            await MainPage.Instance.Loader.ResetAsync();
            MainPage.Instance.Loader.Max = Settings.StorageItemEventListeners.Count;
            for (int i = 0; i < Settings.StorageItemEventListeners.Count; i++)
            {
                var listener = Settings.StorageItemEventListeners[i];
                listener.ExecuteFolderEvent(Settings.settings.Tree, new StorageItemEventArgs(StorageItemEventType.Update));
                await MainPage.Instance.Loader.IncrementAsync();
            }
            MusicPlayer.RemoveBadMusic();
            App.Save();
            MainPage.Instance.Loader.Hide();
            return true;
        }

        private static async Task<bool> LoadFolder(StorageFolder folder, FolderTree tree)
        {
            HashSet<string> existingFolders = tree.Trees.Select(i => i.Path).ToHashSet();
            HashSet<string> newFolders = new HashSet<string>();
            foreach (var subFolder in await folder.GetFoldersAsync())
            {
                if (LoadingStatus == ExecutionStatus.Break) return false;
                var branch = tree.FindFolder(subFolder.Path) ?? new FolderTree();
                await LoadFolder(subFolder, branch);
                newFolders.Add(subFolder.Path);
                if (branch.Id == 0 && branch.IsNotEmpty)
                {
                    tree.Trees.Add(branch);
                }
            }
            foreach (var branch in tree.Trees)
            {
                if (!newFolders.Contains(branch.Path))
                {
                    branch.State = ActiveState.Inactive;
                }
            }
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
        private static async Task ResetFolderData(FolderTree folder, FolderUpdateResult result = null)
        {
            if (folder.State.IsInactive())
            {
                SQLHelper.Run(c => c.Update(folder.ToDAO()));
                result?.RemoveFolder(folder.Path);
                foreach (var item in folder.Trees)
                {
                    item.State = ActiveState.Inactive;
                    await ResetFolderData(item, result);
                }
                foreach (var item in folder.Files)
                {
                    result?.RemoveFile(item.Path);
                    Settings.settings.RemoveFile(item);
                    Log.Info("file is deleted, path {0}", item.Path);
                }
                return;
            }
            SQLHelper.Run(c =>
            {
                FolderTree existing = c.SelectFolderInfoByPath(folder.Path);
                if (existing == null)
                {
                    c.InsertFolder(folder);
                    result?.AddFolder(folder.Path);
                }
                else
                {
                    folder.Id = existing.Id;
                }
            });
            foreach (var item in folder.Trees)
            {
                item.ParentId = folder.Id;
                await ResetFolderData(item, result);
            }
            folder.Trees.RemoveAll(i => i.State.IsInactive());
            foreach (var item in folder.Files)
            {
                if (item.Id == 0)
                {
                    SQLHelper.Run(c =>
                    {
                        item.ParentId = folder.Id;
                        if (item.FileType == FileType.Music)
                        {
                            Music music = (Music)item.Source;
                            Settings.settings.AddMusic(c, music);
                            item.FileId = music.Id;
                        }
                        result?.AddFile(item.Path);
                        c.InsertFile(item);
                        Log.Debug("file is added, path {0}", item.Path);
                    });
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
            FolderTree folderTree = Settings.FindFullFolder(tree.Id);
            bool unbroken = await LoadFolder(storageFolder, folderTree);
            LoadingStatus = ExecutionStatus.Done;
            if (!unbroken)
            {
                ExitChecking("");
                return;
            }
            var result = new FolderUpdateResult();
            await ResetFolderData(folderTree, result);
            if (result.FilesAdded.IsNotEmpty() || result.FilesRemoved.IsNotEmpty())
            {
                NotifyLibraryChange(folderTree);
            }
            MainPage.Instance?.Loader.Hide();
            Helper.ShowNotificationRaw(result.ToDisplayMessage());
        }

        private static void ExitChecking(string message)
        {
            if (!string.IsNullOrEmpty(message))
                MainPage.Instance.ShowNotification(message);
            MainPage.Instance?.Loader.Hide();
        }
    }

    public class UpdateLog
    {
        public string LastReleaseNotesVersion { get; set; }
    }

    class FolderUpdateResult
    {
        public List<string> FilesAdded { get; set; } = new List<string>();
        public List<string> FilesRemoved { get; set; } = new List<string>();
        public List<string> FoldersAdded { get; set; } = new List<string>();
        public List<string> FoldersRemoved { get; set; } = new List<string>();

        public string FirstFileAdded { get => System.IO.Path.GetFileNameWithoutExtension(FilesAdded[0]); }
        public string SecondFileAdded { get => System.IO.Path.GetFileNameWithoutExtension(FilesAdded[1]); }
        public string FirstFileRemoved { get => System.IO.Path.GetFileNameWithoutExtension(FilesRemoved[0]); }
        public string SecondFileRemoved { get => System.IO.Path.GetFileNameWithoutExtension(FilesRemoved[1]); }

        public void AddFile(string path)
        {
            FilesAdded.Add(path);
        }
        public void RemoveFile(string path) 
        {
            FilesRemoved.Add(path);
        }
        public void AddFolder(string path)
        {
            FoldersAdded.Add(path);
        }
        public void RemoveFolder(string path)
        {
            FoldersRemoved.Add(path);
        }

        public string ToDisplayMessage()
        {
            if (FilesAdded.IsNotEmpty() && FilesRemoved.IsNotEmpty())
            {
                if (FilesAdded.Count == 1 && FilesRemoved.Count == 1)
                {
                    return Helper.LocalizeMessage("CheckNewMusicResult1Added1Removed", FirstFileAdded, FirstFileRemoved);
                }
                else
                {
                    return Helper.LocalizeMessage("CheckNewMusicResultChange", FilesAdded.Count, FilesRemoved.Count);
                }
            }
            if (FilesRemoved.IsNotEmpty())
            {
                if (FilesRemoved.Count == 1)
                {
                    return Helper.LocalizeMessage("CheckNewMusicResult1Removed", FirstFileRemoved);
                }
                else if (FilesRemoved.Count == 2)
                {
                    return Helper.LocalizeMessage("CheckNewMusicResult2Removed", FirstFileRemoved, SecondFileRemoved);
                }
                else
                {
                    return Helper.LocalizeMessage("CheckNewMusicResultMultipleRemoved", FilesRemoved.Count);
                }
            }
            if (FilesAdded.IsNotEmpty())
            {
                if (FilesAdded.Count == 1)
                {
                    return Helper.LocalizeMessage("CheckNewMusicResult1Added", FirstFileAdded);
                }
                else if (FilesAdded.Count == 2)
                {
                    return Helper.LocalizeMessage("CheckNewMusicResult2Added", FirstFileAdded, SecondFileAdded);
                }
                else
                {
                    return Helper.LocalizeMessage("CheckNewMusicResultMultipleAdded", FilesAdded.Count);
                }
            }
            return Helper.LocalizeMessage("CheckNewMusicResultNoChange");
        }
    }
}

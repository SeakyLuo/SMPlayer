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

        public static async Task<bool> UpdateMusicLibrary(string message = null)
        {
            return Helper.CurrentFolder == null || await UpdateMusicLibrary(Helper.CurrentFolder, message);
        }

        public static async Task<bool> UpdateMusicLibrary(StorageFolder folder, string loadingMessage = null)
        {
            MainPage.Instance.Loader.ShowDeterminant(loadingMessage ?? "LoadMusicLibrary", true);
            FolderProgressUpdater updater = new FolderProgressUpdater
            {
                Message = loadingMessage,
                Max = await folder.CountFilesAsync(),
            };
            if (!await SQLHelper.RunAsync(async c => await LoadFolder(c, folder, updater)))
            {
                return false;
            }
            Helper.CurrentFolder = folder;
            Settings.settings.Tree = Settings.FindFolderInfo(folder.Path);
            Settings.settings.RootPath = folder.Path;

            if (loadingMessage != null)
                MainPage.Instance.Loader.SetMessage(loadingMessage);
            MainPage.Instance.Loader.Progress = 0;
            MainPage.Instance.Loader.Max = Settings.StorageItemEventListeners.Count;
            for (int i = 0; i < Settings.StorageItemEventListeners.Count; i++)
            {
                var listener = Settings.StorageItemEventListeners[i];
                listener.ExecuteFolderEvent(Settings.settings.Tree, new StorageItemEventArgs(StorageItemEventType.Update));
                MainPage.Instance.Loader.Progress = i + 1;
            }
            MusicPlayer.RemoveBadMusic();
            App.Save();
            MainPage.Instance.Loader.Hide();
            return true;
        }

        private static async Task<bool> LoadFolder(SQLiteConnection c, StorageFolder folder, FolderProgressUpdater updater)
        {
            LoadingStatus = ExecutionStatus.Running;
            FolderTree tree = new FolderTree();
            bool unbroken = await LoadFolder(c, folder, tree, updater);
            if (await LoadFolder(c, folder, tree, updater))
            {
                updater.UpdateProgressBar(Helper.LocalizeMessage("UpdateMusicLibrary"));
                ResetFolderData(c, tree, updater);
            }
            LoadingStatus = ExecutionStatus.Done;
            return unbroken;
        }

        private static async Task<bool> LoadFolder(SQLiteConnection c, StorageFolder folder, FolderTree tree, FolderProgressUpdater updater)
        {
            foreach (var subFolder in await folder.GetFoldersAsync())
            {
                if (LoadingStatus == ExecutionStatus.Break) return false;
                var branch = tree.FindFolder(subFolder.Path) ?? new FolderTree();
                await LoadFolder(c, subFolder, branch, updater);
                if (branch.Id == 0 && branch.IsNotEmpty)
                {
                    tree.Trees.Add(branch);
                }
            }
            HashSet<string> existing = tree.Files.Select(i => i.Path).ToHashSet();
            HashSet<string> newFiles = new HashSet<string>();
            foreach (var file in await folder.GetFilesAsync())
            {
                if (LoadingStatus == ExecutionStatus.Break) return false;
                if (file.IsMusicFile())
                {
                    newFiles.Add(file.Path);
                    if (!existing.Contains(file.Path))
                    {
                        Music music = await Music.LoadFromFileAsync(file);
                        tree.Files.Add(music.ToFolderFile());
                        updater.UpdateProgressBar(file.Name);
                        Log.Info("file is added, path {0}", file.Path);
                    }
                }
            }
            foreach (var file in tree.Files)
            {
                if (!newFiles.Contains(file.Path))
                {
                    file.State = ActiveState.Inactive;
                    Log.Info("file is deleted, path {0}", file.Path);
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
        private static void ResetFolderData(SQLiteConnection c, FolderTree folder, FolderProgressUpdater updater)
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
                ResetFolderData(c, item, updater);
            }
            foreach (var item in folder.Files)
            {
                if (item.Id == 0)
                {
                    item.ParentId = folder.Id;
                    if (item.FileType == FileType.Music)
                    {
                        Music music = (Music)item.Source;
                        Settings.settings.AddMusic(c, music);
                        item.FileId = music.Id;
                    }
                    updater.Added++;
                    c.InsertFile(item);
                }
                else
                {
                    if (item.State.isInactive())
                    {
                        updater.Removed++;
                        Settings.settings.RemoveFile(item);
                    }
                }
            }
        }

        public static async void RefreshFolder(FolderTree tree)
        {
            MainPage.Instance?.Loader.ShowIndeterminant("ProcessRequest");
            StorageFolder folder;
            try
            {
                folder = await tree.GetStorageFolderAsync();
                if (folder == null)
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
            var updater = new FolderProgressUpdater();
            LoadingStatus = ExecutionStatus.Running;
            FolderTree fullFolder = Settings.FindFullFolder(tree.Id);
            bool unbroken = await SQLHelper.Run(async c => await LoadFolder(c, folder, fullFolder, updater));
            LoadingStatus = ExecutionStatus.Done;
            if (!unbroken)
            {
                ExitChecking("");
                return;
            }
            SQLHelper.Run(c => ResetFolderData(c, fullFolder, updater));
            if (updater.Added != 0 || updater.Removed != 0)
            {
                NotifyLibraryChange(fullFolder);
            }
            MainPage.Instance?.Loader.Hide();
            string message;
            if (updater.Added == 0 && updater.Removed == 0)
                message = Helper.LocalizeMessage("CheckNewMusicResultNoChange");
            else if (updater.Added == 0)
                message = Helper.LocalizeMessage("CheckNewMusicResultRemoved", updater.Removed);
            else if (updater.Removed == 0)
                message = Helper.LocalizeMessage("CheckNewMusicResultAdded", updater.Added);
            else
                message = Helper.LocalizeMessage("CheckNewMusicResultChange", updater.Added, updater.Removed);
            Helper.ShowNotificationRaw(message);
        }

        private static void ExitChecking(string message)
        {
            if (!string.IsNullOrEmpty(message))
                MainPage.Instance.ShowNotification(message);
            MainPage.Instance?.Loader.Hide();
        }
    }

    public interface IFolderUpdateListener
    {
        void FolderUpdated(FolderTree folder);
    }

    public class UpdateLog
    {
        public string LastReleaseNotesVersion { get; set; }
    }

    class FolderProgressUpdater
    {
        public string Message { get; set; }
        public int Progress { get; set; } = 0;
        public int Max { get; set; } = 0;
        public int Added { get; set; } = 0;
        public int Removed { get; set; } = 0;

        public void UpdateProgressBar(string message)
        {
            bool isDeterminant = Max != 0;
            if (MainPage.Instance.Loader.IsDeterminant != isDeterminant)
                MainPage.Instance.Loader.IsDeterminant = isDeterminant;
            if (isDeterminant)
            {
                MainPage.Instance.Loader.Max = Max;
                MainPage.Instance.Loader.Progress = ++Progress;
                MainPage.Instance.Loader.SetRawMessage(Message ?? message);
            }
        }
    }
}

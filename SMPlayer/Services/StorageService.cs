using SMPlayer.Helpers;
using SMPlayer.Interfaces;
using SMPlayer.Models;
using SMPlayer.Models.DAO;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SMPlayer.Services
{
    public class StorageService
    {
        public static void AddStorageItemEventListener(IStorageItemEventListener listener) { StorageItemEventListeners.Add(listener); }
        public static readonly List<IStorageItemEventListener> StorageItemEventListeners = new List<IStorageItemEventListener>();

        public static FolderTree Root { get => FindFolder(Settings.settings.Tree.Id) ?? new FolderTree(); }
        public static List<FolderTree> AllFolders
        {
            get => SQLHelper.Run(c => c.Query<FolderDAO>("select * from Folder where State = ?", ActiveState.Active))
                                       .Select(i => i.FromDAO()).ToList();
        }
        public static FolderTree FindFolderInfo(long id)
        {
            return SQLHelper.Run(c => c.SelectFolderInfo(id));
        }
        public static FolderTree FindFolderInfo(string path)
        {
            return SQLHelper.Run(c => c.SelectFolderInfo(path));
        }
        public static FolderTree FindFolder(long id)
        {
            return SQLHelper.Run(c => c.SelectFolder(id));
        }
        public static FolderTree FindFolder(string path)
        {
            return SQLHelper.Run(c => c.SelectFolder(path));
        }
        public static FolderTree FindFullFolder(long id)
        {
            return SQLHelper.Run(c => c.SelectFullFolder(id));
        }
        public static FolderTree FindFullFolder(string path)
        {
            return SQLHelper.Run(c => c.SelectFullFolder(path));
        }
        public static List<FolderTree> FindSubFolders(FolderTree folder)
        {
            return SQLHelper.Run(c => c.SelectSubFolders(folder)).Select(i => i).OrderBy(i => i.Name).ToList();
        }
        public static List<FolderFile> FindSubFiles(FolderTree folder)
        {
            return SQLHelper.Run(c => c.SelectSubFiles(folder));
        }
        public static FolderFile FindFile(long id)
        {
            return SQLHelper.Run(c => c.SelectFile(id));
        }
        public static FolderFile FindFile(string path)
        {
            return SQLHelper.Run(c => c.SelectFileByPath(path));
        }
        public static List<Music> FindSubSongs(FolderTree folder)
        {
            return SQLHelper.Run(c =>
            {
                return c.SelectMusicByIds(c.SelectSubFiles(folder).Select(i => i.FileId)).ToList();
            });
        }
        public static int FindNextFolderNameIndex(FolderTree parent, string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                var siblings = SQLHelper.Run(c => c.SelectSubFolders(parent)).Select(p => p.Name).ToHashSet();
                for (int i = 1; i <= siblings.Count; i++)
                    if (!siblings.Contains(Helper.GetNextName(name, i)))
                        return i;
            }
            return 0;
        }

        public static string FindNextFolderName(FolderTree parent, string Name)
        {
            int index = FindNextFolderNameIndex(parent, Name);
            return index == 0 ? Name : Helper.GetNextName(Name, index);
        }

        public static async Task AddFile(FolderFile item)
        {
            if (item.IsMusicFile())
            {
                Music music = (Music)item.Source;
                await MusicService.AddMusic(music);
                item.FileId = music.Id;
                SQLHelper.Run(c => c.InsertFile(item));
            }
        }

        public static async Task DeleteFile(FolderFile file)
        {
            await StorageHelper.DeleteFile(file.Path);
            RemoveFile(file);
        }

        public static void RemoveFile(FolderFile file)
        {
            if (file.IsMusicFile())
            {
                MusicService.RemoveMusic(MusicService.FindMusic(file.FileId));
            }
        }

        public static async Task<NamingError> ValidateFolderName(string root, string newName)
        {
            if (string.IsNullOrEmpty(newName) || string.IsNullOrWhiteSpace(newName))
                return NamingError.EmptyOrWhiteSpace;
            if (newName.Length > 50)
                return NamingError.TooLong;
            if (await StorageHelper.FolderExists(Path.Combine(root, newName)))
                return NamingError.Used;
            return NamingError.Good;
        }

        public static void UpdateFolder(FolderTree folder)
        {
            SQLHelper.Run(c => c.Update(folder.ToDAO()));
        }

        /**
         * Add branch to root
         */
        public static async Task AddFolder(FolderTree branch, FolderTree root)
        {
            StorageFolder folder = await root.GetStorageFolderAsync();
            await folder.CreateFolderAsync(branch.Name);
            branch.ParentId = root.Id;
            SQLHelper.Run(c => c.InsertFolder(branch));
            foreach (var listener in StorageItemEventListeners)
                listener.ExecuteFolderEvent(branch, new StorageItemEventArgs(StorageItemEventType.Add) { Folder = root });
        }

        public static async Task RenameFolder(FolderTree original, string newName)
        {
            StorageFolder folder = await original.GetStorageFolderAsync();
            await folder.RenameAsync(newName);
            string newPath = folder.Path;
            FolderTree originalTree = FindFolderInfo(original.Id);
            SQLHelper.Run(c =>
            {
                RenameFolder(c, originalTree, originalTree.Path, newPath);
                c.Execute("update PreferenceItem set ItemName = ? where ItemId = ?", newName, original.Id);
            });
            foreach (var listener in StorageItemEventListeners)
                listener.ExecuteFolderEvent(original, new StorageItemEventArgs(StorageItemEventType.Rename) { Path = newPath });
            original.Rename(newPath);
        }


        private static void RenameFolder(SQLiteConnection c, FolderTree folder, string oldPath, string newPath)
        {
            folder.Rename(oldPath, newPath);
            c.Update(folder.ToDAO());
            foreach (var item in c.SelectSubFolders(folder))
            {
                RenameFolder(c, item, oldPath, newPath);
            }
            foreach (var item in c.SelectSubFiles(folder))
            {
                item.RenameFolder(oldPath, newPath);
                c.Update(item.ToDAO());
                if (item.IsMusicFile())
                {
                    Music music = MusicService.FindMusic(item.FileId);
                    Music old = music.Copy();
                    music.RenameFolder(oldPath, newPath);
                    MusicService.MusicModified(old, music);
                }
            }
        }

        public static void DeleteFolder(FolderTree target)
        {
            SQLHelper.Run(c =>
            {
                DeleteFolder(c, target);
                foreach (var music in c.SelectAllMusic())
                {
                    if (music.Path.StartsWith(target.Path))
                    {
                        MusicService.RemoveMusic(music);
                    }
                }
            });
            foreach (var listener in StorageItemEventListeners)
                listener.ExecuteFolderEvent(target, new StorageItemEventArgs(StorageItemEventType.Remove));
        }

        private static void DeleteFolder(SQLiteConnection c, FolderTree target)
        {
            FolderDAO folderDAO = target.ToDAO();
            folderDAO.State = ActiveState.Inactive;
            c.Update(folderDAO);
            c.Execute("update PreferenceItem set State = ? where ItemId = ?", ActiveState.Inactive, target.Id.ToString());
            foreach (var item in c.SelectSubFolders(target))
            {
                DeleteFolder(c, item);
            }
            foreach (var item in c.SelectSubFiles(target))
            {
                FileDAO dao = item.ToDAO();
                dao.State = ActiveState.Inactive;
                c.Update(dao);
            }
        }

        public static async Task<bool> MoveFolderAsync(FolderTree folder, FolderTree target)
        {
            bool moved = await MoveFolder(new FolderTree(folder), target);
            foreach (var listener in StorageItemEventListeners)
                listener.ExecuteFolderEvent(folder, new StorageItemEventArgs(StorageItemEventType.Move) { Folder = target });
            return moved;
        }

        private static async Task<bool> MoveFolder(FolderTree folder, FolderTree target)
        {
            StorageFolder localFolder = await folder.GetStorageFolderAsync();
            StorageFolder localTarget = await target.GetStorageFolderAsync();
            StorageFolder newFolder = await localTarget.CreateFolderAsync(folder.Name, CreationCollisionOption.OpenIfExists);
            FolderTree duplicate = FindFolderInfo(newFolder.Path);
            if (duplicate == null)
            {
                folder.MoveToFolder(target);
            }
            foreach (var item in SQLHelper.Run(c => c.SelectSubFolders(folder)))
            {
                await MoveFolder(item, duplicate ?? folder);
            }
            bool moved = true;
            foreach (var item in SQLHelper.Run(c => c.SelectSubFiles(folder)))
            {
                moved &= await MoveFileAsync(item, duplicate ?? folder);
            }
            if ((await localFolder.GetFilesAsync()).IsEmpty())
            {
                await localFolder.DeleteAsync();
            }
            if (duplicate != null && moved)
            {
                folder.State = ActiveState.Inactive;
            }
            SQLHelper.Run(c => c.Update(folder.ToDAO()));
            return moved;
        }

        public static async Task<bool> MoveFileAsync(FolderFile file, FolderTree newParent)
        {
            if (await StorageHelper.FileExists(Path.Combine(newParent.Path, file.NameWithExtension)))
            {
                string message = Helper.LocalizeMessage("DuplicateFoundWhenMovingFile", file.Name, newParent.Name);
                bool moved = true;
                await Helper.ShowMessageDialog(message, 2, 2,
                    ("MoveAndReplace", async () => { await MoveAndReplaceFile(file, newParent); }
                ),
                    ("KeepBoth", async () => { await MoveAndKeepBothFile(file, newParent); }
                ),
                    ("SkipThis", () => { moved = false; }
                ));
                return moved;
            }
            else
            {
                await MoveFile(file, newParent);
                return true;
            }
        }

        private static async Task MoveFile(FolderFile file, FolderTree folder)
        {
            StorageFile localFile = await StorageHelper.LoadFileAsync(file.Path);
            StorageFolder targetFolder = await StorageHelper.LoadFolderAsync(folder.Path);
            await localFile.MoveAsync(targetFolder);
            SQLHelper.Run(c => MoveFile(c, file, folder));
            foreach (var listener in StorageItemEventListeners)
                listener.ExecuteFileEvent(file, new StorageItemEventArgs(StorageItemEventType.Move) { Folder = folder });
        }

        private static async Task MoveAndReplaceFile(FolderFile file, FolderTree newParent)
        {
            await DeleteFile(FindFile(Path.Combine(newParent.Path, file.NameWithExtension)));
            await MoveFile(file, newParent);
        }

        private static async Task MoveAndKeepBothFile(FolderFile file, FolderTree newParent)
        {
            StorageFile localFile = await file.GetStorageFileAsync();
            StorageFolder localFolder = await StorageHelper.LoadFolderAsync(newParent.Path);
            HashSet<string> filenames = (await localFolder.GetFilesAsync()).Select(i => i.Name).ToHashSet();
            string newFilename;
            int index = 1;
            do
            {
                newFilename = Helper.GetNextName(localFile.DisplayName, index++) + localFile.FileType;
            } while (filenames.Contains(newFilename));
            await localFile.MoveAsync(localFolder, newFilename);
            foreach (var listener in StorageItemEventListeners)
                listener.ExecuteFileEvent(file, new StorageItemEventArgs(StorageItemEventType.Move) { Folder = newParent });
            SQLHelper.Run(c => MoveFile(c, file, newParent, newFilename));
        }

        private static void MoveFile(SQLiteConnection c, FolderFile file, FolderTree newParent, string newFilename = "")
        {
            FolderFile copy = file.Copy();
            copy.MoveToFolder(newParent, newFilename);
            c.Update(copy.ToDAO());
            if (copy.IsMusicFile())
            {
                MoveMusic(c, MusicService.FindMusic(copy.FileId), newParent.Path);
            }
        }

        private static void MoveMusic(SQLiteConnection c, Music music, string newPath)
        {
            Music oldMusic = music.Copy();
            music.MoveToFolder(newPath);
            MusicService.MusicModified(c, oldMusic, music);
        }
    }
}

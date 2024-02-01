﻿using SMPlayer.Dialogs;
using SMPlayer.Models;
using SMPlayer.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace SMPlayer.Helpers
{
    public static class StorageHelper
    {
        public static async Task<StorageFolder> AuthorizeFolder()
        {
            StorageFolder folder = await PickFolder();
            if (folder == null) return null;
            //if (folder.Path == Settings.settings.RootPath)
            //{
            //    Helper.ShowNotification("AuthorizeSuccessful");
            //}
            //else
            //{
            //    Helper.ShowNotification("AuthorizeFolderFailed");
            //}
            return folder;
        }
        public static async Task<StorageFolder> PickFolder(PickerLocationId pickerLocation = PickerLocationId.MusicLibrary)
        {
            FolderPicker picker = new FolderPicker
            {
                SuggestedStartLocation = pickerLocation,
            };
            picker.FileTypeFilter.Add("*");
            try
            {
                StorageFolder folder = await picker.PickSingleFolderAsync();
                if (folder == null)
                {
                    return null;
                }
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                return folder;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"pick folder failed {ex}");
                Helper.ShowOperationFailedNotification(ex);
                return null;
            }
        }
        public static bool IsParentDirectory(string child, string parent)
        {
            return GetParentPath(child) == parent;
        }
        public static async Task AddFolder(FolderTree folder)
        {
            string path = folder.Path;
            string defaultName = StorageService.FindNextFolderName(folder, Helper.LocalizeText("NewFolderName"));
            RenameDialog renameDialog = new RenameDialog(RenameOption.Create, RenameTarget.Folder, defaultName)
            {
                ValidateAsync = async (newName) => await StorageService.ValidateFolderName(path, newName),
                Confirmed = async (newName) =>
                {
                    FolderTree tree = new FolderTree() { Path = Path.Combine(path, newName) };
                    await StorageService.AddFolder(tree, folder);
                }
            };
            await renameDialog.ShowAsync();
        }

        public static string GetParentPath(string path)
        {
            int endIndex = path.EndsWith(Path.DirectorySeparatorChar) ? path.Length - 1 : path.Length;
            int index = path.Substring(0, endIndex).LastIndexOf(Path.DirectorySeparatorChar);
            return index == -1 ? "" : path.Substring(0, index);
        }

        public static string MoveToPath(string path, string newPath)
        {
            return path.Replace(GetParentPath(path), newPath);
        }

        public static async Task<StorageFolder> LoadFolderAsync(string path)
        {
            try
            {
                return await StorageFolder.GetFolderFromPathAsync(path);
            }
            catch (Exception e)
            {
                Log.Info("LoadFolderAsync Exception {0}", e);
                return null;
            }
        }

        public static async Task<StorageFile> LoadFileAsync(string path)
        {
            try
            {
                return await StorageFile.GetFileFromPathAsync(path);
            }
            catch (Exception e)
            {
                Log.Warn($"LoadFileAsync Exception {e}");
                return null;
            }
        }

        public static async Task<StorageFile> LoadFileAsync(this StorageFolder folder, string filename)
        {
            try
            {
                return await folder.GetFileAsync(filename);
            }
            catch (Exception e)
            {
                Log.Info("LoadFileAsync Exception {0}", e);
                return null;
            }
        }

        public static async Task<StorageFolder> CreateFolder(string folderName)
        {
            return await ApplicationData.Current.LocalFolder.CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists);
        }

        public static async Task<bool> FileNotExist(string path)
        {
            return !await FileExists(path);
        }

        public static async Task<bool> FileExists(string path)
        {
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(path);
                return file != null;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task<bool> FolderNotExist(string path)
        {
            return !await FolderExists(path);
        }

        public static async Task<bool> FolderExists(string path)
        {
            try
            {
                return await StorageFolder.GetFolderFromPathAsync(path) != null;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task DeleteLocalFile(string filename)
        {
            await DeleteFile(Helper.LocalFolder, filename);
        }

        public static async Task DeleteFile(string filePath)
        {
            StorageFile file = await LoadFileAsync(filePath);
            await TryDeleteFile(file);
        }

        public static async Task DeleteFile(StorageFolder folder, string filename)
        {
            if (folder == null || string.IsNullOrEmpty(filename)) return;
            var file = await folder.GetFileAsync(filename);
            await TryDeleteFile(file);
        }

        private static async Task TryDeleteFile(StorageFile file)
        {
            if (file == null)
            {
                return;
            }
            try
            {
                await file.DeleteAsync();
            }
            catch (Exception e)
            {
                Log.Warn($"delete file failed {e}");
            }
        }

        public static async Task<string> ReadFileAsync(StorageFolder folder, string filename)
        {
            if (folder == null) return null;
            if (await FileNotExist(Path.Combine(folder.Path, filename)))
            {
                return null;
            }
            StorageFile file = await folder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
            return await FileIO.ReadTextAsync(file);
        }

        public static async Task WriteFileAsync(StorageFolder folder, string filename, string content)
        {
            if (folder == null || content == null) return;
            StorageFile file = await folder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
            while (true)
            {
                try
                {
                    await FileIO.WriteTextAsync(file, content);
                    break;
                }
                catch (FileLoadException)
                {
                    System.Threading.Thread.Sleep(1000);
                }
                catch (Exception)
                {
                    // 无法删除要被替换的文件
                    break;
                }
            }
        }
    }
}

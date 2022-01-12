using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SMPlayer.Helpers
{
    public static class FileHelper
    {
        private const string PathJoiner = "\\";

        public static string GetParentPath(string path)
        {
            int index = path.LastIndexOf(PathJoiner);
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
                Log.Info("LoadFileAsync Exception {0}", e);
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

        public static async void DeleteLocalFile(string filename)
        {
            await DeleteFile(Helper.LocalFolder, filename);
        }

        public static async Task DeleteFile(string filePath)
        {
            StorageFile file = await LoadFileAsync(filePath);
            if (file != null)
            {
                await file.DeleteAsync();
            }
        }

        public static async Task DeleteFile(StorageFolder folder, string filename)
        {
            if (folder == null || string.IsNullOrEmpty(filename)) return;
            var file = await folder.GetFileAsync(filename);
            await file.DeleteAsync();
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

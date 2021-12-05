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

        public static string GetFilename(string path)
        {
            int startIndex = path.LastIndexOf(PathJoiner) + 1;
            return path.Substring(startIndex);
        }

        public static string GetDisplayName(string path)
        {
            string filename = GetFilename(path);
            int dot = path.LastIndexOf('.');
            return dot == -1 ? filename : filename.Substring(0, dot);
        }

        public static string MoveToPath(string path, string newPath)
        {
            return path.Replace(GetParentPath(path), newPath);
        }

        public static string JoinPaths(params string[] values)
        {
            return string.Join(PathJoiner, values);
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
                //Log.Info("LoadFileAsync Exception {0}", e);
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
    }
}

using Newtonsoft.Json;
using SMPlayer.Helpers;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace SMPlayer.Models
{
    public class JsonFileHelper
    {
        private const string extension = ".json";
        public static async Task<string> ReadAsync(StorageFolder folder, string filename)
        {
            if (folder == null) return null;
            if (!filename.EndsWith(extension)) filename += extension;
            StorageFile file = await folder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
            return await FileIO.ReadTextAsync(file);
        }

        public static async Task<T> ReadObjectAsync<T>(string filename) where T : class
        {
            string json = await ReadAsync(Helper.LocalFolder, filename);
            return string.IsNullOrEmpty(json) ? null : JsonConvert.DeserializeObject<T>(json);
        }

        public static async Task<string> ReadAsync(string filename)
        {
            return await ReadAsync(Helper.LocalFolder, filename);
        }

        public static async void SaveAsync<T>(StorageFolder folder, string filename, T data)
        {
            if (folder == null || data == null) return;
            if (!filename.EndsWith(extension)) filename += extension;
            StorageFile file = await folder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
            string json;
            lock (data)
            {
                try
                {
                    json = Serialize(data);
                }
                catch (Exception e)
                {
                    Log.Warn("serialize json failed {0}", e);
                    return;
                }
            }
            while (true)
            {
                try
                {
                    await FileIO.WriteTextAsync(file, json);
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

        public static void SaveAsync<T>(string filename, T data)
        {
            SaveAsync(Helper.LocalFolder, filename, data);
        }

        public static T Convert<T>(string json) where T : class
        {
            return json == null ? null : JsonConvert.DeserializeObject<T>(json);
        }

        public static string Serialize(object obj)
        {
            return obj == null ? null : JsonConvert.SerializeObject(obj);
        }

        public static void DeleteFile(string filename)
        {
            DeleteFile(Helper.LocalFolder, filename);
        }

        public static async void DeleteFile(StorageFolder folder, string filename)
        {
            var file = await folder.GetFileAsync(filename);
            await file.DeleteAsync();
        }
    }
}

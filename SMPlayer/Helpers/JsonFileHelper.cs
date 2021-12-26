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
        
        public static async Task<string> ReadAsync(string filename, ReadFilePolicy policy = ReadFilePolicy.CreateIfNotExist)
        {
            if (!filename.EndsWith(extension)) filename += extension;
            return await FileHelper.ReadFileAsync(Helper.LocalFolder, filename, policy);
        }

        public static async Task<T> ReadObjectAsync<T>(string filename, ReadFilePolicy policy = ReadFilePolicy.CreateIfNotExist) where T : class
        {
            string json = await ReadAsync(filename, policy);
            return string.IsNullOrEmpty(json) ? null : JsonConvert.DeserializeObject<T>(json);
        }

        public static async void SaveAsync<T>(StorageFolder folder, string filename, T data)
        {
            if (folder == null || data == null) return;
            if (!filename.EndsWith(extension)) filename += extension;
            string json;
            lock (data)
            {
                try
                {
                    json = ToJson(data);
                }
                catch (Exception e)
                {
                    Log.Warn("serialize json failed {0}", e);
                    return;
                }
            }
            await FileHelper.WriteFileAsync(folder, filename, json);
        }

        public static void SaveAsync<T>(string filename, T data)
        {
            SaveAsync(Helper.LocalFolder, filename, data);
        }

        public static T FromJson<T>(string json) where T : class
        {
            return json == null ? null : JsonConvert.DeserializeObject<T>(json);
        }

        public static string ToJson(object obj)
        {
            return obj == null ? null : JsonConvert.SerializeObject(obj);
        }

    }
}

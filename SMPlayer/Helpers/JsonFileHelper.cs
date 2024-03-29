﻿using Newtonsoft.Json;
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
        
        public static async Task<string> ReadAsync(string filename)
        {
            if (!filename.EndsWith(extension)) filename += extension;
            return await StorageHelper.ReadFileAsync(Helper.LocalFolder, filename);
        }

        public static async Task<T> ReadObjectAsync<T>(string filename) where T : class
        {
            string json = await ReadAsync(filename);
            return string.IsNullOrEmpty(json) ? null : JsonConvert.DeserializeObject<T>(json);
        }

        public static async void SaveAsync<T>(StorageFolder folder, string filename, T data)
        {
            if (folder == null || data == null) return;
            if (!filename.EndsWith(extension)) filename += extension;
            try
            {
                string json = ToJson(data);
                await StorageHelper.WriteFileAsync(folder, filename, json);
            }
            catch (Exception e)
            {
                Log.Warn("serialize json failed {0}", e);
            }
        }

        public static void Save<T>(string filename, T data)
        {
            SaveAsync(Helper.LocalFolder, filename, data);
        }

        public static async Task DeleteFile(string filename)
        {
            if (!filename.EndsWith(extension)) filename += extension;
            await StorageHelper.DeleteLocalFile(filename);
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

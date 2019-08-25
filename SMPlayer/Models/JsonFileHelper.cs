using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Windows.Storage;

namespace SMPlayer.Models
{
    public class JsonFileHelper
    {
        private static readonly StorageFolder Folder = ApplicationData.Current.LocalFolder;

        public static async Task<string> ReadAsync(string filename)
        {
            StorageFile file = await Folder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
            return await FileIO.ReadTextAsync(file);
        }

        public static async void SaveAsync<T>(string filename, T data)
        {
            StorageFile file = await Folder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
            var json = JsonConvert.SerializeObject(data);
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
                    continue;
                }
            }
        }

        public static T Convert<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static async void DeleteFile(string filename)
        {
            var file = await Folder.GetFileAsync(filename);
            await file.DeleteAsync();
        }
    }
}

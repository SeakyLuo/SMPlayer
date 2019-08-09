using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SMPlayer.Models
{
    [Serializable]
    public class Settings
    {
        public static Settings settings;
        private static readonly string filename = "Settings.json";

        public string RootPath { get; set; }
        public FolderTree Tree { get; set; }
        public Music LastMusic { get; set; }
        public AppLanguage Language { get; set; }
        public PlayMode Mode { get; set; }
        public double Volume { get; set; }
        public bool IsNavigationCollapsed { get; set; }

        public Settings()
        {
            RootPath = KnownFolders.MusicLibrary.Path;
            Tree = new FolderTree();
            Language = AppLanguage.FollowSystem;
            Mode = PlayMode.Once;
            Volume = 50.0d;
            IsNavigationCollapsed = true;
        }

        public static async void Init()
        {
            var json = await JsonFileHelper.ReadAsync(filename);
            if (string.IsNullOrEmpty(json))
            {
                settings = new Settings();
                Save();
            }
            else
            {
                settings = JsonFileHelper.Convert<Settings>(json);
            }
        }

        public static void Save()
        {
            JsonFileHelper.SaveAsync(filename, settings);
        }

        public static void SetTreeFolder(StorageFolder folder, Action afterTreeSet = null)
        {
            settings.Tree = new FolderTree(folder, AfterInitiation);
            afterTreeSet?.Invoke();
        }

        public static void AfterInitiation()
        {
            Save();
            MusicManager.AllSongs = settings.Tree.Flatten();
            MusicManager.Save();
        }
    }
}

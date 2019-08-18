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
        private static readonly string FILENAME = "Settings.json";

        public string RootPath { get; set; }
        public FolderTree Tree { get; set; }
        public Music LastMusic { get; set; }
        public AppLanguage Language { get; set; }
        public PlayMode Mode { get; set; }
        public double Volume { get; set; }
        public bool IsNavigationCollapsed { get; set; }

        public Settings()
        {
            RootPath = "";
            Tree = new FolderTree();
            Language = AppLanguage.FollowSystem;
            Mode = PlayMode.Once;
            Volume = 50.0d;
            IsNavigationCollapsed = true;
        }

        public static async Task Init()
        {
            var json = await JsonFileHelper.ReadAsync(FILENAME);
            if (string.IsNullOrEmpty(json))
            {
                settings = new Settings();
                Save();
            }
            else
            {
                settings = JsonFileHelper.Convert<Settings>(json);
                var folder = await StorageFolder.GetFolderFromPathAsync(settings.RootPath);
                AfterTreeSet(folder);
            }
        }

        public static void Save()
        {
            JsonFileHelper.SaveAsync(FILENAME, settings);
        }

        public static async Task SetTreeFolder(StorageFolder folder)
        {
            await settings.Tree.Init(folder);
            Save();
            AfterTreeSet(folder);
        }

        private static void AfterTreeSet(StorageFolder folder)
        {
            MusicLibraryPage.AllSongs = settings.Tree.Flatten();
            MusicLibraryPage.Save();
            Helper.CurrentFolder = folder;
        }
    }
}

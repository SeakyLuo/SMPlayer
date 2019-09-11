using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;

namespace SMPlayer.Models
{
    [Serializable]
    public class Settings
    {
        public static Settings settings;
        private const string FILENAME = "SMPlayerSettings.json";

        public string RootPath { get; set; }
        public FolderTree Tree { get; set; }
        public Music LastMusic { get; set; }
        public AppLanguage Language { get; set; }
        public PlayMode Mode { get; set; }
        public double Volume { get; set; }
        public bool IsNavigationCollapsed { get; set; }
        public Color ThemeColor { get; set; }
        public ShowNotification Notification { get; set; }
        public string LastPage { get; set; }
        public List<Playlist> Playlists { get; set; }

        public string LastPlaylist { get; set; }

        public Settings()
        {
            RootPath = "";
            Tree = new FolderTree();
            Language = AppLanguage.FollowSystem;
            Mode = PlayMode.Once;
            Volume = 50.0d;
            IsNavigationCollapsed = true;
            ThemeColor = (Color)Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(Color), "#0078D7");
            LastPage = "";
            Playlists = new List<Playlist>();
        }
        public int FindNextPlaylistNameIndex(string Name)
        {
            var siblings = Playlists.FindAll((p) => p.Name.StartsWith(Name)).Select((p) => p.Name).ToHashSet();
            for (int i = 1; i <= siblings.Count; i++)
                if (!siblings.Contains(string.Format("{0} {1}", Name, i)))
                    return i;
            return 0;
        }
        public string FindNextPlaylistName(string Name)
        {
            int index = FindNextPlaylistNameIndex(Name);
            return index == 0 ? Name : string.Format("{0} {1}", Name, index);
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
                if (string.IsNullOrEmpty(settings.RootPath)) return;
                Helper.CurrentFolder = await StorageFolder.GetFolderFromPathAsync(settings.RootPath);
            }

            Helper.ThumbnailFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Thumbnails", CreationCollisionOption.OpenIfExists);
            foreach (var item in await Helper.ThumbnailFolder.GetFilesAsync())
                await item.DeleteAsync();
        }

        public static void Save()
        {
            JsonFileHelper.SaveAsync(FILENAME, settings);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public bool LocalMusicGridView { get; set; }
        public bool LocalFolderGridView { get; set; }
        public ObservableCollection<string> FavSongs { get; set; }
        public ObservableCollection<string> Recent { get; set; }

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
            LastPlaylist = "";
            LocalMusicGridView = true;
            LocalFolderGridView = true;
            FavSongs = new ObservableCollection<string>();
            Recent = new ObservableCollection<string>();
        }
        public int FindNextPlaylistNameIndex(string Name)
        {
            if (!string.IsNullOrEmpty(Name))
            {
                var siblings = Playlists.FindAll((p) => p.Name.StartsWith(Name)).Select((p) => p.Name).ToHashSet();
                for (int i = 1; i <= siblings.Count; i++)
                    if (!siblings.Contains($"{Name} {i}"))
                        return i;
            }
            return 0;
        }
        public string FindNextPlaylistName(string Name)
        {
            int index = FindNextPlaylistNameIndex(Name);
            return index == 0 ? Name : $"{Name} {index}";
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
                try
                {
                    Helper.CurrentFolder = await StorageFolder.GetFolderFromPathAsync(settings.RootPath);
                }
                catch (Exception)
                {

                }
            }

        }

        public static void Save()
        {
            JsonFileHelper.SaveAsync(FILENAME, settings);
        }

        public void LikeMusic(Music music)
        {
            if (FavSongs.Contains(music.Path)) return;
            music.Favorite = true;
            FavSongs.Add(music.Path);
        }

        public void LikeMusic(ICollection<Music> playlist)
        {
            var hashset = FavSongs.ToHashSet();
            foreach (var music in playlist)
            {
                if (!hashset.Contains(music.Path))
                {
                    music.Favorite = true;
                    FavSongs.Add(music.Path);
                }
            }
        }

        public void DislikeMusic(Music music)
        {
            FavSongs.Remove(music.Path);
            music.Favorite = false;
        }

        public void DeleteMusic(Music music)
        {
            Tree.RemoveMusic(music);
            foreach (var playlist in Playlists)
                playlist.Songs.Remove(music);
            FavSongs.Remove(music.Path);
        }

        public void Played(Music music)
        {
            if (music == null) return;
            Recent.Remove(music.Path);
            Recent.Insert(0, music.Path);
        }
    }
}

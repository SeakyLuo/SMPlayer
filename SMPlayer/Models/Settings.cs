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
        public static List<LikeMusicListener> LikeMusicListeners = new List<LikeMusicListener>();

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
        public Playlist MyFavorites { get; set; }
        public ObservableCollection<string> Recent { get; set; }
        public bool MiniModeWithDropdown { get; set; }
        public bool IsMuted { get; set; }

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
            MyFavorites = new Playlist(MenuFlyoutHelper.MyFavorites);
            Recent = new ObservableCollection<string>();
            MiniModeWithDropdown = false;
            IsMuted = false;
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
                foreach (var item in await ApplicationData.Current.LocalFolder.GetItemsAsync())
                    if (item.Name.EndsWith(".TMP"))
                        await item.DeleteAsync();
            }

        }

        public static void Save()
        {
            JsonFileHelper.SaveAsync(FILENAME, settings);
        }

        public void LikeMusic(Music music)
        {
            if (MyFavorites.Contains(music)) return;
            music.Favorite = true;
            MyFavorites.Add(music);
            foreach (var listener in LikeMusicListeners) listener.MusicLiked(music, true);
        }

        public void LikeMusic(ICollection<Music> playlist)
        {
            var hashset = MyFavorites.Songs.ToHashSet();
            foreach (var music in playlist)
            {
                if (!hashset.Contains(music))
                {
                    music.Favorite = true;
                    MyFavorites.Add(music);
                    foreach (var listener in LikeMusicListeners) listener.MusicLiked(music, true);
                }
            }
        }

        public void DislikeMusic(Music music)
        {
            MyFavorites.Remove(music);
            music.Favorite = false;
            foreach (var listener in LikeMusicListeners) listener.MusicLiked(music, false);
        }

        public void DeleteMusic(Music music)
        {
            Tree.RemoveMusic(music);
            foreach (var playlist in Playlists)
                playlist.Songs.Remove(music);
            MyFavorites.Remove(music);
        }

        public void Played(Music music)
        {
            if (music == null) return;
            Recent.Remove(music.Path);
            Recent.Insert(0, music.Path);
        }

        public NamingError CheckPlaylistNamingError(string newName)
        {
            if (string.IsNullOrEmpty(newName) || string.IsNullOrWhiteSpace(newName))
                return NamingError.EmptyOrWhiteSpace;
            if (newName == MenuFlyoutHelper.NowPlaying || newName == MenuFlyoutHelper.MyFavorites ||
                Playlists.FindIndex((p) => p.Name == newName) != -1)
                return NamingError.Used;
            if (newName.Contains("+++"))
                return NamingError.Special;
            return NamingError.Good;
        }

        public void RenamePlaylist(string oldName, string newName, RenameOption option, object data = null)
        {
            switch (option)
            {
                case RenameOption.New:
                    Playlist playlist = new Playlist(newName);
                    if (data != null) playlist.Add(data);
                    Playlists.Add(playlist);
                    PlaylistsPage.Playlists.Add(playlist);
                    break;
                case RenameOption.Rename:
                    if (oldName == newName) break;
                    int index = Playlists.FindIndex((p) => p.Name == oldName);
                    Playlists[index].Name = newName;
                    PlaylistsPage.Playlists[index].Name = newName;
                    break;
            }
        }

    }

    public interface LikeMusicListener
    {
        void MusicLiked(Music music, bool isFavorite);
    }

    public enum NamingError
    {
        Good = 0,
        EmptyOrWhiteSpace = 1,
        Used = 2,
        Special = 3
    }

    public enum RenameOption
    {
        New = 0,
        Rename = 1
    }
}

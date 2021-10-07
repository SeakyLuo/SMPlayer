using SMPlayer.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;

namespace SMPlayer.Models
{
    [Serializable]
    public class Settings
    {
        public static Settings settings;
        public const string JsonFilename = "SMPlayerSettings";
        public static List<ILikeMusicListener> LikeMusicListeners = new List<ILikeMusicListener>();
        public static List<Action<Playlist>> PlaylistAddedListeners = new List<Action<Playlist>>();
        public static bool Inited { get; private set; } = false;

        public string RootPath { get; set; } = "";
        public FolderTree Tree { get; set; } = new FolderTree();
        public int LastMusicIndex { get; set; } = -1;
        public PlayMode Mode { get; set; } = PlayMode.Once;
        public double Volume { get; set; } = 50.0d;
        public bool IsNavigationCollapsed { get; set; } = true;
        public Color ThemeColor { get; set; } = (Color)Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(Color), "#0078D7");
        public NotificationSendMode NotificationSend { get; set; } = NotificationSendMode.MusicChanged;
        public NotificationDisplayMode NotificationDisplay { get; set; } = NotificationDisplayMode.Normal;
        public string LastPage { get; set; } = "";
        public List<Playlist> Playlists { get; set; } = new List<Playlist>();
        public string LastPlaylist { get; set; } = "";
        public bool LocalMusicGridView { get; set; } = true;
        public bool LocalFolderGridView { get; set; } = true;
        public Playlist MyFavorites { get; set; }
        public ObservableCollection<string> RecentPlayed { get; set; } = new ObservableCollection<string>();
        public bool MiniModeWithDropdown { get; set; } = false;
        public bool IsMuted { get; set; } = false;
        public int LimitedRecentPlayedItems { get; set; } = -1;
        public bool AutoPlay { get; set; } = false;
        public bool AutoLyrics { get; set; } = false;
        public bool SaveMusicProgress { get; set; } = false;
        public double MusicProgress { get; set; } = 0;
        public SortBy MusicLibraryCriterion { get; set; } = SortBy.Title;
        public SortBy AlbumsCriterion { get; set; } = SortBy.Default;
        public bool HideMultiSelectCommandBarAfterOperation { get; set; } = true;
        public bool ShowCount { get; set; } = true;
        public bool ShowLyricsInNotification { get; set; } = false;
        public ObservableCollection<string> RecentSearches = new ObservableCollection<string>();
        public VoiceAssistantLanguage VoiceAssistantPreferredLanguage = VoiceAssistantHelper.ConvertLanguage(Helper.CurrentLanguage);

        public SortBy SearchArtistsCriterion { get; set; } = SortBy.Default;
        public SortBy SearchAlbumsCriterion { get; set; } = SortBy.Default;
        public SortBy SearchSongsCriterion { get; set; } = SortBy.Default;
        public SortBy SearchPlaylistsCriterion { get; set; } = SortBy.Default;
        public SortBy SearchFoldersCriterion { get; set; } = SortBy.Default;

        public PreferenceSetting Preference { get; set; } = new PreferenceSetting();

        [Newtonsoft.Json.JsonIgnore]
        private List<Music> JustRemoved = new List<Music>();

        public Settings()
        {
            MyFavorites = new Playlist(MenuFlyoutHelper.MyFavorites);
        }

        public int FindNextPlaylistNameIndex(string Name)
        {
            if (!string.IsNullOrEmpty(Name))
            {
                var siblings = Playlists.FindAll(p => p.Name.StartsWith(Name)).Select(p => p.Name).ToHashSet();
                for (int i = 1; i <= siblings.Count; i++)
                    if (!siblings.Contains(Helper.GetPlaylistName(Name, i)))
                        return i;
            }
            return 0;
        }

        public string FindNextPlaylistName(string Name)
        {
            int index = FindNextPlaylistNameIndex(Name);
            return index == 0 ? Name : Helper.GetPlaylistName(Name, index);
        }

        public static async Task Init()
        {
            Inited = false;
            var json = await JsonFileHelper.ReadAsync(JsonFilename);
            if (string.IsNullOrEmpty(json))
            {
                settings = new Settings();
                Save();
            }
            else
            {
                settings = JsonFileHelper.Convert<Settings>(json);
                await Init(settings);
            }
            Inited = true;
        }

        public static async Task Init(Settings settings)
        {
            if (string.IsNullOrEmpty(settings.RootPath)) return;
            try
            {
                Helper.CurrentFolder = await StorageFolder.GetFolderFromPathAsync(settings.RootPath);
            }
            catch (FileNotFoundException)
            {
                App.LoadedListeners.Add(() =>
                {
                    MainPage.Instance.ShowLocalizedNotification("RootNotFound");
                    MainPage.Instance.NavigateToPage(typeof(SettingsPage));
                });
            }
            catch (Exception)
            {

            }
            MediaControl.AddMusicModifiedListener((before, after) =>
            {
                settings.FindAllMusicAndOperate(before, music => music.CopyFrom(after));
            });
            foreach (var item in await ApplicationData.Current.LocalFolder.GetItemsAsync())
                if (item.Name.EndsWith(".TMP") || item.Name.EndsWith(".~tmp"))
                    await item.DeleteAsync();
        }

        public static void Save()
        {
            if (settings == null) return;
            settings.MusicProgress = MediaHelper.Position;
            JsonFileHelper.SaveAsync(JsonFilename, settings);
            JsonFileHelper.SaveAsync(Helper.TempFolder, JsonFilename + Helper.TimeStamp, settings);
        }

        private void FindAllMusicAndOperate(Music target, Action<Music> action) {
            Music music;
            if ((music = Tree.FindMusic(target)) is Music)
                action.Invoke(music);
            foreach (var playlist in settings.Playlists)
                if ((music = playlist.Songs.FirstOrDefault(m => m == target)) is Music)
                    action.Invoke(music);
            if ((music = MyFavorites.Songs.FirstOrDefault(m => m == target)) is Music)
                action.Invoke(music);
        }

        public static Music FindMusic(IMusicable target) { return settings.Tree.FindMusic(target.ToMusic()); }
        public static Music FindMusic(Music target) { return settings.Tree.FindMusic(target); }
        public static Music FindMusic(string target) { return settings.Tree.FindMusic(target); }

        public void LikeMusic(Music music)
        {
            if (MyFavorites.Contains(music)) return;
            music.Favorite = true;
            MyFavorites.Add(music);
            FindAllMusicAndOperate(music, m => m.CopyFrom(music));
            MediaHelper.LikeMusic(music);
            foreach (var listener in LikeMusicListeners) listener.MusicLiked(music, true);
        }

        public void LikeMusic(IEnumerable<IMusicable> playlist)
        {
            var hashset = MyFavorites.Songs.ToHashSet();
            foreach (var item in playlist)
            {
                var music = item.ToMusic();
                if (!hashset.Contains(music))
                {
                    music.Favorite = true;
                    MyFavorites.Add(music);
                    FindAllMusicAndOperate(music, m => m.CopyFrom(music));
                    MediaHelper.LikeMusic(music);
                    foreach (var listener in LikeMusicListeners) listener.MusicLiked(music, true);
                }
            }
        }

        public void DislikeMusic(Music music)
        {
            music.Favorite = false;
            MyFavorites.Remove(music);
            FindAllMusicAndOperate(music, m => m.CopyFrom(music));
            MediaHelper.DislikeMusic(music);
            foreach (var listener in LikeMusicListeners) listener.MusicLiked(music, false);
        }

        public async void AddMusic(Music music)
        {
            if (JustRemoved.Any(m => m.Name == music.Name && m.Artist == music.Artist && m.Album == music.Album && m.Duration == music.Duration))
                return;
            RecentPage.RecentAdded.Add(music);
            if (AutoLyrics)
            {
                await Task.Run(async() =>
                {
                    string lyrics = await music.GetLyricsAsync();
                    if (string.IsNullOrEmpty(lyrics))
                    {
                        await music.SaveLyricsAsync(await LyricsHelper.SearchLyrics(music));
                    }
                });
            }
        }

        private Dictionary<string, int> RemovedPlaylist = new Dictionary<string, int>();
        private int myFavoratesRemovedIndex = -1, recentPlayedRemovedIndex = -1;

        public void RemoveMusic(Music music)
        {
            JustRemoved.Add(music);
            Tree.RemoveMusic(music);
            RemovedPlaylist.Clear();
            int removeIndex;
            foreach (var playlist in Playlists)
            {
                if ((removeIndex = playlist.Songs.IndexOf(music)) > -1)
                {
                    RemovedPlaylist.Add(playlist.Name, removeIndex);
                    playlist.Remove(removeIndex);
                }
            }
            if ((myFavoratesRemovedIndex = MyFavorites.Songs.IndexOf(music)) > -1)
                MyFavorites.Remove(music);
            RecentPlayed.Remove(music.Path);
            RecentPage.RecentAdded.Remove(music);
        }

        public void UndoRemoveMusic(Music music)
        {
            JustRemoved.Remove(music);
            if (Tree.FindTree(music) is FolderTree tree)
            {
                tree.Files.Add(music);
                tree.Sort();
            }
            foreach (var pair in RemovedPlaylist)
                Playlists.FirstOrDefault(p => p.Name == pair.Key)?.Songs.Insert(pair.Value, music);
            if (myFavoratesRemovedIndex > -1)
                MyFavorites.Songs.Insert(myFavoratesRemovedIndex, music);
            if (recentPlayedRemovedIndex > -1)
                RecentPlayed.Insert(recentPlayedRemovedIndex, music.Path);
            RecentPage.RecentAdded.Add(music);
        }

        public void Played(Music music)
        {
            if (music == null) return;
            RecentPlayed.AddOrMoveToTheFirst(music.Path);
            if (LimitedRecentPlayedItems > -1 && RecentPlayed.Count > LimitedRecentPlayedItems)
                RecentPlayed.RemoveAt(LimitedRecentPlayedItems);
        }

        public void Search(string keyword)
        {
            RecentSearches.AddOrMoveToTheFirst(keyword);
        }

        public const int PlaylistNameMaxLength = 50;
        public NamingError CheckPlaylistNamingError(string newName)
        {
            if (string.IsNullOrEmpty(newName) || string.IsNullOrWhiteSpace(newName))
                return NamingError.EmptyOrWhiteSpace;
            if (newName == MenuFlyoutHelper.NowPlaying || newName == MenuFlyoutHelper.MyFavorites || Playlists.Any(p => p.Name == newName))
                return NamingError.Used;
            if (newName.Contains(TileHelper.StringConcatenationFlag) || newName.Contains("{0}"))
                return NamingError.Special;
            if (newName.Length > PlaylistNameMaxLength)
                return NamingError.TooLong;
            return NamingError.Good;
        }

        public async void RenamePlaylist(string oldName, string newName, RenameOption option, object data = null)
        {
            switch (option)
            {
                case RenameOption.New:
                    Playlist playlist = new Playlist(newName);
                    if (data != null) playlist.Add(data);
                    await playlist.SetDisplayItemAsync();
                    Playlists.Add(playlist);
                    PlaylistsPage.Playlists.Add(playlist);
                    foreach (var listener in PlaylistAddedListeners)
                        listener.Invoke(playlist);
                    break;
                case RenameOption.Rename:
                    if (oldName == newName) break;
                    int index = Playlists.FindIndex(p => p.Name == oldName);
                    Playlists[index].Name = newName;
                    PlaylistsPage.Playlists[index].Name = newName;
                    break;
            }
        }


        public static List<Music> PathToCollection(IEnumerable<string> paths, bool isFavorite = false)
        {
            List<Music> collection = new List<Music>();
            foreach (var path in paths)
            {
                if (FindMusic(path) is Music music)
                {
                    if (isFavorite) music.Favorite = true;
                    collection.Add(music);
                }
            }
            return collection;
        }

        public static async Task<StorageFolder> GetRootFolder()
        {
            return await StorageFolder.GetFolderFromPathAsync(settings.RootPath);
        }
    }

    public interface ILikeMusicListener
    {
        void MusicLiked(Music music, bool isFavorite);
    }

    public enum NamingError
    {
        Good = 0,
        EmptyOrWhiteSpace = 1,
        Used = 2,
        Special = 3,
        TooLong = 4
    }

    public static class NamingErrorConverter
    {
        public static string ToStr(this NamingError error)
        {
            switch (error)
            {
                case NamingError.EmptyOrWhiteSpace:
                    return "NamingErrorEmptyOrWhiteSpace";
                case NamingError.Used:
                    return "NamingErrorUsed";
                case NamingError.Special:
                    return "NamingErrorSpecial";
                case NamingError.TooLong:
                    return "NamingErrorTooLong";
                default:
                    return "";
            }
        }
    }

    public enum RenameOption
    {
        New = 0,
        Rename = 1
    }
}

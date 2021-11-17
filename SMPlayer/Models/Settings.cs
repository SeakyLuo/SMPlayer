using SMPlayer.Helpers;
using SMPlayer.Models.DAO;
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
        public const string JsonFilename = "SMPlayerSettings", NewFilename = "SMPlayerSettingsFile";
        public static List<ILikeMusicListener> LikeMusicListeners = new List<ILikeMusicListener>();
        public static List<Action<Playlist>> PlaylistAddedListeners = new List<Action<Playlist>>();
        public static bool Inited { get; private set; } = false;

        public Dictionary<long, Music> MusicLibrary { get; set; } = new Dictionary<long, Music>();
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
        public long LastPlaylistId { get; set; } = 0;
        public bool LocalMusicGridView { get; set; } = true;
        public bool LocalFolderGridView { get; set; } = true;
        public Playlist MyFavorites { get; set; }
        public ObservableCollection<string> RecentPlayed { get; set; } = new ObservableCollection<string>();
        public List<long> RecentPlayedSongs { get; set; } = new List<long>();
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

        public PreferenceSettings Preference { get; set; } = new PreferenceSettings();

        [Newtonsoft.Json.JsonIgnore]
        private List<Music> JustRemoved = new List<Music>();

        public IdGenerator IdGenerator = new IdGenerator();

        public Settings()
        {
            MyFavorites = new Playlist(MenuFlyoutHelper.MyFavorites);
        }

        public static async Task Init()
        {
            Inited = false;
            var newSettings = await JsonFileHelper.ReadAsync(NewFilename);
            if (string.IsNullOrEmpty(newSettings))
            {
                if (await JsonFileHelper.ReadAsync(JsonFilename) is string json)
                {
                    settings = JsonFileHelper.Convert<Settings>(json);
                    if (settings.MusicLibrary.IsEmpty() && !settings.Tree.IsEmpty)
                    {
                        settings.MusicLibrary = settings.Tree.Flatten()
                                                             .Select(m => { m.Id = settings.IdGenerator.GenerateMusicId(); return m; })
                                                             .ToDictionary(m => m.Id);
                    }
                    await Init(settings);
                }
                else
                {
                    settings = new Settings();
                    Save();
                }
            }
            else
            {
                settings = JsonFileHelper.Convert<SettingsDAO>(newSettings).FromDAO();
                await Init(settings);
            }
            Inited = true;
        }

        private static async Task Init(Settings settings)
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
                settings.SelectById(before.Id).CopyFrom(after);
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
            try
            {
                SettingsDAO settingsDAO = settings.ToDAO();
                JsonFileHelper.SaveAsync(NewFilename, settingsDAO);
                JsonFileHelper.SaveAsync(Helper.TempFolder, NewFilename + Helper.TimeStamp, settingsDAO);
            }
            catch (Exception e)
            {
                Helper.Print(e.ToString());
            }
        }

        public static Music FindMusic(IMusicable target) { return FindMusic(target.ToMusic()); }
        public static Music FindMusic(Music target) { return settings.Tree.FindMusic(target) is Music music ? settings.SelectById(music.Id) : null; }
        public static Music FindMusic(string target) { return settings.Tree.FindMusic(target) is Music music ? settings.SelectById(music.Id) : null; }

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

        public int FindNextFolderNameIndex(string path, string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                var siblings = Tree.FindTree(path)?.Trees.Select(p => p.Directory).ToHashSet();
                for (int i = 1; i <= siblings.Count; i++)
                    if (!siblings.Contains(Helper.GetPlaylistName(name, i)))
                        return i;
            }
            return 0;
        }

        public string FindNextFolderName(string path, string Name)
        {
            int index = FindNextFolderNameIndex(path, Name);
            return index == 0 ? Name : Helper.GetPlaylistName(Name, index);
        }

        public void LikeMusic(Music music)
        {
            if (MyFavorites.Contains(music)) return;
            music.Favorite = true;
            MyFavorites.Add(music);
            MediaHelper.LikeMusic(music);
            foreach (var listener in LikeMusicListeners) listener.MusicLiked(music, true);
        }

        public void LikeMusic(IEnumerable<IMusicable> playlist)
        {
            foreach (var item in playlist)
            {
                LikeMusic(item.ToMusic());
            }
        }

        public void DislikeMusic(Music music)
        {
            music.Favorite = false;
            MyFavorites.Remove(music);
            MediaHelper.DislikeMusic(music);
            foreach (var listener in LikeMusicListeners) listener.MusicLiked(music, false);
        }

        public async void AddMusic(Music music)
        {
            if (JustRemoved.Any(m => m.Name == music.Name && m.Artist == music.Artist && m.Album == music.Album && m.Duration == music.Duration))
                return;
            if (Tree.FindMusic(music) != null)
                return;
            music.Id = settings.IdGenerator.GenerateMusicId();
            MusicLibrary.Add(music.Id, music);
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

        // playlist.Id - musicRemoveIndex
        private Dictionary<long, int> removedMusicInPlaylist = new Dictionary<long, int>();
        private int myFavoratesRemovedIndex = -1, recentPlayedRemovedIndex = -1, preferredMusicRemovedIndex = -1;
        public void RemoveMusic(Music music)
        {
            if (!Tree.RemoveMusic(music))
            {
                return;
            }
            MusicLibrary.Remove(music.Id);
            JustRemoved.Add(music);
            removedMusicInPlaylist.Clear();
            int removeIndex;
            foreach (var playlist in Playlists)
            {
                if ((removeIndex = playlist.Songs.IndexOf(music)) > -1)
                {
                    removedMusicInPlaylist.Add(playlist.Id, removeIndex);
                    playlist.Remove(removeIndex);
                }
            }
            if ((myFavoratesRemovedIndex = MyFavorites.Songs.IndexOf(music)) > -1)
                MyFavorites.Remove(music);
            if ((recentPlayedRemovedIndex = RecentPlayedSongs.IndexOf(music.Id)) > -1)
                RecentPlayedSongs.RemoveAt(recentPlayedRemovedIndex);
            if ((preferredMusicRemovedIndex = Preference.PreferredSongs.FindIndex(m => m.Id == music.Id.ToString())) > -1)
                Preference.PreferredSongs.RemoveAt(preferredMusicRemovedIndex);
            RecentPage.RecentAdded.Remove(music); // TODO: ugly impl
        }

        public void UndoRemoveMusic(Music music)
        {
            JustRemoved.Remove(music);
            if (Tree.FindTree(music) is FolderTree tree)
            {
                tree.Files.Add(music);
                tree.Sort();
            }
            foreach (var pair in removedMusicInPlaylist)
                Playlists.FirstOrDefault(p => p.Id == pair.Key)?.Songs.Insert(pair.Value, music);
            if (myFavoratesRemovedIndex > -1)
                MyFavorites.Songs.Insert(myFavoratesRemovedIndex, music);
            if (recentPlayedRemovedIndex > -1)
                RecentPlayed.Insert(recentPlayedRemovedIndex, music.Path);
            RecentPage.RecentAdded.Add(music); // TODO: ugly impl
        }

        public Music SelectById(long id)
        {
            return MusicLibrary[id];
        }

        public List<Music> SelectByIds(IEnumerable<long> ids)
        {
            return ids.Select(i => SelectById(i)).ToList();
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

        public NamingError CheckPlaylistNamingError(string newName)
        {
            if (string.IsNullOrEmpty(newName) || string.IsNullOrWhiteSpace(newName))
                return NamingError.EmptyOrWhiteSpace;
            if (newName == MenuFlyoutHelper.NowPlaying || newName == MenuFlyoutHelper.MyFavorites || Playlists.Any(p => p.Name == newName))
                return NamingError.Used;
            if (newName.Contains(TileHelper.StringConcatenationFlag) || newName.Contains("{0}"))
                return NamingError.Special;
            if (newName.Length > 50)
                return NamingError.TooLong;
            return NamingError.Good;
        }

        public async void RenamePlaylist(string oldName, string newName, RenameOption option, object data = null)
        {
            switch (option)
            {
                case RenameOption.Create:
                    Playlist playlist = new Playlist(newName)
                    {
                        Id = settings.IdGenerator.GeneratePlaylistId()
                    };
                    if (data != null) playlist.Add(data);
                    await playlist.LoadDisplayItemAsync();
                    Playlists.Add(playlist);
                    PlaylistsPage.Playlists.Add(playlist); // TODO: ugly impl
                    foreach (var listener in PlaylistAddedListeners)
                        listener.Invoke(playlist);
                    break;
                case RenameOption.Rename:
                    if (oldName == newName) break;
                    int index = Playlists.FindIndex(p => p.Name == oldName);
                    Playlists[index].Name = newName;
                    PlaylistsPage.Playlists[index].Name = newName; // TODO: ugly impl
                    break;
            }
        }

        public void InsertPlaylist(Playlist playlist, int index)
        {
            playlist.Id = settings.IdGenerator.GeneratePlaylistId();
            settings.Playlists.Insert(index, playlist);
        }

        public void RenameFolder(FolderTree original, string newPath)
        {
            Tree.FindTree(original)?.Rename(newPath);
        }

        public void DeleteFolder(FolderTree target)
        {
            FolderTree folderTree = Tree.FindTree(target.ParentPath);
            if (folderTree == null) return;
            folderTree.Trees.Remove(target);
            MusicLibrary.RemoveAll(pair => pair.Value.Path.StartsWith(target.Path));
            RecentPlayedSongs.RemoveAll(id => SelectById(id).Path.StartsWith(target.Path));
            foreach (var playlist in Playlists) playlist.RemoveAll(i => i.Path.StartsWith(target.Path));
            MyFavorites.RemoveAll(i => i.Path.StartsWith(target.Path));
            Preference.PreferredSongs.RemoveAll(i => SelectById(int.Parse(i.Id)).Path.StartsWith(target.Path));
            Preference.PreferredFolders.RemoveAll(i => i.Id == folderTree.Id.ToString());
            RecentPage.RecentAdded.DeleteFolder(target.Path);
        }

        public void MoveMusic(Music music, string path)
        {
            Tree.FindMusic(music).MoveToFolder(path);
            MusicLibrary[music.Id].MoveToFolder(path);
        }

        public void MoveFolder(FolderTree tree, string path)
        {
            FolderTree branch = Tree.FindTree(tree);
            Tree.FindTree(tree).MoveToFolder(path);
            foreach (var music in MusicLibrary.Values)
            {
                if (music.Path.StartsWith(path))
                {
                    music.MoveToFolder(path);
                }
            }
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
        Create,
        Rename,
    }

    public enum RenameTarget
    {
        Playlist,
        Folder,
    }
}

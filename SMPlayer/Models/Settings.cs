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
        private static readonly List<IMusicEventListener> MusicEventListeners = new List<IMusicEventListener>();
        public static void AddMusicEventListener(IMusicEventListener listener) { MusicEventListeners.Add(listener); }
        private static readonly List<IPlaylistEventListener> PlaylistEventListeners = new List<IPlaylistEventListener>();
        public static void AddPlaylistEventListener(IPlaylistEventListener listener) { PlaylistEventListeners.Add(listener); }
        private static readonly List<IFolderTreeEventListener> FolderTreeEventListeners = new List<IFolderTreeEventListener>();
        public static void AddFolderTreeEventListener(IFolderTreeEventListener listener) { FolderTreeEventListeners.Add(listener); }

        public Dictionary<long, Music> MusicLibrary { get; set; } = new Dictionary<long, Music>();
        [Newtonsoft.Json.JsonIgnore]
        public IEnumerable<Music> AllSongs { get => MusicLibrary.Values; }
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
        public List<long> RecentAdded = new List<long>();

        public Settings()
        {
            MyFavorites = new Playlist(MenuFlyoutHelper.MyFavorites);
        }

        public static Music FindMusic(IMusicable target) { return FindMusic(target.ToMusic()); }
        public static Music FindMusic(Music target) { return FindMusic(target?.Path); }
        public static Music FindMusic(string target) { return settings.Tree.FindMusic(target) is Music music ? settings.SelectMusicById(music.Id) : null; }
        public static Music FindMusic(FolderFile target) { return settings.SelectMusicById(target.Id); }
        public static Music FindMusic(long id) { return settings.SelectMusicById(id); }
        public static List<Music> FindMusicList(List<long> ids) { return settings.SelectMusicByIds(ids); }

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
                var siblings = Tree.FindTree(path)?.Trees.Select(p => p.Name).ToHashSet();
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
            foreach (var listener in MusicEventListeners) listener.Liked(music, true);
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
            foreach (var listener in MusicEventListeners) listener.Liked(music, false);
        }

        public async void AddMusic(Music music)
        {
            if (JustRemoved.Any(m => m.Name == music.Name && m.Artist == music.Artist && m.Album == music.Album && m.Duration == music.Duration))
                return;
            if (Tree.FindMusic(music) is Music oldItem)
            {
                music.Id = oldItem.Id;
                oldItem.CopyMusicProperties(music, false);
                return;
            }
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
            foreach (var listener in MusicEventListeners) listener.Added(music);
        }

        public void RemoveFile(FolderFile file)
        {
            if (file.IsMusicFile())
            {
                RemoveMusic(SelectMusicById(file.Id));
            }
        }

        // playlist.Id - musicRemoveIndex
        private Dictionary<long, int> removedMusicInPlaylist = new Dictionary<long, int>();
        private int myFavoratesRemovedIndex = -1, recentPlayedRemovedIndex = -1, preferredMusicRemovedIndex = -1;
        public void RemoveMusic(Music music)
        {
            if (!Tree.RemoveFile(music.Path))
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
            foreach (var listener in MusicEventListeners) listener.Removed(music);
        }

        public void UndoRemoveMusic(Music music)
        {
            JustRemoved.Remove(music);
            if (Tree.FindTree(music) is FolderTree tree)
            {
                tree.Files.Add(new FolderFile(music));
                tree.Sort();
            }
            foreach (var pair in removedMusicInPlaylist)
                SelectPlaylistById(pair.Key)?.Songs.Insert(pair.Value, music);
            if (myFavoratesRemovedIndex > -1)
                MyFavorites.Songs.Insert(myFavoratesRemovedIndex, music);
            if (recentPlayedRemovedIndex > -1)
                RecentPlayed.Insert(recentPlayedRemovedIndex, music.Path);
            RecentPage.RecentAdded.Add(music); // TODO: ugly impl
            foreach (var listener in MusicEventListeners) listener.Added(music);
        }

        public Music SelectMusicById(long id)
        {
            MusicLibrary.TryGetValue(id, out Music music);
            return music;
        }

        public List<Music> SelectMusicByIds(IEnumerable<long> ids)
        {
            return ids.Select(i => SelectMusicById(i)).Where(i => i != null).ToList();
        }

        public Playlist SelectPlaylistById(long id)
        {
            return Playlists.FirstOrDefault(i => i.Id == id);
        }

        public void Played(Music music)
        {
            if (music == null) return;
            Music newMusic = SelectMusicById(music.Id);
            Music oldMusic = newMusic.Copy();
            newMusic.Played();
            RecentPlayedSongs.AddOrMoveToTheFirst(music.Id);
            if (LimitedRecentPlayedItems > -1 && RecentPlayedSongs.Count > LimitedRecentPlayedItems)
                RecentPlayedSongs.RemoveAt(LimitedRecentPlayedItems);
            NotifyMusicModified(oldMusic, newMusic);
        }

        public void Search(string keyword)
        {
            RecentSearches.AddOrMoveToTheFirst(keyword);
        }

        public NamingError ValidatePlaylistName(string newName)
        {
            if (string.IsNullOrEmpty(newName) || string.IsNullOrWhiteSpace(newName))
                return NamingError.EmptyOrWhiteSpace;
            if (newName.Length > 50)
                return NamingError.TooLong;
            if (newName == Constants.NowPlaying || newName == Constants.MyFavorites || Playlists.Any(p => p.Name == newName))
                return NamingError.Used;
            if (newName.Contains(TileHelper.StringConcatenationFlag) || newName.Contains("{0}"))
                return NamingError.Special;
            return NamingError.Good;
        }

        public static async Task<NamingError> ValidateFolderName(string root, string newName)
        {
            if (string.IsNullOrEmpty(newName) || string.IsNullOrWhiteSpace(newName))
                return NamingError.EmptyOrWhiteSpace;
            if (newName.Length > 50)
                return NamingError.TooLong;
            if (await FileHelper.FolderExists(Path.Combine(root, newName)))
                return NamingError.Used;
            return NamingError.Good;
        }

        public void AddPlaylist(string name, object data = null)
        {
            Playlist playlist = new Playlist(name)
            {
                Id = settings.IdGenerator.GeneratePlaylistId()
            };
            if (data != null) playlist.Add(data);
            Playlists.Add(playlist);
            foreach (var listener in PlaylistEventListeners)
                listener.Added(playlist, null);
        }

        public void RenamePlaylist(Playlist playlist, string newName)
        {
            Playlist target = SelectPlaylistById(playlist.Id);
            if (target.Name == newName)
            {
                return;
            }
            foreach (var item in Preference.PreferredPlaylists)
            {
                if (item.Id == playlist.Id.ToString())
                {
                    item.Name = newName;
                }
            }
            target.Name = newName;
            foreach (var listener in PlaylistEventListeners)
                listener.Renamed(target);
        }

        public void InsertPlaylist(Playlist playlist, int index)
        {
            playlist.Id = IdGenerator.GeneratePlaylistId();
            Playlists.Insert(index, playlist);
            foreach (var listener in PlaylistEventListeners)
                listener.Added(playlist, index);
        }

        public int RemovePlaylist(Playlist playlist)
        {
            int index = Playlists.IndexOf(playlist);
            Playlists.RemoveAt(index);
            foreach (var listener in PlaylistEventListeners)
                listener.Removed(playlist, index);
            return index;
        }

        /**
         * Add branch to root
         */
        public async Task AddFolder(FolderTree branch, FolderTree root)
        {
            StorageFolder folder = await root.GetStorageFolderAsync();
            await folder.CreateFolderAsync(branch.Name);

            Tree.FindTree(root)?.Trees.Add(branch);
            foreach (var listener in FolderTreeEventListeners)
                listener.Added(branch, root);
        }

        public async Task RenameFolder(FolderTree original, string newName)
        {
            StorageFolder folder = await original.GetStorageFolderAsync();
            await folder.RenameAsync(newName);
            string newPath = folder.Path;

            foreach (var item in Preference.PreferredFolders)
            {
                if (original.Equals(Tree.FindTree(item.Id)))
                {
                    item.Name = newName;
                }
            }
            Tree.FindTree(original)?.Rename(newPath);
            foreach (var listener in FolderTreeEventListeners)
                listener.Renamed(original, newPath);
            original.Rename(newPath);
        }

        public void DeleteFolder(FolderTree target)
        {
            FolderTree folderTree = Tree.FindTree(target.ParentPath);
            if (folderTree == null) return;
            folderTree.Trees.Remove(target);
            MusicLibrary.RemoveAll(pair => pair.Value.Path.StartsWith(target.Path));
            RecentPlayedSongs.RemoveAll(id => SelectMusicById(id).Path.StartsWith(target.Path));
            foreach (var playlist in Playlists) playlist.RemoveAll(i => i.Path.StartsWith(target.Path));
            MyFavorites.RemoveAll(i => i.Path.StartsWith(target.Path));
            Preference.PreferredSongs.RemoveAll(i => SelectMusicById(int.Parse(i.Id)).Path.StartsWith(target.Path));
            Preference.PreferredFolders.RemoveAll(i => i.Id == folderTree.Id.ToString());

            foreach (var listener in FolderTreeEventListeners)
                listener.Removed(target);
        }

        public void MoveFolder(FolderTree tree, string path)
        {
            Tree.MoveBranch(tree, path);
            foreach (var music in MusicLibrary.Values)
            {
                if (music.Path.StartsWith(path))
                {
                    MoveMusic(music, path);
                }
            }

            foreach (var listener in FolderTreeEventListeners)
                listener.Renamed(tree, tree.Path);
        }

        public void MoveFile(FolderFile file, string path)
        {
            Tree.MoveFile(file, path);
            if (file.IsMusicFile())
            {
                MoveMusic(SelectMusicById(file.Id), path);
            }
        }

        private void MoveMusic(Music music, string newPath)
        {
            Music oldMusic = music.Copy();
            music.MoveToFolder(newPath);
            MusicModified(oldMusic, music);
        }

        public void MusicModified(Music before, Music after)
        {
            SelectMusicById(before.Id)?.CopyFrom(after, false);
            NotifyMusicModified(before, after);
        }

        private void NotifyMusicModified(Music before, Music after)
        {
            foreach (var listener in MusicEventListeners) listener?.Modified(before, after);
        }

        public List<Music> GetMostPlayed(int limit)
        {
            List<Music> list = new List<Music>();
            foreach (var group in AllSongs.GroupBy(m => m.PlayCount).OrderByDescending(g => g.Key))
            {
                if (list.Count > limit) break;
                list.AddRange(group);
            }
            return list;
        }

        public List<Music> GetLeastPlayed(int limit)
        {
            List<Music> list = new List<Music>();
            foreach (var group in AllSongs.GroupBy(m => m.PlayCount).OrderBy(g => g.Key))
            {
                if (list.Count > limit) break;
                list.AddRange(group);
            }
            return list;
        }
    }

    public interface IMusicEventListener
    {
        void Liked(Music music, bool isFavorite);
        void Added(Music music);
        void Removed(Music music);
        void Modified(Music before, Music after);
    }

    public interface IPlaylistEventListener
    {
        void Added(Playlist playlist, int? index);
        void Renamed(Playlist playlist);
        void Removed(Playlist playlist, int index);
    }

    public interface IFolderTreeEventListener
    {
        void Added(FolderTree folder, FolderTree root);
        void Renamed(FolderTree folder, string newPath);
        void Removed(FolderTree folder);
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

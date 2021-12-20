using SMPlayer.Helpers;
using SMPlayer.Models.DAO;
using SQLite;
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
        public static IEnumerable<Music> AllSongs { get => SQLHelper.Run(c => c.SelectAllMusic()); }
        public static IEnumerable<Music> MyFavoriteSongs { get => SQLHelper.Run(c => c.SelectPlaylistItems(settings.MyFavoritesId)); }
        public static List<Playlist> AllPlaylists { get => SQLHelper.Run(c => c.SelectAllPlaylists()); }

        public long Id { get; set; }
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
        public long MyFavoritesId { get; set; }
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

        public List<long> RecentAdded = new List<long>();

        public Settings()
        {
            MyFavorites = new Playlist(Constants.MyFavorites);
        }

        public static Music FindMusic(IMusicable target) { return FindMusic(target.ToMusic()); }
        public static Music FindMusic(Music target) 
        {
            if (target == null) return null;
            return SQLHelper.Run(c => target.Id == 0 ? c.SelectMusicByPath(target.Path) : c.SelectMusicById(target.Id)); 
        }
        public static Music FindMusic(string target) { return SQLHelper.Run(c => c.SelectMusicByPath(target)); }
        public static Music FindMusic(FolderFile target) { return FindMusic(target.FileId); }
        public static Music FindMusic(long id) { return SQLHelper.Run(c => c.SelectMusicById(id)); }
        public static List<Music> FindMusicList(IEnumerable<long> ids) { return SQLHelper.Run(c => c.SelectMusicByIds(ids)); }
        public static Playlist FindPlaylist(long id) { return SQLHelper.Run(c => c.SelectPlaylistById(id)); }
        public static Playlist FindPlaylist(Playlist playlist) { return FindPlaylist(playlist.Id); }
        public static List<Music> FindPlaylistItems(long id) { return SQLHelper.Run(c => c.SelectPlaylistItems(id)); }

        public int FindNextPlaylistNameIndex(string Name)
        {
            if (!string.IsNullOrEmpty(Name))
            {
                var siblings = SQLHelper.Run(c => c.SelectAllPlaylists())
                                        .FindAll(p => p.Name.StartsWith(Name))
                                        .Select(p => p.Name).ToHashSet();
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

        public int FindNextFolderNameIndex(FolderTree parent, string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                var siblings = SQLHelper.Run(c => c.SelectSubFolders(parent)).Select(p => p.Name).ToHashSet();
                for (int i = 1; i <= siblings.Count; i++)
                    if (!siblings.Contains(Helper.GetPlaylistName(name, i)))
                        return i;
            }
            return 0;
        }

        public string FindNextFolderName(FolderTree parent, string Name)
        {
            int index = FindNextFolderNameIndex(parent, Name);
            return index == 0 ? Name : Helper.GetPlaylistName(Name, index);
        }

        public bool IsFavorite(SQLiteConnection c, Music music)
        {
            return c.SelectPlaylistItems(MyFavoritesId).Contains(music);
        }

        public void LikeMusic(Music music)
        {
            music.Favorite = true;
            SQLHelper.Run(c =>
            {
                if (IsFavorite(c, music)) return;
                AddMusicToPlaylist(c, MyFavoritesId, music);
                foreach (var listener in MusicEventListeners) listener.Liked(music, true);
            });
        }

        public void AddMusicToPlaylist(Playlist playlist, Music music)
        {
            SQLHelper.Run(c =>
            {
                AddMusicToPlaylist(c, playlist.Id, music);
            });
        }

        private void AddMusicToPlaylist(SQLiteConnection c, long playlist, Music music)
        {
            c.Insert(music.ToPlaylistItemDAO(playlist));
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
            SQLHelper.Run(c =>
            {
                RemoveMusicFromPlaylist(MyFavoritesId, music);
            });
            foreach (var listener in MusicEventListeners) listener.Liked(music, false);
        }

        public void RemoveMusicFromPlaylist(Playlist playlist, Music music)
        {
            RemoveMusicFromPlaylist(playlist.Id, music);
        }

        private void RemoveMusicFromPlaylist(long playlistId, Music music)
        {
            SQLHelper.Run(c =>
            {
                c.Execute("delete from PlaylistItem where playlistId = ? and itemId = ?", playlistId, music.Id);
            });
        }

        public async void AddMusic(SQLiteConnection c, Music music)
        {
            if (JustRemoved.Any(m => m.PossiblyEquals(music)))
                return;
            if (music.Id != 0)
            {
                MusicModified(c, music, music);
                return;
            }
            SQLHelper.InsertMusic(c, music);
            SQLHelper.InsertRecentAdded(c, music);
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
                RemoveMusic(FindMusic(file.FileId));
            }
        }

        public void RemoveMusic(Music music)
        {
            JustRemoved.Add(music);
            SQLHelper.Run(c => ActivateMusic(c, music, ActiveState.Inactive));
            foreach (var listener in MusicEventListeners) listener.Removed(music);
        }

        public void UndoRemoveMusic(Music music)
        {
            JustRemoved.Remove(music);
            SQLHelper.Run(c => ActivateMusic(c, music, ActiveState.Active));
            foreach (var listener in MusicEventListeners) listener.Added(music);
        }

        private void ActivateMusic(SQLiteConnection c, Music music, ActiveState state)
        {
            c.Execute("update Music set State = ? where Id = ?", state, music.Id);
            c.Execute("update File set State = ? where Path = ?", state, music.Path);
            c.Execute("update PlaylistItem set State = ? where ItemId = ?", state, music.Id);
            UpdateRecentRecordState(c, RecentType.Add, music.Id.ToString(), state);
            UpdateRecentRecordState(c, RecentType.Play, music.Id.ToString(), state);
            c.Execute("update PreferenceItem set State = ? where Type = ? and ItemId = ? ", state, PreferType.Song, music.Id);
        }

        public void Played(Music music)
        {
            if (music == null) return;
            SQLHelper.Run(c =>
            {
                Music newMusic = c.SelectMusicById(music.Id);
                Music oldMusic = newMusic.Copy();
                newMusic.Played();
                c.Update(newMusic);
                UpdateRecentRecordState(c, RecentType.Play, music.Id.ToString(), ActiveState.Inactive);
                c.InsertRecentPlayed(music);
                NotifyMusicModified(oldMusic, newMusic);
            });
        }

        public void Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return;
            }
            SQLHelper.Run(c =>
            {
                UpdateRecentRecordState(c, RecentType.Search, keyword, ActiveState.Inactive);
                c.Insert(new RecentRecordDAO()
                {
                    Type = RecentType.Search,
                    Item = keyword,
                    Time = DateTimeOffset.Now,
                });
            });
        }

        private void UpdateRecentRecordState(SQLiteConnection c, RecentType recentType, string item, ActiveState state)
        {
            c.Execute("update RecentRecord set State = ? where Type = ? and Item = ? ", state, recentType, item);
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

        public Playlist AddPlaylist(string name, object data = null)
        {
            Playlist playlist = new Playlist(name);
            if (data != null) playlist.Add(data);
            SQLHelper.Run(c => c.InsertPlaylist(playlist));
            foreach (var listener in PlaylistEventListeners)
                listener.Added(playlist);
            return playlist;
        }

        public void AddPlaylist(Playlist playlist)
        {
            SQLHelper.Run(c =>
            {
                if (playlist.Id == 0)
                {
                    c.InsertPlaylist(playlist);
                }
                else
                {
                    UpdatePlaylistState(c, playlist, ActiveState.Active);
                }
            });
            foreach (var listener in PlaylistEventListeners)
                listener.Added(playlist);
        }

        public void RenamePlaylist(Playlist playlist, string newName)
        {
            SQLHelper.Run(c =>
            {
                Playlist target = c.SelectPlaylistById(playlist.Id);
                if (target.Name == newName)
                {
                    return;
                }
                target.Name = newName;
                c.UpdatePlaylist(playlist);
                PreferenceItemDAO item = c.SelectPreferenceItem(PreferType.Playlist, playlist.Id.ToString());
                if (item != null)
                {
                    item.ItemName = newName;
                    c.Update(item);
                }
                foreach (var listener in PlaylistEventListeners)
                    listener.Renamed(target);
            });
        }

        public void RemovePlaylist(Playlist playlist)
        {
            SQLHelper.Run(c =>
            {
                UpdatePlaylistState(c, playlist, ActiveState.Inactive);
            });
            foreach (var listener in PlaylistEventListeners)
                listener.Removed(playlist);
        }

        private void UpdatePlaylistState(SQLiteConnection c, Playlist playlist, ActiveState state)
        {
            c.Execute("update Playlist set State = ? where Id = ?", state, playlist.Id);
        }

        /**
         * Add branch to root
         */
        public async Task AddFolder(FolderTree branch, FolderTree root)
        {
            StorageFolder folder = await root.GetStorageFolderAsync();
            await folder.CreateFolderAsync(branch.Name);
            branch.ParentId = root.Id;
            SQLHelper.Run(c => c.InsertFolder(branch));
            foreach (var listener in FolderTreeEventListeners)
                listener.Added(branch, root);
        }

        public async Task RenameFolder(FolderTree original, string newName)
        {
            StorageFolder folder = await original.GetStorageFolderAsync();
            await folder.RenameAsync(newName);
            string newPath = folder.Path;
            original.Rename(newPath);
            SQLHelper.Run(c =>
            {
                c.Update(original.ToDAO());
                c.Execute("update PreferenceItem set ItemName = ? where ItemId = ?", newName, original.Id);
            });
            foreach (var listener in FolderTreeEventListeners)
                listener.Renamed(original, newPath);
            original.Rename(newPath);
        }

        public void DeleteFolder(FolderTree target)
        {
            SQLHelper.Run(c =>
            {
                DeleteFolder(c, target);
                foreach (var music in c.SelectAllMusic())
                {
                    if (music.Path.StartsWith(target.Path))
                    {
                        RemoveMusic(music);
                    }
                }
            });
            foreach (var listener in FolderTreeEventListeners)
                listener.Removed(target);
        }

        private void DeleteFolder(SQLiteConnection c, FolderTree target)
        {
            FolderDAO folderDAO = target.ToDAO();
            folderDAO.State = ActiveState.Inactive;
            c.Update(folderDAO);
            c.Execute("update PreferenceItem set State = ? where ItemId = ?", ActiveState.Inactive, target.Id.ToString());
            foreach (var item in c.SelectSubFolders(target))
            {
                DeleteFolder(c, item);
            }
            foreach (var item in c.SelectSubFiles(target))
            {
                FileDAO dao = item.ToDAO();
                dao.State = ActiveState.Inactive;
                c.Update(dao);
            }
        }

        public async Task MoveFolder(FolderTree tree, string path)
        {
            await MoveFolder(await tree.GetStorageFolderAsync(), await StorageFolder.GetFolderFromPathAsync(path));
            tree.Rename(tree.ParentPath, path);
            SQLHelper.Run(c =>
            {
                MoveFolder(c, tree, path);
                foreach (var music in AllSongs)
                {
                    if (music.Path.StartsWith(path))
                    {
                        MoveMusic(c, music, path);
                    }
                }
            });
            foreach (var listener in FolderTreeEventListeners)
                listener.Renamed(tree, tree.Path);
        }

        private async Task MoveFolder(StorageFolder folder, StorageFolder target)
        {
            StorageFolder subFolder = await target.CreateFolderAsync(folder.Name, CreationCollisionOption.OpenIfExists);
            foreach (var item in await folder.GetFoldersAsync())
            {
                await MoveFolder(item, subFolder);
            }
            foreach (var item in await folder.GetFilesAsync())
            {
                await item.MoveAsync(subFolder);
            }
        }

        private void MoveFolder(SQLiteConnection c, FolderTree tree, string path)
        {
            FolderTree target = c.SelectFolderByPath(path);
            tree.ParentId = target.Id;
            c.Update(target);
            foreach (var item in c.SelectSubFolders(target))
            {
                MoveFolder(c, item, path);
            }
            foreach (var item in c.SelectSubFiles(target))
            {
                MoveFile(c, item, path);
            }
        }

        public void MoveFile(FolderFile file, string path)
        {
            SQLHelper.Run(c => MoveFile(c, file, path));
        }

        private void MoveFile(SQLiteConnection c, FolderFile file, string path)
        {
            file.MoveToFolder(path);
            c.Update(file.ToDAO());
            if (file.IsMusicFile())
            {
                MoveMusic(c, FindMusic(file.FileId), path);
            }
        }

        private void MoveMusic(SQLiteConnection c, Music music, string newPath)
        {
            Music oldMusic = music.Copy();
            music.MoveToFolder(newPath);
            MusicModified(c, oldMusic, music);
        }

        public void MusicModified(Music before, Music after)
        {
            SQLHelper.Run(c => MusicModified(c, before, after));
        }

        public void MusicModified(SQLiteConnection c, Music before, Music after)
        {
            c.UpdateMusic(after);
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
        void Added(Playlist playlist);
        void Renamed(Playlist playlist);
        void Removed(Playlist playlist);
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

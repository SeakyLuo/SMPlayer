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
        public static void AddMusicEventListener(IMusicEventListener listener) { MusicEventListeners.Add(listener); }
        private static readonly List<IMusicEventListener> MusicEventListeners = new List<IMusicEventListener>();
        public static void AddPlaylistEventListener(IPlaylistEventListener listener) { PlaylistEventListeners.Add(listener); }
        private static readonly List<IPlaylistEventListener> PlaylistEventListeners = new List<IPlaylistEventListener>();
        public static void AddStorageItemEventListener(IStorageItemEventListener listener) { StorageItemEventListeners.Add(listener); }
        public static readonly List<IStorageItemEventListener> StorageItemEventListeners = new List<IStorageItemEventListener>();
        public static void AddRecentEventListener(IRecentEventListener listener) { RecentEventListeners.Add(listener); }
        private static readonly List<IRecentEventListener> RecentEventListeners = new List<IRecentEventListener>();
        public static IEnumerable<Music> AllSongs { get => SQLHelper.Run(c => c.SelectAllMusic()); }
        public static Playlist MyFavoritesPlaylist { get => FindPlaylist(settings.MyFavoritesId); }
        public static List<Playlist> AllPlaylists
        {
            get => SQLHelper.Run(c => c.SelectAllPlaylists(i => i.Id != settings.MyFavoritesId));
        }
        public static List<Playlist> AllPlaylistsWithSongs
        {
            get => SQLHelper.Run(c => c.SelectAllPlaylists(i => i.Id != settings.MyFavoritesId))
                             .AsParallel().AsOrdered()
                             .Select(i =>
                             {
                                 i.Songs = new ObservableCollection<Music>(SQLHelper.Run(c => c.SelectPlaylistItems(i.Id)));
                                 return i;
                             }).ToList();
        }
        public static FolderTree Root { get => FindFolder(settings.Tree.Id) ?? new FolderTree(); }
        public static List<FolderTree> AllFolders { get => SQLHelper.Run(c => c.Query<FolderDAO>("select * from Folder where State = ?", ActiveState.Active))
                                                                  .Select(i => i.FromDAO()).ToList(); }
        public static Music FindMusic(Music target)
        {
            if (target == null) return null;
            return SQLHelper.Run(c => target.Id == 0 ? c.SelectMusicByPath(target.Path) : c.SelectMusicById(target.Id));
        }
        public static Music FindMusic(string target) { return SQLHelper.Run(c => c.SelectMusicByPath(target)); }
        public static Music FindMusic(long id) { return SQLHelper.Run(c => c.SelectMusicById(id)); }
        public static List<Music> FindMusicList(IEnumerable<long> ids) { return ids.IsEmpty() ? new List<Music>() : SQLHelper.Run(c => c.SelectMusicByIds(ids)); }
        public static Playlist FindPlaylist(long id) { return SQLHelper.Run(c => c.SelectPlaylistById(id)); }
        public static Playlist FindPlaylist(string name) { return SQLHelper.Run(c => c.SelectPlaylistByName(name)); }
        public static List<Music> FindPlaylistItems(long id) { return SQLHelper.Run(c => c.SelectPlaylistItems(id)); }
        public static FolderTree FindFolderInfo(long id)
        {
            return SQLHelper.Run(c => c.SelectFolderInfo(id));
        }
        public static FolderTree FindFolderInfo(string path)
        {
            return SQLHelper.Run(c => c.SelectFolderInfo(path));
        }
        public static FolderTree FindFolder(long id)
        {
            return SQLHelper.Run(c => c.SelectFolder(id));
        }
        public static FolderTree FindFolder(string path)
        {
            return SQLHelper.Run(c => c.SelectFolder(path));
        }
        public static FolderTree FindFullFolder(long id)
        {
            return SQLHelper.Run(c => c.SelectFullFolder(id));
        }
        public static FolderTree FindFullFolder(string path)
        {
            return SQLHelper.Run(c => c.SelectFullFolder(path));
        }
        public static List<FolderTree> FindSubFolders(FolderTree folder)
        {
            return SQLHelper.Run(c => c.SelectSubFolders(folder));
        }
        public static FolderFile FindFile(long id)
        {
            return SQLHelper.Run(c => c.SelectFile(id));
        }
        public static FolderFile FindFile(string path)
        {
            return SQLHelper.Run(c => c.SelectFileByPath(path));
        }
        public static List<Music> RecentPlay
        {
            get => SQLHelper.Run(c => c.SelectRecentRecords(RecentType.Play)
                                       .Select(r => r.ItemId)
                                       .Select(i => c.SelectMusicById(long.Parse(i)))
                                       .ToList());
        }
        public static List<string> RecentSearch
        {
            get => SQLHelper.Run(c => c.SelectRecentRecords(RecentType.Search)
                                       .Select(r => r.ItemId)
                                       .ToList());
        }
        public static AlbumView FindAlbum(string album, string artist)
        {
            return new AlbumView(album, artist)
            {
                Songs = new ObservableCollection<Music>(AllSongs.Where(music => music.Album == album)),
            };
        } 

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
        public LocalPageViewMode LocalViewMode { get; set; } = LocalPageViewMode.Grid;
        public Playlist MyFavorites { get; set; }
        public long MyFavoritesId { get; set; }
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
        public ObservableCollection<string> RecentSearches { get; set; } = new ObservableCollection<string>();
        public VoiceAssistantLanguage VoiceAssistantPreferredLanguage { get; set; } = VoiceAssistantHelper.ConvertLanguage(Helper.CurrentLanguage);

        public SortBy SearchArtistsCriterion { get; set; } = SortBy.Default;
        public SortBy SearchAlbumsCriterion { get; set; } = SortBy.Default;
        public SortBy SearchSongsCriterion { get; set; } = SortBy.Default;
        public SortBy SearchPlaylistsCriterion { get; set; } = SortBy.Default;
        public SortBy SearchFoldersCriterion { get; set; } = SortBy.Default;
        public PreferenceSettings Preference { get; set; } = new PreferenceSettings();
        public string LastReleaseNotesVersion { get; set; }

        public Settings()
        {
            MyFavorites = new Playlist(Constants.MyFavorites);
        }

        public int FindNextPlaylistNameIndex(string Name)
        {
            if (!string.IsNullOrEmpty(Name))
            {
                var siblings = AllPlaylists.FindAll(p => p.Name.StartsWith(Name)).Select(p => p.Name).ToHashSet();
                for (int i = 1; i <= siblings.Count; i++)
                    if (!siblings.Contains(Helper.GetNextName(Name, i)))
                        return i;
            }
            return 0;
        }

        public string FindNextPlaylistName(string Name)
        {
            int index = FindNextPlaylistNameIndex(Name);
            return index == 0 ? Name : Helper.GetNextName(Name, index);
        }

        public int FindNextFolderNameIndex(FolderTree parent, string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                var siblings = SQLHelper.Run(c => c.SelectSubFolders(parent)).Select(p => p.Name).ToHashSet();
                for (int i = 1; i <= siblings.Count; i++)
                    if (!siblings.Contains(Helper.GetNextName(name, i)))
                        return i;
            }
            return 0;
        }

        public string FindNextFolderName(FolderTree parent, string Name)
        {
            int index = FindNextFolderNameIndex(parent, Name);
            return index == 0 ? Name : Helper.GetNextName(Name, index);
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
            });
            foreach (var listener in MusicEventListeners)
                listener?.Execute(music, new MusicEventArgs(MusicEventType.Like) { IsFavorite = true });
        }

        public void AddMusicToPlaylist(Playlist playlist, Music music)
        {
            SQLHelper.Run(c =>
            {
                AddMusicToPlaylist(c, playlist.Id, music);
            });
        }

        public void AddMusicToPlaylist(Playlist playlist, IEnumerable<IMusicable> musicables)
        {
            SQLHelper.Run(c =>
            {
                foreach (var musicable in musicables)
                {
                    AddMusicToPlaylist(c, playlist.Id, musicable.ToMusic());
                }
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
            foreach (var listener in MusicEventListeners)
                listener?.Execute(music, new MusicEventArgs(MusicEventType.Like) { IsFavorite = false });
        }

        public void RemoveMusicFromPlaylist(Playlist playlist, Music music)
        {
            RemoveMusicFromPlaylist(playlist.Id, music);
        }

        private void RemoveMusicFromPlaylist(long playlistId, Music music)
        {
            SQLHelper.Run(c =>
            {
                c.Execute("delete from PlaylistItem where PlaylistId = ? and ItemId = ?", playlistId, music.Id);
            });
        }

        public async Task AddFile(FolderFile item)
        {
            if (item.IsMusicFile())
            {
                Music music = (Music)item.Source;
                if (await AddMusic(music))
                {
                    item.FileId = music.Id;
                    SQLHelper.Run(c => c.InsertFile(item));
                }
            }
        }

        public async Task<bool> AddMusic(Music music)
        {
            bool isNew = SQLHelper.Run(c =>
            {
                MusicDAO musicDAO = c.Query<MusicDAO>("select * from Music where Path = ?", music.Path).FirstOrDefault();
                if (musicDAO == null)
                {
                    c.InsertMusic(music);
                }
                else
                {
                    music.Id = musicDAO.Id;
                    ActivateMusic(c, music, ActiveState.Active);
                }
                return musicDAO == null;
            });
            if (isNew && AutoLyrics) // 默认上面那种情况有了
            {
                await Task.Run(async () =>
                {
                    string lyrics = await music.GetLyricsAsync();
                    if (string.IsNullOrEmpty(lyrics))
                    {
                        await music.SaveLyricsAsync(await LyricsHelper.SearchLyrics(music));
                    }
                });
            }
            foreach (var listener in MusicEventListeners)
                listener?.Execute(music, new MusicEventArgs(MusicEventType.Add));
            return isNew;
        }

        public async Task DeleteFile(FolderFile file)
        {
            await FileHelper.DeleteFile(file.Path);
            RemoveFile(file);
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
            if (music == null) return;
            SQLHelper.Run(c => ActivateMusic(c, music, ActiveState.Inactive));
            foreach (var listener in MusicEventListeners)
                listener?.Execute(music, new MusicEventArgs(MusicEventType.Remove));
        }

        public void UndoRemoveMusic(Music music)
        {
            SQLHelper.Run(c => ActivateMusic(c, music, ActiveState.Active));
            foreach (var listener in MusicEventListeners)
                listener?.Execute(music, new MusicEventArgs(MusicEventType.Add));
        }

        private void ActivateMusic(SQLiteConnection c, Music music, ActiveState state)
        {
            c.Execute("update Music set State = ? where Id = ?", state, music.Id);
            c.Execute("update File set State = ? where Path = ?", state, music.Path);
            c.Execute("update PlaylistItem set State = ? where ItemId = ?", state, music.Id);
            UpdateRecentRecordState(c, RecentType.Play, music.Id.ToString(), state);
            c.Execute("update PreferenceItem set State = ? where Type = ? and ItemId = ? ", state, EntityType.Song, music.Id);
        }

        public void Played(Music music)
        {
            if (music == null) return;
            Music newMusic = null, oldMusic = null;
            SQLHelper.Run(c =>
            {
                newMusic = c.SelectMusicById(music.Id) ?? c.SelectMusicByPath(music.Path);
                if (newMusic == null) return; // 直接从本地文件播放而不是读取的数据库会导致null
                oldMusic = newMusic.Copy();
                newMusic.Played();
                c.UpdateMusic(newMusic);
                UpdateRecentRecordState(c, RecentType.Play, music.Id.ToString(), ActiveState.Inactive);
                c.InsertRecentPlayed(music);
            });
            if (newMusic != null)
            {
                NotifyMusicModified(oldMusic, newMusic);
                foreach (var listener in RecentEventListeners)
                    listener.Played(music);
            }
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
                    ItemId = keyword,
                    Time = DateTimeOffset.Now,
                });
            });
            foreach (var listener in RecentEventListeners)
                listener.Search(keyword);
        }

        private void UpdateRecentRecordState(SQLiteConnection c, RecentType recentType, string item, ActiveState state)
        {
            string sql = "update RecentRecord set State = ? where Type = ?";
            if (item == null)
            {
                c.Execute(sql, state, recentType);
            }
            else
            {
                c.Execute(sql + " and ItemId = ?", state, recentType, item);
            }
        }

        public NamingError ValidatePlaylistName(string newName)
        {
            if (string.IsNullOrEmpty(newName) || string.IsNullOrWhiteSpace(newName))
                return NamingError.EmptyOrWhiteSpace;
            if (newName.Length > 50)
                return NamingError.TooLong;
            if (newName == Constants.NowPlaying || newName == Constants.MyFavorites || AllPlaylists.Any(p => p.Name == newName))
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
                PreferenceItemDAO item = c.SelectPreferenceItem(EntityType.Playlist, playlist.Id.ToString());
                if (item != null)
                {
                    item.ItemName = newName;
                    c.Update(item);
                }
                foreach (var listener in PlaylistEventListeners)
                    listener.Renamed(target);
            });
        }

        public void SortPlaylist(Playlist playlist, SortBy criterion)
        {
            playlist.SetCriterionAndSort(criterion);
            SQLHelper.Run(c =>
            {
                c.UpdatePlaylist(playlist);
            });
            foreach (var listener in PlaylistEventListeners)
                listener.Sorted(playlist, criterion);
        }

        public void UpdatePlaylists(IEnumerable<Playlist> playlists)
        {
            SQLHelper.Run(c =>
            {
                foreach (var playlist in playlists)
                    c.UpdatePlaylist(playlist);
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

        public void UpdateFolder(FolderTree folder)
        {
            SQLHelper.Run(c => c.Update(folder.ToDAO()));
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
            foreach (var listener in StorageItemEventListeners)
                listener.ExecuteFolderEvent(branch, new StorageItemEventArgs(StorageItemEventType.Add) { Folder = root });
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
            foreach (var listener in StorageItemEventListeners)
                listener.ExecuteFolderEvent(original, new StorageItemEventArgs(StorageItemEventType.Rename) { Path = newPath });
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
            foreach (var listener in StorageItemEventListeners)
                listener.ExecuteFolderEvent(target, new StorageItemEventArgs(StorageItemEventType.Remove));
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

        public async Task<bool> MoveFolderAsync(FolderTree folder, FolderTree target)
        {
            bool moved = await MoveFolder(folder, target);
            foreach (var listener in StorageItemEventListeners)
                listener.ExecuteFolderEvent(folder, new StorageItemEventArgs(StorageItemEventType.Move) { Folder = target });
            return moved;
        }

        private async Task<bool> MoveFolder(FolderTree folder, FolderTree target)
        {
            StorageFolder localFolder = await folder.GetStorageFolderAsync();
            StorageFolder localTarget = await target.GetStorageFolderAsync();
            StorageFolder newFolder = await localTarget.CreateFolderAsync(folder.Name, CreationCollisionOption.OpenIfExists);
            FolderTree duplicate = FindFolderInfo(newFolder.Path);
            if (duplicate == null)
            {
                folder.MoveToFolder(target);
            }
            foreach (var item in SQLHelper.Run(c => c.SelectSubFolders(folder)))
            {
                await MoveFolder(item, duplicate ?? folder);
            }
            bool moved = true;
            foreach (var item in SQLHelper.Run(c => c.SelectSubFiles(folder)))
            {
                moved &= await MoveFileAsync(item, duplicate ?? folder);
            }
            if ((await localFolder.GetFilesAsync()).IsEmpty())
            {
                await localFolder.DeleteAsync();
            }
            if (duplicate != null && moved)
            {
                folder.State = ActiveState.Inactive;
            }
            SQLHelper.Run(c => c.Update(folder.ToDAO()));
            return moved;
        }

        public async Task<bool> MoveFileAsync(FolderFile file, FolderTree newParent)
        {
            if (await FileHelper.FileExists(Path.Combine(newParent.Path, file.NameWithExtension)))
            {
                string message = Helper.LocalizeMessage("DuplicateFoundWhenMovingFile", file.Name, newParent.Name);
                bool moved = true;
                await Helper.ShowMessageDialog(message, 2, 2,
                    ("MoveAndReplace", async () => { await MoveAndReplaceFile(file, newParent); }),
                    ("KeepBoth", async () => { await MoveAndKeepBothFile(file, newParent); }),
                    ("SkipThis", () => { moved = false; }
                ));
                return moved;
            }
            else
            {
                await MoveFile(file, newParent);
                return true;
            }
        }

        private async Task MoveFile(FolderFile file, FolderTree folder)
        {
            StorageFile localFile = await FileHelper.LoadFileAsync(file.Path);
            StorageFolder targetFolder = await FileHelper.LoadFolderAsync(folder.Path);
            await localFile.MoveAsync(targetFolder);
            SQLHelper.Run(c => MoveFile(c, file, folder));
            foreach (var listener in StorageItemEventListeners)
                listener.ExecuteFileEvent(file, new StorageItemEventArgs(StorageItemEventType.Move) { Folder = folder});
        }

        private async Task MoveAndReplaceFile(FolderFile file, FolderTree newParent)
        {
            await DeleteFile(FindFile(Path.Combine(newParent.Path, file.NameWithExtension)));
            await MoveFile(file, newParent);
        }

        private async Task MoveAndKeepBothFile(FolderFile file, FolderTree newParent)
        {
            StorageFile localFile = await file.GetStorageFileAsync();
            StorageFolder localFolder = await FileHelper.LoadFolderAsync(newParent.Path);
            HashSet<string> filenames = (await localFolder.GetFilesAsync()).Select(i => i.Name).ToHashSet();
            string newFilename;
            int index = 1;
            do
            {
                newFilename = Helper.GetNextName(localFile.DisplayName, index++) + localFile.FileType;
            } while (filenames.Contains(newFilename));
            await localFile.MoveAsync(localFolder, newFilename);
            foreach (var listener in StorageItemEventListeners)
                listener.ExecuteFileEvent(file, new StorageItemEventArgs(StorageItemEventType.Move) { Folder = newParent });
            SQLHelper.Run(c => MoveFile(c, file, newParent, newFilename));
        }

        private void MoveFile(SQLiteConnection c, FolderFile file, FolderTree newParent, string newFilename = "")
        {
            FolderFile copy = file.Copy();
            copy.MoveToFolder(newParent, newFilename);
            c.Update(copy.ToDAO());
            if (copy.IsMusicFile())
            {
                MoveMusic(c, FindMusic(copy.FileId), newParent.Path);
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

        private void MusicModified(SQLiteConnection c, Music before, Music after)
        {
            c.UpdateMusic(after);
            NotifyMusicModified(before, after);
        }

        private void NotifyMusicModified(Music before, Music after)
        {
            foreach (var listener in MusicEventListeners)
                listener?.Execute(before, new MusicEventArgs(MusicEventType.Modify) { ModifiedMusic = after });
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

        public void RemoveRecentPlayed(Music music = null)
        {
            SQLHelper.Run(c =>
            {
                UpdateRecentRecordState(c, RecentType.Play, music?.Id.ToString(), ActiveState.Inactive);
            });
        }

        // 极端情况可能更改多个RecentRecord，先忽略吧
        public void UndoRemoveRecentPlayed(Music music)
        {
            SQLHelper.Run(c =>
            {
                UpdateRecentRecordState(c, RecentType.Play, music.Id.ToString(), ActiveState.Active);
            });
        }

        public void RemoveSearchHistory(string keyword = null)
        {
            SQLHelper.Run(c =>
            {
                UpdateRecentRecordState(c, RecentType.Search, keyword, ActiveState.Inactive);
            });
        }

        public void UndoRemoveSearchHistory(string keyword)
        {
            SQLHelper.Run(c =>
            {
                UpdateRecentRecordState(c, RecentType.Search, keyword, ActiveState.Active);
            });
        }
    }

    public interface IMusicEventListener
    {
        void Execute(Music music, MusicEventArgs args);
    }

    public interface IPlaylistEventListener
    {
        void Added(Playlist playlist);
        void Renamed(Playlist playlist);
        void Removed(Playlist playlist);
        void Sorted(Playlist playlist, SortBy criterion);
    }

    public interface IRecentEventListener
    {
        void Search(string keyword);
        void Played(Music music);
    }

    public interface IStorageItemEventListener
    {
        void ExecuteFileEvent(FolderFile file, StorageItemEventArgs args);
        void ExecuteFolderEvent(FolderTree folder, StorageItemEventArgs args);
    }

    public enum MusicEventType
    {
        Like, Add, Remove, Modify
    }

    public class MusicEventArgs
    {
        public MusicEventType EventType { get; set; }
        public bool IsFavorite { get; set; }
        public Music ModifiedMusic { get; set; } 

        public MusicEventArgs(MusicEventType eventType)
        {
            EventType = eventType;
        }
    }

    public class StorageItemEventArgs
    {
        public StorageItemEventType EventType { get; set; }
        public string Path { get; set; }
        public FolderTree Folder { get; set; }
        public StorageItemEventArgs(StorageItemEventType eventType)
        {
            EventType = eventType;
        }
    }

    public enum StorageItemEventType
    {
        Add, Rename, Remove, Move, Update, BeforeReset, Reset
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

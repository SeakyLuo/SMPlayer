using Microsoft.Data.Sqlite;
using SMPlayer.Models;
using SMPlayer.Models.DAO;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SMPlayer.Helpers
{
    public static class SQLHelper
    {
        public const string DBFileName = "SMPlayerSettings.db";
        private static bool Inited = false;

        private static string BuildDBPath()
        {
            return Path.Combine(Helper.LocalFolder.Path, DBFileName);
        }

        public static async Task<bool> Initialized()
        {
            if (Inited) return true;
            return await StorageHelper.FileExists(BuildDBPath());
        }

        public async static Task Init()
        {
            if (await Initialized())
            {
                Run(c =>
                {
                    c.CreateTable<PreferenceSettingsDAO>(CreateFlags.AllImplicit | CreateFlags.AutoIncPK);
                    c.CreateTable<AuthorizedDeviceDAO>(CreateFlags.AllImplicit | CreateFlags.AutoIncPK);
                    c.AlterTableAddColumn("Settings", "RemotePlayPassword VARCHAR(50) DEFAULT ''");
                    c.AlterTableAddColumn("Settings", "UseFilenameNotMusicName INTEGER DEFAULT ''");
                    c.AlterTableAddColumn("Settings", "NotificationLyricsSource INTEGER DEFAULT 0");
                    c.AlterTableAddColumn("Settings", "NotificationLyricsSource INTEGER DEFAULT 0");
                    c.AlterTableAddColumn("Settings", "SaveLyricsImmediately INTEGER DEFAULT 0");
                });
                return;
            }
            MainPage.AddMainPageLoadedListener(async () =>
            {
                MainPage.Instance.Loader.ShowIndeterminant("UpdateDBMsgInitDatabase");
                await Helper.LocalFolder.CreateFileAsync(DBFileName, CreationCollisionOption.OpenIfExists);
                Run(c =>
                {
                    CreateFlags flag = CreateFlags.AllImplicit | CreateFlags.AutoIncPK;
                    c.CreateTable<SettingsDAO>(flag);
                    c.CreateTable<MusicDAO>(flag);
                    c.CreateIndex<MusicDAO>(i => i.Path, true);
                    c.CreateTable<FolderDAO>(flag);
                    c.CreateIndex<FolderDAO>(i => i.Path, true);
                    c.CreateIndex<FolderDAO>(i => i.ParentId);
                    c.CreateTable<FileDAO>(flag);
                    c.CreateIndex<FileDAO>(i => i.Path, true);
                    c.CreateIndex<FileDAO>(i => i.ParentId);
                    c.CreateTable<PlaylistDAO>(flag);
                    c.CreateTable<PlaylistItemDAO>(flag);
                    c.CreateIndex<PlaylistItemDAO>(i => i.PlaylistId);
                    c.CreateTable<PreferenceSettingsDAO>(flag);
                    c.CreateTable<PreferenceItemDAO>(flag);
                    c.CreateIndex(PreferenceItemDAO.TableName, new string[] { "Type", "ItemId" });
                    c.CreateTable<RecentRecordDAO>(flag);
                    c.CreateIndex(RecentRecordDAO.TableName, new string[] { "Type", "ItemId" });
                });
                try
                {
                    await SettingsHelper.LoadSettingsAndInsertToDb();
                }
                catch (Exception ex)
                {
                    Log.Error("SettingsHelper.LoadSettingsAndInsertToDb Exception {0}", ex);
                    Run(c =>
                    {
                        c.DeleteAll<SettingsDAO>();
                        c.DeleteAll<MusicDAO>();
                        c.DeleteAll<FolderDAO>();
                        c.DeleteAll<FileDAO>();
                        c.DeleteAll<PlaylistDAO>();
                        c.DeleteAll<PlaylistItemDAO>();
                        c.DeleteAll<PreferenceSettingsDAO>();
                        c.DeleteAll<PreferenceItemDAO>();
                        c.DeleteAll<RecentRecordDAO>();
                    });
                    Helper.ShowNotification("LoadSettingsAndInsertToDbFailed", 10000);
                }
                MainPage.Instance.Loader.Hide();
            });
        }

        private static void AlterTableAddColumn(this SQLiteConnection c, string tableName, string column)
        {
            string sql = $"Alter Table {tableName} Add Column {column}";
            try
            {
                c.Execute(sql);
            }
            catch (SQLiteException ex)
            {
                Log.Warn($"execute sql {sql} Exception {ex}");
            }
        }

        public static void Run(Action<SQLiteConnection> connectionHandler)
        {
            using (SQLiteConnection connection = new SQLiteConnection(BuildDBPath()))
            {
                connectionHandler.Invoke(connection);
            }
        }

        public static T Run<T>(Func<SQLiteConnection, T> connectionHandler)
        {
            using (SQLiteConnection connection = new SQLiteConnection(BuildDBPath()))
            {
                try
                {
                    return connectionHandler.Invoke(connection);
                }
                catch (Exception e)
                {
                    Log.Error($"Run SQL failed, Exception {e}");
                    throw e;
                }
            }
        }

        public static async Task<T> RunAsync<T>(Func<SQLiteConnection, Task<T>> connectionHandler)
        {
            using (SQLiteConnection connection = new SQLiteConnection(BuildDBPath()))
            {
                return await connectionHandler.Invoke(connection);
            }
        }

        public static void ClearInactive()
        {
            Run(c =>
            {
                c.Execute("delete from File where State = ?", ActiveState.Inactive);
                c.Execute("delete from Folder where State = ?", ActiveState.Inactive);
                c.Execute("delete from Music where State = ?", ActiveState.Inactive);
                c.Execute("delete from Playlist where State = ?", ActiveState.Inactive);
                c.Execute("delete from PlaylistItem where State = ?", ActiveState.Inactive);
                c.Execute("delete from PreferenceItem where State = ?", ActiveState.Inactive);
                c.Execute("delete from RecentRecord where State = ?", ActiveState.Inactive);
            });
        }

        public static SettingsDAO InsertSettings(this SQLiteConnection c, Settings src)
        {
            SettingsDAO dao = src.ToDAO();
            c.Insert(dao);
            src.Id = dao.Id;
            return dao;
        }

        public static Settings SelectSettings(this SQLiteConnection c)
        {
            Settings settings = c.Query<SettingsDAO>("select * from Settings order by Id desc").FirstOrDefault()?.FromDAO();
            if (settings == null) return null;
            settings.Tree = c.SelectFolderInfo(settings.RootPath) ?? new FolderTree();
            return settings;
        }

        public static PreferenceSettings SelectPreferenceSettings(this SQLiteConnection c)
        {
            PreferenceSettingsDAO dao;
            try
            {
                dao = c.Query<PreferenceSettingsDAO>("select * from PreferenceSetting order by Id desc").FirstOrDefault() ??
                      c.Query<PreferenceSettingsDAO>("select * from PreferenceSettings limit 1").FirstOrDefault();
            }
            catch (Exception e)
            {
                Log.Warn("query PreferenceSetting failed {0}", e);
                return null;
            }
            if (dao == null) return null;
            PreferenceSettings settings = dao.FromDAO();
            settings.RecentAdded = c.SelectPreferenceItem(dao.RecentAddedId).FromDAO();
            settings.MyFavorites = c.SelectPreferenceItem(dao.MyFavoritesId).FromDAO();
            settings.MostPlayed = c.SelectPreferenceItem(dao.MostPlayedId).FromDAO();
            settings.LeastPlayed = c.SelectPreferenceItem(dao.LeastPlayedId).FromDAO();
            return settings;
        }

        public static void UpdatePreferenceSettings(this SQLiteConnection c, PreferenceSettings settings)
        {
            c.Update(settings.ToDAO());
        }

        public static MusicDAO InsertMusic(this SQLiteConnection c, Music src)
        {
            MusicDAO dao = src.ToDAO();
            c.Insert(dao);
            src.Id = dao.Id;
            return dao;
        }

        public static void UpdateMusic(this SQLiteConnection c, Music src)
        {
            MusicDAO dao = src.ToDAO();
            c.Update(dao);
        }

        public static FolderDAO InsertFolder(this SQLiteConnection c, FolderTree src)
        {
            FolderDAO dao = src.ToDAO();
            c.Insert(dao);
            src.Id = dao.Id;
            return dao;
        }

        public static FileDAO InsertFile(this SQLiteConnection c, FolderFile src)
        {
            FileDAO dao = src.ToDAO();
            c.Insert(dao);
            src.Id = dao.Id;
            return dao;
        }
        public static PlaylistDAO InsertPlaylist(this SQLiteConnection c, Playlist src)
        {
            src.Priority = c.SelectAllPlaylists().Count;
            PlaylistDAO dao = src.ToDAO();
            c.Insert(dao);
            c.InsertAll(dao.Songs.Select(i => { i.PlaylistId = dao.Id; return i; }));
            src.Id = dao.Id;
            return dao;
        }

        public static PlaylistDAO UpdatePlaylist(this SQLiteConnection c, Playlist src)
        {
            PlaylistDAO dao = src.ToDAO();
            c.Update(dao);
            return dao;
        }

        public static void UpdateAuthorizedDevice(this SQLiteConnection c, AuthorizedDevice src)
        {
            src.UpdateTime = DateTime.Now;
            c.Update(src.ToDao());
        }

        public static PreferenceItemDAO InsertPreferenceItem(this SQLiteConnection c, PreferenceItem src, EntityType type)
        {
            PreferenceItemDAO dao = src.ToDAO(type);
            c.Insert(dao);
            src.ThisId = dao.Id;
            return dao;
        }

        public static PreferenceItemDAO InsertPreferenceItem(this SQLiteConnection c, PreferenceItem src)
        {
            PreferenceItemDAO dao = src.ToDAO();
            c.Insert(dao);
            src.ThisId = dao.Id;
            return dao;
        }

        public static void InsertRecentAdded(this SQLiteConnection c, MusicView src)
        {
            c.Insert(new RecentRecordDAO()
            {
                Type = RecentType.Add,
                ItemId = src.Id.ToString(),
                Time = src.DateAdded,
            });
        }

        public static void InsertRecentPlayed(this SQLiteConnection c, Music src)
        {
            c.Insert(new RecentRecordDAO()
            {
                Type = RecentType.Play,
                ItemId = src.Id.ToString(),
                Time = DateTimeOffset.Now,
            });
        }

        public static void InsertRecentAdded(this SQLiteConnection c, string src)
        {
            c.Insert(new RecentRecordDAO()
            {
                Type = RecentType.Add,
                ItemId = src,
                Time = DateTimeOffset.Now,
            });
        }

        public static void InsertAuthorizedDevice(this SQLiteConnection c, AuthorizedDevice src)
        {
            src.CreateTime = DateTime.Now;
            src.UpdateTime = DateTime.Now;
            AuthorizedDeviceDAO dao = src.ToDao();
            c.Insert(dao);
            src.Id = dao.Id;
        }

        public static IEnumerable<Music> SelectAllMusic(this SQLiteConnection c)
        {
            return c.Query<MusicDAO>("select * from Music where State = ?", ActiveState.Active).Select(i => i.FromDAO());
        }

        public static int CountAllMusic(this SQLiteConnection c)
        {
            return c.ExecuteScalar<int>("select count(*) from Music where State = ?", ActiveState.Active);
        }

        public static Music SelectMusicByIdIncludeHidden(this SQLiteConnection c, long id)
        {
            ActiveState[] activeStates = { ActiveState.Active, ActiveState.Hidden, ActiveState.ParentHidden };
            string states = activeStates.Select(i => (int)i).Join(",");
            return c.Query<MusicDAO>($"select * from Music where Id = ? and State in ({states})", id, ActiveState.Active).FirstOrDefault()?.FromDAO();
        }

        public static Music SelectMusicById(this SQLiteConnection c, long id)
        {
            return c.Query<MusicDAO>("select * from Music where Id = ? and State = ?", id, ActiveState.Active).FirstOrDefault()?.FromDAO();
        }
        public static List<Music> SelectMusicByIds(this SQLiteConnection c, IEnumerable<long> Ids)
        {
            return c.Query<MusicDAO>($"select * from Music where Id in ({Ids.Join(",")}) and State = ?", ActiveState.Active).Select(i => i.FromDAO()).ToList();
        }

        public static Music SelectMusicByPath(this SQLiteConnection c, string path)
        {
            return c.Query<MusicDAO>("select * from Music where Path = ? and State = ?", path, ActiveState.Active).FirstOrDefault().FromDAO();
        }
        public static List<Music> SelectMusicByPaths(this SQLiteConnection c, List<string> paths)
        {
            if (paths.IsEmpty())
            {
                return new List<Music>();
            }
            object[] args = new object[paths.Count + 1];
            for (int i = 0; i < args.Length - 1; i++)
            {
                args[i] = paths[i];
            }
            args[paths.Count] = ActiveState.Active;
            return c.Query<MusicDAO>($"select * from Music where Path in ({paths.Select(p => "?").Join(",")}) and State = ?", args)
                .Select(i => i.FromDAO())
                .Where(i => i != null).ToList();
        }

        public static FolderTree SelectFullFolder(this SQLiteConnection c, long id)
        {
            FolderTree root = c.SelectFolder(id);
            if (root == null) return null;
            root.Trees = root.Trees.AsParallel().Select(i => c.SelectFullFolder(i.Id)).ToList();
            return root;
        }
        public static FolderTree SelectFullFolder(this SQLiteConnection c, string path)
        {
            FolderTree root = c.SelectFolder(path);
            if (root == null) return null;
            root.Trees = root.Trees.AsParallel().Select(i => c.SelectFullFolder(i.Id)).ToList();
            return root;
        }
        public static FolderTree SelectFolder(this SQLiteConnection c, long id)
        {
            FolderTree root = c.SelectFolderInfo(id);
            if (root == null) return null;
            root.Files = c.SelectSubFiles(root);
            root.Trees = c.SelectSubFolders(root);
            return root;
        }
        public static FolderTree SelectFolder(this SQLiteConnection c, string path)
        {
            FolderTree root = c.SelectFolderInfoOfStates(path, ActiveState.Active);
            if (root == null) return null;
            root.Files = c.SelectSubFiles(root);
            root.Trees = c.SelectSubFolders(root);
            return root;
        }

        public static FolderTree SelectFolderIncludingHidden(this SQLiteConnection c, string path)
        {
            ActiveState[] states = { ActiveState.Active, ActiveState.Hidden, ActiveState.ParentHidden };
            FolderTree root = c.SelectFolderInfoOfStates(path, states);
            if (root == null) return null;
            root.Files = c.SelectSubFilesOfStates(root, states);
            root.Trees = c.SelectSubFoldersOfStates(root, states);
            return root;
        }

        public static FolderTree SelectFolderInfo(this SQLiteConnection c, long id)
        {
            return c.Query<FolderDAO>("select * from Folder where Id = ? and State = ?", id, ActiveState.Active).FirstOrDefault()?.FromDAO();
        }

        public static FolderTree SelectFolderInfo(this SQLiteConnection c, string path)
        {
            return SelectFolderDAOByPath(c, path)?.FromDAO();
        }

        public static FolderTree SelectAnyFolderInfo(this SQLiteConnection c, string path)
        {
            return c.Query<FolderDAO>("select * from Folder where Path = ?", path).FirstOrDefault()?.FromDAO();
        }

        public static FolderTree SelectFolderInfoOfStates(this SQLiteConnection c, string path, params ActiveState[] activeStates)
        {
            string states = string.Join(",", activeStates.Select(i => (int)i));
            return c.Query<FolderDAO>($"select * from Folder where Path = ? and State in ({states})", path).FirstOrDefault()?.FromDAO();
        }

        public static bool FolderExists(this SQLiteConnection c, string path)
        {
            return SelectFolderDAOByPath(c, path) != null;
        }

        private static FolderDAO SelectFolderDAOByPath(this SQLiteConnection c, string path)
        {
            return c.Query<FolderDAO>("select * from Folder where Path = ? and State = ?", path, ActiveState.Active).FirstOrDefault();
        }

        public static List<FolderTree> SelectSubFolders(this SQLiteConnection c, FolderTree folder)
        {
            return c.Query<FolderDAO>("select * from Folder where ParentId = ? and State = ?", folder.Id, ActiveState.Active).Select(i => i.FromDAO()).ToList();
        }

        public static List<FolderTree> SelectSubFoldersOfStates(this SQLiteConnection c, FolderTree folder, params ActiveState[] activeStates)
        {
            string states = string.Join(",", activeStates.Select(i => (int)i));
            return c.Query<FolderDAO>($"select * from Folder where ParentId = ? and State in ({states})", folder.Id).Select(i => i.FromDAO()).ToList();
        }

        public static FolderFile SelectFile(this SQLiteConnection c, long Id)
        {
            return c.Query<FileDAO>("select * from File where Id = ?", Id).FirstOrDefault()?.FromDAO();
        }

        public static FolderFile SelectFileOfState(this SQLiteConnection c, long Id, ActiveState state = ActiveState.Active)
        {
            return c.Query<FileDAO>("select * from File where Id = ? and State = ?", Id, state).FirstOrDefault()?.FromDAO();
        }

        public static FolderFile SelectFileByPath(this SQLiteConnection c, string path)
        {
            return SelectFileDAOByPath(c, path)?.FromDAO();
        }
        
        public static FolderFile SelectFileByPathIncludingHidden(this SQLiteConnection c, string path)
        {
            ActiveState[] states = { ActiveState.Active, ActiveState.Hidden, ActiveState.ParentHidden };
            return SelectFileDAOByPathOfStates(c, path, states)?.FromDAO();
        }

        public static bool FileExists(this SQLiteConnection c, string path)
        {
            return SelectFileDAOByPath(c, path) != null;
        }

        private static FileDAO SelectFileDAOByPath(this SQLiteConnection c, string path)
        {
            return c.Query<FileDAO>("select * from File where Path = ? and State = ?", path, ActiveState.Active).FirstOrDefault();
        }

        private static FileDAO SelectFileDAOByPathOfStates(this SQLiteConnection c, string path, params ActiveState[] activeStates)
        {
            string states = string.Join(",", activeStates.Select(i => (int)i));
            return c.Query<FileDAO>($"select * from File where Path = ? and State in ({states})", path).FirstOrDefault();
        }

        public static List<FolderFile> SelectSubFiles(this SQLiteConnection c, FolderTree folder)
        {
            return c.Query<FileDAO>("select * from File where ParentId = ? and State = ?", folder.Id, ActiveState.Active).Select(i => i.FromDAO()).ToList();
        }

        public static List<FolderFile> SelectSubFilesOfStates(this SQLiteConnection c, FolderTree folder, params ActiveState[] activeStates)
        {
            string states = string.Join(",", activeStates.Select(i => (int)i));
            return c.Query<FileDAO>($"select * from File where ParentId = ? and State in ({states})", folder.Id, states).Select(i => i.FromDAO()).ToList();
        }

        public static List<Playlist> SelectAllPlaylists(this SQLiteConnection c, Func<Playlist, bool> predicate = null)
        {
            return c.Query<PlaylistDAO>("select * from Playlist where Id != ? and State = ?", Settings.settings.MyFavoritesId, ActiveState.Active)
                    .Select(i => i.FromDAO())
                    .OrderBy(i => i.Priority)
                    .ToList();
        }

        public static Playlist SelectPlaylistByName(this SQLiteConnection c, string name)
        {
            return c.Query<PlaylistDAO>("select * from Playlist where Name = ? and State = ?", name, ActiveState.Active).FirstOrDefault()?.FromDAO();
        }

        public static Playlist SelectPlaylistById(this SQLiteConnection c, long id)
        {
            Playlist playlist = c.Query<PlaylistDAO>("select * from Playlist where Id = ? and State = ?", id, ActiveState.Active).FirstOrDefault()?.FromDAO();
            if (playlist == null) return null;
            playlist.Songs = SelectPlaylistItems(c, id);
            return playlist;
        }

        public static List<Music> SelectPlaylistItems(this SQLiteConnection c, long id)
        {
            List<PlaylistItemDAO> items = c.Query<PlaylistItemDAO>("select * from PlaylistItem where PlaylistId = ? and State = ?", id, ActiveState.Active);
            return SelectMusicByIds(c, items.Select(i => i.ItemId)).ToList();
        }

        public static List<PreferenceItem> SelectPreferenceItems(this SQLiteConnection c, EntityType preferType)
        {
            return c.Query<PreferenceItemDAO>("select * from PreferenceItem where Type = ? and State = ?", preferType, ActiveState.Active).Select(i => i.FromDAO()).ToList();
        }

        public static PreferenceItemDAO SelectPreferenceItem(this SQLiteConnection c, long id)
        {
            return c.Query<PreferenceItemDAO>("select * from PreferenceItem where Id = ?", id, ActiveState.Active).FirstOrDefault();
        }

        public static PreferenceItemDAO SelectPreferenceItem(this SQLiteConnection c, EntityType preferType, string itemId)
        {
            return c.Query<PreferenceItemDAO>("select * from PreferenceItem where Type = ? and ItemId = ? and State = ?", preferType, itemId, ActiveState.Active).FirstOrDefault();
        }

        public static List<RecentRecordDAO> SelectRecentRecords(this SQLiteConnection c, RecentType recentType)
        {
            return c.Query<RecentRecordDAO>("select * from RecentRecord where Type = ? and State = ? order by Id desc", recentType, ActiveState.Active);
        }
        public static List<AuthorizedDevice> SelectAuthorizedDevices(this SQLiteConnection c, ActiveState state)
        {
            return c.Query<AuthorizedDeviceDAO>("select * from AuthorizedDevice where State = ? order by Id desc", state)
                    .Select(i => i.FromDao()).ToList();
        }
    }

}

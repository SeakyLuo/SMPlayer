using Microsoft.Data.Sqlite;
using SMPlayer.Models;
using SMPlayer.Models.DAO;
using SQLite;
using System;
using System.Collections.Generic;
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
            return Inited = await FileHelper.FileExists(BuildDBPath());
        }

        public async static Task Init()
        {
            if (await Initialized())
            {
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
                return connectionHandler.Invoke(connection);
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
            settings.Tree = c.SelectFolderInfoByPath(settings.RootPath) ?? new FolderTree();
            return settings;
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

        public static void UpdatePlaylistItems(this SQLiteConnection c, long id, IEnumerable<Music> songs)
        {
            c.Execute("delete from PlaylistItem where PlaylistId = ?", id);
            c.InsertAll(songs.Select(i => i.ToPlaylistItemDAO(id)));
        }

        public static PreferenceItemDAO InsertPreferenceItem(this SQLiteConnection c, PreferenceItem src, PreferType type)
        {
            PreferenceItemDAO dao = src.ToDAO(type);
            c.Insert(dao);
            return dao;
        }

        public static void InsertRecentAdded(this SQLiteConnection c, Music src)
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

        public static IEnumerable<Music> SelectAllMusic(this SQLiteConnection c)
        {
            return c.Query<MusicDAO>("select * from Music where State = ?", ActiveState.Active).Select(i => i.FromDAO());
        }

        public static Music SelectMusicById(this SQLiteConnection c, long id)
        {
            return c.Query<MusicDAO>("select * from Music where Id = ? and State = ?", id, ActiveState.Active).FirstOrDefault()?.FromDAO();
        }
        public static List<Music> SelectMusicByIds(this SQLiteConnection c, IEnumerable<long> Ids)
        {
            return Ids.Select(id => c.SelectMusicById(id)).Where(i => i != null).ToList();
        }

        public static Music SelectMusicByPath(this SQLiteConnection c, string path)
        {
            return c.Query<MusicDAO>("select * from Music where Path = ? and State = ?", path, ActiveState.Active).FirstOrDefault().FromDAO();
        }

        public static FolderTree SelectFullFolder(this SQLiteConnection c, long id)
        {
            FolderTree root = c.SelectFolder(id);
            if (root == null) return null;
            root.Trees = root.Trees.AsParallel().AsOrdered().Select(i => c.SelectFullFolder(i.Id)).ToList();
            return root;
        }
        public static FolderTree SelectFullFolder(this SQLiteConnection c, string path)
        {
            FolderTree root = c.SelectFolder(path);
            if (root == null) return null;
            root.Trees = root.Trees.AsParallel().AsOrdered().Select(i => c.SelectFullFolder(i.Id)).ToList();
            return root;
        }
        public static FolderTree SelectFolder(this SQLiteConnection c, long id)
        {
            FolderTree root = c.SelectFolderInfoById(id);
            if (root == null) return null;
            root.Files = c.SelectSubFiles(root);
            root.Trees = c.SelectSubFolders(root);
            return root;
        }
        public static FolderTree SelectFolder(this SQLiteConnection c, string path)
        {
            FolderTree root = c.SelectFolderInfoByPath(path);
            if (root == null) return null;
            root.Files = c.SelectSubFiles(root);
            root.Trees = c.SelectSubFolders(root);
            return root;
        }

        public static FolderTree SelectFolderInfoById(this SQLiteConnection c, long id)
        {
            return c.Query<FolderDAO>("select * from Folder where Id = ? and State = ?", id, ActiveState.Active).FirstOrDefault()?.FromDAO();
        }

        public static FolderTree SelectFolderInfoByPath(this SQLiteConnection c, string path)
        {
            return SelectFolderDAOByPath(c, path)?.FromDAO();
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

        public static FolderFile SelectFile(this SQLiteConnection c, long Id)
        {
            return c.Query<FileDAO>("select * from File where Id = ? and State = ?", Id, ActiveState.Active).FirstOrDefault()?.FromDAO();
        }

        public static FolderFile SelectFileByPath(this SQLiteConnection c, string path)
        {
            return SelectFileDAOByPath(c, path)?.FromDAO();
        }

        public static bool FileExists(this SQLiteConnection c, string path)
        {
            return SelectFileDAOByPath(c, path) != null;
        }

        private static FileDAO SelectFileDAOByPath(this SQLiteConnection c, string path)
        {
            return c.Query<FileDAO>("select * from File where Path = ? and State = ?", path, ActiveState.Active).FirstOrDefault();
        }

        public static List<FolderFile> SelectSubFiles(this SQLiteConnection c, FolderTree folder)
        {
            return c.Query<FileDAO>("select * from File where ParentId = ? and State = ?", folder.Id, ActiveState.Active).Select(i => i.FromDAO()).ToList();
        }

        public static List<Playlist> SelectAllPlaylists(this SQLiteConnection c, Func<Playlist, bool> predicate = null)
        {
            return c.Query<PlaylistDAO>("select * from Playlist where State = ?", ActiveState.Active)
                    .Select(i => i.FromDAO())
                    .Where(i => predicate == null || predicate.Invoke(i))
                    .OrderBy(i => i.Priority)
                    .ToList();
        }

        public static Playlist SelectPlaylistByName(this SQLiteConnection c, string name)
        {
            return c.Query<PlaylistDAO>("select * from Playlist where Name = ? and State = ?", name, ActiveState.Active).FirstOrDefault().FromDAO();
        }

        public static Playlist SelectPlaylistById(this SQLiteConnection c, long id)
        {
            Playlist playlist = c.Query<PlaylistDAO>("select * from Playlist where Id = ? and State = ?", id, ActiveState.Active).FirstOrDefault().FromDAO();
            if (playlist == null) return null;
            playlist.Songs.SetTo(SelectPlaylistItems(c, id));
            return playlist;
        }

        public static List<Music> SelectPlaylistItems(this SQLiteConnection c, long id)
        {
            List<PlaylistItemDAO> items = c.Query<PlaylistItemDAO>("select * from PlaylistItem where PlaylistId = ? and State = ?", id, ActiveState.Active);
            return SelectMusicByIds(c, items.Select(i => i.ItemId));
        }

        public static PreferenceItemDAO SelectPreferenceItem(this SQLiteConnection c, PreferType preferType, string itemId)
        {
            return c.Query<PreferenceItemDAO>("select * from PreferenceItem where Type = ? and ItemId = ? and State = ?", preferType, itemId, ActiveState.Active).FirstOrDefault();
        }

        public static List<RecentRecordDAO> SelectRecentRecords(this SQLiteConnection c, RecentType recentType)
        {
            return c.Query<RecentRecordDAO>("select * from RecentRecord where Type = ? and State order by Id desc", recentType, ActiveState.Active);
        }
    }

}

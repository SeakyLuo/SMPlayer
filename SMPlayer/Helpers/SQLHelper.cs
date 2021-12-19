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
        private const string DBName = "SMPlayerSettings.db";

        private static string BuildDBPath()
        {
            return Path.Combine(ApplicationData.Current.LocalFolder.Path, DBName);
        }

        public async static void Init()
        {
            await ApplicationData.Current.LocalFolder.CreateFileAsync(DBName, CreationCollisionOption.OpenIfExists);
            Run(c =>
            {
                c.CreateTable<SettingsDAO>();
                c.CreateTable<MusicDAO>();
                c.CreateTable<FolderDAO>();
                c.CreateTable<FileDAO>();
                c.CreateTable<PlaylistDAO>();
                c.CreateTable<PlaylistItemDAO>();
                c.CreateTable<PreferenceSettingsDAO>();
                c.CreateTable<PreferenceItemDAO>();
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

        public static MusicDAO InsertMusic(this SQLiteConnection c, Music src)
        {
            MusicDAO dao = src.ToDAO();
            c.Insert(dao);
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
                Item = src.Id.ToString(),
                Time = src.DateAdded,
            });
        }

        public static void InsertRecentPlayed(this SQLiteConnection c, Music src)
        {
            c.Insert(new RecentRecordDAO()
            {
                Type = RecentType.Play,
                Item = src.Id.ToString(),
                Time = DateTimeOffset.Now,
            });
        }

        public static void InsertRecentAdded(this SQLiteConnection c, string src)
        {
            c.Insert(new RecentRecordDAO()
            {
                Type = RecentType.Search,
                Item = src,
                Time = DateTimeOffset.Now,
            });
        }

        public static IEnumerable<Music> SelectAllMusic(this SQLiteConnection c)
        {
            return c.Query<MusicDAO>("select * from Music where State = ?", ActiveState.Active).Select(i => i.FromDAO());
        }

        public static Music SelectMusicById(this SQLiteConnection c, long id)
        {
            return c.Query<MusicDAO>("select * from Music where Id = ? and State = ?", id, ActiveState.Active).FirstOrDefault().FromDAO();
        }
        public static List<Music> SelectMusicByIds(this SQLiteConnection c, IEnumerable<long> Ids)
        {
            return c.Query<MusicDAO>("select * from Music where Id in ? and State = ?", Ids, ActiveState.Active).Select(i => i.FromDAO()).ToList();
        }

        public static Music SelectMusicByPath(this SQLiteConnection c, string path)
        {
            return c.Query<MusicDAO>("select * from Music where Path = ? and State = ?", path, ActiveState.Active).FirstOrDefault().FromDAO();
        }

        public static FolderTree SelectFolderByPath(this SQLiteConnection c, string path)
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

        public static List<Playlist> SelectAllPlaylists(this SQLiteConnection c)
        {
            return c.Query<PlaylistDAO>("select * from Playlist where State = ?", ActiveState.Active).Select(i => i.FromDAO()).ToList();
        }

        public static Playlist SelectPlaylistByName(this SQLiteConnection c, string name)
        {
            return c.Query<PlaylistDAO>("select * from Playlist where Name = ? and State = ?", name, ActiveState.Active).FirstOrDefault().FromDAO();
        }

        public static Playlist SelectPlaylistById(this SQLiteConnection c, long id)
        {
            return c.Query<PlaylistDAO>("select * from Playlist where Id = ? and State = ?", id, ActiveState.Active).FirstOrDefault().FromDAO();
        }

        public static PreferenceItemDAO SelectPreferenceItem(this SQLiteConnection c, PreferType preferType, string itemId)
        {
            return c.Query<PreferenceItemDAO>("select * from PreferenceItem where Type = ? and ItemId = ? and State = ?", preferType, itemId, ActiveState.Active).FirstOrDefault();
        }
    }

}

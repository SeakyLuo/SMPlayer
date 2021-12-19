using Newtonsoft.Json.Linq;
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
    class SettingsHelper
    {
        public static bool Inited { get; private set; } = false;
        public const string JsonFilename = "SMPlayerSettings";

        public static async Task Init()
        {
            Inited = false;
            string json = await JsonFileHelper.ReadAsync(JsonFilename);
            if (!string.IsNullOrEmpty(json) && JsonFileHelper.Convert<Settings>(json) is Settings settings)
            {
                Settings.settings = settings;
                var jsonObject = JsonFileHelper.Convert<JObject>(json);
                List<Music> songs = FlattenFolderTreeInJson(jsonObject["Tree"]);
                //List<Music> recentAdded = await JsonFileHelper.ReadObjectAsync<List<Music>>(JsonFileName) ?? new List<Music>();
                SQLHelper.Run(c =>
                {
                    c.InsertAll(songs.Select(i => i.ToDAO()));
                    InsertTree(c, settings.Tree);
                    InsertPlaylists(c, settings.Playlists);
                    InsertPreferenceSettings(c, settings.Preference);
                    InsertRecentPlayed(c, settings);
                    InsertRecentSearches(c, settings);
                });
                await Init(settings);
            }
            else
            {
                Settings.settings = new Settings();
                Save();
            }
            Inited = true;
        }

        public static bool Init(string json)
        {
            if (JsonFileHelper.Convert<SettingsDAO>(json) is SettingsDAO dao)
            {
                Settings.settings = dao.FromDAO();
                return true;
            }
            return false;
        }

        private static List<Music> FlattenFolderTreeInJson(JToken tree)
        {
            List<Music> songs = tree["Files"].Select(i => i.ToObject<Music>()).ToList();
            songs.AddRange(tree["Trees"].SelectMany(t => FlattenFolderTreeInJson(t)));
            return songs;
        }

        private static void InsertTree(SQLiteConnection c, FolderTree tree)
        {
            FolderDAO result = c.InsertFolder(tree);
            foreach (var folder in tree.Trees)
                c.InsertFolder(folder);
            foreach (FolderFile file in tree.Files)
            {
                FileDAO fileDAO = file.ToDAO();
                fileDAO.FileId = c.SelectMusicByPath(file.Path).Id;
                fileDAO.ParentId = result.Id;
                c.Insert(fileDAO);
            }
        }
        
        private static void InsertPlaylists(SQLiteConnection c, List<Playlist> playlists)
        {
            foreach (var playlist in playlists)
            {
                foreach (var music in playlist.Songs)
                {
                    music.Id = c.SelectMusicByPath(music.Path).Id;
                }
                c.InsertPlaylist(playlist);
            }
        }

        private static void InsertPreferenceSettings(SQLiteConnection c, PreferenceSettings settings)
        {
            foreach (PreferenceItem item in settings.PreferredFolders)
            {
                item.Id = c.SelectFolderByPath(item.Id).Id.ToString();
                c.InsertPreferenceItem(item, PreferType.Folder);
            }
            foreach (PreferenceItem item in settings.PreferredSongs)
            {
                item.Id = c.SelectMusicByPath(item.Id).Id.ToString();
                c.InsertPreferenceItem(item, PreferType.Song);
            }
            foreach (PreferenceItem item in settings.PreferredPlaylists)
            {
                item.Id = c.SelectPlaylistByName(item.Id).Id.ToString();
                c.InsertPreferenceItem(item, PreferType.Playlist);
            }
            foreach (PreferenceItem item in settings.PreferredAlbums)
            {
                c.InsertPreferenceItem(item, PreferType.Album);
            }
            foreach (PreferenceItem item in settings.PreferredArtists)
            {
                c.InsertPreferenceItem(item, PreferType.Artist);
            }
            PreferenceSettingsDAO dao = settings.ToDAO();
            dao.MostPlayedId = c.InsertPreferenceItem(settings.MostPlayed, PreferType.MostPlayed).Id;
            dao.LeastPlayedId = c.InsertPreferenceItem(settings.LeastPlayed, PreferType.LeastPlayed).Id;
            dao.RecentAddedId = c.InsertPreferenceItem(settings.RecentAdded, PreferType.RecentAdded).Id;
            dao.MyFavoritesId = c.InsertPreferenceItem(settings.MyFavorites, PreferType.MyFavorites).Id;
            c.Insert(dao);
        }

        private static void InsertRecentAdded(SQLiteConnection c, Settings settings)
        {
            foreach (var item in settings.RecentAdded)
            {
                c.Insert(new RecentRecordDAO()
                {
                    Type = RecentType.Search,
                    Item = item.ToString(),
                    Time = DateTimeOffset.Now,
                });
            }
        }

        private static void InsertRecentPlayed(SQLiteConnection c, Settings settings)
        {
            foreach (var item in settings.RecentPlayed)
            {
                c.Insert(new RecentRecordDAO()
                {
                    Type = RecentType.Search,
                    Item = c.SelectMusicByPath(item)?.Id.ToString(),
                    Time = DateTimeOffset.Now,
                });
            }
        }

        private static void InsertRecentSearches(SQLiteConnection c, Settings settings)
        {
            foreach (var item in settings.RecentSearches)
            {
                c.Insert(new RecentRecordDAO()
                {
                    Type = RecentType.Search,
                    Item = item,
                    Time = DateTimeOffset.Now,
                });
            }
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
            foreach (var item in await ApplicationData.Current.LocalFolder.GetItemsAsync())
                if (item.Name.EndsWith(".TMP") || item.Name.EndsWith(".~tmp"))
                    await item.DeleteAsync();
        }

        public static void Save()
        {
            if (Settings.settings == null) return;
            Settings.settings.MusicProgress = MusicPlayer.Position;
            try
            {
                //SettingsDAO settingsDAO = Settings.settings.ToDAO();
                //JsonFileHelper.SaveAsync(NewFilename, settingsDAO);
                //JsonFileHelper.SaveAsync(Helper.TempFolder, NewFilename + Helper.TimeStamp, settingsDAO);
            }
            catch (Exception e)
            {
                Log.Warn("Save Exception {0}", e);
            }
        }
    }
}

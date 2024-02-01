using Newtonsoft.Json.Linq;
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
    class SettingsHelper
    {
        public static bool Inited { get; private set; } = false;
        public const string JsonFilename = "SMPlayerSettings";

        public static async Task InitOld()
        {
            Inited = false;
            Settings settings = await JsonFileHelper.ReadObjectAsync<Settings>(JsonFilename);
            if (await SQLHelper.Initialized() && settings == null)
            {
                return;
            }
            if (settings == null)
            {
                Settings.settings = new Settings();
                PreferenceSettings.settings = new PreferenceSettings();
            }
            else
            {
                Settings.settings = settings;
                await Init(settings);
            }
            Inited = true;
        }

        public static async Task<bool> InitNew()
        {
            if (!await SQLHelper.Initialized())
            {
                Settings.settings = new Settings();
                return false;
            }
            PreferenceSettings.settings = SQLHelper.Run(c => c.SelectPreferenceSettings()) ?? new PreferenceSettings();
            if (!await Init(SQLHelper.Run(c => c.SelectSettings())))
            {
                Settings.settings = new Settings();
                return false;
            }
            Inited = true;
            return true;
        }

        public static async Task<bool> InitWithFile(StorageFile file)
        {
            string filePath = SQLHelper.BuildDBPath();
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            StorageFile newDbFile = await file.CopyAsync(Helper.LocalFolder);
            if (newDbFile.Name != SQLHelper.DBFileName)
            {
                await newDbFile.RenameAsync(SQLHelper.DBFileName);
            }
            return await InitNew();
        }

        public static async Task LoadSettingsAndInsertToDb()
        {
            await MainPage.Instance.Loader.SetMessageAsync("UpdateDBMsgPreparing");
            string json = await JsonFileHelper.ReadAsync(JsonFilename);
            if (string.IsNullOrEmpty(json))
            {
                SQLHelper.Run(c =>
                {
                    InsertMyFavoritesAndSettings(c);
                    InsertPreferenceSettings(c, PreferenceSettings.settings = new PreferenceSettings());
                });
                return;
            }
            var jsonObject = JsonFileHelper.FromJson<JObject>(json);
            List<MusicView> songs = FlattenFolderTreeInJson(jsonObject["Tree"]);
            UpdateLog updateLog = await JsonFileHelper.ReadObjectAsync<UpdateLog>("UpdateLogger") ?? new UpdateLog();
            Settings.settings.LastReleaseNotesVersion = updateLog.LastReleaseNotesVersion;
            List<MusicDAO> list = new List<MusicDAO>();
            foreach (var i in songs)
            {
                if (i.DateAdded == null && await i.GetStorageFileAsync() is StorageFile file)
                {
                    i.DateAdded = file.DateCreated;
                }
                list.Add(i.ToDAO());
            }
            await MainPage.Instance.Loader.SetMessageAsync("UpdateDBMsgUpdatingDatabase");
            SQLHelper.Run(c =>
            {
                c.InsertAll(list);
                InsertTree(c, Settings.settings.Tree);
                InsertPlaylists(c, Settings.settings.Playlists);
                InsertPreferenceSettings(c, Settings.settings.Preference);
                InsertRecentPlayed(c, Settings.settings);
                InsertRecentSearches(c, Settings.settings);
                InsertMyFavoritesAndSettings(c);
            });
            await JsonFileHelper.DeleteFile(JsonFilename);
        }

        private static void InsertMyFavoritesAndSettings(SQLiteConnection c)
        {
            InsertMyFavorites(c);
            c.InsertSettings(Settings.settings);
        }

        public static void InsertMyFavorites(SQLiteConnection c)
        {
            PlaylistView myFavoritesVO = Settings.settings.MyFavorites;
            Playlist myFavorites = myFavoritesVO.FromVO();
            InsertPlaylist(c, myFavorites);
            Settings.settings.MyFavoritesId = Settings.settings.MyFavorites.Id = myFavorites.Id;
        }

        public static void Save()
        {
            if (Settings.settings == null) return;
            Settings.settings.MusicProgress = MusicPlayer.Position;
            SQLHelper.Run(c => c.Update(Settings.settings.ToDAO()));
            SQLHelper.Run(c => c.UpdatePreferenceSettings(PreferenceSettings.settings));
        }

        private static async Task<bool> Init(Settings settings)
        {
            if (settings == null || string.IsNullOrEmpty(settings.RootPath))
            {
                return false;
            }
            Settings.settings = settings;
            bool isSuccessful = false;
            try
            {
                StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(settings.RootPath);
                Helper.MusicFolder = storageFolder;
                isSuccessful = true;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"GetFolderFromPathAsync failed {e}");
                if (App.Inited)
                {
                    Helper.ShowNotification("RootNotFound", 4000);
                    StorageFolder folder = await StorageHelper.PickFolder();
                    if (folder == null)
                    {
                    }
                    else
                    {
                        Helper.MusicFolder = folder;
                        settings.RootPath = folder.Path;
                        isSuccessful = true;
                    }
                }
                else
                {
                    App.LoadedListeners.Add(() =>
                    {
                        Helper.ShowNotification("RootNotFound");
                        MainPage.Instance.NavigateToPage(typeof(SettingsPage));
                    });
                }
            }
            try
            {
                foreach (var item in await Helper.LocalFolder.GetItemsAsync())
                    if (item.Name.EndsWith(".TMP") || item.Name.EndsWith(".~tmp"))
                        await item.DeleteAsync();
            }
            catch (Exception)
            {

            }
            return isSuccessful;
        }

        private static List<MusicView> FlattenFolderTreeInJson(JToken tree)
        {
            List<MusicView> songs = tree["Files"].Select(i => i.ToObject<MusicView>()).ToList();
            songs.AddRange(tree["Trees"].SelectMany(t => FlattenFolderTreeInJson(t)));
            return songs;
        }

        private static void InsertTree(SQLiteConnection c, FolderTree tree)
        {
            FolderDAO result = c.InsertFolder(tree);
            foreach (var folder in tree.Trees)
            {
                folder.ParentId = result.Id;
                InsertTree(c, folder);
            }
            foreach (FolderFile file in tree.Files)
            {
                FileDAO fileDAO = file.ToDAO();
                fileDAO.FileId = c.SelectMusicByPath(file.Path).Id;
                fileDAO.ParentId = result.Id;
                c.Insert(fileDAO);
            }
        }
        
        private static void InsertPlaylists(SQLiteConnection c, List<PlaylistView> playlists)
        {
            for (int i = 0; i < playlists.Count; i++)
            {
                PlaylistView view = playlists[i];
                view.Priority = i;
                Playlist playlist = view.FromVO();
                InsertPlaylist(c, playlist);
                view.Id = playlist.Id;
            }
        }

        private static void InsertPlaylist(SQLiteConnection c, Playlist playlist)
        {
            foreach (var music in playlist.Songs)
            {
                music.Id = c.SelectMusicByPath(music.Path).Id;
            }
            c.InsertPlaylist(playlist);
        }

        private static void InsertPreferenceSettings(SQLiteConnection c, PreferenceSettings settings)
        {
            foreach (PreferenceItem item in settings.PreferredFolders)
            {
                if (c.SelectFolderInfo(item.Id) is FolderTree result)
                {
                    item.Id = result.Id.ToString();
                    c.InsertPreferenceItem(item, EntityType.Folder);
                }
            }
            foreach (PreferenceItem item in settings.PreferredSongs)
            {
                if (c.SelectMusicByPath(item.Id) is Music result)
                {
                    item.Id = result.Id.ToString();
                    c.InsertPreferenceItem(item, EntityType.Song);
                }
            }
            foreach (PreferenceItem item in settings.PreferredPlaylists)
            {
                if (c.SelectPlaylistByName(item.Id) is Playlist result)
                {
                    item.Id = result.Id.ToString();
                    c.InsertPreferenceItem(item, EntityType.Playlist);
                }
            }
            foreach (PreferenceItem item in settings.PreferredAlbums)
            {
                c.InsertPreferenceItem(item, EntityType.Album);
            }
            foreach (PreferenceItem item in settings.PreferredArtists)
            {
                c.InsertPreferenceItem(item, EntityType.Artist);
            }
            PreferenceSettingsDAO dao = settings.ToDAO();
            dao.MostPlayedId = c.InsertPreferenceItem(settings.MostPlayed, EntityType.MostPlayed).Id;
            dao.LeastPlayedId = c.InsertPreferenceItem(settings.LeastPlayed, EntityType.LeastPlayed).Id;
            dao.RecentAddedId = c.InsertPreferenceItem(settings.RecentAdded, EntityType.RecentAdded).Id;
            dao.MyFavoritesId = c.InsertPreferenceItem(settings.MyFavorites, EntityType.MyFavorites).Id;
            c.Insert(dao);
            PreferenceSettings.settings = settings;
        }

        private static void InsertRecentPlayed(SQLiteConnection c, Settings settings)
        {
            foreach (var item in settings.RecentPlayed)
            {
                c.Insert(new RecentRecordDAO()
                {
                    Type = RecentType.Play,
                    ItemId = c.SelectMusicByPath(item)?.Id.ToString(),
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
                    ItemId = item,
                    Time = DateTimeOffset.Now,
                });
            }
        }
    }
}

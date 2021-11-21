using Newtonsoft.Json.Linq;
using SMPlayer.Models;
using SMPlayer.Models.DAO;
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
        public const string JsonFilename = "SMPlayerSettings", NewFilename = "SMPlayerSettingsFile";

        public static async Task Init()
        {
            Inited = false;
            var newSettings = await JsonFileHelper.ReadAsync(NewFilename);
            if (await JsonFileHelper.ReadObjectAsync<SettingsDAO>(NewFilename) is SettingsDAO settingsDAO)
            {
                Settings.settings = settingsDAO.FromDAO();
                await Init(Settings.settings);
            }
            else
            {
                string json = await JsonFileHelper.ReadAsync(JsonFilename);
                if (!string.IsNullOrEmpty(json) && JsonFileHelper.Convert<Settings>(json) is Settings settings)
                {
                    Settings.settings = settings;
                    ResetTreeId(settings.Tree);
                    var jsonObject = JsonFileHelper.Convert<JObject>(json);
                    List<Music> songs = FlattenFolderTreeInJson(jsonObject["Tree"]);
                    settings.MusicLibrary = songs.Select(m => { m.Id = settings.Tree.FindFile(m.Path).Id; return m; })
                                                 .ToDictionary(m => m.Id);
                    settings.Playlists.ForEach(p => p.Id = settings.IdGenerator.GeneratePlaylistId());
                    ResetPlaylistSongsId(Settings.settings.MyFavorites);
                    settings.Playlists.ForEach(p => ResetPlaylistSongsId(p));
                    settings.RecentPlayedSongs = MusicPathToId(Settings.settings.RecentPlayed);

                    foreach (PreferenceItem item in Settings.settings.Preference.PreferredFolders)
                        item.Id = settings.Tree.FindTree(item.Id).Id.ToString();
                    foreach (PreferenceItem item in Settings.settings.Preference.PreferredSongs)
                        item.Id = settings.Tree.FindMusic(item.Id).Id.ToString();
                    foreach (PreferenceItem item in Settings.settings.Preference.PreferredPlaylists)
                        item.Id = settings.Playlists.FirstOrDefault(i => i.Name == item.Id).Id.ToString();
                    await Init(settings);
                }
                else
                {
                    Settings.settings = new Settings();
                    Save();
                }
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

        private static void ResetTreeId(FolderTree tree)
        {
            tree.Id = Settings.settings.IdGenerator.GenerateTreeId();
            foreach (var branch in tree.Trees)
                ResetTreeId(branch);
            foreach (var file in tree.Files)
                file.Id = Settings.settings.IdGenerator.GenerateMusicId();
        }

        private static void ResetPlaylistSongsId(Playlist playlist)
        {
            playlist.SongIds = MusicPathToId(playlist.Songs.ToList().Select(i => i.Path));
        }

        private static List<long> MusicPathToId(IEnumerable<string> paths)
        {
            return paths.Select(i => Settings.FindMusic(i)).Where(i => i != null).Select(i => i.Id).ToList();
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
            Settings.settings.MusicProgress = MediaHelper.Position;
            JsonFileHelper.SaveAsync(JsonFilename, Settings.settings);
            JsonFileHelper.SaveAsync(Helper.TempFolder, JsonFilename + Helper.TimeStamp, Settings.settings);
            try
            {
                SettingsDAO settingsDAO = Settings.settings.ToDAO();
                JsonFileHelper.SaveAsync(NewFilename, settingsDAO);
                JsonFileHelper.SaveAsync(Helper.TempFolder, NewFilename + Helper.TimeStamp, settingsDAO);
            }
            catch (Exception e)
            {
                Helper.Print(e.ToString());
            }
        }
    }
}

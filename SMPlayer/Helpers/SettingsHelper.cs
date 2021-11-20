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
            if (string.IsNullOrEmpty(newSettings))
            {
                if (await JsonFileHelper.ReadAsync(JsonFilename) is string json && !string.IsNullOrEmpty(json))
                {
                    Settings.settings = JsonFileHelper.Convert<Settings>(json);
                    if (Settings.settings.MusicLibrary.IsEmpty() && !Settings.settings.Tree.IsEmpty)
                    {
                        ResetTreeId(Settings.settings.Tree);
                        Settings.settings.MusicLibrary = Settings.settings.Tree.Flatten()
                                                                 .Select(m => { m.Id = Settings.settings.IdGenerator.GenerateMusicId(); return m; })
                                                                 .ToDictionary(m => m.Id);
                        Settings.settings.Playlists.ForEach(p => p.Id = Settings.settings.IdGenerator.GeneratePlaylistId());
                        ResetPlaylistSongsId(Settings.settings.MyFavorites);
                        Settings.settings.Playlists.ForEach(p => ResetPlaylistSongsId(p));
                        Settings.settings.RecentPlayedSongs = MusicPathToId(Settings.settings.RecentPlayed);

                        foreach (PreferenceItem item in Settings.settings.Preference.PreferredFolders)
                            item.Id = Settings.settings.Tree.FindTree(item.Id).Id.ToString();
                        foreach (PreferenceItem item in Settings.settings.Preference.PreferredSongs)
                            item.Id = Settings.settings.Tree.FindMusic(item.Id).Id.ToString();
                        foreach (PreferenceItem item in Settings.settings.Preference.PreferredPlaylists)
                            item.Id = Settings.settings.Playlists.FirstOrDefault(i => i.Name == item.Id).Id.ToString();
                    }
                    await Init(Settings.settings);
                }
                else
                {
                    Settings.settings = new Settings();
                    Save();
                }
            }
            else
            {
                Settings.settings = JsonFileHelper.Convert<SettingsDAO>(newSettings).FromDAO();
                await Init(Settings.settings);
            }
            Inited = true;
        }

        private static void ResetTreeId(FolderTree tree)
        {
            tree.Id = Settings.settings.IdGenerator.GenerateTreeId();
            foreach (var branch in tree.Trees)
                ResetTreeId(branch);
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

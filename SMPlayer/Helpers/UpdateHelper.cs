using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SMPlayer.Helpers
{
    public static class UpdateHelper
    {
        private const string JsonFileName = "UpdateLogger";
        public static UpdateLog Log;

        public static async Task Init()
        {
            Log = await JsonFileHelper.ReadObjectAsync<UpdateLog>(JsonFileName) ?? new UpdateLog();
        }

        public static void Save()
        {
            JsonFileHelper.SaveAsync(JsonFileName, Log);
        }

        public static async Task Update()
        {
            Log.DateAdded = await SettingsPage.UpdateMusicLibrary(Helper.LocalizeMessage("Updating"));
            Save();
        }

        public static void UpdateIds()
        {
            UpdateTreeFileId(Settings.settings.Tree);
            foreach (var playlist in Settings.settings.Playlists)
            {
                UpdatePlaylistId(playlist);
            }
            UpdatePlaylistId(Settings.settings.MyFavorites);
            Log.NewSettings = true;
            Save();
        }

        private static void UpdatePlaylistId(Playlist playlist)
        {
            playlist.Id = Settings.settings.IdGenerator.GeneratePlaylistId();
            foreach (var music in playlist.Songs)
            {
                if (Settings.FindMusic(music) is Music m)
                {
                    music.Id = m.Id;
                }
            }
        }

        private static void UpdateTreeFileId(FolderTree tree)
        {
            foreach (var branch in tree.Trees)
            {
                UpdateTreeFileId(branch);
            }
            foreach (var file in tree.Files)
            {
                file.Id = Settings.settings.IdGenerator.GenerateMusicId();
            }
        }

    }

    public class UpdateLog
    {
        public string LastReleaseNotesVersion { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public bool ShowReleaseNotesDialog { get => LastReleaseNotesVersion != Helper.AppVersion; }
        public bool DateAdded { get; set; } = false;
        public bool NewSettings { get; set; } = false;
        [Newtonsoft.Json.JsonIgnore]
        public bool AllUpdated { get => DateAdded && NewSettings; }
    }
}

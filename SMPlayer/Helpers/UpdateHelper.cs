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
            Log = JsonFileHelper.Convert<UpdateLog>(await JsonFileHelper.ReadAsync(JsonFileName)) ?? new UpdateLog();
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

        public static async Task FillDateAdded(FolderTree tree)
        {
            foreach (var music in tree.Files)
            {
                if (music.DateAdded == null || music.DateAdded == DateTimeOffset.MinValue)
                {
                    StorageFile file = await music.GetStorageFileAsync();
                    if (file == null)
                    {
                        //Settings.settings.RemoveMusic(music);
                    }
                    else
                    {
                        music.DateAdded = file.DateCreated;
                    }
                }
            }
            foreach (var branch in tree.Trees)
            {
                await FillDateAdded(branch);
            }
        }
    }

    public class UpdateLog
    {
        public string LastReleaseNotesVersion { get; set; }
        public bool ShowReleaseNotesDialog { get => LastReleaseNotesVersion != Helper.AppVersion; }
        public bool DateAdded { get; set; } = false;

        public bool AllUpdated { get => DateAdded; }
    }
}

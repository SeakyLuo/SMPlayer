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
        private static UpdateLog OriginalLog;

        public static async Task Init()
        {
            OriginalLog = JsonFileHelper.Convert<UpdateLog>(await JsonFileHelper.ReadAsync(JsonFileName)) ?? new UpdateLog();
            Log = OriginalLog.Copy();
        }

        public static void Save()
        {
            if (!OriginalLog.AllUpdated)
            {
                JsonFileHelper.SaveAsync(JsonFileName, Log);
            }
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
        public bool AllUpdated { get => DateAdded; }
        public bool DateAdded { get; set; } = false;

        public UpdateLog Copy()
        {
            return new UpdateLog
            {
                DateAdded = this.DateAdded
            };
        }
    }
}

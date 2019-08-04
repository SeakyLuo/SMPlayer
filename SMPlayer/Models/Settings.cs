using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SMPlayer.Models
{
    enum AppLanguage
    {
        FollowSystem = 0,
        Chinese = 1,
        English = 2
    }
    class Settings
    {
        public static Settings settings;

        public string RootPath { get; set; }
        public Music LastMusic { get; set; }
        public AppLanguage Language { get; set; }

        public Settings()
        {
            RootPath = KnownFolders.MusicLibrary.Path;
            Language = AppLanguage.FollowSystem;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SMPlayer.Models
{
    class Settings
    {
        public static Settings settings;

        public string rootPath { get; set; }
        public Music lastMusic { get; set; }

        public Settings()
        {
            rootPath = Windows.Storage.Pickers.PickerLocationId.MusicLibrary.ToString();
        }
    }
}

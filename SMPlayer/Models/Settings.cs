﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SMPlayer.Models
{
    [Serializable]
    public class Settings
    {
        public static Settings settings;
        private static readonly string FILENAME = "Settings.json";

        public string RootPath { get; set; }
        public FolderTree Tree { get; set; }
        public Music LastMusic { get; set; }
        public AppLanguage Language { get; set; }
        public PlayMode Mode { get; set; }
        public double Volume { get; set; }
        public bool IsNavigationCollapsed { get; set; }

        public Settings()
        {
            RootPath = "";
            Tree = new FolderTree();
            Language = AppLanguage.FollowSystem;
            Mode = PlayMode.Once;
            Volume = 50.0d;
            IsNavigationCollapsed = true;
        }

        public static async void Init()
        {
            var json = await JsonFileHelper.ReadAsync(FILENAME);
            if (string.IsNullOrEmpty(json))
            {
                settings = new Settings();
                Save();
            }
            else
            {
                settings = JsonFileHelper.Convert<Settings>(json);
            }
        }

        public static void Save()
        {
            JsonFileHelper.SaveAsync(FILENAME, settings);
        }

        public static async Task SetTreeFolder(StorageFolder folder)
        {
            await settings.Tree.Init(folder);
            Save();
            MusicLibraryPage.AllSongs = settings.Tree.Flatten();
            MusicLibraryPage.Save();
            MainPage.CurrentMusicFolder = folder;
            MainPage.CurrentPlayList = MusicLibraryPage.AllSongs.ToList();
            MainPage.CurrentMusicIndex = -1;
        }
    }
}

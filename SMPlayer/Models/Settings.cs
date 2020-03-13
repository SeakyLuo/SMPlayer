﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;

namespace SMPlayer.Models
{
    [Serializable]
    public class Settings
    {
        public static Settings settings;
        private const string FILENAME = "SMPlayerSettings.json";
        public static List<LikeMusicListener> LikeMusicListeners = new List<LikeMusicListener>();

        public string RootPath { get; set; } = "";
        public FolderTree Tree { get; set; } = new FolderTree();
        public Music LastMusic { get; set; } = null;
        public PlayMode Mode { get; set; } = PlayMode.Once;
        public double Volume { get; set; } = 50.0d;
        public bool IsNavigationCollapsed { get; set; } = true;
        public Color ThemeColor { get; set; } = (Color)Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(Color), "#0078D7");
        public ShowToast Toast { get; set; } = ShowToast.Always;
        public string LastPage { get; set; } = "";
        public List<Playlist> Playlists { get; set; } = new List<Playlist>();
        public string LastPlaylist { get; set; } = "";
        public bool LocalMusicGridView { get; set; } = true;
        public bool LocalFolderGridView { get; set; } = true;
        public Playlist MyFavorites { get; set; } = new Playlist(MenuFlyoutHelper.MyFavorites);
        public ObservableCollection<string> RecentPlayed { get; set; } = new ObservableCollection<string>();
        public bool MiniModeWithDropdown { get; set; } = false;
        public bool IsMuted { get; set; } = false;
        public bool KeepLimitedRecentPlayedItems { get; set; } = true;
        public const int RecentPlayedLimit = 100;
        public List<string> RecentAdded { get; set; } = new List<string>();
        public bool AutoPlay { get; set; } = false;
        public bool SaveMusicProgress { get; set; } = false;
        public double MusicProgress { get; set; } = 0;
        public SortBy MusicLibraryCriterion { get; set; } = SortBy.Title;

        public int FindNextPlaylistNameIndex(string Name)
        {
            if (!string.IsNullOrEmpty(Name))
            {
                var siblings = Playlists.FindAll(p => p.Name.StartsWith(Name)).Select(p => p.Name).ToHashSet();
                for (int i = 1; i <= siblings.Count; i++)
                    if (!siblings.Contains($"{Name} {i}"))
                        return i;
            }
            return 0;
        }

        public string FindNextPlaylistName(string Name)
        {
            int index = FindNextPlaylistNameIndex(Name);
            return index == 0 ? Name : $"{Name} {index}";
        }

        public static async Task Init()
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
                if (string.IsNullOrEmpty(settings.RootPath)) return;
                try
                {
                    Helper.CurrentFolder = await StorageFolder.GetFolderFromPathAsync(settings.RootPath);
                }
                catch (Exception)
                {

                }
                MediaControl.AddMusicModifiedListener((before, after) =>
                {
                    settings.Tree.FindMusic(before).CopyFrom(after);
                });
                foreach (var item in await ApplicationData.Current.LocalFolder.GetItemsAsync())
                    if (item.Name.EndsWith(".TMP"))
                        await item.DeleteAsync();
            }
        }

        public static void Save()
        {
            settings.MusicProgress = MediaHelper.Position;
            JsonFileHelper.SaveAsync(FILENAME, settings);
        }

        public void LikeMusic(Music music)
        {
            if (MyFavorites.Contains(music)) return;
            music.Favorite = true;
            MyFavorites.Add(music);
            foreach (var listener in LikeMusicListeners) listener.MusicLiked(music, true);
        }

        public void LikeMusic(ICollection<Music> playlist)
        {
            var hashset = MyFavorites.Songs.ToHashSet();
            foreach (var music in playlist)
            {
                if (!hashset.Contains(music))
                {
                    music.Favorite = true;
                    MyFavorites.Add(music);
                    foreach (var listener in LikeMusicListeners) listener.MusicLiked(music, true);
                }
            }
        }

        public void DislikeMusic(Music music)
        {
            MyFavorites.Remove(music);
            music.Favorite = false;
            foreach (var listener in LikeMusicListeners) listener.MusicLiked(music, false);
        }

        public void AddMusic(Music music)
        {
            RecentAdded.AddOrMoveToTheFirst(music.Path);
        }

        public void RemoveMusic(Music music)
        {
            Tree.RemoveMusic(music);
            foreach (var playlist in Playlists)
                playlist.Songs.Remove(music);
            MyFavorites.Remove(music);
            RecentPlayed.Remove(music.Path);
            RecentAdded.Remove(music.Path);
        }

        public void Played(Music music)
        {
            if (music == null) return;
            RecentPlayed.AddOrMoveToTheFirst(music.Path);
            if (KeepLimitedRecentPlayedItems && RecentPlayed.Count > RecentPlayedLimit)
                RecentPlayed.RemoveAt(RecentPlayedLimit);
        }
        public const int PlaylistNameMaxLength = 50;
        public NamingError CheckPlaylistNamingError(string newName)
        {
            if (string.IsNullOrEmpty(newName) || string.IsNullOrWhiteSpace(newName))
                return NamingError.EmptyOrWhiteSpace;
            if (newName == MenuFlyoutHelper.NowPlaying || newName == MenuFlyoutHelper.MyFavorites ||
                Playlists.FindIndex((p) => p.Name == newName) != -1)
                return NamingError.Used;
            if (newName.Contains(Helper.StringConcatenationFlag) || newName.Contains("{0}"))
                return NamingError.Special;
            if (newName.Length > PlaylistNameMaxLength)
                return NamingError.TooLong;
            return NamingError.Good;
        }

        public async void RenamePlaylist(string oldName, string newName, RenameOption option, object data = null)
        {
            switch (option)
            {
                case RenameOption.New:
                    Playlist playlist = new Playlist(newName);
                    if (data != null) playlist.Add(data);
                    await playlist.SetDisplayItemAsync();
                    Playlists.Add(playlist);
                    PlaylistsPage.Playlists.Add(playlist);
                    break;
                case RenameOption.Rename:
                    if (oldName == newName) break;
                    int index = Playlists.FindIndex((p) => p.Name == oldName);
                    Playlists[index].Name = newName;
                    PlaylistsPage.Playlists[index].Name = newName;
                    break;
            }
        }

    }

    public interface LikeMusicListener
    {
        void MusicLiked(Music music, bool isFavorite);
    }

    public enum NamingError
    {
        Good = 0,
        EmptyOrWhiteSpace = 1,
        Used = 2,
        Special = 3,
        TooLong = 4
    }

    public static class NamingErrorConverter
    {
        public static string ToStr(this NamingError error)
        {
            switch (error)
            {
                case NamingError.EmptyOrWhiteSpace:
                    return "NamingErrorEmptyOrWhiteSpace";
                case NamingError.Used:
                    return "NamingErrorUsed";
                case NamingError.Special:
                    return "NamingErrorSpecial";
                case NamingError.TooLong:
                    return "NamingErrorTooLong";
                default:
                    return "";
            }
        }
    }

    public enum RenameOption
    {
        New = 0,
        Rename = 1
    }
}

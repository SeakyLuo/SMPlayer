using SMPlayer.Models;
using SMPlayer.Models.DAO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Helpers
{
    public static class DAOHelper
    {
        public static Music FromDAO(this MusicDAO src)
        {
            return src == null ? null : new Music()
            {
                Id = src.Id,
                Path = src.Path,
                Name = src.Name,
                Artist = src.Artist,
                Album = src.Album,
                Duration = src.Duration,
                PlayCount = src.PlayCount,
                DateAdded = src.DateAdded
            };
        }

        public static MusicDAO ToDAO(this Music src)
        {
            return new MusicDAO()
            {
                Id = src.Id,
                Path = src.Path,
                Name = src.Name,
                Artist = src.Artist,
                Album = src.Album,
                Duration = src.Duration,
                PlayCount = src.PlayCount,
                DateAdded = src.DateAdded
            };
        }

        public static Playlist FromDAO(this PlaylistDAO src)
        {
            return new Playlist()
            {
                Id = src.Id,
                Name = src.Name,
                Criterion = src.Criterion
            };
        }

        public static PlaylistDAO ToDAO(this Playlist src)
        {
            return new PlaylistDAO()
            {
                Id = src.Id,
                Name = src.Name,
                Criterion = src.Criterion,
                Songs = src.Songs.Select(m => m.ToPlaylistItemDAO(src)).ToList()
            };
        }

        public static PlaylistItemDAO ToPlaylistItemDAO(this Music src, Playlist playlist)
        {
            return new PlaylistItemDAO { PlaylistId = playlist.Id, ItemId = src.Id };
        }

        public static PreferenceItem FromDAO(this PreferenceItemDAO src)
        {
            return new PreferenceItem()
            {
                ItemId = src.Id,
                Id = src.ItemId,
                Name = src.ItemName,
                IsEnabled = src.IsEnabled,
                Level = src.Level
            };
        }

        public static PreferenceItemDAO ToDAO(this PreferenceItem src, PreferType preferType)
        {
            return new PreferenceItemDAO()
            {
                Id = src.ItemId,
                ItemId = src.Id,
                ItemName = src.Name,
                IsEnabled = src.IsEnabled,
                Level = src.Level,
                Type = preferType,

            };
        }

        public static FolderFile FromDAO(this FileDAO src)
        {
            return src == null ? null : new FolderFile()
            {
                Id = src.Id,
                FileId = src.FileId,
                FileType = src.FileType,
                Path = src.Path,
                ParentId = src.ParentId,
            };
        }

        public static FileDAO ToDAO(this FolderFile src)
        {
            return new FileDAO()
            {
                Id = src.Id,
                FileId = src.FileId,
                FileType = src.FileType,
                Path = src.Path,
                ParentId = src.ParentId,
            };
        }

        public static FolderTree FromDAO(this FolderDAO src)
        {
            return new FolderTree()
            {
                Id = src.Id,
                Path = src.Path,
                Criterion = src.Criterion
            };
        }

        public static FolderDAO ToDAO(this FolderTree src)
        {
            return new FolderDAO()
            {
                Id = src.Id,
                Path = src.Path,
                Criterion = src.Criterion,
                Folders = src.Trees.Select(i => i.ToDAO()).ToList(),
                Files = src.Files.Select(i => i.ToDAO()).ToList()
            };
        }

        public static PreferenceSettings FromDAO(this PreferenceSettingsDAO src)
        {
            return new PreferenceSettings()
            {
                Songs = src.Songs,
                Artists = src.Artists,
                Albums = src.Albums,
                Playlists = src.Playlists,
                Folders = src.Folders,
            };
        }

        public static PreferenceSettingsDAO ToDAO(this PreferenceSettings src)
        {
            return new PreferenceSettingsDAO()
            {
                Songs = src.Songs,
                Artists = src.Artists,
                Albums = src.Albums,
                Playlists = src.Playlists,
                Folders = src.Folders,
            };
        }

        public static Settings FromDAO(this SettingsDAO src)
        {
            return new Settings()
            {
                RootPath = src.RootPath,
                LastMusicIndex = src.LastMusicIndex,
                Mode = src.Mode,
                Volume = src.Volume,
                IsNavigationCollapsed = src.IsNavigationCollapsed,
                ThemeColor = src.ThemeColor,
                NotificationSend = src.NotificationSend,
                NotificationDisplay = src.NotificationDisplay,
                LastPage = src.LastPage,
                LastPlaylistId = src.LastPlaylist,
                LocalFolderGridView = src.LocalFolderGridView,
                LocalMusicGridView = src.LocalMusicGridView,
                RecentPlayedSongs = src.RecentPlayed,
                LimitedRecentPlayedItems = src.LimitedRecentPlayedItems,
                MiniModeWithDropdown = src.MiniModeWithDropdown,
                IsMuted = src.IsMuted,
                AutoPlay = src.AutoPlay,
                AutoLyrics = src.AutoLyrics,
                SaveMusicProgress = src.SaveMusicProgress,
                MusicProgress = src.MusicProgress,
                MusicLibraryCriterion = src.MusicLibraryCriterion,
                AlbumsCriterion = src.AlbumsCriterion,
                HideMultiSelectCommandBarAfterOperation = src.HideMultiSelectCommandBarAfterOperation,
                ShowCount = src.ShowCount,
                ShowLyricsInNotification = src.ShowLyricsInNotification,
                RecentSearches = new ObservableCollection<string>(src.RecentSearches),
                VoiceAssistantPreferredLanguage = src.VoiceAssistantPreferredLanguage,
                SearchSongsCriterion = src.SearchSongsCriterion,
                SearchArtistsCriterion = src.SearchArtistsCriterion,
                SearchAlbumsCriterion = src.SearchAlbumsCriterion,
                SearchPlaylistsCriterion = src.SearchPlaylistsCriterion,
                SearchFoldersCriterion = src.SearchFoldersCriterion,
                Preference = src.Preference.FromDAO(),
                RecentAdded = src.RecentAdded,
            };
        }

        public static SettingsDAO ToDAO(this Settings src)
        {
            return new SettingsDAO()
            {
                RootPath = src.RootPath,
                LastMusicIndex = src.LastMusicIndex,
                Mode = src.Mode,
                Volume = src.Volume,
                IsNavigationCollapsed = src.IsNavigationCollapsed,
                ThemeColor = src.ThemeColor,
                NotificationSend = src.NotificationSend,
                NotificationDisplay = src.NotificationDisplay,
                LastPage = src.LastPage,
                LastPlaylist = src.LastPlaylistId,
                LocalFolderGridView = src.LocalFolderGridView,
                LocalMusicGridView = src.LocalMusicGridView,
                RecentPlayed = src.RecentPlayedSongs,
                LimitedRecentPlayedItems = src.LimitedRecentPlayedItems,
                MiniModeWithDropdown = src.MiniModeWithDropdown,
                IsMuted = src.IsMuted,
                AutoPlay = src.AutoPlay,
                AutoLyrics = src.AutoLyrics,
                SaveMusicProgress = src.SaveMusicProgress,
                MusicProgress = src.MusicProgress,
                MusicLibraryCriterion = src.MusicLibraryCriterion,
                AlbumsCriterion = src.AlbumsCriterion,
                HideMultiSelectCommandBarAfterOperation = src.HideMultiSelectCommandBarAfterOperation,
                ShowCount = src.ShowCount,
                ShowLyricsInNotification = src.ShowLyricsInNotification,
                RecentSearches = src.RecentSearches.ToList(),
                VoiceAssistantPreferredLanguage = src.VoiceAssistantPreferredLanguage,
                SearchSongsCriterion = src.SearchSongsCriterion,
                SearchArtistsCriterion = src.SearchArtistsCriterion,
                SearchAlbumsCriterion = src.SearchAlbumsCriterion,
                SearchPlaylistsCriterion = src.SearchPlaylistsCriterion,
                SearchFoldersCriterion = src.SearchFoldersCriterion,
                Preference = src.Preference.ToDAO(),
                RecentAdded = src.RecentAdded,
            };
        }
    }
}

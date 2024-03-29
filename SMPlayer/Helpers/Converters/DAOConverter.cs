﻿using SMPlayer.Models;
using SMPlayer.Models.DAO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace SMPlayer.Helpers
{
    public static class DAOConverter
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
                DateAdded = src.DateAdded,
                State = src.State,
            };
        }

        public static MusicDAO ToDAO(this MusicView src)
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
                DateAdded = src.DateAdded,
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
                DateAdded = src.DateAdded,
                State = src.State,
            };
        }

        public static Playlist FromDAO(this PlaylistDAO src)
        {
            return new Playlist()
            {
                Id = src.Id,
                Name = src.Name,
                Criterion = src.Criterion,
                Priority = src.Priority
            };
        }

        public static PlaylistDAO ToDAO(this Playlist src)
        {
            return new PlaylistDAO()
            {
                Id = src.Id,
                Name = src.Name,
                Criterion = src.Criterion,
                Songs = src.Songs.Select(m => m.ToPlaylistItemDAO(src.Id)).ToList(),
                Priority = src.Priority
            };
        }

        public static PlaylistItemDAO ToPlaylistItemDAO(this Music src, long playlistId)
        {
            return new PlaylistItemDAO { PlaylistId = playlistId, ItemId = src.Id };
        }

        public static PreferenceItem FromDAO(this PreferenceItemDAO src)
        {
            return new PreferenceItem
            {
                ThisId = src.Id,
                Id = src.ItemId,
                Name = src.ItemName,
                IsEnabled = src.IsEnabled,
                Level = src.Level,
                Type = src.Type,
            };
        }

        public static PreferenceItemDAO ToDAO(this PreferenceItem src, EntityType preferType)
        {
            return new PreferenceItemDAO()
            {
                Id = src.ThisId,
                ItemId = src.Id,
                ItemName = src.Name,
                IsEnabled = src.IsEnabled,
                Level = src.Level,
                Type = preferType,
            };
        }

        public static PreferenceItemDAO ToDAO(this PreferenceItem src)
        {
            return ToDAO(src, src.Type);
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
                State = src.State,
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
                State = src.State,
            };
        }

        public static FolderTree FromDAO(this FolderDAO src)
        {
            return new FolderTree()
            {
                Id = src.Id,
                Path = src.Path,
                ParentId = src.ParentId,
                Criterion = src.Criterion,
                State = src.State,
            };
        }

        public static FolderDAO ToDAO(this FolderTree src)
        {
            return new FolderDAO()
            {
                Id = src.Id,
                Path = src.Path,
                ParentId = src.ParentId,
                Criterion = src.Criterion,
                Folders = src.Trees.Select(i => i.ToDAO()).ToList(),
                Files = src.Files.Select(i => i.ToDAO()).ToList(),
                State = src.State,
            };
        }

        public static PreferenceSettings FromDAO(this PreferenceSettingsDAO src)
        {
            return new PreferenceSettings()
            {
                Id = src.Id,
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
                Id = src.Id,
                Songs = src.Songs,
                Artists = src.Artists,
                Albums = src.Albums,
                Playlists = src.Playlists,
                Folders = src.Folders,
                RecentAddedId = src.RecentAdded.ThisId,
                MyFavoritesId = src.MyFavorites.ThisId,
                MostPlayedId = src.MostPlayed.ThisId,
                LeastPlayedId = src.LeastPlayed.ThisId,
            };
        }

        public static Settings FromDAO(this SettingsDAO src)
        {
            return new Settings()
            {
                Id = src.Id,
                RootPath = src.RootPath,
                LastMusicIndex = src.LastMusicIndex,
                Mode = src.Mode,
                Volume = src.Volume,
                IsNavigationCollapsed = src.IsNavigationCollapsed,
                ThemeColor = (Color)Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(Color), src.ThemeColor),
                NotificationSend = src.NotificationSend,
                NotificationDisplay = src.NotificationDisplay,
                LastPage = src.LastPage,
                LastPlaylistId = src.LastPlaylist,
                LocalViewMode = src.LocalViewMode,
                MyFavoritesId = src.MyFavorites,
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
                VoiceAssistantPreferredLanguage = src.VoiceAssistantPreferredLanguage,
                SearchSongsCriterion = src.SearchSongsCriterion,
                SearchArtistsCriterion = src.SearchArtistsCriterion,
                SearchAlbumsCriterion = src.SearchAlbumsCriterion,
                SearchPlaylistsCriterion = src.SearchPlaylistsCriterion,
                SearchFoldersCriterion = src.SearchFoldersCriterion,
                LastReleaseNotesVersion = src.LastReleaseNotesVersion,
                RemotePlayPassword = src.RemotePlayPassword,
                UseFilenameNotMusicName = src.UseFilenameNotMusicName,
                NotificationLyricsSource = src.NotificationLyricsSource,
                SaveLyricsImmediately = src.SaveLyricsImmediately,
            };
        }

        public static SettingsDAO ToDAO(this Settings src)
        {
            return new SettingsDAO()
            {
                Id = src.Id,
                RootPath = src.RootPath,
                LastMusicIndex = src.LastMusicIndex,
                Mode = src.Mode,
                Volume = src.Volume,
                IsNavigationCollapsed = src.IsNavigationCollapsed,
                ThemeColor = src.ThemeColor.ToString(),
                NotificationSend = src.NotificationSend,
                NotificationDisplay = src.NotificationDisplay,
                LastPage = src.LastPage,
                LastPlaylist = src.LastPlaylistId,
                LocalViewMode = src.LocalViewMode,
                MyFavorites = src.MyFavoritesId,
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
                VoiceAssistantPreferredLanguage = src.VoiceAssistantPreferredLanguage,
                SearchSongsCriterion = src.SearchSongsCriterion,
                SearchArtistsCriterion = src.SearchArtistsCriterion,
                SearchAlbumsCriterion = src.SearchAlbumsCriterion,
                SearchPlaylistsCriterion = src.SearchPlaylistsCriterion,
                SearchFoldersCriterion = src.SearchFoldersCriterion,
                LastReleaseNotesVersion = src.LastReleaseNotesVersion,
                RemotePlayPassword = src.RemotePlayPassword,
                UseFilenameNotMusicName = src.UseFilenameNotMusicName,
                NotificationLyricsSource = src.NotificationLyricsSource,
                SaveLyricsImmediately = src.SaveLyricsImmediately,
            };
        }

        public static AuthorizedDevice FromDao(this AuthorizedDeviceDAO src)
        {
            if (src == null) return null;
            return new AuthorizedDevice
            {
                Id = src.Id,
                Ip = src.Ip,
                DeviceName = src.DeviceName,
                State = src.State,
                Auth = src.Auth,
                CreateTime = src.CreateTime,
                UpdateTime = src.UpdateTime,
            };
        }
        public static AuthorizedDeviceDAO ToDao(this AuthorizedDevice src)
        {
            if (src == null) return null;
            return new AuthorizedDeviceDAO
            {
                Id = src.Id,
                Ip = src.Ip,
                DeviceName = src.DeviceName,
                State = src.State,
                Auth = src.Auth,
                CreateTime = src.CreateTime,
                UpdateTime = src.UpdateTime,
            };
        }
    }
}

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
            return new Music()
            {
                Id = src.Id,
                Path = src.Path,
                Name = src.Name,
                Album = src.Album,
                Duration = src.Duration,
                Favorite = src.Favorite,
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
                Album = src.Album,
                Duration = src.Duration,
                Favorite = src.Favorite,
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
                SongIds = src.Songs
            };
        }

        public static PlaylistDAO ToDAO(this Playlist src)
        {
            return new PlaylistDAO()
            {
                Id = src.Id,
                Name = src.Name,
                Songs = src.SongIds
            };
        }

        public static PreferenceItem FromDAO(this PreferenceItemDAO src)
        {
            return new PreferenceItem()
            {
                ItemId = src.ItemId,
                Name = src.ItemName,
                IsEnabled = src.IsEnabled,
                Level = src.Level
            };
        }

        public static PreferenceItemDAO ToDAO(this PreferenceItem src)
        {
            return new PreferenceItemDAO()
            {
                ItemId = src.ItemId,
                ItemName = src.Name,
                IsEnabled = src.IsEnabled,
                Level = src.Level
            };
        }

        public static Music FromDAO(this FileDAO src)
        {
            return new Music()
            {
                Id = src.Id,
                Path = src.Path,
            };
        }

        public static FileDAO ToFileDAO(this Music src)
        {
            return new FileDAO()
            {
                Id = src.Id,
                Type = FileType.Music,
                Path = src.Path,
            };
        }

        public static FolderTree FromDAO(this FolderTreeDAO src)
        {
            return new FolderTree()
            {
                Id = src.Id,
                Trees = src.Trees.Select(i => i.FromDAO()).ToList(),
                Files = src.Files.Select(i => i.FromDAO()).ToList(),
                Path = src.Path,
                Criterion = src.Criterion
            };
        }

        public static FolderTreeDAO ToDAO(this FolderTree src)
        {
            return new FolderTreeDAO()
            {
                Id = src.Id,
                Trees = src.Trees.Select(i => i.ToDAO()).ToList(),
                Files = src.Files.Select(i => i.ToFileDAO()).ToList(),
                Path = src.Path,
                Criterion = src.Criterion
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
                PreferredSongs = src.PreferredSongs.Select(i => i.FromDAO()).ToList(),
                PreferredArtists = src.PreferredArtists.Select(i => i.FromDAO()).ToList(),
                PreferredAlbums = src.PreferredAlbums.Select(i => i.FromDAO()).ToList(),
                PreferredPlaylists = src.PreferredPlaylists.Select(i => i.FromDAO()).ToList(),
                PreferredFolders = src.PreferredFolders.Select(i => i.FromDAO()).ToList(),
                RecentAdded = src.RecentAdded.FromDAO(),
                MyFavorites = src.MyFavorites.FromDAO(),
                MostPlayed = src.MostPlayed.FromDAO(),
                LeastPlayed = src.LeastPlayed.FromDAO()
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
                PreferredSongs = src.PreferredSongs.Select(i => i.ToDAO()).ToList(),
                PreferredArtists = src.PreferredArtists.Select(i => i.ToDAO()).ToList(),
                PreferredAlbums = src.PreferredAlbums.Select(i => i.ToDAO()).ToList(),
                PreferredPlaylists = src.PreferredPlaylists.Select(i => i.ToDAO()).ToList(),
                PreferredFolders = src.PreferredFolders.Select(i => i.ToDAO()).ToList(),
                RecentAdded = src.RecentAdded.ToDAO(),
                MyFavorites = src.MyFavorites.ToDAO(),
                MostPlayed = src.MostPlayed.ToDAO(),
                LeastPlayed = src.LeastPlayed.ToDAO()
            };
        }

        public static Settings FromDAO(this SettingsDAO src)
        {
            return new Settings()
            {
                MusicLibrary = src.MusicLibrary,
                RootPath = src.RootPath,
                Tree = src.Tree.FromDAO(),
                LastMusicIndex = src.LastMusicIndex,
                Mode = src.Mode,
                Volume = src.Volume,
                IsNavigationCollapsed = src.IsNavigationCollapsed,
                ThemeColor = src.ThemeColor,
                NotificationSend = src.NotificationSend,
                NotificationDisplay = src.NotificationDisplay,
                LastPage = src.LastPage,
                Playlists = src.Playlists.Select(p => p.FromDAO()).ToList(),
                LastPlaylistId = src.LastPlaylist,
                LocalFolderGridView = src.LocalFolderGridView,
                LocalMusicGridView = src.LocalMusicGridView,
                MyFavorites = src.MyFavorites.FromDAO(),
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
                IdGenerator = new IdGenerator(src.IdMap)
            };
        }

        public static SettingsDAO ToDAO(this Settings src)
        {
            return new SettingsDAO()
            {
                RootPath = src.RootPath,
                Tree = src.Tree.ToDAO(),
                LastMusicIndex = src.LastMusicIndex,
                Mode = src.Mode,
                Volume = src.Volume,
                IsNavigationCollapsed = src.IsNavigationCollapsed,
                ThemeColor = src.ThemeColor,
                NotificationSend = src.NotificationSend,
                NotificationDisplay = src.NotificationDisplay,
                LastPage = src.LastPage,
                Playlists = src.Playlists.Select(p => p.ToDAO()).ToList(),
                LastPlaylist = src.LastPlaylistId,
                LocalFolderGridView = src.LocalFolderGridView,
                LocalMusicGridView = src.LocalMusicGridView,
                MyFavorites = src.MyFavorites.ToDAO(),
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
                IdMap = src.IdGenerator.IdMap
            };
        }
    }
}

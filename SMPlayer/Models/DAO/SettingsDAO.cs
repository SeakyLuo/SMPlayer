using SMPlayer.Helpers;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace SMPlayer.Models.DAO
{
    [Table("Settings")]
    public class SettingsDAO
    {
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }
        public string RootPath { get; set; }
        public int LastMusicIndex { get; set; }
        public PlayMode Mode { get; set; }
        public double Volume { get; set; }
        public bool IsNavigationCollapsed { get; set; }
        public string ThemeColor { get; set; }
        public NotificationSendMode NotificationSend { get; set; }
        public NotificationDisplayMode NotificationDisplay { get; set; }
        public string LastPage { get; set; }
        public long LastPlaylist { get; set; }
        public LocalPageViewMode LocalViewMode { get; set; }
        public long MyFavorites { get; set; }
        public long NowPlaying { get; set; }
        public bool MiniModeWithDropdown { get; set; }
        public bool IsMuted { get; set; }
        public bool AutoPlay { get; set; }
        public bool AutoLyrics { get; set; }
        public bool SaveMusicProgress { get; set; }
        public double MusicProgress { get; set; }
        public SortBy MusicLibraryCriterion { get; set; }
        public SortBy AlbumsCriterion { get; set; }
        public bool HideMultiSelectCommandBarAfterOperation { get; set; }
        public bool ShowCount { get; set; }
        public bool ShowLyricsInNotification { get; set; }
        public SupportedLanguage VoiceAssistantPreferredLanguage { get; set; }
        public SortBy SearchArtistsCriterion { get; set; }
        public SortBy SearchAlbumsCriterion { get; set; }
        public SortBy SearchSongsCriterion { get; set; }
        public SortBy SearchPlaylistsCriterion { get; set; }
        public SortBy SearchFoldersCriterion { get; set; }
        public string LastReleaseNotesVersion { get; set; }
        public string RemotePlayPassword { get; set; }
        public bool UseFilenameNotMusicName { get; set; }
        public LyricsSource NotificationLyricsSource { get; set; } = LyricsSource.Internet;
        public bool SaveLyricsImmediately { get; set; } = false;

    }
}
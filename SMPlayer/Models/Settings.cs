using SMPlayer.Helpers;
using SMPlayer.Models.DAO;
using SMPlayer.Models.Enums;
using SMPlayer.Services;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
        public long Id { get; set; }
        public string RootPath { get; set; } = "";
        public FolderTree Tree { get; set; } = new FolderTree();
        public int LastMusicIndex { get; set; } = -1;
        public PlayMode Mode { get; set; } = PlayMode.Once;
        public double Volume { get; set; } = 50.0d;
        public bool IsNavigationCollapsed { get; set; } = true;
        public Color ThemeColor { get; set; } = (Color)Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(Color), "#0078D7");
        public NotificationSendMode NotificationSend { get; set; } = NotificationSendMode.MusicChanged;
        public NotificationDisplayMode NotificationDisplay { get; set; } = NotificationDisplayMode.Normal;
        public string LastPage { get; set; } = "";
        public List<PlaylistView> Playlists { get; set; } = new List<PlaylistView>();
        public long LastPlaylistId { get; set; } = 0;
        public LocalPageViewMode LocalViewMode { get; set; } = LocalPageViewMode.Grid;
        public PlaylistView MyFavorites { get; set; } = new PlaylistView(Constants.MyFavorites);
        public long MyFavoritesId { get; set; }
        public ObservableCollection<string> RecentPlayed { get; set; } = new ObservableCollection<string>();
        public bool MiniModeWithDropdown { get; set; } = false;
        public bool IsMuted { get; set; } = false;
        public int LimitedRecentPlayedItems { get; set; } = -1;
        public bool AutoPlay { get; set; } = false;
        public bool AutoLyrics { get; set; } = false;
        public bool SaveMusicProgress { get; set; } = false;
        public double MusicProgress { get; set; } = 0;
        public SortBy MusicLibraryCriterion { get; set; } = SortBy.Title;
        public SortBy AlbumsCriterion { get; set; } = SortBy.Default;
        public bool HideMultiSelectCommandBarAfterOperation { get; set; } = true;
        public bool ShowCount { get; set; } = true;
        public bool ShowLyricsInNotification { get; set; } = false;
        public ObservableCollection<string> RecentSearches { get; set; } = new ObservableCollection<string>();
        public SupportedLanguage VoiceAssistantPreferredLanguage { get; set; } = VoiceAssistantHelper.ConvertLanguage(Helper.CurrentLanguage);

        public SortBy SearchArtistsCriterion { get; set; } = SortBy.Default;
        public SortBy SearchAlbumsCriterion { get; set; } = SortBy.Default;
        public SortBy SearchSongsCriterion { get; set; } = SortBy.Default;
        public SortBy SearchPlaylistsCriterion { get; set; } = SortBy.Default;
        public SortBy SearchFoldersCriterion { get; set; } = SortBy.Default;
        public PreferenceSettings Preference { get; set; } = new PreferenceSettings();
        public string LastReleaseNotesVersion { get; set; }
        public string RemotePlayPassword { get; set; }
        public bool UseFilenameNotMusicName { get; set; } = false;
        public LyricsSource NotificationLyricsSource { get; set; } = LyricsSource.Internet;
        public bool SaveLyricsImmediately { get; set; } = false;
        public Settings() { }
    }

    public enum NamingError
    {
        Good = 0,
        EmptyOrWhiteSpace = 1,
        Used = 2,
        Special = 3,
        TooLong = 4
    }

    public enum RenameOption
    {
        Create,
        Rename,
    }

    public enum RenameTarget
    {
        Playlist,
        Folder,
    }
}

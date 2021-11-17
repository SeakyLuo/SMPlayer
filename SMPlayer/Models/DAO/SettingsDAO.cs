using SMPlayer.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace SMPlayer.Models.DAO
{
    public class SettingsDAO
    {
        public Dictionary<long, Music> MusicLibrary { get; set; } = new Dictionary<long, Music>();
        public string RootPath { get; set; } = "";
        public FolderTreeDAO Tree { get; set; } = new FolderTreeDAO();
        public int LastMusicIndex { get; set; } = -1;
        public PlayMode Mode { get; set; } = PlayMode.Once;
        public double Volume { get; set; } = 50.0d;
        public bool IsNavigationCollapsed { get; set; } = true;
        public Color ThemeColor { get; set; } = (Color)Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(Color), "#0078D7");
        public NotificationSendMode NotificationSend { get; set; } = NotificationSendMode.MusicChanged;
        public NotificationDisplayMode NotificationDisplay { get; set; } = NotificationDisplayMode.Normal;
        public string LastPage { get; set; } = "";
        public List<PlaylistDAO> Playlists { get; set; } = new List<PlaylistDAO>();
        public long LastPlaylist { get; set; }
        public bool LocalMusicGridView { get; set; } = true;
        public bool LocalFolderGridView { get; set; } = true;
        public PlaylistDAO MyFavorites { get; set; } = new PlaylistDAO() { Name = MenuFlyoutHelper.MyFavorites };
        public List<int> RecentPlayed { get; set; } = new List<int>();
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
        public List<string> RecentSearches = new List<string>();
        public VoiceAssistantLanguage VoiceAssistantPreferredLanguage = VoiceAssistantHelper.ConvertLanguage(Helper.CurrentLanguage);

        public SortBy SearchArtistsCriterion { get; set; } = SortBy.Default;
        public SortBy SearchAlbumsCriterion { get; set; } = SortBy.Default;
        public SortBy SearchSongsCriterion { get; set; } = SortBy.Default;
        public SortBy SearchPlaylistsCriterion { get; set; } = SortBy.Default;
        public SortBy SearchFoldersCriterion { get; set; } = SortBy.Default;

        public PreferenceSettingsDAO Preference { get; set; } = new PreferenceSettingsDAO();

        public Dictionary<IdType, int> IdMap = new Dictionary<IdType, int>();

        public SettingsDAO()
        {
            foreach (var idType in Enum.GetValues(typeof(IdType)))
            {
                IdMap.Add((IdType)idType, 1);
            }
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models
{
    [Serializable]
    public class PreferenceSetting
    {
        public const int MaxPreferredSongs = 10,
                         MaxPreferredArtists = 10,
                         MaxPreferredAlbums = 10,
                         MaxPreferredPlaylists = 10,
                         MaxPreferredFolders = 5;

        public List<PreferenceItem> PreferredSongs { get; set; } = new List<PreferenceItem>();
        public List<PreferenceItem> PreferredArtists { get; set; } = new List<PreferenceItem>();
        public List<PreferenceItem> PreferredAlbums { get; set; } = new List<PreferenceItem>();
        public List<PreferenceItem> PreferredPlaylists { get; set; } = new List<PreferenceItem>();
        public List<PreferenceItem> PreferredFolders { get; set; } = new List<PreferenceItem>();
        public bool Songs { get; set; } = false;
        public bool Artists { get; set; } = false;
        public bool Albums { get; set; } = false;
        public bool Playlists { get; set; } = false;
        public bool Folders { get; set; } = false;
        public bool RecentAdded { get; set; } = false;
        public bool MyFavorites { get; set; } = false;
        public bool MostPlayed { get; set; } = false;
        public bool LeastPlayed { get; set; } = false;

        public bool Prefer(IPreferable preferable)
        {
            PreferType preferType = preferable.GetPreferType();
            List<PreferenceItem> items = GetPreferenceItems(preferType);
            if (items.Count >= GetMaxPreferenceItems(preferType))
            {
                return false;
            }
            items.Add(preferable.AsPreferenceItem());
            return true;
        }

        public void UndoPrefer(IPreferable preferable)
        {
            string preferId = preferable.AsPreferenceItem().Id;
            GetPreferenceItems(preferable.GetPreferType()).RemoveAll(i => i.Id.Equals(preferId));
        }

        public bool IsPreferred(IPreferable preferable)
        {
            List<PreferenceItem> items = GetPreferenceItems(preferable.GetPreferType());
            string preferId = preferable.AsPreferenceItem().Id;
            return items.Any(i => i.Id.Equals(preferId));
        }

        private List<PreferenceItem> GetPreferenceItems(PreferType type)
        {
            switch (type)
            {
                case PreferType.Song:
                    return PreferredSongs;
                case PreferType.Artist:
                    return PreferredArtists;
                case PreferType.Album:
                    return PreferredAlbums;
                case PreferType.Playlist:
                    return PreferredPlaylists;
                case PreferType.Folder:
                    return PreferredFolders;
                default:
                    return new List<PreferenceItem>();
            }
        }

        private int GetMaxPreferenceItems(PreferType type)
        {
            switch (type)
            {
                case PreferType.Song:
                    return MaxPreferredSongs;
                case PreferType.Artist:
                    return MaxPreferredArtists;
                case PreferType.Album:
                    return MaxPreferredAlbums;
                case PreferType.Playlist:
                    return MaxPreferredPlaylists;
                case PreferType.Folder:
                    return MaxPreferredFolders;
                default:
                    return 0;
            }
        }
    }
}

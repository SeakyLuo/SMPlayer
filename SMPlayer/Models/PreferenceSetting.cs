using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models
{
    [Serializable]
    public class PreferenceSetting
    {
        public const int MaxPreferredSongs = 30,
                         MaxPreferredArtists = 15,
                         MaxPreferredAlbums = 20,
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

        public void UpdatePlaylistName(string oldName, string newName)
        {
            if (PreferredPlaylists.Find(i => i.Name == oldName) is PreferenceItem item)
            {
                item.Name = newName;
            }
        }

        public bool Prefer(IPreferable preferable)
        {
            PreferType preferType = preferable.AsPreferenceItemView().PreferType;
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
            PreferType preferType = preferable.AsPreferenceItemView().PreferType;
            List<PreferenceItem> items = GetPreferenceItems(preferType);
            int index = items.FindIndex(i => i.Id.Equals(preferId));
            if (index != -1)
            {
                items.RemoveAt(index);
            }
        }

        public bool IsPreferred(IPreferable preferable)
        {
            PreferType preferType = preferable.AsPreferenceItemView().PreferType;
            List<PreferenceItem> items = GetPreferenceItems(preferType);
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

        public static int GetMaxPreferenceItems(PreferType type)
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

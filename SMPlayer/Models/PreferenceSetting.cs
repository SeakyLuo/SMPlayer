using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models
{
    [Serializable]
    public class PreferenceSettings
    {
        public const int MaxPreferredSongs = 30,
                         MaxPreferredArtists = 20,
                         MaxPreferredAlbums = 25,
                         MaxPreferredPlaylists = 15,
                         MaxPreferredFolders = 10;

        public List<PreferenceItem> PreferredSongs { get; set; } = new List<PreferenceItem>();
        public List<PreferenceItem> PreferredArtists { get; set; } = new List<PreferenceItem>();
        public List<PreferenceItem> PreferredAlbums { get; set; } = new List<PreferenceItem>();
        public List<PreferenceItem> PreferredPlaylists { get; set; } = new List<PreferenceItem>();
        public List<PreferenceItem> PreferredFolders { get; set; } = new List<PreferenceItem>();
        public bool Songs { get; set; } = true;
        public bool Artists { get; set; } = true;
        public bool Albums { get; set; } = true;
        public bool Playlists { get; set; } = true;
        public bool Folders { get; set; } = true;
        public PreferenceItem RecentAdded { get; set; } = new PreferenceItem();
        public PreferenceItem MyFavorites { get; set; } = new PreferenceItem();
        public PreferenceItem MostPlayed { get; set; } = new PreferenceItem();
        public PreferenceItem LeastPlayed { get; set; } = new PreferenceItem();

        [Newtonsoft.Json.JsonIgnore]
        public List<PreferenceItem> EnabledPreferredSongs
        {
            get => PreferredSongs.Where(i => i.IsEnabled).ToList();
        }

        [Newtonsoft.Json.JsonIgnore]
        public List<PreferenceItem> EnabledPreferredArtists
        {
            get => PreferredArtists.Where(i => i.IsEnabled).ToList();
        }

        [Newtonsoft.Json.JsonIgnore]
        public List<PreferenceItem> EnabledPreferredAlbums
        {
            get => PreferredAlbums.Where(i => i.IsEnabled).ToList();
        }

        [Newtonsoft.Json.JsonIgnore]
        public List<PreferenceItem> EnabledPreferredPlaylists
        {
            get => PreferredPlaylists.Where(i => i.IsEnabled).ToList();
        }

        [Newtonsoft.Json.JsonIgnore]
        public List<PreferenceItem> EnabledPreferredFolders
        {
            get => PreferredFolders.Where(i => i.IsEnabled).ToList();
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

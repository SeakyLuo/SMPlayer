using SMPlayer.Helpers;
using SMPlayer.Models.DAO;
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
        public static PreferenceSettings settings = new PreferenceSettings();

        public const int MaxPreferredSongs = 100,
                         MaxPreferredArtists = 50,
                         MaxPreferredAlbums = 50,
                         MaxPreferredPlaylists = 30,
                         MaxPreferredFolders = 30;

        public static PreferenceItem GetPreferenceItem(IPreferable preferable)
        {
            PreferenceItem item = preferable.AsPreferenceItem();
            if (item.Type == EntityType.MyFavorites) return FindMyFavorites;
            return SQLHelper.Run(c => c.SelectPreferenceItem(item.Type, item.Id)?.FromDAO());
        }

        public long Id { get; set; }
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

        public static List<PreferenceItem> FindPreferredSongs => FindPreferredItems(EntityType.Song);
        public static List<PreferenceItem> FindPreferredArtists => FindPreferredItems(EntityType.Artist);
        public static List<PreferenceItem> FindPreferredAlbums => FindPreferredItems(EntityType.Album);
        public static List<PreferenceItem> FindPreferredPlaylists => FindPreferredItems(EntityType.Playlist);
        public static List<PreferenceItem> FindPreferredFolders => FindPreferredItems(EntityType.Folder);
        public static PreferenceItem FindRecentAdded => FindPreferredItems(EntityType.RecentAdded).First();
        public static PreferenceItem FindMyFavorites => FindPreferredItems(EntityType.MyFavorites).First();
        public static PreferenceItem FindMostPlayed => FindPreferredItems(EntityType.MostPlayed).First();
        public static PreferenceItem FindLeastPlayed => FindPreferredItems(EntityType.LeastPlayed).First();
        public static List<PreferenceItem> EnabledPreferredSongs => FindEnabledPreferredItems(EntityType.Song);
        public static List<PreferenceItem> EnabledPreferredArtists => FindEnabledPreferredItems(EntityType.Artist);
        public static List<PreferenceItem> EnabledPreferredAlbums => FindEnabledPreferredItems(EntityType.Album);
        public static List<PreferenceItem> EnabledPreferredPlaylists => FindEnabledPreferredItems(EntityType.Playlist);
        public static List<PreferenceItem> EnabledPreferredFolders => FindEnabledPreferredItems(EntityType.Folder);
        private static List<PreferenceItem> FindPreferredItems(EntityType preferType)
        {
            return SQLHelper.Run(c => c.SelectPreferenceItems(preferType));
        }
        private static List<PreferenceItem> FindEnabledPreferredItems(EntityType preferType)
        {
            return SQLHelper.Run(c => c.SelectPreferenceItems(preferType)).Where(i => i.IsEnabled).ToList();
        }
        public static List<PreferenceItem> FindDislikedItems() => FindEnabledByLevel(PreferLevel.Dislike);
        public static List<PreferenceItem> GetDoNotAppearItems() => FindEnabledByLevel(PreferLevel.DoNotAppear);
        public static List<PreferenceItem> FindEnabledByLevel(PreferLevel level)
        {
            return SQLHelper.Run(c => c.Query<PreferenceItemDAO>("select * from PreferenceItem where Level = ? and IsEnabled = 1", level).Select(i => i.FromDAO()).ToList());
        }

        public bool Prefer(PreferenceItem item, PreferLevel level)
        {
            item.Level = level;
            EntityType preferType = item.Type;
            if (preferType == EntityType.MyFavorites)
            {
                item.IsEnabled = true;
            }
            else
            {
                List<PreferenceItem> items = FindPreferredItems(preferType);
                if (items.Count >= GetMaxPreferenceItems(preferType))
                {
                    return false;
                }
            }
            if (item.ThisId == 0)
            {
                SQLHelper.Run(c => c.InsertPreferenceItem(item));
            }
            else
            {
                UpdatePreference(item);
            }
            return true;
        }

        public static void UpdatePreference(PreferenceItem item)
        {
            SQLHelper.Run(c => c.Update(item.ToDAO()));
        }

        public bool Prefer(IPreferable preferable)
        {
            EntityType preferType = preferable.AsPreferenceItem().Type;
            List<PreferenceItem> items = FindPreferredItems(preferType);
            if (items.Count >= GetMaxPreferenceItems(preferType))
            {
                return false;
            }
            SQLHelper.Run(c => c.InsertPreferenceItem(preferable.AsPreferenceItem(), preferType));
            return true;
        }

        public void UndoPrefer(IPreferable preferable)
        {
            PreferenceItem item = preferable.AsPreferenceItem();
            PreferenceItemDAO dao = item.ToDAO();
            if (dao.Type == EntityType.MyFavorites)
            {
                dao.IsEnabled = false;
            }
            else
            {
                dao.State = ActiveState.Inactive;
            }
            SQLHelper.Run(c => c.Update(dao));
        }

        public bool IsPreferred(IPreferable preferable)
        {
            PreferenceItem item = preferable.AsPreferenceItem();
            EntityType preferType = item.Type;
            List<PreferenceItem> items = FindPreferredItems(preferType);
            string preferId = item.Id;
            return items.Any(i => i.Id.Equals(preferId));
        }

        public static int GetMaxPreferenceItems(EntityType type)
        {
            switch (type)
            {
                case EntityType.Song:
                    return MaxPreferredSongs;
                case EntityType.Artist:
                    return MaxPreferredArtists;
                case EntityType.Album:
                    return MaxPreferredAlbums;
                case EntityType.Playlist:
                    return MaxPreferredPlaylists;
                case EntityType.Folder:
                    return MaxPreferredFolders;
                default:
                    return 0;
            }
        }
    }
}

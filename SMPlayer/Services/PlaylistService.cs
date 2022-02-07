using SMPlayer.Helpers;
using SMPlayer.Models;
using SMPlayer.Models.DAO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Services
{
    public class PlaylistService
    {
        public static PlaylistView MyFavoritesView 
        {
            get
            {
                PlaylistView playlist = FindPlaylistView(Settings.settings.MyFavoritesId);
                playlist.EntityType = EntityType.MyFavorites;
                return playlist;
            }
        }
        public static Playlist MyFavorites => FindPlaylist(Settings.settings.MyFavoritesId);
        public static List<MusicView> MyFavoriteSongs => FindPlaylistItemViews(Settings.settings.MyFavoritesId);
        public static List<PlaylistView> AllPlaylistViews => AllPlaylists.Select(i => i.ToVO()).ToList();
        public static List<Playlist> AllPlaylists => SQLHelper.Run(c => c.SelectAllPlaylists(i => i.Id != Settings.settings.MyFavoritesId));
        public static PlaylistView FindPlaylistView(long id) { return SQLHelper.Run(c => c.SelectPlaylistById(id))?.ToVO(); }
        public static Playlist FindPlaylist(string name) { return SQLHelper.Run(c => c.SelectPlaylistByName(name)); }
        public static List<MusicView> FindPlaylistItemViews(long id) { return FindPlaylistItems(id).Select(i => i.ToVO()).ToList(); }
        public static Playlist FindPlaylist(long id) => SQLHelper.Run(c => c.SelectPlaylistById(id));

        public static List<Music> FindPlaylistItems(long id) => SQLHelper.Run(c => c.SelectPlaylistItems(id));

        public static List<long> FindPlaylistIdsByItems(IEnumerable<long> itemIds)
        {
            return SQLHelper.Run(c => c.Query<long>("select distinct PlaylistId from PlaylistItem where ItemId in (?) and State = ?", string.Join(",", itemIds), ActiveState.Active));
        }
    }
}

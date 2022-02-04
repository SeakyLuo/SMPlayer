using SMPlayer.Helpers;
using SMPlayer.Models;
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
        public static Playlist MyFavorites 
        {
            get
            {
                Playlist playlist = FindPlaylist(Settings.settings.MyFavoritesId);
                playlist.EntityType = EntityType.MyFavorites;
                return playlist;
            }
        }
        public static List<Music> MyFavoriteSongs
        {
            get => FindPlaylistItems(Settings.settings.MyFavoritesId);
        }
        public static List<Playlist> AllPlaylists
        {
            get => SQLHelper.Run(c => c.SelectAllPlaylists(i => i.Id != Settings.settings.MyFavoritesId));
        }
        public static List<Playlist> AllPlaylistsWithSongs
        {
            get => SQLHelper.Run(c => c.SelectAllPlaylists(i => i.Id != Settings.settings.MyFavoritesId))
                             .AsParallel().AsOrdered()
                             .Select(i =>
                             {
                                 i.Songs = new ObservableCollection<Music>(SQLHelper.Run(c => c.SelectPlaylistItems(i.Id)));
                                 return i;
                             }).ToList();
        }

        public static Playlist FindPlaylist(long id) { return SQLHelper.Run(c => c.SelectPlaylistById(id)); }
        public static Playlist FindPlaylist(string name) { return SQLHelper.Run(c => c.SelectPlaylistByName(name)); }
        public static List<Music> FindPlaylistItems(long id) { return SQLHelper.Run(c => c.SelectPlaylistItems(id)); }
    }
}

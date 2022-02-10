using SMPlayer.Models;
using SMPlayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Helpers
{
    public class RandomPlayHelper
    {
        public static void PlayMusic(int randomLimit = 100)
        {
            MusicPlayer.SetPlaylistAndPlay(MusicService.AllSongs.RandItems(randomLimit));
        }

        public static string PlayArtist(int randomLimit = 100)
        {
            var artist = MusicService.SelectAllArtists().RandItem();
            MusicPlayer.SetPlaylistAndPlay(MusicService.SelectByArtist(artist).RandItems(randomLimit));
            return artist;
        }

        public static string PlayAlbum(int randomLimit = 100)
        {
            var album = MusicService.SelectAllAlbums().RandItem();
            MusicPlayer.SetPlaylistAndPlay(MusicService.SelectByAlbum(album).RandItems(randomLimit));
            return album;
        }

        public static string PlayPlaylist(int randomLimit = 100)
        {
            List<Playlist> allPlaylists = PlaylistService.AllPlaylists;
            Playlist playlist;
            do
            {
                playlist = allPlaylists.RandItem();
                playlist.Songs = PlaylistService.FindPlaylistItems(playlist.Id);
            } while (playlist.IsEmpty);
            MusicPlayer.SetPlaylistAndPlay(playlist.Songs.RandItems(randomLimit));
            return playlist.Name;
        }

        public static string PlayFolder(int randomLimit = 100)
        {
            List<FolderTree> allFolders = StorageService.AllFolders;
            FolderTree folder;
            List<Music> songs;
            do
            {
                folder = allFolders.RandItem();
                songs = StorageService.FindSubSongs(folder);
            } while (songs.IsEmpty());
            MusicPlayer.SetPlaylistAndPlay(songs.RandItems(randomLimit));
            return folder.Name;
        }
    }
}
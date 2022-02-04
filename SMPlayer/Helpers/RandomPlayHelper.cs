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
            MusicPlayer.SetPlaylistAndPlay(Settings.AllSongs.RandItems(randomLimit));
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
        public static Playlist PlayPlaylist(int randomLimit = 100)
        {
            List<Playlist> allPlaylists = PlaylistService.AllPlaylists;
            Playlist playlist;
            do
            {
                playlist = allPlaylists.RandItem();
                playlist.Songs = new System.Collections.ObjectModel.ObservableCollection<Music>(PlaylistService.FindPlaylistItems(playlist.Id));
            } while (playlist.IsEmpty);
            MusicPlayer.SetPlaylistAndPlay(playlist.Songs.RandItems(randomLimit));
            return playlist;
        }

        public static FolderTree PlayFolder(int randomLimit = 100)
        {
            List<FolderTree> allFolders = Settings.AllFolders;
            FolderTree folder;
            do
            {
                folder = Settings.FindFolder(allFolders.RandItem().Id);
            } while (folder.IsEmpty);
            MusicPlayer.SetMusicAndPlay(folder.Songs.RandItems(randomLimit));
            return folder;
        }
    }
}
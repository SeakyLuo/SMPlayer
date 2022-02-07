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
        public static PlaylistView PlayPlaylist(int randomLimit = 100)
        {
            List<PlaylistView> allPlaylists = PlaylistService.AllPlaylistViews;
            PlaylistView playlist;
            do
            {
                playlist = allPlaylists.RandItem();
                playlist.Songs = new System.Collections.ObjectModel.ObservableCollection<MusicView>(PlaylistService.FindPlaylistItemViews(playlist.Id));
            } while (playlist.IsEmpty);
            MusicPlayer.SetPlaylistAndPlay(playlist.Songs.RandItems(randomLimit));
            return playlist;
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
            MusicPlayer.SetMusicAndPlay(songs.RandItems(randomLimit));
            return folder.Name;
        }
    }
}
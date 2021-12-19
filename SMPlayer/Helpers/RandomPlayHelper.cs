using SMPlayer.Models;
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

        public static IGrouping<string, Music> PlayArtist(int randomLimit = 100)
        {
            var rArtist = Settings.AllSongs.GroupBy(m => m.Artist).RandItem();
            MusicPlayer.SetPlaylistAndPlay(rArtist.RandItems(randomLimit));
            return rArtist;
        }

        public static AlbumInfo PlayAlbum(int randomLimit = 100)
        {
            var album = AlbumsPage.AlbumInfoList.RandItem();
            // 没有检查Artist，不过无所谓
            MusicPlayer.SetPlaylistAndPlay(Settings.AllSongs.Where(i => i.Album == album.Name).RandItems(randomLimit));
            return album;
        }
        public static Playlist PlayPlaylist(int randomLimit = 100)
        {
            var playlist = Settings.settings.Playlists.RandItem();
            MusicPlayer.SetPlaylistAndPlay(playlist.Songs.RandItems(randomLimit));
            return playlist;
        }

        public static FolderTree PlayFolder(int randomLimit = 100)
        {
            var folder = Settings.settings.Tree.GetAllTrees().Where(tree => tree.Files.Count > 0).RandItem();
            MusicPlayer.SetMusicAndPlay(folder.Songs.RandItems(randomLimit));
            return folder;
        }
    }
}
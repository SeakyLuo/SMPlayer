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
            MediaHelper.SetPlaylistAndPlay(MusicLibraryPage.AllSongs.RandItems(randomLimit));
        }

        public static IGrouping<string, Music> PlayArtist(int randomLimit = 100)
        {
            var rArtist = MusicLibraryPage.AllSongs.GroupBy(m => m.Artist).RandItem();
            MediaHelper.SetPlaylistAndPlay(rArtist.RandItems(randomLimit));
            return rArtist;
        }

        public static AlbumInfo PlayAlbum(int randomLimit = 100)
        {
            var album = AlbumsPage.AlbumInfoList.RandItem();
            // 没有检查Artist，不过无所谓
            MediaHelper.SetPlaylistAndPlay(MusicLibraryPage.AllSongs.Where(i => i.Album == album.Name).RandItems(randomLimit));
            return album;
        }
        public static Playlist PlayPlaylist(int randomLimit = 100)
        {
            var playlist = Settings.settings.Playlists.RandItem();
            MediaHelper.SetPlaylistAndPlay(playlist.Songs.RandItems(randomLimit));
            return playlist;
        }

        public static FolderTree PlayFolder(int randomLimit = 100)
        {
            var folder = Settings.settings.Tree.GetAllTrees().Where(tree => tree.Files.Count > 0).RandItem();
            MediaHelper.SetMusicAndPlay(folder.Files.RandItems(randomLimit));
            return folder;
        }
    }
}
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Helpers
{
    public class VoiceAssistantHelper
    {
        public static async void HandleCommand(string text)
        {
            string play = Helper.Localize("Play");
            if (text.StartsWith(play))
            {
                string keyword = text.Substring(play.Length);
                var result = await SearchHelper.Search(keyword);
                if (result.Result == null)
                {
                    Helper.ShowNotification("NoResults");
                    return;
                }
                switch (result.SearchType)
                {
                    case SearchType.Artists:
                        Playlist artist = result.Result as Playlist;
                        MediaHelper.ShuffleAndPlay(artist.Songs);
                        Helper.ShowNotificationWithoutLocalization(Helper.LocalizeMessage("SearchResultArtist", artist.Name), 5000);
                        break;
                    case SearchType.Albums:
                        AlbumView album = result.Result as AlbumView;
                        MediaHelper.ShuffleAndPlay(album.Songs);
                        Helper.ShowNotificationWithoutLocalization(Helper.LocalizeMessage("SearchResultAlbum", album.Name), 5000);
                        break;
                    case SearchType.Playlists:
                        AlbumView playlist = result.Result as AlbumView;
                        MediaHelper.ShuffleAndPlay(playlist.Songs);
                        Helper.ShowNotificationWithoutLocalization(Helper.LocalizeMessage("SearchResultPlaylist", playlist.Name), 5000);
                        break;
                    case SearchType.Folders:
                        GridFolderView folder = result.Result as GridFolderView;
                        MediaHelper.ShuffleAndPlay(folder.Songs);
                        Helper.ShowNotificationWithoutLocalization(Helper.LocalizeMessage("SearchResultFolder", folder.Name), 5000);
                        break;
                    case SearchType.Songs:
                        Music music = result.Result as Music;
                        MediaHelper.SetMusicAndPlay(music);
                        Helper.ShowNotificationWithoutLocalization(Helper.LocalizeMessage("SearchResultMusic", music.Name), 5000);
                        break;
                }
            }
        }
    }
}

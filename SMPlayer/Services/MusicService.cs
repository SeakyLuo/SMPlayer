using SMPlayer.Helpers;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Services
{
    public class MusicService
    {
        public static IEnumerable<Music> AllSongs => SQLHelper.Run(c => c.SelectAllMusic());
        public static MusicView FindMusic(long id) { return SQLHelper.Run(c => c.SelectMusicById(id)).ToVO(); }
        public static List<MusicView> FindMusicList(IEnumerable<long> ids) { return ids.IsEmpty() ? new List<MusicView>() : SQLHelper.Run(c => c.SelectMusicByIds(ids)).Select(i => i.ToVO()).ToList(); }

        public static IEnumerable<MusicView> SelectByAlbum(string album)
        {
            return Settings.AllSongs.Where(m => m.Album == album);
        }

        public static IEnumerable<MusicView> SelectByArtist(string artist)
        {
            return Settings.AllSongs.Where(m => m.Artist == artist);
        }

        public static List<string> SelectAllAlbums()
        {
            return SQLHelper.Run(c => c.QueryScalars<string>("select distinct Album from Music"));
        }

        public static List<string> SelectAllArtists()
        {
            return SQLHelper.Run(c => c.QueryScalars<string>("select distinct Artist from Music"));
        }
    }
}

using SMPlayer.Models;
using SMPlayer.Models.VO;
using SMPlayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Helpers
{
    public static class VOConverter
    {
        public static MusicView ToVO(this Music src)
        {
            return new MusicView
            {
                Id = src.Id,
                Path = src.Path,
                Name = src.Name,
                Artist = src.Artist,
                Album = src.Album,
                Duration = src.Duration,
                PlayCount = src.PlayCount,
                DateAdded = src.DateAdded,
            };
        }

        public static Music FromVO(this MusicView src)
        {
            return new Music
            {
                Id = src.Id,
                Path = src.Path,
                Name = src.Name,
                Artist = src.Artist,
                Album = src.Album,
                Duration = src.Duration,
                PlayCount = src.PlayCount,
                DateAdded = src.DateAdded,
            };
        }

        public static Playlist FromVO(this PlaylistView src)
        {
            return new Playlist
            {
                Id = src.Id,
                Songs = src.Songs.Select(i => i.FromVO()).ToList(),
                Criterion = src.Criterion,
                Priority = src.Priority,
            };
        }

        public static PlaylistView ToVO(this Playlist src)
        {
            return new PlaylistView
            {
                Id = src.Id,
                Name = src.Name,
                Songs = new ObservableCollection<MusicView>(src.Songs.Select(i => i.ToVO()).ToList()),
                Criterion = src.Criterion,
                Priority = src.Priority,
            };
        }

        public static Album FromVO(this AlbumView src)
        {
            return new Album(src.Name, src.Songs.Select(i => i.FromVO()));
        }

        public static AlbumView ToVO(this Album src)
        {
            return new AlbumView(src.Name, src.Songs.Select(i => i.ToVO()));
        }
    }
}

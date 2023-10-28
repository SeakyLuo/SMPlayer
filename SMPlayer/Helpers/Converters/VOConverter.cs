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
        public static MusicView ToVO(this Music src, int index = -1, bool isFavorite = false)
        {
            if (src == null)
            {
                return null;
            }
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
                Index = index,
                IsPlaying = false,
                Favorite = isFavorite,
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
                Name = src.Name,
                Songs = src.Songs.Select(i => i.FromVO()).ToList(),
                Criterion = src.Criterion,
                Priority = src.Priority,
            };
        }

        public static PlaylistView ToVO(this Playlist src, EntityType entityType = EntityType.Playlist)
        {
            return new PlaylistView
            {
                Id = src.Id,
                Name = src.Name,
                Songs = new ObservableCollection<MusicView>(src.Songs.Select(i => i.ToVO()).ToList()),
                Criterion = src.Criterion,
                Priority = src.Priority,
                EntityType = entityType,
            };
        }

        public static Album FromVO(this AlbumView src)
        {
            return new Album(src.Name, src.Songs.Select(i => i.FromVO()));
        }

        public static AlbumView ToVO(this Album src)
        {
            return new AlbumView(src.Name, src.Songs, false);
        }
        public static AuthorizedDevice FromVO(this AuthorizedDeviceView src)
        {
            if (src == null) return null;
            return new AuthorizedDevice
            {
                Id = src.Id,
                Ip = src.Ip,
                DeviceName = src.DeviceName,
                State = src.State,
                Auth = src.Auth,
                CreateTime = src.CreateTime,
                UpdateTime = src.UpdateTime,
            };
        }
        public static AuthorizedDeviceView ToVO(this AuthorizedDevice src)
        {
            if (src == null) return null;
            return new AuthorizedDeviceView
            {
                Id = src.Id,
                Ip = src.Ip,
                DeviceName = src.DeviceName,
                State = src.State,
                Auth = src.Auth,
                CreateTime = src.CreateTime,
                UpdateTime = src.UpdateTime,
            };
        }
    }
}

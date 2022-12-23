using SMPlayer.Helpers;
using SMPlayer.Models;
using SMPlayer.Models.DAO;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Services
{
    public class MusicService
    {
        public static void AddMusicEventListener(IMusicEventListener listener) { MusicEventListeners.Add(listener); }
        private static readonly List<IMusicEventListener> MusicEventListeners = new List<IMusicEventListener>();

        public static IEnumerable<Music> AllSongs => SQLHelper.Run(c => c.SelectAllMusic());
        public static Music FindMusic(long id) { return SQLHelper.Run(c => c.SelectMusicById(id)); }
        public static Music FindMusic(string path) { return SQLHelper.Run(c => c.SelectMusicByPath(path)); }
        public static List<Music> FindMusicList(IEnumerable<long> ids) { return ids.IsEmpty() ? new List<Music>() : SQLHelper.Run(c => c.SelectMusicByIds(ids)).ToList(); }

        public static IEnumerable<Music> SelectByAlbum(string album)
        {
            return AllSongs.Where(m => m.Album == album);
        }

        public static IEnumerable<Music> SelectByArtist(string artist)
        {
            return AllSongs.Where(m => m.Artist == artist);
        }

        public static List<string> SelectAllAlbums()
        {
            return SQLHelper.Run(c => c.QueryScalars<string>("select distinct Album from Music"));
        }

        public static List<string> SelectAllArtists()
        {
            return SQLHelper.Run(c => c.QueryScalars<string>("select distinct Artist from Music"));
        }

        public static void MusicModified(Music before, Music after)
        {
            SQLHelper.Run(c => MusicModified(c, before, after));
        }

        public static void MusicModified(SQLiteConnection c, Music before, Music after)
        {
            c.UpdateMusic(after);
            NotifyMusicModified(before, after);
        }

        public static void NotifyMusicModified(Music before, Music after)
        {
            MusicEventArgs args = new MusicEventArgs(MusicEventType.Modify) { ModifiedMusic = after };
            foreach (var listener in MusicEventListeners)
            {
                try
                {
                    listener?.Execute(before, args);
                }
                catch (Exception e)
                {
                    Log.Warn("NotifyMusicModified Exception {0}", e);
                }
            }
        }

        public static async Task<bool> AddMusic(Music music)
        {
            bool isNew = SQLHelper.Run(c =>
            {
                MusicDAO musicDAO = c.Query<MusicDAO>("select * from Music where Path = ?", music.Path).FirstOrDefault();
                if (musicDAO == null)
                {
                    c.InsertMusic(music);
                }
                else
                {
                    music.Id = musicDAO.Id;
                    ActivateMusic(c, music, ActiveState.Active);
                }
                return musicDAO == null;
            });
            if (isNew && Settings.settings.AutoLyrics) // 默认上面那种情况有了
            {
                await Task.Run(async () => await music.FindLyricsIfEmpty());
            }
            foreach (var listener in MusicEventListeners)
            {
                try
                {
                    listener?.Execute(music, new MusicEventArgs(MusicEventType.Add));
                }
                catch (Exception e)
                {
                    Log.Warn($"listener Execute AddMusic failed {e}");
                }
            }
            return isNew;
        }

        public static void RemoveMusic(Music music)
        {
            if (music == null) return;
            SQLHelper.Run(c => ActivateMusic(c, music, ActiveState.Inactive));
            foreach (var listener in MusicEventListeners)
            {
                try
                {
                    listener?.Execute(music, new MusicEventArgs(MusicEventType.Remove));
                }
                catch (Exception e)
                {
                    Log.Warn($"listener RemoveMusic failed {e}");
                }
            }
        }

        public static void UndoRemoveMusic(Music music)
        {
            SQLHelper.Run(c => ActivateMusic(c, music, ActiveState.Active));
            foreach (var listener in MusicEventListeners)
            {
                try
                {
                    listener?.Execute(music, new MusicEventArgs(MusicEventType.Add));
                }
                catch (Exception e)
                {
                    Log.Warn($"listener UndoRemoveMusic failed {e}");
                }
            }
        }

        private static void ActivateMusic(SQLiteConnection c, Music music, ActiveState state)
        {
            c.Execute("update Music set State = ? where Id = ?", state, music.Id);
            c.Execute("update File set State = ? where Path = ?", state, music.Path);
            c.Execute("update PlaylistItem set State = ? where ItemId = ?", state, music.Id);
            SettingsService.UpdateRecentRecordState(c, RecentType.Play, music.Id.ToString(), state);
            c.Execute("update PreferenceItem set State = ? where Type = ? and ItemId = ? ", state, EntityType.Song, music.Id);
        }

        public static void LikeMusic(IEnumerable<IMusicable> playlist)
        {
            foreach (var item in playlist)
            {
                LikeMusic(item.ToMusic());
            }
        }

        public static void DislikeMusic(Music music)
        {
            if (music == null) return;
            PlaylistService.DislikeMusic(music.ToMusic());
            foreach (var listener in MusicEventListeners)
                listener?.Execute(music, new MusicEventArgs(MusicEventType.Like) { IsFavorite = false });
        }

        public static void LikeMusic(Music music)
        {
            if (music == null || PlaylistService.IsFavorite(music)) return;
            PlaylistService.LikeMusic(music);
            foreach (var listener in MusicEventListeners)
                listener?.Execute(music, new MusicEventArgs(MusicEventType.Like) { IsFavorite = true });
        }

        public static List<Music> GetMostPlayed(int limit)
        {
            List<Music> list = new List<Music>();
            foreach (var group in AllSongs.GroupBy(m => m.PlayCount).OrderByDescending(g => g.Key))
            {
                if (list.Count > limit) break;
                list.AddRange(group);
            }
            return list;
        }

        public static List<Music> GetLeastPlayed(int limit)
        {
            List<Music> list = new List<Music>();
            foreach (var group in AllSongs.GroupBy(m => m.PlayCount).OrderBy(g => g.Key))
            {
                if (list.Count > limit) break;
                list.AddRange(group);
            }
            return list;
        }
    }
}

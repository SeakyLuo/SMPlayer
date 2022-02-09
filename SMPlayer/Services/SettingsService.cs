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
    public class SettingsService
    {
        public static void AddRecentEventListener(IRecentEventListener listener) { RecentEventListeners.Add(listener); }
        private static readonly List<IRecentEventListener> RecentEventListeners = new List<IRecentEventListener>();

        public static List<Music> RecentPlayed
        {
            get => SQLHelper.Run(c => c.SelectRecentRecords(RecentType.Play)
                                       .Select(r => r.ItemId)
                                       .Select(i => c.SelectMusicById(long.Parse(i)))
                                       .ToList());
        }
        public static List<string> RecentSearch
        {
            get => SQLHelper.Run(c => c.SelectRecentRecords(RecentType.Search)
                                       .Select(r => r.ItemId)
                                       .ToList());
        }

        public static void Played(Music music)
        {
            if (music == null) return;
            Music newMusic = null, oldMusic = null;
            SQLHelper.Run(c =>
            {
                newMusic = c.SelectMusicById(music.Id) ?? c.SelectMusicByPath(music.Path);
                if (newMusic == null) return; // 直接从本地文件播放而不是读取的数据库会导致null
                oldMusic = newMusic.Copy();
                newMusic.Played();
                c.UpdateMusic(newMusic);
                UpdateRecentRecordState(c, RecentType.Play, music.Id.ToString(), ActiveState.Inactive);
                c.InsertRecentPlayed(music);
            });
            if (newMusic != null)
            {
                MusicService.NotifyMusicModified(oldMusic, newMusic);
                foreach (var listener in RecentEventListeners)
                    listener.Played(music);
            }
        }

        public static void RemoveRecentPlayed(MusicView music = null)
        {
            SQLHelper.Run(c =>
            {
                UpdateRecentRecordState(c, RecentType.Play, music?.Id.ToString(), ActiveState.Inactive);
            });
        }

        // 极端情况可能更改多个RecentRecord，先忽略吧
        public static void UndoRemoveRecentPlayed(MusicView music)
        {
            SQLHelper.Run(c =>
            {
                UpdateRecentRecordState(c, RecentType.Play, music.Id.ToString(), ActiveState.Active);
            });
        }

        public static void RemoveSearchHistory(string keyword = null)
        {
            SQLHelper.Run(c =>
            {
                UpdateRecentRecordState(c, RecentType.Search, keyword, ActiveState.Inactive);
            });
        }

        public static void UndoRemoveSearchHistory(string keyword)
        {
            SQLHelper.Run(c =>
            {
                UpdateRecentRecordState(c, RecentType.Search, keyword, ActiveState.Active);
            });
        }

        public static void UpdateRecentRecordState(SQLiteConnection c, RecentType recentType, string item, ActiveState state)
        {
            string sql = "update RecentRecord set State = ? where Type = ?";
            if (item == null)
            {
                c.Execute(sql, state, recentType);
            }
            else
            {
                c.Execute(sql + " and ItemId = ?", state, recentType, item);
            }
        }

        public static void Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return;
            }
            SQLHelper.Run(c =>
            {
                UpdateRecentRecordState(c, RecentType.Search, keyword, ActiveState.Inactive);
                c.Insert(new RecentRecordDAO()
                {
                    Type = RecentType.Search,
                    ItemId = keyword,
                    Time = DateTimeOffset.Now,
                });
            });
            foreach (var listener in RecentEventListeners)
                listener.Search(keyword);
        }
    }
}

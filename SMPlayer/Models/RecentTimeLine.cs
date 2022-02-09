using SMPlayer.Helpers;
using SMPlayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models
{
    public delegate void NotifyRecentTimeLineChangedEventHandler(RecentTimeLineChangedEventArgs args);

    public class RecentTimeLine
    {
        public const int MAX_RECENT_TIMELINE_ITEMS = 500;
        public ObservableCollection<MusicView> TimeLine { get; private set; }
        public int Count { get => TimeLine.Count; }
        public NotifyRecentTimeLineChangedEventHandler CollectionChanged;
        private int justRemovedIndex = -1;
        private long justRemovedMusic = -1;

        public RecentTimeLine() 
        {
            TimeLine = new ObservableCollection<MusicView>();
        }

        public RecentTimeLine(IEnumerable<MusicView> songs)
        {
            TimeLine = songs == null ? new ObservableCollection<MusicView>() : new ObservableCollection<MusicView>(songs);
        }

        public void Add(MusicView music)
        {
            if (Count >= MAX_RECENT_TIMELINE_ITEMS)
            {
                Remove(TimeLine.Last());
            }
            TimeLine.Insert(justRemovedMusic == music.Id ? justRemovedIndex : 0, music);
            CollectionChanged?.Invoke(new RecentTimeLineChangedEventArgs() { Item = music, Type = MusicEventType.Add });
        }

        public bool Remove(MusicView music)
        {
            justRemovedIndex = TimeLine.IndexOf(music);
            if (justRemovedIndex >= 0)
            {
                justRemovedMusic = music.Id;
                TimeLine.RemoveAt(justRemovedIndex);
                CollectionChanged?.Invoke(new RecentTimeLineChangedEventArgs() { Item = music, Type = MusicEventType.Remove });
                return true;
            }
            return false;
        }

        public void DeleteByFolder(string path)
        {
            TimeLine.RemoveAll(i => i.Path.StartsWith(path));
        }

        public static RecentTimeLine FromMusicList(IEnumerable<Music> list)
        {
            return new RecentTimeLine(list?.OrderByDescending(m => m.DateAdded).Take(MAX_RECENT_TIMELINE_ITEMS).Select(i => i.ToVO()));
        }

        public static RecentTimeLine FromAllSongs()
        {
            return FromMusicList(MusicService.AllSongs);
        }

        public static string Categorize(DateTimeOffset dateAdded)
        {
            DateTime now = DateTime.Now;
            if (dateAdded.Year == now.Year && dateAdded.Month == now.Month && dateAdded.Day == now.Day)
            {
                return "Today";
            }
            const string dateFormat = "yyyyMMdd";
            string formatedDateAdded = dateAdded.ToString(dateFormat);
            if (formatedDateAdded == now.AddDays(-1).ToString(dateFormat))
            {
                return "Yesterday";
            }
            if (dateAdded > now.AddDays(-7))
            {
                return "Recent7Days";
            }
            if (dateAdded.Year == now.Year && dateAdded.Month == now.Month)
            {
                return "ThisMonth";
            }
            if (dateAdded > now.AddDays(-30))
            {
                return "Recent30Days";
            }
            if (dateAdded.Year == now.Year)
            {
                return "Month" + dateAdded.Month;
            }
            return dateAdded.ToString("yyyy.MM");
        }
    }

    public class RecentTimeLineChangedEventArgs
    {
        public MusicView Item { get; set; }
        public MusicEventType Type { get; set; }
    }
}

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
    public delegate void NotifyRecentTimeLineChangedEventHandler();

    public class RecentTimeLine
    {
        public const int MAX_RECENT_TIMELINE_ITEMS = 500;
        public ObservableCollection<Music> TimeLine { get; private set; }
        public int Count { get => TimeLine.Count; }
        public NotifyRecentTimeLineChangedEventHandler CollectionChanged;
        private int justRemovedIndex = -1;
        private long justRemovedMusic = -1;

        public RecentTimeLine() 
        {
            TimeLine = new ObservableCollection<Music>();
        }

        public RecentTimeLine(IEnumerable<Music> songs)
        {
            TimeLine = songs == null ? new ObservableCollection<Music>() : new ObservableCollection<Music>(songs);
        }

        public void Add(Music music)
        {
            if (Count >= MAX_RECENT_TIMELINE_ITEMS)
            {
                Remove(TimeLine.Last());
            }
            TimeLine.Insert(justRemovedMusic == music.Id ? justRemovedIndex : 0, music);
            CollectionChanged?.Invoke();
        }

        public bool Remove(Music music)
        {
            justRemovedIndex = TimeLine.IndexOf(music);
            if (justRemovedIndex >= 0)
            {
                justRemovedMusic = music.Id;
                TimeLine.RemoveAt(justRemovedIndex);
                CollectionChanged?.Invoke();
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
            return new RecentTimeLine(list?.OrderByDescending(m => m.DateAdded).Take(MAX_RECENT_TIMELINE_ITEMS));
        }

        public static string Categorize(DateTimeOffset dateAdded)
        {
            DateTime now = DateTime.Now;
            if (dateAdded.Year == now.Year)
            {
                if (dateAdded.Month == now.Month)
                {
                    if (dateAdded.Day == now.Day)
                    {
                        return "Today";
                    }
                    else if ((now - dateAdded).Days <= 7)
                    {
                        return "ThisWeek";
                    }
                    else
                    {
                        return "ThisMonth";
                    }
                }
                else if ((now - dateAdded).Days <= 30)
                {
                    return "Recent30Days";
                }
                else
                {
                    return "Month" + now.Month;
                }
            }
            else
            {
                return dateAdded.Year.ToString();
            }
        }
    }
}

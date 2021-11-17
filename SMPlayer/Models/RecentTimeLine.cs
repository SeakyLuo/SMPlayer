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

        // TODO 记录just removed index
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

        public void RenameFolder(string oldPath, string newPath)
        {
            foreach (var item in TimeLine)
            {
                item.RenameFolder(oldPath, newPath);
            }
        }

        public void DeleteFolder(string path)
        {
            TimeLine.RemoveAll(i => i.Path.StartsWith(path));
        }

        public static RecentTimeLine FromMusicList(IEnumerable<Music> list)
        {
            return new RecentTimeLine(list?.OrderByDescending(m => m.DateAdded).Take(MAX_RECENT_TIMELINE_ITEMS));
        }

        public static object Categorize(DateTimeOffset dateAdded)
        {
            if (dateAdded.Year == DateTime.Now.Year)
            {
                if (dateAdded.Month == DateTime.Now.Month)
                {
                    if (dateAdded.Day == DateTime.Now.Day)
                    {
                        return RecentTimeLineCategory.Today;
                    }
                    else if ((DateTime.Now - dateAdded).Days <= 7)
                    {
                        return RecentTimeLineCategory.ThisWeek;
                    }
                    else
                    {
                        return RecentTimeLineCategory.ThisMonth;
                    }
                }
                else if (DateTime.Now.Month - dateAdded.Month <= 3)
                {
                    return RecentTimeLineCategory.Recent3Months;
                }
                else if (DateTime.Now.Month - dateAdded.Month <= 6)
                {
                    return RecentTimeLineCategory.Recent6Months;
                }
                else
                {
                    return RecentTimeLineCategory.ThisYear;
                }
            }
            else
            {
                return dateAdded.Year;
            }
        }
    }

    public enum RecentTimeLineCategory
    {
        Today, ThisWeek, ThisMonth, Recent3Months, Recent6Months, ThisYear
    }
}

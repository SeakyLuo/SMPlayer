using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models
{
    public delegate void RecentTimeLineChangedEventHandler(RecentTimeLineCategory category, Music music);

    public class RecentTimeLine
    {
        public const int MAX_RECENT_TIMELINE_ITEMS = 500;
        public List<Music> All { get; private set; } = new List<Music>();
        public List<Music> Today { get; private set; } = new List<Music>();
        public List<Music> ThisWeek { get; private set; } = new List<Music>();
        public List<Music> ThisMonth { get; private set; } = new List<Music>();
        public List<Music> Recent3Months { get; private set; } = new List<Music>();
        public List<Music> Recent6Months { get; private set; } = new List<Music>();
        public List<Music> ThisYear { get; private set; } = new List<Music>();
        public SortedDictionary<int, List<Music>> Years { get; private set; } = new SortedDictionary<int, List<Music>>();
        public int Count { get => All.Count; }
        public event RecentTimeLineChangedEventHandler CollectionChanged;

        public RecentTimeLine() { }
        public RecentTimeLine(params List<Music>[] list)
        {
            foreach (var timeLine in list)
            {
                foreach (var item in timeLine)
                {
                    Categorize(item);
                }
            }
        }

        public void Add(Music music)
        {
            if (Count == MAX_RECENT_TIMELINE_ITEMS)
            {
                Remove(All.Last());
            }
            RecentTimeLineCategory category = Categorize(music);
            CollectionChanged?.Invoke(category, music);
        }

        public bool Remove(Music music)
        {
            bool successful = All.Remove(music);
            if (successful)
            {
                successful = Today.Remove(music) ||
                             ThisWeek.Remove(music) ||
                             ThisMonth.Remove(music) ||
                             Recent3Months.Remove(music) ||
                             Recent6Months.Remove(music) ||
                             ThisYear.Remove(music) ||
                             Years.Any(y => y.Value.Remove(music));
                CollectionChanged?.Invoke(RecentTimeLineCategory.Remove, music);
            }
            return successful;
        }

        public List<Music> GetYear(int year)
        {
            return Years[year];
        }

        public IEnumerable<int> GetYears()
        {
            return Years.Keys;
        }

        private RecentTimeLineCategory Categorize(Music item)
        {
            All.Add(item);
            DateTimeOffset dateAdded = item.DateAdded;
            if (dateAdded.Year == DateTime.Now.Year)
            {
                if (dateAdded.Month == DateTime.Now.Month)
                {
                    if (dateAdded.Day == DateTime.Now.Day)
                    {
                        Today.Add(item);
                        return RecentTimeLineCategory.Today;
                    }
                    else if ((DateTime.Now - dateAdded).Days <= 7)
                    {
                        ThisWeek.Add(item);
                        return RecentTimeLineCategory.ThisWeek;
                    }
                    else
                    {
                        ThisMonth.Add(item);
                        return RecentTimeLineCategory.ThisMonth;
                    }
                }
                else if (DateTime.Now.Month - dateAdded.Month <= 3)
                {
                    Recent3Months.Add(item);
                    return RecentTimeLineCategory.Recent3Months;
                }
                else if (DateTime.Now.Month - dateAdded.Month <= 6)
                {
                    Recent6Months.Add(item);
                    return RecentTimeLineCategory.Recent6Months;
                }
                else
                {
                    ThisYear.Add(item);
                    return RecentTimeLineCategory.ThisYear;
                }
            }
            else
            {
                List<Music> year = GetYear(dateAdded.Year);
                year.Add(item);
                Years[dateAdded.Year] = year;
                return RecentTimeLineCategory.Year;
            }
        }

        public static RecentTimeLine FromMusicList(IEnumerable<Music> list)
        {
            if (list == null) return null;
            List<Music> songs = list.ToList();
            songs.Sort((i1, i2) => i2.DateAdded.CompareTo(i1.DateAdded));
            RecentTimeLine timeLine = new RecentTimeLine();
            // 取前n个
            foreach (var item in songs.Take(MAX_RECENT_TIMELINE_ITEMS))
            {
                timeLine.Categorize(item);
            }
            return timeLine;
        }
    }

    public enum RecentTimeLineCategory
    {
        Today, ThisWeek, ThisMonth, Recent3Months, Recent6Months, ThisYear, Year, Remove
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models
{
    public delegate void RecentTimeLineChangedEventHandler(RecentTimeLineCategory category, Music music);

    public class RecentTimeLine
    {
        public const int MAX_RECENT_TIMELINE_ITEMS = 500;
        public List<TimeLineMusic> All { get; private set; } = new List<TimeLineMusic>();
        public List<TimeLineMusic> Today { get; private set; } = new List<TimeLineMusic>();
        public List<TimeLineMusic> ThisWeek { get; private set; } = new List<TimeLineMusic>();
        public List<TimeLineMusic> ThisMonth { get; private set; } = new List<TimeLineMusic>();
        public List<TimeLineMusic> Recent3Months { get; private set; } = new List<TimeLineMusic>();
        public List<TimeLineMusic> Recent6Months { get; private set; } = new List<TimeLineMusic>();
        public List<TimeLineMusic> ThisYear { get; private set; } = new List<TimeLineMusic>();
        public SortedDictionary<int, List<TimeLineMusic>> Years { get; private set; } = new SortedDictionary<int, List<TimeLineMusic>>();
        public int Count { get => All.Count; }
        public event RecentTimeLineChangedEventHandler CollectionChanged;

        public RecentTimeLine() { }
        public RecentTimeLine(params List<TimeLineMusic>[] list)
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
                Remove(All.Last().Data);
            }
            RecentTimeLineCategory category = Categorize(music);
            CollectionChanged?.Invoke(category, music);
        }

        public bool Remove(string path)
        {
            return Remove(path, Settings.FindMusic(path));
        }

        public bool Remove(Music music)
        {
            return Remove(music.Path, music);
        }

        private bool Remove(string path, Music music)
        {
            bool successful = All.RemoveMusic(path);
            if (successful)
            {
                successful = Today.RemoveMusic(path) ||
                             ThisWeek.RemoveMusic(path) ||
                             ThisMonth.RemoveMusic(path) ||
                             Recent3Months.RemoveMusic(path) ||
                             Recent6Months.RemoveMusic(path) ||
                             ThisYear.RemoveMusic(path) ||
                             Years.Any(y => y.Value.RemoveMusic(path));
                CollectionChanged?.Invoke(RecentTimeLineCategory.Remove, music);
            }
            return successful;
        }

        public List<TimeLineMusic> GetYear(int year)
        {
            return Years[year];
        }

        public IEnumerable<int> GetYears()
        {
            return Years.Keys;
        }

        private RecentTimeLineCategory Categorize(ITimeLineMusic item)
        {
            All.Add(item);
            DateTimeOffset dateAdded = item.GetDateAdded();
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
                List<TimeLineMusic> year = GetYear(dateAdded.Year);
                year.Add(item);
                Years[dateAdded.Year] = year;
                return RecentTimeLineCategory.Year;
            }
        }

        public static RecentTimeLine FromMusicList(IEnumerable<ITimeLineMusic> list)
        {
            if (list == null) return null;
            List<ITimeLineMusic> songs = list.ToList();
            songs.Sort((i1, i2) => i2.GetDateAdded().CompareTo(i1.GetDateAdded()));
            RecentTimeLine timeLine = new RecentTimeLine();
            foreach (var item in songs.Take(MAX_RECENT_TIMELINE_ITEMS))
            {
                timeLine.Categorize(item);
            }
            // 取前n个
            return timeLine;
        }
    }

    public enum RecentTimeLineCategory
    {
        Today, ThisWeek, ThisMonth, Recent3Months, Recent6Months, ThisYear, Year, Remove
    }
}

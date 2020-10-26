using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models
{
    public class TimeLine<T>
    {
        public object Title { get; set; }
        public List<TimeLineItem<T>> Items { get; set; }
        public IEnumerable<T> Data { get => Items.Select(i => i.Data); }
        public int Count { get => Items.Count; }

        public TimeLine()
        {
        }

        public TimeLine(object title, List<TimeLineItem<T>> list)
        {
            Title = title;
            Items = list;
        }
    }

    public class TimeLineItem<T>
    {
        public T Data { get; set; }
        public DateTimeOffset Time { get; set; }

        public TimeLineItem(T data, DateTimeOffset time)
        {
            Data = data;
            Time = time;
        }
    }

    public static class TimeLineHelper
    {
        public static void Add(this List<TimeLineMusic> timeLine, ITimeLineMusic music)
        {
            TimeLineMusic timeLineMusic = music.ToTimeLineMusic();
            int index = timeLine.FindIndex(m => m.Time <= timeLineMusic.Time);
            if (index == -1)
            {
                timeLine.Add(timeLineMusic);
            }
            else
            {
                timeLine.Insert(index, timeLineMusic);
            }
        }

        public static List<TimeLineItem<Music>> ToTimeLineList(this List<TimeLineItem<string>> recentTimeLine)
        {
            var list = new List<TimeLineItem<Music>>();
            foreach (var item in recentTimeLine)
                list.Add(new TimeLineItem<Music>(Settings.FindMusic(item.Data), item.Time));
            return list;
        }

        public static IEnumerable<Music> ToMusicList(this IEnumerable<TimeLineMusic> list)
        {
            return list.Select(i => Settings.FindMusic(i.Data));
        }

        public static bool RemoveMusic(this List<TimeLineMusic> list, string path)
        {
            return list.RemoveAll(i => i.Data == path) > 0;
        }
    }
}

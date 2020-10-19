using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models
{
    public class TimeLine<T>
    {
        public object Title { get; set; }
        public List<TimeLineItem<T>> Items { get; set; }
        public List<T> Data { get => Items.ConvertAll(i => i.Data); }
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

    public class MusicTimeLine : TimeLine<Music>
    {
        public MusicTimeLine(string title)
        {
            Title = Helper.LocalizeMessage(title);
            Items = new List<TimeLineItem<Music>>();
        }

        public MusicTimeLine(int title)
        {
            Title = title;
            Items = new List<TimeLineItem<Music>>();
        }

        public void AddItem(Music music)
        {
            Items.Add(new TimeLineItem<Music>(music, music.DateAdded));
        }
    }

    public static class TimeLineHelper
    {
        public static ICollection<MusicTimeLine> JoinTimeLine(this ICollection<MusicTimeLine> timeLine, params MusicTimeLine[] musicTimeLines)
        {
            foreach (var musicTimeLine in musicTimeLines)
            {
                if (musicTimeLine.Count > 0)
                {
                    timeLine.Add(musicTimeLine);
                }
            }
            return timeLine;
        }
    }
}

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
    }
}

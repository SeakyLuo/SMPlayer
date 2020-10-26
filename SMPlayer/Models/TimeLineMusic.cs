using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SMPlayer.Models
{
    public class TimeLineMusic : TimeLineItem<string>, ITimeLineMusic, IComparable<TimeLineMusic>
    {
        public TimeLineMusic(string data, DateTimeOffset time) : base(data, time)
        {
        }

        int IComparable<TimeLineMusic>.CompareTo(TimeLineMusic other)
        {
            return other.Time.CompareTo(Time);
        }

        DateTimeOffset ITimeLineMusic.GetDateAdded()
        {
            return Time;
        }

        TimeLineMusic ITimeLineMusic.ToTimeLineMusic()
        {
            return this;
        }
    }

    public class MusicTimeLine : TimeLine<Music>, INotifyPropertyChanged
    {
        public RecentTimeLineCategory Category { get; set; }

        public MusicTimeLine(object title)
        {
            if (title is RecentTimeLineCategory category)
            {
                Category = category;
                Title = Helper.LocalizeMessage(category.ToString());
            }
            else
            {
                Title = title;
            }
            Items = new List<TimeLineItem<Music>>();
        }

        public MusicTimeLine(object title, List<TimeLineItem<Music>> items)
        {
            Title = title;
            Items = items;
        }

        public void AddItem(Music music)
        {
            Items.Add(new TimeLineItem<Music>(music, music.DateAdded));
            OnPropertyChanged();
        }

        public void AddMusic(Music music)
        {
            TimeLineItem<Music> timeLineItem = new TimeLineItem<Music>(music, music.DateAdded);
            int index = Items.FindIndex(m => m.Time <= music.DateAdded);
            if (index == -1)
            {
                Items.Add(timeLineItem);
            }
            else
            {
                Items.Insert(index, timeLineItem);
            }
            OnPropertyChanged();
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public interface ITimeLineMusic
    {
        DateTimeOffset GetDateAdded();
        TimeLineMusic ToTimeLineMusic();
    }
}

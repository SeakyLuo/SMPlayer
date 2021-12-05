using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SMPlayer.Models
{
    public class MusicTimeLine : INotifyPropertyChanged
    {
        public object Title { get; set; }
        public List<Music> Items { get; set; }
        public RecentTimeLineCategory Category { get; set; }

        public MusicTimeLine(object title, List<Music> items)
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
            Items = items;
        }

        public void AddItem(Music music)
        {
            Items.Add(music);
            OnPropertyChanged();
        }

        public void AddMusic(Music music)
        {
            int index = Items.FindIndex(m => m.DateAdded <= music.DateAdded);
            if (index == -1)
            {
                Items.Add(music);
            }
            else
            {
                Items.Insert(index, music);
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
}

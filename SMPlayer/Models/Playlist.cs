﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace SMPlayer.Models
{
    [Serializable]
    public class Playlist : INotifyPropertyChanged
    {
        public static SortBy[] Criteria = new SortBy[] { SortBy.Title, SortBy.Artist, SortBy.Album, SortBy.Duration, SortBy.PlayCount };
        private string name;
        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged();
                }
            }
        }
        private SortBy criterion = SortBy.Title;
        public SortBy Criterion
        {
            get => criterion;
            set
            {
                if (criterion == value) return;
                criterion = value;
                Sort();
            }
        }

        public ObservableCollection<Music> Songs { get; set; }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public Playlist() { }

        public Playlist(string Name)
        {
            this.Name = Name;
            this.Songs = new ObservableCollection<Music>();
        }

        public Playlist(string Name, Music music)
        {
            this.Name = Name;
            this.Songs = new ObservableCollection<Music>() { music };
        }

        public Playlist(string Name, IEnumerable<Music> Songs)
        {
            this.Name = Name;
            this.Songs = new ObservableCollection<Music>(Songs);
        }

        public Playlist Duplicate(string NewName)
        {
            return new Playlist(NewName, Songs);
        }

        public void Add(object item)
        {
            if (item is Music && !Songs.Contains(item))
                Songs.Add(item as Music);
            else if (item is ICollection<Music>)
            {
                var set = Songs.ToHashSet();
                foreach (var music in item as ICollection<Music>)
                    if (!set.Contains(music))
                        Songs.Add(music);
            }
            else return;
            Sort();
        }

        public async Task<List<BitmapImage>> GetThumbnails()
        {
            List<BitmapImage> Thumbnails = new List<BitmapImage>();
            foreach (var group in Songs.GroupBy((m) => m.Album))
                Thumbnails.Add(await Helper.GetThumbnail(group.ElementAt(0).Path, true));
            return Thumbnails;
        }
        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Sort()
        {
            List<Music> list;
            switch (criterion)
            {
                case SortBy.Title:
                    list = Songs.OrderBy((m) => m.Name).ToList();
                    break;
                case SortBy.Artist:
                    list = Songs.OrderBy((m) => m.Artist).ToList();
                    break;
                case SortBy.Album:
                    list = Songs.OrderBy((m) => m.Album).ToList();
                    break;
                case SortBy.Duration:
                    list = Songs.OrderBy((m) => m.Duration).ToList();
                    break;
                case SortBy.PlayCount:
                    list = Songs.OrderBy((m) => m.PlayCount).ToList();
                    break;
                default:
                    return;
            }
            Songs = new ObservableCollection<Music>(list);
            OnPropertyChanged();
        }
    }
    
    public enum SortBy
    {
        Title = 0,
        Artist = 1,
        Album = 2,
        Duration = 3,
        PlayCount = 4
    }

    public static class SortByConverter
    {
        public static string ToStr(this SortBy criterion)
        {
            switch (criterion)
            {
                case SortBy.Title:
                    return "Title";
                case SortBy.Artist:
                    return "Artist";
                case SortBy.Album:
                    return "Album";
                case SortBy.Duration:
                    return "Duration";
                case SortBy.PlayCount:
                    return "Play Count";
                default:
                    return "";
            }
        }
    }
}


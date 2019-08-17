﻿using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace SMPlayer
{
    class MusicDurationConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string && int.TryParse((string)value, out int seconds)) return ToTime(seconds);
            if (value is int) return ToTime((int)value);
            if (value is double) return ToTime((double)value);
            return "0:00";
        }

        public static string ToTime(double seconds)
        {
            return ToTime((int)seconds);
        }

        public static string ToTime(int seconds)
        {
            int second = seconds % 60;
            string zero = second < 10 ? "0" : "";
            int minute = (seconds - second) / 60;
            return string.Format("{0}:{1}{2}", minute, zero, second);
        }

        public static string ToTime(IEnumerable<Music> list)
        {
            int seconds = list.Sum((music) => music.Duration);
            if (seconds == 0) return "";
            int minutes = seconds / 60,
                hours = minutes / 60,
                days = hours / 24;
            minutes %= 60;
            hours %= 60;
            days %= 24;
            string second = seconds < 60 ? seconds + "seconds" : "",
                   minute = minutes == 0 ? "" : minutes + "minutes",
                   hour = hours == 0 ? "" : hours + "hours",
                   day = days == 0 ? "" : days+ "days";
            return day + hour + minute + second;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    class MusicFavoriteConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value.Equals(true) ? "\uEB52" : "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    class MusicPlayCountConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int)
            {
                int count = (int)value;
                if (count > 0) return count;
            }
            return "";

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    class MusicVisibilityConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value.Equals(MediaControl.CurrentMusic.Name) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    class LargeThumbnailVisibilityConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    class AlbumNameVisibilityConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is string && string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    class ArtistAlbumInfoConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is List<AlbumView>)
            {
                var list = (List<AlbumView>)value;
                int songs = 0;
                foreach (var album in (List<AlbumView>)value)
                    songs += album.Songs.Count;
                return string.Format("Albums: {0}   Songs: {1}", list.Count, songs);
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    class SongCountConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is List<Music>)
            {
                var list = (List<Music>)value;
                return string.Format("Songs: {0}", list.Count);
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
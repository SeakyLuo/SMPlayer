﻿using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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

        public static string ToTime(ICollection<Music> list)
        {
            int total_seconds = list.Sum((music) => music.Duration);
            if (total_seconds == 0) return "";
            int seconds = total_seconds % 60,
                minutes = total_seconds / 60,
                hours = minutes / 60,
                days = hours / 24;
            minutes %= 60;
            hours %= 60;
            days %= 24;
            string second = seconds != 0 && (total_seconds < 60 || minutes < 10) ? string.Format("{0} {1}", seconds, TryPlural("second", seconds)) : "",
                   minute = minutes == 0 || days > 0 ? "" : string.Format("{0} {1} ", minutes, TryPlural("minute", minutes)),
                   hour = hours == 0 ? "" : string.Format("{0} {1} ", hours, TryPlural("hour", hours)),
                   day = days == 0 ? "" : string.Format("{0} {1} ", days, TryPlural("day", days));
            return day + hour + minute + second;
        }

        public static string TryPlural(string str, int quantity)
        {
            return quantity == 1 ? str : str + 's';
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


    class MusicIsPlayingConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value.Equals(true) ? "\uE767" : "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    class IntConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public static string ToStr(int value)
        {
            int count = (int)value;
            if (count > 0) return count.ToString();
            return "";
        }
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is int ? ToStr((int)value) : "";
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
            return value.Equals(true) ? Visibility.Visible : Visibility.Collapsed;
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

    class RowColorConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value.Equals(true) ? Helper.GetHighlightBrush() : 
                                        PlaylistControl.CurrentTheme == ElementTheme.Dark ? Helper.WhiteSmokeBrush : Helper.BlackBrush;
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
            if (value is ICollection<AlbumView>)
            {
                var list = (ICollection<AlbumView>)value;
                return string.Format("Albums: {0} • Songs: {1}", list.Count(), list.Sum((a) => a.Songs.Count));
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
            if (value is ICollection<Music>)
            {
                var list = (ICollection<Music>)value;
                int count = list.Count();
                string countStr = "Songs: " + count.ToString();
                return count == 0 ? countStr : string.Format("{0} • {1}", countStr, MusicDurationConverter.ToTime(list));
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    class EnabledConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ICollection<Music>)
                return (value as ICollection<Music>).Count > 0;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
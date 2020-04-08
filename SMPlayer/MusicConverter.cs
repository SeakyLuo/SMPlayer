using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace SMPlayer
{
    class MusicDurationConverter : IValueConverter
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
            return TimeSpan.FromSeconds(seconds).ToString(@"m\:ss");
        }

        public static string ToTime(ICollection<Music> list)
        {
            int total_seconds = list.Sum((music) => music.Duration);
            if (total_seconds == 0) return "";
            var time = TimeSpan.FromSeconds(total_seconds);
            int seconds = time.Seconds,
                minutes = time.Minutes,
                hours = time.Hours,
                days = time.Days;
            string second = seconds != 0 && (total_seconds < 60 || minutes < 10) && hours == 0 && days == 0 ? $"{seconds} {Helper.Localize(TryPlural("second", seconds))}" : "",
                   minute = minutes == 0 || days > 0 ? "" : $"{minutes} {Helper.Localize(TryPlural("minute", minutes))} ",
                   hour = hours == 0 ? "" : $"{hours} {Helper.Localize(TryPlural("hour", hours))} ",
                   day = days == 0 ? "" : $"{days} {Helper.Localize(TryPlural("day", days))} ";
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

    class MusicFavoriteConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value.Equals(true) ? "\uEB52" : string.IsNullOrEmpty((string)parameter) ? "" : "\uEB51";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    class MusicIsPlayingConverter : IValueConverter
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

    class IntConverter : IValueConverter
    {
        public static string ToStr(int value)
        {
            return value > 0 ? value.ToString() : "";
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

    class VisibilityConverter : IValueConverter
    {
        public static bool IsCollapsed(object value)
        {
            if (value is string str) return string.IsNullOrEmpty(str);
            else if (value is bool) return value.Equals(false);
            else if (value is int) return value.Equals(0);
            else if (value is Visibility) return value.Equals(Visibility.Visible);
            return value == null;
        }
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return IsCollapsed(value) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    class PlaylistRowColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value.Equals(ElementTheme.Dark) ? ColorConverter.StringToColor((string)parameter) : ColorHelper.BlackBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    class ColorConverter : IValueConverter
    {
        public static SolidColorBrush StringToColor(string color)
        {
            switch (color)
            {
                case "White": return ColorHelper.WhiteBrush;
                case "Gray": return ColorHelper.GrayBrush;
                default: return ColorHelper.BlackBrush;
            }
        }
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value.Equals(true) ? ColorHelper.HighlightBrush : StringToColor((string)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    class RowColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value.Equals(true) ? ColorHelper.HighlightBrush : ColorHelper.BlackBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    class SongCountConverter : IValueConverter
    {
        public static string ToStr(object value)
        {
            if (value is ICollection<Music> list)
            {
                int count = list.Count();
                string countStr = GetSongCount(count);
                return count < 2 ? countStr : $"{countStr} • {MusicDurationConverter.ToTime(list)}";
            }
            else if (value is int count)
            {
                return GetSongCount(count);
            }
            return "";
        }
        public static string GetSongCount(int count)
        {
            return Helper.LocalizeMessage("Songs:") + count.ToString();
        }
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ToStr(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    class EnabledConverter : IValueConverter
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
    class CriterionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((SortBy)value) == SortByConverter.FromStr(parameter as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
    class MusicArtistAlbumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Music music = value as Music;
            return $"{music.Artist} • {music.Album}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    class HorizontalAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool visible) return visible ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            return HorizontalAlignment.Right;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    class EmptyStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string content = (string)value, defaultString = Helper.LocalizeMessage((string)parameter);
            return string.IsNullOrEmpty(content) ? defaultString : content;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
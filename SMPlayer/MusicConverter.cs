using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Data;

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
            int second = seconds % 60;
            string zero = second < 10 ? "0" : "";
            int minute = (seconds - second) / 60;
            return $"{minute}:{zero}{second}";
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
            string second = seconds != 0 && (total_seconds < 60 || minutes < 10) && hours == 0 && days == 0 ? $"{seconds} {TryPlural("second", seconds)}" : "",
                   minute = minutes == 0 || days > 0 ? "" : $"{minutes} {TryPlural("minute", minutes)} ",
                   hour = hours == 0 ? "" : $"{hours} {TryPlural("hour", hours)} ",
                   day = days == 0 ? "" : $"{days} {TryPlural("day", days)} ";
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
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool collapsed;
            if (value is string) collapsed = string.IsNullOrEmpty(value as string);
            else if (value is bool) collapsed = value.Equals(false);
            else if (value is int) collapsed = value.Equals(0);
            else if (value is Visibility) collapsed = value.Equals(Visibility.Visible);
            else collapsed = value == null;
            return collapsed ? Visibility.Collapsed : Visibility.Visible;
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
            if (value.Equals(true)) return ColorHelper.HighlightBrush;
            else if (PlaylistControl.CurrentTheme != ElementTheme.Dark) return ColorHelper.BlackBrush;
            return ColorConverter.StringToColor((string)parameter);
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
                string countStr = SongCount(count);
                return count < 2 ? countStr : $"{countStr} • {MusicDurationConverter.ToTime(list)}";
            }
            else if (value is int count)
            {
                return SongCount(count);
            }
            return "";
        }
        public static string SongCount(int count)
        {
            return "Songs: " + count.ToString();
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
}
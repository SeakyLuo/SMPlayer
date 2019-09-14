using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

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
            string second = seconds != 0 && (total_seconds < 60 || minutes < 10) ? $"{seconds} {TryPlural("second", seconds)} " : "",
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

    class VisibilityConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string) return string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;
            if (value is bool) return value.Equals(false) ? Visibility.Collapsed : Visibility.Visible;
            if (value is int) return value.Equals(0) ? Visibility.Collapsed : Visibility.Visible;
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    class PlaylistRowColorConverter : Windows.UI.Xaml.Data.IValueConverter
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

    class ColorConverter : Windows.UI.Xaml.Data.IValueConverter
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

    class RowColorConverter : Windows.UI.Xaml.Data.IValueConverter
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

    class ArtistAlbumInfoConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ICollection<AlbumView>)
            {
                var list = (ICollection<AlbumView>)value;
                return $"Albums: {list.Count()} • Songs: {list.Sum((a) => a.Songs.Count)}";
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
                return count == 0 ? countStr : $"{countStr} • {MusicDurationConverter.ToTime(list)}";
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
    class CriterionConverter : Windows.UI.Xaml.Data.IValueConverter
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
    class MusicArtistAlbumConverter : Windows.UI.Xaml.Data.IValueConverter
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
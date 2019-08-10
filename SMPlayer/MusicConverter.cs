using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer
{
    class MusicDurationConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string)
            {
                int seconds;
                if (int.TryParse((string)value, out seconds)) return ToTime(seconds);
            }
            if (value is int || value is double) return ToTime((int)value);
            return "0:00";
        }

        public static string ToTime(int seconds)
        {
            int second = seconds % 60;
            string zero = second < 10 ? "0" : "";
            int minute = (seconds - second) / 60;
            return string.Format("{0}:{1}{2}", minute, zero, second);
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
            //return (value is bool && (bool)value) ? "\uEB52" : "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    class MusicPlayedTimesConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int)
            {
                int times = (int)value;
                if (times > 0) return times;
            }
            return "";

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
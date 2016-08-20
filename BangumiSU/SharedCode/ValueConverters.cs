using BangumiSU.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace BangumiSU.SharedCode
{
    public class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var dt = ((DateTimeOffset)value).LocalDateTime;
            var str = "yyyy/MM/dd ";
            if (dt.TimeOfDay.TotalMinutes > 0)
                str += "HH:mm";
            return dt.ToString(str);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var dt = new DateTime();
            var str = value.ToString();
            if (!DateTime.TryParse(str, out dt))
            {
                var parts = str.Split(' ');
                dt = DateTime.Parse(parts[0]);

                if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
                {
                    var temp = parts[1].Trim().Split('.', ':', ',', ' ');
                    int d = 0, h = 0, m = 0;
                    switch (temp.Length)
                    {
                        case 1: h = int.Parse(temp[0]); break;
                        case 2: h = int.Parse(temp[0]); m = int.Parse(temp[1]); break;
                        case 3: d = int.Parse(temp[0]); h = int.Parse(temp[1]); m = int.Parse(temp[2]); break;
                    }
                    if (h >= 24)
                    { h = h - 24; d = d + 1; }
                    var ts = new TimeSpan(d, h, m, 0);
                    dt += ts;
                }
            }
            return new DateTimeOffset(dt);
        }
    }

    public class TrackingStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var t = (Tracking)value;
            Color clr;
            if (t.Count > t.Progress)
                clr = Colors.SeaGreen;
            else if ((DateTimeOffset.Now - t.LastUpdate).TotalDays >= 7)
                clr = Colors.IndianRed;
            else
                clr = Colors.Transparent;
            return new SolidColorBrush(clr);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var b = (bool?)value ?? false;
            return b ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var v = (Visibility)value;
            return v == Visibility.Visible;
        }
    }

    public class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return string.Empty;
            if (parameter == null)
                return value;
            return string.Format((string)parameter, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class NullableBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool?)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (bool?)value == true;
        }
    }

    public class IntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return 0;
            return (int)value;
        }
    }

    public class TrackingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (Tracking)value;
        }
    }

    public class BangumiConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (Bangumi)value;
        }
    }
}

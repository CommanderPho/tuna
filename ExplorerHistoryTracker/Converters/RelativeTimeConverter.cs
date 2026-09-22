using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ExplorerHistoryTracker.Converters
{
    /// <summary>
    /// Converts a DateTime object into a relative friendly time string (e.g. "Just now", "5 min ago", "Yesterday 14:32").
    /// </summary>
    public class RelativeTimeConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                TimeSpan span = DateTime.Now - dateTime;

                // Handle future date edge cases due to clock skew
                if (span.TotalSeconds < 0)
                {
                    return "Just now";
                }

                if (span.TotalSeconds < 60)
                {
                    return "Just now";
                }
                if (span.TotalMinutes < 60)
                {
                    return $"{(int)span.TotalMinutes} min ago";
                }
                if (span.TotalHours < 24)
                {
                    return $"{(int)span.TotalHours} hr ago";
                }
                if (span.TotalDays < 2)
                {
                    return $"Yesterday {dateTime:HH:mm}";
                }
                if (span.TotalDays < 7)
                {
                    return $"{(int)span.TotalDays} days ago";
                }

                return dateTime.ToString("yyyy-MM-dd HH:mm");
            }
            return string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

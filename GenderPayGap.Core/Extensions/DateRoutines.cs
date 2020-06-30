using System;
using System.Globalization;

namespace GenderPayGap.Extensions
{
    /// <summary>
    ///     Miscellaneous and parsing methods for DateTime
    /// </summary>
    public static class DateTimeRoutines
    {

        public static string ToFriendlyDate(this DateTime date)
        {
            if (date == DateTime.MinValue)
            {
                return null;
            }

            var day = "th";
            switch (date.Day)
            {
                case 1:
                case 21:
                case 31:
                    day = "st";
                    break;
                case 2:
                case 22:
                    day = "nd";
                    break;
                case 3:
                case 23:
                    day = "rd";
                    break;
            }

            return $"{date.ToString("MMMM d")}{day}, {date.Year}";
        }


        public static string ToFriendly(this DateTime date, bool showTime = true, string emptyValue = "Never")
        {
            if (date == DateTime.MinValue)
            {
                return emptyValue;
            }

            TimeSpan span = VirtualDateTime.Now - date;

            // Normalize time span
            var future = false;
            if (span.TotalSeconds < 0)
            {
                // In the future
                span = -span;
                future = true;
            }

            // Test for Now
            double totalSeconds = span.TotalSeconds;
            if (totalSeconds < 0.9)
            {
                return "Now";
            }

            // Date/time near current date/time
            string format = future ? "In {0} {1}" : "{0} {1} ago";
            if (totalSeconds < 55)
            {
                // Seconds
                int seconds = Math.Max(1, span.Seconds);
                return string.Format(
                    format,
                    seconds,
                    seconds == 1 ? "second" : "seconds");
            }

            if (totalSeconds < 55 * 60)
            {
                // Minutes
                int minutes = Math.Max(1, span.Minutes);
                return string.Format(
                    format,
                    minutes,
                    minutes == 1 ? "minute" : "minutes");
            }

            if (totalSeconds < 24 * 60 * 60)
            {
                // Hours
                int hours = Math.Max(1, span.Hours);
                return string.Format(
                    format,
                    hours,
                    hours == 1 ? "hour" : "hours");
            }

            // Format both date and time
            if (totalSeconds < 48 * 60 * 60)
            {
                // 1 Day
                format = future ? "Tomorrow" : "Yesterday";
            }
            else if (totalSeconds < 3 * 24 * 60 * 60)
            {
                // 2 Days
                format = string.Format(format, 2, "days");
            }
            else
            {
                // Absolute date
                if (date.Year == VirtualDateTime.Now.Year)
                {
                    format = date.ToString(@"MMM d");
                }
                else
                {
                    format = date.ToString(@"MMM d, yyyy");
                }
            }

            // Add time
            if (!showTime)
            {
                return format;
            }

            return string.Format("{0} at {1:hh:mm}", format, date);
        }

        public static string ToFriendly(this TimeSpan interval, string zeroText = null, int maxParts = 4)
        {
            if (interval <= TimeSpan.Zero)
            {
                return zeroText;
            }

            string result = null;
            var parts = 0;
            if (interval.Days > 0)
            {
                result += interval.Days;
                result += " day" + (interval.Days > 1 ? "s" : "");
                parts++;
            }

            if (interval.Hours > 0 && parts < maxParts)
            {
                parts++;
                if (result != null)
                {
                    result += parts == maxParts ? " and " : ", ";
                }

                result += interval.Hours;
                result += " hour" + (interval.Hours > 1 ? "s" : "");
            }

            if (interval.Minutes > 0 && parts < maxParts)
            {
                parts++;
                if (result != null)
                {
                    result += parts == maxParts ? " and " : ", ";
                }

                result += interval.Minutes;
                result += " minute" + (interval.Minutes > 1 ? "s" : "");
            }

            if (interval.Days == 0 && interval.Hours == 0 && interval.Seconds > 0 && parts < maxParts)
            {
                parts++;
                if (result != null)
                {
                    result += parts == maxParts ? " and " : ", ";
                }

                result += interval.Seconds;
                result += " second" + (interval.Seconds > 1 ? "s" : "");
            }

            return result;
        }

        public static string ToSmallDateTime(this DateTime dateTime)
        {
            return dateTime.ToString(Time.ShortDateFormat);
        }
        
        public static DateTime FromSmallDateTime(this string shortDateTime, bool defaultMaxDateTime = false)
        {
            DateTime dateTime;
            if (DateTime.TryParseExact(shortDateTime, Time.ShortDateFormat, null, DateTimeStyles.AssumeLocal, out dateTime))
            {
                return dateTime;
            }

            return defaultMaxDateTime ? DateTime.MaxValue : DateTime.MinValue;
        }

        public static int ToTwoDigitYear(this int year)
        {
            return new DateTime(year, 1, 1).ToString("yy").ToInt32();
        }

    }
}

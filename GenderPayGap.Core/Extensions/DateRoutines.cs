using System;

namespace GenderPayGap.Extensions
{
    /// <summary>
    ///     Miscellaneous and parsing methods for DateTime
    /// </summary>
    public static class DateTimeRoutines
    {

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

    }
}

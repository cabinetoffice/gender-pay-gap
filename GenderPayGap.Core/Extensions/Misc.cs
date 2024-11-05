using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace GenderPayGap.Extensions
{
    public static class Misc
    {

        public static bool IsNull(this object item)
        {
            if (item == null || System.Convert.IsDBNull(item))
            {
                return true;
            }

            return false;
        }

        public static bool IsWrapped<T>(this T[] data, T[] prefix, T[] suffix)
        {
            if (data.Length < prefix.Length + suffix.Length)
            {
                return false;
            }

            T[] end = data.SubArray(0, prefix.Length);

            if (!end.SequenceEqual(prefix))
            {
                return false;
            }

            end = data.SubArray(data.Length - suffix.Length, suffix.Length);

            return end.SequenceEqual(suffix);
        }

        public static T[] Wrap<T>(this T[] data, T[] prefix, T[] suffix)
        {
            var result = new T[data.Length + prefix.Length + suffix.Length];
            Buffer.BlockCopy(prefix, 0, result, 0, prefix.Length);
            Buffer.BlockCopy(data, 0, result, prefix.Length, data.Length);
            Buffer.BlockCopy(suffix, 0, result, prefix.Length + data.Length, suffix.Length);
            return result;
        }

        public static T[] Strip<T>(this T[] data, int left, int right)
        {
            var result = new T[data.Length - (left + right)];
            Buffer.BlockCopy(data, left, result, 0, result.Length);
            return result;
        }

        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            if (length > data.Length)
            {
                length = data.Length;
            }

            var result = new T[length];
            Buffer.BlockCopy(data, index, result, 0, length);
            return result;
        }

        public static bool IsAny(this object item, params object[] values)
        {
            if (item == null && values.Contains(null))
            {
                return true;
            }

            foreach (object value in values)
            {
                if (item.Equals(value))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsAny(this char text, params char[] chars)
        {
            foreach (char ch in chars)
            {
                if (text.Equals(ch))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool ToBoolean(this object text, bool defaultValue = false)
        {
            if (text.IsNull())
            {
                return defaultValue;
            }

            if (text is bool)
            {
                return (bool) text;
            }

            string str = System.Convert.ToString(text);
            if (!string.IsNullOrWhiteSpace(str))
            {
                if (str.EqualsI("1", "yes"))
                {
                    return true;
                }

                if (str.EqualsI("0", "no"))
                {
                    return false;
                }

                bool parsedValue;
                if (bool.TryParse(str, out parsedValue))
                {
                    return parsedValue;
                }
            }

            return defaultValue;
        }

        public static int ToInt32(this object text, int defaultValue = 0)
        {
            if (text.IsNull())
            {
                return defaultValue;
            }

            if (text is decimal || text is double || text is int || text is long || text is byte || text.GetType().IsEnum)
            {
                return System.Convert.ToInt32(text);
            }

            string str = System.Convert.ToString(text);
            int parsedValue;
            if (!string.IsNullOrWhiteSpace(str) && int.TryParse(str, out parsedValue))
            {
                return parsedValue;
            }

            return defaultValue;
        }

        public static long ToInt64(this object text, long defaultValue = 0)
        {
            if (text.IsNull())
            {
                return defaultValue;
            }

            if (text is decimal || text is double || text is int || text is long || text is byte || text.GetType().IsEnum)
            {
                return System.Convert.ToInt64(text);
            }

            string str = System.Convert.ToString(text);
            long parsedValue;
            if (!string.IsNullOrWhiteSpace(str) && long.TryParse(str, out parsedValue))
            {
                return parsedValue;
            }

            return defaultValue;
        }

        public static string ToStringOrNull(this object text)
        {
            string result = null;
            if (text is string)
            {
                result = (string) text;
            }
            else if (!text.IsNull())
            {
                result = System.Convert.ToString(text);
            }

            return string.IsNullOrWhiteSpace(result) ? null : result;
        }

        public static DateTime ToDateTime(this object text)
        {
            if (text.IsNull())
            {
                return DateTime.MinValue;
            }

            if (text is DateTime)
            {
                return (DateTime) text;
            }

            string str = System.Convert.ToString(text);
            if (!string.IsNullOrWhiteSpace(str))
            {
                string shortDateFormat = "yyMMddHHmmss";
                
                DateTime parsedValue;
                if (DateTime.TryParseExact(str, shortDateFormat, null, DateTimeStyles.AssumeLocal, out parsedValue))
                {
                    return parsedValue;
                }

                if (DateTime.TryParseExact(str, shortDateFormat, null, DateTimeStyles.AssumeLocal, out parsedValue))
                {
                    return parsedValue;
                }

                if (DateTime.TryParse(str, out parsedValue))
                {
                    return parsedValue;
                }
            }

            return DateTime.MinValue;
        }

    }
}

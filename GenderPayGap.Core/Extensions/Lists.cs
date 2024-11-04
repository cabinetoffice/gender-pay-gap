using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;

namespace GenderPayGap.Extensions
{
    public static class Lists
    {

        public static string[] SplitI(this string list,
            string separators = ";,",
            int maxItems = 0,
            StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
        {
            if (string.IsNullOrWhiteSpace(list))
            {
                return new string[0];
            }

            if (separators == null)
            {
                throw new ArgumentNullException("separators");
            }

            if (separators == string.Empty)
            {
                return list.ToCharArray().Select(c => c.ToString()).ToArray();
            }

            if (maxItems > 0)
            {
                return list.Split(separators.ToCharArray(), maxItems, options);
            }

            return list.Split(separators.ToCharArray(), options);
        }

        public static IEnumerable<string> UniqueI(this IEnumerable<string> list, bool ignoreCase = true)
        {
            return list.Distinct(ignoreCase ? StringComparer.CurrentCultureIgnoreCase : StringComparer.CurrentCulture);
        }

        public static string ToQueryString(this NameValueCollection collection, bool allowDuplicateKeys = false)
        {
            var data = "";
            if (collection != null)
            {
                var keyValues = new List<KeyValuePair<string, string>>();
                foreach (string key in collection.Keys)
                {
                    if (string.IsNullOrWhiteSpace(collection[key]))
                    {
                        continue;
                    }

                    if (allowDuplicateKeys)
                    {
                        foreach (string value in collection[key].SplitI(","))
                        {
                            keyValues.Add(new KeyValuePair<string, string>(key, value));
                        }
                    }
                    else
                    {
                        keyValues.Add(new KeyValuePair<string, string>(key, collection[key]));
                    }
                }

                foreach (KeyValuePair<string, string> keyValue in keyValues)
                {
                    if (string.IsNullOrWhiteSpace(keyValue.Value))
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(data))
                    {
                        data += "&";
                    }

                    if (string.IsNullOrWhiteSpace(keyValue.Key))
                    {
                        data += keyValue.Value;
                    }
                    else
                    {
                        data += $"{WebUtility.UrlEncode(keyValue.Key)}={keyValue.Value}";
                    }
                }
            }

            return data;
        }

        public static NameValueCollection FromQueryString(this string querystring)
        {
            return string.IsNullOrWhiteSpace(querystring) ? null : HttpUtility.ParseQueryString(querystring);
        }

        public static List<T> ToList<T>(this ICollection collection)
        {
            var list = new List<T>(collection.Count);

            list.AddRange(collection.Cast<T>());

            return list;
        }

        public static List<T> ToListOrEmpty<T>(this IEnumerable<T> collection)
        {
            if (collection == null)
            {
                return new List<T>();
            }

            return collection.ToList();
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            foreach (T item in source)
            {
                action(item);
            }
        }

    }
}

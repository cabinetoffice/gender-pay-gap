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
                        foreach (string value in collection[key].Split(","))
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

    }
}

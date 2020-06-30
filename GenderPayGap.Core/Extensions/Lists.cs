using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Net;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
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

        public static bool ContainsI(this IEnumerable<string> list, params string[] text)
        {
            if (list == null || text == null || text.Length == 0)
            {
                return false;
            }

            List<string> li = list.ToList();
            return text.Any(t => li.Any(l => l.EqualsI(t)));
        }

        public static bool ContainsI(this string source, params string[] list)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return false;
            }

            return list.Any(i => source.ContainsI(i));
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
            return string.IsNullOrWhiteSpace(querystring) ? null : QueryHelpers.ParseNullableQuery(querystring).ToNameValueCollection();
        }

        public static NameValueCollection ToNameValueCollection(this Dictionary<string, StringValues> dictionary)
        {
            var collection = new NameValueCollection();
            foreach (string key in dictionary.Keys)
            {
                if (dictionary[key] == "")
                {
                    collection[null] = key;
                }
                else
                {
                    collection[key] = dictionary[key];
                }
            }

            return collection;
        }

        public static string ToDelimitedString<T>(this IEnumerable<T> list, string delimiter = ",", string appendage = null)
        {
            if (list == null)
            {
                return null;
            }

            string result = null;

            foreach (T item in list)
            {
                if (item == null)
                {
                    continue;
                }

                string text = item.ToString();
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                if (result != null && !string.IsNullOrEmpty(delimiter) && !result.EndsWithI(delimiter))
                {
                    result += delimiter;
                }

                result += text + appendage;
            }

            return result;
        }

        public static string ToEncapsulatedString<T>(this IEnumerable<T> list,
            string prefix,
            string suffix,
            string separator = null,
            string lastSeparator = null,
            bool allowDuplicates = true)
        {
            return list.ToList().ToEncapsulatedString(prefix, suffix, separator, lastSeparator, allowDuplicates);
        }

        public static string ToEncapsulatedString<T>(this List<T> list,
            string prefix,
            string suffix,
            string separator = null,
            string lastSeparator = null,
            bool allowDuplicates = true)
        {
            if (list == null)
            {
                return null;
            }

            string result = null;

            for (var i = 0; i < list.Count; i++)
            {
                T item = list[i];
                string text = item.ToString();
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                if (result != null)
                {
                    result += i == list.Count - 1 ? lastSeparator : separator;
                }

                string str = item.ToString();
                if (allowDuplicates || !str.StartsWithI(prefix))
                {
                    result += prefix;
                }

                result += str;
                if (allowDuplicates || !str.EndsWithI(suffix))
                {
                    result += suffix;
                }
            }

            return result;
        }

        public static SortedSet<T> ToSortedSet<T>(this IEnumerable<T> list)
        {
            if (list == null || !list.Any())
            {
                return new SortedSet<T>();
            }

            return new SortedSet<T>(list);
        }


        public static void AddRange<T>(this HashSet<T> targetCollection, params T[] pars)
        {
            AddRange(targetCollection, pars.AsEnumerable());
        }

        public static void AddRange<T>(this HashSet<T> targetCollection, IEnumerable<T> collection)
        {
            foreach (T item in collection)
            {
                targetCollection.Add(item);
            }
        }

        public static void AddRange<T>(this SortedSet<T> targetCollection, params T[] pars)
        {
            AddRange(targetCollection, pars.AsEnumerable());
        }

        public static void AddRange<T>(this SortedSet<T> targetCollection, IEnumerable<T> collection)
        {
            foreach (T item in collection)
            {
                targetCollection.Add(item);
            }
        }

        public static DataTable ToDataTable<T>(this IEnumerable<T> list, string tableName = null)
        {
            var table = new DataTable(tableName);

            foreach (T item in list)
            {
                var jObject = item as JObject;

                if (jObject == null)
                {
                    jObject = JObject.FromObject(item);
                }

                DataRow row = table.NewRow();

                foreach (KeyValuePair<string, JToken> col in jObject)
                {
                    if (!table.Columns.Contains(col.Key))
                    {
                        table.Columns.Add(col.Key);
                    }

                    row[col.Key] = col.Value;
                }

                table.Rows.Add(row);
            }

            return table;
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

        public static IEnumerable<T> Randomise<T>(this IList<T> list)
        {
            int[] indexes = Enumerable.Range(0, list.Count).ToArray();
            var generator = new Random();

            for (var i = 0; i < list.Count; ++i)
            {
                int position = generator.Next(i, list.Count);

                yield return list[indexes[position]];

                indexes[position] = indexes[i];
            }
        }

        public static IEnumerable<T> Page<T>(this IEnumerable<T> list, int pageSize, int page)
        {
            int skip = (page - 1) * pageSize;
            return list.Skip(skip).Take(pageSize);
        }

        public static IEnumerable<IEnumerable<T>> GetPermutations<T>(this IEnumerable<T> list, int length)
        {
            if (length == 1)
            {
                return list.Select(t => new[] {t});
            }

            return GetPermutations(list, length - 1)
                .SelectMany(
                    t => list.Where(e => !t.Contains(e)),
                    (t1, t2) => t1.Concat(new[] {t2}));
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

        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> e, Func<T, IEnumerable<T>> f)
        {
            if (e == null)
            {
                return null;
            }

            return e.SelectMany(c => f(c).Flatten(f)).Concat(e);
        }

    }

    [Serializable]
    public class NameValueSetting
    {

        public NameValueSetting() { }

        public NameValueSetting(string name, string value)
        {
            Name = name;
            Value = value;
        }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }

    }

    [Serializable]
    public class NameValueElement
    {

        public NameValueElement() { }

        public NameValueElement(string name, string value)
        {
            Name = name;
            Value = value;
        }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlText]
        public string Value { get; set; }

    }
}

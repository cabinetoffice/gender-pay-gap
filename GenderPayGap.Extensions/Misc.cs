using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace GenderPayGap.Extensions
{
    public static class Misc
    {

        public static Assembly GetTopAssembly()
        {
            return Assembly.GetEntryAssembly();
        }

        public static async Task WaitForAllAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, Task> method)
        {
            await Task.WhenAll(source.Select(async s => await method(s)));
        }

        public static TValue GetAttributeValue<TAttribute, TValue>(this Type type, Func<TAttribute, TValue> valueSelector)
            where TAttribute : Attribute
        {
            var att = type.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() as TAttribute;
            if (att != null)
            {
                return valueSelector(att);
            }

            return default;
        }

        public static string CorrectNull(this object text)
        {
            var str = text as string;
            if (string.IsNullOrWhiteSpace(str) || str.EqualsI("null"))
            {
                return null;
            }

            return str;
        }

        public static bool IsNull(this object item)
        {
            if (item == null || System.Convert.IsDBNull(item))
            {
                return true;
            }

            return false;
        }

        public static bool IsEnumerable(this object list)
        {
            var enumerable = list as IEnumerable;
            return enumerable != null;
        }

        public static DateTime GetAssemblyCreationTime(this Assembly assembly)
        {
            string filePath = assembly.Location;

            return File.GetCreationTime(filePath);
            ;
        }

        public static string GetAssemblyCopyright(this Assembly assembly)
        {
            FileVersionInfo version = FileVersionInfo.GetVersionInfo(assembly.Location);

            return version.LegalCopyright;
        }


        public static string Resolve(this object obj, string text)
        {
            //Bind the parameters
            if (obj != null && !string.IsNullOrWhiteSpace(text))
            {
                foreach (PropertyInfo prop in obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    string value = prop.GetValue(obj, null)?.ToString();
                    if (string.IsNullOrWhiteSpace(prop.Name) || string.IsNullOrWhiteSpace(value))
                    {
                        continue;
                    }

                    text = text.ReplaceI("{" + prop.Name + "}", value);
                }
            }

            return text;
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


        public static bool EqualsI(this object item, params object[] values)
        {
            if (item == null && values.Contains(null))
            {
                return true;
            }

            if (item != null && values != null)
            {
                foreach (object value in values)
                {
                    if (item.Equals(value))
                    {
                        return true;
                    }
                }
            }

            return false;
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

        public static long ToLong(this object text, long defaultValue = 0)
        {
            return ToInt64(text, defaultValue);
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

        public static string FormatDecimal(this decimal? value, string format)
        {
            if (value == null)
            {
                value = default(decimal);
            }

            return value.Value.ToString(format);
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

        public static string ToStringOrEmpty(this object text)
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

            return string.IsNullOrWhiteSpace(result) ? string.Empty : result;
        }

        public static string ToStringOr(this object text, string replacement)
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

            return string.IsNullOrWhiteSpace(result) ? replacement : result;
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
                DateTime parsedValue;
                if (DateTime.TryParseExact(str, Time.ShortDateFormat, null, DateTimeStyles.AssumeLocal, out parsedValue))
                {
                    return parsedValue;
                }

                if (DateTime.TryParseExact(str, Time.ShortDateFormat, null, DateTimeStyles.AssumeLocal, out parsedValue))
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

        public static bool IsSimpleType(this Type type)
        {
            return
                type.IsValueType
                || type.IsPrimitive
                || new[] {typeof(string), typeof(decimal), typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan), typeof(Guid)}
                    .Contains(type)
                || System.Convert.GetTypeCode(type) != TypeCode.Object;
        }

        public static object CopyProperties(this object source, object target)
        {
            Type targetType = target.GetType();
            foreach (PropertyInfo sourceProperty in source.GetType().GetProperties())
            {
                MethodInfo propGetter = sourceProperty.GetGetMethod();
                PropertyInfo targetProperty = targetType.GetProperty(sourceProperty.Name);
                if (targetProperty == null)
                {
                    continue;
                }

                MethodInfo propSetter = targetProperty.GetSetMethod();
                // check the property has a setter
                if (propSetter != null)
                {
                    object valueToSet = propGetter.Invoke(source, null);
                    propSetter.Invoke(target, new[] {valueToSet});
                }
            }

            return target;
        }

        public static void SetProperty(this object source, string propName, object valueToSet)
        {
            PropertyInfo sourceProperty = source.GetType().GetProperty(propName);
            MethodInfo propSetter = sourceProperty.GetSetMethod();
            propSetter.Invoke(source, new[] {valueToSet});
        }

        public static object GetPropertyValue(object Object, string PropertyName)
        {
            try
            {
                PropertyInfo myInfo = Object.GetType().GetPropertyInfo(PropertyName);
                return myInfo.GetValue(Object, null);
            }
            catch { }

            return null;
        }

        public static PropertyInfo GetPropertyInfo(this Type type, string propertyName)
        {
            try
            {
                int i = propertyName.IndexOf(".");
                if (i > -1)
                {
                    return type.GetType().GetProperty(propertyName.Substring(0, i));
                }

                return type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }
            catch { }

            return null;
        }

        public static Dictionary<string, object> ToPropertyDictionary(this object obj,
            StringComparer comparer = null,
            bool newIfEmpty = false)
        {
            if (comparer == null)
            {
                comparer = StringComparer.CurrentCultureIgnoreCase;
            }

            if (obj == null || obj.IsEnumerable())
            {
                return newIfEmpty ? new Dictionary<string, object>(comparer) : null;
            }

            return obj.GetType()
                .GetProperties()
                .Where(p => !p.GetIndexParameters().Any())
                .ToDictionary(x => x.Name.Replace('_', '-'), x => x.GetValue(obj, null), comparer);
        }

        public static bool SetPropertyValue(object obj, string propertyName, object value, bool recursive = false)
        {
            PropertyInfo myInfo = null;
            Type myType = obj.GetType();
            try
            {
                myInfo = GetPropertyInfo(myType, propertyName);
                if (myInfo == null)
                {
                    if (!recursive)
                    {
                        return false;
                    }

                    PropertyInfo[] properties = myType.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        object propValue = property.GetValue(obj, null);
                        if (IsSimpleType(property.PropertyType)) { }
                        else if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                        {
                            var enumerable = (IEnumerable) propValue;
                            foreach (object child in enumerable)
                            {
                                if (SetPropertyValue(child, propertyName, value, recursive))
                                {
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            if (SetPropertyValue(propValue, propertyName, value, recursive))
                            {
                                return true;
                            }
                        }
                    }
                }

                myInfo.SetValue(obj, value, null);
                return true;
            }
            catch (Exception ex)
            {
                string m = ex.Message;
            }

            return false;
        }


        public static void FormatDecimals(this object obj)
        {
            // Loop through all properties
            foreach (PropertyInfo p in obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                object value = p.GetValue(obj);
                if (value == null)
                {
                    continue;
                }

                Type type = value.GetType();
                if (type != typeof(decimal))
                {
                    continue;
                }

                // for every property loop through all attributes
                foreach (DisplayFormatAttribute a in p.GetCustomAttributes(typeof(DisplayFormatAttribute), false))
                {
                    if (string.IsNullOrWhiteSpace(a.DataFormatString))
                    {
                        continue;
                    }

                    p.SetValue(obj, decimal.Parse(string.Format(a.DataFormatString, value)));
                }
            }
        }

        public static MethodBase FindParentWithAttribute<T>(this MethodBase callingMethod, int parentOffset = 0) where T : Attribute
        {
            // Iterate throught all attributes
            StackFrame[] frames = new StackTrace().GetFrames();

            for (int i = 1 + parentOffset; i < frames.Length; i++)
            {
                StackFrame frame = frames[i];
                if (frame.HasMethod())
                {
                    MethodBase method = frame.GetMethod();

                    if (method.GetCustomAttribute<T>() != null)
                    {
                        return method;
                    }
                }
            }

            return callingMethod;
        }

    }
}

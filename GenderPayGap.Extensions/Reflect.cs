using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GenderPayGap.Extensions
{
    public static class Reflect
    {

        public static Dictionary<string, object> GetPropertiesDictionary(this object source,
            BindingFlags filterFlags = BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance)
        {
            return source.GetType()
                .GetProperties(filterFlags)
                .Select(prop => new {Key = prop.Name, Value = prop.GetValue(source, null)})
                // ignore null values
                .Where(prop => prop.Value != null)
                // convert to dictionary
                .ToDictionary(prop => prop.Key, prop => prop.Value);
        }

        public static R GetProperty<R>(this object obj, string property)
        {
            PropertyInfo value = obj.GetType().GetProperty(property);
            return (R) value.GetValue(obj);
        }

    }
}

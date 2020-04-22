using System.Reflection;

namespace GenderPayGap.Extensions
{
    public static class Reflect
    {

        public static R GetProperty<R>(this object obj, string property)
        {
            PropertyInfo value = obj.GetType().GetProperty(property);
            return (R) value.GetValue(obj);
        }

    }
}

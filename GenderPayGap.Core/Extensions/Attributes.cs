using System;
using System.Linq;
using System.Reflection;

namespace GenderPayGap.Extensions
{

    public static class Attributes
    {

        // Enum
        public static TAttribute GetAttribute<TAttribute>(this Enum value) where TAttribute : Attribute
        {
            return value.GetType()
                .GetMember(value.ToString())
                .FirstOrDefault()
                ?.GetCustomAttribute<TAttribute>();
        }

    }

}

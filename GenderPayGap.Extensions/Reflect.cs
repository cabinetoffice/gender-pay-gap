using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace GenderPayGap.Extensions
{
    public static class Reflect
    {

        public static R GetProperty<R>(this object obj, string property)
        {
            PropertyInfo value = obj.GetType().GetProperty(property);
            return (R) value.GetValue(obj);
        }

        public static bool IsAsyncMethod(this MethodInfo method)
        {
            // Obtain the custom attribute for the method. 
            // The value returned contains the StateMachineType property. 
            // Null is returned if the attribute isn't present for the method. 
            var attrib = (AsyncStateMachineAttribute)method.GetCustomAttribute(typeof(AsyncStateMachineAttribute));

            return attrib != null;
        }

    }
}

using System;
using System.Reflection;
using GovUkDesignSystem.Helpers;

namespace GovUkDesignSystem.Attributes.ValidationAttributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class GovUkValidationAttribute : Attribute
    {

        public abstract bool CheckForValidationErrors<TProperty>(
            GovUkViewModel model,
            PropertyInfo property,
            TProperty parameterValue);

        protected static T GetAttribute<T>(MemberInfo property) where T: Attribute
        {
            return property.GetSingleCustomAttribute<T>();
        }

    }
}

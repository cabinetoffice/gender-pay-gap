using System;
using System.Reflection;

namespace GovUkDesignSystem.Attributes.ValidationAttributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class GovUkValidationAttribute : Attribute
    {

        public abstract bool CheckForValidationErrors<TProperty>(
            GovUkViewModel model,
            PropertyInfo property,
            TProperty parameterValue);

    }
}

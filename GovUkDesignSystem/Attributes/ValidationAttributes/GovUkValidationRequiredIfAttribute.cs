using System;
using System.Reflection;
using GovUkDesignSystem.Helpers;

namespace GovUkDesignSystem.Attributes.ValidationAttributes
{
    public class GovUkValidationRequiredIfAttribute: Attribute
    {

        public string TargetPropertyName { get; set; }

        public object TargetPropertyDesiredValue { get; set; }
        
        public string ErrorMessageIfMissing { get; set;  }

        public GovUkValidationRequiredIfAttribute(string targetPropertyName, object targetPropertyDesiredValue)
        {
            TargetPropertyName = targetPropertyName;
            TargetPropertyDesiredValue = targetPropertyDesiredValue;
        }
        
        public GovUkValidationRequiredIfAttribute(string targetPropertyName, object targetPropertyDesiredValue, string errorMessageIfMissing)
        {
            TargetPropertyName = targetPropertyName;
            TargetPropertyDesiredValue = targetPropertyDesiredValue;
            ErrorMessageIfMissing = errorMessageIfMissing;
        }

        public static bool RequiredIfConditionIsMet(GovUkViewModel model, PropertyInfo property)
        {
            var attribute = GetAttribute<GovUkValidationRequiredIfAttribute>(property);
            object targetPropertyValue = GetTargetPropertyValue(model, attribute.TargetPropertyName);
            return ActualValueIsEqualToDesiredValue(targetPropertyValue, attribute.TargetPropertyDesiredValue);
        }

        private static object GetTargetPropertyValue<T>(T source, string propertyName)
        {
            return source.GetType().GetProperty(propertyName)?.GetValue(source);
        }

        private static bool ActualValueIsEqualToDesiredValue(object actualValue, object desiredValue)
        {
            return actualValue?.ToString() == desiredValue?.ToString();
        }
        
        private static T GetAttribute<T>(MemberInfo property) where T : Attribute
        {
            return property.GetSingleCustomAttribute<T>();
        }
    }
}

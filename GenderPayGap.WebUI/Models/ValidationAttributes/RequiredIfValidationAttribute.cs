using System;
using System.ComponentModel.DataAnnotations;

namespace GenderPayGap.WebUI.Views.Models.ValidationAttributes
{
    public class RequiredIfAttribute : RequiredAttribute
    {

        private string PropertyName { get; set; }
        private object DesiredValue { get; set; }

        public RequiredIfAttribute(string propertyName, object desiredValue)
        {
            PropertyName = propertyName;
            DesiredValue = desiredValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            object instance = context.ObjectInstance;
            Type type = instance.GetType();
            object actualValue = type.GetProperty(PropertyName)?.GetValue(instance, null);
            return actualValue?.ToString() == DesiredValue.ToString() ? base.IsValid(value, context) : ValidationResult.Success;
        }

    }
}

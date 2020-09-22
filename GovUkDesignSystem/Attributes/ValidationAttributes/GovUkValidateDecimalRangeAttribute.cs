using System;
using System.Reflection;
using GovUkDesignSystem.Helpers;

namespace GovUkDesignSystem.Attributes.ValidationAttributes
{
    public class GovUkValidateDecimalRangeAttribute : GovUkValidationAttribute
    {

        public double Minimum { get; set; }
        public double Maximum { get; set; }
        
        public override bool CheckForValidationErrors<TProperty>(
            GovUkViewModel model,
            PropertyInfo property,
            TProperty parameterValue)
        {
            
            if (typeof(TProperty) != typeof(decimal))
            {
                throw new Exception("Paramater value has the wrong type");
            }

            var value = parameterValue as decimal?;

            if (value.HasValue)
            {
                if (ValueIsOutOfRange(property, value.Value))
                {
                    AddValueIsOutOfRangeErrorMessage(model, property);
                    return false;
                }
            }

            return true;
        }

        private static bool ValueIsOutOfRange(PropertyInfo property, decimal value)
        {
            var decimalRangeAttribute = property.GetSingleCustomAttribute<GovUkValidateDecimalRangeAttribute>();

            bool decimalRangeInForce = decimalRangeAttribute != null;

            if (decimalRangeInForce)
            {
                decimal minimumAllowed = (decimal) decimalRangeAttribute.Minimum;
                decimal maximumAllowed = (decimal) decimalRangeAttribute.Maximum;

                bool outOfRange = value < minimumAllowed || value > maximumAllowed;
                return outOfRange;
            }

            return false;
        }

        private static void AddValueIsOutOfRangeErrorMessage(GovUkViewModel model, PropertyInfo property)
        {
            var decimalRangeAttribute = property.GetSingleCustomAttribute<GovUkValidateDecimalRangeAttribute>();

            decimal minimum = (decimal) decimalRangeAttribute.Minimum;
            decimal maximum = (decimal) decimalRangeAttribute.Maximum;

            ParserHelpers.AddErrorMessageBasedOnPropertyDisplayName(model, property,
                name => $"{name} must be between {minimum} and {maximum}",
                ErrorMessagePropertyNamePosition.StartOfMessage);
        }

    }
}

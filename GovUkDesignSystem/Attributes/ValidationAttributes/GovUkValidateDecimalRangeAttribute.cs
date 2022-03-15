using System;
using System.Reflection;
using GovUkDesignSystem.Helpers;

namespace GovUkDesignSystem.Attributes.ValidationAttributes
{
    public class GovUkValidateDecimalRangeAttribute : GovUkValidationAttribute
    {

        public double Minimum { get; set; }
        public double Maximum { get; set; }
        public string CustomErrorMessage { get; set; }
        
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
                decimal minimumAllowed = SafelyConvertDoubleToDecimal(decimalRangeAttribute.Minimum);
                decimal maximumAllowed = SafelyConvertDoubleToDecimal(decimalRangeAttribute.Maximum);

                bool outOfRange = value < minimumAllowed || value > maximumAllowed;
                return outOfRange;
            }

            return false;
        }

        private static void AddValueIsOutOfRangeErrorMessage(GovUkViewModel model, PropertyInfo property)
        {
            var decimalRangeAttribute = property.GetSingleCustomAttribute<GovUkValidateDecimalRangeAttribute>();

            decimal minimum = SafelyConvertDoubleToDecimal(decimalRangeAttribute.Minimum);
            decimal maximum = SafelyConvertDoubleToDecimal(decimalRangeAttribute.Maximum);
            string customErrorMessage = decimalRangeAttribute.CustomErrorMessage;

            ParserHelpers.AddErrorMessageBasedOnPropertyDisplayName(model, property,
                name => customErrorMessage ?? $"{name} must be between {minimum} and {maximum}",
                ErrorMessagePropertyNamePosition.StartOfMessage);
        }

        private static decimal SafelyConvertDoubleToDecimal(double value)
        {
            // We do this because converting the lowest/highest possible value for a decimal into a double
            // causes the resulting double to be just over what a decimal can store,
            // hence making conversion back to a decimal impossible.

            if (value >= (double) decimal.MaxValue)
            {
                return decimal.MaxValue;
            }

            if (value <= (double) decimal.MinValue)
            {
                return decimal.MinValue;
            }
            
            return Convert.ToDecimal(value);
        }

    }
}

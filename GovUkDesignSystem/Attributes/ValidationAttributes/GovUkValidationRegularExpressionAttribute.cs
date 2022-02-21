using System.Reflection;
using System.Text.RegularExpressions;

namespace GovUkDesignSystem.Attributes.ValidationAttributes
{
    public class GovUkValidationRegularExpressionAttribute: GovUkValidationAttribute
    {
        
        public string Pattern { get; set;  }
        
        public string ErrorMessage { get; set; }

        public GovUkValidationRegularExpressionAttribute(string pattern, string errorMessage)
        {
            Pattern = pattern;
            ErrorMessage = errorMessage;
        }

        public override bool CheckForValidationErrors<TProperty>(GovUkViewModel model, PropertyInfo property, TProperty parameterValue)
        {
            var attribute = GetAttribute<GovUkValidationRegularExpressionAttribute>(property);

            var value = parameterValue?.ToString();
            bool matches = Matches(attribute.Pattern, value);

            if (matches)
            {
                return true;
            }

            model.AddErrorFor(property, attribute.ErrorMessage);
            return false;

        }

        private bool Matches(string regexString, string value)
        {
            var regex = new Regex(regexString);
            // Automatically pass if value is null or empty. RequiredAttribute should be used to assert a value is not empty.
            if (string.IsNullOrEmpty(value))
            {
                return true;
            }

            // We are looking for an exact match, not just a search hit. This matches what
            // the RegularExpressionValidator control does
            Match match = regex.Match(value);
            return match.Success && match.Index == 0 && match.Length == value.Length;
        }
    }
}

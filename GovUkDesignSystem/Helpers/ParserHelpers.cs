using System;
using System.Reflection;
using GovUkDesignSystem.Attributes;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace GovUkDesignSystem.Helpers
{
    public static class ParserHelpers
    {

        public static void ThrowIfMoreThanOneValue(StringValues parameterValues, PropertyInfo property)
        {
            if (parameterValues.Count > 1)
            {
                throw new ArgumentException(
                    $"This property should only be able to send 1 value at a time, " +
                    $"but we just received [{parameterValues.Count}] values [{parameterValues}] " +
                    $"for property [{property.Name}] on type [{property.DeclaringType.FullName}]"
                );
            }
        }

        public static void SaveUnparsedValueFromRequestToModel(GovUkViewModel model, HttpRequest httpRequest, string parameterName)
        {
            StringValues unparsedValuesFromRequestForThisProperty = HttpRequestParameterHelper.GetRequestParameter(httpRequest, parameterName);

            model.AddUnparsedValues(parameterName, unparsedValuesFromRequestForThisProperty);
        }


        public static bool IsValueRequiredAndMissing(PropertyInfo property, StringValues parameterValues, GovUkViewModel model)
        {
            bool valueIsRequired = IsValueRequired(property, model);
            bool valueIsMissing = parameterValues.Count == 0;

            return (valueIsRequired && valueIsMissing);
        }

        public static bool IsValueRequiredAndMissingOrEmpty(PropertyInfo property, StringValues parameterValues, GovUkViewModel model)
        {
            bool valueIsRequired = IsValueRequired(property, model);
            bool valueIsMissing = parameterValues.Count == 0 || string.IsNullOrWhiteSpace(parameterValues[0]);

            return valueIsRequired && valueIsMissing;
        }

        public static void AddRequiredAndMissingErrorMessage(GovUkViewModel model, PropertyInfo property)
        {
            var requiredAttribute = property.GetSingleCustomAttribute<GovUkValidateRequiredAttribute>();
            var requiredIfAttribute = property.GetSingleCustomAttribute<GovUkValidationRequiredIfAttribute>();
            
            var displayNameForErrorsAttribute = property.GetSingleCustomAttribute<GovUkDisplayNameForErrorsAttribute>();

            string requiredErrorMessage = requiredAttribute != null
                ? requiredAttribute.ErrorMessageIfMissing
                : requiredIfAttribute?.ErrorMessageIfMissing;

            string errorMessage;
            if (requiredErrorMessage != null)
            {
                errorMessage = requiredErrorMessage;
            }
            else if (displayNameForErrorsAttribute != null)
            {
                errorMessage = $"{displayNameForErrorsAttribute.NameAtStartOfSentence} is required";
            }
            else
            {
                errorMessage = $"{property.Name} is required";
            }

            model.AddErrorFor(property, errorMessage);
        }

        public static void AddErrorMessageBasedOnPropertyDisplayName(
            GovUkViewModel model,
            PropertyInfo property,
            Func<string, string> getErrorMessageBasedOnPropertyDisplayName,
            ErrorMessagePropertyNamePosition position)
        {
            var displayNameForErrorsAttribute = property.GetSingleCustomAttribute<GovUkDisplayNameForErrorsAttribute>();

            string errorMessage;
            if (displayNameForErrorsAttribute != null)
            {
                switch (position)
                {
                    case ErrorMessagePropertyNamePosition.StartOfMessage:
                        errorMessage = getErrorMessageBasedOnPropertyDisplayName(displayNameForErrorsAttribute.NameAtStartOfSentence);
                        break;
                    case ErrorMessagePropertyNamePosition.WithinMessage:
                        errorMessage = getErrorMessageBasedOnPropertyDisplayName(displayNameForErrorsAttribute.NameWithinSentence);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(position), position, null);
                }
            }
            else
            {
                errorMessage = getErrorMessageBasedOnPropertyDisplayName(property.Name);
            }

            model.AddErrorFor(property, errorMessage);
        }
        
        private static bool IsValueRequired(PropertyInfo property, GovUkViewModel model)
        {
            var requiredAttribute = property.GetSingleCustomAttribute<GovUkValidateRequiredAttribute>();
            var requiredIfAttribute = property.GetSingleCustomAttribute<GovUkValidationRequiredIfAttribute>();
            
            bool hasRequiredAttribute = requiredAttribute != null;
            bool hasRequiredIfAttribute = requiredIfAttribute != null;

            bool requiredIfAttributeConditionIsMet =
                hasRequiredIfAttribute && GovUkValidationRequiredIfAttribute.RequiredIfConditionIsMet(model, property);

            return hasRequiredAttribute || requiredIfAttributeConditionIsMet;
        }

    }

    public enum ErrorMessagePropertyNamePosition
    {
        StartOfMessage,
        WithinMessage
    }
}

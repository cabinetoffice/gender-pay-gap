using System;
using System.Reflection;
using GovUkDesignSystem.Helpers;

namespace GovUkDesignSystem.Attributes.ValidationAttributes
{
    public class GovUkValidateCharacterCountAttribute : GovUkValidationAttribute
    {

        public int MaxCharacters { get; set; }
        
        public override bool CheckForValidationErrors<TProperty>(
            GovUkViewModel model,
            PropertyInfo property,
            TProperty parameterValue)
        {
            
            if (typeof(TProperty) != typeof(string))
            {
                throw new Exception("Paramater value has the wrong type");
            }

            var value = parameterValue as string;
            
            if (ExceedsCharacterCount(property, value))
            {
                AddExceedsCharacterCountErrorMessage(model, property);
                return false;
            }

            return true;
        }

        private static void AddExceedsCharacterCountErrorMessage(GovUkViewModel model, PropertyInfo property)
        {
            var characterCountAttribute = property.GetSingleCustomAttribute<GovUkValidateCharacterCountAttribute>();
            var displayNameForErrorsAttribute = property.GetSingleCustomAttribute<GovUkDisplayNameForErrorsAttribute>();

            string errorMessage;
            if (displayNameForErrorsAttribute != null)
            {
                errorMessage =
                    $"{displayNameForErrorsAttribute.NameAtStartOfSentence} must be {characterCountAttribute.MaxCharacters} characters or fewer";
            }
            else
            {
                errorMessage = $"{property.Name} must be {characterCountAttribute.MaxCharacters} characters or fewer";
            }

            model.AddErrorFor(property, errorMessage);
        }

        private static bool ExceedsCharacterCount(PropertyInfo property, string parameterValue)
        {
            var characterCountAttribute = property.GetSingleCustomAttribute<GovUkValidateCharacterCountAttribute>();

            bool characterCountInForce = characterCountAttribute != null;

            if (characterCountInForce)
            {
                int parameterLength = parameterValue.Length;
                int maximumLength = characterCountAttribute.MaxCharacters;

                bool exceedsCharacterCount = parameterLength > maximumLength;
                return exceedsCharacterCount;
            }

            return false;
        }

    }
}

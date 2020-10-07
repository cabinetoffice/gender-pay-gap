using System;
using System.Linq;
using System.Reflection;

namespace GovUkDesignSystem.Attributes.ValidationAttributes
{
    public class GpgPasswordValidationAttribute : GovUkValidationAttribute
    {

        public override bool CheckForValidationErrors<TProperty>(
            GovUkViewModel model,
            PropertyInfo property,
            TProperty parameterValue)
        {
            if (typeof(TProperty) != typeof(string))
            {
                throw new Exception("Parameter value has the wrong type");
            }

            var value = parameterValue as string;
            
            if (value.Length < 8)
            {
                model.AddErrorFor(property, "Password must be at least 8 characters long");
                return false;
            }

            if (!value.Any(char.IsLower) || !value.Any(char.IsUpper) || !value.Any(char.IsNumber))
            {
                model.AddErrorFor(property, "Password must have at least one lower case letter, one capital letter and one number");
                return false;
            }

            if (value.Contains("password"))
            {
                model.AddErrorFor(property, "Enter a password that doesn't contain 'password'");
                return false;
            }

            return true;
        }

    }
}

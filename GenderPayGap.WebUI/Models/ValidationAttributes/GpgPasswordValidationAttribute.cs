using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace GovUkDesignSystem.Attributes.ValidationAttributes
{
    public class GpgPasswordValidationAttribute : ValidationAttribute
    {

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var password = value as string;
            if (password is null)
            {
                return ValidationResult.Success;
            }
            if (password.Length < 8)
            {
                return new ValidationResult("Password must be at least 8 characters long");
            }

            if (!password.Any(char.IsLower) || !password.Any(char.IsUpper) || !password.Any(char.IsNumber))
            {
                return new ValidationResult("Password must have at least one lower case letter, one capital letter and one number");
            }

            if (password.Contains("password"))
            {
                return new ValidationResult("Enter a password that doesn't contain 'password'");
            }

            return ValidationResult.Success;
        }
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace GenderPayGap.WebUI.Classes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class PasswordAttribute : RegularExpressionAttribute
    {

        public PasswordAttribute() : base("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)[A-Za-z\\d\\W]{8,}$")
        {
            ErrorMessage = "Password must contain at least one upper case, 1 lower case character and 1 digit";
        }

    }
}

using System;
using System.ComponentModel.DataAnnotations;
using GenderPayGap.Core;

namespace GenderPayGap.WebUI.Classes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class PasswordAttribute : RegularExpressionAttribute
    {

        public PasswordAttribute() : base(Global.PasswordRegex)
        {
            ErrorMessage = Global.PasswordRegexError;
        }

    }
}

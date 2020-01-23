using System;
using System.ComponentModel.DataAnnotations;
using GenderPayGap.Core;

namespace GenderPayGap.WebUI.Classes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class PinAttribute : RegularExpressionAttribute
    {

        public PinAttribute() : base(Global.PinRegex)
        {
            ErrorMessage = Global.PinRegexError;
        }

    }
}

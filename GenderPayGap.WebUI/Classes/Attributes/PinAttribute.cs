using System;
using System.ComponentModel.DataAnnotations;
using GenderPayGap.Core;

namespace GenderPayGap.WebUI.Classes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class PinAttribute : RegularExpressionAttribute
    {

        public PinAttribute() : base("^[A-Za-z0-9]{7}$")
        {
            ErrorMessage = "PIN code must contain 7 alpha or numeric characters";
        }

    }
}

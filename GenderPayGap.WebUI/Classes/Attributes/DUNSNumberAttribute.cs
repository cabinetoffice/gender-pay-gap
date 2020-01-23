using System;
using System.ComponentModel.DataAnnotations;
using GenderPayGap.Core;

namespace GenderPayGap.WebUI.Classes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class DUNSNumberAttribute : RegularExpressionAttribute
    {

        private const string pattern = @"^[0-9]{9}$";

        public DUNSNumberAttribute() : base(pattern)
        {
            ErrorMessage = Global.CompanyNumberRegexError;
        }

    }
}

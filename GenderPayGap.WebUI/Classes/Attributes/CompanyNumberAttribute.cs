using System;
using System.ComponentModel.DataAnnotations;
using GenderPayGap.Core;

namespace GenderPayGap.WebUI.Classes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class CompanyNumberAttribute : RegularExpressionAttribute
    {

        private const string pattern = @"^[0-9A-Za-z]{8}$";

        public CompanyNumberAttribute() : base(pattern)
        {
            ErrorMessage = "Company number must contain 8 characters only";
        }

    }
}

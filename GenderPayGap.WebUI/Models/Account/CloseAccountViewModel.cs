﻿using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Account
{
    public class CloseAccountViewModel 
    {

        [GovUkValidateRequired(ErrorMessageIfMissing = "Enter your password")]
        public string Password { get; set; }
        
        [BindNever]
        public bool IsSoleUserRegisteredToAnOrganisation { get; set; }

    }
}

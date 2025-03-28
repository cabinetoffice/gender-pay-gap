﻿using GovUkDesignSystem.Attributes.ValidationAttributes;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminImpersonateUserViewModel 
    {
        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a valid email address")]
        public string EmailAddress { get; set; }
    }
}

﻿using GovUkDesignSystem.Attributes.ValidationAttributes;

namespace GenderPayGap.WebUI.Models.Account
{
    public class ChangeEmailViewModel 
    {

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a new email address")]
        public string NewEmailAddress { get; set; }

    }
}

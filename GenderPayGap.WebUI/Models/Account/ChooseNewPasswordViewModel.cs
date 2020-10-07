using GenderPayGap.Database;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Account
{
    public class ChooseNewPasswordViewModel : GovUkViewModel
    {
        public string ResetCode { get; set; }

        [GpgPasswordValidation]
        [GovUkValidateRequired(ErrorMessageIfMissing = "Enter a new password")]
        public string NewPassword { get; set; }
        
        [GovUkValidateRequired(ErrorMessageIfMissing = "Confirm your new password")]
        public string ConfirmNewPassword { get; set; }

    }
}

using GovUkDesignSystem.Attributes.ValidationAttributes;

namespace GenderPayGap.WebUI.Models.Account
{
    public class ChooseNewPasswordViewModel 
    {
        public string ResetCode { get; set; }

        [GpgPasswordValidation]
        [GovUkValidateRequired(ErrorMessageIfMissing = "Enter a new password")]
        public string NewPassword { get; set; }
        
        [GovUkValidateRequired(ErrorMessageIfMissing = "Confirm your new password")]
        public string ConfirmNewPassword { get; set; }

    }
}

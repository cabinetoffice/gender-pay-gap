using GenderPayGap.Database;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;

namespace GenderPayGap.WebUI.Models.Account
{
    public class ChooseNewPasswordViewModel : GovUkViewModel
    {
        public User User { get; set; }

        [GpgPasswordValidation]
        [GovUkValidateRequired(ErrorMessageIfMissing = "Enter a new password")]
        public string NewPassword { get; set; }
        
        [GovUkValidateRequired(ErrorMessageIfMissing = "Confirm your new password")]
        public string ConfirmNewPassword { get; set; }

    }
}

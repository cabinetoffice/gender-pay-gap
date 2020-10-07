using System.ComponentModel.DataAnnotations;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;

namespace GenderPayGap.WebUI.Models.Account
{
    public class ChangePasswordNewViewModel : GovUkViewModel
    {

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter your current password")]
        public string CurrentPassword { get; set; }
        
        [GpgPasswordValidation]
        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a new password")]
        public string NewPassword { get; set; }
        
        [GovUkValidateRequired(ErrorMessageIfMissing = "Please confirm your new password")]
        public string ConfirmNewPassword { get; set; }

    }
}

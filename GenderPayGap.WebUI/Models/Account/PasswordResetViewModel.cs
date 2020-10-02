using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;

namespace GenderPayGap.WebUI.Models.Account
{
    public class PasswordResetViewModel : GovUkViewModel
    {

        [GovUkValidateRequired(ErrorMessageIfMissing = "Enter your email address")]
        public string EmailAddress { get; set; }

    }
}

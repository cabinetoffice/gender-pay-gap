using GovUkDesignSystem.Attributes.ValidationAttributes;

namespace GenderPayGap.WebUI.Models.Login
{
    public class LoginViewModel 
    {

        public string ReturnUrl { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter an email address.")]
        public string EmailAddress { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a password.")]
        public string Password { get; set; }

    }
}

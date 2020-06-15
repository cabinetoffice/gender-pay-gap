using GenderPayGap.Database;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;

namespace GenderPayGap.WebUI.Models.Account
{
    public class VerifyEmailChangeViewModel : GovUkViewModel
    {

        #region Only used to send data to the view
        public User User { get; set; }

        public string NewEmailAddress { get; set; }
        #endregion


        public string Code { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter your password")]
        public string Password { get; set; }

    }
}

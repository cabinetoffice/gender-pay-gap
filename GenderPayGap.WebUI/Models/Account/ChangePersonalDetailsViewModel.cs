using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;

namespace GenderPayGap.WebUI.Models.Account
{
    public class ChangePersonalDetailsViewModel : GovUkViewModel
    {
        [GovUkValidateRequired(ErrorMessageIfMissing = "Enter your first name")]
        public string FirstName { get; set; }
        
        [GovUkValidateRequired(ErrorMessageIfMissing = "Enter your last name")]
        public string LastName { get; set; }
        
        [GovUkValidateRequired(ErrorMessageIfMissing = "Enter your job title")]
        public string JobTitle { get; set; }

        public string ContactPhoneNumber { get; set; }

    }
}

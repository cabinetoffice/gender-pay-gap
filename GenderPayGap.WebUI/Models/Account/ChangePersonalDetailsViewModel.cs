using System.Dynamic;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;

namespace GenderPayGap.WebUI.Models.Account
{
    public class ChangePersonalDetailsViewModel : GovUkViewModel
    {
        [GovUkValidateRequired(ErrorMessageIfMissing = "You need to enter your first name")]
        public string FirstName { get; set; }
        
        [GovUkValidateRequired(ErrorMessageIfMissing = "You need to enter your last name")]
        public string LastName { get; set; }
        
        [GovUkValidateRequired(ErrorMessageIfMissing = "You need to enter your job title")]
        public string JobTitle { get; set; }

        public string ContactPhoneNumber { get; set; }

    }
}

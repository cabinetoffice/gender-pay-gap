using GenderPayGap.Database;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Account
{
    public class VerifyEmailChangeViewModel 
    {

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public User User { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public string NewEmailAddress { get; set; }


        public string Code { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter your password")]
        public string Password { get; set; }

    }
}

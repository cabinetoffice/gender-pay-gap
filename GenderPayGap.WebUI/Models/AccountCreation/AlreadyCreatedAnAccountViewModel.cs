using GovUkDesignSystem;
using GovUkDesignSystem.Attributes;

namespace GenderPayGap.WebUI.Models.AccountCreation
{

    public class AlreadyCreatedAnAccountViewModel 
    {

        public HaveYouAlreadyCreatedYourUserAccount? HaveYouAlreadyCreatedYourUserAccount { get; set; }

    }

    public enum HaveYouAlreadyCreatedYourUserAccount
    {

        [GovUkRadioCheckboxLabelText(Text = "Yes, I have a user account")]
        Yes = 0,

        [GovUkRadioCheckboxLabelText(Text = "No, I don't have a user account")]
        No = 1,

        [GovUkRadioCheckboxLabelText(Text = "My employer has used this service before, but I haven't")]
        NotSure = 2,

        [GovUkRadioCheckboxLabelText(Text = "Unspecified")]
        Unspecified = 3

    }

}

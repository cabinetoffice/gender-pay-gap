using GovUkDesignSystem;
using GovUkDesignSystem.Attributes;

namespace GenderPayGap.WebUI.Models.AccountCreation
{

    public class AlreadyCreatedAnAccountViewModel : GovUkViewModel
    {

        public HaveYouAlreadyCreatedYourUserAccount? HaveYouAlreadyCreatedYourUserAccount { get; set; }

    }

    public enum HaveYouAlreadyCreatedYourUserAccount
    {

        [GovUkRadioCheckboxLabelText(Text = "Yes")]
        Yes = 0,

        [GovUkRadioCheckboxLabelText(Text = "No")]
        No = 1,

        [GovUkRadioCheckboxLabelText(Text = "Not sure")]
        NotSure = 2,

        [GovUkRadioCheckboxLabelText(Text = "Unspecified")]
        Unspecified = 3

    }

}

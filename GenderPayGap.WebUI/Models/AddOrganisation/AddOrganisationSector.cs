using GovUkDesignSystem.Attributes;

namespace GenderPayGap.WebUI.Models.AddOrganisation {
    public enum AddOrganisationSector
    {
        // "Public, then Private" is the order we want on the page /add-employer/choose-employer-type
        [GovUkRadioCheckboxLabelText(Text = "Public authority employer")]
        Public,

        [GovUkRadioCheckboxLabelText(Text = "Private or voluntary sector employer")]
        Private
    }
}

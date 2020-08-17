using GovUkDesignSystem.Attributes;

namespace GenderPayGap.WebUI.Models.AddOrganisation {
    public enum AddOrganisationSector
    {
        // "Public, then Private" is the order we want on the page /add-organisation/choose-sector
        [GovUkRadioCheckboxLabelText(Text = "Public sector organisation")]
        Public,

        [GovUkRadioCheckboxLabelText(Text = "Private sector organisation")]
        Private
    }
}

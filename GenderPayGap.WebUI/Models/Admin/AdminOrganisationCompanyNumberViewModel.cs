using GenderPayGap.WebUI.ExternalServices.CompaniesHouse;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes;
using GovUkDesignSystem.Attributes.ValidationAttributes;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminOrganisationCompanyNumberViewModel : GovUkViewModel
    {

        public Database.Organisation Organisation { get; set; }

        public AdminOrganisationCompanyNumberViewModelCurrentPage CurrentPage { get; set; }

        public CompaniesHouseCompany CompaniesHouseCompany { get; set; }


        [GovUkValidateRequired(ErrorMessageIfMissing = "Please select whether you want to change or remove the company number")]
        public AdminOrganisationCompanyNumberChangeOrRemove? ChangeOrRemove { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change")]
        public string Reason { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a company number")]
        public string NewCompanyNumber { get; set; }

    }

    public enum AdminOrganisationCompanyNumberViewModelCurrentPage
    {
        Unknown,
        OfferChangeOrRemove,
        Remove,
        Change,
        ConfirmNew,
        BackToChange
    }

    public enum AdminOrganisationCompanyNumberChangeOrRemove
    {
        [GovUkRadioCheckboxLabelText(Text = "Change the company number")]
        Change,
        [GovUkRadioCheckboxLabelText(Text = "Remove the company number")]
        Remove
    }
}

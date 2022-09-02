using GenderPayGap.WebUI.ExternalServices.CompaniesHouse;
using GovUkDesignSystem.Attributes;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminOrganisationCompanyNumberViewModel 
    {

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public Database.Organisation Organisation { get; set; }

        public AdminOrganisationCompanyNumberViewModelCurrentPage CurrentPage { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public CompaniesHouseCompany CompaniesHouseCompany { get; set; }
        
        [GovUkValidateRequiredIf(IsRequiredPropertyName = nameof(ChangeOrRemoveRequired), 
            ErrorMessageIfMissing = "Please select whether you want to change or remove the company number")]
        public AdminOrganisationCompanyNumberChangeOrRemove? ChangeOrRemove { get; set; }

        public bool ChangeOrRemoveRequired => CurrentPage is AdminOrganisationCompanyNumberViewModelCurrentPage.OfferChangeOrRemove;
        [GovUkValidateRequiredIf(IsRequiredPropertyName = nameof(ReasonRequired), 
            ErrorMessageIfMissing = "Please enter a reason for this change")]
        [GovUkValidateCharacterCount(MaxCharacters = 250, NameAtStartOfSentence = "Reason", NameWithinSentence = "Reason")]
        public string Reason { get; set; }

        public bool ReasonRequired => CurrentPage is AdminOrganisationCompanyNumberViewModelCurrentPage.Remove ||
                                      CurrentPage is AdminOrganisationCompanyNumberViewModelCurrentPage.ConfirmNew;

        [GovUkValidateRequiredIf(IsRequiredPropertyName = nameof(NewCompanyNumberRequired), 
            ErrorMessageIfMissing = "Please enter a company number")]
        public string NewCompanyNumber { get; set; }

        public bool NewCompanyNumberRequired => CurrentPage is AdminOrganisationCompanyNumberViewModelCurrentPage.Change;

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

using GenderPayGap.Database;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class ChangeOrganisationAddressViewModel : GovUkViewModel
    {

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public Database.Organisation Organisation { get; set; }

        public ManuallyChangeOrganisationAddressViewModelActions Action { get; set; }

        #region Used by Manual Change page
        public string PoBox { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string TownCity { get; set; }
        public string County { get; set; }
        public string Country { get; set; }
        public string PostCode { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Select whether or not this is a UK address")]
        public ManuallyChangeOrganisationAddressIsUkAddress? IsUkAddress { get; set; }

        public void PopulateFromOrganisationAddress(OrganisationAddress address)
        {
            PoBox = address?.PoBox;
            Address1 = address?.Address1;
            Address2 = address?.Address2;
            Address3 = address?.Address3;
            TownCity = address?.TownCity;
            County = address?.County;
            Country = address?.Country;
            PostCode = address?.GetPostCodeInAllCaps();

            if (address?.IsUkAddress != null)
            {
                IsUkAddress = address.IsUkAddress.Value
                    ? ManuallyChangeOrganisationAddressIsUkAddress.Yes
                    : ManuallyChangeOrganisationAddressIsUkAddress.No;
            }
        }
        #endregion

        [GovUkValidateRequired(ErrorMessageIfMissing =
            "Select whether you want to use this address from Companies House or to enter an address manually")]
        public AcceptCompaniesHouseAddress? AcceptCompaniesHouseAddress { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change.")]
        [GovUkValidateCharacterCount(MaxCharacters = 250)]
        public string Reason { get; set; }

    }

    public enum ManuallyChangeOrganisationAddressIsUkAddress
    {
        Yes,
        No
    }

    public enum AcceptCompaniesHouseAddress
    {
        [GovUkRadioCheckboxLabelText(Text = "Yes, use this address from Companies House")]
        Accept,

        [GovUkRadioCheckboxLabelText(Text = "No, enter an address manually")]
        Reject
    }

    public enum ManuallyChangeOrganisationAddressViewModelActions
    {
        Unknown = 0,
        OfferNewCompaniesHouseAddress = 1,
        ManualChange = 2,
        CheckChangesManual = 3,
        CheckChangesCoHo = 4
    }
}

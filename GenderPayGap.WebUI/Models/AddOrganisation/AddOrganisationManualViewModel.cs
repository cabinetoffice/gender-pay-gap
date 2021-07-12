using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GenderPayGap.Core;
using GenderPayGap.Database;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.AddOrganisation
{
    public class AddOrganisationManualViewModel : GovUkViewModel
    {

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public List<SicSection> SicSections { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public List<SicCode> SelectedSicCodes { get; set; }

        // This is a bool? (nullable) because it makes for better "Back" links
        //   e.g. in Views/AddOrganisation/Search.cshtml, we create a new AddOrganisationChooseSectorViewModel,
        //   but we only want to pass the Sector back to the previous page (we haven't specified a value for Validate)
        public bool? Validate { get; set; }

        // This is a bool? (nullable) because it makes for better "Back" and "edit" links
        //   e.g. in Views/AddOrganisation/Search.cshtml, we create a new AddOrganisationChooseSectorViewModel,
        //   but we only want to pass the Sector back to the previous page (we haven't specified a value for Validate)
        public bool? Editing { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Choose which type of employer you would like to add")]
        public AddOrganisationSector? Sector { get; set; }

        public string Query { get; set; }

        [GovUkDisplayNameForErrors(NameAtStartOfSentence = "Employer name", NameWithinSentence = "employer name")]
        [GovUkValidateRequired(ErrorMessageIfMissing = "Enter the name of the employer")]
        [GovUkValidateCharacterCount(MaxCharacters = 100)]
        public string OrganisationName { get; set; }

        [GovUkDisplayNameForErrors(NameAtStartOfSentence = "PO Box", NameWithinSentence = "PO Box")]
        [GovUkValidateCharacterCount(MaxCharacters = 30)]
        public string PoBox { get; set; }

        [GovUkDisplayNameForErrors(NameAtStartOfSentence = "Address line", NameWithinSentence = "address line")]
        [GovUkValidateRequired(ErrorMessageIfMissing = "Enter the registered address of the employer")]
        [GovUkValidateCharacterCount(MaxCharacters = 100)]
        public string Address1 { get; set; }

        [GovUkDisplayNameForErrors(NameAtStartOfSentence = "Address line", NameWithinSentence = "address line")]
        [GovUkValidateCharacterCount(MaxCharacters = 100)]
        public string Address2 { get; set; }

        [GovUkDisplayNameForErrors(NameAtStartOfSentence = "Address line", NameWithinSentence = "address line")]
        [GovUkValidateCharacterCount(MaxCharacters = 100)]
        public string Address3 { get; set; }

        [GovUkDisplayNameForErrors(NameAtStartOfSentence = "Town or city", NameWithinSentence = "town or city")]
        [GovUkValidateCharacterCount(MaxCharacters = 100)]
        public string TownCity { get; set; }

        [GovUkDisplayNameForErrors(NameAtStartOfSentence = "County", NameWithinSentence = "county")]
        [GovUkValidateCharacterCount(MaxCharacters = 100)]
        public string County { get; set; }

        [GovUkDisplayNameForErrors(NameAtStartOfSentence = "Country", NameWithinSentence = "country")]
        [GovUkValidateCharacterCount(MaxCharacters = 100)]
        public string Country { get; set; }

        [GovUkDisplayNameForErrors(NameAtStartOfSentence = "Postcode", NameWithinSentence = "postcode")]
        [GovUkValidateCharacterCount(MaxCharacters = 20)]
        public string PostCode { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Select if this employer's registered address is a UK address")]
        public AddOrganisationIsUkAddress? IsUkAddress { get; set; }

        public List<int> SicCodes { get; set; }

        public SectorTypes GetSectorType()
        {
            switch (Sector)
            {
                case AddOrganisationSector.Public:
                    return SectorTypes.Public;
                case AddOrganisationSector.Private:
                    return SectorTypes.Private;
                case null:
                    return SectorTypes.Unknown;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool? GetIsUkAddressAsBoolean()
        {
            switch (IsUkAddress)
            {
                case AddOrganisationIsUkAddress.Yes:
                    return true;
                case AddOrganisationIsUkAddress.No:
                    return false;
                case null:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }
}

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
    public class AddOrganisationManualViewModel 
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

        public AddOrganisationSector? Sector { get; set; }

        public string Query { get; set; }

        [GovUkValidateCharacterCount(MaxCharacters = 100, NameAtStartOfSentence = "Employer name", NameWithinSentence = "employer name")]
        public string OrganisationName { get; set; }

        [GovUkValidateCharacterCount(MaxCharacters = 30, NameAtStartOfSentence = "PO Box", NameWithinSentence = "PO Box")]
        public string PoBox { get; set; }
        
        [GovUkValidateCharacterCount(MaxCharacters = 100, NameAtStartOfSentence = "Address line", NameWithinSentence = "address line")]
        public string Address1 { get; set; }

        [GovUkValidateCharacterCount(MaxCharacters = 100, NameAtStartOfSentence = "Address line", NameWithinSentence = "address line")]
        public string Address2 { get; set; }

        [GovUkValidateCharacterCount(MaxCharacters = 100, NameAtStartOfSentence = "Address line", NameWithinSentence = "address line")]
        public string Address3 { get; set; }

        [GovUkValidateCharacterCount(MaxCharacters = 100, NameAtStartOfSentence = "Town or city", NameWithinSentence = "town or city")]
        public string TownCity { get; set; }

        [GovUkValidateCharacterCount(MaxCharacters = 100, NameAtStartOfSentence = "County", NameWithinSentence = "county")]
        public string County { get; set; }

        [GovUkValidateCharacterCount(MaxCharacters = 100, NameAtStartOfSentence = "Country", NameWithinSentence = "country")]
        public string Country { get; set; }

        [GovUkValidateCharacterCount(MaxCharacters = 20, NameAtStartOfSentence = "Postcode", NameWithinSentence = "postcode")]
        public string PostCode { get; set; }

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

using System.Collections.Generic;
using GenderPayGap.Database;
using GenderPayGap.WebUI.ExternalServices.CompaniesHouse;
using GenderPayGap.WebUI.Models.Admin;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.AddOrganisation
{
    public class AddOrganisationFoundViewModel : GovUkViewModel
    {

        public string Id { get; set; }
        public string CompanyNumber { get; set; }
        public string Query { get; set; }

        // Used to construct the Back link
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public AddOrganisationSector Sector { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public string Name { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public List<string> AddressLines { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public List<SicCode> SicCodes { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public UserOrganisation ExistingUserOrganisation { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Select whether or not this is a UK address")]
        public AddOrganisationFoundViewModelIsUkAddress? IsUkAddress { get; set; }

    }

    public enum AddOrganisationFoundViewModelIsUkAddress
    {
        Yes,
        No
    }
}

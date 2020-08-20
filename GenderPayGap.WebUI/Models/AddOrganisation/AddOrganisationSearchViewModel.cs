using System.Collections.Generic;
using GovUkDesignSystem;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.AddOrganisation
{
    public class AddOrganisationSearchViewModel : GovUkViewModel
    {

        public AddOrganisationSector Sector { get; set; }

        public string Query { get; set; }

        [BindNever]
        public List<AddOrganisationSearchResult> SearchResults { get; set; }
        
        [BindNever]
        public bool TooManyResults { get; set; }

    }

    public class AddOrganisationSearchResult
    {
        public string OrganisationName { get; set; }
        public string OrganisationAddress { get; set; }
        public string EncryptedOrganisationId { get; set; }
        public string CompanyNumber { get; set; }
        public List<AddOrganisationSearchResultOrganisationIdentifier> Identifiers { get; set; }
    }

    public class AddOrganisationSearchResultOrganisationIdentifier
    {
        public string IdentifierType { get; set; }
        public string Identifier { get; set; }
    }
}

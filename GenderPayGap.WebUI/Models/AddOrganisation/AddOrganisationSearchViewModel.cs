using System.Collections.Generic;
using GovUkDesignSystem;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.AddOrganisation
{
    public class AddOrganisationSearchViewModel 
    {

        public AddOrganisationSector Sector { get; set; }

        public string Query { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public AddOrganisationSearchResults SearchResults { get; set; }

    }

    public class AddOrganisationSearchResults
    {
        public List<AddOrganisationSearchResult> SearchResults { get; set; }
        public bool TooManyResults { get; set; }
    }

    public class AddOrganisationSearchResult
    {
        public string OrganisationName { get; set; }
        public string OrganisationAddress { get; set; }
        public string EncryptedOrganisationId { get; set; }
        public string CompanyNumber { get; set; }
        public List<AddOrganisationSearchResultOrganisationIdentifier> Identifiers { get; set; } = new List<AddOrganisationSearchResultOrganisationIdentifier>();
    }

    public class AddOrganisationSearchResultOrganisationIdentifier
    {
        public string IdentifierType { get; set; }
        public string Identifier { get; set; }
    }
}

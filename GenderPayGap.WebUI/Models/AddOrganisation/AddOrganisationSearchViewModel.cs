﻿using Microsoft.AspNetCore.Mvc.ModelBinding;

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
        public long? OrganisationId { get; set; }
        public string CompanyNumber { get; set; }
    }

    public class AddOrganisationSeparateSearchResults
    {
        public List<AddOrganisationSearchResult> SearchResultsFromOurDatabase { get; set; }
        public List<AddOrganisationSearchResult> SearchResultsFromCompaniesHouse { get; set; }
    }

}

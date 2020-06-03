using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Models.Search;

namespace GenderPayGap.WebUI.Search.CachedObjects {

    public class SearchCachedOrganisation
    {
        public long OrganisationId { get; set; }
        public string EncryptedId { get; set; }
        public SearchReadyValue OrganisationName { get; set; }
        public List<SearchReadyValue> OrganisationNames { get; set; } // All names (current and previous)
        public string CompanyNumber { get; set; }
        public string EmployerReference { get; set; }
        public OrganisationStatuses Status { get; set; }
        public int MinEmployees { get; set; }
        public List<OrganisationSizes> OrganisationSizes { get; set; }
        public List<char> SicSectionIds { get; set; }

    }
}

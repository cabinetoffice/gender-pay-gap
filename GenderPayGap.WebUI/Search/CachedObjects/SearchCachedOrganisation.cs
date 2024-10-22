using System.Collections.Generic;
using GenderPayGap.Core;

namespace GenderPayGap.WebUI.Search.CachedObjects
{

    public class SearchCachedOrganisation
    {

        public long OrganisationId { get; set; }
        public string EncryptedId { get; set; }
        public SearchReadyValue OrganisationName { get; set; }
        public List<SearchReadyValue> OrganisationNames { get; set; } // All names (current and previous)
        public string CompanyNumber { get; set; }
        public OrganisationStatuses Status { get; set; }
        public int MinEmployees { get; set; }
        public List<string> SicSectionIds { get; set; }
        public bool IncludeInViewingService { get; set; }
        public List<OrganisationSizes> OrganisationSizes { get; set; }
        public List<int> ReportedLateYears { get; set; }
        public string Address { get; set; }
        
    }
}

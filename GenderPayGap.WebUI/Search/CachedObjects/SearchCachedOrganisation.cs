using System.Collections.Generic;
using GenderPayGap.Core;

namespace GenderPayGap.WebUI.Search.CachedObjects {

    internal class SearchCachedOrganisation
    {
        public long OrganisationId { get; set; }
        public string OrganisationName { get; set; }
        public List<string> OrganisationNames { get; set; } // All names (current and previous)
        public string CompanyNumber { get; set; }
        public string EmployerReference { get; set; }
        public OrganisationStatuses Status { get; set; }
    }
}

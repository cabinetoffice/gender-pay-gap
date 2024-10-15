using System.Collections.Generic;
using GenderPayGap.Core;

namespace GenderPayGap.WebUI.Models.Search
{
    public class SearchPageViewModel
    {

        public string EmployerName { get; set; }
        public List<OrganisationSizes> EmployerSize { get; set; } = new List<OrganisationSizes>();
        public List<string> Sector { get; set; } = new List<string>();
        public bool ReportedLate { get; set; }
        public string OrderBy { get; set; } = "relevance";

    }
}

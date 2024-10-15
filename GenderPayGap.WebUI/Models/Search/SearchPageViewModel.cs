using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.Database;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Search
{
    public class SearchPageViewModel
    {

        public string EmployerName { get; set; }
        public List<OrganisationSizes> EmployerSize { get; set; } = new List<OrganisationSizes>();
        public List<string> Sector { get; set; } = new List<string>();
        public bool ReportedLate { get; set; }
        public string OrderBy { get; set; } = "relevance";

        [BindNever]
        public List<SicSection> PossibleSectors { get; set; }

    }
}

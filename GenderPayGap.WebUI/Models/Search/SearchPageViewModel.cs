using System.Collections.Generic;
using System.Linq;
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
        public List<string> ReportedLateYear { get; set; } = new List<string>();
        public string OrderBy { get; set; } = "relevance";

        [BindNever]
        public List<SicSection> PossibleSectors { get; set; }
        [BindNever]
        public List<int> PossibleReportedLateYears { get; set; }

        public List<int> GetReportedLateYearsAsInts()
        {
            return ReportedLateYear
                .Where(year => int.TryParse(year, out _))
                .Select(year => int.Parse(year))
                .ToList();
        }

        public bool IsOrderByRelevance()
        {
            return OrderBy == "relevance";
        }

    }
}

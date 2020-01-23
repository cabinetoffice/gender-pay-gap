using System;
using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.Extensions;

namespace GenderPayGap.WebUI.Models
{

    [Serializable]
    public class SearchResultsQuery
    {

        // TODO: dotnet core supports defining named query params using FromQueryAttribute
        // Example:
        // [FromQuery("s")] 
        // public IEnumerable<char> OrganisationSize { get; set; }

        // Keywords
        public string search { get; set; }

        // Sector
        public IEnumerable<char> s { get; set; }

        // Employer Size
        public IEnumerable<int> es { get; set; }

        // Reporting Year
        public IEnumerable<int> y { get; set; }

        // Reporting Status
        public IEnumerable<int> st { get; set; }

        // Employer Type
        public SearchType t { get; set; } = SearchType.ByEmployerName;

        // Page
        public int p { get; set; } = 1;

        public bool IsPageValid()
        {
            return p > 0;
        }

        public bool IsEmployerSizeValid()
        {
            // if null then we won't filter on this so its valid
            if (es == null)
            {
                return true;
            }

            foreach (int size in es)
            {
                // ensure we have a valid org size
                if (!Enum.IsDefined(typeof(OrganisationSizes), size))
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsReportingStatusValid()
        {
            // if null then we won't filter on this so its valid
            if (st == null)
            {
                return true;
            }

            foreach (int value in st)
            {
                // ensure we have a valid enum entry
                if (!Enum.IsDefined(typeof(SearchReportingStatusFilter), value))
                {
                    return false;
                }
            }

            return true;
        }

        public bool TryValidateSearchParams(out HttpStatusViewResult exception)
        {
            exception = null;

            if (!IsPageValid())
            {
                exception = new HttpBadRequestResult($"EmployerSearch: Invalid page {p}");
            }

            if (!IsEmployerSizeValid())
            {
                exception = new HttpBadRequestResult($"EmployerSearch: Invalid EmployerSize {es.ToDelimitedString()}");
            }

            if (!IsReportingStatusValid())
            {
                exception = new HttpBadRequestResult($"EmployerSearch: Invalid ReportingStatus {st.ToDelimitedString()}");
            }

            return exception == null;
        }

    }

}

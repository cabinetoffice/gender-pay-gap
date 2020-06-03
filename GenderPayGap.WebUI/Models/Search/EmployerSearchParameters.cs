using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GenderPayGap.Core;
using GenderPayGap.Extensions;
using Microsoft.Azure.Search.Models;

namespace GenderPayGap.WebUI.Models.Search
{
    [Serializable]
    public class EmployerSearchParameters
    {

        public string Keywords { get; set; }

        public IEnumerable<char> FilterSicSectionIds { get; set; }

        public IEnumerable<int> FilterEmployerSizes { get; set; }

        public IEnumerable<int> FilterReportedYears { get; set; }

        public IEnumerable<int> FilterCodeIds { get; set; }

        public IEnumerable<int> FilterReportingStatus { get; set; }

        public int Page { get; set; } = 1;

        public string SearchFields { get; set; }

        public SearchType SearchType { get; set; }

        public SearchMode SearchMode { get; set; }

        public int PageSize { get; set; } = 20;

        public string RemoveTheMostCommonTermsOnOurDatabaseFromTheKeywords()
        {
            if (string.IsNullOrEmpty(Keywords))
            {
                return Keywords;
            }

            const string patternToReplace = "(?i)(limited|ltd|llp|uk | uk|\\(uk\\)|-uk|plc)[\\.]*";

            string resultingString;

            resultingString = Regex.Replace(Keywords, patternToReplace, string.Empty);
            resultingString = resultingString.Trim();

            bool willThisReplacementClearTheString = resultingString == string.Empty;

            return willThisReplacementClearTheString
                ? Keywords // don't replace - user wants to search 'limited' or 'uk'...
                : resultingString;
        }

        public string ToFilterQuery()
        {
            var queryFilter = new List<string>();

            if (FilterSicSectionIds != null && FilterSicSectionIds.Any())
            {
                IEnumerable<string> sectorQuery = FilterSicSectionIds.Select(x => $"id eq '{x}'");
                queryFilter.Add($"SicSectionIds/any(id: {string.Join(" or ", sectorQuery)})");
            }

            if (FilterEmployerSizes != null && FilterEmployerSizes.Any())
            {
                IEnumerable<string> sizeQuery = FilterEmployerSizes.Select(x => $"Size eq {x}");
                queryFilter.Add($"({string.Join(" or ", sizeQuery)})");
            }

            if (FilterCodeIds != null && FilterCodeIds.Any())
            {
                IEnumerable<string> codeIdQuery = FilterCodeIds.Select(x => $"id eq '{x}'");
                queryFilter.Add($"SicCodeIds/any(id: {string.Join(" or ", codeIdQuery)})");
            }

            var anyReportedYearParam = "";
            if (FilterReportedYears != null && FilterReportedYears.Any())
            {
                anyReportedYearParam = "ReportedYear: " + string.Join(" or ", FilterReportedYears.Select(x => $"ReportedYear eq '{x}'"));
                queryFilter.Add($"ReportedYears/any({anyReportedYearParam})");
            }

            if (FilterReportingStatus != null && FilterReportingStatus.Any())
            {
                var statusQuery = new List<string>();
                foreach (int status in FilterReportingStatus)
                {
                    var reportStatus = (SearchReportingStatusFilter) status;
                    if (reportStatus == SearchReportingStatusFilter.ReportedWithCompanyLinkToGpgInfo)
                    {
                        statusQuery.Add($"ReportedExplanationYears/any({anyReportedYearParam})");
                    }
                    else if (reportStatus == SearchReportingStatusFilter.ReportedLate)
                    {
                        statusQuery.Add($"ReportedLateYears/any({anyReportedYearParam})");
                    }
                    else if (reportStatus == SearchReportingStatusFilter.ReportedInTheLast7Days)
                    {
                        statusQuery.Add($"LatestReportedDate gt {VirtualDateTime.UtcNow.Date.AddDays(-7).ToString("O")}");
                    }
                    else if (reportStatus == SearchReportingStatusFilter.ReportedInTheLast30Days)
                    {
                        statusQuery.Add($"LatestReportedDate gt {VirtualDateTime.UtcNow.Date.AddDays(-30).ToString("O")}");
                    }
                }

                queryFilter.Add($"({string.Join(" or ", statusQuery.ToArray())})");
            }

            return string.Join(" and ", queryFilter);
        }

    }
}

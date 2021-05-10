using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using GenderPayGap.Core;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Extensions;

namespace GenderPayGap.WebUI.Search.CachedObjects
{

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
        public List<int> ReportingYears { get; set; }
        public List<string> SicCodeIds { get; set; }
        public List<char> SicSectionIds { get; set; }
        public List<SearchReadyValue> SicCodeSynonyms { get; set; }
        public bool IncludeInViewingService { get; set; }
        public SectorTypes Sector { get; set; }
        public Dictionary<int, List<OrganisationSizes>> ReportingYearToOrganisationSizesMap { get; set; }
        public List<int> ReportedLateYears { get; set; }
        public List<int> ReportedWithCompanyLinkYears { get; set; }
        public Dictionary<int, DateTime> ReportingYearToDateOfLatestReportMap { get; set; }
        
        public List<OrganisationSizes> GetOrganisationSizes(List<int> years)
        {
            if (years.IsNullOrEmpty())
            {
                years = ReportingYearsHelper.GetReportingYears();
            }

            var organisationSizes = new List<OrganisationSizes>();

            foreach (var year in years)
            {
                if (ReportingYearToOrganisationSizesMap.TryGetValue(year, out List<OrganisationSizes> sizes))
                {
                    organisationSizes = organisationSizes.Union(sizes).ToList();
                }
            }

            return organisationSizes;
        }

        public List<DateTime> GetDatesOfLatestReports(List<int> years)
        {
            if (years.IsNullOrEmpty())
            {
                years = ReportingYearsHelper.GetReportingYears();
            }

            var latestReports = new List<DateTime>();

            foreach (var year in years)
            {
                if (ReportingYearToDateOfLatestReportMap.TryGetValue(year, out DateTime latestDate))
                {
                    latestReports.Add(latestDate);
                }
            }

            return latestReports;
        }

        public bool HasReportedWithCompanyLink(List<int> years)
        {
            if (years.IsNullOrEmpty())
            {
                return !ReportedWithCompanyLinkYears.IsNullOrEmpty();
            }

            return ReportedWithCompanyLinkYears.Intersect(years).Any();
        }

        public bool HasReportedLate(List<int> years)
        {
            if (years.IsNullOrEmpty())
            {
                return !ReportedLateYears.IsNullOrEmpty();
            }

            return ReportedLateYears.Intersect(years).Any();
        }
    }
}

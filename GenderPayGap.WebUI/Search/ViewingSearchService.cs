using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Models.Search;
using GenderPayGap.WebUI.Search.CachedObjects;

namespace GenderPayGap.WebUI.Search
{

    internal class RankedViewingSearchOrganisation
    {

        public ViewingSearchResultOrganisationViewModel ViewingSearchResult { get; set; }
        public List<RankedName> Names { get; set; }
        public RankedName TopName { get; set; }

    }

    public class ViewingSearchResultOrganisationViewModel
    {

        public string OrganisationName { get; set; }
        public List<string> OrganisationPreviousNames { get; set; }
        public long OrganisationId { get; set; }
        public string EmployerRef { get; set; }
        public string CompanyNumber { get; set; }
        public OrganisationStatuses Status { get; set; }
        public string EncryptedId { get; set; }

    }

    public class ViewingSearchService
    {

        private readonly IDataRepository dataRepository;

        public ViewingSearchService(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        public PagedResult<EmployerSearchModel> Search(EmployerSearchParameters searchParams, bool orderByRelevance)
        {
            List<SearchCachedOrganisation> allOrganisations = SearchRepository.CachedOrganisations
                .Where(o => o.IncludeInViewingService)
                .ToList();

            List<SearchCachedOrganisation> filteredOrganisations = FilterByOrganisations(allOrganisations, searchParams);

            if (searchParams.Keywords == null)
            {
                List<SearchCachedOrganisation> orderedOrganisations =
                    filteredOrganisations.OrderBy(o => o.OrganisationName.OriginalValue).ToList();

                List<SearchCachedOrganisation> paginatedResultsForAllOrganisations = PaginateResults(
                    orderedOrganisations,
                    searchParams.Page,
                    searchParams.PageSize);

                return new PagedResult<EmployerSearchModel>
                {
                    Results = ConvertToEmployerSearchModels(paginatedResultsForAllOrganisations),
                    CurrentPage = searchParams.Page,
                    PageSize = searchParams.PageSize,
                    ActualRecordTotal = orderedOrganisations.Count,
                    VirtualRecordTotal = orderedOrganisations.Count
                };
            }

            string query = searchParams.Keywords.Trim().ToLower();

            bool queryContainsPunctuation = WordSplittingRegex.ContainsPunctuationCharacters(query);

            List<string> searchTerms = SearchHelper.ExtractSearchTermsFromQuery(query, queryContainsPunctuation);

            var matchingOrganisations = new List<SearchCachedOrganisation>();
            var convertedResults = new List<EmployerSearchModel>();

            if (searchParams.SearchType == SearchType.NotSet)
            {
                throw new NotImplementedException();
            }

            if (searchParams.SearchType == SearchType.ByEmployerName)
            {
                matchingOrganisations = GetMatchingOrganisationsByName(
                    filteredOrganisations,
                    searchTerms,
                    query,
                    queryContainsPunctuation);

                List<RankedViewingSearchOrganisation> organisationsWithRankings = CalculateOrganisationRankings(
                    matchingOrganisations,
                    searchTerms,
                    query,
                    queryContainsPunctuation);

                List<RankedViewingSearchOrganisation> rankedOrganisations = orderByRelevance
                    ? OrderOrganisationsByRank(organisationsWithRankings)
                    : OrderOrganisationsAlphabetically(organisationsWithRankings);

                List<RankedViewingSearchOrganisation> paginatedResults = PaginateResults(
                    rankedOrganisations,
                    searchParams.Page,
                    searchParams.PageSize);

                convertedResults = ConvertRankedOrgsToEmployerSearchModels(paginatedResults);
            }

            if (searchParams.SearchType == SearchType.BySectorType)
            {
                matchingOrganisations = GetMatchingOrganisationsBySicCode(
                    filteredOrganisations,
                    searchTerms,
                    query,
                    queryContainsPunctuation);

                // Only alphabetically for SIC code search
                List<SearchCachedOrganisation> orderedOrganisations =
                    matchingOrganisations.OrderBy(o => o.OrganisationName.OriginalValue).ToList();

                List<SearchCachedOrganisation> paginatedSicCodeResults = PaginateResults(
                    orderedOrganisations,
                    searchParams.Page,
                    searchParams.PageSize);

                convertedResults = ConvertSearchCachedOrganisationsToEmployerSearchModels(paginatedSicCodeResults);
            }

            var pagedResult = new PagedResult<EmployerSearchModel>
            {
                Results = convertedResults,
                CurrentPage = searchParams.Page,
                PageSize = searchParams.PageSize,
                ActualRecordTotal = matchingOrganisations.Count,
                VirtualRecordTotal = matchingOrganisations.Count
            };

            return pagedResult;
        }

        private List<SearchCachedOrganisation> FilterByOrganisations(List<SearchCachedOrganisation> organisations,
            EmployerSearchParameters searchParams)
        {
            IEnumerable<OrganisationSizes> selectedOrganisationSizes = searchParams.FilterEmployerSizes.Select(s => (OrganisationSizes) s);
            IEnumerable<char> selectedSicSections = searchParams.FilterSicSectionIds;
            List<int> selectedReportingYears = searchParams.FilterReportedYears.ToList();
            IEnumerable<SearchReportingStatusFilter> selectedReportingStatuses =
                searchParams.FilterReportingStatus.Select(s => (SearchReportingStatusFilter) s);

            IEnumerable<SearchCachedOrganisation> filteredOrgs = organisations.AsEnumerable();

            if (selectedOrganisationSizes.Any())
            {
                filteredOrgs = filteredOrgs.Where(o => o.GetOrganisationSizes(selectedReportingYears).Intersect(selectedOrganisationSizes).Any());
            }

            if (selectedSicSections.Any())
            {
                filteredOrgs = filteredOrgs.Where(o => o.SicSectionIds.Intersect(selectedSicSections).Any());
            }

            if (selectedReportingYears.Any())
            {
                filteredOrgs = filteredOrgs.Where(o => o.ReportingYears.Intersect(selectedReportingYears).Any());
            }

            if (selectedReportingStatuses.Any())
            {
                var reportingStatusFilteredOrgs = new List<SearchCachedOrganisation>();

                foreach (SearchReportingStatusFilter status in selectedReportingStatuses)
                {
                    reportingStatusFilteredOrgs = reportingStatusFilteredOrgs.Union(ApplyReportingStatusesFilter(filteredOrgs, status, selectedReportingYears)).ToList();
                }

                filteredOrgs = reportingStatusFilteredOrgs;
            }

            

            return filteredOrgs.ToList();
        }

        private IEnumerable<SearchCachedOrganisation> ApplyReportingStatusesFilter(IEnumerable<SearchCachedOrganisation> organisations,
            SearchReportingStatusFilter filter, List<int> reportingYears)
        {
            switch (filter)
            {
                case SearchReportingStatusFilter.ReportedInTheLast7Days:
                    return organisations.Where(o => o.GetDatesOfLatestReports(reportingYears).Any(d => d > VirtualDateTime.Now.AddDays(-7)));
                case SearchReportingStatusFilter.ReportedInTheLast30Days:
                    return organisations.Where(o => o.GetDatesOfLatestReports(reportingYears).Any(d => d > VirtualDateTime.Now.AddDays(-30)));
                case SearchReportingStatusFilter.ReportedLate:
                    return organisations.Where(o => o.HasReportedLate(reportingYears));
                case SearchReportingStatusFilter.ReportedWithCompanyLinkToGpgInfo:
                    return organisations.Where(o => o.HasReportedWithCompanyLink(reportingYears));
                default:
                    throw new Exception();
            }
        }


        private static List<T> PaginateResults<T>(List<T> results, int currentPage, int pageSize)
        {
            return results.Skip(pageSize * (currentPage - 1)).Take(pageSize).ToList();
        }

        private List<EmployerSearchModel> ConvertToEmployerSearchModels(List<SearchCachedOrganisation> organisations)
        {
            return organisations.Select(
                    searchCachedOrganisation =>
                    {
                        var organisation = dataRepository.Get<Organisation>(searchCachedOrganisation.OrganisationId);
                        string[] sicSectionNames = organisation.GetSicCodes()
                            .Select(s => s.SicCode.SicSection.Description)
                            .UniqueI()
                            .ToArray();

                        string previousName = searchCachedOrganisation.OrganisationNames.Count > 1
                            ? searchCachedOrganisation.OrganisationNames[1].OriginalValue
                            : null;

                        return new EmployerSearchModel
                        {
                            OrganisationIdEncrypted = searchCachedOrganisation.EncryptedId,
                            Name = searchCachedOrganisation.OrganisationName.OriginalValue,
                            PreviousName = previousName,
                            Address = organisation.GetLatestAddress()?.GetAddressString(),
                            SicSectionNames = sicSectionNames
                        };
                    })
                .ToList();
        }

        private static List<SearchCachedOrganisation> GetMatchingOrganisationsByName(List<SearchCachedOrganisation> allOrganisations,
            List<string> searchTerms,
            string query,
            bool queryContainsPunctuation)
        {
            return allOrganisations
                .Where(org => SearchHelper.CurrentOrPreviousOrganisationNameMatchesSearchTerms(org, searchTerms, queryContainsPunctuation))
                .ToList();
        }

        private static List<RankedViewingSearchOrganisation> CalculateOrganisationRankings(
            List<SearchCachedOrganisation> matchingOrganisations,
            List<string> searchTerms,
            string query,
            bool queryContainsPunctuation)
        {
            return matchingOrganisations
                .Select(organisation => CalculateRankForOrganisation(organisation, searchTerms, query, queryContainsPunctuation))
                .ToList();
        }

        private static RankedViewingSearchOrganisation CalculateRankForOrganisation(SearchCachedOrganisation organisation,
            List<string> searchTerms,
            string query,
            bool queryContainsPunctuation)
        {
            var rankedViewingSearchOrganisation = new RankedViewingSearchOrganisation {Names = new List<RankedName>()};

            rankedViewingSearchOrganisation.Names =
                RankValueHelper.GetRankedNames(organisation.OrganisationNames, searchTerms, query, queryContainsPunctuation);

            rankedViewingSearchOrganisation.TopName = rankedViewingSearchOrganisation.Names
                .RankHelperOrderByListOfDoubles(name => name.Ranks)
                .First();

            List<double> ranks = RankValueHelper.ApplyCompanySizeMultiplierToRanks(
                rankedViewingSearchOrganisation.TopName.Ranks,
                organisation.MinEmployees);
            rankedViewingSearchOrganisation.TopName.Ranks = ranks;

            List<string> previousNames = rankedViewingSearchOrganisation.Names
                .Where((item, nameIndex) => nameIndex != 0)
                .Select(name => name.Name)
                .ToList();

            rankedViewingSearchOrganisation.ViewingSearchResult = new ViewingSearchResultOrganisationViewModel
            {
                OrganisationName = rankedViewingSearchOrganisation.Names[0].Name,
                OrganisationPreviousNames = previousNames,
                OrganisationId = organisation.OrganisationId,
                EncryptedId = organisation.EncryptedId
            };

            return rankedViewingSearchOrganisation;
        }

        private List<RankedViewingSearchOrganisation> OrderOrganisationsByRank(
            List<RankedViewingSearchOrganisation> organisationsWithRankings)
        {
            return organisationsWithRankings
                .RankHelperOrderByListOfDoubles(org => org.TopName.Ranks)
                .ThenBy(org => org.Names[0].Name)
                .ToList();
        }

        private static List<RankedViewingSearchOrganisation> OrderOrganisationsAlphabetically(
            List<RankedViewingSearchOrganisation> organisationsWithRankings)
        {
            return organisationsWithRankings.OrderBy(o => o.Names[0].Name).ToList();
        }

        private List<EmployerSearchModel> ConvertRankedOrgsToEmployerSearchModels(List<RankedViewingSearchOrganisation> organisations)
        {
            return organisations.Select(
                    rankedViewingSearchOrganisation =>
                    {
                        var organisation =
                            dataRepository.Get<Organisation>(rankedViewingSearchOrganisation.ViewingSearchResult.OrganisationId);
                        string[] sicSectionNames = organisation.GetSicCodes()
                            .Select(s => s.SicCode.SicSection.Description)
                            .UniqueI()
                            .ToArray();

                        string previousName = rankedViewingSearchOrganisation.ViewingSearchResult.OrganisationPreviousNames.Count > 0
                            ? rankedViewingSearchOrganisation.ViewingSearchResult.OrganisationPreviousNames[0]
                            : null;

                        return new EmployerSearchModel
                        {
                            OrganisationIdEncrypted = rankedViewingSearchOrganisation.ViewingSearchResult.EncryptedId,
                            Name = rankedViewingSearchOrganisation.ViewingSearchResult.OrganisationName,
                            PreviousName = previousName,
                            Address = organisation.GetLatestAddress()?.GetAddressString(),
                            SicSectionNames = sicSectionNames
                        };
                    })
                .ToList();
        }

        private static List<SearchCachedOrganisation> GetMatchingOrganisationsBySicCode(List<SearchCachedOrganisation> allOrganisations,
            List<string> searchTerms,
            string query,
            bool queryContainsPunctuation)
        {
            List<SearchCachedOrganisation> matchesOnSicCode = allOrganisations.Where(org => org.SicCodeIds.Contains(query)).ToList();

            if (matchesOnSicCode.Any())
            {
                return matchesOnSicCode;
            }

            return allOrganisations
                .Where(org => SearchHelper.OrganisationSicCodesMatchSearchTerms(org, searchTerms, queryContainsPunctuation))
                .ToList();
        }

        private List<EmployerSearchModel> ConvertSearchCachedOrganisationsToEmployerSearchModels(
            List<SearchCachedOrganisation> organisations)
        {
            return organisations.Select(
                    org =>
                    {
                        var organisation =
                            dataRepository.Get<Organisation>(org.OrganisationId);
                        string[] sicSectionNames = organisation.GetSicCodes()
                            .Select(s => s.SicCode.SicSection.Description)
                            .UniqueI()
                            .ToArray();

                        string previousName = org.OrganisationNames.Count > 1
                            ? org.OrganisationNames[1].OriginalValue
                            : null;

                        return new EmployerSearchModel
                        {
                            OrganisationIdEncrypted = org.EncryptedId,
                            Name = org.OrganisationName.OriginalValue,
                            PreviousName = previousName,
                            Address = organisation.GetLatestAddress()?.GetAddressString(),
                            SicSectionNames = sicSectionNames
                        };
                    })
                .ToList();
        }

    }
}

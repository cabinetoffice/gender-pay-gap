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

        public PagedResult<EmployerSearchModel> Search(EmployerSearchParameters searchParams,
            Dictionary<string, Dictionary<object, long>> facets)
        {
            List<SearchCachedOrganisation> allOrganisations = SearchRepository.CachedOrganisations;

            // no search terms entered
            if (searchParams.Keywords == null)
            {
                List<SearchCachedOrganisation> paginatedResultsForAllOrganisations = PaginateResults(
                    allOrganisations,
                    searchParams.Page,
                    searchParams.PageSize);

                return new PagedResult<EmployerSearchModel>
                {
                    Results = ConvertToEmployerSearchModels(paginatedResultsForAllOrganisations),
                    CurrentPage = searchParams.Page,
                    PageSize = searchParams.PageSize,
                    ActualRecordTotal = allOrganisations.Count,
                    VirtualRecordTotal = allOrganisations.Count
                };
            }

            string query = searchParams.Keywords.Trim().ToLower();

            bool queryContainsPunctuation = WordSplittingRegex.ContainsPunctuationCharacters(query);

            List<string> searchTerms = SearchHelper.ExtractSearchTermsFromQuery(query, queryContainsPunctuation);

            List<SearchCachedOrganisation> matchingOrganisations = GetMatchingOrganisations(
                allOrganisations,
                searchTerms,
                query,
                queryContainsPunctuation);


            // filter out on extra conditions
//            .Where(
//                org => org.Returns.Any(r => r.Status == ReturnStatuses.Submitted)
//                       || org.OrganisationScopes.Any(
//                           sc => sc.Status == ScopeRowStatuses.Active
//                                 && (sc.ScopeStatus == ScopeStatuses.InScope || sc.ScopeStatus == ScopeStatuses.PresumedInScope)))


            // need to handle SicCode search and employer type search

            // apply facets

            List<RankedViewingSearchOrganisation> organisationsWithRankings = CalculateOrganisationRankings(
                matchingOrganisations,
                searchTerms,
                query,
                queryContainsPunctuation);

            List<RankedViewingSearchOrganisation> orderedOrganisations = OrderOrganisationsByRank(organisationsWithRankings);

            List<RankedViewingSearchOrganisation> paginatedResults = PaginateResults(
                orderedOrganisations,
                searchParams.Page,
                searchParams.PageSize);

            List<EmployerSearchModel> convertedResults = ConvertRankedOrgsToEmployerSearchModels(paginatedResults);

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

        private static List<SearchCachedOrganisation> GetMatchingOrganisations(List<SearchCachedOrganisation> allOrganisations,
            List<string> searchTerms,
            string query,
            bool queryContainsPunctuation)
        {
            return allOrganisations
                .Where(org => org.Status == OrganisationStatuses.Active || org.Status == OrganisationStatuses.Retired)
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
                RankValueHelper.GetRankedNames(organisation, searchTerms, query, queryContainsPunctuation);

            rankedViewingSearchOrganisation.TopName = rankedViewingSearchOrganisation.Names
                .RankHelperOrderByListOfDoubles(name => name.Ranks)
                .First();

            List<double> ranks = RankValueHelper.ApplyCompanySizeMultiplierToRanks(
                rankedViewingSearchOrganisation.TopName.Ranks,
                organisation.MinEmployees);
            rankedViewingSearchOrganisation.TopName.Ranks = ranks;

            string employerRefMatch = organisation.EmployerReference == query ? organisation.EmployerReference : null;
            string companyNumberMatch = organisation.CompanyNumber == query ? organisation.CompanyNumber : null;

            List<string> previousNames = rankedViewingSearchOrganisation.Names
                .Where((item, nameIndex) => nameIndex != 0)
                .Select(name => name.Name)
                .ToList();

            rankedViewingSearchOrganisation.ViewingSearchResult = new ViewingSearchResultOrganisationViewModel
            {
                OrganisationName = rankedViewingSearchOrganisation.Names[0].Name,
                OrganisationPreviousNames = previousNames,
                EmployerRef = employerRefMatch,
                CompanyNumber = companyNumberMatch,
                OrganisationId = organisation.OrganisationId,
                Status = organisation.Status,
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

    }
}

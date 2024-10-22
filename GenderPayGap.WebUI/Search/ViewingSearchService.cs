using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
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
        public string EncryptedId { get; set; }
        public string Address { get; set; }
        public List<string> Sectors { get; set; }

    }

    public class ViewingSearchService
    {

        private readonly List<SicSection> sicSections;
        private readonly Dictionary<string,string> sectorsDictionary;

        public ViewingSearchService(IDataRepository dataRepository)
        {
            sicSections = dataRepository.GetAll<SicSection>().ToList();
            sectorsDictionary = sicSections.ToDictionary(sicSection => sicSection.SicSectionId, sicSection => sicSection.Description);
        }

        public SearchApiResult Search(SearchPageViewModel searchParams)
        {
            List<SearchCachedOrganisation> allOrganisations = SearchRepository.CachedOrganisations
                .Where(o => o.IncludeInViewingService)
                .ToList();

            List<SearchCachedOrganisation> filteredOrganisations = FilterByOrganisations(allOrganisations, searchParams);

            if (searchParams.EmployerName == null)
            {
                List<SearchCachedOrganisation> orderedOrganisations =
                    filteredOrganisations.OrderBy(o => o.OrganisationName.OriginalValue).ToList();
                
                return new SearchApiResult
                {
                    Sectors = sectorsDictionary,
                    SearchParameters = searchParams,
                    Employers = orderedOrganisations.Select(ConvertToSearchApiResultEmployer).ToList()
                };
            }

            string query = searchParams.EmployerName.Trim().ToLower();

            bool queryContainsPunctuation = WordSplittingRegex.ContainsPunctuationCharacters(query);

            List<string> searchTerms = SearchHelper.ExtractSearchTermsFromQuery(query, queryContainsPunctuation);

            List<SearchCachedOrganisation> matchingOrganisations = GetMatchingOrganisationsByName(
                filteredOrganisations,
                searchTerms,
                query,
                queryContainsPunctuation);

            List<RankedViewingSearchOrganisation> organisationsWithRankings = CalculateOrganisationRankings(
                matchingOrganisations,
                searchTerms,
                query,
                queryContainsPunctuation,
                sicSections);

            List<RankedViewingSearchOrganisation> rankedOrganisations = searchParams.IsOrderByRelevance()
                ? OrderOrganisationsByRank(organisationsWithRankings)
                : OrderOrganisationsAlphabetically(organisationsWithRankings);
            
            return new SearchApiResult
            {
                Sectors = sectorsDictionary,
                SearchParameters = searchParams,
                Employers = rankedOrganisations.Select(ConvertRankedOrgsToSearchApiResultEmployer).ToList()
            };
        }

        private SearchApiResultEmployer ConvertRankedOrgsToSearchApiResultEmployer(RankedViewingSearchOrganisation rankedViewingSearchOrganisation)
        {
            string previousName = rankedViewingSearchOrganisation.ViewingSearchResult.OrganisationPreviousNames.Count > 0
                ? rankedViewingSearchOrganisation.ViewingSearchResult.OrganisationPreviousNames[0]
                : null;
            
            return new SearchApiResultEmployer
            {
                Id = rankedViewingSearchOrganisation.ViewingSearchResult.OrganisationId,
                EncId = rankedViewingSearchOrganisation.ViewingSearchResult.EncryptedId,
                Name = rankedViewingSearchOrganisation.ViewingSearchResult.OrganisationName,
                PreviousName = previousName,
                Address = rankedViewingSearchOrganisation.ViewingSearchResult.Address,
                Sectors = rankedViewingSearchOrganisation.ViewingSearchResult.Sectors
            };
        }

        private SearchApiResultEmployer ConvertToSearchApiResultEmployer(SearchCachedOrganisation searchCachedOrganisation)
        {
            string previousName = searchCachedOrganisation.OrganisationNames.Count > 1
                ? searchCachedOrganisation.OrganisationNames[1].OriginalValue
                : null;

            return new SearchApiResultEmployer
            {
                Id = searchCachedOrganisation.OrganisationId,
                EncId = searchCachedOrganisation.EncryptedId,
                Name = searchCachedOrganisation.OrganisationName.OriginalValue,
                PreviousName = previousName,
                Address = searchCachedOrganisation.Address,
                Sectors = searchCachedOrganisation.SicSectionIds
            };
        }

        private List<SearchCachedOrganisation> FilterByOrganisations(List<SearchCachedOrganisation> organisations,
            SearchPageViewModel searchParams)
        {
            List<int> selectedReportedLateYears = searchParams.GetReportedLateYearsAsInts();

            IEnumerable<SearchCachedOrganisation> filteredOrgs = organisations.AsEnumerable();

            if (searchParams.EmployerSize.Any())
            {
                filteredOrgs = filteredOrgs.Where(o => o.OrganisationSizes.Intersect(searchParams.EmployerSize).Any());
            }

            if (searchParams.Sector.Any())
            {
                filteredOrgs = filteredOrgs.Where(o => o.SicSectionIds.Intersect(searchParams.Sector).Any());
            }

            if (selectedReportedLateYears.Any())
            {
                filteredOrgs = filteredOrgs.Where(o => o.ReportedLateYears.Intersect(selectedReportedLateYears).Any());
            }

            return filteredOrgs.ToList();
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
            bool queryContainsPunctuation,
            List<SicSection> sicSections)
        {
            return matchingOrganisations
                .Select(organisation => CalculateRankForOrganisation(organisation, searchTerms, query, queryContainsPunctuation, sicSections))
                .ToList();
        }

        private static RankedViewingSearchOrganisation CalculateRankForOrganisation(SearchCachedOrganisation organisation,
            List<string> searchTerms,
            string query,
            bool queryContainsPunctuation,
            List<SicSection> sicSections)
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
                EncryptedId = organisation.EncryptedId,
                Address = organisation.Address,
                Sectors = organisation.SicSectionIds
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

    }
}

using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Models.Admin;
using GenderPayGap.WebUI.Search.CachedObjects;

namespace GenderPayGap.WebUI.Search
{

    internal class RankedAdminSearchOrganisation
    {
        public AdminSearchResultOrganisationViewModel AdminSearchResult { get; set; }
        public List<RankedName> Names { get; set; }
        public RankedName TopName { get; set; }

    }
    
    internal class RankedAdminSearchUser
    {
        public AdminSearchResultUserViewModel AdminSearchResult { get; set; }
        public List<RankedName> Values { get; set; }
        public RankedName TopValue { get; set; }

    }
    
    public class AdminSearchService
    {

        public AdminSearchResultsViewModel Search(string query, bool orderByRelevance)
        {
            query = query.Trim();

            bool queryContainsPunctuation = WordSplittingRegex.ContainsPunctuationCharacters(query);

            List<string> searchTerms = SearchHelper.ExtractSearchTermsFromQuery(query, queryContainsPunctuation);

            // Do this before we run the search, in case the cache is updated whilst the search is running
            DateTime timeDetailsLoaded = SearchRepository.CacheLastUpdated;

            var results = new AdminSearchResultsViewModel
            {
                OrganisationResults = SearchOrganisations(query, searchTerms, queryContainsPunctuation, orderByRelevance),
                UserResults = SearchUsers(query, searchTerms, queryContainsPunctuation, orderByRelevance),
                SearchCacheUpdatedSecondsAgo = (int) VirtualDateTime.Now.Subtract(timeDetailsLoaded).TotalSeconds,
            };
            return results;
        }

        #region Search Organisations

        private static List<AdminSearchResultOrganisationViewModel> SearchOrganisations(string query,
            List<string> searchTerms,
            bool queryContainsPunctuation,
            bool orderByRelevance)
        {
            List<SearchCachedOrganisation> allOrganisations = SearchRepository.CachedOrganisations;

            List<SearchCachedOrganisation> matchingOrganisations = GetMatchingOrganisations(allOrganisations, searchTerms, query, queryContainsPunctuation);

            List<RankedAdminSearchOrganisation> organisationsWithRankings = CalculateOrganisationRankings(matchingOrganisations, searchTerms, query, queryContainsPunctuation);

            List<RankedAdminSearchOrganisation> rankedOrganisations = orderByRelevance
                ? OrderOrganisationsByRank(organisationsWithRankings)
                : OrderOrganisationsAlphabetically(organisationsWithRankings);

            List<AdminSearchResultOrganisationViewModel> results = ConvertOrganisationsToSearchResults(rankedOrganisations);

            return results;
        }

        private static List<SearchCachedOrganisation> GetMatchingOrganisations(
            List<SearchCachedOrganisation> allOrganisations,
            List<string> searchTerms,
            string query,
            bool queryContainsPunctuation)
        {
            return allOrganisations
                .Where(
                    organisation =>
                    {
                        bool nameMatches = SearchHelper
                            .CurrentOrPreviousOrganisationNameMatchesSearchTerms(organisation, searchTerms, queryContainsPunctuation);
                        bool companyNumberMatches = organisation.CompanyNumber == query;
                        return nameMatches || companyNumberMatches;
                    })
                .ToList();
        }


        private static List<RankedAdminSearchOrganisation> CalculateOrganisationRankings(List<SearchCachedOrganisation> matchingOrganisations,
            List<string> searchTerms,
            string query,
            bool queryContainsPunctuation)
        {
            return matchingOrganisations
                .Select(organisation => CalculateRankForOrganisation(organisation, searchTerms, query, queryContainsPunctuation))
                .ToList();
        }

        private static RankedAdminSearchOrganisation CalculateRankForOrganisation(SearchCachedOrganisation organisation,
            List<string> searchTerms,
            string query,
            bool queryContainsPunctuation)
        {
            var rankedAdminSearchOrganisation = new RankedAdminSearchOrganisation
            {
                Names = new List<RankedName>()
            };

            rankedAdminSearchOrganisation.Names =
                RankValueHelper.GetRankedNames(organisation.OrganisationNames, searchTerms, query, queryContainsPunctuation);
            
            rankedAdminSearchOrganisation.TopName = rankedAdminSearchOrganisation.Names
                .RankHelperOrderByListOfDoubles(name => name.Ranks)
                .First();

            var ranks = RankValueHelper.ApplyCompanySizeMultiplierToRanks(
                rankedAdminSearchOrganisation.TopName.Ranks,
                organisation.MinEmployees);
            rankedAdminSearchOrganisation.TopName.Ranks = ranks;

            string companyNumberMatch = organisation.CompanyNumber == query ? organisation.CompanyNumber : null;

            var previousNames = rankedAdminSearchOrganisation.Names
                .Where((item, nameIndex) => nameIndex != 0)
                .Select(name => name.Name)
                .ToList();

            rankedAdminSearchOrganisation.AdminSearchResult = new AdminSearchResultOrganisationViewModel
            {
                OrganisationName = rankedAdminSearchOrganisation.Names[0].Name,
                OrganisationPreviousNames = previousNames,
                CompanyNumber = companyNumberMatch,
                OrganisationId = organisation.OrganisationId,
                Status = organisation.Status,
            };

            return rankedAdminSearchOrganisation;
        }
        
        private static List<RankedAdminSearchOrganisation> OrderOrganisationsByRank(List<RankedAdminSearchOrganisation> organisationsWithRankings)
        {
            return organisationsWithRankings
                .RankHelperOrderByListOfDoubles(org => org.TopName.Ranks)
                .ThenBy(org => org.Names[0].Name)
                .ToList();
        }

        private static List<RankedAdminSearchOrganisation> OrderOrganisationsAlphabetically(List<RankedAdminSearchOrganisation> organisationsWithRankings)
        {
            return organisationsWithRankings.OrderBy(o => o.Names[0].Name).ToList();
        }

        private static List<AdminSearchResultOrganisationViewModel> ConvertOrganisationsToSearchResults(
            List<RankedAdminSearchOrganisation> orderedOrganisations)
        {
            return orderedOrganisations
                .Select(organisation => organisation.AdminSearchResult)
                .ToList();
        }

        #endregion


        #region Search Users

        private static List<AdminSearchResultUserViewModel> SearchUsers(string query,
            List<string> searchTerms,
            bool queryContainsPunctuation,
            bool orderByRelevance)
        {
            List<SearchCachedUser> allUsers = SearchRepository.CachedUsers;

            List<SearchCachedUser> matchingUsers = GetMatchingUsers(allUsers, searchTerms);

            List<RankedAdminSearchUser> usersWithRankings = CalculateUserRankings(matchingUsers, searchTerms, query, queryContainsPunctuation);

            List<RankedAdminSearchUser> rankedUsers = orderByRelevance
                ? OrderUsersByRank(usersWithRankings)
                : OrderUsersAlphabetically(usersWithRankings);

            List<AdminSearchResultUserViewModel> results = ConvertUsersToSearchResults(rankedUsers);
            
            return results;
        }

        private static List<SearchCachedUser> GetMatchingUsers(List<SearchCachedUser> allUsers, List<string> searchTerms)
        {
            return allUsers
                .Where(user => user.FullName.Matches(searchTerms) || user.EmailAddress.Matches(searchTerms))
                .ToList();
        }
        
        private static List<RankedAdminSearchUser> CalculateUserRankings(List<SearchCachedUser> matchingUsers, List<string> searchTerms, string query, bool queryContainsPunctuation)
        {
            return matchingUsers
                .Select(user => CalculateRankForUser(user, searchTerms, query, queryContainsPunctuation))
                .ToList();
        }

        private static RankedAdminSearchUser CalculateRankForUser(SearchCachedUser user, List<string> searchTerms, string query, bool queryContainsPunctuation)
        {
            var rankedAdminSearchUser = new RankedAdminSearchUser
            {
                Values = new List<RankedName>()
            };
            
            rankedAdminSearchUser.Values.Add(CalculateRankForUserValue(user.FullName, searchTerms, query, queryContainsPunctuation));
            rankedAdminSearchUser.Values.Add(CalculateRankForUserValue(user.EmailAddress, searchTerms, query, queryContainsPunctuation));

            rankedAdminSearchUser.TopValue = rankedAdminSearchUser.Values
                .RankHelperOrderByListOfDoubles(value => value.Ranks)
                .First();
            
            rankedAdminSearchUser.AdminSearchResult = new AdminSearchResultUserViewModel
            {
                UserFullName = user.FullName.OriginalValue,
                UserEmailAddress = user.EmailAddress.OriginalValue,
                UserId = user.UserId,
                Status = user.Status
            };

            return rankedAdminSearchUser;
        }

        private static RankedName CalculateRankForUserValue(SearchReadyValue value, List<string> searchTerms, string query, bool queryContainsPunctuation)
        {
            var rankValues = new List<double>();
            
            rankValues.Add(RankValueHelper.CalculateRankValueForPrefixMatch(value, query));
            
            for (int searchTermIndex = 0; searchTermIndex < searchTerms.Count; searchTermIndex++)
            {
                string searchTerm = searchTerms[searchTermIndex];

                List<string> words = queryContainsPunctuation ? value.LowercaseWordsWithPunctuation : value.LowercaseWords;
                for (int wordIndex = 0; wordIndex < words.Count; wordIndex++)
                {
                    string word = words[wordIndex];

                    rankValues.Add(
                        CalculateRankValueForUserWordMatch(word, query, searchTerm, searchTermIndex, wordIndex));
                }
            }

            return new RankedName {Name = value.OriginalValue, Ranks = rankValues.OrderByDescending(r => r).Take(5).ToList()};
        }

        public static double CalculateRankValueForUserWordMatch(string word,
            string query,
            string searchTerm,
            int searchTermIndex,
            int wordIndex
            )
        {
            if (word.StartsWith(searchTerm))
            {
                double prefixAmount = (double) query.Length / (double) word.Length;
                
                double startOfSearchness = ((double) 4) / (searchTermIndex + wordIndex  + 4);

                return prefixAmount * startOfSearchness;
            }

            return 0;   
        }
        
        private static List<RankedAdminSearchUser> OrderUsersByRank(List<RankedAdminSearchUser> usersWithRankings)
        {
            return usersWithRankings
                .RankHelperOrderByListOfDoubles(user => user.TopValue.Ranks)
                .ThenBy(user => user.Values[0].Name)
                .ToList();
        }

        private static List<RankedAdminSearchUser> OrderUsersAlphabetically(List<RankedAdminSearchUser> usersWithRankings)
        {
            return usersWithRankings.OrderBy(u => u.AdminSearchResult.UserFullName).ToList();
        }

        private static List<AdminSearchResultUserViewModel> ConvertUsersToSearchResults(
            List<RankedAdminSearchUser> users
        )
        {
            return users
                .Select(user => user.AdminSearchResult)
                .ToList();
        }

        #endregion

    }

}

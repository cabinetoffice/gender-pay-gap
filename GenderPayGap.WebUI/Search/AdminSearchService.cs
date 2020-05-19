using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Models.Admin;
using GenderPayGap.WebUI.Search.CachedObjects;

namespace GenderPayGap.WebUI.Search
{
    
    internal class RankedAdminSearchOrganisation
    {
        
        public AdminSearchResultOrganisationViewModel AdminSearchResult { get; set; }
        public List<RankedAdminSearchOrganisationName> Names { get; set; }
        public RankedAdminSearchOrganisationName TopName { get; set; }

    }
    
    internal class RankedAdminSearchOrganisationName
    {
        public string Name { get; set; }
        public List<double> Ranks { get; set; }
    }

    
    public class AdminSearchService
    {
        
        public AdminSearchResultsViewModel Search(string query)
        {
            query = query.Trim();
            
            bool queryContainsPunctuation = WordSplittingRegex.ContainsPunctuationCharacters(query);
            
            List<string> searchTerms = SearchHelper.ExtractSearchTermsFromQuery(query, queryContainsPunctuation);

            // Do this before we run the search, in case the cache is updated whilst the search is running
            DateTime timeDetailsLoaded = SearchRepository.CacheLastUpdated; 

            var results = new AdminSearchResultsViewModel
            {
                OrganisationResults = SearchOrganisations(query, searchTerms, queryContainsPunctuation),
                UserResults = SearchUsers(searchTerms),

                SearchCacheUpdatedSecondsAgo = (int)VirtualDateTime.Now.Subtract(timeDetailsLoaded).TotalSeconds,
            };
            return results;
        }
        
        #region Search Organisations
        private List<AdminSearchResultOrganisationViewModel> SearchOrganisations(string query, List<string> searchTerms, bool queryContainsPunctuation)
        {
            List<SearchCachedOrganisation> allOrganisations = SearchRepository.CachedOrganisations;

            List<SearchCachedOrganisation> matchingOrganisations = GetMatchingOrganisations(allOrganisations, searchTerms, query, queryContainsPunctuation);

            List<RankedAdminSearchOrganisation> organisationsWithRankings = CalculateRankings(matchingOrganisations, searchTerms, query, queryContainsPunctuation);

            List<RankedAdminSearchOrganisation> rankedOrganisations = OrderByRank(organisationsWithRankings);
            
            List<AdminSearchResultOrganisationViewModel> results = ConvertOrganisationsToSearchResults(rankedOrganisations);

            return results;
 
        }

        private List<SearchCachedOrganisation> GetMatchingOrganisations(
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
                        bool employerRefMatches = organisation.EmployerReference == query;
                        bool companyNumberMatches = organisation.CompanyNumber == query;
                        return nameMatches || employerRefMatches || companyNumberMatches;
                    })
                .ToList();
        }

        
        private static List<RankedAdminSearchOrganisation> CalculateRankings(List<SearchCachedOrganisation> matchingOrganisations,
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
                Names = new List<RankedAdminSearchOrganisationName>(),
            };

            
            for (var nameIndex = 0; nameIndex < organisation.OrganisationNames.Count; nameIndex++)
            {
                SearchReadyValue name = organisation.OrganisationNames[nameIndex];
                rankedAdminSearchOrganisation.Names.Add(CalculateRankForName(name, searchTerms, query, queryContainsPunctuation, nameIndex));
            }

            rankedAdminSearchOrganisation.TopName = rankedAdminSearchOrganisation.Names
                .RankHelperOrderByListOfDoubles(name => name.Ranks)
                .First();

            var ranks = RankValueHelper.ApplyCompanySizeMultiplierToRanks(
                rankedAdminSearchOrganisation.TopName.Ranks,
                organisation.MinEmployees);
            rankedAdminSearchOrganisation.TopName.Ranks = ranks;
            
            string employerRefMatch = organisation.EmployerReference == query
                ? organisation.EmployerReference
                : null;

            string companyNumberMatch = organisation.CompanyNumber == query
                ? organisation.CompanyNumber
                : null;
            
            var previousNames = rankedAdminSearchOrganisation.Names
                .Where((item, nameIndex) => nameIndex != 0)
                .Select(name => name.Name)
                .ToList();
            
            rankedAdminSearchOrganisation.AdminSearchResult = new AdminSearchResultOrganisationViewModel
            {
                OrganisationName = rankedAdminSearchOrganisation.Names[0].Name,
                OrganisationPreviousNames = previousNames,
                EmployerRef = employerRefMatch,
                CompanyNumber = companyNumberMatch,
                OrganisationId = organisation.OrganisationId,
                Status = organisation.Status,
            };
            
            return rankedAdminSearchOrganisation;
        }

        private static RankedAdminSearchOrganisationName CalculateRankForName(SearchReadyValue name,
            List<string> searchTerms,
            string query,
            bool queryContainsPunctuation,
            int nameIndex)
        {
            var rankValues = new List<double>();

            rankValues.Add(RankValueHelper.CalculateRankValueForPrefixMatch(name, query));

            rankValues.Add(RankValueHelper.CalculateRankValueForAcronymMatch(name, query));

            for (int searchTermIndex = 0; searchTermIndex < searchTerms.Count; searchTermIndex++)
            {
                string searchTerm = searchTerms[searchTermIndex];

                List<string> words = queryContainsPunctuation ? name.LowercaseWordsWithPunctuation : name.LowercaseWords;
                for (int wordIndex = 0; wordIndex < words.Count; wordIndex++)
                {
                    string word = words[wordIndex];

                    rankValues.Add(RankValueHelper.CalculateRankValueForWordMatch(word, query, searchTerm, searchTermIndex, wordIndex, nameIndex));
                }
            }

            return new RankedAdminSearchOrganisationName
            {
                Name = name.OriginalValue,
                Ranks = rankValues.OrderByDescending(r => r).Take(5).ToList()
            };
        }

        private List<RankedAdminSearchOrganisation> OrderByRank(List<RankedAdminSearchOrganisation> organisationsWithRankings)
        {
            return organisationsWithRankings
                .RankHelperOrderByListOfDoubles(org => org.TopName.Ranks)
                .ThenBy(org => org.Names[0].Name)
                .ToList();
        }
        
        private static List<AdminSearchResultOrganisationViewModel> ConvertOrganisationsToSearchResults(List<RankedAdminSearchOrganisation> orderedOrganisations)
        {
            return orderedOrganisations
                .Select(organisation => organisation.AdminSearchResult)
                .ToList();
        }
        
        #endregion


        #region Search Users
        private List<AdminSearchResultUserViewModel> SearchUsers(List<string> searchTerms)
        {
            List<SearchCachedUser> allUsers = SearchRepository.CachedUsers;

            List<SearchCachedUser> matchingUsers = GetMatchingUsers(allUsers, searchTerms);

            List<SearchCachedUser> matchingUsersOrderedByName =
                matchingUsers.OrderBy(u => u.FullName.OriginalValue).ToList();

            List<AdminSearchResultUserViewModel> matchingUsersWithHighlightedMatches =
                ConvertType(matchingUsersOrderedByName);

            return matchingUsersWithHighlightedMatches;
        }

        private List<SearchCachedUser> GetMatchingUsers(List<SearchCachedUser> allUsers, List<string> searchTerms)
        {
            return allUsers
                .Where(user => user.FullName.Matches(searchTerms) || user.EmailAddress.Matches(searchTerms))
                .ToList();
        }

        private List<AdminSearchResultUserViewModel> ConvertType(
            List<SearchCachedUser> users
        )
        {
            return users
                .Select(
                    user => new AdminSearchResultUserViewModel
                    {
                        UserId = user.UserId,
                        UserFullName = user.FullName.OriginalValue,
                        UserEmailAddress = user.EmailAddress.OriginalValue,
                        Status = user.Status
                    })
                .ToList();
        }
        #endregion
        
    }

}

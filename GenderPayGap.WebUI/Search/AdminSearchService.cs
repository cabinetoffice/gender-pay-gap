using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Models.Admin;
using GenderPayGap.WebUI.Search.CachedObjects;

namespace GenderPayGap.WebUI.Search
{
    public class AdminSearchService
    {

        public AdminSearchResultsViewModel Search(string query)
        {
            query = query.Trim();
            
            bool queryContainsPunctuation = WordSplittingRegex.ContainsPunctuationCharacters(query);
            
            List<string> searchTerms = SearchHelper.ExtractSearchTermsFromQuery(query, queryContainsPunctuation);

            DateTime timeDetailsLoaded = SearchRepository.CacheLastUpdated; // Do this before we run the search, in case the cache is updated whilst the search is running

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

            List<SearchCachedOrganisation> matchingOrganisationsOrderedByName =
                matchingOrganisations.OrderBy(o => o.OrganisationName.OriginalValue.ToLower()).ToList();

            List<AdminSearchResultOrganisationViewModel> matchingOrganisationsWithHighlightedMatches =
                HighlightOrganisationMatches(matchingOrganisationsOrderedByName, searchTerms, query);

            return matchingOrganisationsWithHighlightedMatches;
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
                        bool nameMatches = CurrentOrPreviousOrganisationNameMatchesSearchTerms(organisation, searchTerms, queryContainsPunctuation);
                        bool employerRefMatches = organisation.EmployerReference == query;
                        bool companyNumberMatches = organisation.CompanyNumber == query;
                        return nameMatches || employerRefMatches || companyNumberMatches;
                    })
                .ToList();
        }

        private static bool CurrentOrPreviousOrganisationNameMatchesSearchTerms(
            SearchCachedOrganisation organisation,
            List<string> searchTerms,
            bool queryContainsPunctuation)
        {
            return organisation.OrganisationNames.Any(on => on.Matches(searchTerms, queryContainsPunctuation));
        }

        private List<AdminSearchResultOrganisationViewModel> HighlightOrganisationMatches(
            List<SearchCachedOrganisation> organisations,
            List<string> searchTerms,
            string query)
        {
            return organisations
                .Select(
                    organisation =>
                    {
                        AdminSearchMatchViewModel matchGroupsForCurrentName = GetMatchGroups(organisation.OrganisationName.OriginalValue, searchTerms);

                        IEnumerable<SearchReadyValue> previousNames = organisation.OrganisationNames
                            .Where(on => on.OriginalValue != organisation.OrganisationName.OriginalValue);

                        List<AdminSearchMatchViewModel> matchGroupsForPreviousNames = previousNames
                            .Where(on => on.Matches(searchTerms))
                            .Select(on => GetMatchGroups(on.OriginalValue, searchTerms))
                            .ToList();

                        string employerRefMatch = organisation.EmployerReference == query
                            ? organisation.EmployerReference
                            : null;

                        string companyNumberMatch = organisation.CompanyNumber == query
                            ? organisation.CompanyNumber
                            : null;

                        return new AdminSearchResultOrganisationViewModel
                        {
                            OrganisationName = matchGroupsForCurrentName,
                            OrganisationPreviousNames = matchGroupsForPreviousNames,
                            EmployerRef = employerRefMatch,
                            CompanyNumber = companyNumberMatch,
                            OrganisationId = organisation.OrganisationId,
                            Status = organisation.Status
                        };
                    })
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
                HighlightUserMatches(matchingUsersOrderedByName, searchTerms);

            return matchingUsersWithHighlightedMatches;
        }

        private List<SearchCachedUser> GetMatchingUsers(List<SearchCachedUser> allUsers, List<string> searchTerms)
        {
            return allUsers
                .Where(user => user.FullName.Matches(searchTerms) || user.EmailAddress.Matches(searchTerms))
                .ToList();
        }

        private List<AdminSearchResultUserViewModel> HighlightUserMatches(
            List<SearchCachedUser> users,
            List<string> searchTerms
        )
        {
            return users
                .Select(
                    user =>
                    {
                        AdminSearchMatchViewModel matchGroupsForFullName = GetMatchGroups(user.FullName.OriginalValue, searchTerms);
                        AdminSearchMatchViewModel matchGroupsForEmailAddress = GetMatchGroups(user.EmailAddress.OriginalValue, searchTerms);

                        return new AdminSearchResultUserViewModel
                        {
                            UserId = user.UserId,
                            UserFullName = matchGroupsForFullName,
                            UserEmailAddress = matchGroupsForEmailAddress,
                            Status = user.Status
                        };
                    })
                .ToList();
        }
        #endregion


        #region Helpers
        private AdminSearchMatchViewModel GetMatchGroups(string organisationName, List<string> searchTerms)
        {
            var matchGroups = new List<AdminSearchMatchGroupViewModel>();

            var stillSearching = true;
            var searchStart = 0;
            while (stillSearching)
            {
                AdminSearchMatchGroupViewModel nextMatch = GetNextMatch(organisationName, searchTerms, searchStart);
                if (nextMatch != null)
                {
                    matchGroups.Add(nextMatch);
                    searchStart = nextMatch.Start + nextMatch.Length;
                    if (searchStart >= organisationName.Length)
                    {
                        stillSearching = false;
                    }
                }
                else
                {
                    stillSearching = false;
                }
            }

            return new AdminSearchMatchViewModel { Text = organisationName, MatchGroups = matchGroups };
        }

        private static AdminSearchMatchGroupViewModel GetNextMatch(string organisationName, List<string> searchTerms, int searchStart)
        {
            var possibleMatches = new List<AdminSearchMatchGroupViewModel>();

            foreach (string searchTerm in searchTerms)
            {
                int matchStart = organisationName.IndexOf(searchTerm, searchStart, StringComparison.InvariantCultureIgnoreCase);
                if (matchStart != -1)
                {
                    possibleMatches.Add(new AdminSearchMatchGroupViewModel { Start = matchStart, Length = searchTerm.Length });
                }
            }

            return possibleMatches
                .OrderBy(m => m.Start)
                .ThenByDescending(m => m.Length)
                .FirstOrDefault();
        }
        #endregion

    }

}

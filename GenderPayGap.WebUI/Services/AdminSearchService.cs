using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Models.Admin;
using Microsoft.EntityFrameworkCore;

namespace GenderPayGap.WebUI.Services
{
    public class AdminSearchService
    {

        private readonly IDataRepository dataRepository;

        public AdminSearchService(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        public AdminSearchResultsViewModel Search(string query)
        {
            List<string> searchTerms = ExtractSearchTermsFromQuery(query);

            DateTime loadingStart = DateTime.Now;
            List<Organisation> allOrganisations = LoadAllOrganisations();
            List<User> allUsers = LoadAllUsers();
            DateTime loadingEnd = DateTime.Now;

            DateTime filteringStart = DateTime.Now;
            List<Organisation> matchingOrganisations = GetMatchingOrganisations(allOrganisations, searchTerms, query);
            List<User> matchingUsers = GetMatchingUsers(allUsers, searchTerms);
            DateTime filteringEnd = DateTime.Now;

            DateTime orderingStart = DateTime.Now;
            List<Organisation> matchingOrganisationsOrderedByName =
                matchingOrganisations.OrderBy(o => o.OrganisationName.ToLower()).ToList();
            List<User> matchingUsersOrderedByName =
                matchingUsers.OrderBy(u => u.Fullname).ToList();
            DateTime orderingEnd = DateTime.Now;

            DateTime highlightingStart = DateTime.Now;
            List<AdminSearchResultOrganisationViewModel> matchingOrganisationsWithHighlightedMatches =
                HighlightOrganisationMatches(matchingOrganisationsOrderedByName, searchTerms, query);
            List<AdminSearchResultUserViewModel> matchingUsersWithHighlightedMatches =
                HighlightUserMatches(matchingUsersOrderedByName, searchTerms);
            DateTime highlightingEnd = DateTime.Now;

            var results = new AdminSearchResultsViewModel {
                OrganisationResults = matchingOrganisationsWithHighlightedMatches,
                UserResults = matchingUsersWithHighlightedMatches,
                LoadingMilliSeconds = loadingEnd.Subtract(loadingStart).TotalMilliseconds,
                FilteringMilliSeconds = filteringEnd.Subtract(filteringStart).TotalMilliseconds,
                OrderingMilliSeconds = orderingEnd.Subtract(orderingStart).TotalMilliseconds,
                HighlightingMilliSeconds = highlightingEnd.Subtract(highlightingStart).TotalMilliseconds
            };
            return results;
        }

        private List<string> ExtractSearchTermsFromQuery(string query)
        {
            return query.Split(" ", StringSplitOptions.RemoveEmptyEntries)
                .Select(st => st.ToLower())
                .ToList();
        }

        private List<Organisation> LoadAllOrganisations()
        {
            return dataRepository
                .GetAll<Organisation>()
                .Include(o => o.OrganisationNames)
                .ToList();
        }

        private List<User> LoadAllUsers()
        {
            return dataRepository
                .GetAll<User>()
                .ToList();
        }

        private List<Organisation> GetMatchingOrganisations(
            List<Organisation> allOrganisations,
            List<string> searchTerms,
            string query)
        {
            return allOrganisations
                .Where(
                    organisation => {
                        bool nameMatches = CurrentOrPreviousOrganisationNameMatchesSearchTerms(organisation, searchTerms);
                        bool employerRefMatches = organisation.EmployerReference?.Trim() == query.Trim();
                        bool companyNumberMatches = organisation.CompanyNumber?.Trim() == query.Trim();
                        return nameMatches || employerRefMatches || companyNumberMatches;
                    })
                .ToList();
        }

        private List<User> GetMatchingUsers(List<User> allUsers, List<string> searchTerms)
        {
            return allUsers
                .Where(user => NameMatchesSearchTerms(user.Fullname, searchTerms) || NameMatchesSearchTerms(user.EmailAddress, searchTerms))
                .ToList();
        }

        private bool CurrentOrPreviousOrganisationNameMatchesSearchTerms(Organisation organisation, List<string> searchTerms)
        {
            return organisation.OrganisationNames.Any(on => NameMatchesSearchTerms(on.Name, searchTerms));
        }

        private bool NameMatchesSearchTerms(string name, List<string> searchTerms)
        {
            return searchTerms.All(st => name.ToLower().Contains(st));
        }

        private List<AdminSearchResultOrganisationViewModel> HighlightOrganisationMatches(
            List<Organisation> organisations,
            List<string> searchTerms,
            string query)
        {
            return organisations
                .Select(
                    organisation => {
                        AdminSearchMatchViewModel matchGroupsForCurrentName = GetMatchGroups(organisation.OrganisationName, searchTerms);

                        IEnumerable<string> previousNames = organisation.OrganisationNames
                            .Select(on => on.Name)
                            .Except(new[] {organisation.OrganisationName});

                        List<AdminSearchMatchViewModel> matchGroupsForPreviousNames = previousNames
                            .Where(on => NameMatchesSearchTerms(on, searchTerms))
                            .Select(on => GetMatchGroups(on, searchTerms))
                            .ToList();

                        string employerRefMatch = organisation.EmployerReference?.Trim() == query.Trim()
                            ? organisation.EmployerReference
                            : null;

                        string companyNumberMatch = organisation.CompanyNumber?.Trim() == query.Trim()
                            ? organisation.CompanyNumber
                            : null;

                        return new AdminSearchResultOrganisationViewModel {
                            OrganisationName = matchGroupsForCurrentName,
                            OrganisationPreviousNames = matchGroupsForPreviousNames,
                            EmployerRef = employerRefMatch,
                            CompanyNumber = companyNumberMatch,
                            OrganisationId = organisation.OrganisationId
                        };
                    })
                .ToList();
        }

        private List<AdminSearchResultUserViewModel> HighlightUserMatches(
            List<User> users,
            List<string> searchTerms
        )
        {
            return users
                .Select(
                    user => {
                        AdminSearchMatchViewModel matchGroupsForFullName = GetMatchGroups(user.Fullname, searchTerms);
                        AdminSearchMatchViewModel matchGroupsForEmailAddress = GetMatchGroups(user.EmailAddress, searchTerms);

                        return new AdminSearchResultUserViewModel {
                            UserFullName = matchGroupsForFullName, UserEmailAddress = matchGroupsForEmailAddress, UserId = user.UserId
                        };
                    })
                .ToList();
        }

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

            return new AdminSearchMatchViewModel {Text = organisationName, MatchGroups = matchGroups};
        }

        private static AdminSearchMatchGroupViewModel GetNextMatch(string organisationName, List<string> searchTerms, int searchStart)
        {
            var possibleMatches = new List<AdminSearchMatchGroupViewModel>();

            foreach (string searchTerm in searchTerms)
            {
                int matchStart = organisationName.IndexOf(searchTerm, searchStart, StringComparison.InvariantCultureIgnoreCase);
                if (matchStart != -1)
                {
                    possibleMatches.Add(new AdminSearchMatchGroupViewModel {Start = matchStart, Length = searchTerm.Length});
                }
            }

            return possibleMatches
                .OrderBy(m => m.Start)
                .ThenByDescending(m => m.Length)
                .FirstOrDefault();
        }

    }
}

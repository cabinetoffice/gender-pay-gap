using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Models.Admin;
using Microsoft.EntityFrameworkCore;

namespace GenderPayGap.WebUI.Services
{
    internal class AdminSearchServiceOrganisation
    {
        public long OrganisationId { get; set; }
        public string OrganisationName { get; set; }
        public List<string> OrganisationNames { get; set; } // All names (current and previous)
        public string CompanyNumber { get; set; }
        public string EmployerReference { get; set; }
    }

    internal class AdminSearchServiceUser
    {
        public long UserId { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
    }

    public class AdminSearchService
    {
        private readonly IDataRepository dataRepository;

        private static List<AdminSearchServiceOrganisation> cachedOrganisations;
        private static List<AdminSearchServiceUser> cachedUsers;
        private static DateTime cacheLastUpdated = DateTime.MinValue;

        public AdminSearchService(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        public static void StartCacheUpdateThread()
        {
            CustomLogger.Information("Starting timer (AdminSearchService.StartCacheUpdateThread)");

            TimerCallback timerCallback = state =>
            {
                CustomLogger.Information("Starting cache update (AdminSearchService.StartCacheUpdateThread)");

                var dataRepository = MvcApplication.ContainerIoC.Resolve<IDataRepository>();
                List<AdminSearchServiceOrganisation> allOrganisations = LoadAllOrganisations(dataRepository);
                List<AdminSearchServiceUser> allUsers = LoadAllUsers(dataRepository);

                cachedOrganisations = allOrganisations;
                cachedUsers = allUsers;
                cacheLastUpdated = VirtualDateTime.Now;

                CustomLogger.Information("Finished cache update (AdminSearchService.StartCacheUpdateThread)");
            };

            new Timer(
                timerCallback,
                null,
                dueTime: TimeSpan.FromSeconds(10), // How long to wait before the cache is first updated
                period: TimeSpan.FromMinutes(1));  // How often is the cache updated

            CustomLogger.Information("Started timer (AdminSearchService.StartCacheUpdateThread)");
        }

        public AdminSearchResultsViewModel Search(string query)
        {
            List<string> searchTerms = ExtractSearchTermsFromQuery(query);

            List<AdminSearchServiceOrganisation> allOrganisations;
            List<AdminSearchServiceUser> allUsers;
            DateTime timeDetailsLoaded;
            bool usedCache;

            DateTime loadingStart = VirtualDateTime.Now;
            if (cacheLastUpdated < VirtualDateTime.Now.AddSeconds(-70))
            {
                allOrganisations = LoadAllOrganisations(dataRepository);
                allUsers = LoadAllUsers(dataRepository);
                timeDetailsLoaded = VirtualDateTime.Now;
                usedCache = false;
            }
            else
            {
                allOrganisations = cachedOrganisations;
                allUsers = cachedUsers;
                timeDetailsLoaded = cacheLastUpdated;
                usedCache = true;
            }
            DateTime loadingEnd = VirtualDateTime.Now;

            DateTime filteringStart = VirtualDateTime.Now;
            List<AdminSearchServiceOrganisation> matchingOrganisations = GetMatchingOrganisations(allOrganisations, searchTerms, query);
            List<AdminSearchServiceUser> matchingUsers = GetMatchingUsers(allUsers, searchTerms);
            DateTime filteringEnd = VirtualDateTime.Now;

            DateTime orderingStart = VirtualDateTime.Now;
            List<AdminSearchServiceOrganisation> matchingOrganisationsOrderedByName =
                matchingOrganisations.OrderBy(o => o.OrganisationName.ToLower()).ToList();
            List<AdminSearchServiceUser> matchingUsersOrderedByName =
                matchingUsers.OrderBy(u => u.FullName).ToList();
            DateTime orderingEnd = VirtualDateTime.Now;

            DateTime highlightingStart = VirtualDateTime.Now;
            List<AdminSearchResultOrganisationViewModel> matchingOrganisationsWithHighlightedMatches =
                HighlightOrganisationMatches(matchingOrganisationsOrderedByName, searchTerms, query);
            List<AdminSearchResultUserViewModel> matchingUsersWithHighlightedMatches =
                HighlightUserMatches(matchingUsersOrderedByName, searchTerms);
            DateTime highlightingEnd = VirtualDateTime.Now;

            var results = new AdminSearchResultsViewModel {
                OrganisationResults = matchingOrganisationsWithHighlightedMatches,
                UserResults = matchingUsersWithHighlightedMatches,

                LoadingMilliSeconds = loadingEnd.Subtract(loadingStart).TotalMilliseconds,
                FilteringMilliSeconds = filteringEnd.Subtract(filteringStart).TotalMilliseconds,
                OrderingMilliSeconds = orderingEnd.Subtract(orderingStart).TotalMilliseconds,
                HighlightingMilliSeconds = highlightingEnd.Subtract(highlightingStart).TotalMilliseconds,

                SearchCacheUpdatedSecondsAgo = (int)VirtualDateTime.Now.Subtract(timeDetailsLoaded).TotalSeconds,
                UsedCache = usedCache
            };
            return results;
        }

        private List<string> ExtractSearchTermsFromQuery(string query)
        {
            return query.Split(" ", StringSplitOptions.RemoveEmptyEntries)
                .Select(st => st.ToLower())
                .ToList();
        }

        private static List<AdminSearchServiceOrganisation> LoadAllOrganisations(IDataRepository repository)
        {
            return repository
                .GetAll<Organisation>()
                .Include(o => o.OrganisationNames)
                .Select(o => new AdminSearchServiceOrganisation
                {
                    OrganisationId = o.OrganisationId,
                    OrganisationName = o.OrganisationName,
                    CompanyNumber = o.CompanyNumber,
                    EmployerReference = o.EmployerReference,
                    OrganisationNames = o.OrganisationNames.Select(on => on.Name).ToList()
                })
                .ToList();
        }

        private static List<AdminSearchServiceUser> LoadAllUsers(IDataRepository repository)
        {
            return repository
                .GetAll<User>()
                .Select(u => new AdminSearchServiceUser
                {
                    UserId = u.UserId,
                    FullName = u.Fullname,
                    EmailAddress = u.EmailAddress
                })
                .ToList();
        }

        private List<AdminSearchServiceOrganisation> GetMatchingOrganisations(
            List<AdminSearchServiceOrganisation> allOrganisations,
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

        private List<AdminSearchServiceUser> GetMatchingUsers(List<AdminSearchServiceUser> allUsers, List<string> searchTerms)
        {
            return allUsers
                .Where(user => NameMatchesSearchTerms(user.FullName, searchTerms) || NameMatchesSearchTerms(user.EmailAddress, searchTerms))
                .ToList();
        }

        private bool CurrentOrPreviousOrganisationNameMatchesSearchTerms(AdminSearchServiceOrganisation organisation, List<string> searchTerms)
        {
            return organisation.OrganisationNames.Any(on => NameMatchesSearchTerms(on, searchTerms));
        }

        private bool NameMatchesSearchTerms(string name, List<string> searchTerms)
        {
            return searchTerms.All(st => name.ToLower().Contains(st));
        }

        private List<AdminSearchResultOrganisationViewModel> HighlightOrganisationMatches(
            List<AdminSearchServiceOrganisation> organisations,
            List<string> searchTerms,
            string query)
        {
            return organisations
                .Select(
                    organisation => {
                        AdminSearchMatchViewModel matchGroupsForCurrentName = GetMatchGroups(organisation.OrganisationName, searchTerms);

                        IEnumerable<string> previousNames = organisation.OrganisationNames
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
            List<AdminSearchServiceUser> users,
            List<string> searchTerms
        )
        {
            return users
                .Select(
                    user => {
                        AdminSearchMatchViewModel matchGroupsForFullName = GetMatchGroups(user.FullName, searchTerms);
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

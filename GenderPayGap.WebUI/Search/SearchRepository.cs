using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Search.CachedObjects;
using Microsoft.EntityFrameworkCore;

namespace GenderPayGap.WebUI.Search
{

    public static class SearchRepository
    {

        internal static List<SearchCachedOrganisation> CachedOrganisations { get; private set; }
        internal static Dictionary<string, int> OrganisationNameWords { get; private set; }
        internal static int MaxOrganisationNameWords { get; private set; }
        internal static int NumberOfOrganisations { get; private set; }

        internal static List<SearchCachedUser> CachedUsers { get; private set; }
        internal static DateTime CacheLastUpdated { get; private set; } = DateTime.MinValue;


        public static void LoadSearchDataIntoCache()
        {
            using (ILifetimeScope scope = Startup.ContainerIoC.BeginLifetimeScope())
            {
                var dataRepository = scope.Resolve<IDataRepository>();

                CachedOrganisations = LoadAllOrganisations(dataRepository);
                CalculateOrganisationWords();
                NumberOfOrganisations = CachedOrganisations.Count;

                CachedUsers = LoadAllUsers(dataRepository);

                CacheLastUpdated = VirtualDateTime.Now;
            }
        }

        private static void CalculateOrganisationWords()
        {
            var allNames = CachedOrganisations.SelectMany(org => org.OrganisationNames);

            var allWords = allNames.SelectMany(name => name.LowercaseWords.Concat(name.LowercaseWordsWithPunctuation).Distinct());

            Dictionary<string, int> groupedWords = allWords
                .GroupBy(
                    word => word,
                    word => 1,
                    (word, listOfOnes) => new Tuple<string, int>(word, listOfOnes.Count()))
                .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);

            OrganisationNameWords = groupedWords;
            MaxOrganisationNameWords = groupedWords.Count > 0
                ? groupedWords.Values.Max()
                : 1 /* groupedWords would only be empty if the database is empty - we use 1 to prevent a divide-by-zero error */;
        }

        private static List<SearchCachedOrganisation> LoadAllOrganisations(IDataRepository repository)
        {
            DateTime start = VirtualDateTime.Now;

            // IMPORTANT: This variable isn't used, but running this query makes the next query much faster
            var allOrgsWithNames = repository.GetAll<Organisation>()
                .Include(o => o.OrganisationNames)
                .ToList();

            CustomLogger.Information($"Search Repository: Time taken to load Names: {VirtualDateTime.Now.Subtract(start).TotalSeconds} seconds");

            // IMPORTANT: This variable isn't used, but running this query makes the next query much faster
            var allOrgsWithReturns = repository.GetAll<Organisation>()
                .Include(o => o.Returns)
                .ToList();

            CustomLogger.Information($"Search Repository: Time taken to load Returns: {VirtualDateTime.Now.Subtract(start).TotalSeconds} seconds");

            // IMPORTANT: This variable isn't used, but running this query makes the next query much faster
            var allOrgsWithScopes = repository.GetAll<Organisation>()
                .Include(o => o.OrganisationScopes)
                .ToList();

            CustomLogger.Information($"Search Repository: Time taken to load Scopes: {VirtualDateTime.Now.Subtract(start).TotalSeconds} seconds");

            // IMPORTANT: This variable isn't used, but running this query makes the next query much faster
            var allOrgsWithAddresses = repository.GetAll<Organisation>()
                .Include(o => o.OrganisationAddresses)
                .ToList();

            CustomLogger.Information($"Search Repository: Time taken to load Addresses: {VirtualDateTime.Now.Subtract(start).TotalSeconds} seconds");

            List<Organisation> allOrganisations = repository
                .GetAll<Organisation>()
                //.Include(o => o.OrganisationNames) // Moved into separate pre-load query 
                //.Include(o => o.Returns) // Moved into separate pre-load query 
                //.Include(o => o.OrganisationScopes) // Moved into separate pre-load query 
                .Include(o => o.OrganisationSicCodes)
                .ThenInclude(osc => osc.SicCode)
                .ThenInclude(sc => sc.SicSection)
                .ToList();

            CustomLogger.Information($"Search Repository: Time taken to load Organisations: {VirtualDateTime.Now.Subtract(start).TotalSeconds} seconds");

            List<SearchCachedOrganisation> searchCachedOrganisations = allOrganisations
                .Select(
                    o =>
                    {
                        var submittedReports = o.Returns.Where(r => r.Status == ReturnStatuses.Submitted).ToList();

                        return new SearchCachedOrganisation
                            {
                                OrganisationId = o.OrganisationId,
                                OrganisationName = new SearchReadyValue(o.OrganisationName),
                                CompanyNumber = o.CompanyNumber?.Trim(),
                                OrganisationNames =
                                    o.OrganisationNames.OrderByDescending(n => n.Created)
                                        .Select(on => new SearchReadyValue(on.Name))
                                        .ToList(),
                                MinEmployees = o.GetLatestReturn()?.MinEmployees ?? 0,
                                Status = o.Status,
                                OrganisationSizes = submittedReports.Select(r => r.OrganisationSize).Distinct().ToList(),
                                SicSectionIds = o.OrganisationSicCodes.Select(osc => osc.SicCode.SicSection.SicSectionId).Distinct().ToList(),
                                ReportedLateYears = submittedReports.Where(r => r.IsLateSubmission).Select(r => r.AccountingDate.Year).ToList(),
                                IncludeInViewingService = GetIncludeInViewingService(o),
                                Address = o.GetLatestAddress()?.GetAddressString() ?? ""
                            };
                    })
                .ToList();

            CustomLogger.Information($"Search Repository: Time taken to convert Organisations into SearchCachedOrganisations: {VirtualDateTime.Now.Subtract(start).TotalSeconds} seconds");

            return searchCachedOrganisations;
        }

        private static bool GetIncludeInViewingService(Organisation organisation)
        {
            return OrganisationIsActiveAndInScope(organisation) || OrganisationHasSubmittedReturns(organisation);
        }

        private static bool OrganisationIsActiveAndInScope(Organisation organisation)
        {
            return organisation.Status == OrganisationStatuses.Active
                   && organisation.OrganisationScopes.Any(
                       sc => sc.Status == ScopeRowStatuses.Active
                             && (sc.ScopeStatus == ScopeStatuses.InScope || sc.ScopeStatus == ScopeStatuses.PresumedInScope));
        }

        private static bool OrganisationHasSubmittedReturns(Organisation organisation)
        {
            return (organisation.Status == OrganisationStatuses.Active || organisation.Status == OrganisationStatuses.Retired)
                   && organisation.Returns.Any(r => r.Status == ReturnStatuses.Submitted);
        }

        private static List<SearchCachedUser> LoadAllUsers(IDataRepository repository)
        {
            DateTime start = VirtualDateTime.Now;

            var allUsers = repository
                .GetAll<User>()
                .Select(
                    u => new SearchCachedUser
                    {
                        UserId = u.UserId,
                        FullName = new SearchReadyValue(u.Fullname),
                        EmailAddress = new SearchReadyValue(u.EmailAddress),
                        Status = u.Status
                    })
                .ToList();

            CustomLogger.Information($"Search Repository: Time taken to load Users: {VirtualDateTime.Now.Subtract(start).TotalSeconds} seconds");

            return allUsers;
        }

    }
}

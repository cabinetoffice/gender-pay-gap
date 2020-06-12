using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Search.CachedObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

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
            var dataRepository = Global.ContainerIoC.Resolve<IDataRepository>();

            CachedOrganisations = LoadAllOrganisations(dataRepository);
            CalculateOrganisationWords();
            NumberOfOrganisations = CachedOrganisations.Count;

            CachedUsers = LoadAllUsers(dataRepository);

            CacheLastUpdated = VirtualDateTime.Now;
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
            return repository
                .GetAll<Organisation>()
                .Include(o => o.OrganisationNames)
                .Include(o => o.Returns)
                .Include(o => o.OrganisationSicCodes)
                .ToList()
                .Select(
                    o =>
                    {
                        var sicCodeSynonyms = o.OrganisationSicCodes.Select(osc => osc.SicCode.Synonyms)
                            .Where(s => s != null)
                            .Select(s => new SearchReadyValue(s))
                            .ToList();

                        foreach (var osc in o.OrganisationSicCodes)
                        {
                                sicCodeSynonyms.Add(new SearchReadyValue(osc.SicCode.Description));
                        }
                        
                        return new SearchCachedOrganisation
                            {
                                OrganisationId = o.OrganisationId,
                                EncryptedId = o.GetEncryptedId(),
                                OrganisationName = new SearchReadyValue(o.OrganisationName),
                                CompanyNumber = o.CompanyNumber?.Trim(),
                                EmployerReference = o.EmployerReference?.Trim(),
                                OrganisationNames =
                                    o.OrganisationNames.OrderByDescending(n => n.Created)
                                        .Select(on => new SearchReadyValue(@on.Name))
                                        .ToList(),
                                MinEmployees = o.GetLatestReturn()?.MinEmployees ?? 0,
                                Status = o.Status,
                                OrganisationSizes = o.Returns.Where(r => r.Status == ReturnStatuses.Submitted).Select(r => r.OrganisationSize).Distinct().ToList(),
                                SicSectionIds =
                                    o.OrganisationSicCodes.Select(osc => Convert.ToChar(osc.SicCode.SicSection.SicSectionId)).ToList(),
                                ReportingYears = o.Returns.Where(r => r.Status == ReturnStatuses.Submitted).Select(r => r.AccountingDate.Year).ToList(),
                                DateOfLatestReport =
                                    o.GetLatestReturn() != null ? o.GetLatestReturn().StatusDate.Date : new DateTime(1999, 1, 1),
                                ReportedWithCompanyLinkToGpgInfo = o.Returns.Where(r => r.Status == ReturnStatuses.Submitted).Any(r => r.CompanyLinkToGPGInfo != null),
                                ReportedLate = o.Returns.Where(r => r.Status == ReturnStatuses.Submitted).Any(r => r.IsLateSubmission),
                                SicCodeIds = o.OrganisationSicCodes.Select(osc => osc.SicCode.SicCodeId.ToString()).ToList(),
                                SicCodeSynonyms = sicCodeSynonyms,
                                IncludeInViewingService = GetIncludeInViewingService(o)
                            };
                    })
                .ToList();
        }

        private static bool GetIncludeInViewingService(Organisation organisation)
        {
            return (organisation.Status == OrganisationStatuses.Active || organisation.Status == OrganisationStatuses.Retired) 
                && (organisation.Returns.Any(r => r.Status == ReturnStatuses.Submitted) || organisation.OrganisationScopes.Any(
                             sc => sc.Status == ScopeRowStatuses.Active
                                   && (sc.ScopeStatus == ScopeStatuses.InScope || sc.ScopeStatus == ScopeStatuses.PresumedInScope)));
        }

        private static List<SearchCachedUser> LoadAllUsers(IDataRepository repository)
        {
            return repository
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
        }

    }
}

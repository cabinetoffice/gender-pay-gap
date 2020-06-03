using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
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
            var dataRepository = MvcApplication.ContainerIoC.Resolve<IDataRepository>();

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
                .ToList()
                .Select(
                    o => new SearchCachedOrganisation
                    {
                        OrganisationId = o.OrganisationId,
                        EncryptedId = o.GetEncryptedId(),
                        OrganisationName = new SearchReadyValue(o.OrganisationName),
                        CompanyNumber = o.CompanyNumber?.Trim(),
                        EmployerReference = o.EmployerReference?.Trim(),
                        OrganisationNames = o.OrganisationNames.OrderByDescending(n => n.Created).Select(on => new SearchReadyValue(on.Name)).ToList(),
                        MinEmployees = o.GetLatestReturn()?.MinEmployees ?? 0,
                        Status = o.Status,
                        OrganisationSizes = o.Returns.Select(r => r.OrganisationSize).ToList()
                    })
                .ToList();
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

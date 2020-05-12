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
        internal static List<SearchCachedUser> CachedUsers { get; private set; }
        internal static DateTime CacheLastUpdated { get; private set; } = DateTime.MinValue;


        public static void LoadSearchDataIntoCache()
        {
            var dataRepository = MvcApplication.ContainerIoC.Resolve<IDataRepository>();

            CachedOrganisations = LoadAllOrganisations(dataRepository);
            CachedUsers = LoadAllUsers(dataRepository);

            CacheLastUpdated = VirtualDateTime.Now;
        }

        private static List<SearchCachedOrganisation> LoadAllOrganisations(IDataRepository repository)
        {
            return repository
                .GetAll<Organisation>()
                .Include(o => o.OrganisationNames)
                .Select(
                    o => new SearchCachedOrganisation
                    {
                        OrganisationId = o.OrganisationId,
                        OrganisationName = new SearchReadyValue(o.OrganisationName),
                        CompanyNumber = o.CompanyNumber != null ? o.CompanyNumber.Trim() : null,
                        EmployerReference = o.EmployerReference != null ? o.EmployerReference.Trim() : null,
                        OrganisationNames = o.OrganisationNames.Select(on => new SearchReadyValue(on.Name)).ToList(),
                        Status = o.Status
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

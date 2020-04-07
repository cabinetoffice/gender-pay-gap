using System;
using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.Database;
using Moq;

namespace GenderPayGap.Tests.Common.TestHelpers
{
    public static class OrganisationHelper
    {

        public static Organisation GetConcreteOrganisation(string employerRef,
            string organisationName = "Org123",
            OrganisationStatuses organisationStatus = OrganisationStatuses.Active,
            ICollection<OrganisationStatus> organisationStatuses = null)
        {
            if (string.IsNullOrWhiteSpace(employerRef))
            {
                int someId = new Random().Next(1000, 9999);
                employerRef = employerRef ?? $"Org_{someId}";
            }

            // todo: Use Moq
            return new Organisation {
                EmployerReference = employerRef,
                //OrganisationId = 123,
                OrganisationName = organisationName,
                OrganisationAddresses = new List<OrganisationAddress> { new OrganisationAddress() },
                SectorType = SectorTypes.Private,
                Status = organisationStatus,
                OrganisationStatuses = organisationStatuses ?? new List<OrganisationStatus>()
            };
        }

        public static Organisation GetPublicOrganisation(string employerRef = null)
        {
            Organisation organisation = GetConcreteOrganisation(employerRef);
            organisation.SectorType = SectorTypes.Public;
            return organisation;
        }

        public static Organisation GetPrivateOrganisation(string employerRef = null)
        {
            Organisation organisation = GetConcreteOrganisation(employerRef);
            organisation.SectorType = SectorTypes.Private;
            return organisation;
        }

        public static Organisation GetMockedOrganisation(string employerRef = null)
        {
            int someId = new Random().Next(1000, 9999);
            var mockedOrg = Mock.Of<Organisation>(org => org.OrganisationId == someId);
            mockedOrg.EmployerReference = employerRef ?? $"Org_{someId}";
            mockedOrg.SectorType = SectorTypes.Private;
            mockedOrg.Status = OrganisationStatuses.Active;
            mockedOrg.OrganisationName = $"OrgName_{someId}";
            return mockedOrg;
        }

        public static Organisation GetOrganisationInScope(string employerRef = null, int snapshotYear = 0)
        {
            return GetOrganisationInAGivenScope(ScopeStatuses.InScope, employerRef, snapshotYear);
        }

        public static Organisation GetOrganisationOutOfScope(string employerRef = null, int snapshotYear = 0)
        {
            return GetOrganisationInAGivenScope(ScopeStatuses.OutOfScope, employerRef, snapshotYear);
        }

        public static Organisation GetOrganisationInAGivenScope(ScopeStatuses scope, string employerRef = null, int snapshotYear = 0)
        {
            Organisation org = GetPrivateOrganisation(employerRef);

            OrganisationScope organisationScope = OrganisationScopeHelper.GetOrgScopeWithThisScope(snapshotYear, org.SectorType, scope);
            org.OrganisationScopes.Add(organisationScope);
            return org;
        }

        public static void LinkOrganisationAndReturn(Organisation org, Return ret)
        {
            org.Returns.Add(ret);
            ret.Organisation = org;
        }

        public static void LinkOrganisationAndScope(Organisation org, OrganisationScope scope, bool isLatestScope)
        {
            org.OrganisationScopes.Add(scope);
            scope.Organisation = org;
        }

    }
}

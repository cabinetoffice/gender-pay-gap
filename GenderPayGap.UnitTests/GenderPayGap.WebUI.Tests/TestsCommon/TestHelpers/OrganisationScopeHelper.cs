using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Database;

namespace GenderPayGap.Tests.Common.TestHelpers
{
    public class OrganisationScopeHelper
    {

        public static OrganisationScope GetOrganisationScope(int snapshotYear, OrganisationSectors organisationOrganisationSector)
        {
            return new OrganisationScope {
                SnapshotDate = SectorTypeHelper.GetSnapshotDateForSector(snapshotYear, organisationOrganisationSector),
                Status = ScopeRowStatuses.Active
            };
        }

        public static OrganisationScope GetOrgScopeWithThisScope(int snapshotYear, OrganisationSectors organisationOrganisationSector, ScopeStatuses scope)
        {
            if (snapshotYear == 0)
            {
                snapshotYear = organisationOrganisationSector.GetAccountingStartDate().Year;
            }

            OrganisationScope org = GetOrganisationScope(snapshotYear, organisationOrganisationSector);
            org.ScopeStatus = scope;
            return org;
        }

    }
}

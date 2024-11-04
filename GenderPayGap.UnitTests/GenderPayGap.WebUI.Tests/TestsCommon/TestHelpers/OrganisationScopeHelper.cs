using GenderPayGap.Core;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Database;

namespace GenderPayGap.Tests.Common.TestHelpers
{
    public class OrganisationScopeHelper
    {

        public static OrganisationScope GetOrganisationScope(int snapshotYear, SectorTypes organisationSectorType)
        {
            return new OrganisationScope {
                SnapshotDate = SectorTypeHelper.GetSnapshotDateForSector(snapshotYear, organisationSectorType),
                Status = ScopeRowStatuses.Active
            };
        }

        public static OrganisationScope GetOrgScopeWithThisScope(int snapshotYear, SectorTypes organisationSectorType, ScopeStatuses scope)
        {
            if (snapshotYear == 0)
            {
                snapshotYear = organisationSectorType.GetAccountingStartDate().Year;
            }

            OrganisationScope org = GetOrganisationScope(snapshotYear, organisationSectorType);
            org.ScopeStatus = scope;
            return org;
        }

    }
}

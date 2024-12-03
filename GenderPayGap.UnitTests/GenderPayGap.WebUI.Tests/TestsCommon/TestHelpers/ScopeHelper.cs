using GenderPayGap.Core;
using GenderPayGap.Database;

namespace GenderPayGap.Tests.Common.TestHelpers
{
    public static class ScopeHelper
    {

        public static OrganisationScope CreateScope(ScopeStatuses scopeStatus, DateTime snapshotDate)
        {
            return new OrganisationScope {ScopeStatus = scopeStatus, Status = ScopeRowStatuses.Active, SnapshotDate = snapshotDate};
        }

    }
}

using System;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Extensions;
using GenderPayGap.Tests.Common.TestHelpers;
using NUnit.Framework;

namespace GenderPayGap.Database.ReturnTests
{
    [TestFixture]
    public class IsVoluntarySubmissionTests
    {

        [TestCase(SectorTypes.Private, 249, ScopeStatuses.OutOfScope)]
        [TestCase(SectorTypes.Private, 249, ScopeStatuses.PresumedOutOfScope)]
        [TestCase(SectorTypes.Public, 249, ScopeStatuses.OutOfScope)]
        [TestCase(SectorTypes.Public, 249, ScopeStatuses.PresumedOutOfScope)]
        public void IsTrueWhenVoluntary(SectorTypes testSector, int testOrgSize, ScopeStatuses testScopeStatus)
        {
            // Arrange 
            Organisation testOrganisation = testSector == SectorTypes.Private
                ? OrganisationHelper.GetPrivateOrganisation()
                : OrganisationHelper.GetPublicOrganisation();

            DateTime snapshotDate = testSector.GetAccountingStartDate(VirtualDateTime.Now.Year);
            OrganisationScope testScope = ScopeHelper.CreateScope(testScopeStatus, snapshotDate);
            Return testReturn = ReturnHelper.CreateTestReturn(testOrganisation, snapshotDate.Year);
            testReturn.MinEmployees = 0;
            testReturn.MaxEmployees = testOrgSize;

            OrganisationHelper.LinkOrganisationAndReturn(testOrganisation, testReturn);
            OrganisationHelper.LinkOrganisationAndScope(testOrganisation, testScope, true);

            // Act
            bool actual = testReturn.IsVoluntarySubmission();

            // Assert
            Assert.IsTrue(actual);
        }

        [TestCase(SectorTypes.Private, 0, ScopeStatuses.InScope)]
        [TestCase(SectorTypes.Private, 0, ScopeStatuses.PresumedInScope)]
        [TestCase(SectorTypes.Private, 0, ScopeStatuses.Unknown)]
        [TestCase(SectorTypes.Private, 249, ScopeStatuses.InScope)]
        [TestCase(SectorTypes.Private, 249, ScopeStatuses.PresumedInScope)]
        [TestCase(SectorTypes.Private, 249, ScopeStatuses.Unknown)]
        [TestCase(SectorTypes.Private, 499, ScopeStatuses.InScope)]
        [TestCase(SectorTypes.Private, 499, ScopeStatuses.PresumedInScope)]
        [TestCase(SectorTypes.Private, 499, ScopeStatuses.Unknown)]
        [TestCase(SectorTypes.Private, 999, ScopeStatuses.InScope)]
        [TestCase(SectorTypes.Private, 999, ScopeStatuses.PresumedInScope)]
        [TestCase(SectorTypes.Private, 999, ScopeStatuses.Unknown)]
        [TestCase(SectorTypes.Private, 4999, ScopeStatuses.InScope)]
        [TestCase(SectorTypes.Private, 4999, ScopeStatuses.PresumedInScope)]
        [TestCase(SectorTypes.Private, 4999, ScopeStatuses.Unknown)]
        [TestCase(SectorTypes.Private, 19999, ScopeStatuses.InScope)]
        [TestCase(SectorTypes.Private, 19999, ScopeStatuses.PresumedInScope)]
        [TestCase(SectorTypes.Private, 19999, ScopeStatuses.Unknown)]
        [TestCase(SectorTypes.Private, int.MaxValue, ScopeStatuses.InScope)]
        [TestCase(SectorTypes.Private, int.MaxValue, ScopeStatuses.PresumedInScope)]
        [TestCase(SectorTypes.Private, int.MaxValue, ScopeStatuses.Unknown)]
        [TestCase(SectorTypes.Public, 0, ScopeStatuses.InScope)]
        [TestCase(SectorTypes.Public, 0, ScopeStatuses.PresumedInScope)]
        [TestCase(SectorTypes.Public, 0, ScopeStatuses.Unknown)]
        [TestCase(SectorTypes.Public, 249, ScopeStatuses.InScope)]
        [TestCase(SectorTypes.Public, 249, ScopeStatuses.PresumedInScope)]
        [TestCase(SectorTypes.Public, 249, ScopeStatuses.Unknown)]
        [TestCase(SectorTypes.Public, 499, ScopeStatuses.InScope)]
        [TestCase(SectorTypes.Public, 499, ScopeStatuses.PresumedInScope)]
        [TestCase(SectorTypes.Public, 499, ScopeStatuses.Unknown)]
        [TestCase(SectorTypes.Public, 999, ScopeStatuses.InScope)]
        [TestCase(SectorTypes.Public, 999, ScopeStatuses.PresumedInScope)]
        [TestCase(SectorTypes.Public, 999, ScopeStatuses.Unknown)]
        [TestCase(SectorTypes.Public, 4999, ScopeStatuses.InScope)]
        [TestCase(SectorTypes.Public, 4999, ScopeStatuses.PresumedInScope)]
        [TestCase(SectorTypes.Public, 4999, ScopeStatuses.Unknown)]
        [TestCase(SectorTypes.Public, 19999, ScopeStatuses.InScope)]
        [TestCase(SectorTypes.Public, 19999, ScopeStatuses.PresumedInScope)]
        [TestCase(SectorTypes.Public, 19999, ScopeStatuses.Unknown)]
        [TestCase(SectorTypes.Public, int.MaxValue, ScopeStatuses.InScope)]
        [TestCase(SectorTypes.Public, int.MaxValue, ScopeStatuses.PresumedInScope)]
        [TestCase(SectorTypes.Public, int.MaxValue, ScopeStatuses.Unknown)]
        public void IsFalseWhenNotVoluntary(SectorTypes testSector, int testOrgSize, ScopeStatuses testScopeStatus)
        {
            // Arrange 
            Organisation testOrganisation = testSector == SectorTypes.Private
                ? OrganisationHelper.GetPrivateOrganisation()
                : OrganisationHelper.GetPublicOrganisation();

            DateTime snapshotDate = testSector.GetAccountingStartDate(VirtualDateTime.Now.Year);
            OrganisationScope testScope = ScopeHelper.CreateScope(testScopeStatus, snapshotDate);
            Return testReturn = ReturnHelper.CreateTestReturn(testOrganisation, snapshotDate.Year);
            testReturn.MaxEmployees = testOrgSize;

            OrganisationHelper.LinkOrganisationAndReturn(testOrganisation, testReturn);
            OrganisationHelper.LinkOrganisationAndScope(testOrganisation, testScope, true);

            // Act
            bool actual = testReturn.IsVoluntarySubmission();

            // Assert
            Assert.IsFalse(actual);
        }

    }
}

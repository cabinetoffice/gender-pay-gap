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

        [TestCase(OrganisationSectors.Private, 249, ScopeStatuses.OutOfScope)]
        [TestCase(OrganisationSectors.Private, 249, ScopeStatuses.PresumedOutOfScope)]
        [TestCase(OrganisationSectors.Public, 249, ScopeStatuses.OutOfScope)]
        [TestCase(OrganisationSectors.Public, 249, ScopeStatuses.PresumedOutOfScope)]
        public void IsTrueWhenVoluntary(OrganisationSectors testSector, int testOrgSize, ScopeStatuses testScopeStatus)
        {
            // Arrange 
            Organisation testOrganisation = testSector == OrganisationSectors.Private
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

        [TestCase(OrganisationSectors.Private, 0, ScopeStatuses.InScope)]
        [TestCase(OrganisationSectors.Private, 0, ScopeStatuses.PresumedInScope)]
        [TestCase(OrganisationSectors.Private, 0, ScopeStatuses.Unknown)]
        [TestCase(OrganisationSectors.Private, 249, ScopeStatuses.InScope)]
        [TestCase(OrganisationSectors.Private, 249, ScopeStatuses.PresumedInScope)]
        [TestCase(OrganisationSectors.Private, 249, ScopeStatuses.Unknown)]
        [TestCase(OrganisationSectors.Private, 499, ScopeStatuses.InScope)]
        [TestCase(OrganisationSectors.Private, 499, ScopeStatuses.PresumedInScope)]
        [TestCase(OrganisationSectors.Private, 499, ScopeStatuses.Unknown)]
        [TestCase(OrganisationSectors.Private, 999, ScopeStatuses.InScope)]
        [TestCase(OrganisationSectors.Private, 999, ScopeStatuses.PresumedInScope)]
        [TestCase(OrganisationSectors.Private, 999, ScopeStatuses.Unknown)]
        [TestCase(OrganisationSectors.Private, 4999, ScopeStatuses.InScope)]
        [TestCase(OrganisationSectors.Private, 4999, ScopeStatuses.PresumedInScope)]
        [TestCase(OrganisationSectors.Private, 4999, ScopeStatuses.Unknown)]
        [TestCase(OrganisationSectors.Private, 19999, ScopeStatuses.InScope)]
        [TestCase(OrganisationSectors.Private, 19999, ScopeStatuses.PresumedInScope)]
        [TestCase(OrganisationSectors.Private, 19999, ScopeStatuses.Unknown)]
        [TestCase(OrganisationSectors.Private, int.MaxValue, ScopeStatuses.InScope)]
        [TestCase(OrganisationSectors.Private, int.MaxValue, ScopeStatuses.PresumedInScope)]
        [TestCase(OrganisationSectors.Private, int.MaxValue, ScopeStatuses.Unknown)]
        [TestCase(OrganisationSectors.Public, 0, ScopeStatuses.InScope)]
        [TestCase(OrganisationSectors.Public, 0, ScopeStatuses.PresumedInScope)]
        [TestCase(OrganisationSectors.Public, 0, ScopeStatuses.Unknown)]
        [TestCase(OrganisationSectors.Public, 249, ScopeStatuses.InScope)]
        [TestCase(OrganisationSectors.Public, 249, ScopeStatuses.PresumedInScope)]
        [TestCase(OrganisationSectors.Public, 249, ScopeStatuses.Unknown)]
        [TestCase(OrganisationSectors.Public, 499, ScopeStatuses.InScope)]
        [TestCase(OrganisationSectors.Public, 499, ScopeStatuses.PresumedInScope)]
        [TestCase(OrganisationSectors.Public, 499, ScopeStatuses.Unknown)]
        [TestCase(OrganisationSectors.Public, 999, ScopeStatuses.InScope)]
        [TestCase(OrganisationSectors.Public, 999, ScopeStatuses.PresumedInScope)]
        [TestCase(OrganisationSectors.Public, 999, ScopeStatuses.Unknown)]
        [TestCase(OrganisationSectors.Public, 4999, ScopeStatuses.InScope)]
        [TestCase(OrganisationSectors.Public, 4999, ScopeStatuses.PresumedInScope)]
        [TestCase(OrganisationSectors.Public, 4999, ScopeStatuses.Unknown)]
        [TestCase(OrganisationSectors.Public, 19999, ScopeStatuses.InScope)]
        [TestCase(OrganisationSectors.Public, 19999, ScopeStatuses.PresumedInScope)]
        [TestCase(OrganisationSectors.Public, 19999, ScopeStatuses.Unknown)]
        [TestCase(OrganisationSectors.Public, int.MaxValue, ScopeStatuses.InScope)]
        [TestCase(OrganisationSectors.Public, int.MaxValue, ScopeStatuses.PresumedInScope)]
        [TestCase(OrganisationSectors.Public, int.MaxValue, ScopeStatuses.Unknown)]
        public void IsFalseWhenNotVoluntary(OrganisationSectors testSector, int testOrgSize, ScopeStatuses testScopeStatus)
        {
            // Arrange 
            Organisation testOrganisation = testSector == OrganisationSectors.Private
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

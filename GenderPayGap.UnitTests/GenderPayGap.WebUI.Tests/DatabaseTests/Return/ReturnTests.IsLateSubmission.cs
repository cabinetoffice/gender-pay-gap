using System;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Extensions;
using GenderPayGap.Tests.Common.TestHelpers;
using NUnit.Framework;

namespace GenderPayGap.Database.ReturnTests
{
    [TestFixture]
    public class IsLateSubmissionTests
    {

        [TestCase(SectorTypes.Public, ScopeStatuses.InScope)]
        [TestCase(SectorTypes.Public, ScopeStatuses.PresumedInScope)]
        [TestCase(SectorTypes.Private, ScopeStatuses.InScope)]
        [TestCase(SectorTypes.Private, ScopeStatuses.PresumedInScope)]
        public void Is_True_When_ModifiedDate_Is_Late_And_InScope(SectorTypes sector, ScopeStatuses scopeStatus)
        {
            var totalYearOffsets = 4;
            for (var yearOffset = 0; yearOffset < totalYearOffsets; yearOffset++)
            {
                // Arrange 
                int testYear = VirtualDateTime.Now.Year - yearOffset;
                DateTime snapshotDate = sector.GetAccountingStartDate(testYear);
                DateTime nextSnapshotDate = snapshotDate.AddYears(1);
                DateTime modifiedDate = nextSnapshotDate.AddDays(2);

                Organisation testOrganisation = sector == SectorTypes.Private
                    ? OrganisationHelper.GetPrivateOrganisation()
                    : OrganisationHelper.GetPublicOrganisation();

                OrganisationScope testScope = ScopeHelper.CreateScope(scopeStatus, snapshotDate);

                Return testReturn = ReturnHelper.CreateLateReturn(testOrganisation, snapshotDate, modifiedDate, testScope);

                // Act
                bool actual = testReturn.IsLateSubmission;

                // Assert
                Assert.AreEqual(true, actual);
            }
        }

        [TestCase(SectorTypes.Public, ScopeStatuses.InScope)]
        [TestCase(SectorTypes.Public, ScopeStatuses.PresumedInScope)]
        [TestCase(SectorTypes.Private, ScopeStatuses.InScope)]
        [TestCase(SectorTypes.Private, ScopeStatuses.PresumedInScope)]
        [TestCase(SectorTypes.Public, ScopeStatuses.OutOfScope)]
        [TestCase(SectorTypes.Public, ScopeStatuses.PresumedOutOfScope)]
        [TestCase(SectorTypes.Private, ScopeStatuses.OutOfScope)]
        [TestCase(SectorTypes.Private, ScopeStatuses.PresumedOutOfScope)]
        public void Is_False_When_ModifiedDate_Is_Not_Late_And_AnyScope(SectorTypes sector, ScopeStatuses scopeStatus)
        {
            var totalYearOffsets = 4;
            for (var yearOffset = 0; yearOffset < totalYearOffsets; yearOffset++)
            {
                // Arrange 
                int testYear = VirtualDateTime.Now.Year - yearOffset;
                DateTime snapshotDate = sector.GetAccountingStartDate(testYear);
                DateTime nextSnapshotDate = snapshotDate.AddYears(1);
                DateTime modifiedDate = nextSnapshotDate.AddDays(-1);

                Organisation testOrganisation = sector == SectorTypes.Private
                    ? OrganisationHelper.GetPrivateOrganisation()
                    : OrganisationHelper.GetPublicOrganisation();

                OrganisationScope testScope = ScopeHelper.CreateScope(scopeStatus, snapshotDate);

                Return testReturn = ReturnHelper.CreateLateReturn(testOrganisation, snapshotDate, modifiedDate, testScope);

                // Act
                bool actual = testReturn.IsLateSubmission;

                // Assert
                Assert.AreEqual(false, actual);
            }
        }

        [Test]
        [TestCase(SectorTypes.Public, ScopeStatuses.OutOfScope)]
        [TestCase(SectorTypes.Public, ScopeStatuses.PresumedOutOfScope)]
        [TestCase(SectorTypes.Private, ScopeStatuses.OutOfScope)]
        [TestCase(SectorTypes.Private, ScopeStatuses.PresumedOutOfScope)]
        public void Is_False_When_ModifiedDate_Is_Late_And_OutOfScope(SectorTypes sector, ScopeStatuses scopeStatus)
        {
            var totalYearOffsets = 4;

            for (var yearOffset = 0; yearOffset < totalYearOffsets; yearOffset++)
            {
                // Arrange 
                int testYear = VirtualDateTime.Now.Year - yearOffset;
                DateTime snapshotDate = sector.GetAccountingStartDate(testYear);
                DateTime nextSnapshotDate = snapshotDate.AddYears(1);
                DateTime modifiedDate = nextSnapshotDate.AddDays(2);

                Organisation testOrganisation = sector == SectorTypes.Private
                    ? OrganisationHelper.GetPrivateOrganisation()
                    : OrganisationHelper.GetPublicOrganisation();

                OrganisationScope testScope = ScopeHelper.CreateScope(scopeStatus, snapshotDate);

                Return testReturn = ReturnHelper.CreateLateReturn(testOrganisation, snapshotDate, modifiedDate, testScope);

                // Act
                bool actual = testReturn.IsLateSubmission;

                // Assert
                Assert.AreEqual(false, actual);
            }
        }

    }
}

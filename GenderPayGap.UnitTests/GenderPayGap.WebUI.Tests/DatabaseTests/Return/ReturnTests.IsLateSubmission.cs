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

        [TestCase(OrganisationSectors.Public, ScopeStatuses.InScope)]
        [TestCase(OrganisationSectors.Public, ScopeStatuses.PresumedInScope)]
        [TestCase(OrganisationSectors.Private, ScopeStatuses.InScope)]
        [TestCase(OrganisationSectors.Private, ScopeStatuses.PresumedInScope)]
        public void Is_True_When_ModifiedDate_Is_Late_And_InScope(OrganisationSectors sector, ScopeStatuses scopeStatus)
        {
            var totalYearOffsets = 4;
            for (var yearOffset = 0; yearOffset < totalYearOffsets; yearOffset++)
            {
                // Arrange 
                int testYear = VirtualDateTime.Now.Year - yearOffset;
                DateTime snapshotDate = sector.GetAccountingStartDate(testYear);
                DateTime nextSnapshotDate = snapshotDate.AddYears(1);
                DateTime modifiedDate = nextSnapshotDate.AddDays(2);

                Organisation testOrganisation = sector == OrganisationSectors.Private
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

        [TestCase(OrganisationSectors.Public, ScopeStatuses.InScope)]
        [TestCase(OrganisationSectors.Public, ScopeStatuses.PresumedInScope)]
        [TestCase(OrganisationSectors.Private, ScopeStatuses.InScope)]
        [TestCase(OrganisationSectors.Private, ScopeStatuses.PresumedInScope)]
        [TestCase(OrganisationSectors.Public, ScopeStatuses.OutOfScope)]
        [TestCase(OrganisationSectors.Public, ScopeStatuses.PresumedOutOfScope)]
        [TestCase(OrganisationSectors.Private, ScopeStatuses.OutOfScope)]
        [TestCase(OrganisationSectors.Private, ScopeStatuses.PresumedOutOfScope)]
        public void Is_False_When_ModifiedDate_Is_Not_Late_And_AnyScope(OrganisationSectors sector, ScopeStatuses scopeStatus)
        {
            var totalYearOffsets = 4;
            for (var yearOffset = 0; yearOffset < totalYearOffsets; yearOffset++)
            {
                // Arrange 
                int testYear = VirtualDateTime.Now.Year - yearOffset;
                DateTime snapshotDate = sector.GetAccountingStartDate(testYear);
                DateTime nextSnapshotDate = snapshotDate.AddYears(1);
                DateTime modifiedDate = nextSnapshotDate.AddDays(-1);

                Organisation testOrganisation = sector == OrganisationSectors.Private
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
        [TestCase(OrganisationSectors.Public, ScopeStatuses.OutOfScope)]
        [TestCase(OrganisationSectors.Public, ScopeStatuses.PresumedOutOfScope)]
        [TestCase(OrganisationSectors.Private, ScopeStatuses.OutOfScope)]
        [TestCase(OrganisationSectors.Private, ScopeStatuses.PresumedOutOfScope)]
        public void Is_False_When_ModifiedDate_Is_Late_And_OutOfScope(OrganisationSectors sector, ScopeStatuses scopeStatus)
        {
            var totalYearOffsets = 4;

            for (var yearOffset = 0; yearOffset < totalYearOffsets; yearOffset++)
            {
                // Arrange 
                int testYear = VirtualDateTime.Now.Year - yearOffset;
                DateTime snapshotDate = sector.GetAccountingStartDate(testYear);
                DateTime nextSnapshotDate = snapshotDate.AddYears(1);
                DateTime modifiedDate = nextSnapshotDate.AddDays(2);

                Organisation testOrganisation = sector == OrganisationSectors.Private
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

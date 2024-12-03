using GenderPayGap.Core;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Tests.Common.TestHelpers;

namespace GenderPayGap.Database.ReturnTests
{
    [TestFixture]
    public class IsLateSubmissionTests
    {

        private static List<int> reportingStartYearsToExcludeFromLateFlagEnforcement = Global.ReportingStartYearsToExcludeFromLateFlagEnforcement;

        [TestCase(SectorTypes.Public, ScopeStatuses.InScope)]
        [TestCase(SectorTypes.Public, ScopeStatuses.PresumedInScope)]
        [TestCase(SectorTypes.Private, ScopeStatuses.InScope)]
        [TestCase(SectorTypes.Private, ScopeStatuses.PresumedInScope)]
        public void Is_True_When_ModifiedDate_Is_Late_And_InScope(SectorTypes sector, ScopeStatuses scopeStatus)
        {
            // Arrange 
            int testYear = GetRandomReportingYear(ignoreYearsExcludedFromLateFlagEnforcement: true);
            DateTime snapshotDate = sector.GetAccountingStartDate(testYear);
            DateTime modifiedDate = ReportingYearsHelper.GetDeadlineForAccountingDate(snapshotDate).AddDays(2);

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
            // Arrange 
            int testYear = GetRandomReportingYear();
            int modifiedDateOffset = -1;

            Return testReturn = ReturnHelper.CreateLateReturn(testYear, sector, scopeStatus, modifiedDateOffset);

            // Act
            bool actual = testReturn.IsLateSubmission;

            // Assert
            Assert.AreEqual(false, actual);
        }

        [Test]
        [TestCase(SectorTypes.Public, ScopeStatuses.OutOfScope)]
        [TestCase(SectorTypes.Public, ScopeStatuses.PresumedOutOfScope)]
        [TestCase(SectorTypes.Private, ScopeStatuses.OutOfScope)]
        [TestCase(SectorTypes.Private, ScopeStatuses.PresumedOutOfScope)]
        public void Is_False_When_ModifiedDate_Is_Late_And_OutOfScope(SectorTypes sector, ScopeStatuses scopeStatus)
        {
            // Arrange 
            int testYear = GetRandomReportingYear();
            int modifiedDateOffset = 2;

            Return testReturn = ReturnHelper.CreateLateReturn(testYear, sector, scopeStatus, modifiedDateOffset);

            // Act
            bool actual = testReturn.IsLateSubmission;

            // Assert
            Assert.AreEqual(false, actual);
        }

        [TestCase(SectorTypes.Public, ScopeStatuses.InScope)]
        [TestCase(SectorTypes.Public, ScopeStatuses.PresumedInScope)]
        [TestCase(SectorTypes.Private, ScopeStatuses.InScope)]
        [TestCase(SectorTypes.Private, ScopeStatuses.PresumedInScope)]
        [TestCase(SectorTypes.Public, ScopeStatuses.OutOfScope)]
        [TestCase(SectorTypes.Public, ScopeStatuses.PresumedOutOfScope)]
        [TestCase(SectorTypes.Private, ScopeStatuses.OutOfScope)]
        [TestCase(SectorTypes.Private, ScopeStatuses.PresumedOutOfScope)]
        public void Is_False_When_ModifiedDate_Is_Late_And_ExcludeFromLateFlagEnforcement_Year_And_AnyScope(SectorTypes sector,
            ScopeStatuses scopeStatus)
        {
            // Arrange 
            int testYear = Global.ReportingStartYearsToExcludeFromLateFlagEnforcement.First();
            int modifiedDateOffset = 2;

            Return testReturn = ReturnHelper.CreateLateReturn(testYear, sector, scopeStatus, modifiedDateOffset);


            // Act
            bool actual = testReturn.IsLateSubmission;

            // Assert
            Assert.AreEqual(false, actual);
        }

        [TestCaseSource(nameof(reportingStartYearsToExcludeFromLateFlagEnforcement))]
        public void Is_False_When_ModifiedDate_Is_Late_Given_Any_ExcludeFromLateFlagEnforcement_Year(int testYear)
        {
            // Arrange 
            int modifiedDateOffset = 2;

            Return testReturn = ReturnHelper.CreateLateReturn(testYear, SectorTypes.Private, ScopeStatuses.InScope, modifiedDateOffset);

            // Act
            bool actual = testReturn.IsLateSubmission;

            // Assert
            Assert.AreEqual(false, actual);
        }

        private int GetRandomReportingYear(bool ignoreYearsExcludedFromLateFlagEnforcement = false)
        {
            var reportingYears = ReportingYearsHelper.GetReportingYears();
            if (ignoreYearsExcludedFromLateFlagEnforcement)
            {
                reportingYears = ReportingYearsHelper.GetReportingYears()
                    .Except(reportingStartYearsToExcludeFromLateFlagEnforcement)
                    .ToList();
            }

            var random = new Random();
            int index = random.Next(reportingYears.Count);

            var reportingYear = reportingYears[index];
            Console.WriteLine("Testing Reporting Year: " + reportingYear);

            return reportingYear;
        }

    }
}

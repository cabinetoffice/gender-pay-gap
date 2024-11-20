using System;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Tests.Common.Classes;
using GenderPayGap.WebUI.BusinessLogic.Services;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.BusinessLogic.Tests.ScopeBusinessLogic
{

    [TestFixture]
    [SetCulture("en-GB")]
    public class FillMissingScopesTests
    {

        [SetUp]
        public void BeforeEach()
        {
            // setup mocks
            mockDataRepository = MoqHelpers.CreateMockDataRepository();

            // sut
            scopeBusinessLogic = new WebUI.BusinessLogic.Services.ScopeBusinessLogic(mockDataRepository.Object);
        }

        private Mock<IDataRepository> mockDataRepository;

        // sut
        private WebUI.BusinessLogic.Services.ScopeBusinessLogic scopeBusinessLogic;

        [TestCase(SectorTypes.Private)]
        [TestCase(SectorTypes.Public)]
        public void PresumesOutOfScopeForSnapshotYearsBeforeOrgCreatedDate(SectorTypes testSectorType)
        {
            // setup
            Organisation testOrg = CreateOrgWithNoScopes(1, testSectorType, VirtualDateTime.Now);

            // act
            bool actualChanged = scopeBusinessLogic.FillMissingScopes(testOrg);

            // assert
            Assert.IsTrue(actualChanged, "Expected change for missing scopes");

            // test the count of scopes set is correct
            DateTime currentSnapshotDate = testOrg.SectorType.GetAccountingStartDate();
            int expectedScopeCount = (currentSnapshotDate.Year - Global.FirstReportingYear) + 1;
            Assert.AreEqual(expectedScopeCount, testOrg.OrganisationScopes.Count);

            // check each scope before current snapshot year are set to presumed out of scope
            OrganisationScope[] actualScopesArray = testOrg.OrganisationScopes.ToArray();
            for (var i = 0; i < actualScopesArray.Length - 1; i++)
            {
                OrganisationScope scope = actualScopesArray[i];
                Assert.AreEqual(ScopeStatuses.PresumedOutOfScope, scope.ScopeStatus);
            }

            // assert current year is presumed in scope
            Assert.AreEqual(ScopeStatuses.PresumedInScope, actualScopesArray[actualScopesArray.Length - 1].ScopeStatus);
        }

        [TestCase(SectorTypes.Private)]
        [TestCase(SectorTypes.Public)]
        public void PresumesInScopeForSnapshotYearsDuringAndAfterOrgCreatedDate(SectorTypes testSectorType)
        {
            // setup
            DateTime testCreatedDate = testSectorType.GetAccountingStartDate().AddYears(-1);
            Organisation testOrg = CreateOrgWithNoScopes(1, testSectorType, testCreatedDate);

            // act
            bool actualChanged = scopeBusinessLogic.FillMissingScopes(testOrg);

            // assert
            Assert.IsTrue(actualChanged, "Expected change for missing scopes");

            // test the count of scopes set is correct
            DateTime currentSnapshotDate = testOrg.SectorType.GetAccountingStartDate();
            int expectedScopeCount = (currentSnapshotDate.Year - Global.FirstReportingYear) + 1;
            Assert.AreEqual(expectedScopeCount, testOrg.OrganisationScopes.Count);

            // check each scope after created date is set to presumed in of scope
            OrganisationScope[] actualScopesArray = testOrg.OrganisationScopes.ToArray();
            for (int i = actualScopesArray.Length - 2; i < actualScopesArray.Length; i++)
            {
                OrganisationScope scope = actualScopesArray[i];
                Assert.AreEqual(ScopeStatuses.PresumedInScope, scope.ScopeStatus);
            }
        }

        [TestCase(SectorTypes.Private, ScopeStatuses.InScope, ScopeStatuses.PresumedInScope)]
        [TestCase(SectorTypes.Public, ScopeStatuses.InScope, ScopeStatuses.PresumedInScope)]
        [TestCase(SectorTypes.Private, ScopeStatuses.OutOfScope, ScopeStatuses.PresumedOutOfScope)]
        [TestCase(SectorTypes.Public, ScopeStatuses.OutOfScope, ScopeStatuses.PresumedOutOfScope)]
        public void PreservesPresumedScopesFromPreviousYear(SectorTypes testSectorType,
            ScopeStatuses testDeclaredScopeStatus,
            ScopeStatuses expectedPresumedScopeStatus)
        {
            // setup
            DateTime testSnapshotDate = testSectorType.GetAccountingStartDate(Global.FirstReportingYear);
            Organisation testOrg = CreateOrgWithScopeForAllYears(1, testSectorType, testDeclaredScopeStatus, testSnapshotDate);

            // act
            bool actualChanged = scopeBusinessLogic.FillMissingScopes(testOrg);

            // assert
            Assert.IsTrue(actualChanged, "Expected change to be true for missing scopes");
        }

        [TestCase(SectorTypes.Private, ScopeStatuses.InScope, ScopeStatuses.PresumedInScope)]
        [TestCase(SectorTypes.Public, ScopeStatuses.InScope, ScopeStatuses.PresumedInScope)]
        [TestCase(SectorTypes.Private, ScopeStatuses.OutOfScope, ScopeStatuses.PresumedOutOfScope)]
        [TestCase(SectorTypes.Public, ScopeStatuses.OutOfScope, ScopeStatuses.PresumedOutOfScope)]
        public void PreservesDeclaredScopes(SectorTypes testSectorType,
            ScopeStatuses testDeclaredScopeStatus,
            ScopeStatuses expectedPresumedScopeStatus)
        {
            // setup
            DateTime testCreatedDate = testSectorType.GetAccountingStartDate(Global.FirstReportingYear);
            Organisation testOrg = CreateOrgWithDeclaredAndPresumedScopes(
                testSectorType,
                testDeclaredScopeStatus,
                testCreatedDate,
                testCreatedDate);

            // act
            bool actualChanged = scopeBusinessLogic.FillMissingScopes(testOrg);

            // assert
            Assert.IsTrue(actualChanged, "Expected change to be true for missing scopes");

            OrganisationScope[] actualScopesArray = testOrg.OrganisationScopes.ToArray();
            Assert.AreEqual(testDeclaredScopeStatus, actualScopesArray[0].ScopeStatus, "Expected first year scope status to match");

            // check that each year is presumed out of scope after first year
            for (var i = 1; i < actualScopesArray.Length; i++)
            {
                OrganisationScope scope = actualScopesArray[i];
                Assert.AreEqual(expectedPresumedScopeStatus, scope.ScopeStatus, "Expected presumed scope statuses to match");
            }
        }

        private Organisation CreateOrgWithNoScopes(int testOrgId, SectorTypes testSector, DateTime testCreated)
        {
            return new Organisation {
                OrganisationId = testOrgId, SectorType = testSector, Status = OrganisationStatuses.Active, Created = testCreated
            };
        }

        private Organisation CreateOrgWithDeclaredAndPresumedScopes(
            SectorTypes testSector,
            ScopeStatuses testDeclaredScopeStatus,
            DateTime testCreated,
            DateTime testSnapshotDate)
        {
            Organisation testOrg = CreateOrgWithNoScopes(1, testSector, testCreated);

            testOrg.OrganisationScopes.Add(
                new OrganisationScope {
                    OrganisationScopeId = 1,
                    Status = ScopeRowStatuses.Active,
                    SnapshotDate = testSnapshotDate,
                    ScopeStatus = testDeclaredScopeStatus
                });

            testOrg.OrganisationScopes.Add(
                new OrganisationScope {
                    OrganisationScopeId = 2,
                    Status = ScopeRowStatuses.Active,
                    SnapshotDate = testSnapshotDate.AddYears(1),
                    ScopeStatus = testDeclaredScopeStatus == ScopeStatuses.InScope
                        ? ScopeStatuses.PresumedInScope
                        : ScopeStatuses.PresumedOutOfScope
                });

            return testOrg;
        }

        private Organisation CreateOrgWithScopeForAllYears(int testOrgId,
            SectorTypes testSector,
            ScopeStatuses testScopeStatus,
            DateTime snapshotDate)
        {
            int firstYear = Global.FirstReportingYear;
            int lastYear = SectorTypes.Private.GetAccountingStartDate().Year;

            Organisation testOrg = CreateOrgWithNoScopes(testOrgId, testSector, VirtualDateTime.Now);

            // for all snapshot years check if scope exists
            for (int year = firstYear; year < lastYear; year++)
            {
                testOrg.OrganisationScopes.Add(
                    new OrganisationScope {
                        OrganisationScopeId = 1,
                        Status = ScopeRowStatuses.Active,
                        SnapshotDate = new DateTime(year, snapshotDate.Month, snapshotDate.Day),
                        ScopeStatus = testScopeStatus
                    });
            }

            return testOrg;
        }

    }

}

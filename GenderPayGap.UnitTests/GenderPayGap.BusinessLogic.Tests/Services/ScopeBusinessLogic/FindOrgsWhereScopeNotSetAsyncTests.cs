using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic.Models.Scope;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.Tests.Common.Classes;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.BusinessLogic.Tests.ScopeBusinessLogic
{

    [TestFixture]
    [SetCulture("en-GB")]
    public class FindOrgsWhereScopeNotSetAsyncTests : BaseBusinessLogicTests
    {

        [SetUp]
        public void BeforeEach()
        {
            // setup mocks
            mockDataRepository = MoqHelpers.CreateMockAsyncDataRepository();
            mockCommonBusinessLogic = new CommonBusinessLogic(Config.Configuration);

            // setup data
            DateTime currentPrivateSnapshotDate = mockCommonBusinessLogic.GetAccountingStartDate(SectorTypes.Private);
            DateTime currentPublicSnapshotDate = mockCommonBusinessLogic.GetAccountingStartDate(SectorTypes.Public);

            testOrgs = new List<Organisation>();
            testOrgs.Add(CreateOrgWithExistingScopeForAllYears(1, SectorTypes.Private, currentPrivateSnapshotDate));
            testOrgs.Add(CreateOrgWithExistingScopeForAllYears(2, SectorTypes.Public, currentPublicSnapshotDate));

            testOrgs.Add(CreateOrgWithMissingScopesForAllYears(3, SectorTypes.Private));
            testOrgs.Add(CreateOrgWithMissingScopesForAllYears(4, SectorTypes.Public));

            testOrgs.Add(CreateOrgWithUnknownScopesForAllYears(5, SectorTypes.Private, currentPrivateSnapshotDate));
            testOrgs.Add(CreateOrgWithUnknownScopesForAllYears(6, SectorTypes.Public, currentPublicSnapshotDate));

            mockDataRepository.SetupGetAll(testOrgs);

            // sut
            scopeBusinessLogic = new BusinessLogic.ScopeBusinessLogic(
                mockCommonBusinessLogic,
                mockDataRepository.Object,
                null);
        }

        private Mock<IDataRepository> mockDataRepository;
        private ICommonBusinessLogic mockCommonBusinessLogic;
        private List<Organisation> testOrgs;

        // sut
        private IScopeBusinessLogic scopeBusinessLogic;

        [TestCase(1)]
        [TestCase(2)]
        public async Task IgnoresOrgsWhereAllScopesAreSet(int expectedIgnoredId)
        {
            // act
            HashSet<OrganisationMissingScope> actualMissingOrgScopes = await scopeBusinessLogic.FindOrgsWhereScopeNotSetAsync();

            // assert
            OrganisationMissingScope actualMissingEntry = actualMissingOrgScopes
                .Where(missing => missing.Organisation.OrganisationId == expectedIgnoredId)
                .FirstOrDefault();
            Assert.IsNull(actualMissingEntry, "Expected to return organisations who have missing scopes");
        }

        [TestCase(3, SectorTypes.Private)]
        [TestCase(3, SectorTypes.Public)]
        [TestCase(4, SectorTypes.Private)]
        [TestCase(4, SectorTypes.Public)]
        public async Task FindsOrgsWhereScopeIsMissing(int expectedMissingOrgId, SectorTypes testSector)
        {
            // act
            HashSet<OrganisationMissingScope> actualMissingOrgScopes = await scopeBusinessLogic.FindOrgsWhereScopeNotSetAsync();

            // assert
            OrganisationMissingScope actualMissingEntry = actualMissingOrgScopes.Where(
                    missing => missing.Organisation.OrganisationId == expectedMissingOrgId)
                .FirstOrDefault();

            Assert.IsNotNull(actualMissingEntry, "Expected to find organisations who have null scopes");

            DateTime currentSnapshotDate = mockCommonBusinessLogic.GetAccountingStartDate(testSector);
            List<int> testYears = GetAllSnapshotYearsForSector(currentSnapshotDate);
            foreach (int testYear in testYears)
            {
                Assert.IsTrue(actualMissingEntry.MissingSnapshotYears.Contains(testYear), "Expected missing year");
            }
        }

        [TestCase(5, SectorTypes.Private)]
        [TestCase(5, SectorTypes.Public)]
        [TestCase(6, SectorTypes.Private)]
        [TestCase(6, SectorTypes.Public)]
        public async Task FindsOrgsWhereScopeIsUnknown(int expectedUnknownOrgId, SectorTypes testSector)
        {
            // act
            HashSet<OrganisationMissingScope> actualMissingOrgScopes = await scopeBusinessLogic.FindOrgsWhereScopeNotSetAsync();

            // assert
            OrganisationMissingScope actualMissingEntry = actualMissingOrgScopes
                .Where(missing => missing.Organisation.OrganisationId == expectedUnknownOrgId)
                .FirstOrDefault();

            Assert.IsNotNull(actualMissingEntry, "Expected to find organisations who have unknown scopes");

            DateTime currentSnapshotDate = mockCommonBusinessLogic.GetAccountingStartDate(testSector);
            List<int> testYears = GetAllSnapshotYearsForSector(currentSnapshotDate);
            foreach (int testYear in testYears)
            {
                Assert.IsTrue(actualMissingEntry.MissingSnapshotYears.Contains(testYear), "Expected missing year");
            }
        }

        private Organisation CreateOrgWithExistingScopeForAllYears(int testOrgId, SectorTypes testSector, DateTime testLastSnapshotDate)
        {
            var mockOrg = new Organisation {OrganisationId = testOrgId, SectorType = testSector, Status = OrganisationStatuses.Active};

            for (int year = Global.FirstReportingYear; year <= testLastSnapshotDate.Year; year++)
            {
                mockOrg.OrganisationScopes.Add(
                    new OrganisationScope {
                        OrganisationId = mockOrg.OrganisationId,
                        Organisation = mockOrg,
                        SnapshotDate = new DateTime(year, testLastSnapshotDate.Month, testLastSnapshotDate.Day),
                        ScopeStatus = ScopeStatuses.InScope,
                        Status = ScopeRowStatuses.Active
                    });
            }

            return mockOrg;
        }

        private Organisation CreateOrgWithMissingScopesForAllYears(int testOrgId, SectorTypes testSector)
        {
            return new Organisation {OrganisationId = testOrgId, SectorType = testSector, Status = OrganisationStatuses.Active};
        }

        private Organisation CreateOrgWithUnknownScopesForAllYears(int testOrgId, SectorTypes testSector, DateTime testLastSnapshotDate)
        {
            var mockOrg = new Organisation {OrganisationId = testOrgId, SectorType = testSector, Status = OrganisationStatuses.Active};

            for (int year = Global.FirstReportingYear; year <= testLastSnapshotDate.Year; year++)
            {
                mockOrg.OrganisationScopes.Add(
                    new OrganisationScope {
                        OrganisationId = mockOrg.OrganisationId,
                        Organisation = mockOrg,
                        SnapshotDate = new DateTime(year, testLastSnapshotDate.Month, testLastSnapshotDate.Day),
                        ScopeStatus = ScopeStatuses.Unknown,
                        Status = ScopeRowStatuses.Active
                    });
            }

            return mockOrg;
        }

        private List<int> GetAllSnapshotYearsForSector(DateTime currentSnapshotDate)
        {
            int currentYear = currentSnapshotDate.Year;
            var results = new List<int>();
            for (int year = Global.FirstReportingYear; year <= currentYear; year++)
            {
                results.Add(year);
            }

            return results;
        }

    }

}

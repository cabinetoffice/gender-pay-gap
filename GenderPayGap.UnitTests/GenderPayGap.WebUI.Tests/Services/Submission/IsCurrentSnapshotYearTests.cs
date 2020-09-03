using System;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Tests.Common.Classes;
using GenderPayGap.WebUI.BusinessLogic.Services;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.Tests.Services.SubmissionService
{

    public class IsCurrentSnapshotYearTests
    {

        private Mock<IDataRepository> mockDataRepo;
        private Mock<IDraftFileBusinessLogic> mockDraftFileBL;
        private Mock<IScopeBusinessLogic> mockScopeBL;

        [SetUp]
        public void BeforeEach()
        {
            mockDataRepo = MoqHelpers.CreateMockAsyncDataRepository();
            mockScopeBL = new Mock<IScopeBusinessLogic>();
            mockDraftFileBL = new Mock<IDraftFileBusinessLogic>();
        }

        [TestCase(OrganisationSectors.Private, 2017)]
        [TestCase(OrganisationSectors.Public, 2017)]
        [TestCase(OrganisationSectors.Private, 2018)]
        [TestCase(OrganisationSectors.Public, 2018)]
        public void ReturnsFalseWhenNotCurrentYear(OrganisationSectors testSector, int testYear)
        {
            // Arrange
            DateTime testSnapshotDate = testSector.GetAccountingStartDate();
            var expectCalledGetSnapshotDate = false;

            // Mocks
            var mockService = new Mock<WebUI.Classes.Services.SubmissionService>(
                mockDataRepo.Object,
                mockScopeBL.Object,
                mockDraftFileBL.Object);
            mockService.CallBase = true;

            // Override GetPreviousReportingStartDate and return expectedYear
            mockService.Setup(ss => ss.GetSnapshotDate(It.IsIn(testSector), It.IsAny<int>()))
                .Returns(
                    () => {
                        expectCalledGetSnapshotDate = true;
                        return testSnapshotDate;
                    });

            // Assert
            WebUI.Classes.Services.SubmissionService testService = mockService.Object;
            bool actual = testService.IsCurrentSnapshotYear(testSector, testYear);

            Assert.IsTrue(expectCalledGetSnapshotDate, "Expected to call GetSnapshotDate");
            Assert.IsFalse(actual, "Expected IsCurrentSnapshotYear to return true");
        }

        [TestCase(OrganisationSectors.Private)]
        [TestCase(OrganisationSectors.Public)]
        public void ReturnsTrueForCurrentYear(OrganisationSectors testSector)
        {
            // Arrange
            DateTime testSnapshotDate = testSector.GetAccountingStartDate();
            int testYear = testSnapshotDate.Year;

            // Mocks
            var testService = new WebUI.Classes.Services.SubmissionService(
                mockDataRepo.Object,
                mockScopeBL.Object,
                mockDraftFileBL.Object);

            // Assert
            bool actual = testService.IsCurrentSnapshotYear(testSector, testYear);

            Assert.IsTrue(actual, "Expected IsCurrentSnapshotYear to return true");
        }

    }

}

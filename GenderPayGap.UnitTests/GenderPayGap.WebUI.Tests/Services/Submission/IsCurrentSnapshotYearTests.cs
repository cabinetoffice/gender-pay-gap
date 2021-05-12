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
            mockDataRepo = MoqHelpers.CreateMockDataRepository();
            mockScopeBL = new Mock<IScopeBusinessLogic>();
            mockDraftFileBL = new Mock<IDraftFileBusinessLogic>();
        }

        [TestCase(SectorTypes.Private, 2017)]
        [TestCase(SectorTypes.Public, 2017)]
        [TestCase(SectorTypes.Private, 2018)]
        [TestCase(SectorTypes.Public, 2018)]
        public void ReturnsFalseWhenNotCurrentYear(SectorTypes testSector, int testYear)
        {
            // Mocks
            var mockService = new Mock<WebUI.Classes.Services.SubmissionService>(
                mockDataRepo.Object,
                mockScopeBL.Object,
                mockDraftFileBL.Object);
            mockService.CallBase = true;

            // Assert
            WebUI.Classes.Services.SubmissionService testService = mockService.Object;
            bool actual = testService.IsCurrentSnapshotYear(testSector, testYear);

            Assert.IsFalse(actual, "Expected IsCurrentSnapshotYear to return true");
        }

        [TestCase(SectorTypes.Private)]
        [TestCase(SectorTypes.Public)]
        public void ReturnsTrueForCurrentYear(SectorTypes testSector)
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

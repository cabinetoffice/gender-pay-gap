using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic;
using GenderPayGap.BusinessLogic.Models.Organisation;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Tests.Common.Classes;
using GenderPayGap.WebUI.Options;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.Tests.Services.SubmissionService
{

    public class GetAllEditableReportsAsyncTests
    {

        private ICommonBusinessLogic mockCommonBusinessLogic;
        private Mock<IDataRepository> mockDataRepo;
        private Mock<IDraftFileBusinessLogic> mockDraftFileBL;
        private Mock<IScopeBusinessLogic> mockScopeBL;

        [SetUp]
        public void BeforeEach()
        {
            mockCommonBusinessLogic = new CommonBusinessLogic();
            mockDataRepo = MoqHelpers.CreateMockAsyncDataRepository();
            mockScopeBL = new Mock<IScopeBusinessLogic>();
            mockDraftFileBL = new Mock<IDraftFileBusinessLogic>();
        }

        [TestCase(SectorTypes.Public, 3)]
        [TestCase(SectorTypes.Public, 2)]
        [TestCase(SectorTypes.Public, 1)]
        [TestCase(SectorTypes.Private, 3)]
        [TestCase(SectorTypes.Private, 2)]
        [TestCase(SectorTypes.Private, 1)]
        public async Task ReportCountIsControlledBySubmissionOptions(SectorTypes testSector, int testEditableReportCount)
        {
            // Arrange
            var testConfig = new SubmissionOptions {EditableReportCount = testEditableReportCount};
            var testOrg = new Organisation {OrganisationId = 1, SectorType = testSector};
            var testUserOrg = new UserOrganisation {Organisation = testOrg};
            DateTime testSnapshotDate = mockCommonBusinessLogic.GetAccountingStartDate(testOrg.SectorType);

            var mockService = new Mock<WebUI.Classes.Services.SubmissionService>(
                mockDataRepo.Object,
                mockScopeBL.Object,
                mockDraftFileBL.Object,
                MoqHelpers.CreateIOptionsSnapshotMock(testConfig));

            // Call the real functions unless overridden
            mockService.CallBase = true;

            // Act
            WebUI.Classes.Services.SubmissionService testService = mockService.Object;
            List<ReportInfoModel> actualResults = await testService.GetAllEditableReportsAsync(testUserOrg, testSnapshotDate);

            // Assert
            Assert.AreEqual(
                testEditableReportCount,
                actualResults.Count,
                $"Expected editable report count to be {testEditableReportCount}");
        }

    }

}

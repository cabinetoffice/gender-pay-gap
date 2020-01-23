using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.BusinessLogic.Models;
using GenderPayGap.BusinessLogic.Models.Submit;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Tests.Common.TestHelpers;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.BusinessLogic.Tests.Services
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class SubmissionBusinessLogicTests
    {

        /*
         * These tests are specifically left here containing 'returnId' and 'size' because there was a discussion in regards to whether size was relevant to determine if an organisation is expected to provide data (report) or not for a given year.
         * In the end it was decided it didn't, the message 'you need to report' or 'you didn't need to report' will be based on organisationScope only.
         *         */

        [TestCase(0, 249, ScopeStatuses.InScope, true)]
        [TestCase(0, 499, ScopeStatuses.InScope, true)]
        [TestCase(0, 249, ScopeStatuses.OutOfScope, false)]
        [TestCase(0, 499, ScopeStatuses.OutOfScope, false)]
        [TestCase(0, 249, ScopeStatuses.PresumedInScope, true)]
        [TestCase(0, 499, ScopeStatuses.PresumedInScope, true)]
        [TestCase(0, 249, ScopeStatuses.PresumedOutOfScope, false)]
        [TestCase(0, 499, ScopeStatuses.PresumedOutOfScope, false)]
        [TestCase(1, 249, ScopeStatuses.InScope, true)]
        [TestCase(1, 499, ScopeStatuses.InScope, true)]
        [TestCase(1, 249, ScopeStatuses.OutOfScope, false)]
        [TestCase(1, 499, ScopeStatuses.OutOfScope, false)]
        [TestCase(1, 249, ScopeStatuses.PresumedInScope, true)]
        [TestCase(1, 499, ScopeStatuses.PresumedInScope, true)]
        [TestCase(1, 249, ScopeStatuses.PresumedOutOfScope, false)]
        [TestCase(1, 499, ScopeStatuses.PresumedOutOfScope, false)]
        public void SubmissionBusinessLogic_IsInScopeForThisReportYear_ReturnsTrueFalseCorrectly(int returnId,
            int maxEmployees,
            ScopeStatuses scope,
            bool expected)
        {
            // Arrange
            var submissionBusinessLogic = new SubmissionBusinessLogic(
                Mock.Of<ICommonBusinessLogic>(),
                Mock.Of<IDataRepository>());

            Organisation organisation = OrganisationHelper.GetOrganisationInAGivenScope(scope, "employerRefInScope", 2017);
            Return returnToConvert = ReturnHelper.CreateTestReturn(organisation);

            returnToConvert.ReturnId = returnId;
            returnToConvert.MaxEmployees = maxEmployees;

            // Act
            ReturnViewModel actualConvertedReturnViewModel =
                submissionBusinessLogic.ConvertSubmissionReportToReturnViewModel(returnToConvert);

            // Assert
            Assert.AreEqual(expected, actualConvertedReturnViewModel.IsInScopeForThisReportYear);
        }

        [Test]
        public void SubmissionBusinessLogic_GetLateSubmissions_()
        {
            // Arrange
            var camlimLimited = new Organisation {
                OrganisationId = 127350, OrganisationName = "CAMLIN LIMITED", SectorType = SectorTypes.Private
            };

            DateTime currentYearAccountingStartDate = camlimLimited.SectorType.GetAccountingStartDate(); // 2019
            DateTime yearBeforeLastAccountingStartDate = currentYearAccountingStartDate.AddYears(-2); // 2017
            DateTime lastYearAccountingStartDate = currentYearAccountingStartDate.AddYears(-1); // 2018

            var listOfReturns = new List<Return> {
                new Return {
                    ReturnId = 1219470,
                    OrganisationId = camlimLimited.OrganisationId,
                    Organisation = camlimLimited,
                    AccountingDate = yearBeforeLastAccountingStartDate, // year before last must not be included in the late submissions
                    Modified = new DateTime(lastYearAccountingStartDate.Year, 04, 04, 9, 1, 45), // 2018-04-04 09:01:45.7479162
                    Modifications = null,
                    Status = ReturnStatuses.Submitted,
                    FirstName = "Joe",
                    LastName = "Bloggs",
                    JobTitle = "Finance Director",
                    EHRCResponse = false
                },
                new Return {
                    ReturnId = 1374970,
                    OrganisationId = camlimLimited.OrganisationId,
                    Organisation = camlimLimited,
                    AccountingDate = currentYearAccountingStartDate.AddYears(-1),
                    Modified = new DateTime(currentYearAccountingStartDate.Year, 04, 05, 18, 10, 27), // 2019-04-05 18:10:27.1811838
                    Modifications = null,
                    Status = ReturnStatuses.Retired, // Retired returns must not be included in the late submissions
                    FirstName = "Joe",
                    LastName = "Bloggs",
                    JobTitle = "Finance Director",
                    EHRCResponse = false
                },
                new Return {
                    ReturnId = 1375400,
                    OrganisationId = camlimLimited.OrganisationId,
                    Organisation = camlimLimited,
                    AccountingDate = currentYearAccountingStartDate.AddYears(-1),
                    Modified = new DateTime(currentYearAccountingStartDate.Year, 04, 08, 11, 55, 48), // 2019-04-08 11:55:48.5883128
                    Modifications = null,
                    Status = ReturnStatuses.Retired, // must not be included in the late submissions
                    FirstName = "Joe",
                    LastName = "Bloggs",
                    JobTitle = "Finance Director",
                    EHRCResponse = false
                },
                new Return {
                    ReturnId = 1375410,
                    OrganisationId = camlimLimited.OrganisationId,
                    Organisation = camlimLimited,
                    AccountingDate = currentYearAccountingStartDate.AddYears(-1),
                    Modified = new DateTime(currentYearAccountingStartDate.Year, 04, 08, 11, 58, 28), // 2019-04-08 11:58:28.0299466
                    Modifications = "Figures,LateReason",
                    LateReason =
                        "The pay gap report for female mean pay gap is lower but the female median is higher. The finished report showed that both were higher and therefore needed to be adjusted by inserting a minus sign.",
                    Status = ReturnStatuses.Submitted,
                    FirstName = "Joe",
                    LastName = "Bloggs",
                    JobTitle = "Finance Director",
                    EHRCResponse = false
                }
            };

            var listOfOrganisationScope = new List<OrganisationScope> {
                new OrganisationScope {
                    OrganisationScopeId = 258343,
                    OrganisationId = camlimLimited.OrganisationId,
                    SnapshotDate = currentYearAccountingStartDate.AddYears(-2),
                    ScopeStatusDate = new DateTime(yearBeforeLastAccountingStartDate.Year, 12, 12, 16, 16, 58),
                    Status = ScopeRowStatuses.Active,
                    ScopeStatus = ScopeStatuses.InScope
                },
                new OrganisationScope {
                    OrganisationScopeId = 2273603,
                    OrganisationId = camlimLimited.OrganisationId,
                    ScopeStatusDate = new DateTime(lastYearAccountingStartDate.Year, 06, 07, 17, 46, 22),
                    SnapshotDate = currentYearAccountingStartDate.AddYears(-1),
                    Status = ScopeRowStatuses.Active,
                    ScopeStatus = ScopeStatuses.PresumedInScope
                }
            };

            var mockedDataRepository = new Mock<IDataRepository>();

            mockedDataRepository
                .Setup(x => x.GetAll<Return>())
                .Returns(listOfReturns.AsQueryable());

            mockedDataRepository
                .Setup(x => x.GetAll<OrganisationScope>())
                .Returns(listOfOrganisationScope.AsQueryable());

            var submissionBusinessLogic = new SubmissionBusinessLogic(
                Mock.Of<ICommonBusinessLogic>(),
                mockedDataRepository.Object);

            // Act
            IEnumerable<LateSubmissionsFileModel> actualConvertedReturnViewModel = submissionBusinessLogic.GetLateSubmissions();

            // Assert
            LateSubmissionsFileModel yearBeforeLastReport = actualConvertedReturnViewModel.FirstOrDefault(x => x.ReportId == 1219470);
            Assert.Null(
                yearBeforeLastReport,
                "ReportId 21947 was marked as submitted the 'year before last', therefore it must not be included in the late submissions report");

            LateSubmissionsFileModel firstRetiredReport = actualConvertedReturnViewModel.FirstOrDefault(x => x.ReportId == 1374970);
            Assert.Null(firstRetiredReport, "Retired returns must not be included in the late submissions");

            LateSubmissionsFileModel secondRetiredReport = actualConvertedReturnViewModel.FirstOrDefault(x => x.ReportId == 1375400);
            Assert.Null(secondRetiredReport, "Retired returns must not be included in the late submissions");

            LateSubmissionsFileModel foundReport = actualConvertedReturnViewModel.FirstOrDefault(x => x.ReportId == 1375410);
            Assert.NotNull(foundReport, "Expected reportId 37541 to be included in the list of late submissions");
        }

    }
}

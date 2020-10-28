using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Tests.Common.Classes;
using GenderPayGap.Tests.Common.TestHelpers;
using GenderPayGap.Tests.TestHelpers;
using GenderPayGap.WebUI.BusinessLogic.Classes;
using GenderPayGap.WebUI.BusinessLogic.Models.Submit;
using GenderPayGap.WebUI.BusinessLogic.Services;
using GenderPayGap.WebUI.Classes.Services;
using GenderPayGap.WebUI.Models.Submit;
using GenderPayGap.WebUI.Tests.TestHelpers;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using RangeAttribute = System.ComponentModel.DataAnnotations.RangeAttribute;

namespace GenderPayGap.Tests
{

    [TestFixture]
    [SetCulture("en-GB")]
    public class SubmissionServiceTests : AssertionHelper
    {

        [SetUp]
        public void BeforeEach()
        {
            mockDataRepo = MoqHelpers.CreateMockDataRepository();
            mockScopeBL = new Mock<IScopeBusinessLogic>();
            _mockDraftFileBL = new Mock<IDraftFileBusinessLogic>();
        }

        private Mock<IDraftFileBusinessLogic> _mockDraftFileBL;
        private Mock<IDataRepository> mockDataRepo;
        private Mock<IScopeBusinessLogic> mockScopeBL;

        [Test]
        [Description("CreateDraftSubmissionFromViewModel: Converts View Model to Return")]
        public void CreateDraftSubmissionFromViewModel_Converts_View_Model_to_Return()
        {
            Organisation mockedOrganisation = OrganisationHelper.GetPublicOrganisation();
            var testModel = new ReturnViewModel
            {
                AccountingDate = new DateTime(1999, 4, 5),
                CompanyLinkToGPGInfo = "https://CompanyLinkToGPGInfo",
                DiffMeanBonusPercent = 100,
                DiffMeanHourlyPayPercent = 99,
                DiffMedianBonusPercent = 98,
                DiffMedianHourlyPercent = 97,
                FemaleLowerPayBand = 96,
                FemaleMedianBonusPayPercent = 95,
                FemaleMiddlePayBand = 94,
                FemaleUpperPayBand = 93,
                FemaleUpperQuartilePayBand = 92,
                FirstName = "Test",
                LastName = "User",
                JobTitle = "QA",
                MaleLowerPayBand = 91,
                MaleMedianBonusPayPercent = 90,
                MaleUpperQuartilePayBand = 89,
                MaleMiddlePayBand = 88,
                MaleUpperPayBand = 87,
                OrganisationId = mockedOrganisation.OrganisationId,
                OrganisationSize = OrganisationSizes.Employees250To499,
                LateReason = "A LateReason"
            };

            // Mocks
            var testService = new SubmissionService(
                mockDataRepo.Object,
                mockScopeBL.Object,
                _mockDraftFileBL.Object);

            mockDataRepo.Setup(x => x.Get<Organisation>(It.IsAny<long>())).Returns(mockedOrganisation);

            // Assert
            Return testReturn = testService.CreateDraftSubmissionFromViewModel(testModel);

            var testOrgSizeRange = testModel.OrganisationSize.GetAttribute<RangeAttribute>();

            Expect(testReturn.AccountingDate == testModel.AccountingDate);
            Expect(testReturn.CompanyLinkToGPGInfo == testModel.CompanyLinkToGPGInfo);
            Expect(testReturn.DiffMeanBonusPercent == testModel.DiffMeanBonusPercent.Value);
            Expect(testReturn.DiffMeanHourlyPayPercent == testModel.DiffMeanHourlyPayPercent.Value);
            Expect(testReturn.DiffMedianBonusPercent == testModel.DiffMedianBonusPercent.Value);
            Expect(testReturn.DiffMedianHourlyPercent == testModel.DiffMedianHourlyPercent.Value);
            Expect(testReturn.FemaleLowerPayBand == testModel.FemaleLowerPayBand.Value);
            Expect(testReturn.FemaleMedianBonusPayPercent == testModel.FemaleMedianBonusPayPercent.Value);
            Expect(testReturn.FemaleMiddlePayBand == testModel.FemaleMiddlePayBand.Value);
            Expect(testReturn.FemaleUpperPayBand == testModel.FemaleUpperPayBand.Value);
            Expect(testReturn.FemaleUpperQuartilePayBand == testModel.FemaleUpperQuartilePayBand.Value);
            Expect(testReturn.FirstName == testModel.FirstName);
            Expect(testReturn.LastName == testModel.LastName);
            Expect(testReturn.JobTitle == testModel.JobTitle);
            Expect(testReturn.MaleLowerPayBand == testModel.MaleLowerPayBand.Value);
            Expect(testReturn.MaleMedianBonusPayPercent == testModel.MaleMedianBonusPayPercent.Value);
            Expect(testReturn.MaleUpperQuartilePayBand == testModel.MaleUpperQuartilePayBand.Value);
            Expect(testReturn.MaleMiddlePayBand == testModel.MaleMiddlePayBand.Value);
            Expect(testReturn.MaleUpperPayBand == testModel.MaleUpperPayBand.Value);
            Expect(testReturn.Status == ReturnStatuses.Draft);
            Expect(testReturn.OrganisationId == testModel.OrganisationId);
            Expect(testReturn.MinEmployees == (int)testOrgSizeRange.Minimum);
            Expect(testReturn.MaxEmployees == (int)testOrgSizeRange.Maximum);
            Expect(testReturn.LateReason == testModel.LateReason);
        }
        
        [Test]
        [Description("GetSubmissionChangeSummary: Detects no changes")]
        public void GetSubmissionChangeSummary_Detects_no_changes()
        {
            var testOldReturn = new Return
            {
                DiffMeanBonusPercent = 100,
                DiffMeanHourlyPayPercent = 99,
                DiffMedianBonusPercent = 98,
                DiffMedianHourlyPercent = 97,
                FemaleLowerPayBand = 96,
                FemaleMedianBonusPayPercent = 95,
                FemaleMiddlePayBand = 94,
                FemaleUpperPayBand = 93,
                FemaleUpperQuartilePayBand = 92,
                MaleLowerPayBand = 91,
                MaleMedianBonusPayPercent = 90,
                MaleUpperQuartilePayBand = 89,
                MaleMiddlePayBand = 88,
                MaleUpperPayBand = 87,
                FirstName = "Test",
                LastName = "User",
                JobTitle = "QA",
                CompanyLinkToGPGInfo = "http://unittesting",
                MinEmployees = 250,
                MaxEmployees = 499,
                Organisation = new Organisation { SectorType = SectorTypes.Private }
            };

            // Mocks
            var testService = new SubmissionService(
                mockDataRepo.Object,
                mockScopeBL.Object,
                _mockDraftFileBL.Object);

            // Copy the original
            var testNewReturn = (Return)testOldReturn.CopyProperties(new Return());

            // Assert
            SubmissionChangeSummary testChangeSummary = testService.GetSubmissionChangeSummary(testNewReturn, testOldReturn);
            string failMessage = $"Assert: Failed to detect {nameof(testNewReturn.CompanyLinkToGPGInfo)} had changed";
            Expect(testChangeSummary.HasChanged == false, failMessage);
            Expect(testChangeSummary.FiguresChanged == false, failMessage);
            Expect(testChangeSummary.OrganisationSizeChanged == false, failMessage);
            Expect(testChangeSummary.PersonResonsibleChanged == false, failMessage);
            Expect(testChangeSummary.WebsiteUrlChanged == false, failMessage);
        }

        [Test]
        [Description("GetSubmissionChangeSummary: Detects when the figures have changed")]
        public void GetSubmissionChangeSummary_Detects_when_the_figures_have_changed()
        {
            var testOldReturn = new Return
            {
                DiffMeanBonusPercent = 100,
                DiffMeanHourlyPayPercent = 99,
                DiffMedianBonusPercent = 98,
                DiffMedianHourlyPercent = 97,
                FemaleLowerPayBand = 96,
                FemaleMedianBonusPayPercent = 95,
                FemaleMiddlePayBand = 94,
                FemaleUpperPayBand = 93,
                FemaleUpperQuartilePayBand = 92,
                MaleLowerPayBand = 91,
                MaleMedianBonusPayPercent = 90,
                MaleUpperQuartilePayBand = 89,
                MaleMiddlePayBand = 88,
                MaleUpperPayBand = 87,
                Organisation = new Organisation { SectorType = SectorTypes.Private }
            };

            string[] figureProps = {
                nameof(testOldReturn.DiffMeanBonusPercent),
                nameof(testOldReturn.DiffMeanHourlyPayPercent),
                nameof(testOldReturn.DiffMedianBonusPercent),
                nameof(testOldReturn.DiffMedianHourlyPercent),
                nameof(testOldReturn.FemaleLowerPayBand),
                nameof(testOldReturn.FemaleMedianBonusPayPercent),
                nameof(testOldReturn.FemaleMiddlePayBand),
                nameof(testOldReturn.FemaleUpperPayBand),
                nameof(testOldReturn.FemaleUpperQuartilePayBand),
                nameof(testOldReturn.MaleLowerPayBand),
                nameof(testOldReturn.MaleMedianBonusPayPercent),
                nameof(testOldReturn.MaleUpperQuartilePayBand),
                nameof(testOldReturn.MaleMiddlePayBand),
                nameof(testOldReturn.MaleUpperPayBand)
            };

            decimal changeValue = 0;

            // Mocks
            var testService = new SubmissionService(
                mockDataRepo.Object,
                mockScopeBL.Object,
                _mockDraftFileBL.Object);

            // Assert all figures
            foreach (string figurePropName in figureProps)
            {
                // Restore the original
                var testNewReturn = (Return)testOldReturn.CopyProperties(new Return());

                // Make the change
                testNewReturn.SetProperty(figurePropName, changeValue);

                // Assert
                SubmissionChangeSummary testChangeSummary = testService.GetSubmissionChangeSummary(testNewReturn, testOldReturn);
                string failMessage = $"Assert: Failed to detect {figurePropName} had changed";
                Expect(testChangeSummary.HasChanged, failMessage);
                Expect(testChangeSummary.FiguresChanged, failMessage);
                Expect(testChangeSummary.OrganisationSizeChanged == false, failMessage);
                Expect(testChangeSummary.PersonResonsibleChanged == false, failMessage);
                Expect(testChangeSummary.WebsiteUrlChanged == false, failMessage);
            }
        }

        [Test]
        [Description("GetSubmissionChangeSummary: Detects when the organisation size has changed")]
        public void GetSubmissionChangeSummary_Detects_when_the_organisation_size_has_changed()
        {
            var testOldReturn = new Return { Organisation = new Organisation { SectorType = SectorTypes.Private } };

            // Mocks
            var testService = new SubmissionService(
                mockDataRepo.Object,
                mockScopeBL.Object,
                _mockDraftFileBL.Object);

            // Copy the original
            var testNewReturn = (Return)testOldReturn.CopyProperties(new Return());

            // Make the change
            testNewReturn.MinEmployees = 250;
            testNewReturn.MaxEmployees = 499;

            // Assert
            SubmissionChangeSummary testChangeSummary = testService.GetSubmissionChangeSummary(testNewReturn, testOldReturn);
            string failMessage = $"Assert: Failed to detect {nameof(testNewReturn.CompanyLinkToGPGInfo)} had changed";
            Expect(testChangeSummary.HasChanged, failMessage);
            Expect(testChangeSummary.FiguresChanged == false, failMessage);
            Expect(testChangeSummary.OrganisationSizeChanged, failMessage);
            Expect(testChangeSummary.PersonResonsibleChanged == false, failMessage);
            Expect(testChangeSummary.WebsiteUrlChanged == false, failMessage);
        }

        [Test]
        [Description("GetSubmissionChangeSummary: Detects when the person responsible has changed")]
        public void GetSubmissionChangeSummary_Detects_when_the_person_responsible_has_changed()
        {
            var testOldReturn = new Return
            {
                FirstName = "Test",
                LastName = "User",
                JobTitle = "QA",
                Organisation = new Organisation { SectorType = SectorTypes.Private }
            };

            string[] personProps = { nameof(testOldReturn.FirstName), nameof(testOldReturn.LastName), nameof(testOldReturn.JobTitle) };

            var changeValue = "Mr T";

            // Mocks
            var testService = new SubmissionService(
                mockDataRepo.Object,
                mockScopeBL.Object,
                _mockDraftFileBL.Object);

            // Assert all figures
            foreach (string personPropName in personProps)
            {
                // Restore the original
                var testNewReturn = (Return)testOldReturn.CopyProperties(new Return());

                // Make the change
                testNewReturn.SetProperty(personPropName, changeValue);

                // Assert
                SubmissionChangeSummary testChangeSummary = testService.GetSubmissionChangeSummary(testNewReturn, testOldReturn);
                string failMessage = $"Assert: Failed to detect {personPropName} had changed";
                Expect(testChangeSummary.HasChanged, failMessage);
                Expect(testChangeSummary.FiguresChanged == false, failMessage);
                Expect(testChangeSummary.OrganisationSizeChanged == false, failMessage);
                Expect(testChangeSummary.PersonResonsibleChanged, failMessage);
                Expect(testChangeSummary.WebsiteUrlChanged == false, failMessage);
            }
        }

        [Test]
        [Description("GetSubmissionChangeSummary: Detects when the website url has changed")]
        public void GetSubmissionChangeSummary_Detects_when_the_website_url_has_changed()
        {
            var testOldReturn = new Return { CompanyLinkToGPGInfo = "", Organisation = new Organisation { SectorType = SectorTypes.Private } };

            // Mocks
            var testService = new SubmissionService(
                mockDataRepo.Object,
                mockScopeBL.Object,
                _mockDraftFileBL.Object);

            // Copy the original
            var testNewReturn = (Return)testOldReturn.CopyProperties(new Return());

            // Make the change
            testNewReturn.CompanyLinkToGPGInfo = "http://unittesting";

            // Assert
            SubmissionChangeSummary testChangeSummary = testService.GetSubmissionChangeSummary(testNewReturn, testOldReturn);
            string failMessage = $"Assert: Failed to detect {nameof(testNewReturn.CompanyLinkToGPGInfo)} had changed";
            Expect(testChangeSummary.HasChanged, failMessage);
            Expect(testChangeSummary.FiguresChanged == false, failMessage);
            Expect(testChangeSummary.OrganisationSizeChanged == false, failMessage);
            Expect(testChangeSummary.PersonResonsibleChanged == false, failMessage);
            Expect(testChangeSummary.WebsiteUrlChanged, failMessage);
        }

        [Test]
        [Description("GetSubmissionForSnapshotYear: Returns latest submission for specified org and year")]
        public async Task GetSubmissionForSnapshotYear_Returns_latest_submission_for_specified_org_and_yearAsync()
        {
            // Mocks
            var returns = new[] {
                new Return {ReturnId = 1, OrganisationId = 2, AccountingDate = new DateTime(1999, 1, 1), Status = ReturnStatuses.Submitted},
                new Return {ReturnId = 2, OrganisationId = 1, AccountingDate = new DateTime(1999, 1, 2), Status = ReturnStatuses.Submitted},
                new Return {ReturnId = 3, OrganisationId = 1, AccountingDate = new DateTime(2000, 1, 3), Status = ReturnStatuses.Submitted},
                new Return {ReturnId = 4, OrganisationId = 1, AccountingDate = new DateTime(2000, 1, 4), Status = ReturnStatuses.Submitted},
                new Return {ReturnId = 5, OrganisationId = 2, AccountingDate = new DateTime(2000, 1, 5), Status = ReturnStatuses.Submitted}
            };

            mockDataRepo.SetupGetAll((ICollection<Return>)returns);

            var mockService = new Mock<SubmissionService>(
                mockDataRepo.Object,
                mockScopeBL.Object,
                _mockDraftFileBL.Object);

            mockService.CallBase = true;
            mockService.Setup(s => s.IsValidSnapshotYear(It.IsAny<int>())).Returns(true);

            // Assert
            SubmissionService testService = mockService.Object;
            Return actualReturn = await testService.GetReturnFromDatabaseAsync(1, 2000);
            Expect(actualReturn != null);
            Expect(actualReturn.ReturnId == 4);
        }

        [Test]
        [Description("GetSubmissionForSnapshotYear: Returns null when year does not exist")]
        public async Task GetSubmissionForSnapshotYear_Returns_null_when_year_does_not_exist()
        {
            // Mocks
            mockDataRepo.Setup(dr => dr.GetAll<Return>())
                .Returns(
                    new[] {
                            new Return {ReturnId = 1, OrganisationId = 2, AccountingDate = new DateTime(1999, 1, 1)},
                            new Return {ReturnId = 2, OrganisationId = 1, AccountingDate = new DateTime(1999, 1, 2)},
                            new Return {ReturnId = 3, OrganisationId = 1, AccountingDate = new DateTime(2000, 1, 3)},
                            new Return {ReturnId = 4, OrganisationId = 1, AccountingDate = new DateTime(2000, 1, 4)},
                            new Return {ReturnId = 5, OrganisationId = 2, AccountingDate = new DateTime(2000, 1, 5)}
                        }.AsQueryable()
                        .BuildMock()
                        .Object);

            var testService = new SubmissionService(
                mockDataRepo.Object,
                mockScopeBL.Object,
                _mockDraftFileBL.Object);

            // Assert
            Return actualReturn = await testService.GetReturnFromDatabaseAsync(1, 1998);
            Expect(actualReturn == null);
        }

        [Test]
        [Description("GetViewModelForSubmission: Given a new submission Then returns empty model with org id and reporting date only")]
        public async Task
            GetViewModelForSubmission_Given_a_new_submission_Then_returns_empty_model_with_org_id_and_reporting_date_onlyAsync()
        {
            var testUserId = 123;
            int testYear = VirtualDateTime.Now.AddYears(-1).Year;

            // Mocks
            var testOrganisation = new Organisation { OrganisationId = 524124, SectorType = SectorTypes.Private };

            mockDataRepo.Setup(dr => dr.GetAll<UserOrganisation>())
                .Returns(
                    new[] {
                            new UserOrganisation {
                                UserId = testUserId, OrganisationId = testOrganisation.OrganisationId, Organisation = testOrganisation
                            }
                        }.AsQueryable()
                        .BuildMock()
                        .Object);

            mockDataRepo.Setup(dr => dr.GetAll<Return>())
                .Returns(new Return[] { }.AsQueryable().BuildMock().Object);

            var testDraftFileFileBusinessLogic = new DraftFileBusinessLogic(MoqHelpers.CreateMockDataRepository().Object);
            Draft actualDraftFile = testDraftFileFileBusinessLogic.GetExistingOrNew(
                testOrganisation.OrganisationId,
                testYear,
                testUserId);

            var testService = new SubmissionService(
                mockDataRepo.Object,
                mockScopeBL.Object,
                testDraftFileFileBusinessLogic);

            // Test
            ReturnViewModel actualModel = await testService.GetReturnViewModelAsync(testOrganisation.OrganisationId, testYear, testUserId);

            // Assert
            Assert.NotNull(actualModel);
            Assert.AreEqual(testOrganisation.OrganisationId, actualModel.OrganisationId);
            Assert.AreEqual(testYear, actualModel.AccountingDate.Year);

            testDraftFileFileBusinessLogic.DiscardDraft(actualDraftFile);
        }

        [Test]
        [Description("GetViewModelForSubmission: Given an existing submission Then returns return data")]
        public async Task GetViewModelForSubmission_Given_an_existing_submission_Then_returns_return_dataAsync()
        {
            var testUserId = 123;
            var testOrganisationId = 524124;
            var testYear = 2001;

            var testReturn = new Return
            {
                AccountingDate = new DateTime(testYear, 1, 1),
                OrganisationId = testOrganisationId,
                DiffMeanBonusPercent = 100,
                DiffMeanHourlyPayPercent = 99,
                DiffMedianBonusPercent = 98,
                DiffMedianHourlyPercent = 97,
                FemaleLowerPayBand = 96,
                FemaleMedianBonusPayPercent = 95,
                FemaleMiddlePayBand = 94,
                FemaleUpperPayBand = 93,
                FemaleUpperQuartilePayBand = 92,
                MaleLowerPayBand = 91,
                MaleMedianBonusPayPercent = 90,
                MaleUpperQuartilePayBand = 89,
                MaleMiddlePayBand = 88,
                MaleUpperPayBand = 87,
                FirstName = "Test",
                LastName = "User",
                JobTitle = "QA",
                CompanyLinkToGPGInfo = "http://unittesting",
                MinEmployees = 250,
                MaxEmployees = 499,
                Status = ReturnStatuses.Submitted,
                Organisation = new Organisation { SectorType = SectorTypes.Private }
            };

            // Mocks
            mockDataRepo.Setup(dr => dr.GetAll<UserOrganisation>())
                .Returns(
                    new[] {
                            new UserOrganisation {
                                UserId = testUserId, OrganisationId = testOrganisationId, Organisation = testReturn.Organisation
                            }
                        }.AsQueryable()
                        .BuildMock()
                        .Object);

            mockDataRepo.Setup(dr => dr.GetAll<Return>()).Returns(new[] { testReturn }.AsQueryable().BuildMock().Object);

            var mockService = new Mock<SubmissionService>(
                mockDataRepo.Object,
                mockScopeBL.Object,
                _mockDraftFileBL.Object);
            mockService.CallBase = true;
            mockService.Setup(s => s.IsValidSnapshotYear(It.IsIn(testYear))).Returns(true);

            // Test
            SubmissionService testService = mockService.Object;
            ReturnViewModel actualModel = await testService.GetReturnViewModelAsync(testOrganisationId, testYear, testUserId);

            // Assert
            Expect(actualModel != null);
            Expect(actualModel.OrganisationId == testOrganisationId);
            Expect(actualModel.AccountingDate.Year == testYear);

            var testOrgSizeRange = actualModel.OrganisationSize.GetAttribute<RangeAttribute>();
            Expect(testReturn.CompanyLinkToGPGInfo == actualModel.CompanyLinkToGPGInfo);
            Expect(testReturn.DiffMeanBonusPercent == actualModel.DiffMeanBonusPercent.Value);
            Expect(testReturn.DiffMeanHourlyPayPercent == actualModel.DiffMeanHourlyPayPercent.Value);
            Expect(testReturn.DiffMedianBonusPercent == actualModel.DiffMedianBonusPercent.Value);
            Expect(testReturn.DiffMedianHourlyPercent == actualModel.DiffMedianHourlyPercent.Value);
            Expect(testReturn.FemaleLowerPayBand == actualModel.FemaleLowerPayBand.Value);
            Expect(testReturn.FemaleMedianBonusPayPercent == actualModel.FemaleMedianBonusPayPercent.Value);
            Expect(testReturn.FemaleMiddlePayBand == actualModel.FemaleMiddlePayBand.Value);
            Expect(testReturn.FemaleUpperPayBand == actualModel.FemaleUpperPayBand.Value);
            Expect(testReturn.FemaleUpperQuartilePayBand == actualModel.FemaleUpperQuartilePayBand.Value);
            Expect(testReturn.FirstName == actualModel.FirstName);
            Expect(testReturn.LastName == actualModel.LastName);
            Expect(testReturn.JobTitle == actualModel.JobTitle);
            Expect(testReturn.MaleLowerPayBand == actualModel.MaleLowerPayBand.Value);
            Expect(testReturn.MaleMedianBonusPayPercent == actualModel.MaleMedianBonusPayPercent.Value);
            Expect(testReturn.MaleUpperQuartilePayBand == actualModel.MaleUpperQuartilePayBand.Value);
            Expect(testReturn.MaleMiddlePayBand == actualModel.MaleMiddlePayBand.Value);
            Expect(testReturn.MaleUpperPayBand == actualModel.MaleUpperPayBand.Value);
            Expect(testReturn.MinEmployees == (int)testOrgSizeRange.Minimum);
            Expect(testReturn.MaxEmployees == (int)testOrgSizeRange.Maximum);
        }

        [Test]
        [Description("GetViewModelForSubmission: Throws AuthenticationException when the user organisation isnt found")]
        public void GetViewModelForSubmission_Throws_AuthenticationException_when_the_user_organisation_isnt_found()
        {
            var testUserId = 123;
            var testOrganisationId = 524124;

            // Mocks
            mockDataRepo.Setup(dr => dr.GetAll<UserOrganisation>()).Returns(new UserOrganisation[] { }.AsQueryable().BuildMock().Object);

            var testService = new SubmissionService(
                mockDataRepo.Object,
                mockScopeBL.Object,
                _mockDraftFileBL.Object);

            // Test
            var ex = Assert.ThrowsAsync<AuthenticationException>(
                () => testService.GetReturnViewModelAsync(testOrganisationId, 2001, testUserId));

            // Assert
            Assert.That(
                ex.Message.Contains(
                    $"GetViewModelForSubmission: Failed to find the UserOrganisation for userId {testUserId} and organisationId {testOrganisationId}"));
        }

        [Test]
        [Description("IsCurrentSnapshotYear: Returns false When SnapshotYear is not the Current Year")]
        public void IsCurrentSnapshotYear_Returns_false_When_SnapshotYear_is_not_the_Current_Year()
        {
            var currentSnapshotYear = 2001;
            var testSnapshotYear = 1999;

            // Mocks
            var mockService = new Mock<SubmissionService>(
                mockDataRepo.Object,
                mockScopeBL.Object,
                _mockDraftFileBL.Object);
            mockService.CallBase = true;

            // Override GetCurrentReportingStartDate and return expectedYear
            mockService.Setup(ss => ss.GetSnapshotDate(It.IsIn(SectorTypes.Private), It.IsIn(testSnapshotYear)))
                .Returns(new DateTime(currentSnapshotYear, 4, 5));

            // Sanity checks
            Expect(currentSnapshotYear != testSnapshotYear, "SanityCheck: curReportingYear should not equal the testReportingStartYear");

            // Assert
            SubmissionService testService = mockService.Object;
            Expect(testService.IsCurrentSnapshotYear(SectorTypes.Private, testSnapshotYear) == false);
        }

        [Test]
        [Description("IsCurrentSnapshotYear: Returns true When SnapshotYear is the Current Year")]
        public void IsCurrentSnapshotYear_Returns_true_When_SnapshotYear_is_the_Current_Year()
        {
            var currentSnapshotYear = 2001;
            var testSnapshotYear = 2001;

            // Mocks
            var mockService = new Mock<SubmissionService>(
                mockDataRepo.Object,
                mockScopeBL.Object,
                _mockDraftFileBL.Object);
            mockService.CallBase = true;

            // Override GetReportingStartDate and return expectedYear
            mockService.Setup(ss => ss.GetSnapshotDate(It.IsAny<SectorTypes>(), It.IsAny<int>()))
                .Returns(new DateTime(currentSnapshotYear, 4, 5));

            // Sanity checks
            Expect(currentSnapshotYear == testSnapshotYear, "SanityCheck: curReportingYear should equal the testReportingStartYear");

            // Assert
            SubmissionService testService = mockService.Object;
            Expect(testService.IsCurrentSnapshotYear(SectorTypes.Private, testSnapshotYear));
        }

    }

}

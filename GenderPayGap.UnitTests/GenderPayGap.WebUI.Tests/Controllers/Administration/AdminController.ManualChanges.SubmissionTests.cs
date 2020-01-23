using System;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Tests.Common.TestHelpers;
using GenderPayGap.Tests.TestHelpers;
using GenderPayGap.WebUI.Controllers.Administration;
using GenderPayGap.WebUI.Models.Admin;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Controllers.Administration
{
    [TestFixture]
    public class AdminControllerManualChangesSubmissionTests
    {

        private const string DeleteSubmissionsCommand = "Delete submissions";

        [Test]
        public async Task AdminController_ManualChanges_POST_Delete_Submissions_Works_When_Run_In_Test_Mode_Async()
        {
            // Arrange
            User databaseAdminUser = UserHelper.GetDatabaseAdmin();
            Organisation publicOrganisationWithSubmissionsToBeDeleted =
                OrganisationHelper.GetPublicOrganisation("EmployerReference05");
            Return mockedReturn = ReturnHelper.CreateTestReturn(
                publicOrganisationWithSubmissionsToBeDeleted,
                VirtualDateTime.Now.AddYears(-1).Year);

            var testController = UiTestHelper.GetController<AdminController>(
                databaseAdminUser.UserId,
                null,
                databaseAdminUser,
                publicOrganisationWithSubmissionsToBeDeleted,
                mockedReturn);

            var manualChangesViewModelMock = Mock.Of<ManualChangesViewModel>();
            manualChangesViewModelMock.Command = DeleteSubmissionsCommand;
            manualChangesViewModelMock.Parameters =
                $"{publicOrganisationWithSubmissionsToBeDeleted.EmployerReference}={mockedReturn.AccountingDate.Year}";

            // Act
            IActionResult actualManualChangesResult = await testController.ManualChanges(manualChangesViewModelMock);

            // Assert
            Assert.NotNull(actualManualChangesResult, "Expected a Result");

            var manualChangesViewResult = actualManualChangesResult as ViewResult;
            Assert.NotNull(manualChangesViewResult, "Expected ViewResult");

            var actualManualChangesViewModel = manualChangesViewResult.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            Assert.Multiple(
                () => {
                    Assert.AreEqual("SUCCESSFULLY TESTED 'Delete submissions': 1 of 1", actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual(
                        $"1: EMPLOYERREFERENCE05: Org123 Year='{VirtualDateTime.Now.AddYears(-1).Year}' Status='Submitted' set to 'Deleted'\r\n",
                        actualManualChangesViewModel.Results);
                    Assert.AreEqual("Delete submissions", actualManualChangesViewModel.LastTestedCommand);
                    Assert.AreEqual(
                        $"EmployerReference05={mockedReturn.AccountingDate.Year}",
                        actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be tested=true as this case is running in TEST mode");
                    Assert.IsNull(actualManualChangesViewModel.Comment);
                });
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Delete_Submissions_Works_When_Run_In_Live_Mode_Async()
        {
            // Arrange
            User databaseAdminUser = UserHelper.GetDatabaseAdmin();
            Organisation publicOrganisationWithSubmissionsToBeDeleted =
                OrganisationHelper.GetPublicOrganisation("EmployerReference052");
            Return mockedReturn = ReturnHelper.CreateTestReturn(
                publicOrganisationWithSubmissionsToBeDeleted,
                DateTime.Now.AddYears(-1).Year);

            var testController = UiTestHelper.GetController<AdminController>(
                databaseAdminUser.UserId,
                null,
                databaseAdminUser,
                publicOrganisationWithSubmissionsToBeDeleted,
                mockedReturn);

            var manualChangesViewModelMock = Mock.Of<ManualChangesViewModel>();
            manualChangesViewModelMock.Command = DeleteSubmissionsCommand;
            manualChangesViewModelMock.Parameters =
                $"{publicOrganisationWithSubmissionsToBeDeleted.EmployerReference}={mockedReturn.AccountingDate.Year}";

            /* live */
            manualChangesViewModelMock.LastTestedCommand = manualChangesViewModelMock.Command;
            manualChangesViewModelMock.LastTestedInput = manualChangesViewModelMock.Parameters;

            // Act
            IActionResult actualManualChangesResult = await testController.ManualChanges(manualChangesViewModelMock);

            // Assert
            Assert.NotNull(actualManualChangesResult, "Expected a Result");

            var manualChangesViewResult = actualManualChangesResult as ViewResult;
            Assert.NotNull(manualChangesViewResult, "Expected ViewResult");

            var actualManualChangesViewModel = manualChangesViewResult.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            Assert.AreEqual("SUCCESSFULLY EXECUTED 'Delete submissions': 1 of 1", actualManualChangesViewModel.SuccessMessage);
            Assert.AreEqual(
                $"1: EMPLOYERREFERENCE052: Org123 Year='{DateTime.Now.Year - 1}' Status='Submitted' set to 'Deleted'\r\n",
                actualManualChangesViewModel.Results);
            Assert.IsNull(actualManualChangesViewModel.LastTestedCommand);
            Assert.IsNull(actualManualChangesViewModel.LastTestedInput);
            Assert.False(actualManualChangesViewModel.Tested, "Must be tested=true as this case is running in LIVE mode");
            Assert.IsNull(actualManualChangesViewModel.Comment);
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Delete_Submissions_Fails_When_An_Equals_Sign_Is_Not_Sent_Async()
        {
            // Arrange
            User databaseAdminUser = UserHelper.GetDatabaseAdmin();
            var testController = UiTestHelper.GetController<AdminController>(databaseAdminUser.UserId, null, databaseAdminUser);

            var manualChangesViewModelMock = Mock.Of<ManualChangesViewModel>();
            manualChangesViewModelMock.Command = DeleteSubmissionsCommand;
            manualChangesViewModelMock.Parameters = "Not_Equals_Sign";

            // Act
            IActionResult actualManualChangesResult = await testController.ManualChanges(manualChangesViewModelMock);

            // Assert
            Assert.NotNull(actualManualChangesResult, "Expected a Result");

            var manualChangesViewResult = actualManualChangesResult as ViewResult;
            Assert.NotNull(manualChangesViewResult, "Expected ViewResult");

            var actualManualChangesViewModel = manualChangesViewResult.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            Assert.AreEqual("SUCCESSFULLY TESTED 'Delete submissions': 0 of 1", actualManualChangesViewModel.SuccessMessage);
            Assert.AreEqual(
                "<span style=\"color:Red\">1: ERROR: 'Not_Equals_Sign' does not contain '=' character</span>\r\n",
                actualManualChangesViewModel.Results);
            Assert.AreEqual("Delete submissions", actualManualChangesViewModel.LastTestedCommand);
            Assert.AreEqual("Not_Equals_Sign", actualManualChangesViewModel.LastTestedInput);
            Assert.True(actualManualChangesViewModel.Tested, "Must be true as this case is running in TEST mode");
            Assert.Null(actualManualChangesViewModel.Comment);
        }

        [TestCase("")]
        [TestCase(null)]
        public async Task AdminController_ManualChanges_POST_Delete_Submissions_Fails_When_No_Parameters_Are_Provided_Async(
            string nullOrEmptyParameter)
        {
            // Arrange
            User databaseAdminUser = UserHelper.GetDatabaseAdmin();
            Organisation publicOrganisationWithSubmissionsToBeDeleted =
                OrganisationHelper.GetPublicOrganisation("EmployerReference05");
            Return mockedReturn = ReturnHelper.CreateTestReturn(
                publicOrganisationWithSubmissionsToBeDeleted,
                DateTime.Now.AddYears(-1).Year);

            var testController = UiTestHelper.GetController<AdminController>(
                databaseAdminUser.UserId,
                null,
                databaseAdminUser,
                publicOrganisationWithSubmissionsToBeDeleted,
                mockedReturn);

            var manualChangesViewModelMock = Mock.Of<ManualChangesViewModel>();
            manualChangesViewModelMock.Command = DeleteSubmissionsCommand;
            manualChangesViewModelMock.Parameters = nullOrEmptyParameter;

            // Act
            IActionResult actualManualChangesResult = await testController.ManualChanges(manualChangesViewModelMock);

            // Assert
            Assert.NotNull(actualManualChangesResult, "Expected a Result");

            var manualChangesViewResult = actualManualChangesResult as ViewResult;
            Assert.NotNull(manualChangesViewResult, "Expected ViewResult");

            var actualManualChangesViewModel = manualChangesViewResult.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            ModelStateEntry modelState = testController.ModelState[""];
            ModelError reportedError = modelState.Errors.First();

            Assert.AreEqual("ERROR: You must supply 1 or more input parameters", reportedError.ErrorMessage);
            Assert.IsNull(actualManualChangesViewModel.SuccessMessage);
            Assert.AreEqual("", actualManualChangesViewModel.Results);
            Assert.IsNull(actualManualChangesViewModel.LastTestedCommand);
            Assert.IsNull(actualManualChangesViewModel.LastTestedInput);
            Assert.False(actualManualChangesViewModel.Tested, "Must be false as this case has failed");
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Delete_Submissions_Fails_When_Employer_Reference_Is_Duplicated_Async()
        {
            // Arrange
            User databaseAdminUser = UserHelper.GetDatabaseAdmin();
            Organisation publicOrganisationWithSubmissionsToBeDeleted =
                OrganisationHelper.GetPublicOrganisation("EmployerReference05");
            Return mockedReturn = ReturnHelper.CreateTestReturn(
                publicOrganisationWithSubmissionsToBeDeleted,
                DateTime.Now.AddYears(-1).Year);

            var testController = UiTestHelper.GetController<AdminController>(
                databaseAdminUser.UserId,
                null,
                databaseAdminUser,
                publicOrganisationWithSubmissionsToBeDeleted,
                mockedReturn);

            var manualChangesViewModelMock = Mock.Of<ManualChangesViewModel>();
            manualChangesViewModelMock.Command = DeleteSubmissionsCommand;
            manualChangesViewModelMock.Parameters =
                $"{publicOrganisationWithSubmissionsToBeDeleted.EmployerReference}={mockedReturn.AccountingDate.Year}"
                + Environment.NewLine
                + $"{publicOrganisationWithSubmissionsToBeDeleted.EmployerReference}={mockedReturn.AccountingDate.Year}";

            // Act
            IActionResult actualManualChangesResult = await testController.ManualChanges(manualChangesViewModelMock);

            // Assert
            Assert.NotNull(actualManualChangesResult, "Expected a Result");

            var manualChangesViewResult = actualManualChangesResult as ViewResult;
            Assert.NotNull(manualChangesViewResult, "Expected ViewResult");

            var actualManualChangesViewModel = manualChangesViewResult.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            Assert.AreEqual("SUCCESSFULLY TESTED 'Delete submissions': 1 of 2", actualManualChangesViewModel.SuccessMessage);
            Assert.AreEqual(
                $"1: EMPLOYERREFERENCE05: Org123 Year='{mockedReturn.AccountingDate.Year}' Status='Submitted' set to 'Deleted'\r\n<span style=\"color:Red\">2: ERROR 'EMPLOYERREFERENCE05' duplicate organisation</span>\r\n",
                actualManualChangesViewModel.Results);
            Assert.AreEqual("Delete submissions", actualManualChangesViewModel.LastTestedCommand);
            Assert.AreEqual(
                $"EmployerReference05={mockedReturn.AccountingDate.Year};EmployerReference05={mockedReturn.AccountingDate.Year}",
                actualManualChangesViewModel.LastTestedInput);
            Assert.True(actualManualChangesViewModel.Tested, "Must be tested=true as this case is running in TEST mode");
            Assert.IsNull(actualManualChangesViewModel.Comment);
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Delete_Submissions_Fails_When_Reference_Is_Not_On_The_Database_Async()
        {
            // Arrange
            User databaseAdminUser = UserHelper.GetDatabaseAdmin();
            var testController = UiTestHelper.GetController<AdminController>(databaseAdminUser.UserId, null, databaseAdminUser);

            var manualChangesViewModelMock = Mock.Of<ManualChangesViewModel>();
            manualChangesViewModelMock.Command = DeleteSubmissionsCommand;
            manualChangesViewModelMock.Parameters = Environment.NewLine
                                                    + "   =" // empty lines must not break the processing
                                                    + Environment.NewLine // null lines must not break the processing 
                                                    + Environment.NewLine // null lines must not break the processing
                                                    + "Reference_Not_On_Database=1999";

            // Act
            IActionResult actualManualChangesResult = await testController.ManualChanges(manualChangesViewModelMock);

            // Assert
            Assert.NotNull(actualManualChangesResult, "Expected a Result");

            var manualChangesViewResult = actualManualChangesResult as ViewResult;
            Assert.NotNull(manualChangesViewResult, "Expected ViewResult");

            var actualManualChangesViewModel = manualChangesViewResult.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            Assert.Multiple(
                () => {
                    Assert.AreEqual("SUCCESSFULLY TESTED 'Delete submissions': 0 of 2", actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual(
                        "<span style=\"color:Red\">2: ERROR: 'REFERENCE_NOT_ON_DATABASE' Cannot find organisation with this employer reference</span>\r\n",
                        actualManualChangesViewModel.Results);
                    Assert.AreEqual("Delete submissions", actualManualChangesViewModel.LastTestedCommand);
                    Assert.AreEqual(";   =;;Reference_Not_On_Database=1999", actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be tested=true as this case is running in TEST mode");
                    Assert.IsNull(actualManualChangesViewModel.Comment);
                });
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Delete_Submissions_Fails_When_Year_Parameter_Is_Not_Valid_Async()
        {
            // Arrange
            User databaseAdminUser = UserHelper.GetDatabaseAdmin();
            Organisation publicOrganisationWithSubmissions =
                OrganisationHelper.GetPublicOrganisation("EmployerReference656262");
            Return mockedReturn = ReturnHelper.CreateTestReturn(publicOrganisationWithSubmissions, DateTime.Now.AddYears(-1).Year);
            var testController = UiTestHelper.GetController<AdminController>(
                databaseAdminUser.UserId,
                null,
                databaseAdminUser,
                publicOrganisationWithSubmissions,
                mockedReturn);

            var manualChangesViewModelMock = Mock.Of<ManualChangesViewModel>();
            manualChangesViewModelMock.Command = DeleteSubmissionsCommand;
            manualChangesViewModelMock.Parameters = "EmployerReference656262=Invalid_Year_Parameter";

            // Act
            IActionResult actualManualChangesResult = await testController.ManualChanges(manualChangesViewModelMock);

            // Assert
            Assert.NotNull(actualManualChangesResult, "Expected a Result");

            var manualChangesViewResult = actualManualChangesResult as ViewResult;
            Assert.NotNull(manualChangesViewResult, "Expected ViewResult");

            var actualManualChangesViewModel = manualChangesViewResult.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            Assert.Multiple(
                () => {
                    Assert.AreEqual("SUCCESSFULLY TESTED 'Delete submissions': 0 of 1", actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual(
                        "<span style=\"color:Red\">1: ERROR: 'EMPLOYERREFERENCE656262' invalid year 'Invalid_Year_Parameter'</span>\r\n",
                        actualManualChangesViewModel.Results);
                    Assert.AreEqual("Delete submissions", actualManualChangesViewModel.LastTestedCommand);
                    Assert.AreEqual("EmployerReference656262=Invalid_Year_Parameter", actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be tested=true as this case is running in TEST mode");
                    Assert.IsNull(actualManualChangesViewModel.Comment);
                });
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Delete_Submissions_Fails_When_Return_Is_Not_On_The_Database_Async()
        {
            // Arrange
            int yearToTest = DateTime.Now.AddYears(-1).Year;
            User databaseAdminUser = UserHelper.GetDatabaseAdmin();
            Organisation publicOrganisationWithSubmissionsToBeDeleted =
                OrganisationHelper.GetPublicOrganisation("EmployerReference99778");

            var testController = UiTestHelper.GetController<AdminController>(
                databaseAdminUser.UserId,
                null,
                databaseAdminUser,
                publicOrganisationWithSubmissionsToBeDeleted);

            var manualChangesViewModelMock = Mock.Of<ManualChangesViewModel>();
            manualChangesViewModelMock.Command = DeleteSubmissionsCommand;
            manualChangesViewModelMock.Parameters = $"{publicOrganisationWithSubmissionsToBeDeleted.EmployerReference}={yearToTest}";

            // Act
            IActionResult actualManualChangesResult = await testController.ManualChanges(manualChangesViewModelMock);

            // Assert
            Assert.NotNull(actualManualChangesResult, "Expected a Result");

            var manualChangesViewResult = actualManualChangesResult as ViewResult;
            Assert.NotNull(manualChangesViewResult, "Expected ViewResult");

            var actualManualChangesViewModel = manualChangesViewResult.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            Assert.AreEqual("SUCCESSFULLY TESTED 'Delete submissions': 0 of 1", actualManualChangesViewModel.SuccessMessage);
            Assert.AreEqual(
                $"<span style=\"color:Orange\">1: WARNING: 'EMPLOYERREFERENCE99778' Cannot find submitted data for year {yearToTest}</span>\r\n",
                actualManualChangesViewModel.Results);
            Assert.AreEqual("Delete submissions", actualManualChangesViewModel.LastTestedCommand);
            Assert.AreEqual($"EmployerReference99778={yearToTest}", actualManualChangesViewModel.LastTestedInput);
            Assert.True(actualManualChangesViewModel.Tested, "Must be tested=true as this case is running in TEST mode");
            Assert.IsNull(actualManualChangesViewModel.Comment);
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Delete_Submissions_Succeeds_Changing_Submitted_To_Deleted_One_Return_Async()
        {
            // Arrange
            User databaseAdminUser = UserHelper.GetDatabaseAdmin();
            Organisation publicOrganisationWithSubmissionsToBeDeleted =
                OrganisationHelper.GetPublicOrganisation("EmployerReference77775");
            Return mockedReturn = ReturnHelper.CreateTestReturn(
                publicOrganisationWithSubmissionsToBeDeleted,
                DateTime.Now.AddYears(-1).Year);
            mockedReturn.Status = ReturnStatuses.Submitted;
            publicOrganisationWithSubmissionsToBeDeleted.LatestReturn = mockedReturn;
            mockedReturn.Organisation = publicOrganisationWithSubmissionsToBeDeleted;

            var testController = UiTestHelper.GetController<AdminController>(
                databaseAdminUser.UserId,
                null,
                databaseAdminUser,
                publicOrganisationWithSubmissionsToBeDeleted,
                mockedReturn);

            var manualChangesViewModelMock = Mock.Of<ManualChangesViewModel>();
            manualChangesViewModelMock.Command = DeleteSubmissionsCommand;
            manualChangesViewModelMock.Parameters =
                $"{publicOrganisationWithSubmissionsToBeDeleted.EmployerReference}={mockedReturn.AccountingDate.Year}";

            // Act
            IActionResult actualManualChangesResult = await testController.ManualChanges(manualChangesViewModelMock);

            // Assert
            Assert.NotNull(actualManualChangesResult, "Expected a Result");

            var manualChangesViewResult = actualManualChangesResult as ViewResult;
            Assert.NotNull(manualChangesViewResult, "Expected ViewResult");

            var actualManualChangesViewModel = manualChangesViewResult.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            Assert.AreEqual("SUCCESSFULLY TESTED 'Delete submissions': 1 of 1", actualManualChangesViewModel.SuccessMessage);
            Assert.AreEqual(
                $"1: EMPLOYERREFERENCE77775: Org123 Year='{mockedReturn.AccountingDate.Year}' Status='Submitted' set to 'Deleted'\r\n",
                actualManualChangesViewModel.Results);
            Assert.AreEqual("Delete submissions", actualManualChangesViewModel.LastTestedCommand);
            Assert.AreEqual($"EmployerReference77775={mockedReturn.AccountingDate.Year}", actualManualChangesViewModel.LastTestedInput);
            Assert.True(actualManualChangesViewModel.Tested, "Must be tested=true as this case is running in TEST mode");
            Assert.IsNull(actualManualChangesViewModel.Comment);
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Delete_Submissions_Succeeds_Changing_Submitted_To_Deleted_Many_Returns_Async()
        {
            // Arrange
            User databaseAdminUser = UserHelper.GetDatabaseAdmin();
            Organisation publicOrganisationWithSubmissionsToBeDeleted =
                OrganisationHelper.GetPublicOrganisation("EmployerReference549549");
            Return mockedReturn = ReturnHelper.CreateTestReturn(
                publicOrganisationWithSubmissionsToBeDeleted,
                DateTime.Now.AddYears(-1).Year);
            mockedReturn.ReturnId = new Random().Next(10000, 99999);
            publicOrganisationWithSubmissionsToBeDeleted.LatestReturn = mockedReturn;
            publicOrganisationWithSubmissionsToBeDeleted.Returns.Add(mockedReturn);
            mockedReturn.Organisation = publicOrganisationWithSubmissionsToBeDeleted;

            Return additionalMockedReturn = ReturnHelper.CreateTestReturn(
                publicOrganisationWithSubmissionsToBeDeleted,
                DateTime.Now.AddYears(-1).Year);
            additionalMockedReturn.ReturnId = new Random().Next(10000, 99999);
            additionalMockedReturn.StatusDate = additionalMockedReturn.StatusDate.AddDays(-3);
            publicOrganisationWithSubmissionsToBeDeleted.Returns.Add(additionalMockedReturn); // This will be found by search on line 1051

            var testController = UiTestHelper.GetController<AdminController>(
                databaseAdminUser.UserId,
                null,
                databaseAdminUser,
                publicOrganisationWithSubmissionsToBeDeleted,
                mockedReturn,
                additionalMockedReturn);

            var manualChangesViewModelMock = Mock.Of<ManualChangesViewModel>();
            manualChangesViewModelMock.Command = DeleteSubmissionsCommand;
            manualChangesViewModelMock.Parameters =
                $"{publicOrganisationWithSubmissionsToBeDeleted.EmployerReference}={mockedReturn.AccountingDate.Year}";

            // Act
            IActionResult actualManualChangesResult = await testController.ManualChanges(manualChangesViewModelMock);

            // Assert
            Assert.NotNull(actualManualChangesResult, "Expected a Result");

            var manualChangesViewResult = actualManualChangesResult as ViewResult;
            Assert.NotNull(manualChangesViewResult, "Expected ViewResult");

            var actualManualChangesViewModel = manualChangesViewResult.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            Assert.AreEqual("SUCCESSFULLY TESTED 'Delete submissions': 1 of 1", actualManualChangesViewModel.SuccessMessage);
            Assert.AreEqual(
                $"1: EMPLOYERREFERENCE549549: Org123 Year='{mockedReturn.AccountingDate.Year}' Status='Submitted' set to 'Deleted'\r\n",
                actualManualChangesViewModel.Results);
            Assert.AreEqual("Delete submissions", actualManualChangesViewModel.LastTestedCommand);
            Assert.AreEqual($"EmployerReference549549={mockedReturn.AccountingDate.Year}", actualManualChangesViewModel.LastTestedInput);
            Assert.True(actualManualChangesViewModel.Tested, "Must be tested=true as this case is running in TEST mode");
            Assert.IsNull(actualManualChangesViewModel.Comment);
        }

    }
}

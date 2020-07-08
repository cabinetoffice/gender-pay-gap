using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Database;
using GenderPayGap.Tests.TestHelpers;
using GenderPayGap.WebUI.Controllers.Administration;
using GenderPayGap.WebUI.Models.Admin;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Controllers.Administration
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class AdminControllerManualChangesGenericTests
    {

        [Test]
        public void AdminController_ManualChanges_GET_When_User_Is_A_Database_Admin_Returns_Empty_ManualChangesViewModel()
        {
            // Arrange
            User databaseAdminUser = UserHelper.GetDatabaseAdmin();
            var adminController = UiTestHelper.GetController<AdminController>(databaseAdminUser.UserId, null, databaseAdminUser);

            // Act
            IActionResult manualChangesResult = adminController.ManualChanges();
            Assert.NotNull(manualChangesResult);

            // Assert
            var manualChangesViewResult = manualChangesResult as ViewResult;
            Assert.NotNull(manualChangesViewResult, "Expected ViewResult");

            var actualManualChangesViewModel = manualChangesViewResult.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_When_Command_Was_Not_Selected_Returns_Error_Async()
        {
            // Arrange
            User notAdminUser = UserHelper.GetDatabaseAdmin();
            var adminController = UiTestHelper.GetController<AdminController>(notAdminUser.UserId, null, notAdminUser);

            ManualChangesViewModel manualChangesViewModelMockObject = ManualChangesViewModelHelper.GetMock("Please select..", string.Empty);

            // Act
            IActionResult manualChangesResult = await adminController.ManualChanges(manualChangesViewModelMockObject);
            Assert.NotNull(manualChangesResult);

            var manualChangesViewResult = manualChangesResult as ViewResult;
            Assert.NotNull(manualChangesViewResult, "Expected ViewResult");

            var actualManualChangesViewModel = manualChangesViewResult.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            ModelStateEntry modelState = adminController.ModelState[""];
            ModelError reportedError = modelState.Errors.First();

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual("ERROR: You must first select a command", reportedError.ErrorMessage);
                    Assert.IsNull(actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual("", actualManualChangesViewModel.Results);
                    Assert.IsNull(actualManualChangesViewModel.LastTestedCommand);
                    Assert.IsNull(actualManualChangesViewModel.LastTestedInput);
                    Assert.IsFalse(actualManualChangesViewModel.Tested);
                    Assert.IsNull(actualManualChangesViewModel.Comment);
                });
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_When_Command_Was_Not_Implemented_Returns_Error_Async()
        {
            // Arrange
            User notAdminUser = UserHelper.GetDatabaseAdmin();
            var adminController = UiTestHelper.GetController<AdminController>(notAdminUser.UserId, null, notAdminUser);

            ManualChangesViewModel manualChangesViewModelMockObject =
                ManualChangesViewModelHelper.GetMock("Some undefined command", string.Empty);

            // Act
            IActionResult manualChangesResult = await adminController.ManualChanges(manualChangesViewModelMockObject);
            Assert.NotNull(manualChangesResult);

            var manualChangesViewResult = manualChangesResult as ViewResult;
            Assert.NotNull(manualChangesViewResult, "Expected ViewResult");

            var actualManualChangesViewModel = manualChangesViewResult.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            ModelStateEntry modelState = adminController.ModelState[""];
            ModelError reportedError = modelState.Errors.First();

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual("ERROR: The command 'Some undefined command' has not yet been implemented", reportedError.ErrorMessage);
                    Assert.IsNull(actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual("", actualManualChangesViewModel.Results);
                    Assert.IsNull(actualManualChangesViewModel.LastTestedCommand);
                    Assert.IsNull(actualManualChangesViewModel.LastTestedInput);
                    Assert.IsFalse(actualManualChangesViewModel.Tested);
                    Assert.IsNull(actualManualChangesViewModel.Comment);
                });
        }
    }
}

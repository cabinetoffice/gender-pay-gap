using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Tests.Common.TestHelpers;
using GenderPayGap.Tests.TestHelpers;
using GenderPayGap.WebUI.Classes.Services;
using GenderPayGap.WebUI.Controllers.Administration;
using GenderPayGap.WebUI.Models.Admin;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Controllers.Administration
{
    /// <summary>
    ///     This class contains tests to assert the correct functioning of calls to the 'Manual changes' method that belong to
    ///     a Pure website-administrator role (such as recreate a search index, or fix database errors).
    ///     Other calls like 'change the status of an organisation' or 'expire a submission' are NOT expected to be here
    /// </summary>
    [TestFixture]
    public class AdminControllerManualChangesPureAdminFunctionTests
    {

        private const string UpdateSearchIndexesCommand = "Update search indexes";

        #region "Update search indexes"

        [Test]
        public async Task AdminController_ManualChanges_POST_Update_Search_Indexes_Works_When_Run_In_Test_Mode_Async()
        {
            // Arrange
            User databaseAdmin = UserHelper.GetDatabaseAdmin();
            var adminController = UiTestHelper.GetController<AdminController>(databaseAdmin.UserId, null, null);

            Mock<IDataRepository> configurableDataRepository = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            configurableDataRepository
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(databaseAdmin);

            Mock<IAdminService> configurableAdmin = AutoFacExtensions.ResolveAsMock<IAdminService>();

            configurableAdmin
                .Setup(x => x.GetSearchDocumentCountAsync())
                .ReturnsAsync(65488L);


            var manualChangesViewModel = new ManualChangesViewModel {Command = UpdateSearchIndexesCommand};

            // Act
            IActionResult manualChangesResult = await adminController.ManualChanges(manualChangesViewModel);

            // Assert
            Assert.NotNull(manualChangesResult, "Expected a Result");

            var manualChangesViewResult = manualChangesResult as ViewResult;
            Assert.NotNull(manualChangesViewResult, "Expected ViewResult");

            var actualManualChangesViewModel = manualChangesViewResult.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            Assert.Multiple(
                () => {
                    Assert.AreEqual(
                        "SUCCESSFULLY TESTED 'Update search indexes': 65488 items",
                        actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual("", actualManualChangesViewModel.Results);
                    Assert.AreEqual("Update search indexes", actualManualChangesViewModel.LastTestedCommand);
                    Assert.IsNull(actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be tested=true as this case is running in TEST mode");
                    Assert.IsNull(actualManualChangesViewModel.Comment);
                });
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Update_Search_Indexes_Works_When_Run_In_Live_Mode_Async()
        {
            // Arrange
            User databaseAdmin = UserHelper.GetDatabaseAdmin();
            var adminController = UiTestHelper.GetController<AdminController>(databaseAdmin.UserId, null, null);

            Mock<IDataRepository> configurableDataRepository = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            configurableDataRepository
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(databaseAdmin);

            Mock<IAdminService> configurableAdmin = AutoFacExtensions.ResolveAsMock<IAdminService>();

            configurableAdmin
                .Setup(x => x.GetSearchDocumentCountAsync())
                .ReturnsAsync(12545L);


            var manualChangesViewModel = new ManualChangesViewModel {Command = UpdateSearchIndexesCommand};

            /* live */
            manualChangesViewModel.LastTestedCommand = manualChangesViewModel.Command;
            manualChangesViewModel.LastTestedInput = null;

            // Act
            IActionResult manualChangesResult = await adminController.ManualChanges(manualChangesViewModel);

            // Assert
            Assert.NotNull(manualChangesResult, "Expected a Result");

            var manualChangesViewResult = manualChangesResult as ViewResult;
            Assert.NotNull(manualChangesViewResult, "Expected ViewResult");

            var actualManualChangesViewModel = manualChangesViewResult.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            Assert.Multiple(
                () => {
                    Assert.AreEqual(
                        "SUCCESSFULLY EXECUTED 'Update search indexes': 12545 items",
                        actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual(
                        "An email will be sent to 'databaseadmin@email.com' when the background task 'UpdateSearchIndexesAsync' has completed\r\n",
                        actualManualChangesViewModel.Results);
                    Assert.IsNull(actualManualChangesViewModel.LastTestedCommand);
                    Assert.IsNull(actualManualChangesViewModel.LastTestedInput);
                    Assert.False(actualManualChangesViewModel.Tested, "Must be tested=false as this case is running in LIVE mode");
                    Assert.IsNull(actualManualChangesViewModel.Comment);
                });
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Update_Search_Indexes_Fails_When_Parameters_Are_Provided_Async()
        {
            // Arrange
            User databaseAdmin = UserHelper.GetDatabaseAdmin();
            var adminController = UiTestHelper.GetController<AdminController>(databaseAdmin.UserId, null, databaseAdmin);
            var manualChangesViewModel = Mock.Of<ManualChangesViewModel>();
            manualChangesViewModel.Command = UpdateSearchIndexesCommand;
            manualChangesViewModel.Parameters = "not empty";

            // Act
            var retireOrgActionResult = await adminController.ManualChanges(manualChangesViewModel) as ViewResult;
            Assert.NotNull(retireOrgActionResult);
            var actualManualChangesViewModel = (ManualChangesViewModel) retireOrgActionResult.Model;

            ModelStateEntry modelState = adminController.ModelState[""];
            ModelError reportedError = modelState.Errors.First();

            // Assert
            Assert.AreEqual("ERROR: parameters must be empty", reportedError.ErrorMessage);
            Assert.IsNull(actualManualChangesViewModel.SuccessMessage);
            Assert.AreEqual("", actualManualChangesViewModel.Results);
            Assert.IsNull(actualManualChangesViewModel.LastTestedCommand);
            Assert.IsNull(actualManualChangesViewModel.LastTestedInput);
            Assert.False(actualManualChangesViewModel.Tested, "Must be false as this case has failed");
        }

        #endregion

    }
}

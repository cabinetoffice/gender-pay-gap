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
        private const string UpdateGPGDownloadDataFilesCommand = "Update GPG download data files";
        private const string FixDatabaseErrorsCommand = "Fix database errors";

        #region "Update GPG download data files"

        [Test]
        public async Task AdminController_ManualChanges_POST_Update_GPG_Download_Data_Files_Works_When_Run_In_Test_Mode_Async()
        {
            // Arrange
            User notAdminUser = UserHelper.GetDatabaseAdmin();
            var adminController = UiTestHelper.GetController<AdminController>(notAdminUser.UserId, null, null);

            Mock<IDataRepository> configurableDataRepository = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            configurableDataRepository
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(notAdminUser);

            Mock<IAdminService> configurableAdmin = AutoFacExtensions.ResolveAsMock<IAdminService>();

            configurableAdmin
                .Setup(x => x.GetSearchDocumentCountAsync())
                .ReturnsAsync(21212L);


            var manualChangesViewModel = new ManualChangesViewModel {Command = UpdateGPGDownloadDataFilesCommand};

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
                        "SUCCESSFULLY TESTED 'Update GPG download data files': 21212 items",
                        actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual("", actualManualChangesViewModel.Results);
                    Assert.AreEqual("Update GPG download data files", actualManualChangesViewModel.LastTestedCommand);
                    Assert.IsNull(actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be tested=true as this case is running in TEST mode");
                    Assert.IsNull(actualManualChangesViewModel.Comment);
                });
        }

        #endregion

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

        #region "Fix database errors"

        [Test]
        public async Task AdminController_ManualChanges_POST_Fix_Database_Errors_Fails_When_Parameters_Are_Provided_Async()
        {
            // Arrange
            User databaseAdminUser = UserHelper.GetDatabaseAdmin();
            var adminController = UiTestHelper.GetController<AdminController>(databaseAdminUser.UserId, null, databaseAdminUser);

            var manualChangesViewModel = Mock.Of<ManualChangesViewModel>();
            manualChangesViewModel.Command = FixDatabaseErrorsCommand;
            manualChangesViewModel.Parameters = "not empty";

            // Act
            var fixDatabaseErrorsActualResult = await adminController.ManualChanges(manualChangesViewModel) as ViewResult;
            Assert.NotNull(fixDatabaseErrorsActualResult);
            var actualManualChangesViewModel = (ManualChangesViewModel) fixDatabaseErrorsActualResult.Model;

            ModelStateEntry modelState = adminController.ModelState[""];
            ModelError reportedError = modelState.Errors.First();

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual("ERROR: parameters must be empty", reportedError.ErrorMessage);
                    Assert.IsNull(actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual("", actualManualChangesViewModel.Results);
                    Assert.IsNull(actualManualChangesViewModel.LastTestedCommand);
                    Assert.IsNull(actualManualChangesViewModel.LastTestedInput);
                    Assert.False(actualManualChangesViewModel.Tested, "Must be false as this case has failed");
                });
        }

        [Test]
        public async Task
            AdminController_ManualChanges_POST_Fix_Database_Error_Missing_Latest_Registration_Works_When_Run_In_Test_Mode_Async()
        {
            // Arrange
            User notAdminUser = UserHelper.GetDatabaseAdmin();
            var adminController = UiTestHelper.GetController<AdminController>(notAdminUser.UserId, null, null);

            #region Set up organisation missing the latest registration link so it's picked up by the test

            Organisation organisationMissingLatestRegistration = OrganisationHelper.GetPrivateOrganisation("EmployerReference564");
            UserOrganisationHelper.LinkUserWithOrganisation(
                notAdminUser,
                organisationMissingLatestRegistration); // user registered link indeed exists for this organisation
            organisationMissingLatestRegistration.LatestRegistration = null; // missing latest registration -link-

            #endregion

            Mock<IDataRepository> configurableDataRepository = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            configurableDataRepository
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(notAdminUser);

            configurableDataRepository
                .Setup(x => x.GetAll<Organisation>())
                .Returns(new[] {organisationMissingLatestRegistration}.AsQueryable());

            var manualChangesViewModel = new ManualChangesViewModel {Command = FixDatabaseErrorsCommand};

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
                    Assert.AreEqual("SUCCESSFULLY TESTED 'Fix database errors': 1 items", actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual(
                        "001: Organisation 'EmployerReference564:Org123' missing a latest registration will be fixed\r\nNo organisations missing a latest scope\r\n",
                        actualManualChangesViewModel.Results);
                    Assert.AreEqual("Fix database errors", actualManualChangesViewModel.LastTestedCommand);
                    Assert.IsNull(actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be tested=true as this case is running in TEST mode");
                    Assert.IsNull(actualManualChangesViewModel.Comment);
                });
        }

        [Test]
        public async Task
            AdminController_ManualChanges_POST_Fix_Database_Error_Missing_Latest_Registration_Works_When_Run_In_Live_Mode_Async()
        {
            // Arrange
            User notAdminUser = UserHelper.GetDatabaseAdmin();
            var adminController = UiTestHelper.GetController<AdminController>(notAdminUser.UserId, null, null);

            #region Set up organisation missing the latest registration link so it's picked up by the test

            Organisation organisationMissingLatestRegistration = OrganisationHelper.GetPrivateOrganisation("EmployerReference96585");
            UserOrganisationHelper.LinkUserWithOrganisation(
                notAdminUser,
                organisationMissingLatestRegistration); // user registered link indeed exists for this organisation
            organisationMissingLatestRegistration.LatestRegistration = null; // missing latest registration -link-

            #endregion

            Mock<IDataRepository> configurableDataRepository = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            configurableDataRepository
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(notAdminUser);

            configurableDataRepository
                .Setup(x => x.GetAll<Organisation>())
                .Returns(new[] {organisationMissingLatestRegistration}.AsQueryable());

            var manualChangesViewModel = new ManualChangesViewModel {Command = FixDatabaseErrorsCommand};

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
                    Assert.AreEqual("SUCCESSFULLY EXECUTED 'Fix database errors': 1 items", actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual(
                        "001: Organisation 'EmployerReference96585:Org123' missing a latest registration was successfully fixed\r\nNo organisations missing a latest scope\r\n",
                        actualManualChangesViewModel.Results);
                    Assert.IsNull(actualManualChangesViewModel.LastTestedCommand);
                    Assert.IsNull(actualManualChangesViewModel.LastTestedInput);
                    Assert.False(actualManualChangesViewModel.Tested, "Must be tested=false as this case is running in LIVE mode");
                    Assert.IsNull(actualManualChangesViewModel.Comment);
                });
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Fix_Database_Error_Missing_Latest_Scope_Works_When_Run_In_Test_Mode_Async()
        {
            // Arrange
            User notAdminUser = UserHelper.GetDatabaseAdmin();
            var adminController = UiTestHelper.GetController<AdminController>(notAdminUser.UserId, null, null);

            #region Set up organisation missing the latest scope link so it's picked up by the test

            Organisation organisation_MissingLatestScope = OrganisationHelper.GetPrivateOrganisation("EmployerReference444");

            UserOrganisation userOrganisation_MissingLatestScope =
                UserOrganisationHelper.LinkUserWithOrganisation(notAdminUser, organisation_MissingLatestScope);
            organisation_MissingLatestScope.LatestRegistration = userOrganisation_MissingLatestScope;

            Return return_MissingLatestScope = ReturnHelper.GetSubmittedReturnForOrganisationAndYear(
                userOrganisation_MissingLatestScope,
                VirtualDateTime.Now.AddYears(-1).Year);
            organisation_MissingLatestScope.Returns.Add(return_MissingLatestScope);

            OrganisationScope scope_MissingLatestScope = ScopeHelper.CreateScope(ScopeStatuses.InScope, VirtualDateTime.Now.AddYears(-1));
            organisation_MissingLatestScope.OrganisationScopes.Add(
                scope_MissingLatestScope); // latest scope indeed exists for this organisation
            organisation_MissingLatestScope.LatestScope = null; // missing latest scope -link-

            #endregion

            Mock<IDataRepository> configurableDataRepository = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            configurableDataRepository
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(notAdminUser);

            configurableDataRepository
                .Setup(x => x.GetAll<Organisation>())
                .Returns(new[] {organisation_MissingLatestScope}.AsQueryable());

            var manualChangesViewModel = new ManualChangesViewModel {Command = FixDatabaseErrorsCommand};

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
                    Assert.AreEqual("SUCCESSFULLY TESTED 'Fix database errors': 1 items", actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual(
                        "No organisations missing a latest registration\r\n001: Organisation 'EmployerReference444:Org123' missing a latest scope will be fixed\r\n",
                        actualManualChangesViewModel.Results);
                    Assert.AreEqual("Fix database errors", actualManualChangesViewModel.LastTestedCommand);
                    Assert.IsNull(actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be tested=true as this case is running in TEST mode");
                    Assert.IsNull(actualManualChangesViewModel.Comment);
                });
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Fix_Database_Error_Missing_Latest_Scope_Works_When_Run_In_Live_Mode_Async()
        {
            // Arrange
            User notAdminUser = UserHelper.GetDatabaseAdmin();
            var adminController = UiTestHelper.GetController<AdminController>(notAdminUser.UserId, null, null);

            #region Set up organisation missing the latest scope link so it's picked up by the test

            Organisation organisation_MissingLatestScope = OrganisationHelper.GetPrivateOrganisation("EmployerReference5487548");

            UserOrganisation userOrganisation_MissingLatestScope =
                UserOrganisationHelper.LinkUserWithOrganisation(notAdminUser, organisation_MissingLatestScope);
            organisation_MissingLatestScope.LatestRegistration = userOrganisation_MissingLatestScope;

            Return return_MissingLatestScope = ReturnHelper.GetSubmittedReturnForOrganisationAndYear(
                userOrganisation_MissingLatestScope,
                VirtualDateTime.Now.AddYears(-1).Year);
            organisation_MissingLatestScope.Returns.Add(return_MissingLatestScope);

            OrganisationScope scope_MissingLatestScope = ScopeHelper.CreateScope(ScopeStatuses.InScope, VirtualDateTime.Now.AddYears(-1));
            organisation_MissingLatestScope.OrganisationScopes.Add(
                scope_MissingLatestScope); // latest scope indeed exists for this organisation
            organisation_MissingLatestScope.LatestScope = null; // missing latest scope -link-

            #endregion

            Mock<IDataRepository> configurableDataRepository = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            configurableDataRepository
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(notAdminUser);

            configurableDataRepository
                .Setup(x => x.GetAll<Organisation>())
                .Returns(new[] {organisation_MissingLatestScope}.AsQueryable());

            var manualChangesViewModel = new ManualChangesViewModel {Command = FixDatabaseErrorsCommand};

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

            Assert.AreEqual("SUCCESSFULLY EXECUTED 'Fix database errors': 1 items", actualManualChangesViewModel.SuccessMessage);
            var expectedManualChangesViewModelResults =
                "No organisations missing a latest registration\r\n001: Organisation 'EmployerReference5487548:Org123' missing a latest scope was successfully fixed\r\n";
            Assert.AreEqual(
                expectedManualChangesViewModelResults,
                actualManualChangesViewModel.Results,
                $"EXPECTED -{expectedManualChangesViewModelResults}- BUT WAS -{actualManualChangesViewModel.Results}-");
            Assert.IsNull(actualManualChangesViewModel.LastTestedCommand);
            Assert.IsNull(actualManualChangesViewModel.LastTestedInput);
            Assert.False(actualManualChangesViewModel.Tested, "Must be tested=false as this case is running in LIVE mode");
            Assert.IsNull(actualManualChangesViewModel.Comment);
        }

        #endregion

    }
}

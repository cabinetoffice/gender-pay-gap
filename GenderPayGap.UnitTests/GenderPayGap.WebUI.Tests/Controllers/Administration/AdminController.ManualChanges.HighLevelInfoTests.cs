using System;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Tests.Common.TestHelpers;
using GenderPayGap.Tests.TestHelpers;
using GenderPayGap.WebUI.Controllers.Administration;
using GenderPayGap.WebUI.Models.Admin;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Controllers.Administration
{
    [TestFixture]
    public class AdminControllerManualChangesHighLevelInfoTests
    {
        private const string AddOrganisationsLatestNameCommand = "Add organisations latest name";
        private const string ResetOrganisationToOnlyOriginalNameCommand = "Reset organisation to only original name";
        private const string SetOrganisationCompanyNumberCommand = "Set organisation company number";

        #region Reset organisation to only original name

        [Test]
        public async Task AdminController_ManualChanges_POST_Reset_Organisation_To_Only_Original_Name_Works_When_Run_In_Test_Mode_Async()
        {
            // Arrange
            Organisation organisationToChangeName = OrganisationHelper.GetPrivateOrganisation("EmployerReference02");

            #region setting up database and controller 

            User notAdminUser = UserHelper.GetDatabaseAdmin();
            var adminController = UiTestHelper.GetController<AdminController>(notAdminUser.UserId, null, null);

            Mock<IDataRepository> configurableDataRepository = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            configurableDataRepository
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(notAdminUser);

            configurableDataRepository
                .Setup(x => x.GetAll<Organisation>())
                .Returns(new[] {organisationToChangeName}.AsQueryable().BuildMock().Object);

            var manualChangesViewModel = new ManualChangesViewModel {
                Command = ResetOrganisationToOnlyOriginalNameCommand,
                Parameters = $"{organisationToChangeName.EmployerReference.ToLower()}=New name to reset ltd"
            };

            #endregion

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
                        "SUCCESSFULLY TESTED 'Reset organisation to only original name': 1 of 1",
                        actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual(
                        "1: EMPLOYERREFERENCE02: 'Org123' set to 'New name to reset ltd'\r\n",
                        actualManualChangesViewModel.Results);
                    Assert.AreEqual(ResetOrganisationToOnlyOriginalNameCommand, actualManualChangesViewModel.LastTestedCommand);
                    Assert.AreEqual("employerreference02=New name to reset ltd", actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be tested=true as this case is running in TEST mode");
                    Assert.IsNull(actualManualChangesViewModel.Comment);
                });
        }

        #endregion

        #region Organisation Company number

        [Test]
        public async Task AdminController_ManualChanges_POST_Set_Organisation_Company_Number_Works_When_Run_In_Test_Mode_Async()
        {
            Organisation orgWhoseCompanyNumberWillBeSet = OrganisationHelper.GetPrivateOrganisation("EmployerReference018");

            #region setting up database and controller 

            User databaseAdminUser = UserHelper.GetDatabaseAdmin();
            var adminController = UiTestHelper.GetController<AdminController>(databaseAdminUser.UserId, null, null);

            Mock<IDataRepository> configurableDataRepository = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            configurableDataRepository
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(databaseAdminUser);

            configurableDataRepository
                .Setup(x => x.GetAll<Organisation>())
                .Returns(new[] {orgWhoseCompanyNumberWillBeSet}.AsQueryable().BuildMock().Object);

            var manualChangesViewModel = new ManualChangesViewModel {
                Command = SetOrganisationCompanyNumberCommand, Parameters = $"{orgWhoseCompanyNumberWillBeSet.EmployerReference}=COMPNUM9"
            };

            #endregion

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
                        "SUCCESSFULLY TESTED 'Set organisation company number': 1 of 1",
                        actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual(
                        "1: EMPLOYERREFERENCE018: Org123 Company Number='COMPNUM9' set to 'COMPNUM9'\r\n",
                        actualManualChangesViewModel.Results);
                    Assert.AreEqual("Set organisation company number", actualManualChangesViewModel.LastTestedCommand);
                    Assert.AreEqual("EmployerReference018=COMPNUM9", actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be tested=true as this case is running in TEST mode");
                    Assert.IsNull(actualManualChangesViewModel.Comment);
                });
        }

        #endregion

        #region Add organisations latest name

        [Test]
        public async Task AdminController_ManualChanges_POST_Add_Organisation_Latest_Name_Works_When_Run_In_Test_Mode_Async()
        {
            // Arrange
            Organisation organisationToChangeName = OrganisationHelper.GetPrivateOrganisation("EmployerReference01");

            #region setting up database and controller 

            User notAdminUser = UserHelper.GetDatabaseAdmin();
            var adminController = UiTestHelper.GetController<AdminController>(notAdminUser.UserId, null, null);

            Mock<IDataRepository> configurableDataRepository = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            configurableDataRepository
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(notAdminUser);

            configurableDataRepository
                .Setup(x => x.GetAll<Organisation>())
                .Returns(new[] {organisationToChangeName}.AsQueryable().BuildMock().Object);

            var manualChangesViewModel = new ManualChangesViewModel {
                Command = AddOrganisationsLatestNameCommand,
                Parameters = $"{organisationToChangeName.EmployerReference.ToLower()}=New name ltd"
            };

            #endregion

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
                        "SUCCESSFULLY TESTED 'Add organisations latest name': 1 of 1",
                        actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual("1: EMPLOYERREFERENCE01: 'Org123' set to 'New name ltd'\r\n", actualManualChangesViewModel.Results);
                    Assert.AreEqual(AddOrganisationsLatestNameCommand, actualManualChangesViewModel.LastTestedCommand);
                    Assert.AreEqual("employerreference01=New name ltd", actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be tested=true as this case is running in TEST mode");
                    Assert.IsNull(actualManualChangesViewModel.Comment);
                });
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Add_Organisation_Latest_Name_Works_When_Run_In_Live_Mode_Async()
        {
            // Arrange
            Organisation organisationToChangeName = OrganisationHelper.GetPrivateOrganisation("EmployerReference3335");

            #region setting up database and controller 

            User notAdminUser = UserHelper.GetDatabaseAdmin();
            var adminController = UiTestHelper.GetController<AdminController>(notAdminUser.UserId, null, null);

            Mock<IDataRepository> configurableDataRepository = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            configurableDataRepository
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(notAdminUser);

            configurableDataRepository
                .Setup(x => x.GetAll<Organisation>())
                .Returns(new[] {organisationToChangeName}.AsQueryable().BuildMock().Object);

            var manualChangesViewModel = new ManualChangesViewModel {
                Command = AddOrganisationsLatestNameCommand,
                Parameters = $"{organisationToChangeName.EmployerReference.ToLower()}=New name ltd"
            };

            #endregion

            /* live */
            manualChangesViewModel.LastTestedCommand = manualChangesViewModel.Command;
            manualChangesViewModel.LastTestedInput = manualChangesViewModel.Parameters;

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
                        "SUCCESSFULLY EXECUTED 'Add organisations latest name': 1 of 1",
                        actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual("1: EMPLOYERREFERENCE3335: 'Org123' set to 'New name ltd'\r\n", actualManualChangesViewModel.Results);
                    Assert.IsNull(actualManualChangesViewModel.LastTestedCommand);
                    Assert.IsNull(actualManualChangesViewModel.LastTestedInput);
                    Assert.False(actualManualChangesViewModel.Tested, "Must be tested=false as this case is running in LIVE mode");
                    Assert.IsNull(actualManualChangesViewModel.Comment);
                });
        }

        [TestCase("")]
        [TestCase(null)]
        public async Task AdminController_ManualChanges_POST_Add_Organisation_Latest_Name_Fails_When_No_Parameters_Are_Provided_Async(
            string nullOrEmptyParameter)
        {
            // Arrange
            User databaseAdmin = UserHelper.GetDatabaseAdmin();
            var adminController = UiTestHelper.GetController<AdminController>(databaseAdmin.UserId, null, databaseAdmin);
            var manualChangesViewModel = Mock.Of<ManualChangesViewModel>();
            manualChangesViewModel.Command = AddOrganisationsLatestNameCommand;
            manualChangesViewModel.Parameters = nullOrEmptyParameter; // this test requires null or empty parameters to be provided

            // Act
            var retireOrgActionResult = await adminController.ManualChanges(manualChangesViewModel) as ViewResult;
            Assert.NotNull(retireOrgActionResult);
            var actualManualChangesViewModel = (ManualChangesViewModel) retireOrgActionResult.Model;

            ModelStateEntry modelState = adminController.ModelState[""];
            ModelError reportedError = modelState.Errors.First();

            // Assert
            Assert.AreEqual("ERROR: You must supply 1 or more input parameters", reportedError.ErrorMessage);
            Assert.IsNull(actualManualChangesViewModel.SuccessMessage);
            Assert.AreEqual("", actualManualChangesViewModel.Results);
            Assert.IsNull(actualManualChangesViewModel.LastTestedCommand);
            Assert.IsNull(actualManualChangesViewModel.LastTestedInput);
            Assert.False(actualManualChangesViewModel.Tested, "Must be false as this case has failed");
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Add_Organisation_Latest_Name_Fails_When_An_Equals_Sign_Is_Not_Sent_Async()
        {
            // Arrange
            User databaseAdmin = UserHelper.GetDatabaseAdmin();
            var adminController = UiTestHelper.GetController<AdminController>(databaseAdmin.UserId, null, databaseAdmin);
            var manualChangesViewModelMockObject = Mock.Of<ManualChangesViewModel>();
            manualChangesViewModelMockObject.Command = AddOrganisationsLatestNameCommand;
            manualChangesViewModelMockObject.Parameters = "parameter_without_equals_sign";

            // Act
            IActionResult actualManualChanges = await adminController.ManualChanges(manualChangesViewModelMockObject);
            Assert.NotNull(actualManualChanges);
            var actualManualChangesViewResult = actualManualChanges as ViewResult;
            Assert.NotNull(actualManualChangesViewResult);
            var actualManualChangesViewModel = actualManualChangesViewResult.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel);

            // Assert
            Assert.AreEqual("SUCCESSFULLY TESTED 'Add organisations latest name': 0 of 1", actualManualChangesViewModel.SuccessMessage);
            Assert.AreEqual(
                "<span style=\"color:Red\">1: ERROR: 'parameter_without_equals_sign' does not contain '=' character</span>\r\n",
                actualManualChangesViewModel.Results);
            Assert.AreEqual("Add organisations latest name", actualManualChangesViewModel.LastTestedCommand);
            Assert.AreEqual("parameter_without_equals_sign", actualManualChangesViewModel.LastTestedInput);
            Assert.True(actualManualChangesViewModel.Tested, "Must be true as this case is running in TEST mode");
            Assert.Null(actualManualChangesViewModel.Comment);
        }

        [Test]
        public async Task
            AdminController_ManualChanges_POST_Add_Organisation_Latest_Name_Fails_When_Employer_Reference_Is_Duplicated_Async()
        {
            // Arrange
            Organisation organisationToChangeName = OrganisationHelper.GetPrivateOrganisation("EmployerReference0111");

            #region setting up database and controller 

            User notAdminUser = UserHelper.GetDatabaseAdmin();
            var adminController = UiTestHelper.GetController<AdminController>(notAdminUser.UserId, null, null);

            Mock<IDataRepository> configurableDataRepository = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            configurableDataRepository
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(notAdminUser);

            configurableDataRepository
                .Setup(x => x.GetAll<Organisation>())
                .Returns(new[] {organisationToChangeName}.AsQueryable().BuildMock().Object);

            var manualChangesViewModel = new ManualChangesViewModel {
                Command = AddOrganisationsLatestNameCommand,
                Parameters = $"{organisationToChangeName.EmployerReference.ToLower()}=New name ltd"
                             + Environment.NewLine
                             + $"{organisationToChangeName.EmployerReference.ToLower()}=New name ltd"
            };

            #endregion

            // Act
            IActionResult manualChangesResult = await adminController.ManualChanges(manualChangesViewModel);

            // Assert
            Assert.NotNull(manualChangesResult, "Expected a Result");

            var manualChangesViewResult = manualChangesResult as ViewResult;
            Assert.NotNull(manualChangesViewResult, "Expected ViewResult");

            var actualManualChangesViewModel = manualChangesViewResult.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            Assert.AreEqual("SUCCESSFULLY TESTED 'Add organisations latest name': 1 of 2", actualManualChangesViewModel.SuccessMessage);
            Assert.AreEqual(
                "1: EMPLOYERREFERENCE0111: 'Org123' set to 'New name ltd'\r\n<span style=\"color:Red\">2: ERROR: 'EMPLOYERREFERENCE0111' duplicate organisation</span>\r\n",
                actualManualChangesViewModel.Results);
            Assert.AreEqual("Add organisations latest name", actualManualChangesViewModel.LastTestedCommand);
            Assert.AreEqual(
                "employerreference0111=New name ltd;employerreference0111=New name ltd",
                actualManualChangesViewModel.LastTestedInput);
            Assert.True(actualManualChangesViewModel.Tested, "Must be tested=true as this case is running in TEST mode");
            Assert.IsNull(actualManualChangesViewModel.Comment);
        }

        [Test]
        public async Task
            AdminController_ManualChanges_POST_Add_Organisation_Latest_Name_Fails_When_Reference_Is_Not_On_The_Database_Async()
        {
            // Arrange
            User notAdminUser = UserHelper.GetDatabaseAdmin();
            var adminController = UiTestHelper.GetController<AdminController>(notAdminUser.UserId, null, null);

            Mock<IDataRepository> configurableDataRepository = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            configurableDataRepository
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(notAdminUser);

            var manualChangesViewModel = new ManualChangesViewModel {
                Command = AddOrganisationsLatestNameCommand,
                Parameters = Environment.NewLine
                             + "   =" // empty lines must not break the processing
                             + Environment.NewLine // null lines must not break the processing 
                             + Environment.NewLine // null lines must not break the processing
                             + "Reference_Not_On_Database=Missing_Reference"
            };

            // Act
            var retireOrgActionResult = await adminController.ManualChanges(manualChangesViewModel) as ViewResult;
            Assert.NotNull(retireOrgActionResult);
            var actualManualChangesViewModel = (ManualChangesViewModel) retireOrgActionResult.Model;

            // Assert
            Assert.AreEqual("SUCCESSFULLY TESTED 'Add organisations latest name': 0 of 2", actualManualChangesViewModel.SuccessMessage);
            Assert.AreEqual(
                "<span style=\"color:Red\">2: ERROR: 'REFERENCE_NOT_ON_DATABASE' Cannot find organisation with this employer reference</span>\r\n",
                actualManualChangesViewModel.Results); // Note "2: ERROR" instead of "1: ERROR" here
            Assert.AreEqual("Add organisations latest name", actualManualChangesViewModel.LastTestedCommand);
            Assert.AreEqual(";   =;;Reference_Not_On_Database=Missing_Reference", actualManualChangesViewModel.LastTestedInput);
            Assert.True(actualManualChangesViewModel.Tested, "Must be true as this case is running in test mode");
        }

        [Test]
        public async Task
            AdminController_ManualChanges_POST_Add_Organisation_Latest_Name_Fails_When_Organisation_Name_Is_Not_Specified_Async()
        {
            // Arrange
            User notAdminUser = UserHelper.GetDatabaseAdmin();
            var adminController = UiTestHelper.GetController<AdminController>(notAdminUser.UserId, null, null);

            Mock<IDataRepository> configurableDataRepository = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            configurableDataRepository
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(notAdminUser);

            var manualChangesViewModel = new ManualChangesViewModel {
                Command = AddOrganisationsLatestNameCommand, Parameters = "OrgNameNotSpecified="
            };

            // Act
            var retireOrgActionResult = await adminController.ManualChanges(manualChangesViewModel) as ViewResult;
            Assert.NotNull(retireOrgActionResult);
            var actualManualChangesViewModel = (ManualChangesViewModel) retireOrgActionResult.Model;

            // Assert
            Assert.AreEqual("SUCCESSFULLY TESTED 'Add organisations latest name': 0 of 1", actualManualChangesViewModel.SuccessMessage);
            Assert.AreEqual(
                "<span style=\"color:Red\">1: ERROR: 'ORGNAMENOTSPECIFIED' No organisation name specified</span>\r\n",
                actualManualChangesViewModel.Results);
            Assert.AreEqual("Add organisations latest name", actualManualChangesViewModel.LastTestedCommand);
            Assert.AreEqual("OrgNameNotSpecified=", actualManualChangesViewModel.LastTestedInput);
            Assert.True(actualManualChangesViewModel.Tested, "Must be true as this case is running in test mode");
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Add_Organisation_Latest_Name_Fails_When_The_New_Name_Was_Already_Set_Async()
        {
            // Arrange
            Organisation organisationToChangeName = OrganisationHelper.GetPrivateOrganisation("EmployerReference373737");
            organisationToChangeName.OrganisationName = "New name limited";

            #region setting up database and controller 

            User notAdminUser = UserHelper.GetDatabaseAdmin();
            var adminController = UiTestHelper.GetController<AdminController>(notAdminUser.UserId, null, null);

            Mock<IDataRepository> configurableDataRepository = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            configurableDataRepository
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(notAdminUser);

            configurableDataRepository
                .Setup(x => x.GetAll<Organisation>())
                .Returns(new[] {organisationToChangeName}.AsQueryable().BuildMock().Object);

            var manualChangesViewModel = new ManualChangesViewModel {
                Command = AddOrganisationsLatestNameCommand,
                Parameters =
                    $"{organisationToChangeName.EmployerReference.ToLower()}={organisationToChangeName.OrganisationName}" // We're calling change name, but in reality we're setting the same one
            };

            #endregion

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
                        "SUCCESSFULLY TESTED 'Add organisations latest name': 1 of 1",
                        actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual(
                        "<span style=\"color:Orange\">1: WARNING: 'EMPLOYERREFERENCE373737' 'New name limited' already set to 'New name limited'</span>\r\n",
                        actualManualChangesViewModel.Results);
                    Assert.AreEqual("Add organisations latest name", actualManualChangesViewModel.LastTestedCommand);
                    Assert.AreEqual("employerreference373737=New name limited", actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be tested=true as this case is running in TEST mode");
                    Assert.IsNull(actualManualChangesViewModel.Comment);
                });
        }

        [Test]
        public async Task
            AdminController_ManualChanges_POST_Add_Organisation_Latest_Name_Fails_When_The_New_Name_Exists_In_Another_Org_In_Database_Async()
        {
            // Arrange
            Organisation organisationToChangeName = OrganisationHelper.GetMockedOrganisation("EmployerReference565658");
            organisationToChangeName.OrganisationName = "Old name";

            Organisation organisationWithSameNameInDb = OrganisationHelper.GetMockedOrganisation("EmployerReference55441122");
            organisationWithSameNameInDb.OrganisationName = "Another org with this name limited";

            #region setting up database and controller 

            User notAdminUser = UserHelper.GetDatabaseAdmin();
            var adminController = UiTestHelper.GetController<AdminController>(notAdminUser.UserId, null, null);

            Mock<IDataRepository> configurableDataRepository = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            configurableDataRepository
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(notAdminUser);

            configurableDataRepository
                .Setup(x => x.GetAll<Organisation>())
                .Returns(new[] {organisationToChangeName, organisationWithSameNameInDb}.AsQueryable().BuildMock().Object);

            var manualChangesViewModel = new ManualChangesViewModel {
                Command = AddOrganisationsLatestNameCommand,
                Parameters =
                    $"{organisationToChangeName.EmployerReference.ToLower()}=Another org with this name limited" // This name already exists in another org in DB
            };

            #endregion

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
                        "SUCCESSFULLY TESTED 'Add organisations latest name': 0 of 1",
                        actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual(
                        "<span style=\"color:Red\">1: ERROR: 'EMPLOYERREFERENCE565658' Another organisation exists with this company name</span>\r\n",
                        actualManualChangesViewModel.Results);
                    Assert.AreEqual("Add organisations latest name", actualManualChangesViewModel.LastTestedCommand);
                    Assert.AreEqual(
                        "employerreference565658=Another org with this name limited",
                        actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be tested=true as this case is running in TEST mode");
                    Assert.IsNull(actualManualChangesViewModel.Comment);
                });
        }

        #endregion

    }
}

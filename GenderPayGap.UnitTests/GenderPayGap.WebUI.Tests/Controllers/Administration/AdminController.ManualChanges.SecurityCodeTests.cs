using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using GenderPayGap.BusinessLogic;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Tests.Common.Classes;
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
    [SetCulture("en-GB")]
    public class AdminControllerManualChangesSecurityCodeTests
    {

        private const string TestEmployerReference = "S3CUR1TYC0D3T35T5";
        private const string SetToCreateSecurityCodeCommand = "Create security code";
        private const string SetToExtendSecurityCodeCommand = "Extend security code";
        private const string SetToExpireSecurityCodeCommand = "Expire security code";
        private const string SetToCreateSecurityCodeInBulkCommand = "Create security codes for all active and pending orgs";
        private const string SetToExtendSecurityCodeInBulkCommand = "Extend security codes for all active and pending orgs";
        private const string SetToExpireSecurityCodeInBulkCommand = "Expire security codes for all active and pending orgs";
        private User _databaseAdminUser;
        private object[] _dbObjects;

        [SetUp]
        public void InitialiseDbObjectsBeforeEachTest()
        {
            Organisation org = OrganisationHelper.GetOrganisationInScope(TestEmployerReference, Global.FirstReportingYear);
            org.SetSecurityCode(VirtualDateTime.Now.AddDays(90));
            _databaseAdminUser = UserHelper.GetDatabaseAdmin();
            _dbObjects = new object[] {org, _databaseAdminUser};
        }

        #region helpers for this class

        private AdminController GetControllerForOutOfScopeTests()
        {
            var mockedController = UiTestHelper.GetController<AdminController>(-1, null, _dbObjects);

            var organisationBusinessLogic = new OrganisationBusinessLogic(
                UiTestHelper.DIContainer.Resolve<ICommonBusinessLogic>(),
                UiTestHelper.DIContainer.Resolve<IDataRepository>(),
                UiTestHelper.DIContainer.Resolve<ISubmissionBusinessLogic>(),
                UiTestHelper.DIContainer.Resolve<IScopeBusinessLogic>(),
                UiTestHelper.DIContainer.Resolve<IEncryptionHandler>(),
                UiTestHelper.DIContainer.Resolve<ISecurityCodeBusinessLogic>());

            mockedController.OrganisationBusinessLogic = organisationBusinessLogic;
            return mockedController;
        }

        private static async Task<ManualChangesViewModel> ActOnTheAdminControllerManualChangesAsync(AdminController adminController,
            ManualChangesViewModel manualChangesViewModel)
        {
            var manualChangesViewResult = await adminController.ManualChanges(manualChangesViewModel) as ViewResult;
            Assert.NotNull(manualChangesViewResult, "Expected ViewResult");

            var actualManualChangesViewModel = manualChangesViewResult.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            return actualManualChangesViewModel;
        }

        private static AdminController AdminController(out ManualChangesViewModel manualChangesViewModel,
            IEnumerable<Organisation> listOfOrganisations = null)
        {
            User databaseAdminUser = UserHelper.CreateUser("databaseadmin@email.com");

            var controller = UiTestHelper.GetController<AdminController>(-1, null, databaseAdminUser, listOfOrganisations);

            manualChangesViewModel = Mock.Of<ManualChangesViewModel>();

            return controller;
        }

        private static Organisation SetOrganisation(Mock<Organisation> org, int i, OrganisationStatuses orgStatus)
        {
            Organisation organisation = org.Object;
            organisation.OrganisationName = $"Org_{i}";
            organisation.EmployerReference = $"EmpRef_{i}";
            organisation.Status = orgStatus;
            return organisation;
        }

        #endregion

        #region Expire security code

        [Test]
        public async Task AdminController_ManualChanges_POST_Extend_SecurityCode_Fails_When_Applied_To_Expired_Security_CodeAsync()
        {
            // Arrange
            Organisation thisTestOrg = OrganisationHelper.GetOrganisationInScope(TestEmployerReference, Global.FirstReportingYear);
            thisTestOrg.SetSecurityCode(VirtualDateTime.Now.AddDays(90));
            thisTestOrg.SetSecurityCodeExpiryDate(VirtualDateTime.Now.AddDays(-5));

            User thisTestDbAdminUser = UserHelper.GetDatabaseAdmin();
            var thisTestDbObjects = new object[] {thisTestOrg, thisTestDbAdminUser};

            var adminController = UiTestHelper.GetController<AdminController>(thisTestDbAdminUser.UserId, null, thisTestDbObjects);

            string securityCodeExpirationParameters = $"{TestEmployerReference}={VirtualDateTime.Now.AddDays(-7):dd/MM/yyyy}";
            ManualChangesViewModel manualChangesViewModelMockObject = ManualChangesViewModelHelper.GetMock(
                SetToExtendSecurityCodeCommand,
                securityCodeExpirationParameters);

            // Act
            ManualChangesViewModel actualManualChangesViewModel =
                await ActOnTheAdminControllerManualChangesAsync(adminController, manualChangesViewModelMockObject);

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual("SUCCESSFULLY TESTED 'Extend security code': 0 of 1", actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual(
                        "<span style=\"color:Red\">1: ERROR: 'ref:S3CUR1TYC0D3T35T5, name:Org123' Cannot modify the security code information of an already expired security code</span>\r\n",
                        actualManualChangesViewModel.Results);
                    Assert.AreEqual(manualChangesViewModelMockObject.LastTestedCommand, actualManualChangesViewModel.LastTestedCommand);
                    Assert.AreEqual($"{securityCodeExpirationParameters}", actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be true as this case is running in test mode");
                    Assert.Null(actualManualChangesViewModel.Comment);
                });
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Expire_SecurityCode_Succeeds_When_Running_In_Test_ModeAsync()
        {
            // Arrange
            AdminController adminControllerToExtendSecurityCodeTests = GetControllerForOutOfScopeTests();

            string securityCodeExpirationParameters = $"{TestEmployerReference}";
            ManualChangesViewModel manualChangesViewModelMockObject = ManualChangesViewModelHelper.GetMock(
                SetToExpireSecurityCodeCommand,
                securityCodeExpirationParameters);

            // Act
            ManualChangesViewModel actualManualChangesViewModel = await ActOnTheAdminControllerManualChangesAsync(
                adminControllerToExtendSecurityCodeTests,
                manualChangesViewModelMockObject);

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual(actualManualChangesViewModel.SuccessMessage, "SUCCESSFULLY TESTED 'Expire security code': 1 of 1");
                    Assert.AreEqual(actualManualChangesViewModel.Results, "1: ref:S3CUR1TYC0D3T35T5, name:Org123: will be expired.\r\n");
                    Assert.AreEqual(manualChangesViewModelMockObject.LastTestedCommand, actualManualChangesViewModel.LastTestedCommand);
                    Assert.AreEqual($"{securityCodeExpirationParameters}", actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be true as this case is running in test mode");
                    Assert.Null(actualManualChangesViewModel.Comment);
                });
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Expire_SecurityCode_Succeeds_When_Running_In_Live_ModeAsync()
        {
            // Arrange
            AdminController adminControllerToExtendSecurityCodeTests = GetControllerForOutOfScopeTests();
            string securityCodeExpirationParameters = $"{TestEmployerReference}";
            ManualChangesViewModel manualChangesViewModelMockObject = ManualChangesViewModelHelper.GetMock(
                SetToExpireSecurityCodeCommand,
                securityCodeExpirationParameters);

            /* live */
            manualChangesViewModelMockObject.LastTestedCommand = manualChangesViewModelMockObject.Command;
            manualChangesViewModelMockObject.LastTestedInput = manualChangesViewModelMockObject.Parameters;

            // Act
            ManualChangesViewModel actualManualChangesViewModel = await ActOnTheAdminControllerManualChangesAsync(
                adminControllerToExtendSecurityCodeTests,
                manualChangesViewModelMockObject);

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual(
                        actualManualChangesViewModel.SuccessMessage,
                        "SUCCESSFULLY EXECUTED 'Expire security code': 1 of 1");
                    var expectedValue = "1: S3CUR1TYC0D3T35T5: has been updated to be '";
                    Assert.True(
                        actualManualChangesViewModel.Results.Contains("1: ref:S3CUR1TYC0D3T35T5, name:Org123: has been expired."),
                        $"Result [{actualManualChangesViewModel.Results}] does NOT contain Expected [{expectedValue}]");
                    Assert.Null(actualManualChangesViewModel.LastTestedCommand);
                    Assert.Null(actualManualChangesViewModel.LastTestedInput);
                    Assert.False(actualManualChangesViewModel.Tested, "Must be Tested=false as this case is running in LIVE mode");
                    Assert.Null(actualManualChangesViewModel.Comment);
                });
        }

        #endregion

        #region Expire security code in Bulk

        [Test]
        public async Task AdminController_ManualChanges_POST_Expire_SecurityCode_Fails_When_Applied_To_Expired_Security_CodeAsync()
        {
            // Arrange
            Organisation thisTestOrg = OrganisationHelper.GetOrganisationInScope(TestEmployerReference, Global.FirstReportingYear);
            thisTestOrg.SetSecurityCode(VirtualDateTime.Now.AddDays(90));
            thisTestOrg.SetSecurityCodeExpiryDate(VirtualDateTime.Now.AddDays(-5));

            User thisTestDbAdminUser = UserHelper.GetDatabaseAdmin();
            var thisTestDbObjects = new object[] {thisTestOrg, thisTestDbAdminUser};

            var adminController = UiTestHelper.GetController<AdminController>(thisTestDbAdminUser.UserId, null, thisTestDbObjects);

            string securityCodeExpirationParameters = $"{TestEmployerReference}";
            ManualChangesViewModel manualChangesViewModelMockObject = ManualChangesViewModelHelper.GetMock(
                SetToExpireSecurityCodeCommand,
                securityCodeExpirationParameters);

            // Act
            ManualChangesViewModel actualManualChangesViewModel =
                await ActOnTheAdminControllerManualChangesAsync(adminController, manualChangesViewModelMockObject);

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual("SUCCESSFULLY TESTED 'Expire security code': 0 of 1", actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual(
                        "<span style=\"color:Red\">1: ERROR: 'ref:S3CUR1TYC0D3T35T5, name:Org123' Cannot modify the security code information of an already expired security code</span>\r\n",
                        actualManualChangesViewModel.Results);
                    Assert.AreEqual(manualChangesViewModelMockObject.LastTestedCommand, actualManualChangesViewModel.LastTestedCommand);
                    Assert.AreEqual($"{securityCodeExpirationParameters}", actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be true as this case is running in test mode");
                    Assert.Null(actualManualChangesViewModel.Comment);
                });
        }
        
        #endregion

        #region Extend security code

        [Test]
        public async Task AdminController_ManualChanges_POST_Extend_SecurityCode_Fails_When_Security_Code_Is_NullAsync()
        {
            // Arrange (data for this test is slightly different than others in this class)
            Organisation organisationWithoutSecurityCode =
                OrganisationHelper.GetOrganisationInScope(TestEmployerReference, Global.FirstReportingYear);
            _dbObjects = new object[] {organisationWithoutSecurityCode, _databaseAdminUser};

            AdminController adminControllerToExtendSecurityCodeTests = GetControllerForOutOfScopeTests();
            string securityCodeExpiryDate = $"{VirtualDateTime.Now.AddDays(10):dd/MM/yyyy}";
            string securityCodeExpirationParameters = $"{TestEmployerReference}={securityCodeExpiryDate}";
            ManualChangesViewModel manualChangesViewModelMockObject = ManualChangesViewModelHelper.GetMock(
                SetToExtendSecurityCodeCommand,
                securityCodeExpirationParameters);

            // Act
            ManualChangesViewModel actualManualChangesViewModel = await ActOnTheAdminControllerManualChangesAsync(
                adminControllerToExtendSecurityCodeTests,
                manualChangesViewModelMockObject);

            // Assert
            //Assert.Multiple(() => {
            Assert.AreEqual(actualManualChangesViewModel.SuccessMessage, "SUCCESSFULLY TESTED 'Extend security code': 0 of 1");
            Assert.True(
                actualManualChangesViewModel.Results.Contains(
                    "<span style=\"color:Red\">1: ERROR: 'ref:S3CUR1TYC0D3T35T5, name:Org123' Nothing to modify: Unable to modify the security code's expiry date of the organisation because it's security code is null</span>"));
            Assert.AreEqual(manualChangesViewModelMockObject.LastTestedCommand, actualManualChangesViewModel.LastTestedCommand);
            Assert.AreEqual($"{securityCodeExpirationParameters}", actualManualChangesViewModel.LastTestedInput);
            Assert.True(actualManualChangesViewModel.Tested, "Must be true as this case is running in test mode");
            Assert.Null(actualManualChangesViewModel.Comment);
            //});
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Extend_SecurityCode_Succeeds_When_Running_In_Test_ModeAsync()
        {
            // Arrange
            AdminController adminControllerToExtendSecurityCodeTests = GetControllerForOutOfScopeTests();
            DateTime securityCodeExpiryDate = VirtualDateTime.Now.AddDays(10);
            string securityCodeExpirationParameters = $"{TestEmployerReference}={securityCodeExpiryDate:dd/MM/yyyy}";
            ManualChangesViewModel manualChangesViewModelMockObject = ManualChangesViewModelHelper.GetMock(
                SetToExtendSecurityCodeCommand,
                securityCodeExpirationParameters);

            // Act
            ManualChangesViewModel actualManualChangesViewModel = await ActOnTheAdminControllerManualChangesAsync(
                adminControllerToExtendSecurityCodeTests,
                manualChangesViewModelMockObject);

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual(actualManualChangesViewModel.SuccessMessage, "SUCCESSFULLY TESTED 'Extend security code': 1 of 1");
                    Assert.AreEqual(
                        $"1: ref:S3CUR1TYC0D3T35T5, name:Org123: will be extended to be '*******' and will expire on '{securityCodeExpiryDate:dd/MMMM/yyyy}'\r\n",
                        actualManualChangesViewModel.Results);
                    Assert.AreEqual(manualChangesViewModelMockObject.LastTestedCommand, actualManualChangesViewModel.LastTestedCommand);
                    Assert.AreEqual($"{securityCodeExpirationParameters}", actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be true as this case is running in test mode");
                    Assert.Null(actualManualChangesViewModel.Comment);
                });
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Extend_SecurityCode_Succeeds_When_Running_In_Live_ModeAsync()
        {
            // Arrange
            AdminController adminControllerToExtendSecurityCodeTests = GetControllerForOutOfScopeTests();
            DateTime securityCodeExpiryDate = VirtualDateTime.Now.AddDays(10);
            string securityCodeExpirationParameters = $"{TestEmployerReference}={securityCodeExpiryDate:dd/MM/yyyy}";
            ManualChangesViewModel manualChangesViewModelMockObject = ManualChangesViewModelHelper.GetMock(
                SetToExtendSecurityCodeCommand,
                securityCodeExpirationParameters);

            /* live */
            manualChangesViewModelMockObject.LastTestedCommand = manualChangesViewModelMockObject.Command;
            manualChangesViewModelMockObject.LastTestedInput = manualChangesViewModelMockObject.Parameters;

            // Act
            ManualChangesViewModel actualManualChangesViewModel = await ActOnTheAdminControllerManualChangesAsync(
                adminControllerToExtendSecurityCodeTests,
                manualChangesViewModelMockObject);

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual(actualManualChangesViewModel.SuccessMessage, "SUCCESSFULLY EXECUTED 'Extend security code': 1 of 1");
                    var expectedValue = "1: ref:S3CUR1TYC0D3T35T5, name:Org123: has been extended to be '";
                    Assert.True(
                        actualManualChangesViewModel.Results.Contains(expectedValue),
                        $"Result [{actualManualChangesViewModel.Results}] does NOT contain Expected [{expectedValue}]");
                    Assert.True(
                        actualManualChangesViewModel.Results.Contains($"' and will expire on '{securityCodeExpiryDate:dd/MMMM/yyyy}'\r\n"));
                    Assert.Null(actualManualChangesViewModel.LastTestedCommand);
                    Assert.Null(actualManualChangesViewModel.LastTestedInput);
                    Assert.False(actualManualChangesViewModel.Tested, "Must be Tested=false as this case is running in LIVE mode");
                    Assert.Null(actualManualChangesViewModel.Comment);
                });
        }

        #endregion
        
        #region Create security code

        [Test]
        public async Task AdminController_ManualChanges_POST_Create_SecurityCode_Succeeds_When_Running_In_Test_ModeAsync()
        {
            // Arrange
            AdminController adminControllerForCreateSecurityCodeTests = GetControllerForOutOfScopeTests();
            DateTime securityCodeExpiryDate = VirtualDateTime.Now.AddDays(1);
            string securityCodeExpirationParameters = $"{TestEmployerReference}={securityCodeExpiryDate:dd/MM/yyyy}";
            ManualChangesViewModel manualChangesViewModelMockObject = ManualChangesViewModelHelper.GetMock(
                SetToCreateSecurityCodeCommand,
                securityCodeExpirationParameters);

            // Act
            ManualChangesViewModel actualManualChangesViewModel = await ActOnTheAdminControllerManualChangesAsync(
                adminControllerForCreateSecurityCodeTests,
                manualChangesViewModelMockObject);

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual(actualManualChangesViewModel.SuccessMessage, "SUCCESSFULLY TESTED 'Create security code': 1 of 1");
                    var expectedValue = "1: ref:S3CUR1TYC0D3T35T5, name:Org123: will be created to be '";
                    Assert.True(
                        actualManualChangesViewModel.Results.Contains(expectedValue),
                        $"Result [{actualManualChangesViewModel.Results}] does NOT contain Expected [{expectedValue}]");
                    Assert.True(
                        actualManualChangesViewModel.Results.Contains($"' and will expire on '{securityCodeExpiryDate:dd/MMMM/yyyy}'"));
                    Assert.AreEqual(manualChangesViewModelMockObject.LastTestedCommand, actualManualChangesViewModel.LastTestedCommand);
                    Assert.AreEqual($"{securityCodeExpirationParameters}", actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be true as this case is running in test mode");
                    Assert.Null(actualManualChangesViewModel.Comment);
                });
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Create_SecurityCode_Succeeds_When_Running_In_Live_ModeAsync()
        {
            // Arrange
            AdminController adminControllerForCreateSecurityCodeTests = GetControllerForOutOfScopeTests();
            string securityCodeExpiryDate = $"{VirtualDateTime.Now.AddDays(1):dd/MMMM/yyyy}";
            string securityCodeExpirationParameters = $"{TestEmployerReference}={securityCodeExpiryDate}";
            ManualChangesViewModel manualChangesViewModelMockObject = ManualChangesViewModelHelper.GetMock(
                SetToCreateSecurityCodeCommand,
                securityCodeExpirationParameters);

            /* live */
            manualChangesViewModelMockObject.LastTestedCommand = manualChangesViewModelMockObject.Command;
            manualChangesViewModelMockObject.LastTestedInput = manualChangesViewModelMockObject.Parameters;

            // Act
            ManualChangesViewModel actualManualChangesViewModel = await ActOnTheAdminControllerManualChangesAsync(
                adminControllerForCreateSecurityCodeTests,
                manualChangesViewModelMockObject);

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual(actualManualChangesViewModel.SuccessMessage, "SUCCESSFULLY EXECUTED 'Create security code': 1 of 1");
                    Assert.True(
                        actualManualChangesViewModel.Results.Contains("1: ref:S3CUR1TYC0D3T35T5, name:Org123: has been created to be '"),
                        $"Result [{actualManualChangesViewModel.Results}] does NOT contain Expected [1: ref:S3CUR1TYC0D3T35T5, name:Org123: has been created to be ']");
                    Assert.True(
                        actualManualChangesViewModel.Results.Contains($"' and will expire on '{securityCodeExpiryDate}'"),
                        $"Result [{actualManualChangesViewModel.Results}] does NOT contain Expected [' and to expire on '{securityCodeExpiryDate}']");
                    Assert.True(
                        actualManualChangesViewModel.Results.Contains("<span style=\"color:Green\">INFO: Changes saved to database</span>"),
                        $"Result [{actualManualChangesViewModel.Results}] does NOT contain Expected [<span style=\"color:Green\">INFO: Changes saved to database</span>]");
                    Assert.Null(
                        actualManualChangesViewModel.LastTestedCommand,
                        "During a LIVE mode execution, the LastTestedCommand is expected to be 'null'");
                    Assert.Null(
                        actualManualChangesViewModel.LastTestedInput,
                        "During a LIVE mode execution, the LastTestedInput is expected to be 'null'");
                    Assert.False(actualManualChangesViewModel.Tested, "Must be tested=false as this case is running in LIVE mode");
                    Assert.Null(actualManualChangesViewModel.Comment);
                });
        }

        #endregion
        
    }
}

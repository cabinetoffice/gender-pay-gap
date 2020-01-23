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


        [Test]
        public async Task AdminController_ManualChanges_POST_Expire_Security_Code_In_Bulk_Reports_Relevant_FailuresAsync()
        {
            var listOfOrganisations = new List<Organisation>();
            for (var i = 0; i < 1000; i++)
            {
                Organisation organisation;
                var org = new Mock<Organisation> {CallBase = true};

                if (i % 37 == 0)
                {
                    /* These will fail when trying to expire a security code that doesn't exist */
                    organisation = SetOrganisation(org, i, OrganisationStatuses.Pending);
                    listOfOrganisations.Add(organisation);
                    continue;
                }

                if (i % 45 == 0)
                {
                    /* These will fail when trying to expire an already expired security code*/
                    organisation = SetOrganisation(org, i, OrganisationStatuses.Active);
                    organisation.SetSecurityCode(VirtualDateTime.Now.AddDays(1));
                    organisation.SetSecurityCodeExpiryDate(VirtualDateTime.Now.AddDays(-1));
                    listOfOrganisations.Add(organisation);
                    continue;
                }

                if (i % 3 == 0)
                {
                    organisation = SetOrganisation(org, i, OrganisationStatuses.Pending);
                    organisation.SetSecurityCode(VirtualDateTime.Now.AddDays(7));
                    listOfOrganisations.Add(organisation);
                    continue;
                }

                organisation = SetOrganisation(org, i, OrganisationStatuses.Active);
                organisation.SetSecurityCode(VirtualDateTime.Now.AddDays(7));
                listOfOrganisations.Add(organisation);
            }

            User databaseAdminUser = UserHelper.CreateUser("databaseadmin@email.com");
            var adminController = UiTestHelper.GetController<AdminController>(_databaseAdminUser.UserId);
            var manualChangesViewModel = Mock.Of<ManualChangesViewModel>();
            manualChangesViewModel.Command = SetToExpireSecurityCodeInBulkCommand;
            manualChangesViewModel.Parameters = string.Empty;

            Mock<IDataRepository> mockedDatabase = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            mockedDatabase.SetupGetAll(listOfOrganisations);
            mockedDatabase.Setup(x => x.Get<User>(It.IsAny<long>())).Returns(_databaseAdminUser);

            // Act
            var retireOrgActionResult = await adminController.ManualChanges(manualChangesViewModel) as ViewResult;
            Assert.NotNull(retireOrgActionResult);

            var actualManualChangesViewModel = retireOrgActionResult.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel);

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual(
                        "SUCCESSFULLY TESTED 'Expire security codes for all active and pending orgs': 950 items",
                        actualManualChangesViewModel.SuccessMessage);

                    var securityCodeNullErrors =
                        "<span style=\"color:Red\">28 ERROR(S) of type '0' Nothing to modify: Unable to modify the security code's expiry date of the organisation because it's security code is null</span>\r\n";
                    Assert.True(
                        actualManualChangesViewModel.Results.Contains(securityCodeNullErrors),
                        $"Result [{actualManualChangesViewModel.Results}] does NOT contain Expected [{securityCodeNullErrors}]");

                    var securityCodeNullDetail =
                        "[ref:EmpRef_814, name:Org_814 0 'Nothing to modify: Unable to modify the security code's expiry date of the organisation because it's security code is null']";
                    Assert.True(
                        actualManualChangesViewModel.Results.Contains(securityCodeNullDetail),
                        $"Result [{actualManualChangesViewModel.Results}] does NOT contain Expected [{securityCodeNullDetail}]");

                    var alreadyExpiredErrors =
                        "<span style=\"color:Red\">22 ERROR(S) of type '4004' Cannot modify the security code information of an already expired security code</span>\r\n";
                    Assert.True(
                        actualManualChangesViewModel.Results.Contains(alreadyExpiredErrors),
                        $"Result [{actualManualChangesViewModel.Results}] does NOT contain Expected [{alreadyExpiredErrors}]");

                    var alreadyExpiredDetail =
                        "[ref:EmpRef_810, name:Org_810 4004 'Cannot modify the security code information of an already expired security code']";
                    Assert.True(
                        actualManualChangesViewModel.Results.Contains(alreadyExpiredDetail),
                        $"Result [{actualManualChangesViewModel.Results}] does NOT contain Expected [{alreadyExpiredDetail}]");

                    var expectedSummary =
                        "A Total of 50 records FAILED and will be ignored. A total of 950 security codes will be expired.";
                    Assert.True(
                        actualManualChangesViewModel.Results.Contains(expectedSummary),
                        $"Result [{actualManualChangesViewModel.Results}] does NOT contain Expected [{expectedSummary}]");

                    Assert.AreEqual(
                        "Expire security codes for all active and pending orgs",
                        actualManualChangesViewModel.LastTestedCommand);

                    Assert.Null(actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be true as this case is running in test mode");
                    Assert.Null(actualManualChangesViewModel.Comment);
                });
        }


        [Test]
        public async Task AdminController_ManualChanges_POST_Expire_Security_Code_In_Bulk_Succeeds_When_Run_In_Test_ModeAsync()
        {
            var listOfOrganisations = new List<Organisation>();
            for (var i = 0; i < 1000; i++)
            {
                Organisation organisation;
                var org = new Mock<Organisation> {CallBase = true};

                if (i % 37 == 0)
                {
                    /* These will fail when trying to extend a security code that doesn't exist */
                    organisation = org.Object;
                    organisation.Status = OrganisationStatuses.Pending;
                    listOfOrganisations.Add(org.Object);
                    continue;
                }

                if (i % 3 == 0)
                {
                    organisation = org.Object;
                    organisation.SetSecurityCode(VirtualDateTime.Now.AddDays(7));
                    organisation.Status = OrganisationStatuses.Pending;
                    listOfOrganisations.Add(org.Object);
                    continue;
                }

                organisation = org.Object;
                organisation.SetSecurityCode(VirtualDateTime.Now.AddDays(7));
                organisation.Status = OrganisationStatuses.Active;
                listOfOrganisations.Add(org.Object);
            }

            User databaseAdminUser = UserHelper.CreateUser("databaseadmin@email.com");
            var adminController = UiTestHelper.GetController<AdminController>(_databaseAdminUser.UserId);
            var manualChangesViewModel = Mock.Of<ManualChangesViewModel>();
            manualChangesViewModel.Command = SetToExpireSecurityCodeInBulkCommand;
            manualChangesViewModel.Parameters = string.Empty;

            Mock<IDataRepository> mockedDatabase = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            mockedDatabase.SetupGetAll((ICollection<Organisation>) listOfOrganisations);
            mockedDatabase.Setup(x => x.Get<User>(It.IsAny<long>())).Returns(_databaseAdminUser);

            // Act
            var retireOrgActionResult = await adminController.ManualChanges(manualChangesViewModel) as ViewResult;
            Assert.NotNull(retireOrgActionResult);

            var actualManualChangesViewModel = retireOrgActionResult.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel);

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual(
                        "SUCCESSFULLY TESTED 'Expire security codes for all active and pending orgs': 972 items",
                        actualManualChangesViewModel.SuccessMessage);

                    var securityCodeNullErrors =
                        "<span style=\"color:Red\">28 ERROR(S) of type '0' Nothing to modify: Unable to modify the security code's expiry date of the organisation because it's security code is null</span>\r\n";
                    Assert.True(
                        actualManualChangesViewModel.Results.Contains(securityCodeNullErrors),
                        $"Result [{actualManualChangesViewModel.Results}] does NOT contain Expected [{securityCodeNullErrors}]");

                    var expectedSummary =
                        "A Total of 28 records FAILED and will be ignored. A total of 972 security codes will be expired.";
                    Assert.True(
                        actualManualChangesViewModel.Results.Contains(expectedSummary),
                        $"Result [{actualManualChangesViewModel.Results}] does NOT contain Expected [{expectedSummary}]");

                    Assert.AreEqual(
                        "Expire security codes for all active and pending orgs",
                        actualManualChangesViewModel.LastTestedCommand);

                    Assert.Null(actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be true as this case is running in test mode");
                    Assert.Null(actualManualChangesViewModel.Comment);
                });
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Expire_Security_Code_In_Bulk_Succeeds_When_Run_In_Live_ModeAsync()
        {
            var listOfOrganisations = new List<Organisation>();
            for (var i = 0; i < 1000; i++)
            {
                Organisation organisation;
                var org = new Mock<Organisation> {CallBase = true};

                if (i % 37 == 0)
                {
                    /* These will fail when trying to extend a security code that doesn't exist */
                    organisation = org.Object;
                    organisation.Status = OrganisationStatuses.Pending;
                    listOfOrganisations.Add(org.Object);
                    continue;
                }

                if (i % 3 == 0)
                {
                    organisation = org.Object;
                    organisation.SetSecurityCode(VirtualDateTime.Now.AddDays(7));
                    organisation.Status = OrganisationStatuses.Pending;
                    listOfOrganisations.Add(org.Object);
                    continue;
                }

                organisation = org.Object;
                organisation.SetSecurityCode(VirtualDateTime.Now.AddDays(7));
                organisation.Status = OrganisationStatuses.Active;
                listOfOrganisations.Add(org.Object);
            }

            User databaseAdminUser = UserHelper.CreateUser("databaseadmin@email.com");
            var adminController = UiTestHelper.GetController<AdminController>(_databaseAdminUser.UserId);
            var manualChangesViewModel = Mock.Of<ManualChangesViewModel>();
            manualChangesViewModel.Command = SetToExpireSecurityCodeInBulkCommand;
            manualChangesViewModel.Parameters = null;

            Mock<IDataRepository> mockedDatabase = AutoFacExtensions.ResolveAsMock<IDataRepository>();
            mockedDatabase.SetupGetAll((ICollection<Organisation>) listOfOrganisations);
            mockedDatabase.Setup(x => x.Get<User>(It.IsAny<long>())).Returns(_databaseAdminUser);

            /* live */
            manualChangesViewModel.LastTestedCommand = manualChangesViewModel.Command;
            manualChangesViewModel.LastTestedInput = manualChangesViewModel.Parameters;

            // Act
            var retireOrgActionResult = await adminController.ManualChanges(manualChangesViewModel) as ViewResult;
            Assert.NotNull(retireOrgActionResult);

            var actualManualChangesViewModel = retireOrgActionResult.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel);

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual(
                        "SUCCESSFULLY EXECUTED 'Expire security codes for all active and pending orgs': 972 items",
                        actualManualChangesViewModel.SuccessMessage);

                    var securityCodeNullErrors =
                        "<span style=\"color:Red\">28 ERROR(S) of type '0' Nothing to modify: Unable to modify the security code's expiry date of the organisation because it's security code is null</span>\r\n";
                    Assert.True(
                        actualManualChangesViewModel.Results.Contains(securityCodeNullErrors),
                        $"Result [{actualManualChangesViewModel.Results}] does NOT contain Expected [{securityCodeNullErrors}]");

                    var expectedSummary =
                        "A Total of 28 records FAILED and have been ignored. A total of 972 security codes have been successfully expired.";
                    Assert.True(
                        actualManualChangesViewModel.Results.Contains(expectedSummary),
                        $"Result [{actualManualChangesViewModel.Results}] does NOT contain Expected [{expectedSummary}]");

                    Assert.Null(actualManualChangesViewModel.LastTestedCommand);
                    Assert.Null(actualManualChangesViewModel.LastTestedInput);
                    Assert.False(actualManualChangesViewModel.Tested, "Must be Tested=false as this case is running in LIVE mode");
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

        #region Extend security code in Bulk

        [Test]
        public async Task AdminController_ManualChanges_POST_Extend_Security_Code_In_Bulk_Succeeds_When_Run_In_Test_ModeAsync()
        {
            var listOfOrganisations = new List<Organisation>();
            for (var i = 0; i < 1000; i++)
            {
                Organisation organisation;
                var org = new Mock<Organisation> {CallBase = true};

                if (i % 37 == 0)
                {
                    /* These will fail when trying to extend a security code that doesn't exist */
                    organisation = org.Object;
                    organisation.Status = OrganisationStatuses.Pending;
                    listOfOrganisations.Add(org.Object);
                    continue;
                }

                if (i % 3 == 0)
                {
                    organisation = org.Object;
                    organisation.SetSecurityCode(VirtualDateTime.Now.AddDays(7));
                    organisation.Status = OrganisationStatuses.Pending;
                    listOfOrganisations.Add(org.Object);
                    continue;
                }

                organisation = org.Object;
                organisation.SetSecurityCode(VirtualDateTime.Now.AddDays(7));
                organisation.Status = OrganisationStatuses.Active;
                listOfOrganisations.Add(org.Object);
            }

            User databaseAdminUser = UserHelper.CreateUser("databaseadmin@email.com");
            var adminController = UiTestHelper.GetController<AdminController>(_databaseAdminUser.UserId);
            DateTime securityCodeExtendToDate = VirtualDateTime.Now.AddDays(9);
            var manualChangesViewModel = Mock.Of<ManualChangesViewModel>();
            manualChangesViewModel.Command = SetToExtendSecurityCodeInBulkCommand;
            manualChangesViewModel.Parameters = $"{securityCodeExtendToDate:dd/MM/yyyy}";

            Mock<IDataRepository> mockedDatabase = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            mockedDatabase.SetupGetAll((ICollection<Organisation>) listOfOrganisations);
            mockedDatabase.Setup(x => x.Get<User>(It.IsAny<long>())).Returns(_databaseAdminUser);

            // Act
            IActionResult result = await adminController.ManualChanges(manualChangesViewModel);
            var retireOrgActionResult = (ViewResult) result;
            var actualManualChangesViewModel = (ManualChangesViewModel) retireOrgActionResult.Model;

            // Assert
            Assert.NotNull(actualManualChangesViewModel);

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual(
                        "SUCCESSFULLY TESTED 'Extend security codes for all active and pending orgs': 972 of 1000",
                        actualManualChangesViewModel.SuccessMessage);

                    var securityCodeNullErrors =
                        "<span style=\"color:Red\">28 ERROR(S) of type '0' Nothing to modify: Unable to modify the security code's expiry date of the organisation because it's security code is null</span>\r\n";
                    Assert.True(
                        actualManualChangesViewModel.Results.Contains(securityCodeNullErrors),
                        $"Result [{actualManualChangesViewModel.Results}] does NOT contain Expected [{securityCodeNullErrors}]");

                    string expectedSummary =
                        $"A Total of 28 records FAILED and will be ignored. A total of 972 security codes will be set to expire on '{securityCodeExtendToDate:dd/MMMM/yyyy}'";
                    Assert.True(
                        actualManualChangesViewModel.Results.Contains(expectedSummary),
                        $"Result [{actualManualChangesViewModel.Results}] does NOT contain Expected [{expectedSummary}]");

                    Assert.AreEqual(
                        "Extend security codes for all active and pending orgs",
                        actualManualChangesViewModel.LastTestedCommand);

                    Assert.AreEqual($"{securityCodeExtendToDate:dd/MM/yyyy}", actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be true as this case is running in test mode");
                    Assert.Null(actualManualChangesViewModel.Comment);
                });
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Extend_Security_Code_In_Bulk_Succeeds_When_Run_In_Live_ModeAsync()
        {
            var listOfOrganisations = new List<Organisation>();
            for (var i = 0; i < 1000; i++)
            {
                Organisation organisation;
                var org = new Mock<Organisation> {CallBase = true};

                if (i % 37 == 0)
                {
                    /* These will fail when trying to extend a security code that doesn't exist */
                    organisation = org.Object;
                    organisation.Status = OrganisationStatuses.Pending;
                    listOfOrganisations.Add(org.Object);
                    continue;
                }

                if (i % 3 == 0)
                {
                    organisation = org.Object;
                    organisation.SetSecurityCode(VirtualDateTime.Now.AddDays(7));
                    organisation.Status = OrganisationStatuses.Pending;
                    listOfOrganisations.Add(org.Object);
                    continue;
                }

                organisation = org.Object;
                organisation.SetSecurityCode(VirtualDateTime.Now.AddDays(7));
                organisation.Status = OrganisationStatuses.Active;
                listOfOrganisations.Add(org.Object);
            }

            User databaseAdminUser = UserHelper.CreateUser("databaseadmin@email.com");
            var adminController = UiTestHelper.GetController<AdminController>(_databaseAdminUser.UserId);
            DateTime securityCodeExtendToDate = VirtualDateTime.Now.AddDays(9);
            var manualChangesViewModel = Mock.Of<ManualChangesViewModel>();
            manualChangesViewModel.Command = SetToExtendSecurityCodeInBulkCommand;
            manualChangesViewModel.Parameters = $"{securityCodeExtendToDate:dd/MM/yyyy}";

            Mock<IDataRepository> mockedDatabase = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            mockedDatabase.SetupGetAll((ICollection<Organisation>) listOfOrganisations);
            mockedDatabase.Setup(x => x.Get<User>(It.IsAny<long>())).Returns(_databaseAdminUser);

            /* live */
            manualChangesViewModel.LastTestedCommand = manualChangesViewModel.Command;
            manualChangesViewModel.LastTestedInput = manualChangesViewModel.Parameters;

            // Act
            var retireOrgActionResult = await adminController.ManualChanges(manualChangesViewModel) as ViewResult;
            Assert.NotNull(retireOrgActionResult);
            var actualManualChangesViewModel = retireOrgActionResult.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel);

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual(
                        "SUCCESSFULLY EXECUTED 'Extend security codes for all active and pending orgs': 972 of 1000",
                        actualManualChangesViewModel.SuccessMessage);

                    var securityCodeNullErrors =
                        "<span style=\"color:Red\">28 ERROR(S) of type '0' Nothing to modify: Unable to modify the security code's expiry date of the organisation because it's security code is null</span>\r\n";
                    Assert.True(
                        actualManualChangesViewModel.Results.Contains(securityCodeNullErrors),
                        $"Result [{actualManualChangesViewModel.Results}] does NOT contain Expected [{securityCodeNullErrors}]");

                    string expectedSummary =
                        $"A Total of 28 records FAILED and have been ignored. A total of 972 security codes have been successfully set to expire on '{securityCodeExtendToDate:dd/MMMM/yyyy}'";
                    Assert.True(
                        actualManualChangesViewModel.Results.Contains(expectedSummary),
                        $"Result [{actualManualChangesViewModel.Results}] does NOT contain Expected [{expectedSummary}]");

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

        #region Create security code in Bulk

        [Test]
        public async Task AdminController_ManualChanges_POST_Create_Security_Code_In_Bulk_Succeeds_When_Run_In_Test_ModeAsync()
        {
            var listOfOrganisations = new List<Organisation>();
            for (var i = 0; i < 1000; i++)
            {
                Organisation organisation;
                var org = new Mock<Organisation>();

                if (i % 3 == 0)
                {
                    org.Setup(x => x.SetSecurityCode(It.IsAny<DateTime>())).Throws(new Exception("Kaboom!"));
                    organisation = org.Object;
                    organisation.Status = OrganisationStatuses.Pending;
                    listOfOrganisations.Add(org.Object);
                    continue;
                }

                if (i % 4 == 0)
                {
                    org.Setup(x => x.SetSecurityCode(It.IsAny<DateTime>())).Throws(new Exception("Boom!"));
                    organisation = org.Object;
                    organisation.Status = OrganisationStatuses.Pending;
                    listOfOrganisations.Add(org.Object);
                    continue;
                }

                if (i % 5 == 0)
                {
                    org.Setup(x => x.SetSecurityCode(It.IsAny<DateTime>())).Throws(new Exception("Kapow!"));
                    organisation = org.Object;
                    organisation.Status = OrganisationStatuses.Pending;
                    listOfOrganisations.Add(org.Object);
                    continue;
                }

                organisation = org.Object;
                organisation.Status = OrganisationStatuses.Active;
                listOfOrganisations.Add(org.Object);
            }

            User databaseAdminUser = UserHelper.CreateUser("databaseadmin@email.com");
            var adminController = UiTestHelper.GetController<AdminController>(_databaseAdminUser.UserId);
            DateTime securityCodeExpiryDate = VirtualDateTime.Now.AddDays(7);
            var manualChangesViewModel = Mock.Of<ManualChangesViewModel>();
            manualChangesViewModel.Command = SetToCreateSecurityCodeInBulkCommand;
            manualChangesViewModel.Parameters = $"{securityCodeExpiryDate:dd/MM/yyyy}";

            Mock<IDataRepository> mockedDatabase = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            mockedDatabase.SetupGetAll((ICollection<Organisation>) listOfOrganisations);
            mockedDatabase.Setup(x => x.Get<User>(It.IsAny<long>())).Returns(_databaseAdminUser);

            // Act
            var retireOrgActionResult = await adminController.ManualChanges(manualChangesViewModel) as ViewResult;
            var actualManualChangesViewModel = (ManualChangesViewModel) retireOrgActionResult.Model;

            // Assert
            Assert.NotNull(actualManualChangesViewModel);

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual(
                        "SUCCESSFULLY TESTED 'Create security codes for all active and pending orgs': 400 of 1000",
                        actualManualChangesViewModel.SuccessMessage);

                    var expectedKaboomErrors = "<span style=\"color:Red\">334 ERROR(S) of type '0' Kaboom!</span>\r\n";
                    Assert.True(
                        actualManualChangesViewModel.Results.Contains(expectedKaboomErrors),
                        $"Result [{actualManualChangesViewModel.Results}] does NOT contain Expected [{expectedKaboomErrors}]");

                    var expectedBoomErrors = "<span style=\"color:Red\">166 ERROR(S) of type '0' Boom!</span>\r\n";
                    Assert.True(
                        actualManualChangesViewModel.Results.Contains(expectedBoomErrors),
                        $"Result [{actualManualChangesViewModel.Results}] does NOT contain Expected [{expectedBoomErrors}]");

                    var expectedKapowErrors = "<span style=\"color:Red\">100 ERROR(S) of type '0' Kapow!</span>\r\n";
                    Assert.True(
                        actualManualChangesViewModel.Results.Contains(expectedKapowErrors),
                        $"Result [{actualManualChangesViewModel.Results}] does NOT contain Expected [{expectedKapowErrors}]");

                    string expectedSummary =
                        $"A Total of 600 records FAILED and will be ignored. A total of 400 security codes will be set to expire on '{securityCodeExpiryDate:dd/MMMM/yyyy}'";
                    Assert.True(
                        actualManualChangesViewModel.Results.Contains(expectedSummary),
                        $"Result [{actualManualChangesViewModel.Results}] does NOT contain Expected [{expectedSummary}]");

                    Assert.AreEqual(
                        "Create security codes for all active and pending orgs",
                        actualManualChangesViewModel.LastTestedCommand);

                    Assert.AreEqual($"{securityCodeExpiryDate:dd/MM/yyyy}", actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be true as this case is running in test mode");
                    Assert.Null(actualManualChangesViewModel.Comment);
                });
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Create_Security_Code_In_Bulk_Succeeds_When_Run_In_Live_ModeAsync()
        {
            var listOfOrganisations = new List<Organisation>();
            for (var i = 0; i < 100; i++)
            {
                Organisation organisation;
                var org = new Mock<Organisation> {CallBase = true};

                if (i % 3 == 0)
                {
                    org.Setup(x => x.SetSecurityCode(It.IsAny<DateTime>())).Throws(new Exception("Kaboom!"));
                    organisation = SetOrganisation(org, i, OrganisationStatuses.Pending);
                    listOfOrganisations.Add(organisation);
                    continue;
                }

                if (i % 4 == 0)
                {
                    org.Setup(x => x.SetSecurityCode(It.IsAny<DateTime>())).Throws(new Exception("Boom!"));
                    organisation = SetOrganisation(org, i, OrganisationStatuses.Active);
                    listOfOrganisations.Add(organisation);
                    continue;
                }

                if (i % 5 == 0)
                {
                    org.Setup(x => x.SetSecurityCode(It.IsAny<DateTime>())).Throws(new Exception("Kapow!"));
                    organisation = SetOrganisation(org, i, OrganisationStatuses.Pending);
                    listOfOrganisations.Add(organisation);
                    continue;
                }

                organisation = SetOrganisation(org, i, OrganisationStatuses.Active);
                listOfOrganisations.Add(organisation);
            }

            var adminController = UiTestHelper.GetController<AdminController>(_databaseAdminUser.UserId);

            var organisationBusinessLogic = new OrganisationBusinessLogic(
                UiTestHelper.DIContainer.Resolve<ICommonBusinessLogic>(),
                UiTestHelper.DIContainer.Resolve<IDataRepository>(),
                UiTestHelper.DIContainer.Resolve<ISubmissionBusinessLogic>(),
                UiTestHelper.DIContainer.Resolve<IScopeBusinessLogic>(),
                UiTestHelper.DIContainer.Resolve<IEncryptionHandler>(),
                UiTestHelper.DIContainer.Resolve<ISecurityCodeBusinessLogic>());

            adminController.OrganisationBusinessLogic = organisationBusinessLogic;

            DateTime securityCodeExpiryDate = VirtualDateTime.Now.AddDays(7);
            var manualChangesViewModel = Mock.Of<ManualChangesViewModel>();
            manualChangesViewModel.Command = SetToCreateSecurityCodeInBulkCommand;
            manualChangesViewModel.Parameters = $"{securityCodeExpiryDate:dd/MM/yyyy}";

            Mock<IDataRepository> mockedDatabase = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            mockedDatabase.SetupGetAll((ICollection<Organisation>) listOfOrganisations);
            mockedDatabase.Setup(x => x.Get<User>(It.IsAny<long>())).Returns(_databaseAdminUser);

            /* live */
            manualChangesViewModel.LastTestedCommand = manualChangesViewModel.Command;
            manualChangesViewModel.LastTestedInput = manualChangesViewModel.Parameters;

            // Act
            var retireOrgActionResult = await adminController.ManualChanges(manualChangesViewModel) as ViewResult;
            Assert.NotNull(retireOrgActionResult);
            var actualManualChangesViewModel = retireOrgActionResult.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel);

            // Assert
            Assert.AreEqual(
                "SUCCESSFULLY EXECUTED 'Create security codes for all active and pending orgs': 40 of 100",
                actualManualChangesViewModel.SuccessMessage);

            var expectedKaboomErrors = "<span style=\"color:Red\">34 ERROR(S) of type '0' Kaboom!</span>\r\n";
            Assert.True(
                actualManualChangesViewModel.Results.Contains(expectedKaboomErrors),
                $"Result [{actualManualChangesViewModel.Results}] does NOT contain Expected [{expectedKaboomErrors}]");

            var expectedKaboomDetail = "[ref:EmpRef_45, name:Org_45 0 'Kaboom!']";
            Assert.True(
                actualManualChangesViewModel.Results.Contains(expectedKaboomDetail),
                $"Result [{actualManualChangesViewModel.Results}] does NOT contain Expected [{expectedKaboomDetail}]");

            var expectedBoomErrors = "<span style=\"color:Red\">16 ERROR(S) of type '0' Boom!</span>\r\n";
            Assert.True(
                actualManualChangesViewModel.Results.Contains(expectedBoomErrors),
                $"Result [{actualManualChangesViewModel.Results}] does NOT contain Expected [{expectedBoomErrors}]");

            var expectedBoomDetail = "[ref:EmpRef_64, name:Org_64 0 'Boom!']";
            Assert.True(
                actualManualChangesViewModel.Results.Contains(expectedBoomDetail),
                $"Result [{actualManualChangesViewModel.Results}] does NOT contain Expected [{expectedBoomDetail}]");

            var expectedKapowErrors = "<span style=\"color:Red\">10 ERROR(S) of type '0' Kapow!</span>\r\n";
            Assert.True(
                actualManualChangesViewModel.Results.Contains(expectedKapowErrors),
                $"Result [{actualManualChangesViewModel.Results}] does NOT contain Expected [{expectedKapowErrors}]");

            var expectedKapowDetail = "[ref:EmpRef_65, name:Org_65 0 'Kapow!']";
            Assert.True(
                actualManualChangesViewModel.Results.Contains(expectedKapowDetail),
                $"Result [{actualManualChangesViewModel.Results}] does NOT contain Expected [{expectedKapowDetail}]");

            string expectedSummary =
                $"A Total of 60 records FAILED and have been ignored. A total of 40 security codes have been successfully set to expire on '{securityCodeExpiryDate:dd/MMMM/yyyy}'";
            Assert.True(
                actualManualChangesViewModel.Results.Contains(expectedSummary),
                $"Result [{actualManualChangesViewModel.Results}] does NOT contain Expected [{expectedSummary}]");

            Assert.Null(actualManualChangesViewModel.LastTestedCommand);

            Assert.Null(actualManualChangesViewModel.LastTestedInput);
            Assert.False(actualManualChangesViewModel.Tested, "Must be Tested=false as this case is running in LIVE mode");
            Assert.Null(actualManualChangesViewModel.Comment);
        }

        [TestCase("")]
        [TestCase(null)]
        public async Task AdminController_ManualChanges_POST_Create_Security_Code_In_Bulk_Fails_When_No_Parameters_Are_Supplied_Async(
            string nullOrEmptyParameter)
        {
            var adminController = UiTestHelper.GetController<AdminController>(_databaseAdminUser.UserId, null, _dbObjects);
            var manualChangesViewModel = Mock.Of<ManualChangesViewModel>();
            manualChangesViewModel.Command = SetToCreateSecurityCodeInBulkCommand;
            manualChangesViewModel.Parameters = nullOrEmptyParameter; // this test requires null or empty parameters to be provided

            // Act
            var retireOrgActionResult = await adminController.ManualChanges(manualChangesViewModel) as ViewResult;
            Assert.NotNull(retireOrgActionResult);
            var actualManualChangesViewModel = retireOrgActionResult.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel);

            ModelStateEntry modelState = adminController.ModelState[""];
            ModelError reportedError = modelState.Errors.First();

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual("ERROR: You must supply the expiry date for the security codes", reportedError.ErrorMessage);
                    Assert.IsNull(actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual("", actualManualChangesViewModel.Results);
                    Assert.IsNull(actualManualChangesViewModel.LastTestedCommand);
                    Assert.IsNull(actualManualChangesViewModel.LastTestedInput);
                    Assert.IsFalse(actualManualChangesViewModel.Tested);
                    Assert.IsNull(actualManualChangesViewModel.Comment);
                });
        }

        #endregion

    }
}

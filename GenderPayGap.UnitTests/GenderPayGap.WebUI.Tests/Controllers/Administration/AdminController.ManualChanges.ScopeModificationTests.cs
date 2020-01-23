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
    public class AdminControllerManualChangesScopeModificationTests
    {

        private const string SetToOutOfScopeTestCommand = "Set organisation as out of scope";
        private const string SetToInScopeTestCommand = "Set organisation as in scope";
        private const string UnretireOrganisationsCommand = "Unretire organisations";
        private const string RetireOrganisationsCommand = "Retire organisations";

        private const string TestEmployerReference = "A1B2C3D4";
        private User _databaseAdminUser;

        private IQueryable<object> _dbObjects;

        private int TestSnapshotYear => SectorTypes.Private.GetAccountingStartDate().Year - 1;

        [SetUp]
        public void InitialiseDbObjectsBeforeEachTest()
        {
            int callRequiredToInitialiseVirtualDateTime = Global.FirstReportingYear;
            OrganisationScope orgStatusToProveItDoesNotGetRetired =
                OrganisationScopeHelper.GetOrganisationScope_InScope(2001, SectorTypes.Private);
            Organisation org = OrganisationHelper.GetOrganisationInScope(TestEmployerReference, TestSnapshotYear);

            org.OrganisationScopes.Add(orgStatusToProveItDoesNotGetRetired);

            _databaseAdminUser = UserHelper.GetDatabaseAdmin();
            _dbObjects = new object[] {org, _databaseAdminUser}.AsQueryable();
        }

        #region Set organisation as IN scope

        [Test]
        [Description("AdminController ManualChanges POST: Set Organisation as in scope successfully")]
        public async Task AdminController_ManualChanges_POST_Set_Organisation_As_In_Scope_Succeeds_When_Running_In_Live_Mode()
        {
            // Arrange (data for this test is slightly different than others in this class)
            Organisation organisationOutOfScopeFor2017 =
                OrganisationHelper.GetOrganisationOutOfScope(TestEmployerReference, TestSnapshotYear);
            _dbObjects = new object[] {organisationOutOfScopeFor2017, _databaseAdminUser}.AsQueryable();

            AdminController adminControllerForOutOfScopeTests = GetControllerForOutOfScopeTests(_dbObjects);

            var updateSearchIndexCalled = false;
            var mockedSearchBusinessLogic = new Mock<ISearchBusinessLogic>();
            mockedSearchBusinessLogic.Setup(r => r.UpdateSearchIndexAsync(It.IsAny<Organisation>()))
                .Returns(Task.CompletedTask)
                .Callback(() => updateSearchIndexCalled = true);

            adminControllerForOutOfScopeTests.SearchBusinessLogic = mockedSearchBusinessLogic.Object;

            ManualChangesViewModel manualChangesViewModelMockObject = ManualChangesViewModelHelper.GetMock(
                SetToInScopeTestCommand,
                $"{TestEmployerReference}={TestSnapshotYear},comment to be included in the in scope change");

            /* live */
            manualChangesViewModelMockObject.LastTestedCommand = manualChangesViewModelMockObject.Command;
            manualChangesViewModelMockObject.LastTestedInput = manualChangesViewModelMockObject.Parameters;

            // Act
            ManualChangesViewModel actualManualChangesViewModel = await ActOnTheAdminControllerManualChangesAsync(
                adminControllerForOutOfScopeTests,
                manualChangesViewModelMockObject);

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual(
                        actualManualChangesViewModel.SuccessMessage,
                        "SUCCESSFULLY EXECUTED 'Set organisation as in scope': 1 of 1");
                    Assert.AreEqual(
                        $"1: A1B2C3D4: has been set as 'InScope' for snapshotYear '{TestSnapshotYear}' with comment 'comment to be included in the in scope change'\r\n",
                        actualManualChangesViewModel.Results);
                    Assert.IsNull(actualManualChangesViewModel.LastTestedCommand);
                    Assert.IsNull(actualManualChangesViewModel.LastTestedInput);
                    Assert.False(actualManualChangesViewModel.Tested, "Must be tested=false as this case is running in LIVE mode");
                    Assert.IsNull(actualManualChangesViewModel.Comment);
                    Assert.IsTrue(
                        updateSearchIndexCalled,
                        "Expected this method to have called 'UpdateSearchIndex' on 'SearchBusinessLogic'");
                });

            var modifiedOrg = _dbObjects.FirstOrDefault(x => x.GetType() == typeof(Organisation)) as Organisation;
            Assert.NotNull(modifiedOrg, "Expected at least one organisation");

            OrganisationScope retiredScope =
                modifiedOrg.OrganisationScopes.FirstOrDefault(orgScope => orgScope.Status == ScopeRowStatuses.Retired);
            Assert.NotNull(retiredScope, "Expected that the previous scope was set to Retired");

            OrganisationScope newScope = modifiedOrg.OrganisationScopes.FirstOrDefault(
                orgScope =>
                    orgScope.Status == ScopeRowStatuses.Active && orgScope.SnapshotDate.Year == TestSnapshotYear);
            Assert.NotNull(newScope, "Expected the new scope to be present");
            Assert.AreEqual("comment to be included in the in scope change", newScope.Reason);
        }

        #endregion

        #region helpers for this class

        private static AdminController AdminController(out ManualChangesViewModel manualChangesViewModel,
            IEnumerable<Organisation> listOfOrganisations = null)
        {
            User databaseAdminUser = UserHelper.CreateUser("databaseadmin@email.com");

            var dbObjects = new List<object> {databaseAdminUser};
            if (listOfOrganisations != null)
            {
                dbObjects.Add(listOfOrganisations);
            }

            var controller = UiTestHelper.GetController<AdminController>(-1, null, dbObjects.ToArray());

            manualChangesViewModel = Mock.Of<ManualChangesViewModel>();

            return controller;
        }

        private AdminController GetControllerForOutOfScopeTests(params object[] databaseObjects)
        {
            var mockedController = UiTestHelper.GetController<AdminController>(_databaseAdminUser.UserId, null, databaseObjects);

            var scopeBusinessLogic = new ScopeBusinessLogic(
                UiTestHelper.DIContainer.Resolve<ICommonBusinessLogic>(),
                UiTestHelper.DIContainer.Resolve<IDataRepository>(),
                UiTestHelper.DIContainer.Resolve<ISearchBusinessLogic>());

            var organisationBusinessLogic = new OrganisationBusinessLogic(
                UiTestHelper.DIContainer.Resolve<ICommonBusinessLogic>(),
                UiTestHelper.DIContainer.Resolve<IDataRepository>(),
                UiTestHelper.DIContainer.Resolve<ISubmissionBusinessLogic>(),
                scopeBusinessLogic,
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

        #endregion

        #region Retire organisations

        [Test]
        public async Task AdminController_ManualChanges_POST_Retire_Organisations_Succeeds_When_Executing_In_Test_Mode()
        {
            // Arrange
            Organisation activeOrganisation =
                OrganisationHelper.GetConcreteOrganisation(
                    "Four_Letter_Employer_Reference_54",
                    "OrgFourLetter123_54",
                    OrganisationStatuses.Active,
                    new List<OrganisationStatus> {
                        new OrganisationStatus {Status = OrganisationStatuses.New, StatusDate = VirtualDateTime.Now.AddSeconds(-1)},
                        new OrganisationStatus {Status = OrganisationStatuses.Pending, StatusDate = VirtualDateTime.Now.AddSeconds(+1)}
                    });

            Organisation[] listOfOrganisations = {activeOrganisation};
            AdminController adminController = AdminController(out ManualChangesViewModel manualChangesViewModel, listOfOrganisations);
            manualChangesViewModel.Command = RetireOrganisationsCommand;
            manualChangesViewModel.Parameters = string.Join(Environment.NewLine, listOfOrganisations.Select(x => x.EmployerReference));

            // Act
            var retireOrgActionResult = await adminController.ManualChanges(manualChangesViewModel) as ViewResult;
            Assert.NotNull(retireOrgActionResult);
            var actualManualChangesViewModel = (ManualChangesViewModel) retireOrgActionResult.Model;

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual("SUCCESSFULLY TESTED 'Retire organisations': 1 of 1", actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual(
                        "1: FOUR_LETTER_EMPLOYER_REFERENCE_54: OrgFourLetter123_54 Status='Active' set to 'Retired'\r\n",
                        actualManualChangesViewModel.Results);
                    Assert.AreEqual("Retire organisations", actualManualChangesViewModel.LastTestedCommand);
                    Assert.AreEqual("Four_Letter_Employer_Reference_54", actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be true as this case is running in test mode");
                });
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Retire_Organisations_Succeeds_When_Executing_In_Live_Mode()
        {
            // Arrange
            Organisation activeOrganisation =
                OrganisationHelper.GetConcreteOrganisation(
                    "Four_Letter_Employer_Reference_774455",
                    "OrgFourLetter123_774455",
                    OrganisationStatuses.Active,
                    new List<OrganisationStatus> {
                        new OrganisationStatus {Status = OrganisationStatuses.New, StatusDate = VirtualDateTime.Now.AddSeconds(-1)},
                        new OrganisationStatus {Status = OrganisationStatuses.Pending, StatusDate = VirtualDateTime.Now.AddSeconds(+1)}
                    });

            Organisation[] listOfOrganisations = {activeOrganisation};
            AdminController adminController = AdminController(out ManualChangesViewModel manualChangesViewModel, listOfOrganisations);
            manualChangesViewModel.Command = RetireOrganisationsCommand;
            manualChangesViewModel.Parameters = string.Join(Environment.NewLine, listOfOrganisations.Select(x => x.EmployerReference));
            string expectedLastTestedInput = string.Join(";", listOfOrganisations.Select(x => x.EmployerReference));

            /* live */
            manualChangesViewModel.LastTestedCommand = manualChangesViewModel.Command;
            manualChangesViewModel.LastTestedInput = expectedLastTestedInput;

            // Act
            var retireOrgActionResult = await adminController.ManualChanges(manualChangesViewModel) as ViewResult;
            Assert.NotNull(retireOrgActionResult);
            var actualManualChangesViewModel = (ManualChangesViewModel) retireOrgActionResult.Model;

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual("SUCCESSFULLY EXECUTED 'Retire organisations': 1 of 1", actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual(
                        "1: FOUR_LETTER_EMPLOYER_REFERENCE_774455: OrgFourLetter123_774455 Status='Active' set to 'Retired'\r\n",
                        actualManualChangesViewModel.Results);
                    Assert.IsNull(actualManualChangesViewModel.LastTestedCommand);
                    Assert.IsNull(actualManualChangesViewModel.LastTestedInput);
                    Assert.False(actualManualChangesViewModel.Tested, "Must be false as this case is running in live mode");
                });
        }

        [TestCase("")]
        [TestCase(null)]
        public async Task AdminController_ManualChanges_POST_Retire_Organisations_Fails_When_No_Parameters_Are_Provided_Async(
            string nullOrEmptyParameter)
        {
            // Arrange
            AdminController adminController = AdminController(out ManualChangesViewModel manualChangesViewModel);
            manualChangesViewModel.Command = RetireOrganisationsCommand;
            manualChangesViewModel.Parameters = nullOrEmptyParameter; // this test requires null or empty parameters to be provided

            // Act
            var retireOrgActionResult = await adminController.ManualChanges(manualChangesViewModel) as ViewResult;
            Assert.NotNull(retireOrgActionResult);
            var actualManualChangesViewModel = (ManualChangesViewModel) retireOrgActionResult.Model;

            ModelStateEntry modelState = adminController.ModelState[""];
            ModelError reportedError = modelState.Errors.First();

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual("ERROR: You must supply 1 or more input parameters", reportedError.ErrorMessage);
                    Assert.IsNull(actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual("", actualManualChangesViewModel.Results);
                    Assert.IsNull(actualManualChangesViewModel.LastTestedCommand);
                    Assert.IsNull(actualManualChangesViewModel.LastTestedInput);
                    Assert.False(actualManualChangesViewModel.Tested, "Must be false as this case has failed");
                });
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Retire_Organisations_Fails_When_Employer_Reference_Is_Duplicated_Async()
        {
            // Arrange
            Organisation activeOrganisation =
                OrganisationHelper.GetConcreteOrganisation(
                    "Four_Letter_Employer_Reference_151",
                    "OrgFourLetter123_151",
                    OrganisationStatuses.Active,
                    new List<OrganisationStatus> {
                        new OrganisationStatus {Status = OrganisationStatuses.New, StatusDate = VirtualDateTime.Now.AddSeconds(-1)},
                        new OrganisationStatus {Status = OrganisationStatuses.Pending, StatusDate = VirtualDateTime.Now.AddSeconds(+1)}
                    });

            Organisation[] listOfOrganisations = {activeOrganisation};
            AdminController adminController = AdminController(out ManualChangesViewModel manualChangesViewModel, listOfOrganisations);
            manualChangesViewModel.Command = RetireOrganisationsCommand;
            manualChangesViewModel.Parameters = activeOrganisation.EmployerReference
                                                + Environment.NewLine
                                                + activeOrganisation.EmployerReference;

            // Act
            var retireOrgActionResult = await adminController.ManualChanges(manualChangesViewModel) as ViewResult;
            Assert.NotNull(retireOrgActionResult);
            var actualManualChangesViewModel = (ManualChangesViewModel) retireOrgActionResult.Model;

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual("SUCCESSFULLY TESTED 'Retire organisations': 1 of 2", actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual(
                        "1: FOUR_LETTER_EMPLOYER_REFERENCE_151: OrgFourLetter123_151 Status='Active' set to 'Retired'\r\n<span style=\"color:Red\">2: ERROR: 'FOUR_LETTER_EMPLOYER_REFERENCE_151' duplicate organisation</span>\r\n",
                        actualManualChangesViewModel.Results);
                    Assert.AreEqual("Retire organisations", actualManualChangesViewModel.LastTestedCommand);
                    Assert.AreEqual(
                        "Four_Letter_Employer_Reference_151;Four_Letter_Employer_Reference_151",
                        actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be true as this case is running in test mode");
                });
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Retire_Organisations_Fails_When_Parameters_Contain_An_Equals_Sign()
        {
            // Arrange
            Organisation activeOrganisation =
                OrganisationHelper.GetConcreteOrganisation(
                    "Four_Letter_Employer_Reference_177",
                    "OrgFourLetter123_177",
                    OrganisationStatuses.Active,
                    new List<OrganisationStatus> {
                        new OrganisationStatus {Status = OrganisationStatuses.New, StatusDate = VirtualDateTime.Now.AddSeconds(-1)},
                        new OrganisationStatus {Status = OrganisationStatuses.Pending, StatusDate = VirtualDateTime.Now.AddSeconds(+1)}
                    });

            Organisation[] listOfOrganisations = {activeOrganisation};
            AdminController adminController = AdminController(out ManualChangesViewModel manualChangesViewModel, listOfOrganisations);
            manualChangesViewModel.Command = RetireOrganisationsCommand;
            manualChangesViewModel.Parameters = "=" + activeOrganisation.EmployerReference;

            // Act
            var retireOrgActionResult = await adminController.ManualChanges(manualChangesViewModel) as ViewResult;
            Assert.NotNull(retireOrgActionResult);
            var actualManualChangesViewModel = (ManualChangesViewModel) retireOrgActionResult.Model;

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual("SUCCESSFULLY TESTED 'Retire organisations': 0 of 1", actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual(
                        "<span style=\"color:Red\">1: ERROR: '=Four_Letter_Employer_Reference_177' contains '=' character</span>\r\n",
                        actualManualChangesViewModel.Results);
                    Assert.AreEqual("Retire organisations", actualManualChangesViewModel.LastTestedCommand);
                    Assert.AreEqual("=Four_Letter_Employer_Reference_177", actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be true as this case is running in test mode");
                });
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Retire_Organisations_Fails_When_Reference_Is_Not_On_The_Database_Async()
        {
            // Arrange
            AdminController adminController = AdminController(out ManualChangesViewModel manualChangesViewModel);
            manualChangesViewModel.Command = RetireOrganisationsCommand;
            manualChangesViewModel.Parameters = Environment.NewLine
                                                + "   " // empty lines must not break the processing
                                                + Environment.NewLine // null lines must not break the processing 
                                                + Environment.NewLine // null lines must not break the processing
                                                + "Reference_Not_On_Database";

            // Act
            var retireOrgActionResult = await adminController.ManualChanges(manualChangesViewModel) as ViewResult;
            Assert.NotNull(retireOrgActionResult);
            var actualManualChangesViewModel = (ManualChangesViewModel) retireOrgActionResult.Model;

            // Assert
            Assert.AreEqual("SUCCESSFULLY TESTED 'Retire organisations': 0 of 2", actualManualChangesViewModel.SuccessMessage);
            Assert.AreEqual(
                "<span style=\"color:Red\">2: ERROR: 'REFERENCE_NOT_ON_DATABASE' Cannot find organisation with this employer reference</span>\r\n",
                actualManualChangesViewModel.Results); // Note "2: ERROR" instead of "1: ERROR" here
            Assert.AreEqual("Retire organisations", actualManualChangesViewModel.LastTestedCommand);
            Assert.AreEqual(";   ;;Reference_Not_On_Database", actualManualChangesViewModel.LastTestedInput);
            Assert.True(actualManualChangesViewModel.Tested, "Must be true as this case is running in test mode");
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Retire_Organisations_Fails_When_Employer_Is_Already_Retired()
        {
            // Arrange
            Organisation activeOrganisation =
                OrganisationHelper.GetConcreteOrganisation(
                    "Four_Letter_Employer_Reference_1878",
                    "OrgFourLetter123_1878",
                    OrganisationStatuses.Retired, // already retired - guarantees the error message expected by this test
                    new List<OrganisationStatus> {
                        new OrganisationStatus {Status = OrganisationStatuses.New, StatusDate = VirtualDateTime.Now.AddSeconds(-1)},
                        new OrganisationStatus {Status = OrganisationStatuses.Pending, StatusDate = VirtualDateTime.Now.AddSeconds(+1)}
                    });

            Organisation[] listOfOrganisations = {activeOrganisation};
            AdminController adminController = AdminController(out ManualChangesViewModel manualChangesViewModel, listOfOrganisations);
            manualChangesViewModel.Command = RetireOrganisationsCommand;
            manualChangesViewModel.Parameters = activeOrganisation.EmployerReference;

            // Act
            var retireOrgActionResult = await adminController.ManualChanges(manualChangesViewModel) as ViewResult;
            Assert.NotNull(retireOrgActionResult);
            var actualManualChangesViewModel = (ManualChangesViewModel) retireOrgActionResult.Model;

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual("SUCCESSFULLY TESTED 'Retire organisations': 1 of 1", actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual(
                        "<span style=\"color:Orange\">1: WARNING: 'FOUR_LETTER_EMPLOYER_REFERENCE_1878' 'OrgFourLetter123_1878' Status='Retired' already set to 'Retired'</span>\r\n",
                        actualManualChangesViewModel.Results);
                    Assert.AreEqual("Retire organisations", actualManualChangesViewModel.LastTestedCommand);
                    Assert.AreEqual("Four_Letter_Employer_Reference_1878", actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be true as this case is running in test mode");
                });
        }

        #endregion

        #region Unretire organisations

        [Test]
        [Description(
            "AdminController ManualChanges POST: UnRetireOrganisations successfully sets an organisation as 'retired' when executing in TEST mode")]
        public async Task AdminController_ManualChanges_POST_UnRetire_Organisations_Succeeds_When_Executing_In_Test_Mode()
        {
            // Arrange
            Organisation retiredOrganisationThatWasActive =
                OrganisationHelper.GetConcreteOrganisation(
                    "Four_Letter_Employer_Reference",
                    "OrgFourLetter123",
                    OrganisationStatuses.Retired,
                    new List<OrganisationStatus> {
                        new OrganisationStatus {Status = OrganisationStatuses.Active, StatusDate = VirtualDateTime.Now.AddSeconds(-1)},
                        new OrganisationStatus {Status = OrganisationStatuses.Retired, StatusDate = VirtualDateTime.Now.AddSeconds(+1)}
                    });

            Organisation activeOrganisation =
                OrganisationHelper.GetConcreteOrganisation(
                    "Four_Letter_Employer_Reference_2",
                    "OrgFourLetter123_2",
                    OrganisationStatuses.Active,
                    new List<OrganisationStatus> {
                        new OrganisationStatus {Status = OrganisationStatuses.New, StatusDate = VirtualDateTime.Now.AddSeconds(-1)},
                        new OrganisationStatus {Status = OrganisationStatuses.Pending, StatusDate = VirtualDateTime.Now.AddSeconds(+1)}
                    });

            Organisation retiredOrganisationThatWasPending =
                OrganisationHelper.GetConcreteOrganisation(
                    "Four_Letter_Employer_Reference_3",
                    "OrgFourLetter123_3",
                    OrganisationStatuses.Retired,
                    new List<OrganisationStatus> {
                        new OrganisationStatus {Status = OrganisationStatuses.Pending, StatusDate = VirtualDateTime.Now.AddSeconds(-1)},
                        new OrganisationStatus {Status = OrganisationStatuses.Retired, StatusDate = VirtualDateTime.Now.AddSeconds(+1)}
                    });

            Organisation[] listOfOrganisations = {retiredOrganisationThatWasActive, activeOrganisation, retiredOrganisationThatWasPending};
            AdminController adminController = AdminController(out ManualChangesViewModel manualChangesViewModel, listOfOrganisations);
            manualChangesViewModel.Command = UnretireOrganisationsCommand;
            manualChangesViewModel.Parameters = string.Join(Environment.NewLine, listOfOrganisations.Select(x => x.EmployerReference));
            string expectedLastTestedInput = string.Join(";", listOfOrganisations.Select(x => x.EmployerReference));

            // Act
            var unRetireOrgActionResult = await adminController.ManualChanges(manualChangesViewModel) as ViewResult;
            var actualManualChangesViewModel = (ManualChangesViewModel) unRetireOrgActionResult.Model;

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual("SUCCESSFULLY TESTED 'Unretire organisations': 2 of 3", actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual(
                        "1: FOUR_LETTER_EMPLOYER_REFERENCE: OrgFourLetter123 reverted from status 'Retired' to 'Active'\r\n<span style=\"color:Red\">2: ERROR: Only organisations with current status 'Retired' are allowed to be reverted, Organisation 'OrgFourLetter123_2' employerReference 'Four_Letter_Employer_Reference_2' has status 'Active'.</span>\r\n3: FOUR_LETTER_EMPLOYER_REFERENCE_3: OrgFourLetter123_3 reverted from status 'Retired' to 'Pending'\r\n",
                        actualManualChangesViewModel.Results);
                    Assert.AreEqual("Unretire organisations", actualManualChangesViewModel.LastTestedCommand);
                    Assert.AreEqual(expectedLastTestedInput, actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be true as this case is running in test mode");
                });
        }

        [Test]
        [Description(
            "AdminController ManualChanges POST: UnRetireRetireOrganisations successfully sets an organisation as 'retired' when executing in LIVE mode")]
        public async Task AdminController_ManualChanges_POST_UnRetire_Organisations_Succeeds_When_Executing_In_Live_ModeAsync()
        {
            // Arrange
            Organisation retiredOrganisationThatWasActive =
                OrganisationHelper.GetConcreteOrganisation(
                    "Four_Letter_Employer_Reference",
                    "OrgFourLetter123",
                    OrganisationStatuses.Retired,
                    new List<OrganisationStatus> {
                        new OrganisationStatus {Status = OrganisationStatuses.Active, StatusDate = VirtualDateTime.Now.AddSeconds(-1)},
                        new OrganisationStatus {Status = OrganisationStatuses.Retired, StatusDate = VirtualDateTime.Now.AddSeconds(+1)}
                    });

            Organisation activeOrganisation =
                OrganisationHelper.GetConcreteOrganisation(
                    "Four_Letter_Employer_Reference_2",
                    "OrgFourLetter123_2",
                    OrganisationStatuses.Active,
                    new List<OrganisationStatus> {
                        new OrganisationStatus {Status = OrganisationStatuses.New, StatusDate = VirtualDateTime.Now.AddSeconds(-1)},
                        new OrganisationStatus {Status = OrganisationStatuses.Pending, StatusDate = VirtualDateTime.Now.AddSeconds(+1)}
                    });

            Organisation retiredOrganisationThatWasPending =
                OrganisationHelper.GetConcreteOrganisation(
                    "Four_Letter_Employer_Reference_3",
                    "OrgFourLetter123_3",
                    OrganisationStatuses.Retired,
                    new List<OrganisationStatus> {
                        new OrganisationStatus {Status = OrganisationStatuses.Pending, StatusDate = VirtualDateTime.Now.AddSeconds(-1)},
                        new OrganisationStatus {Status = OrganisationStatuses.Retired, StatusDate = VirtualDateTime.Now.AddSeconds(+1)}
                    });

            Organisation[] listOfOrganisations = {retiredOrganisationThatWasActive, activeOrganisation, retiredOrganisationThatWasPending};
            AdminController adminController = AdminController(out ManualChangesViewModel manualChangesViewModel, listOfOrganisations);
            manualChangesViewModel.Command = UnretireOrganisationsCommand;
            manualChangesViewModel.Parameters = string.Join(Environment.NewLine, listOfOrganisations.Select(x => x.EmployerReference));
            string expectedLastTestedInput = string.Join(";", listOfOrganisations.Select(x => x.EmployerReference));

            /* live */
            manualChangesViewModel.LastTestedCommand = manualChangesViewModel.Command;
            manualChangesViewModel.LastTestedInput = expectedLastTestedInput;

            // Act
            var retireOrgActionResult = await adminController.ManualChanges(manualChangesViewModel) as ViewResult;
            Assert.NotNull(retireOrgActionResult);
            var actualManualChangesViewModel = retireOrgActionResult.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel);

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual("SUCCESSFULLY EXECUTED 'Unretire organisations': 2 of 3", actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual(
                        "1: FOUR_LETTER_EMPLOYER_REFERENCE: OrgFourLetter123 reverted from status 'Retired' to 'Active'\r\n<span style=\"color:Red\">2: ERROR: Only organisations with current status 'Retired' are allowed to be reverted, Organisation 'OrgFourLetter123_2' employerReference 'Four_Letter_Employer_Reference_2' has status 'Active'.</span>\r\n3: FOUR_LETTER_EMPLOYER_REFERENCE_3: OrgFourLetter123_3 reverted from status 'Retired' to 'Pending'\r\n",
                        actualManualChangesViewModel.Results);
                    Assert.IsNull(actualManualChangesViewModel.LastTestedCommand);
                    Assert.IsNull(actualManualChangesViewModel.LastTestedInput);
                    Assert.False(actualManualChangesViewModel.Tested, "Must be false as this case is running in LIVE mode");
                });
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_UnRetire_Organisations_Fails_When_An_Equals_Sign_Is_Sent_Async()
        {
            // Arrange
            AdminController adminControllerForOutOfScopeTests = GetControllerForOutOfScopeTests(_dbObjects);

            ManualChangesViewModel manualChangesViewModelMockObject = ManualChangesViewModelHelper.GetMock(
                UnretireOrganisationsCommand,
                $"{TestEmployerReference}=blah");

            // Act
            ManualChangesViewModel actualManualChangesViewModel = await ActOnTheAdminControllerManualChangesAsync(
                adminControllerForOutOfScopeTests,
                manualChangesViewModelMockObject);

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual(actualManualChangesViewModel.SuccessMessage, "SUCCESSFULLY TESTED 'Unretire organisations': 0 of 1");
                    Assert.AreEqual(
                        "<span style=\"color:Red\">1: ERROR: 'A1B2C3D4=blah' contains '=' character</span>\r\n",
                        actualManualChangesViewModel.Results);
                    Assert.AreEqual("Unretire organisations", actualManualChangesViewModel.LastTestedCommand);
                    Assert.AreEqual("A1B2C3D4=blah", actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be true as this case is running in test mode");
                    Assert.Null(actualManualChangesViewModel.Comment);
                });
        }

        [TestCase("")]
        [TestCase(null)]
        public async Task AdminController_ManualChanges_POST_UnRetire_Organisations_Fails_When_No_Parameters_Are_Supplied_Async(
            string nullOrEmptyParameter)
        {
            // Arrange
            AdminController adminControllerForOutOfScopeTests = GetControllerForOutOfScopeTests(_dbObjects);

            ManualChangesViewModel manualChangesViewModelMockObject =
                ManualChangesViewModelHelper.GetMock(
                    UnretireOrganisationsCommand,
                    nullOrEmptyParameter); // this test requires null or empty parameters to be provided

            // Act
            ManualChangesViewModel actualManualChangesViewModel = await ActOnTheAdminControllerManualChangesAsync(
                adminControllerForOutOfScopeTests,
                manualChangesViewModelMockObject);

            ModelStateEntry modelState = adminControllerForOutOfScopeTests.ModelState[""];
            ModelError reportedError = modelState.Errors.First();

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual("ERROR: You must supply 1 or more input parameters", reportedError.ErrorMessage);
                    Assert.IsNull(actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual("", actualManualChangesViewModel.Results);
                    Assert.IsNull(actualManualChangesViewModel.LastTestedCommand);
                    Assert.IsNull(actualManualChangesViewModel.LastTestedInput);
                    Assert.IsFalse(actualManualChangesViewModel.Tested);
                    Assert.IsNull(actualManualChangesViewModel.Comment);
                });
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_UnRetire_Organisations_Fails_When_EmployerReference_Is_Not_Supplied()
        {
            // Arrange
            AdminController adminControllerForOutOfScopeTests = GetControllerForOutOfScopeTests(_dbObjects);

            ManualChangesViewModel manualChangesViewModelMockObject = ManualChangesViewModelHelper.GetMock(
                UnretireOrganisationsCommand,
                $"ABCD1234{Environment.NewLine} {Environment.NewLine}EFGH5678");

            // Act
            ManualChangesViewModel actualManualChangesViewModel = await ActOnTheAdminControllerManualChangesAsync(
                adminControllerForOutOfScopeTests,
                manualChangesViewModelMockObject);

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual("SUCCESSFULLY TESTED 'Unretire organisations': 0 of 3", actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual(
                        "<span style=\"color:Red\">1: ERROR: 'ABCD1234' Cannot find organisation with this employer reference</span>\r\n<span style=\"color:Red\">3: ERROR: 'EFGH5678' Cannot find organisation with this employer reference</span>\r\n",
                        actualManualChangesViewModel.Results);
                    Assert.AreEqual("Unretire organisations", actualManualChangesViewModel.LastTestedCommand);
                    Assert.AreEqual("ABCD1234; ;EFGH5678", actualManualChangesViewModel.LastTestedInput);
                    Assert.IsTrue(actualManualChangesViewModel.Tested);
                    Assert.IsNull(actualManualChangesViewModel.Comment);
                });
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_UnRetire_Organisations_Fails_When_EmployerReference_Is_Duplicated_Async()
        {
            // Arrange
            AdminController adminControllerForOutOfScopeTests = GetControllerForOutOfScopeTests(_dbObjects);

            ManualChangesViewModel manualChangesViewModelMockObject = ManualChangesViewModelHelper.GetMock(
                UnretireOrganisationsCommand,
                $"A1B2C3D4{Environment.NewLine}A1B2C3D4");

            // Act
            ManualChangesViewModel actualManualChangesViewModel = await ActOnTheAdminControllerManualChangesAsync(
                adminControllerForOutOfScopeTests,
                manualChangesViewModelMockObject);

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual("SUCCESSFULLY TESTED 'Unretire organisations': 0 of 2", actualManualChangesViewModel.SuccessMessage);
                    Assert.IsTrue(actualManualChangesViewModel.Results.Contains("2: ERROR: 'A1B2C3D4' duplicate organisation"));
                    Assert.AreEqual("Unretire organisations", actualManualChangesViewModel.LastTestedCommand);
                    Assert.AreEqual("A1B2C3D4;A1B2C3D4", actualManualChangesViewModel.LastTestedInput);
                    Assert.IsTrue(actualManualChangesViewModel.Tested);
                    Assert.IsNull(actualManualChangesViewModel.Comment);
                });
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_UnRetire_Organisations_Exceptions_Are_Caught_And_Reported()
        {
            // Arrange

            #region Configure organisation

            var mockOrg = new Mock<Organisation>();
            mockOrg.Setup(o => o.UnRetire(It.IsAny<long>(), It.IsAny<string>()))
                .Throws(new Exception("Testing exception handling"));
            Organisation org = mockOrg.Object;
            org.EmployerReference = TestEmployerReference;

            #endregion

            AdminController adminControllerForOutOfScopeTests = GetControllerForOutOfScopeTests(null);
            ManualChangesViewModel manualChangesViewModelMockObject = ManualChangesViewModelHelper.GetMock(
                UnretireOrganisationsCommand,
                "A1B2C3D4");

            #region Configure DataRepository

            Mock<IDataRepository> dataRepo = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            dataRepo.SetupGetAll(org);
            dataRepo.Setup(x => x.Get<User>(It.IsAny<long>())).Returns(_databaseAdminUser);

            #endregion

            // Act
            ManualChangesViewModel actualManualChangesViewModel = await ActOnTheAdminControllerManualChangesAsync(
                adminControllerForOutOfScopeTests,
                manualChangesViewModelMockObject);

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual("SUCCESSFULLY TESTED 'Unretire organisations': 0 of 1", actualManualChangesViewModel.SuccessMessage);
                    Assert.IsTrue(
                        actualManualChangesViewModel.Results.Contains(
                            "<span style=\"color:Red\">1: ERROR: 'A1B2C3D4' Testing exception handling</span>\r\n"));
                    Assert.AreEqual("Unretire organisations", actualManualChangesViewModel.LastTestedCommand);
                    Assert.AreEqual("A1B2C3D4", actualManualChangesViewModel.LastTestedInput);
                    Assert.IsTrue(actualManualChangesViewModel.Tested);
                    Assert.IsNull(actualManualChangesViewModel.Comment);
                });
        }

        #endregion

        #region Set organisation as OUT of scope

        [TestCase(
            "= missing employer reference",
            "<span style=\"color:Red\">1: ERROR: '= missing employer reference' expected an employer reference before the '=' character (i.e. EmployerReference=SnapshotYear,Comment to add to the scope change for this particular employer)</span>\r\n")]
        [TestCase(
            "EmployerReference=1999,",
            "<span style=\"color:Red\">1: ERROR: 'EMPLOYERREFERENCE' please enter a comment in the comments section of this page</span>\r\n")]
        public async Task
            AdminController_ManualChanges_POST_Set_Organisation_As_Out_Of_Scope_Fails_When_No_Employer_Reference_Is_Supplied_Async(
                string parametersRequired,
                string expectedResultMessage)
        {
            // Arrange
            AdminController adminControllerForOutOfScopeTests = GetControllerForOutOfScopeTests(_dbObjects);

            ManualChangesViewModel manualChangesViewModelMockObject =
                ManualChangesViewModelHelper.GetMock(SetToOutOfScopeTestCommand, parametersRequired);

            // Act
            ManualChangesViewModel actualManualChangesViewModel = await ActOnTheAdminControllerManualChangesAsync(
                adminControllerForOutOfScopeTests,
                manualChangesViewModelMockObject);

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual(
                        "SUCCESSFULLY TESTED 'Set organisation as out of scope': 0 of 1",
                        actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual(expectedResultMessage, actualManualChangesViewModel.Results);
                    Assert.AreEqual("Set organisation as out of scope", actualManualChangesViewModel.LastTestedCommand);
                    Assert.AreEqual(parametersRequired, actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be true as this case is running in test mode");
                });
        }

        [TestCase("")]
        [TestCase(null)]
        public async Task AdminController_ManualChanges_POST_Set_Organisation_As_Out_Of_Scope_Fails_When_No_Parameters_Are_Supplied_Async(
            string nullOrEmptyParameter)
        {
            // Arrange
            AdminController adminControllerForOutOfScopeTests = GetControllerForOutOfScopeTests(_dbObjects);

            ManualChangesViewModel manualChangesViewModelMockObject =
                ManualChangesViewModelHelper.GetMock(
                    SetToOutOfScopeTestCommand,
                    nullOrEmptyParameter); // this test requires null or empty parameters to be provided

            // Act
            ManualChangesViewModel actualManualChangesViewModel = await ActOnTheAdminControllerManualChangesAsync(
                adminControllerForOutOfScopeTests,
                manualChangesViewModelMockObject);

            ModelStateEntry modelState = adminControllerForOutOfScopeTests.ModelState[""];
            ModelError reportedError = modelState.Errors.First();

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual("ERROR: You must supply 1 or more input parameters", reportedError.ErrorMessage);
                    Assert.IsNull(actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual("", actualManualChangesViewModel.Results);
                    Assert.IsNull(actualManualChangesViewModel.LastTestedCommand);
                    Assert.IsNull(actualManualChangesViewModel.LastTestedInput);
                    Assert.IsFalse(actualManualChangesViewModel.Tested);
                    Assert.IsNull(actualManualChangesViewModel.Comment);
                });
        }

        [Test]
        public async Task
            AdminController_ManualChanges_POST_Set_Organisation_As_Out_Of_Scope_Selects_Current_Year_When_Snapshot_Date_Is_Missing()
        {
            // Arrange (data for this test is slightly different than others in this class)
            int snapshotYear = VirtualDateTime.Now.Year;
            Organisation organisationInScopeForCurrentYear =
                OrganisationHelper.GetOrganisationOutOfScope(TestEmployerReference, snapshotYear);
            _dbObjects = new object[] {organisationInScopeForCurrentYear, _databaseAdminUser}.AsQueryable();

            AdminController adminControllerForOutOfScopeTests = GetControllerForOutOfScopeTests(_dbObjects);
            string
                parametersWithoutAComment =
                    $"{TestEmployerReference}=,"; // nothing before the comma, so it's expected to pick up 'current year' as the snapshot date
            ManualChangesViewModel manualChangesViewModelMockObject = ManualChangesViewModelHelper.GetMock(
                SetToOutOfScopeTestCommand,
                parametersWithoutAComment,
                "Generic comment for all tests");

            /* live */
            manualChangesViewModelMockObject.LastTestedCommand = manualChangesViewModelMockObject.Command;
            manualChangesViewModelMockObject.LastTestedInput = manualChangesViewModelMockObject.Parameters;

            // Act
            await ActOnTheAdminControllerManualChangesAsync(adminControllerForOutOfScopeTests, manualChangesViewModelMockObject);

            var modifiedOrg = _dbObjects.FirstOrDefault(x => x.GetType() == typeof(Organisation)) as Organisation;
            Assert.NotNull(modifiedOrg, "Expected at least one organisation");

            OrganisationScope newScope = modifiedOrg.OrganisationScopes.FirstOrDefault(
                orgScope =>
                    orgScope.Status == ScopeRowStatuses.Active && orgScope.SnapshotDate.Year == snapshotYear);
            Assert.NotNull(newScope, "Expected the new scope to be present");

            // Assert
            Assert.AreEqual(
                VirtualDateTime.Now.Year,
                newScope.SnapshotDate.Year,
                "When the snapshot is not provided, the application should default to the current year.");
        }

        [Test]
        public async Task
            AdminController_ManualChanges_POST_Set_Organisation_As_Out_Of_Scope_Selects_Generic_Comment_When_Individual_Comment_Is_Missing()
        {
            // Arrange
            AdminController adminControllerForOutOfScopeTests = GetControllerForOutOfScopeTests(_dbObjects);
            string parametersWithoutAComment = $"{TestEmployerReference}={TestSnapshotYear},"; // nothing after the comma 
            ManualChangesViewModel manualChangesViewModelMockObject = ManualChangesViewModelHelper.GetMock(
                SetToOutOfScopeTestCommand,
                parametersWithoutAComment,
                "Generic comment for all tests");

            /* live */
            manualChangesViewModelMockObject.LastTestedCommand = manualChangesViewModelMockObject.Command;
            manualChangesViewModelMockObject.LastTestedInput = manualChangesViewModelMockObject.Parameters;

            // Act
            await ActOnTheAdminControllerManualChangesAsync(adminControllerForOutOfScopeTests, manualChangesViewModelMockObject);

            var modifiedOrg = _dbObjects.FirstOrDefault(x => x.GetType() == typeof(Organisation)) as Organisation;
            Assert.NotNull(modifiedOrg, "Expected at least one organisation");

            OrganisationScope newScope = modifiedOrg.OrganisationScopes.FirstOrDefault(
                orgScope =>
                    orgScope.Status == ScopeRowStatuses.Active && orgScope.SnapshotDate.Year == TestSnapshotYear);
            Assert.NotNull(newScope, "Expected the new scope to be present");

            // Assert
            Assert.AreEqual("Generic comment for all tests", newScope.Reason);
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Set_Organisation_As_Out_Of_Scope_Throws_Exception_When_Changing_ToSameScope()
        {
            // Arrange (data for this test is slightly different than others in this class)
            Organisation organisationOutOfScopeFor2017 =
                OrganisationHelper.GetOrganisationOutOfScope(TestEmployerReference, TestSnapshotYear);
            _dbObjects = new object[] {organisationOutOfScopeFor2017, _databaseAdminUser}.AsQueryable();

            AdminController adminControllerForOutOfScopeTests = GetControllerForOutOfScopeTests(_dbObjects);

            ManualChangesViewModel manualChangesViewModelMockObject = ManualChangesViewModelHelper.GetMock(
                SetToOutOfScopeTestCommand,
                $"{TestEmployerReference}={TestSnapshotYear},comment to be included in the out of scope");

            // Act
            ManualChangesViewModel actualManualChangesViewModel = await ActOnTheAdminControllerManualChangesAsync(
                adminControllerForOutOfScopeTests,
                manualChangesViewModelMockObject);

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual(
                        "SUCCESSFULLY TESTED 'Set organisation as out of scope': 0 of 1",
                        actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual(
                        $"<span style=\"color:Red\">1: ERROR: 'A1B2C3D4' Unable to update to OutOfScope as the record for {TestSnapshotYear} is already showing as OutOfScope</span>\r\n",
                        actualManualChangesViewModel.Results);
                    Assert.AreEqual(SetToOutOfScopeTestCommand, actualManualChangesViewModel.LastTestedCommand);
                    Assert.AreEqual(
                        $"A1B2C3D4={TestSnapshotYear},comment to be included in the out of scope",
                        actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be true as this case is running in test mode");
                    Assert.Null(actualManualChangesViewModel.Comment);
                });
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Set_Organisation_As_Out_Of_Scope_Succeeds_When_Running_In_Live_Mode()
        {
            // Arrange
            AdminController adminControllerForOutOfScopeTests = GetControllerForOutOfScopeTests(_dbObjects);

            var updateSearchIndexCalled = false;
            var mockedSearchBusinessLogic = new Mock<ISearchBusinessLogic>();
            mockedSearchBusinessLogic.Setup(r => r.UpdateSearchIndexAsync(It.IsAny<Organisation>()))
                .Returns(Task.CompletedTask)
                .Callback(() => updateSearchIndexCalled = true);

            adminControllerForOutOfScopeTests.SearchBusinessLogic = mockedSearchBusinessLogic.Object;

            ManualChangesViewModel manualChangesViewModelMockObject = ManualChangesViewModelHelper.GetMock(
                SetToOutOfScopeTestCommand,
                $"{TestEmployerReference}={TestSnapshotYear},comment to be included in the out of scope");

            /* live */
            manualChangesViewModelMockObject.LastTestedCommand = manualChangesViewModelMockObject.Command;
            manualChangesViewModelMockObject.LastTestedInput = manualChangesViewModelMockObject.Parameters;

            // Act
            ManualChangesViewModel actualManualChangesViewModel = await ActOnTheAdminControllerManualChangesAsync(
                adminControllerForOutOfScopeTests,
                manualChangesViewModelMockObject);

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual(
                        actualManualChangesViewModel.SuccessMessage,
                        "SUCCESSFULLY EXECUTED 'Set organisation as out of scope': 1 of 1");
                    Assert.AreEqual(
                        $"1: A1B2C3D4: has been set as 'OutOfScope' for snapshotYear '{TestSnapshotYear}' with comment 'comment to be included in the out of scope'\r\n",
                        actualManualChangesViewModel.Results);
                    Assert.IsNull(actualManualChangesViewModel.LastTestedCommand);
                    Assert.IsNull(actualManualChangesViewModel.LastTestedInput);
                    Assert.False(actualManualChangesViewModel.Tested, "Must be tested=false as this case is running in LIVE mode");
                    Assert.IsNull(actualManualChangesViewModel.Comment);
                    Assert.IsTrue(
                        updateSearchIndexCalled,
                        "Expected this method to have called 'UpdateSearchIndex' on 'SearchBusinessLogic'");
                });

            var modifiedOrg = _dbObjects.FirstOrDefault(x => x.GetType() == typeof(Organisation)) as Organisation;
            Assert.NotNull(modifiedOrg, "Expected at least one organisation");

            OrganisationScope retiredScope =
                modifiedOrg.OrganisationScopes.FirstOrDefault(orgScope => orgScope.Status == ScopeRowStatuses.Retired);
            Assert.NotNull(retiredScope, "Expected that the previous scope was set to Retired");

            OrganisationScope notRetiredScope = modifiedOrg.OrganisationScopes.FirstOrDefault(
                orgScope => orgScope.Status != ScopeRowStatuses.Retired && orgScope.SnapshotDate.Year == 2001);
            Assert.NotNull(notRetiredScope, "Expected that the scope for year 2001 will NOT change to Retired");

            OrganisationScope newScope = modifiedOrg.OrganisationScopes.FirstOrDefault(
                orgScope =>
                    orgScope.Status == ScopeRowStatuses.Active && orgScope.SnapshotDate.Year == TestSnapshotYear);
            Assert.Multiple(
                () => {
                    Assert.NotNull(newScope, "Expected the new scope to be present");
                    Assert.AreEqual("comment to be included in the out of scope", newScope.Reason);
                    Assert.AreEqual(_databaseAdminUser.EmailAddress, newScope.ContactEmailAddress);
                    Assert.AreEqual(_databaseAdminUser.Firstname, newScope.ContactFirstname);
                    Assert.AreEqual(_databaseAdminUser.Lastname, newScope.ContactLastname);
                });
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Set_Organisation_As_Out_Of_Scope_Succeeds_When_Running_In_Test_Mode()
        {
            // Arrange
            AdminController adminControllerForOutOfScopeTests = GetControllerForOutOfScopeTests(_dbObjects);

            ManualChangesViewModel manualChangesViewModelMockObject = ManualChangesViewModelHelper.GetMock(
                SetToOutOfScopeTestCommand,
                $"{TestEmployerReference}={TestSnapshotYear},comment to be included in the out of scope");

            // Act
            ManualChangesViewModel actualManualChangesViewModel = await ActOnTheAdminControllerManualChangesAsync(
                adminControllerForOutOfScopeTests,
                manualChangesViewModelMockObject);

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual(
                        actualManualChangesViewModel.SuccessMessage,
                        "SUCCESSFULLY TESTED 'Set organisation as out of scope': 1 of 1");
                    Assert.AreEqual(
                        $"1: A1B2C3D4: will be set as 'OutOfScope' for snapshotYear '{TestSnapshotYear}' with comment 'comment to be included in the out of scope'\r\n",
                        actualManualChangesViewModel.Results);
                    Assert.AreEqual(SetToOutOfScopeTestCommand, actualManualChangesViewModel.LastTestedCommand);
                    Assert.AreEqual(
                        $"A1B2C3D4={TestSnapshotYear},comment to be included in the out of scope",
                        actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be true as this case is running in test mode");
                    Assert.Null(actualManualChangesViewModel.Comment);
                });
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Set_Organisation_As_Out_Of_Scope_When_Employer_Reference_Is_Duplicated_Async()
        {
            // Arrange
            AdminController adminControllerForOutOfScopeTests = GetControllerForOutOfScopeTests(_dbObjects);
            string inputParametersToBeTested = $"{TestEmployerReference}={TestSnapshotYear},comment to be included in the out of scope"
                                               + Environment.NewLine
                                               + $"{TestEmployerReference}={TestSnapshotYear},comment to be included in the out of scope";
            ManualChangesViewModel manualChangesViewModelMockObject =
                ManualChangesViewModelHelper.GetMock(SetToOutOfScopeTestCommand, inputParametersToBeTested);

            // Act
            ManualChangesViewModel actualManualChangesViewModel = await ActOnTheAdminControllerManualChangesAsync(
                adminControllerForOutOfScopeTests,
                manualChangesViewModelMockObject);

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual(
                        "SUCCESSFULLY TESTED 'Set organisation as out of scope': 1 of 2",
                        actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual(
                        $"1: {TestEmployerReference}: will be set as 'OutOfScope' for snapshotYear '{TestSnapshotYear}' with comment 'comment to be included in the out of scope'\r\n<span style=\"color:Red\">2: ERROR: '{TestEmployerReference}' duplicate organisation</span>\r\n",
                        actualManualChangesViewModel.Results);
                    Assert.AreEqual("Set organisation as out of scope", actualManualChangesViewModel.LastTestedCommand);
                    Assert.AreEqual(
                        $"{TestEmployerReference}={TestSnapshotYear},comment to be included in the out of scope;{TestEmployerReference}={TestSnapshotYear},comment to be included in the out of scope",
                        actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be true as this case is running in test mode");
                    Assert.Null(actualManualChangesViewModel.Comment);
                });
        }

        [TestCase(
            "={lastYear}",
            "currentYear{lastYear}",
            "1: A1B2C3D4: has been set as 'OutOfScope' for snapshotYear '{lastYear}' with comment 'Generic comment for all tests'\r\n")]
        [TestCase(
            "={lastYear},",
            "currentYear{lastYear}",
            "1: A1B2C3D4: has been set as 'OutOfScope' for snapshotYear '{lastYear}' with comment 'Generic comment for all tests'\r\n")] // will // default to use the generic comment
        [TestCase(
            "=just a comment without comma",
            "nextYear{thisYear}",
            "1: A1B2C3D4: has been set as 'OutOfScope' for snapshotYear '{thisYear}' with comment 'just a comment without comma'\r\n")]
        [TestCase(
            "=,comment and empty date",
            "nextYear{thisYear}",
            "1: A1B2C3D4: has been set as 'OutOfScope' for snapshotYear '{thisYear}' with comment 'comment and empty date'\r\n")]
        [TestCase(
            "=",
            "nextYear{thisYear}",
            "1: A1B2C3D4: has been set as 'OutOfScope' for snapshotYear '{thisYear}' with comment 'Generic comment for all tests'\r\n")] // user entry error
        [TestCase(
            "={lastYear},Some comment with other commas after, will be processed a a single comment.",
            "currentYear{lastYear}",
            "1: A1B2C3D4: has been set as 'OutOfScope' for snapshotYear '{lastYear}' with comment 'Some comment with other commas after, will be processed a a single comment.'\r\n")] // multiple commas
        public async Task AdminController_ManualChanges_POST_Set_Organisation_As_Out_Of_Scope_Understands_Date_After_Equals(
            string dateAfterEquals,
            string relativeYear,
            string expectedResult)
        {
            var parameters = new {lastYear = TestSnapshotYear, thisYear = TestSnapshotYear + 1};

            dateAfterEquals = parameters.Resolve(dateAfterEquals);
            relativeYear = parameters.Resolve(relativeYear);
            expectedResult = parameters.Resolve(expectedResult);

            // Arrange (data for this test is slightly different than others in this class)
            int snapshotYear = relativeYear == $"currentYear{parameters.lastYear}" ? parameters.lastYear : parameters.thisYear;
            Organisation organisationInScopeForCurrentYear = OrganisationHelper.GetOrganisationInScope(TestEmployerReference, snapshotYear);
            _dbObjects = new object[] {organisationInScopeForCurrentYear, _databaseAdminUser}.AsQueryable();

            AdminController adminControllerForOutOfScopeTests = GetControllerForOutOfScopeTests(_dbObjects);
            string parametersWithoutAComment =
                $"{TestEmployerReference}{dateAfterEquals}"; // just year after the equals, so it's expected to understand it as the snapshot date
            ManualChangesViewModel manualChangesViewModelMockObject = ManualChangesViewModelHelper.GetMock(
                SetToOutOfScopeTestCommand,
                parametersWithoutAComment,
                "Generic comment for all tests");

            /* live */
            manualChangesViewModelMockObject.LastTestedCommand = manualChangesViewModelMockObject.Command;
            manualChangesViewModelMockObject.LastTestedInput = manualChangesViewModelMockObject.Parameters;

            // Act
            ManualChangesViewModel actualManualChangesViewModel = await ActOnTheAdminControllerManualChangesAsync(
                adminControllerForOutOfScopeTests,
                manualChangesViewModelMockObject);

            var modifiedOrg = _dbObjects.FirstOrDefault(x => x.GetType() == typeof(Organisation)) as Organisation;
            Assert.NotNull(modifiedOrg, "Expected at least one organisation");

            OrganisationScope newScope = modifiedOrg.OrganisationScopes.FirstOrDefault(
                orgScope =>
                    orgScope.Status == ScopeRowStatuses.Active && orgScope.SnapshotDate.Year == snapshotYear);
            Assert.NotNull(newScope, "Expected the new scope to be present");

            // Assert
            Assert.AreEqual(expectedResult, actualManualChangesViewModel.Results, actualManualChangesViewModel.Results);

            Assert.AreEqual(
                snapshotYear,
                newScope.SnapshotDate.Year,
                "When the snapshot is not provided, the application should default to the current year.");
        }

        [TestCase(
            "=2017 was a wonderful year",
            "<span style=\"color:Red\">1: ERROR: 'A1B2C3D4' the in-line comment appears to begin with a number, if having a number as part of the comment is intended please reenter this line with the format 'A1B2C3D4=SnapshotYear,2017 was a wonderful year', alternatively add a comma after the number.</span>\r\n")]
        [TestCase(
            "= 2017 was a wonderful year",
            "<span style=\"color:Red\">1: ERROR: 'A1B2C3D4' the in-line comment appears to begin with a number, if having a number as part of the comment is intended please reenter this line with the format 'A1B2C3D4=SnapshotYear, 2017 was a wonderful year', alternatively add a comma after the number.</span>\r\n")]
        [TestCase(
            "=    2017 was a wonderful year",
            "<span style=\"color:Red\">1: ERROR: 'A1B2C3D4' the in-line comment appears to begin with a number, if having a number as part of the comment is intended please reenter this line with the format 'A1B2C3D4=SnapshotYear,    2017 was a wonderful year', alternatively add a comma after the number.</span>\r\n")]
        [TestCase(
            "=9 is a number",
            "<span style=\"color:Red\">1: ERROR: 'A1B2C3D4' the in-line comment appears to begin with a number, if having a number as part of the comment is intended please reenter this line with the format 'A1B2C3D4=SnapshotYear,9 is a number', alternatively add a comma after the number.</span>\r\n")]
        public async Task
            AdminController_ManualChanges_POST_Set_Organisation_As_Out_Of_Scope_When_Comment_Begins_With_Number_Then_Fails_Warning_User(
                string stringAfterTheEquals,
                string expectedResult)
        {
            var parameters = new {lastYear = TestSnapshotYear, thisYear = TestSnapshotYear + 1};

            stringAfterTheEquals = parameters.Resolve(stringAfterTheEquals);
            expectedResult = parameters.Resolve(expectedResult);

            // Arrange (data for this test is slightly different than others in this class)
            Organisation organisationInScopeForCurrentYear =
                OrganisationHelper.GetOrganisationInScope(TestEmployerReference, parameters.thisYear);

            _dbObjects = new object[] {organisationInScopeForCurrentYear, _databaseAdminUser}.AsQueryable();

            AdminController adminControllerForOutOfScopeTests = GetControllerForOutOfScopeTests(_dbObjects);
            string parametersWithoutAComment =
                $"{TestEmployerReference}{stringAfterTheEquals}"; // just year after the equals, so it's expected to understand it as the snapshot date
            ManualChangesViewModel manualChangesViewModelMockObject = ManualChangesViewModelHelper.GetMock(
                SetToOutOfScopeTestCommand,
                parametersWithoutAComment,
                "Generic comment for all tests");

            /* live */
            manualChangesViewModelMockObject.LastTestedCommand = manualChangesViewModelMockObject.Command;
            manualChangesViewModelMockObject.LastTestedInput = manualChangesViewModelMockObject.Parameters;

            // Act
            ManualChangesViewModel actualManualChangesViewModel = await ActOnTheAdminControllerManualChangesAsync(
                adminControllerForOutOfScopeTests,
                manualChangesViewModelMockObject);

            var modifiedOrg = _dbObjects.FirstOrDefault(x => x.GetType() == typeof(Organisation)) as Organisation;
            Assert.NotNull(modifiedOrg, "Expected at least one organisation");

            // Assert
            Assert.AreEqual(expectedResult, actualManualChangesViewModel.Results);
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Set_Organisation_As_Out_Of_Scope_Exceptions_Are_Caught_And_Reported()
        {
            // Arrange
            AdminController adminControllerForOutOfScopeTests = GetControllerForOutOfScopeTests(null);
            ManualChangesViewModel manualChangesViewModelMockObject = ManualChangesViewModelHelper.GetMock(
                SetToOutOfScopeTestCommand,
                $"{TestEmployerReference}={TestSnapshotYear},comment");

            #region Configure DataRepository

            Mock<IDataRepository> dataRepo = AutoFacExtensions.ResolveAsMock<IDataRepository>();
            dataRepo.Setup(x => x.Get<User>(It.IsAny<long>())).Returns(_databaseAdminUser);

            #endregion

            // Act
            ManualChangesViewModel actualManualChangesViewModel = await ActOnTheAdminControllerManualChangesAsync(
                adminControllerForOutOfScopeTests,
                manualChangesViewModelMockObject);

            // Assert
            Assert.Multiple(
                () => {
                    Assert.AreEqual(
                        "SUCCESSFULLY TESTED 'Set organisation as out of scope': 0 of 1",
                        actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual(
                        "<span style=\"color:Red\">1: ERROR: 'A1B2C3D4' Cannot find organisation with employerReference A1B2C3D4\r\nParameter name: employerReference</span>\r\n",
                        actualManualChangesViewModel.Results);
                    Assert.AreEqual("Set organisation as out of scope", actualManualChangesViewModel.LastTestedCommand);
                    Assert.AreEqual("A1B2C3D4=2021,comment", actualManualChangesViewModel.LastTestedInput);
                    Assert.IsTrue(actualManualChangesViewModel.Tested);
                    Assert.IsNull(actualManualChangesViewModel.Comment);
                });
        }

        #endregion

    }
}

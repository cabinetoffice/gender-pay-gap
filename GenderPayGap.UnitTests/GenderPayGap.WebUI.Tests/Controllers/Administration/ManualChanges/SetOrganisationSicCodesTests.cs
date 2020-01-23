using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Tests.TestHelpers;
using GenderPayGap.WebUI.Controllers.Administration;
using GenderPayGap.WebUI.Models.Admin;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Controllers.Administration.ManualChanges
{

    [TestFixture]
    [SetCulture("en-GB")]
    public class SetOrganisationSicCodesTests
    {

        private const string MustSupplyOneOrMore = "ERROR: You must supply 1 or more input parameters";
        private const string DoesNotContainEquals = "does not contain '='";
        private const string DuplicateOrganisation = "duplicate organisation";
        private const string CannotFindEmployerRef = "Cannot find organisation with this employer reference";
        private const string HasCompanyNumber = "has a company number so you cannot change this organisation";
        private const string OrganisationMustBeActive = "is not an active organisation so you cannot change its SIC codes";

        private const string SetOrganisationSicCodesCommand = "Set organisation SIC codes";
        private const string MustContainSicCodes = "must contain at least one SIC code";
        private const string SicCodesMustBeNumeric = "you can only input numeric SIC codes";
        private const string SicCodeDoesntExist = "the following SIC codes do not exist in the database";

        private Mock<IDataRepository> mockDataRepo;
        private IList<Organisation> testOrgData;
        private IList<OrganisationSicCode> testOrgSicCodeData;
        private IList<SicCode> testSicCodeData;
        private User testUser;

        private void GenerateTestData()
        {
            testOrgData = new List<Organisation>(
                new[] {
                    new Organisation {
                        OrganisationId = 1,
                        EmployerReference = "6B2LF57C",
                        SectorType = SectorTypes.Public,
                        Status = OrganisationStatuses.Active
                    },
                    new Organisation {
                        OrganisationId = 2,
                        EmployerReference = "DR994D7L",
                        CompanyNumber = "12345678",
                        SectorType = SectorTypes.Private,
                        Status = OrganisationStatuses.Active
                    },
                    new Organisation {
                        OrganisationId = 3,
                        EmployerReference = "23TYLBLB",
                        SectorType = SectorTypes.Public,
                        Status = OrganisationStatuses.Active
                    },
                    new Organisation {
                        OrganisationId = 4,
                        EmployerReference = "SNGNB4BH",
                        CompanyNumber = "87654321",
                        SectorType = SectorTypes.Private,
                        Status = OrganisationStatuses.Active
                    },
                    new Organisation {
                        OrganisationId = 5,
                        EmployerReference = "RWT2TY62",
                        SectorType = SectorTypes.Public,
                        Status = OrganisationStatuses.Active
                    },
                    new Organisation {
                        OrganisationId = 6,
                        EmployerReference = "TY67R5T6",
                        SectorType = SectorTypes.Public,
                        Status = OrganisationStatuses.Pending
                    }
                });

            testOrgSicCodeData = new List<OrganisationSicCode>(
                new[] {
                    new OrganisationSicCode {SicCodeId = 1, OrganisationId = 1},
                    new OrganisationSicCode {SicCodeId = 2, OrganisationId = 2},
                    new OrganisationSicCode {SicCodeId = 3, OrganisationId = 3},
                    new OrganisationSicCode {SicCodeId = 4, OrganisationId = 4},
                    new OrganisationSicCode {SicCodeId = 5, OrganisationId = 5}
                });

            testSicCodeData = new List<SicCode>(
                new[] {
                    new SicCode {SicCodeId = 1111},
                    new SicCode {SicCodeId = 2222},
                    new SicCode {SicCodeId = 3333},
                    new SicCode {SicCodeId = 4444},
                    new SicCode {SicCodeId = 5555},
                    new SicCode {SicCodeId = 1},
                    new SicCode {SicCodeId = 2},
                    new SicCode {SicCodeId = 3},
                    new SicCode {SicCodeId = 4},
                    new SicCode {SicCodeId = 5}
                });
        }

        [SetUp]
        public void BeforeEach()
        {
            mockDataRepo = new Mock<IDataRepository>();
            testUser = UserHelper.GetDatabaseAdmin();
            GenerateTestData();
        }

        [Test]
        public async Task ShouldFailWhenNoLinesSupplied()
        {
            // Arrange
            var thisTestDbObjects = new object[] {testUser};
            var thisTestAdminController = UiTestHelper.GetController<AdminController>(testUser.UserId, null, thisTestDbObjects);
            var thisTestParameters = "";
            ManualChangesViewModel testManualChangesVm =
                ManualChangesViewModelHelper.GetMock(SetOrganisationSicCodesCommand, thisTestParameters);

            // Act
            IActionResult result = await thisTestAdminController.ManualChanges(testManualChangesVm);

            // Assert
            Assert.AreEqual(MustSupplyOneOrMore, thisTestAdminController.ModelState[""].Errors[0].ErrorMessage);
        }

        [Test]
        public async Task ShouldPassWhenLinesAreSupplied()
        {
            // Arrange
            var thisTestDbObjects = new object[] {testUser};
            var thisTestAdminController = UiTestHelper.GetController<AdminController>(testUser.UserId, null, thisTestDbObjects);
            var thisTestParameters = "SOMETHING";
            ManualChangesViewModel testManualChangesVm =
                ManualChangesViewModelHelper.GetMock(SetOrganisationSicCodesCommand, thisTestParameters);

            // Act
            var manualChangesViewResult = await thisTestAdminController.ManualChanges(testManualChangesVm) as ViewResult;

            // Assert
            Assert.NotNull(manualChangesViewResult, "Expected ViewResult");

            var actualManualChangesViewModel = manualChangesViewResult.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");
        }

        [Test]
        public async Task ShouldFailEachLineMissingEqualsCharacter()
        {
            // Arrange
            var thisTestDbObjects = new object[] {testUser};
            var thisTestAdminController = UiTestHelper.GetController<AdminController>(testUser.UserId, null, thisTestDbObjects);
            string thisTestParameters = string.Join(
                "\r\n",
                "test=passed",
                "this line should fail",
                "red=passed",
                "this line should fail also");

            ManualChangesViewModel thisTestManualChangesVm =
                ManualChangesViewModelHelper.GetMock(SetOrganisationSicCodesCommand, thisTestParameters);

            // Act
            var thisManualChangesVr = await thisTestAdminController.ManualChanges(thisTestManualChangesVm) as ViewResult;

            // Assert
            Assert.NotNull(thisManualChangesVr, "Expected ViewResult");

            var actualManualChangesViewModel = thisManualChangesVr.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            string[] actualResults = actualManualChangesViewModel.Results.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);

            Assert.AreEqual(5, actualResults.Length);
            Assert.IsFalse(actualResults[0].Contains(DoesNotContainEquals), "Expected false when line contains '='");
            Assert.IsTrue(actualResults[1].Contains(DoesNotContainEquals), "Expected true when line does not contain '='");
            Assert.IsFalse(actualResults[2].Contains(DoesNotContainEquals), "Expected false when line contains '='");
            Assert.IsTrue(actualResults[3].Contains(DoesNotContainEquals), "Expected true when line does not contain '='");
        }

        [Test]
        public async Task ShouldFailWhenEmployerDoesNotExist()
        {
            // Arrange
            var thisTestDbObjects = new object[] {testUser, testOrgData};
            var thisTestAdminController = UiTestHelper.GetController<AdminController>(testUser.UserId, null, thisTestDbObjects);
            string thisTestParameters = string.Join(
                "\r\n",
                "6B2LF57C=1",
                "EMP2=2",
                "23TYLBLB=3",
                "EMP4=4",
                "RWT2TY62=5");

            ManualChangesViewModel thisTestManualChangesVm =
                ManualChangesViewModelHelper.GetMock(SetOrganisationSicCodesCommand, thisTestParameters);

            // Act
            var thisManualChangesVr = await thisTestAdminController.ManualChanges(thisTestManualChangesVm) as ViewResult;

            // Assert
            Assert.NotNull(thisManualChangesVr, "Expected ViewResult");

            var actualManualChangesViewModel = thisManualChangesVr.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            string[] actualResults = actualManualChangesViewModel.Results.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);
            Assert.AreEqual(6, actualResults.Length);
            Assert.IsFalse(actualResults[0].Contains(CannotFindEmployerRef), "Expected false when organisation exists");
            Assert.IsTrue(actualResults[1].Contains(CannotFindEmployerRef), "Expected true when organisation doesn't exist");
            Assert.IsFalse(actualResults[2].Contains(CannotFindEmployerRef), "Expected false when organisation exists");
            Assert.IsTrue(actualResults[3].Contains(CannotFindEmployerRef), "Expected true when organisation doesn't exist");
            Assert.IsFalse(actualResults[4].Contains(CannotFindEmployerRef), "Expected false when organisation exists");
        }

        [Test]
        public async Task ShouldFailDuplicateEmployers()
        {
            // Arrange
            var thisTestDbObjects = new object[] {testUser};
            var thisTestAdminController = UiTestHelper.GetController<AdminController>(testUser.UserId, null, thisTestDbObjects);
            string thisTestParameters = "  ="
                                        + Environment.NewLine
                                        + string.Join(Environment.NewLine, "EMP1=1", "EMP2=2", "EMP3=3", "EMP2=4", "EMP4=4");

            ManualChangesViewModel thisTestManualChangesVm =
                ManualChangesViewModelHelper.GetMock(SetOrganisationSicCodesCommand, thisTestParameters);

            // Act
            var thisManualChangesVr = await thisTestAdminController.ManualChanges(thisTestManualChangesVm) as ViewResult;

            // Assert
            Assert.NotNull(thisManualChangesVr, "Expected ViewResult");

            var actualManualChangesViewModel = thisManualChangesVr.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            string[] actualResults = actualManualChangesViewModel.Results.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);

            Assert.AreEqual(6, actualResults.Length);
            Assert.IsFalse(actualResults[0].Contains(DuplicateOrganisation), "Expected false when employer is not duplicate");
            Assert.IsFalse(actualResults[1].Contains(DuplicateOrganisation), "Expected false when employer is not duplicate");
            Assert.IsFalse(actualResults[2].Contains(DuplicateOrganisation), "Expected false when employer is not duplicate");
            Assert.IsTrue(actualResults[3].Contains(DuplicateOrganisation), "Expected true when employer is a duplicate");
            Assert.IsFalse(actualResults[4].Contains(DuplicateOrganisation), "Expected false when employer is not duplicate");
        }

        [Test]
        public async Task ShouldFailWhenNoValueAfterEqualsCharacter()
        {
            // Arrange
            var thisTestDbObjects = new object[] {testUser, testOrgData};
            var thisTestAdminController = UiTestHelper.GetController<AdminController>(testUser.UserId, null, thisTestDbObjects);
            string thisTestParameters = string.Join(
                "\r\n",
                "6B2LF57C=",
                "DR994D7L=2",
                "23TYLBLB=",
                "SNGNB4BH=3",
                "RWT2TY62=");

            ManualChangesViewModel thisTestManualChangesVm =
                ManualChangesViewModelHelper.GetMock(SetOrganisationSicCodesCommand, thisTestParameters);

            // Act
            var thisManualChangesVr = await thisTestAdminController.ManualChanges(thisTestManualChangesVm) as ViewResult;

            // Assert
            Assert.NotNull(thisManualChangesVr, "Expected ViewResult");

            var actualManualChangesViewModel = thisManualChangesVr.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            string[] actualResults = actualManualChangesViewModel.Results.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);
            Assert.AreEqual(6, actualResults.Length);
            Assert.IsTrue(actualResults[0].Contains(MustContainSicCodes), "Expected false when line contains a Sic code");
            Assert.IsFalse(actualResults[1].Contains(MustContainSicCodes), "Expected true when line contains a Sic code");
            Assert.IsTrue(actualResults[2].Contains(MustContainSicCodes), "Expected false when line contains a Sic code");
            Assert.IsFalse(actualResults[3].Contains(MustContainSicCodes), "Expected true when line contains a Sic code");
            Assert.IsTrue(actualResults[4].Contains(MustContainSicCodes), "Expected false when line contains a Sic code");
        }

        [Test]
        public async Task ShouldFailWhenSicCodeIsNotNumeric()
        {
            // Arrange
            var thisTestDbObjects = new object[] {testUser, testOrgData};
            var thisTestAdminController = UiTestHelper.GetController<AdminController>(testUser.UserId, null, thisTestDbObjects);
            string thisTestParameters = string.Join(
                "\r\n",
                "6B2LF57C=ABCD",
                "DR994D7L=2",
                "23TYLBLB=EFGH",
                "SNGNB4BH=4",
                "RWT2TY62=IJKL");

            ManualChangesViewModel thisTestManualChangesVm =
                ManualChangesViewModelHelper.GetMock(SetOrganisationSicCodesCommand, thisTestParameters);

            // Act
            var thisManualChangesVr = await thisTestAdminController.ManualChanges(thisTestManualChangesVm) as ViewResult;

            // Assert
            Assert.NotNull(thisManualChangesVr, "Expected ViewResult");

            var actualManualChangesViewModel = thisManualChangesVr.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            string[] actualResults = actualManualChangesViewModel.Results.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);
            Assert.AreEqual(6, actualResults.Length);
            Assert.IsTrue(actualResults[0].Contains(SicCodesMustBeNumeric), "Expected true when Sic code is not an integer");
            Assert.IsFalse(actualResults[1].Contains(SicCodesMustBeNumeric), "Expected false when Sic code is an integer");
            Assert.IsTrue(actualResults[2].Contains(SicCodesMustBeNumeric), "Expected true when Sic code is not an integer");
            Assert.IsFalse(actualResults[3].Contains(SicCodesMustBeNumeric), "Expected false when Sic code is an integer");
            Assert.IsTrue(actualResults[4].Contains(SicCodesMustBeNumeric), "Expected true when Sic code is not an integer");
        }

        [Test]
        public async Task ShouldFailWhenOrganisationIsNotActive()
        {
            // Arrange
            var thisTestDbObjects = new object[] {testUser, testOrgData};
            var thisTestAdminController = UiTestHelper.GetController<AdminController>(testUser.UserId, null, thisTestDbObjects);
            string thisTestParameters = string.Join(
                "\r\n",
                "TY67R5T6=1311",
                "DR994D7L=1241");

            ManualChangesViewModel thisTestManualChangesVm =
                ManualChangesViewModelHelper.GetMock(SetOrganisationSicCodesCommand, thisTestParameters);

            // Act
            var thisManualChangesVr = await thisTestAdminController.ManualChanges(thisTestManualChangesVm) as ViewResult;

            // Assert
            Assert.NotNull(thisManualChangesVr, "Expected ViewResult");

            var actualManualChangesViewModel = thisManualChangesVr.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            string[] actualResults = actualManualChangesViewModel.Results.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);
            Assert.AreEqual(3, actualResults.Length);
            Assert.IsTrue(actualResults[0].Contains(OrganisationMustBeActive), "Expected true when an organisation is not active");
            Assert.IsFalse(actualResults[1].Contains(OrganisationMustBeActive), "Expected false when an organisation is active");
        }

        [Test]
        public async Task ShouldFailWhenEmployerHasCompanyNumber()
        {
            // Arrange
            var thisTestDbObjects = new object[] {testUser, testOrgData};
            var thisTestAdminController = UiTestHelper.GetController<AdminController>(testUser.UserId, null, thisTestDbObjects);
            string thisTestParameters = string.Join(
                "\r\n",
                "6B2LF57C=1",
                "DR994D7L=2",
                "23TYLBLB=3",
                "SNGNB4BH=4",
                "RWT2TY62=5");

            ManualChangesViewModel thisTestManualChangesVm =
                ManualChangesViewModelHelper.GetMock(SetOrganisationSicCodesCommand, thisTestParameters);

            // Act
            var thisManualChangesVr = await thisTestAdminController.ManualChanges(thisTestManualChangesVm) as ViewResult;

            // Assert
            Assert.NotNull(thisManualChangesVr, "Expected ViewResult");

            var actualManualChangesViewModel = thisManualChangesVr.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            string[] actualResults = actualManualChangesViewModel.Results.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);
            Assert.AreEqual(6, actualResults.Length);
            Assert.IsFalse(actualResults[0].Contains(HasCompanyNumber), "Expected false when organisation doesn't have a company number");
            Assert.IsTrue(actualResults[1].Contains(HasCompanyNumber), "Expected true when organisation has a company number");
            Assert.IsFalse(actualResults[2].Contains(HasCompanyNumber), "Expected false when organisation doesn't have a company number");
            Assert.IsTrue(actualResults[3].Contains(HasCompanyNumber), "Expected true when organisation has a company number");
            Assert.IsFalse(actualResults[4].Contains(HasCompanyNumber), "Expected false when organisation doesn't have a company number");
        }

        [Test]
        public async Task ShouldFailWhenSicCodesDontExist()
        {
            // Arrange
            var thisTestDbObjects = new object[] {testUser, testOrgData, testSicCodeData};
            var thisTestAdminController = UiTestHelper.GetController<AdminController>(testUser.UserId, null, thisTestDbObjects);
            string thisTestParameters = string.Join(
                "\r\n",
                "6B2LF57C=1111",
                "DR994D7L=2",
                "23TYLBLB=3333",
                "SNGNB4BH=4",
                "RWT2TY62=5555");

            ManualChangesViewModel thisTestManualChangesVm =
                ManualChangesViewModelHelper.GetMock(SetOrganisationSicCodesCommand, thisTestParameters);

            // Act
            var thisManualChangesVr = await thisTestAdminController.ManualChanges(thisTestManualChangesVm) as ViewResult;

            // Assert
            Assert.NotNull(thisManualChangesVr, "Expected ViewResult");

            var actualManualChangesViewModel = thisManualChangesVr.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            string[] actualResults = actualManualChangesViewModel.Results.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);
            Assert.AreEqual(6, actualResults.Length);
            Assert.IsFalse(actualResults[0].Contains(SicCodeDoesntExist), "Expected false when sic code exists");
            Assert.IsFalse(actualResults[1].Contains(SicCodeDoesntExist), "Expected true when sic code does not exist");
            Assert.IsFalse(actualResults[2].Contains(SicCodeDoesntExist), "Expected false when sic code exists");
            Assert.IsFalse(actualResults[3].Contains(SicCodeDoesntExist), "Expected true when sic code does not exist");
            Assert.IsFalse(actualResults[4].Contains(SicCodeDoesntExist), "Expected false when sic code exists");
        }

        [Test]
        public async Task ShouldReplaceEmployerSicCodes()
        {
            // Arrange
            var thisTestDbObjects = new object[] {testUser, testOrgData, testSicCodeData, testOrgSicCodeData};
            var thisTestAdminController = UiTestHelper.GetController<AdminController>(testUser.UserId, null, thisTestDbObjects);
            string thisTestParameters = string.Join(
                "\r\n",
                "6B2LF57C=1111",
                "23TYLBLB=3333");

            ManualChangesViewModel thisTestManualChangesVm =
                ManualChangesViewModelHelper.GetMock(SetOrganisationSicCodesCommand, thisTestParameters);
            thisTestManualChangesVm.LastTestedCommand = SetOrganisationSicCodesCommand;
            thisTestManualChangesVm.LastTestedInput = thisTestParameters.ReplaceI(Environment.NewLine, ";");

            // Act
            var thisManualChangesVr = await thisTestAdminController.ManualChanges(thisTestManualChangesVm) as ViewResult;

            // Assert
            Assert.NotNull(thisManualChangesVr, "Expected ViewResult");

            var actualManualChangesViewModel = thisManualChangesVr.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            string[] actualResults = actualManualChangesViewModel.Results.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);
            Assert.AreEqual(3, actualResults.Length);

            Organisation org1 = thisTestAdminController.DataRepository.GetAll<Organisation>()
                .SingleOrDefault(x => x.EmployerReference == "6B2LF57C");
            Assert.AreEqual("1111", org1.GetSicCodeIdsString());
            Assert.AreEqual("1: 6B2LF57C: SIC codes=1 has been set to 1111", actualResults[0]);

            Organisation org2 = thisTestAdminController.DataRepository.GetAll<Organisation>()
                .SingleOrDefault(x => x.EmployerReference == "23TYLBLB");
            Assert.AreEqual("3333", org2.GetSicCodeIdsString());
            Assert.AreEqual("2: 23TYLBLB: SIC codes=3 has been set to 3333", actualResults[1]);
        }

    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Tests.Common.TestHelpers;
using GenderPayGap.Tests.TestHelpers;
using GenderPayGap.WebUI.Controllers.Administration;
using GenderPayGap.WebUI.Models.Admin;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Controllers.Administration
{

    [TestFixture]
    [SetCulture("en-GB")]
    public class AdminControllerManualChangesSectorTypeModificationTests
    {

        private const string MustSupplyOneOrMore = "ERROR: You must supply 1 or more input parameters";
        private const string DoesNotContainEquals = "does not contain '='";
        private const string DuplicateOrganisation = "duplicate organisation";
        private const string CannotFindEmployerRef = "Cannot find organisation with this employer reference";
        private const string HasCompanyNumber = "has a company number so you cannot change this organisation";

        private const string SetPublicSectorTypeCommand = "Set public sector type";
        private const string MustContainPublicSectorTypeCode = "must contain a public sector type";
        private const string PublicSectorTypeMustBeInteger = "you can only input a numeric public sector type";
        private const string OrganisationMustBeActive = "is not an active organisation so you cannot change its public sector type";
        private const string IsNotPublicSector = "is not a public sector organisation";
        private const string CanOnlyAssignOneSectorType = "you can only assign one public sector type per organisation";

        private const string ConvertPublicToPrivateCommand = "Convert public to private";
        private const string ConvertPrivateToPublicCommand = "Convert private to public";

        private Mock<IDataRepository> mockDataRepo;
        private IList<Organisation> testOrgData;
        private IList<OrganisationPublicSectorType> testOrgSecTypesData;
        private IList<PublicSectorType> testSecTypesData;
        private User testUser;

        private void GenerateTestData()
        {
            testOrgData = new List<Organisation>(
                new[] {
                    new Organisation {
                        OrganisationId = 1,
                        EmployerReference = "6B2LF57C",
                        OrganisationName = "org 1",
                        SectorType = SectorTypes.Public,
                        Status = OrganisationStatuses.Active
                    },
                    new Organisation {
                        OrganisationId = 2,
                        EmployerReference = "DR994D7L",
                        OrganisationName = "org 2",
                        CompanyNumber = "12345678",
                        SectorType = SectorTypes.Private,
                        Status = OrganisationStatuses.Active
                    },
                    new Organisation {
                        OrganisationId = 3,
                        EmployerReference = "23TYLBLB",
                        OrganisationName = "org 3",
                        SectorType = SectorTypes.Public,
                        Status = OrganisationStatuses.Active
                    },
                    new Organisation {
                        OrganisationId = 4,
                        EmployerReference = "SNGNB4BH",
                        OrganisationName = "org 4",
                        CompanyNumber = "87654321",
                        SectorType = SectorTypes.Private,
                        Status = OrganisationStatuses.Active
                    },
                    new Organisation {
                        OrganisationId = 5,
                        EmployerReference = "RWT2TY62",
                        OrganisationName = "org 5",
                        SectorType = SectorTypes.Public,
                        Status = OrganisationStatuses.Active
                    },
                    new Organisation {
                        OrganisationId = 6,
                        EmployerReference = "ADCE324T",
                        OrganisationName = "org 6",
                        SectorType = SectorTypes.Public,
                        Status = OrganisationStatuses.Active
                    },
                    new Organisation {
                        OrganisationId = 7,
                        EmployerReference = "GR2H67UI",
                        OrganisationName = "org 7",
                        SectorType = SectorTypes.Public,
                        Status = OrganisationStatuses.Active
                    },
                    new Organisation {
                        OrganisationId = 8,
                        EmployerReference = "FG34RT65",
                        OrganisationName = "org 8",
                        SectorType = SectorTypes.Public,
                        Status = OrganisationStatuses.Active
                    },
                    new Organisation {
                        OrganisationId = 9,
                        EmployerReference = "D43TYU76",
                        OrganisationName = "org 9",
                        SectorType = SectorTypes.Public,
                        Status = OrganisationStatuses.Active
                    },
                    new Organisation {
                        OrganisationId = 10,
                        EmployerReference = "RT56YU34",
                        OrganisationName = "org 10",
                        SectorType = SectorTypes.Public,
                        Status = OrganisationStatuses.Pending
                    }
                });

            testOrgSecTypesData = new List<OrganisationPublicSectorType>(
                new[] {
                    new OrganisationPublicSectorType {PublicSectorTypeId = 1, OrganisationId = 1},
                    new OrganisationPublicSectorType {PublicSectorTypeId = 2, OrganisationId = 2},
                    new OrganisationPublicSectorType {PublicSectorTypeId = 3, OrganisationId = 3},
                    new OrganisationPublicSectorType {PublicSectorTypeId = 4, OrganisationId = 4},
                    new OrganisationPublicSectorType {PublicSectorTypeId = 5, OrganisationId = 5},
                    new OrganisationPublicSectorType {PublicSectorTypeId = 6, OrganisationId = 6},
                    new OrganisationPublicSectorType {PublicSectorTypeId = 7, OrganisationId = 7},
                    new OrganisationPublicSectorType {PublicSectorTypeId = 8, OrganisationId = 8}
                });

            testSecTypesData = new List<PublicSectorType>(
                new[] {
                    new PublicSectorType {PublicSectorTypeId = 1, Description = "public sector type 1"},
                    new PublicSectorType {PublicSectorTypeId = 2, Description = "public sector type 2"},
                    new PublicSectorType {PublicSectorTypeId = 3, Description = "public sector type 3"},
                    new PublicSectorType {PublicSectorTypeId = 4, Description = "public sector type 4"},
                    new PublicSectorType {PublicSectorTypeId = 5, Description = "public sector type 5"},
                    new PublicSectorType {PublicSectorTypeId = 6, Description = "public sector type 6"},
                    new PublicSectorType {PublicSectorTypeId = 7, Description = "public sector type 7"},
                    new PublicSectorType {PublicSectorTypeId = 8, Description = "public sector type 8"},
                    new PublicSectorType {PublicSectorTypeId = 9, Description = "public sector type 9"}
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
        public void ShouldFailWhenNoLinesSupplied()
        {
            // Arrange
            var thisTestDbObjects = new object[] {testUser};
            var thisTestAdminController = UiTestHelper.GetController<AdminController>(testUser.UserId, null, thisTestDbObjects);
            var thisTestParameters = "";
            ManualChangesViewModel testManualChangesVm =
                ManualChangesViewModelHelper.GetMock(SetPublicSectorTypeCommand, thisTestParameters);

            // Act
            Task<IActionResult> result = thisTestAdminController.ManualChanges(testManualChangesVm);

            // Assert
            Assert.AreEqual(MustSupplyOneOrMore, thisTestAdminController.ModelState[""].Errors[0].ErrorMessage);
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_ShouldPassWhenLinesAreSupplied()
        {
            // Arrange
            var thisTestDbObjects = new object[] {testUser};
            var thisTestAdminController = UiTestHelper.GetController<AdminController>(testUser.UserId, null, thisTestDbObjects);
            var thisTestParameters = "SOMETHING";
            ManualChangesViewModel testManualChangesVm =
                ManualChangesViewModelHelper.GetMock(SetPublicSectorTypeCommand, thisTestParameters);

            // Act
            var manualChangesViewResult = await thisTestAdminController.ManualChanges(testManualChangesVm) as ViewResult;

            // Assert
            Assert.NotNull(manualChangesViewResult, "Expected ViewResult");

            var actualManualChangesViewModel = manualChangesViewResult.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_ShouldFailEachLineMissingEqualsCharacter()
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
                ManualChangesViewModelHelper.GetMock(SetPublicSectorTypeCommand, thisTestParameters);

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
        public async Task AdminController_ManualChanges_POST_ShouldFailWhenEmployerDoesNotExist()
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
                ManualChangesViewModelHelper.GetMock(SetPublicSectorTypeCommand, thisTestParameters);

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
        public async Task AdminController_ManualChanges_POST_ShouldFailDuplicateEmployers()
        {
            // Arrange
            var thisTestDbObjects = new object[] {testUser};
            var thisTestAdminController = UiTestHelper.GetController<AdminController>(testUser.UserId, null, thisTestDbObjects);
            string thisTestParameters = string.Join(
                "\r\n",
                "EMP1=1",
                "EMP2=2",
                "EMP3=3",
                "EMP2=4",
                "EMP4=4");

            ManualChangesViewModel thisTestManualChangesVm =
                ManualChangesViewModelHelper.GetMock(SetPublicSectorTypeCommand, thisTestParameters);

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
        public async Task AdminController_ManualChanges_POST_ShouldFailWhenNoValueAfterEqualsCharacter()
        {
            // Arrange
            var thisTestDbObjects = new object[] {testUser, testOrgData};
            var thisTestAdminController = UiTestHelper.GetController<AdminController>(testUser.UserId, null, thisTestDbObjects);
            string thisTestParameters = string.Join(
                "\r\n",
                "6B2LF57C=",
                "23TYLBLB=",
                "RWT2TY62=4");

            ManualChangesViewModel thisTestManualChangesVm =
                ManualChangesViewModelHelper.GetMock(SetPublicSectorTypeCommand, thisTestParameters);

            // Act
            var thisManualChangesVr = await thisTestAdminController.ManualChanges(thisTestManualChangesVm) as ViewResult;

            // Assert
            Assert.NotNull(thisManualChangesVr, "Expected ViewResult");

            var actualManualChangesViewModel = thisManualChangesVr.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            string[] actualResults = actualManualChangesViewModel.Results.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);
            Assert.AreEqual(4, actualResults.Length);
            Assert.IsTrue(actualResults[0].Contains(MustContainPublicSectorTypeCode), "Expected true when line contains a value");
            Assert.IsTrue(actualResults[1].Contains(MustContainPublicSectorTypeCode), "Expected true when line contains a value");
            Assert.IsFalse(actualResults[2].Contains(MustContainPublicSectorTypeCode), "Expected true when line contains a value");
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_ShouldFailWhenMoreThanOnePublicSectorTypeEntered()
        {
            // Arrange
            var thisTestDbObjects = new object[] {testUser, testOrgData, testOrgSecTypesData, testSecTypesData};
            var thisTestAdminController = UiTestHelper.GetController<AdminController>(testUser.UserId, null, thisTestDbObjects);
            string thisTestParameters = string.Join(
                "\r\n",
                "6B2LF57C=1,2",
                "23TYLBLB=1",
                "RWT2TY62=4,8,6");

            ManualChangesViewModel thisTestManualChangesVm =
                ManualChangesViewModelHelper.GetMock(SetPublicSectorTypeCommand, thisTestParameters);

            // Act
            var thisManualChangesVr = await thisTestAdminController.ManualChanges(thisTestManualChangesVm) as ViewResult;

            // Assert
            Assert.NotNull(thisManualChangesVr, "Expected ViewResult");

            var actualManualChangesViewModel = thisManualChangesVr.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            string[] actualResults = actualManualChangesViewModel.Results.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);
            Assert.AreEqual(4, actualResults.Length);
            Assert.IsTrue(
                actualResults[0].Contains(CanOnlyAssignOneSectorType),
                "Expected to fail when multiple public sector types are given");
            Assert.IsFalse(
                actualResults[1].Contains(CanOnlyAssignOneSectorType),
                "Expected to succeed when single public sector type is given");
            Assert.IsTrue(
                actualResults[2].Contains(CanOnlyAssignOneSectorType),
                "Expected to fail when multiple public sector types are given");
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_ShouldFailWhenPublicSectorTypeNotInteger()
        {
            // Arrange
            var thisTestDbObjects = new object[] {testUser, testOrgData, testOrgSecTypesData, testSecTypesData};
            var thisTestAdminController = UiTestHelper.GetController<AdminController>(testUser.UserId, null, thisTestDbObjects);
            string thisTestParameters = string.Join(
                "\r\n",
                "6B2LF57C=TEST",
                "23TYLBLB=ABCD",
                "RWT2TY62=4");

            ManualChangesViewModel thisTestManualChangesVm =
                ManualChangesViewModelHelper.GetMock(SetPublicSectorTypeCommand, thisTestParameters);

            // Act
            var thisManualChangesVr = await thisTestAdminController.ManualChanges(thisTestManualChangesVm) as ViewResult;

            // Assert
            Assert.NotNull(thisManualChangesVr, "Expected ViewResult");

            var actualManualChangesViewModel = thisManualChangesVr.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            string[] actualResults = actualManualChangesViewModel.Results.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);
            Assert.AreEqual(4, actualResults.Length);
            Assert.IsTrue(actualResults[0].Contains(PublicSectorTypeMustBeInteger), "Expected to fail when input not an integer");
            Assert.IsTrue(actualResults[1].Contains(PublicSectorTypeMustBeInteger), "Expected to fail when input not an integer");
            Assert.IsFalse(actualResults[2].Contains(PublicSectorTypeMustBeInteger), "Expected to succeed when input is an integer");
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_ShouldFailWhenOrganisationIsNotActive()
        {
            // Arrange
            var thisTestDbObjects = new object[] {testUser, testOrgData};
            var thisTestAdminController = UiTestHelper.GetController<AdminController>(testUser.UserId, null, thisTestDbObjects);
            string thisTestParameters = string.Join(
                "\r\n",
                "RT56YU34=address",
                "DR994D7L=address");

            ManualChangesViewModel thisTestManualChangesVm =
                ManualChangesViewModelHelper.GetMock(SetPublicSectorTypeCommand, thisTestParameters);

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
        public async Task AdminController_ManualChanges_POST_ShouldFailWhenEmployerIsNotPublicSector()
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
                ManualChangesViewModelHelper.GetMock(SetPublicSectorTypeCommand, thisTestParameters);

            // Act
            var thisManualChangesVr = await thisTestAdminController.ManualChanges(thisTestManualChangesVm) as ViewResult;

            // Assert
            Assert.NotNull(thisManualChangesVr, "Expected ViewResult");

            var actualManualChangesViewModel = thisManualChangesVr.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            string[] actualResults = actualManualChangesViewModel.Results.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);
            Assert.AreEqual(6, actualResults.Length);
            Assert.IsFalse(actualResults[0].Contains(IsNotPublicSector), "Expected false when organisation is not public sector");
            Assert.IsTrue(actualResults[1].Contains(IsNotPublicSector), "Expected true when organisation is public sector");
            Assert.IsFalse(actualResults[2].Contains(IsNotPublicSector), "Expected false when organisation is not public sector");
            Assert.IsTrue(actualResults[3].Contains(IsNotPublicSector), "Expected true when organisation is public sector");
            Assert.IsFalse(actualResults[4].Contains(IsNotPublicSector), "Expected false when organisation is not public sector");
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_ShouldFailWhenPublicSectorTypeDoesNotExist()
        {
            // Arrange
            var thisTestDbObjects = new object[] {testUser, testOrgData, testOrgSecTypesData, testSecTypesData};
            var thisTestAdminController = UiTestHelper.GetController<AdminController>(testUser.UserId, null, thisTestDbObjects);
            string thisTestParameters = string.Join(
                "\r\n",
                "6B2LF57C=1412",
                "DR994D7L=2",
                "23TYLBLB=355");

            ManualChangesViewModel thisTestManualChangesVm =
                ManualChangesViewModelHelper.GetMock(SetPublicSectorTypeCommand, thisTestParameters);

            // Act
            var thisManualChangesVr = await thisTestAdminController.ManualChanges(thisTestManualChangesVm) as ViewResult;

            // Assert
            Assert.NotNull(thisManualChangesVr, "Expected ViewResult");

            var actualManualChangesViewModel = thisManualChangesVr.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            string[] actualResults = actualManualChangesViewModel.Results.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);
            Assert.AreEqual(4, actualResults.Length);
            Assert.IsTrue(
                actualResults[0].Contains("public sector type 1412 does not exist"),
                "Expected false when public sector type doesn't exist");
            Assert.IsFalse(actualResults[1].Contains("does not exist"), "Expected true when public sector type exists");
            Assert.IsTrue(
                actualResults[2].Contains("public sector type 355 does not exist"),
                "Expected false when public sector type doesn't exist");
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_ShouldUpdateOrganisationPublicSectorType()
        {
            // Arrange
            var thisTestDbObjects = new object[] {testUser, testOrgData, testSecTypesData, testOrgSecTypesData};
            var thisTestAdminController = UiTestHelper.GetController<AdminController>(testUser.UserId, null, thisTestDbObjects);
            string thisTestParameters = string.Join(
                "\r\n",
                "6B2LF57C=1",
                "D43TYU76=2");

            ManualChangesViewModel thisTestManualChangesVm =
                ManualChangesViewModelHelper.GetMock(SetPublicSectorTypeCommand, thisTestParameters);
            thisTestManualChangesVm.LastTestedCommand = SetPublicSectorTypeCommand;
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
            Assert.AreEqual(1, org1.LatestPublicSectorType.PublicSectorTypeId);
            Assert.AreEqual(
                "1: 6B2LF57C:org 1 public sector type=No previous public sector type has been set to public sector type 1",
                actualResults[0]);

            Organisation org2 = thisTestAdminController.DataRepository.GetAll<Organisation>()
                .SingleOrDefault(x => x.EmployerReference == "D43TYU76");
            Assert.AreEqual(2, org2.LatestPublicSectorType.PublicSectorTypeId);
            Assert.AreEqual(
                "2: D43TYU76:org 9 public sector type=No previous public sector type has been set to public sector type 2",
                actualResults[1]);
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Convert_Public_To_Private_Works_When_Run_In_Test_Mode_Async()
        {
            // Arrange
            Organisation publicOrganisationToBeChangedToPrivate = OrganisationHelper.GetPublicOrganisation("EmployerReference03");

            #region setting up database and controller 

            User notAdminUser = UserHelper.GetDatabaseAdmin();
            var adminController = UiTestHelper.GetController<AdminController>(notAdminUser.UserId, null, null);

            Mock<IDataRepository> configurableDataRepository = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            configurableDataRepository
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(notAdminUser);

            configurableDataRepository
                .Setup(x => x.GetAll<Organisation>())
                .Returns(new[] {publicOrganisationToBeChangedToPrivate}.AsQueryable().BuildMock().Object);

            var manualChangesViewModel = new ManualChangesViewModel {
                Command = ConvertPublicToPrivateCommand, Parameters = publicOrganisationToBeChangedToPrivate.EmployerReference
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
                    Assert.AreEqual("SUCCESSFULLY TESTED 'Convert public to private': 1 of 1", actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual(
                        "1: EMPLOYERREFERENCE03: Org123 sector Public set to 'Private'\r\n",
                        actualManualChangesViewModel.Results);
                    Assert.AreEqual(ConvertPublicToPrivateCommand, actualManualChangesViewModel.LastTestedCommand);
                    Assert.AreEqual("EmployerReference03", actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be tested=true as this case is running in TEST mode");
                    Assert.IsNull(actualManualChangesViewModel.Comment);
                });
        }

        [Test]
        public async Task AdminController_ManualChanges_POST_Convert_Private_To_Public_Works_When_Run_In_Test_Mode_Async()
        {
            // Arrange
            Organisation privateOrganisationToBeChangedToPublic = OrganisationHelper.GetPrivateOrganisation("EmployerReference04");

            #region setting up database and controller 

            User notAdminUser = UserHelper.GetDatabaseAdmin();
            var adminController = UiTestHelper.GetController<AdminController>(notAdminUser.UserId, null, null);

            Mock<IDataRepository> configurableDataRepository = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            configurableDataRepository
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(notAdminUser);

            configurableDataRepository
                .Setup(x => x.GetAll<Organisation>())
                .Returns(new[] {privateOrganisationToBeChangedToPublic}.AsQueryable().BuildMock().Object);

            var manualChangesViewModel = new ManualChangesViewModel {
                Command = ConvertPrivateToPublicCommand, Parameters = privateOrganisationToBeChangedToPublic.EmployerReference
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
                    Assert.AreEqual("SUCCESSFULLY TESTED 'Convert private to public': 1 of 1", actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual(
                        "1: EMPLOYERREFERENCE04: Org123 sector Private set to 'Public'\r\n",
                        actualManualChangesViewModel.Results);
                    Assert.AreEqual(ConvertPrivateToPublicCommand, actualManualChangesViewModel.LastTestedCommand);
                    Assert.AreEqual("EmployerReference04", actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be tested=true as this case is running in TEST mode");
                    Assert.IsNull(actualManualChangesViewModel.Comment);
                });
        }

    }

}

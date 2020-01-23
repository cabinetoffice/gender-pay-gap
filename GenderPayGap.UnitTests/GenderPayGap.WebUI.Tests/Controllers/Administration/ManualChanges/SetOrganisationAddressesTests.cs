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

namespace GenderPayGap.WebUI.Tests.Controllers.Administration.ManualChanges
{

    [TestFixture]
    [SetCulture("en-GB")]
    public class SetOrganisationAddressesTests
    {

        private const string MustSupplyOneOrMore = "ERROR: You must supply 1 or more input parameters";
        private const string DoesNotContainEquals = "does not contain '='";
        private const string DuplicateOrganisation = "duplicate organisation";
        private const string CannotFindEmployerRef = "Cannot find organisation with this employer reference";
        private const string HasCompanyNumber = "has a company number so you cannot change this organisation";

        private const string SetOrganisationAddressesCommand = "Set organisation addresses";
        private const string MustContainAddress = "must contain an address entry";
        private const string IncorrectNumberOfAddressFields = "doesnt have the correct number of address fields.";
        private const string Address1IsRequired = "Address1 is required";
        private const string TownCityIsRequired = "Town\\City is required";
        private const string PostcodeIsRequired = "Postcode is required";
        private const string OrganisationMustBeActive = "is not an active organisation so you cannot change its address";

        private const string Address1IsGreaterThan100 = "Address1 is greater than 100 chars";
        private const string Address2IsGreaterThan100 = "Address2 is greater than 100 chars";
        private const string Address3IsGreaterThan100 = "Address3 is greater than 100 chars";
        private const string TownCityIsGreaterThan100 = "Town\\City is greater than 100 chars";
        private const string CountyIsGreaterThan100 = "County is greater than 100 chars";
        private const string CountryIsGreaterThan100 = "Country is greater than 100 chars";
        private const string PostcodeIsGreaterThan100 = "Postcode is greater than 100 chars";

        private Mock<IDataRepository> mockDataRepo;
        private IList<Organisation> testOrgData;
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
                        EmployerReference = "ADCE324T",
                        SectorType = SectorTypes.Public,
                        Status = OrganisationStatuses.Active
                    },
                    new Organisation {
                        OrganisationId = 7,
                        EmployerReference = "GR2H67UI",
                        SectorType = SectorTypes.Public,
                        Status = OrganisationStatuses.Active
                    },
                    new Organisation {
                        OrganisationId = 8,
                        EmployerReference = "FG34RT65",
                        SectorType = SectorTypes.Public,
                        Status = OrganisationStatuses.Active
                    },
                    new Organisation {
                        OrganisationId = 9,
                        EmployerReference = "D43TYU76",
                        SectorType = SectorTypes.Public,
                        Status = OrganisationStatuses.Active
                    },
                    new Organisation {
                        OrganisationId = 10,
                        EmployerReference = "RT56YU34",
                        SectorType = SectorTypes.Public,
                        Status = OrganisationStatuses.Pending
                    }
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
                ManualChangesViewModelHelper.GetMock(SetOrganisationAddressesCommand, thisTestParameters);

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
                ManualChangesViewModelHelper.GetMock(SetOrganisationAddressesCommand, thisTestParameters);

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
                ManualChangesViewModelHelper.GetMock(SetOrganisationAddressesCommand, thisTestParameters);

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
                ManualChangesViewModelHelper.GetMock(SetOrganisationAddressesCommand, thisTestParameters);

            // Act
            var thisManualChangesVr = await thisTestAdminController.ManualChanges(thisTestManualChangesVm) as ViewResult;

            // Assert
            Assert.NotNull(thisManualChangesVr, "Expected ViewResult");

            var actualManualChangesViewModel = thisManualChangesVr.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            string[] actualResults = actualManualChangesViewModel.Results.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);
            Assert.AreEqual(6, actualResults.Length);
            Assert.IsFalse(actualResults[0].Contains(CannotFindEmployerRef), "Expected false when organisation exists");
            Assert.IsTrue(actualResults[1].Contains(CannotFindEmployerRef), "Expected true when organisation doesnt exist");
            Assert.IsFalse(actualResults[2].Contains(CannotFindEmployerRef), "Expected false when organisation exists");
            Assert.IsTrue(actualResults[3].Contains(CannotFindEmployerRef), "Expected true when organisation doesnt exist");
            Assert.IsFalse(actualResults[4].Contains(CannotFindEmployerRef), "Expected false when organisation exists");
        }

        [Test]
        public async Task ShouldFailDuplicateEmployers()
        {
            // Arrange
            var thisTestDbObjects = new object[] {testUser};
            var thisTestAdminController = UiTestHelper.GetController<AdminController>(testUser.UserId, null, thisTestDbObjects);
            string thisTestParameters = " ="
                                        + Environment.NewLine
                                        + string.Join(Environment.NewLine, "EMP1=1", "EMP2=2", "EMP3=3", "EMP2=4", "EMP4=4");

            ManualChangesViewModel thisTestManualChangesVm =
                ManualChangesViewModelHelper.GetMock(SetOrganisationAddressesCommand, thisTestParameters);

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
                "DR994D7L=address",
                "23TYLBLB=",
                "SNGNB4BH=address",
                "RWT2TY62=");

            ManualChangesViewModel thisTestManualChangesVm =
                ManualChangesViewModelHelper.GetMock(SetOrganisationAddressesCommand, thisTestParameters);

            // Act
            var thisManualChangesVr = await thisTestAdminController.ManualChanges(thisTestManualChangesVm) as ViewResult;

            // Assert
            Assert.NotNull(thisManualChangesVr, "Expected ViewResult");

            var actualManualChangesViewModel = thisManualChangesVr.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            string[] actualResults = actualManualChangesViewModel.Results.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);
            Assert.AreEqual(6, actualResults.Length);
            Assert.IsTrue(actualResults[0].Contains(MustContainAddress), "Expected true when line contains a value");
            Assert.IsFalse(actualResults[1].Contains(MustContainAddress), "Expected false when line does not contain a value");
            Assert.IsTrue(actualResults[2].Contains(MustContainAddress), "Expected true when line contains a value");
            Assert.IsFalse(actualResults[3].Contains(MustContainAddress), "Expected false when line does not contain a value");
            Assert.IsTrue(actualResults[4].Contains(MustContainAddress), "Expected true when line contains a value");
        }

        [Test]
        public async Task ShouldFailWhenOrganisationIsNotActive()
        {
            // Arrange
            var thisTestDbObjects = new object[] {testUser, testOrgData};
            var thisTestAdminController = UiTestHelper.GetController<AdminController>(testUser.UserId, null, thisTestDbObjects);
            string thisTestParameters = string.Join(
                "\r\n",
                "RT56YU34=address",
                "DR994D7L=address");

            ManualChangesViewModel thisTestManualChangesVm =
                ManualChangesViewModelHelper.GetMock(SetOrganisationAddressesCommand, thisTestParameters);

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
                "6B2LF57C=address",
                "DR994D7L=address",
                "23TYLBLB=address",
                "SNGNB4BH=address",
                "RWT2TY62=address");

            ManualChangesViewModel thisTestManualChangesVm =
                ManualChangesViewModelHelper.GetMock(SetOrganisationAddressesCommand, thisTestParameters);

            // Act
            var thisManualChangesVr = await thisTestAdminController.ManualChanges(thisTestManualChangesVm) as ViewResult;

            // Assert
            Assert.NotNull(thisManualChangesVr, "Expected ViewResult");

            var actualManualChangesViewModel = thisManualChangesVr.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            string[] actualResults = actualManualChangesViewModel.Results.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);
            Assert.AreEqual(6, actualResults.Length);
            Assert.IsFalse(actualResults[0].Contains(HasCompanyNumber), "Expected false when organisation doesnt have a company number");
            Assert.IsTrue(actualResults[1].Contains(HasCompanyNumber), "Expected true when organisation has a company number");
            Assert.IsFalse(actualResults[2].Contains(HasCompanyNumber), "Expected false when organisation doesnt have a company number");
            Assert.IsTrue(actualResults[3].Contains(HasCompanyNumber), "Expected true when organisation has a company number");
            Assert.IsFalse(actualResults[4].Contains(HasCompanyNumber), "Expected false when organisation doesnt have a company number");
        }

        [Test]
        public async Task ShouldFailWhenMissingAddressEntries()
        {
            // Arrange
            var thisTestDbObjects = new object[] {testUser, testOrgData};
            var thisTestAdminController = UiTestHelper.GetController<AdminController>(testUser.UserId, null, thisTestDbObjects);
            string thisTestParameters = string.Join(
                "\r\n",
                "6B2LF57C=a1,a2",
                "DR994D7L=a1,a2,a3,t1,c1,c2,pc1",
                "23TYLBLB=1,2,3,4",
                "SNGNB4BH=a1,a2,a3,t1,c1,c2,pc1",
                "RWT2TY62=1,2,3,4,5");

            ManualChangesViewModel thisTestManualChangesVm =
                ManualChangesViewModelHelper.GetMock(SetOrganisationAddressesCommand, thisTestParameters);

            // Act
            var thisManualChangesVr = await thisTestAdminController.ManualChanges(thisTestManualChangesVm) as ViewResult;

            // Assert
            Assert.NotNull(thisManualChangesVr, "Expected ViewResult");

            var actualManualChangesViewModel = thisManualChangesVr.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            string[] actualResults = actualManualChangesViewModel.Results.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);
            Assert.AreEqual(6, actualResults.Length);
            Assert.IsTrue(actualResults[0].Contains(IncorrectNumberOfAddressFields), "Expected true when Sic code is not an integer");
            Assert.IsFalse(actualResults[1].Contains(IncorrectNumberOfAddressFields), "Expected false when Sic code is an integer");
            Assert.IsTrue(actualResults[2].Contains(IncorrectNumberOfAddressFields), "Expected true when Sic code is not an integer");
            Assert.IsFalse(actualResults[3].Contains(IncorrectNumberOfAddressFields), "Expected false when Sic code is an integer");
            Assert.IsTrue(actualResults[4].Contains(IncorrectNumberOfAddressFields), "Expected true when Sic code is not an integer");
        }

        [Test]
        public async Task ShouldFailWhenRequiredFieldIsMissing()
        {
            // Arrange
            var thisTestDbObjects = new object[] {testUser, testOrgData};
            var thisTestAdminController = UiTestHelper.GetController<AdminController>(testUser.UserId, null, thisTestDbObjects);
            string thisTestParameters = string.Join(
                "\r\n",
                "6B2LF57C=,a2,a3,t1,c1,c2,pc1",
                "23TYLBLB=a1,a2,a3,,c1,c2,pc1",
                "RWT2TY62=a1,a2,a3,t1,c1,c2,");

            ManualChangesViewModel thisTestManualChangesVm =
                ManualChangesViewModelHelper.GetMock(SetOrganisationAddressesCommand, thisTestParameters);

            // Act
            var thisManualChangesVr = await thisTestAdminController.ManualChanges(thisTestManualChangesVm) as ViewResult;

            // Assert
            Assert.NotNull(thisManualChangesVr, "Expected ViewResult");

            var actualManualChangesViewModel = thisManualChangesVr.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            string[] actualResults = actualManualChangesViewModel.Results.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);
            Assert.AreEqual(4, actualResults.Length);
            Assert.IsTrue(actualResults[0].Contains(Address1IsRequired), "Expected true when address1 is missing");
            Assert.IsTrue(actualResults[1].Contains(TownCityIsRequired), "Expected true when town\\city is missing");
            Assert.IsTrue(actualResults[2].Contains(PostcodeIsRequired), "Expected true when postcode is missing");
        }

        [Test]
        public async Task ShouldFailWhenAddressFieldLongerThan100()
        {
            // Arrange
            var thisTestDbObjects = new object[] {testUser, testOrgData};
            var thisTestAdminController = UiTestHelper.GetController<AdminController>(testUser.UserId, null, thisTestDbObjects);
            string thisTestParameters = string.Join(
                "\r\n",
                $"6B2LF57C={"".PadLeft(101, 'z')},a2,a3,t1,c1,c2,pc1",
                $"23TYLBLB=a1,{"".PadLeft(101, 'z')},a3,t1,c1,c2,pc1",
                $"RWT2TY62=a1,a2,{"".PadLeft(101, 'z')},t1,c1,c2,pc1",
                $"ADCE324T=a1,a2,a3,{"".PadLeft(101, 'z')},c1,c2,pc1",
                $"GR2H67UI=a1,a2,a3,t1,{"".PadLeft(101, 'z')},c2,pc1",
                $"FG34RT65=a1,a2,a3,t1,c1,{"".PadLeft(101, 'z')},pc1",
                $"D43TYU76=a1,a2,a3,t1,c1,c2,{"".PadLeft(101, 'z')}");

            ManualChangesViewModel thisTestManualChangesVm =
                ManualChangesViewModelHelper.GetMock(SetOrganisationAddressesCommand, thisTestParameters);

            // Act
            var thisManualChangesVr = await thisTestAdminController.ManualChanges(thisTestManualChangesVm) as ViewResult;

            // Assert
            Assert.NotNull(thisManualChangesVr, "Expected ViewResult");

            var actualManualChangesViewModel = thisManualChangesVr.Model as ManualChangesViewModel;
            Assert.NotNull(actualManualChangesViewModel, "Expected ManualChangesViewModel");

            string[] actualResults = actualManualChangesViewModel.Results.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);
            Assert.AreEqual(8, actualResults.Length);
            Assert.IsTrue(actualResults[0].Contains(Address1IsGreaterThan100), "Expected true when address1 is greater 100 chars");
            Assert.IsTrue(actualResults[1].Contains(Address2IsGreaterThan100), "Expected true when address2 is greater 100 chars");
            Assert.IsTrue(actualResults[2].Contains(Address3IsGreaterThan100), "Expected true when address3 is greater 100 chars");
            Assert.IsTrue(actualResults[3].Contains(TownCityIsGreaterThan100), "Expected true when town\\city is greater 100 chars");
            Assert.IsTrue(actualResults[4].Contains(CountyIsGreaterThan100), "Expected true when county is greater 100 chars");
            Assert.IsTrue(actualResults[5].Contains(CountryIsGreaterThan100), "Expected true when country is greater 100 chars");
            Assert.IsTrue(actualResults[6].Contains(PostcodeIsGreaterThan100), "Expected true when postcode is greater 100 chars");
        }

        [Test]
        public async Task ShouldReplaceEmployerAddress()
        {
            // Arrange
            var thisTestDbObjects = new object[] {testUser, testOrgData};
            var thisTestAdminController = UiTestHelper.GetController<AdminController>(testUser.UserId, null, thisTestDbObjects);
            string thisTestParameters = string.Join(
                "\r\n",
                "6B2LF57C=Some House,1 Some Street,,Some Town,,,PC1 11RT",
                "D43TYU76=PO BOX 12,,,Another Town,,,PC2 55RT");

            ManualChangesViewModel thisTestManualChangesVm =
                ManualChangesViewModelHelper.GetMock(SetOrganisationAddressesCommand, thisTestParameters);
            thisTestManualChangesVm.LastTestedCommand = SetOrganisationAddressesCommand;
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
            Assert.AreEqual("Some House, 1 Some Street, Some Town, PC1 11RT", org1.LatestAddress.GetAddressString());
            Assert.AreEqual(
                "1: 6B2LF57C: Address=No previous address has been set to Some House,1 Some Street,,Some Town,,,PC1 11RT",
                actualResults[0]);

            Organisation org2 = thisTestAdminController.DataRepository.GetAll<Organisation>()
                .SingleOrDefault(x => x.EmployerReference == "D43TYU76");
            Assert.AreEqual("PO BOX 12, Another Town, PC2 55RT", org2.LatestAddress.GetAddressString());
            Assert.AreEqual(
                "2: D43TYU76: Address=No previous address has been set to PO BOX 12,,,Another Town,,,PC2 55RT",
                actualResults[1]);
        }


        [Test]
        public async Task AdministrationController_ManualChanges_prevAddress_SetStatus_Address_Statuses_Retired()
        {
            // Arrange
            var existingAddressExpectedToBeRetired = new OrganisationAddress {
                Address1 = "Previous Address1",
                Address2 = "Previous Address2",
                Address3 = "Previous Address3",
                TownCity = "Previous TownCity",
                County = "Previous County",
                Country = "Previous Country",
                PostCode = "Previous PostCode",
                Status = AddressStatuses.Active
            };
            Organisation privateOrgWhoseAddressWillBeChanged = OrganisationHelper.GetPrivateOrganisation("Employer_Reference_989898");
            privateOrgWhoseAddressWillBeChanged.LatestAddress = existingAddressExpectedToBeRetired;

            #region setting up database and controller 

            User notAdminUser = UserHelper.GetDatabaseAdmin();
            var adminController = UiTestHelper.GetController<AdminController>(notAdminUser.UserId, null, null);

            Mock<IDataRepository> configurableDataRepository = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            configurableDataRepository
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(notAdminUser);

            configurableDataRepository
                .Setup(x => x.GetAll<Organisation>())
                .Returns(new[] {privateOrgWhoseAddressWillBeChanged}.AsQueryable().BuildMock().Object);

            var manualChangesViewModel = new ManualChangesViewModel {
                Command = SetOrganisationAddressesCommand,
                Parameters =
                    $"{privateOrgWhoseAddressWillBeChanged.EmployerReference}=New Address1, New Address2, New Address3, New TownCity, New County, New Country, New PostCode"
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
                        "SUCCESSFULLY TESTED 'Set organisation addresses': 1 of 1",
                        actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual(
                        "1: EMPLOYER_REFERENCE_989898:Org123 Address=Previous Address1, Previous Address2, Previous Address3, Previous TownCity, Previous County, Previous Country, Previous PostCode will be set to New Address1, New Address2, New Address3, New TownCity, New County, New Country, New PostCode\r\n",
                        actualManualChangesViewModel.Results);
                    Assert.AreEqual("Set organisation addresses", actualManualChangesViewModel.LastTestedCommand);
                    Assert.AreEqual(
                        "Employer_Reference_989898=New Address1, New Address2, New Address3, New TownCity, New County, New Country, New PostCode",
                        actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be tested=true as this case is running in TEST mode");
                    Assert.IsNull(actualManualChangesViewModel.Comment);
                });
        }

    }

}

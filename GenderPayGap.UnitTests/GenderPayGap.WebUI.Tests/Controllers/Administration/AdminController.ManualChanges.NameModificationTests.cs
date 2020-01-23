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
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Controllers.Administration
{
    [TestFixture]
    public class AdminControllerManualChangesNameModificationTests
    {

        private const string FixOrganisationNamesCommand = "Fix organisation names";
        private const string AddOrganisationsLatestNameCommand = "Add organisations latest name";
        private const string ResetOrganisationToOnlyOriginalNameCommand = "Reset organisation to only original name";

        [Test]
        public async Task AdminController_ManualChanges_POST_Fix_Organisation_Names_Works_When_Run_In_Test_Mode_Async()
        {
            // Arrange
            var orgThatWillBeFixed = new OrganisationName {Name = "Org to fix ltd", Created = VirtualDateTime.Now, OrganisationId = 33};

            var orgThatWontBeSelectedToBeFixed = new OrganisationName {
                Name = "Org not to be fixed", Created = VirtualDateTime.Now, OrganisationId = 44
            };

            #region setting up database and controller 

            User notAdminUser = UserHelper.GetDatabaseAdmin();
            var adminController = UiTestHelper.GetController<AdminController>(notAdminUser.UserId, null, null);

            Mock<IDataRepository> configurableDataRepository = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            configurableDataRepository
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(notAdminUser);

            configurableDataRepository
                .Setup(x => x.GetAll<OrganisationName>())
                .Returns(new[] {orgThatWillBeFixed, orgThatWontBeSelectedToBeFixed}.AsQueryable().BuildMock().Object);

            var manualChangesViewModel = new ManualChangesViewModel {Command = FixOrganisationNamesCommand};

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
                    Assert.AreEqual("SUCCESSFULLY TESTED 'Fix organisation names': 1 items", actualManualChangesViewModel.SuccessMessage);
                    Assert.AreEqual("", actualManualChangesViewModel.Results);
                    Assert.AreEqual(FixOrganisationNamesCommand, actualManualChangesViewModel.LastTestedCommand);
                    Assert.IsNull(actualManualChangesViewModel.LastTestedInput);
                    Assert.True(actualManualChangesViewModel.Tested, "Must be tested=true as this case is running in TEST mode");
                    Assert.IsNull(actualManualChangesViewModel.Comment);
                });
        }

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

    }
}

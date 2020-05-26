using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
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

        private const string ConvertPublicToPrivateCommand = "Convert public to private";
        private const string ConvertPrivateToPublicCommand = "Convert private to public";

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

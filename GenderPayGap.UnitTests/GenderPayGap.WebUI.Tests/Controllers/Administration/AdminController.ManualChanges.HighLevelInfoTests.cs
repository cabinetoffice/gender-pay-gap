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
    public class AdminControllerManualChangesHighLevelInfoTests
    {
        private const string SetOrganisationCompanyNumberCommand = "Set organisation company number";

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

    }
}

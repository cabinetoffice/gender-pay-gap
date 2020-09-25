using System.Threading.Tasks;
using GenderPayGap.Database;
using GenderPayGap.Tests.Common.Classes;
using GenderPayGap.Tests.TestHelpers;
using GenderPayGap.WebUI.Areas.Account.Resources;
using GenderPayGap.WebUI.Areas.Account.ViewModels;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using NUnit.Framework;

namespace Account.Controllers.ChangeDetailsController
{

    public class ChangeDetailsTests
    {

        private RouteData mockRouteData;

        [SetUp]
        public void BeforeEach()
        {
            mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", nameof(GenderPayGap.WebUI.Areas.Account.Controllers.ChangeDetailsController.ChangeDetails));
            mockRouteData.Values.Add("Controller", "ChangeDetails");
        }

        [Test]
        public void GET_RedirectUnauthorizedUsersToSignIn()
        {
            // Arrange
            User unverifiedUser = UserHelper.GetNotAdminUserWithoutVerifiedEmailAddress();
            var controller =
                UiTestHelper.GetController<GenderPayGap.WebUI.Areas.Account.Controllers.ChangeDetailsController>(
                    0,
                    mockRouteData,
                    unverifiedUser);

            // Act
            IActionResult actionResult = controller.ChangeDetails();

            // Assert
            Assert.NotNull(actionResult);
            Assert.IsInstanceOf(typeof(ChallengeResult), actionResult);
        }

        [Test]
        public void GET_ReturnsChangeDetailsViewWhenUserIsAuthorized()
        {
            // Arrange
            User verifiedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            var controller =
                UiTestHelper.GetController<GenderPayGap.WebUI.Areas.Account.Controllers.ChangeDetailsController>(
                    verifiedUser.UserId,
                    mockRouteData,
                    verifiedUser);

            // Act
            var viewResult = controller.ChangeDetails() as ViewResult;

            // Assert
            Assert.NotNull(viewResult);
            Assert.IsInstanceOf(typeof(ChangeDetailsViewModel), viewResult.Model);

            var actualModel = (ChangeDetailsViewModel) viewResult.Model;
            actualModel.Compare(verifiedUser, caseSensitive: false);
        }

        [Test]
        public async Task POST_ReturnsChangeDetailsViewWhenModelStateIsInValid()
        {
            // Arrange
            User verifiedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            var controller =
                UiTestHelper.GetController<GenderPayGap.WebUI.Areas.Account.Controllers.ChangeDetailsController>(
                    verifiedUser.UserId,
                    mockRouteData,
                    verifiedUser);
            var model = new ChangeDetailsViewModel();

            // Act
            controller.ModelState.AddModelError("FirstName", "Required");
            var viewResult = controller.ChangeDetails(model) as ViewResult;

            // Assert
            Assert.NotNull(viewResult);
            Assert.AreEqual(
                nameof(GenderPayGap.WebUI.Areas.Account.Controllers.ChangeDetailsController.ChangeDetails),
                viewResult.ViewName);
            Assert.IsFalse(controller.TempData.ContainsKey(AccountResources.ChangeDetailsSuccessAlert));
            Assert.IsInstanceOf(typeof(ChangeDetailsViewModel), viewResult.Model);
        }

    }
}

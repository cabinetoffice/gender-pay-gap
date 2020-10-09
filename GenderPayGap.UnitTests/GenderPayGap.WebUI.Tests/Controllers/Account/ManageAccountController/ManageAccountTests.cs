using GenderPayGap.Database;
using GenderPayGap.Tests.Common.Classes;
using GenderPayGap.Tests.TestHelpers;
using GenderPayGap.WebUI.Areas.Account.Resources;
using GenderPayGap.WebUI.Areas.Account.ViewModels;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using NUnit.Framework;

namespace Account.Controllers.ManageAccountController
{

    public class ManageAccountTests
    {

        private static object[] AddsSuccessAlertsToViewBagCases = {
            new object[] {nameof(AccountResources.ChangeDetailsSuccessAlert), AccountResources.ChangeDetailsSuccessAlert}
        };

        private RouteData mockRouteData;

        [SetUp]
        public void BeforeEach()
        {
            mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", nameof(GenderPayGap.WebUI.Areas.Account.Controllers.ManageAccountController.ManageAccount));
            mockRouteData.Values.Add("Controller", "ManageAccount");
        }

        [Test]
        public void GET_RedirectUnauthorizedUsersToSignIn()
        {
            // Arrange
            User unverifiedUser = UserHelper.GetNotAdminUserWithoutVerifiedEmailAddress();
            var controller =
                UiTestHelper.GetController<GenderPayGap.WebUI.Areas.Account.Controllers.ManageAccountController>(
                    0,
                    mockRouteData,
                    unverifiedUser);

            // Act
            IActionResult actionResult = controller.ManageAccount();

            // Assert
            Assert.NotNull(actionResult);
            Assert.IsInstanceOf(typeof(ChallengeResult), actionResult);
        }

        [Test]
        public void GET_ReturnsUserAccountWhenUserIsAuthorized()
        {
            // Arrange
            User verifiedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            var controller =
                UiTestHelper.GetController<GenderPayGap.WebUI.Areas.Account.Controllers.ManageAccountController>(
                    verifiedUser.UserId,
                    mockRouteData,
                    verifiedUser);

            // Act
            var viewResult = controller.ManageAccount() as ViewResult;

            // Assert
            Assert.NotNull(viewResult);
            Assert.IsInstanceOf(typeof(ManageAccountViewModel), viewResult.Model);

            var actualModel = (ManageAccountViewModel) viewResult.Model;
            actualModel.Compare(verifiedUser, caseSensitive: false);
        }

        [TestCaseSource(nameof(AddsSuccessAlertsToViewBagCases))]
        public void AddsSuccessAlertsToViewBag(string testSuccessAlert, string expectedAlertMessage)
        {
            // Arrange
            User verifiedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            var controller =
                UiTestHelper.GetController<GenderPayGap.WebUI.Areas.Account.Controllers.ManageAccountController>(
                    verifiedUser.UserId,
                    mockRouteData,
                    verifiedUser);
            controller.TempData.Add(testSuccessAlert, "");

            // Act
            var viewResult = controller.ManageAccount() as ViewResult;

            // Assert
            Assert.NotNull(viewResult);
            Assert.IsTrue(viewResult.ViewData.ContainsKey("ChangeSuccessMessage"), "Expected change success key to exist");
            Assert.IsTrue(viewResult.ViewData.Values.Contains(expectedAlertMessage), "Expected change success value to match");
        }

    }

}

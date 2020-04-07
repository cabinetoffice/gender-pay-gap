using System.Threading.Tasks;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Tests.TestHelpers;
using GenderPayGap.WebUI;
using GenderPayGap.WebUI.Areas.Account.ViewModels;
using GenderPayGap.WebUI.Services;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;

namespace Account.Controllers.ChangeEmailController
{

    public class ChangeEmailTests
    {

        private RouteData mockRouteData;

        [SetUp]
        public void BeforeEach()
        {
            mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", nameof(GenderPayGap.WebUI.Areas.Account.Controllers.ChangeEmailController.ChangeEmail));
            mockRouteData.Values.Add("Controller", "ChangeEmail");
        }

        [Test]
        public void GET_RedirectsUnauthorizedUsersToSignIn()
        {
            // Arrange
            User unverifiedUser = UserHelper.GetNotAdminUserWithoutVerifiedEmailAddress();
            var controller =
                UiTestHelper.GetController<GenderPayGap.WebUI.Areas.Account.Controllers.ChangeEmailController>(
                    0,
                    mockRouteData,
                    unverifiedUser);

            // Act
            IActionResult actionResult = controller.ChangeEmail();

            // Assert
            Assert.NotNull(actionResult);
            Assert.IsInstanceOf(typeof(ChallengeResult), actionResult);
        }

        [Test]
        public async Task POST_RedirectsUnauthorizedUsersToSignIn()
        {
            // Arrange
            User unverifiedUser = UserHelper.GetNotAdminUserWithoutVerifiedEmailAddress();
            var controller =
                UiTestHelper.GetController<GenderPayGap.WebUI.Areas.Account.Controllers.ChangeEmailController>(
                    0,
                    mockRouteData,
                    unverifiedUser);

            // Act
            IActionResult actionResult = await controller.ChangeEmail(new ChangeEmailViewModel());

            // Assert
            Assert.NotNull(actionResult);
            Assert.IsInstanceOf(typeof(ChallengeResult), actionResult);
        }

        [Test]
        public async Task POST_SendsChangeEmailPendingVerification()
        {
            // Arrange
            var testNewEmail = "newemail@testemail.com";
            User verifiedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            var controller =
                UiTestHelper.GetController<GenderPayGap.WebUI.Areas.Account.Controllers.ChangeEmailController>(
                    verifiedUser.UserId,
                    mockRouteData,
                    verifiedUser);

            var mockNotifyEmailQueue = new Mock<IQueue>();
            Program.MvcApplication.SendNotifyEmailQueue = mockNotifyEmailQueue.Object;
            mockNotifyEmailQueue
                .Setup(q => q.AddMessageAsync(It.IsAny<NotifyEmail>()));

            var model = new ChangeEmailViewModel {EmailAddress = testNewEmail, ConfirmEmailAddress = testNewEmail};

            // Act
            var redirectToActionResult = await controller.ChangeEmail(model) as RedirectToActionResult;

            // Assert
            mockNotifyEmailQueue.Verify(
                x => x.AddMessageAsync(
                    It.Is<NotifyEmail>(inst => inst.TemplateId.Contains(EmailTemplates.SendChangeEmailPendingVerificationEmail))),
                Times.Once(),
                $"Expected the correct templateId to be in the email send queue, expected {EmailTemplates.SendChangeEmailPendingVerificationEmail}");
            mockNotifyEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<NotifyEmail>(inst => inst.EmailAddress.Contains(testNewEmail))),
                Times.Once(),
                "Expected the current user's email address to be in the email send queue");

            Assert.NotNull(redirectToActionResult);
            Assert.AreEqual(
                nameof(GenderPayGap.WebUI.Areas.Account.Controllers.ChangeEmailController.ChangeEmailPending),
                redirectToActionResult.ActionName);
        }

    }

}

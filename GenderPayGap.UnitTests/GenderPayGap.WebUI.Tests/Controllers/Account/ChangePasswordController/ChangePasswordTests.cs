using System;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Tests.TestHelpers;
using GenderPayGap.WebUI;
using GenderPayGap.WebUI.Areas.Account.ViewModels;
using GenderPayGap.WebUI.ExternalServices;
using GenderPayGap.WebUI.Services;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;

namespace Account.Controllers.ChangePasswordController
{

    public class ChangePasswordTests
    {

        private RouteData mockRouteData;

        [SetUp]
        public void BeforeEach()
        {
            mockRouteData = new RouteData();
            mockRouteData.Values.Add(
                "Action",
                nameof(GenderPayGap.WebUI.Areas.Account.Controllers.ChangePasswordController.ChangePassword));
            mockRouteData.Values.Add("Controller", "ChangePassword");
        }

        [Test]
        public void GET_RedirectsUnauthorizedUsersToSignIn()
        {
            // Arrange
            User unverifiedUser = UserHelper.GetNotAdminUserWithoutVerifiedEmailAddress();
            var controller =
                UiTestHelper.GetController<GenderPayGap.WebUI.Areas.Account.Controllers.ChangePasswordController>(
                    0,
                    mockRouteData,
                    unverifiedUser);

            // Act
            IActionResult actionResult = controller.ChangePassword();

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
                UiTestHelper.GetController<GenderPayGap.WebUI.Areas.Account.Controllers.ChangePasswordController>(
                    0,
                    mockRouteData,
                    unverifiedUser);

            // Act
            IActionResult actionResult = await controller.ChangePassword(new ChangePasswordViewModel());

            // Assert
            Assert.NotNull(actionResult);
            Assert.IsInstanceOf(typeof(ChallengeResult), actionResult);
        }

        [Test]
        public async Task POST_ChangesPasswordAndSendsCompletedNotification()
        {
            var testOldPassword = "OldPassword123";
            var testNewPassword = "NewPassword123";
            var salt = "TestSalt";
            User verifiedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            verifiedUser.PasswordHash = Crypto.GetPBKDF2(testOldPassword, Convert.FromBase64String(salt));
            verifiedUser.Salt = salt;
            verifiedUser.HashingAlgorithm = HashingAlgorithm.PBKDF2;
            var controller =
                UiTestHelper.GetController<GenderPayGap.WebUI.Areas.Account.Controllers.ChangePasswordController>(
                    verifiedUser.UserId,
                    mockRouteData,
                    verifiedUser);
            
            UiTestHelper.MockBackgroundJobsApi
                .Setup(q => q.AddEmailToQueue(It.IsAny<NotifyEmail>()));

            var model = new ChangePasswordViewModel {
                CurrentPassword = testOldPassword, NewPassword = testNewPassword, ConfirmNewPassword = testNewPassword
            };

            // Act
            var redirectToActionResult = await controller.ChangePassword(model) as RedirectToActionResult;

            // Assert
            UiTestHelper.MockBackgroundJobsApi.Verify(
                x => x.AddEmailToQueue(
                    It.Is<NotifyEmail>(inst => inst.TemplateId.Contains(EmailTemplates.SendChangePasswordCompletedEmail))),
                Times.Once(),
                $"Expected the correct templateId to be in the email send queue, expected {EmailTemplates.SendChangePasswordCompletedEmail}");
            UiTestHelper.MockBackgroundJobsApi.Verify(
                x => x.AddEmailToQueue(It.Is<NotifyEmail>(inst => inst.EmailAddress.Contains(verifiedUser.EmailAddress))),
                Times.Once(),
                "Expected the current user's email address to be in the email send queue");

            Assert.NotNull(redirectToActionResult);
            Assert.AreEqual(
                nameof(GenderPayGap.WebUI.Areas.Account.Controllers.ManageAccountController.ManageAccount),
                redirectToActionResult.ActionName);

            Assert.AreEqual(controller.CurrentUser.PasswordHash, Crypto.GetPBKDF2(testNewPassword, Convert.FromBase64String(controller.CurrentUser.Salt)), "Expected password to be updated");
        }

    }

}

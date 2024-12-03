using GenderPayGap.Core;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Controllers.Account;
using GenderPayGap.WebUI.ExternalServices;
using GenderPayGap.WebUI.Models.AccountCreation;
using GenderPayGap.WebUI.Services;
using GenderPayGap.WebUI.Tests.Builders;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Tests.Controllers.Account
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class AccountCreationTests
    {

        [SetUp]
        public void SetUp()
        {
            UiTestHelper.SetDefaultEncryptionKeys();
        }
        
        [Test]
        [Description("POST: Verification email is sent after creating a user account")]
        public void POST_Verification_Email_Is_Sent_After_Creating_User_Account()
        {
            // Arrange
            var controllerBuilder = new ControllerBuilder<AccountCreationController>();
            var controller = controllerBuilder
                .WithMockUriHelper()
                .Build();

            // Act
            var response = (ViewResult) controller.CreateUserAccountPost(new CreateUserAccountViewModel
            {
                EmailAddress = "test@example.com",
                ConfirmEmailAddress = "test@example.com",
                FirstName = "Test",
                LastName = "Example",
                JobTitle = "JobTitle",
                Password = "Pa55word",
                ConfirmPassword = "Pa55word",
                SendUpdates = true,
                AllowContact = false
            });

            // Assert
            Assert.AreEqual("ConfirmEmailAddress", response.ViewName);

            Assert.AreEqual(1, controllerBuilder.EmailsSent.Count);
            NotifyEmail emailSent = controllerBuilder.EmailsSent[0];

            Assert.AreEqual("test@example.com", emailSent.EmailAddress);
            Assert.AreEqual(EmailTemplates.AccountVerificationEmail, emailSent.TemplateId);
        }
        
        [Test]
        [Description("GET: Clicking link in verification email confirms user")]
        public void GET_Clicking_Link_In_Verification_Email_Confirms_User()
        {
            // Arrange
            var user = new User
            {
                UserId = 1,
                EmailAddress = "test@example.com",
                Firstname = "Test",
                Lastname = "Example",
                JobTitle = "JobTitle",
                EmailVerifySendDate = VirtualDateTime.Now,
                EmailVerifyHash = Guid.NewGuid().ToString("N"),
                Status = UserStatuses.New,
                PasswordHash = "PasswordHash"
            };

            var controller = new ControllerBuilder<AccountCreationController>()
                .WithDatabaseObjects(user)
                .Build();

            // Act
            var response = (RedirectToActionResult) controller.VerifyEmail(user.EmailVerifyHash);

            // Assert
            Assert.AreEqual("AccountCreationConfirmation", response.ActionName);
            Assert.AreEqual(user.Status, UserStatuses.Active);
        }

    }

}

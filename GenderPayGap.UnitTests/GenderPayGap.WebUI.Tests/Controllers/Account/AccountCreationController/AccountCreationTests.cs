using System;
using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.ExternalServices;
using GenderPayGap.WebUI.Models.AccountCreation;
using GenderPayGap.WebUI.Services;
using GenderPayGap.WebUI.Tests.Builders;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Controllers.Account.AccountCreationController
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class AccountCreationTests
    {
        
        [Test]
        [Description("POST: Verification email is sent after creating a user account")]
        public void POST_Verification_Email_Is_Sent_After_Creating_User_Account()
        {
            // Arrange
            var requestFormValues = new Dictionary<string, StringValues>();
            requestFormValues.Add("GovUk_Text_EmailAddress", "test@example.com");
            requestFormValues.Add("GovUk_Text_ConfirmEmailAddress", "test@example.com");
            requestFormValues.Add("GovUk_Text_FirstName", "Test");
            requestFormValues.Add("GovUk_Text_LastName", "Example");
            requestFormValues.Add("GovUk_Text_JobTitle", "Tester");
            requestFormValues.Add("GovUk_Text_Password", "Pa55word");
            requestFormValues.Add("GovUk_Text_ConfirmPassword", "Pa55word");
            requestFormValues.Add("GovUk_Checkbox_SendUpdates", "true");
            requestFormValues.Add("GovUk_Checkbox_AllowContact", "false");

            var controller = new ControllerBuilder<WebUI.Controllers.Account.AccountCreationController>()
                .WithRequestFormValues(requestFormValues)
                .Build();

            // Required to mock out the Url object when creating the verification URL
            controller.AddMockUriHelperNew(new Uri("https://localhost:44371/mockURL").ToString());

            NewUiTestHelper.MockBackgroundJobsApi
                .Setup(q => q.AddEmailToQueue(It.IsAny<NotifyEmail>()));

            // Act
            var response = (ViewResult) controller.CreateUserAccountPost(new CreateUserAccountViewModel());

            // Assert
            Assert.AreEqual("ConfirmEmailAddress", response.ViewName);

            NewUiTestHelper.MockBackgroundJobsApi.Verify(
                x => x.AddEmailToQueue(It.Is<NotifyEmail>(inst => inst.EmailAddress.Contains("test@example.com"))),
                Times.Once(),
                "Expected the existingUser1's email address to be in the email send queue");

            NewUiTestHelper.MockBackgroundJobsApi.Verify(
                x => x.AddEmailToQueue(It.Is<NotifyEmail>(inst => inst.TemplateId.Contains(EmailTemplates.AccountVerificationEmail))),
                Times.Exactly(1),
                $"Expected the correct templateId to be in the email send queue, expected {EmailTemplates.AccountVerificationEmail}");
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
                EmailVerifySendDate = VirtualDateTime.Now,
                EmailVerifyHash = Guid.NewGuid().ToString("N"),
                Status = UserStatuses.New
            };

            var controller = new ControllerBuilder<WebUI.Controllers.Account.AccountCreationController>()
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

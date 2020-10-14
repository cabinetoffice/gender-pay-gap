using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Controllers.Account;
using GenderPayGap.WebUI.Models.Account;
using GenderPayGap.WebUI.Services;
using GenderPayGap.WebUI.Tests.Builders;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Controllers.Account
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class ChangeEmailTests
    {

        [Test]
        [Description("POST: Providing a valid new email address results in a verification email being sent")]
        public void POST_Providing_A_Valid_New_Email_Address_Results_In_A_Verification_Email_Being_Sent()
        {
            // Arrange
            User user = new UserBuilder().WithEmailAddress("old@example.com").Build();
            
            var requestFormValues = new Dictionary<string, StringValues>();
            requestFormValues.Add("GovUk_Text_NewEmailAddress", "new@example.com");

            var controllerBuilder = new ControllerBuilder<ChangeEmailController>();
            var controller = controllerBuilder
                .WithLoggedInUser(user)
                .WithRequestFormValues(requestFormValues)
                .WithDatabaseObjects(user)
                .WithMockUriHelper()
                .Build();
            
            // Act
            controller.ChangeEmailPost(new ChangeEmailViewModel());
            
            // Assert
            Assert.AreEqual(1, controllerBuilder.EmailsSent.Count);

            var email = controllerBuilder.EmailsSent.FirstOrDefault();
            Assert.NotNull(email);
            Assert.AreEqual(EmailTemplates.SendChangeEmailPendingVerificationEmail, email.TemplateId);
            Assert.AreEqual("new@example.com", email.EmailAddress);
        }
        
        [Test]
        [Description("POST: Trying to change email address to current email address does not send verification email")]
        public void POST_Trying_To_Change_Email_Address_To_Current_Email_Address_Does_Not_Send_Verification_Email()
        {
            // Arrange
            User user = new UserBuilder().WithEmailAddress("old@example.com").Build();
            
            var requestFormValues = new Dictionary<string, StringValues>();
            requestFormValues.Add("GovUk_Text_NewEmailAddress", "old@example.com");

            var controllerBuilder = new ControllerBuilder<ChangeEmailController>();
            var controller = controllerBuilder
                .WithLoggedInUser(user)
                .WithRequestFormValues(requestFormValues)
                .WithDatabaseObjects(user)
                .WithMockUriHelper()
                .Build();
            
            // Act
            controller.ChangeEmailPost(new ChangeEmailViewModel());
            
            // Assert
            Assert.AreEqual(0, controllerBuilder.EmailsSent.Count);
        }
        
        [Test]
        [Description("POST: Trying to change email address to other user's email address does not send verification email")]
        public void POST_Trying_To_Change_Email_Address_To_Other_Users_Email_Address_Does_Not_Send_Verification_Email()
        {
            // Arrange
            User user = new UserBuilder().WithEmailAddress("user@example.com").Build();
            User user2 = new UserBuilder().WithEmailAddress("user2@example.com").Build();
            
            var requestFormValues = new Dictionary<string, StringValues>();
            requestFormValues.Add("GovUk_Text_NewEmailAddress", "user2@example.com");

            var controllerBuilder = new ControllerBuilder<ChangeEmailController>();
            var controller = controllerBuilder
                .WithLoggedInUser(user)
                .WithRequestFormValues(requestFormValues)
                .WithDatabaseObjects(user, user2)
                .WithMockUriHelper()
                .Build();
            
            // Act
            controller.ChangeEmailPost(new ChangeEmailViewModel());
            
            // Assert
            Assert.AreEqual(0, controllerBuilder.EmailsSent.Count);
        }

        [Test]
        [Description("POST: User can verify their email address and confirm password to change email address")]
        public void POST_User_Can_Verify_Their_Email_Address_And_Confirm_Password_To_Change_Email_Address()
        {
            // Arrange
            User user = new UserBuilder().WithEmailAddress("old@example.com").WithPassword("password").Build();
            
            var requestFormValues = new Dictionary<string, StringValues>();
            requestFormValues.Add("GovUk_Text_Password", "password");
            
            var controllerBuilder = new ControllerBuilder<ChangeEmailController>();
            var controller = controllerBuilder
                .WithLoggedInUser(user)
                .WithRequestFormValues(requestFormValues)
                .WithDatabaseObjects(user)
                .WithMockUriHelper()
                .Build();

            var emailVerificationCode = Encryption.EncryptModel(
                new ChangeEmailVerificationToken
                {
                    UserId = user.UserId,
                    NewEmailAddress = "new@example.com".ToLower(),
                    TokenTimestamp = VirtualDateTime.Now
                });

            var viewModel = new VerifyEmailChangeViewModel {NewEmailAddress = "new@example.com", Code = emailVerificationCode, User = user};
            
            // Act
            controller.VerifyEmailPost(viewModel);
            
            // Assert
            Assert.AreEqual("new@example.com", user.EmailAddress);
            
            var auditLogs = controllerBuilder.DataRepository.GetAll<AuditLog>();
            Assert.AreEqual(1, auditLogs.Count());

            var auditLog = auditLogs.FirstOrDefault();
            Assert.NotNull(auditLog);
            Assert.AreEqual(AuditedAction.UserChangeEmailAddress, auditLog.Action);
            
            Assert.AreEqual(2, controllerBuilder.EmailsSent.Count);

            var oldEmailNotifications = controllerBuilder.EmailsSent.Where(e => e.EmailAddress == "old@example.com").ToList();
            Assert.AreEqual(1, oldEmailNotifications.Count);
            
            var oldEmailNotification = oldEmailNotifications.FirstOrDefault();
            Assert.AreEqual(EmailTemplates.SendChangeEmailCompletedNotificationEmail, oldEmailNotification.TemplateId);
            
            var newEmailNotifications = controllerBuilder.EmailsSent.Where(e => e.EmailAddress == "new@example.com").ToList();
            Assert.AreEqual(1, newEmailNotifications.Count);
            
            var newEmailNotification = newEmailNotifications.FirstOrDefault();
            Assert.AreEqual(EmailTemplates.SendChangeEmailCompletedVerificationEmail, newEmailNotification.TemplateId);
        }
        
        [Test]
        [Description("POST: Cannot update email address for non-active user")]
        public void POST_Cannot_Update_Email_Address_For_Non_Active_User()
        {
            // Arrange
            User user = new UserBuilder().DefaultRetiredUser().WithEmailAddress("old@example.com").WithPassword("password").Build();
            
            var requestFormValues = new Dictionary<string, StringValues>();
            requestFormValues.Add("GovUk_Text_Password", "password");
            
            var controllerBuilder = new ControllerBuilder<ChangeEmailController>();
            var controller = controllerBuilder
                .WithLoggedInUser(user)
                .WithRequestFormValues(requestFormValues)
                .WithDatabaseObjects(user)
                .WithMockUriHelper()
                .Build();

            var emailVerificationCode = Encryption.EncryptModel(
                new ChangeEmailVerificationToken
                {
                    UserId = user.UserId,
                    NewEmailAddress = "new@example.com".ToLower(),
                    TokenTimestamp = VirtualDateTime.Now
                });

            var viewModel = new VerifyEmailChangeViewModel {NewEmailAddress = "new@example.com", Code = emailVerificationCode, User = user};

            // Act & Assert
            Assert.Throws<ArgumentException>(() => controller.VerifyEmailPost(viewModel));
        }
        
        [Test]
        [Description("POST: Cannot update email address to email associated with another account")]
        public void POST_Cannot_Update_Email_Address_To_Email_Associated_With_Another_Account()
        {
            // Arrange
            User user = new UserBuilder().WithEmailAddress("old@example.com").WithPassword("password").Build();
            User user2 = new UserBuilder().WithEmailAddress("new@example.com").Build();
            
            var requestFormValues = new Dictionary<string, StringValues>();
            requestFormValues.Add("GovUk_Text_Password", "password");
            
            var controllerBuilder = new ControllerBuilder<ChangeEmailController>();
            var controller = controllerBuilder
                .WithLoggedInUser(user)
                .WithRequestFormValues(requestFormValues)
                .WithDatabaseObjects(user, user2)
                .WithMockUriHelper()
                .Build();

            var emailVerificationCode = Encryption.EncryptModel(
                new ChangeEmailVerificationToken
                {
                    UserId = user.UserId,
                    NewEmailAddress = "new@example.com".ToLower(),
                    TokenTimestamp = VirtualDateTime.Now
                });

            var viewModel = new VerifyEmailChangeViewModel {NewEmailAddress = "new@example.com", Code = emailVerificationCode, User = user};
            
            // Act
            controller.VerifyEmailPost(viewModel);

            // Assert
            Assert.AreEqual("old@example.com", user.EmailAddress);

            var auditLogs = controllerBuilder.DataRepository.GetAll<AuditLog>();
            Assert.AreEqual(0, auditLogs.Count());
        }

    }
}

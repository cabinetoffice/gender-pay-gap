using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Controllers;
using GenderPayGap.WebUI.Controllers.Account;
using GenderPayGap.WebUI.ErrorHandling;
using GenderPayGap.WebUI.Models.Account;
using GenderPayGap.WebUI.Repositories;
using GenderPayGap.WebUI.Services;
using GenderPayGap.WebUI.Tests.Builders;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Controllers.PasswordReset
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class PasswordResetControllerTests
    {

        [Test]
        [Description("POST: Password reset email is sent when valid email address is provided")]
        public void POST_Password_Reset_Email_Is_Sent_When_Valid_Email_Address_Is_Provided()
        {
            // Arrange
            User user = new UserBuilder().WithEmailAddress("user@test.com").Build();
            
            var requestFormValues = new Dictionary<string, StringValues>();
            requestFormValues.Add("GovUk_Text_EmailAddress", "user@test.com");

            var controllerBuilder = new ControllerBuilder<PasswordResetController>();
            var controller = controllerBuilder
                .WithLoggedInUser(user)
                .WithRequestFormValues(requestFormValues)
                .WithDatabaseObjects(user)
                .WithMockUriHelper()
                .Build();
            
            // Act
            controller.PasswordResetPost(new PasswordResetViewModel());
            
            // Assert
            Assert.AreEqual(1, controllerBuilder.EmailsSent.Count);

            var email = controllerBuilder.EmailsSent.FirstOrDefault();
            Assert.NotNull(email);
            Assert.AreEqual(EmailTemplates.SendResetPasswordVerificationEmail, email.TemplateId);
            Assert.AreEqual(user.EmailAddress, email.EmailAddress);
        }
        
        [Test]
        [Description("POST: Password reset email is not sent when one has been sent within last 10 minutes")]
        public void POST_Password_Reset_Email_Is_Not_Sent_When_One_Has_Been_Sent_Within_Last_10_Minutes()
        {
            // Arrange
            User user = new UserBuilder().WithEmailAddress("user@test.com").WithPasswordResetCode("code", VirtualDateTime.Now.AddMinutes(-2)).Build();
            
            var requestFormValues = new Dictionary<string, StringValues>();
            requestFormValues.Add("GovUk_Text_EmailAddress", "user@test.com");

            var controllerBuilder = new ControllerBuilder<PasswordResetController>();
            var controller = controllerBuilder
                .WithLoggedInUser(user)
                .WithRequestFormValues(requestFormValues)
                .WithDatabaseObjects(user)
                .WithMockUriHelper()
                .Build();
            
            // Act
            Assert.Throws<UserRecentlySentPasswordResetEmailWithoutChangingPasswordException>(() => controller.PasswordResetPost(new PasswordResetViewModel()));
        }
        
        [Test]
        [Description("POST: Password reset email is not sent when there is no user with email address provided")]
        public void POST_Password_Reset_Email_Is_Not_Sent_When_There_Is_No_User_With_Email_Address_Provided()
        {
            // Arrange
            User user = new UserBuilder().WithEmailAddress("user@test.com").Build();
            
            var requestFormValues = new Dictionary<string, StringValues>();
            requestFormValues.Add("GovUk_Text_EmailAddress", "anotheruser@test.com");

            var controllerBuilder = new ControllerBuilder<PasswordResetController>();
            var controller = controllerBuilder
                .WithLoggedInUser(user)
                .WithRequestFormValues(requestFormValues)
                .WithDatabaseObjects(user)
                .WithMockUriHelper()
                .Build();
            
            // Act
            controller.PasswordResetPost(new PasswordResetViewModel());
            
            // Assert
            Assert.IsEmpty(controllerBuilder.EmailsSent);
        }
        
        [Test]
        [Description("POST: Valid reset code in URL allows user to change their password")]
        public void POST_Valid_Reset_Code_In_URL_Allows_User_To_Change_Their_Password()
        {
            // Arrange
            User user = new UserBuilder().WithEmailAddress("user@test.com").WithPasswordResetCode("code").Build();
            
            var requestFormValues = new Dictionary<string, StringValues>();
            requestFormValues.Add("GovUk_Text_NewPassword", "SomethingNew1");
            requestFormValues.Add("GovUk_Text_ConfirmNewPassword", "SomethingNew1");

            var controllerBuilder = new ControllerBuilder<PasswordResetController>();
            var controller = controllerBuilder
                .WithLoggedInUser(user)
                .WithDatabaseObjects(user)
                .WithRequestFormValues(requestFormValues)
                .WithMockUriHelper()
                .Build();
            
            // Act
            controller.ChooseNewPasswordPost(new ChooseNewPasswordViewModel { ResetCode = "code" });
            
            // Assert
            Assert.AreEqual(Crypto.GetPBKDF2("SomethingNew1", Convert.FromBase64String(user.Salt)), user.PasswordHash);
            Assert.IsNull(user.PasswordResetCode);
            
            Assert.AreEqual(1, controllerBuilder.EmailsSent.Count);

            var email = controllerBuilder.EmailsSent.FirstOrDefault();
            Assert.NotNull(email);
            Assert.AreEqual(EmailTemplates.SendResetPasswordCompletedEmail, email.TemplateId);

            var auditLogs = controllerBuilder.DataRepository.GetAll<AuditLog>();
            Assert.AreEqual(1, auditLogs.Count());

            var log = auditLogs.FirstOrDefault();
            Assert.NotNull(log);
            Assert.AreEqual(AuditedAction.UserChangePassword, log.Action);
        }
        
        [Test]
        [Description("POST: Not providing reset code in URL gives PageNotFoundException")]
        public void POST_Not_Providing_Reset_Code_In_URL_Gives_PageNotFoundException()
        {
            // Arrange
            User user = new UserBuilder().WithEmailAddress("user@test.com").WithPasswordResetCode("code").Build();
            
            var requestFormValues = new Dictionary<string, StringValues>();
            requestFormValues.Add("GovUk_Text_NewPassword", "SomethingNew1");
            requestFormValues.Add("GovUk_Text_ConfirmNewPassword", "SomethingNew1");

            var controllerBuilder = new ControllerBuilder<PasswordResetController>();
            var controller = controllerBuilder
                .WithLoggedInUser(user)
                .WithDatabaseObjects(user)
                .WithRequestFormValues(requestFormValues)
                .WithMockUriHelper()
                .Build();

            // Assert
            Assert.Throws<PageNotFoundException>(() => controller.ChooseNewPasswordPost(new ChooseNewPasswordViewModel()));
        }
        
        [Test]
        [Description("POST: Using expired reset code gives PasswordResetCodeExpiredException")]
        public void POST_Using_Expired_Reset_Code_Gives_PasswordResetCodeExpiredException()
        {
            // Arrange
            User user = new UserBuilder().WithEmailAddress("user@test.com").WithPasswordResetCode("code", DateTime.Now.AddDays(-10)).Build();
            
            var requestFormValues = new Dictionary<string, StringValues>();
            requestFormValues.Add("GovUk_Text_NewPassword", "SomethingNew1");
            requestFormValues.Add("GovUk_Text_ConfirmNewPassword", "SomethingNew1");

            var controllerBuilder = new ControllerBuilder<PasswordResetController>();
            var controller = controllerBuilder
                .WithLoggedInUser(user)
                .WithDatabaseObjects(user)
                .WithRequestFormValues(requestFormValues)
                .WithMockUriHelper()
                .Build();

            // Assert
            Assert.Throws<PasswordResetCodeExpiredException>(() => controller.ChooseNewPasswordPost(new ChooseNewPasswordViewModel { ResetCode = "code" }));
        }

    }
}

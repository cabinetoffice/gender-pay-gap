using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Database;
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
            User user = new UserBuilder().WithEmailAddress("old@test.com").Build();
            
            var requestFormValues = new Dictionary<string, StringValues>();
            requestFormValues.Add("GovUk_Text_NewEmailAddress", "new@test.com");

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
            Assert.AreEqual("new@test.com", email.EmailAddress);
        }
        
        [Test]
        [Description("POST: Trying to change email address to current email address does not send verification email")]
        public void POST_Trying_To_Change_Email_Address_To_Current_Email_Address_Does_Not_Send_Verification_Email()
        {
            // Arrange
            User user = new UserBuilder().WithEmailAddress("old@test.com").Build();
            
            var requestFormValues = new Dictionary<string, StringValues>();
            requestFormValues.Add("GovUk_Text_NewEmailAddress", "old@test.com");

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
            User user = new UserBuilder().WithEmailAddress("user@test.com").Build();
            User user2 = new UserBuilder().WithEmailAddress("user2@test.com").Build();
            
            var requestFormValues = new Dictionary<string, StringValues>();
            requestFormValues.Add("GovUk_Text_NewEmailAddress", "user2@test.com");

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

    }
}

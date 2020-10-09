using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.WebUI.BusinessLogic.Abstractions;
using GenderPayGap.WebUI.ExternalServices;
using GenderPayGap.WebUI.Models.Account;
using GenderPayGap.WebUI.Services;
using GenderPayGap.WebUI.Tests.Builders;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Controllers.Account.ChangePasswordController
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class ChangePasswordControllerTests
    {
        
        private Mock<IDataRepository> mockDataRepo;
        private IUserRepository mockUserRepo;

        [SetUp]
        public void BeforeEach()
        {
            // mock data 
            mockDataRepo = new Mock<IDataRepository>();

            var auditLoggerWithMocks = new AuditLogger(Mock.Of<IDataRepository>());

            // service under test
            mockUserRepo =
                new Repositories.UserRepository(
                    mockDataRepo.Object,
                    auditLoggerWithMocks);
        }

        [Test]
        [Description("POST: Password is updated as expected")]
        public async Task POST_Password_Is_Updated_As_Expected()
        {
            // Arrange
            User user = new UserBuilder().WithPassword("password").Build();
            
            var requestFormValues = new Dictionary<string, StringValues>();
            requestFormValues.Add("GovUk_Text_CurrentPassword", "password");
            requestFormValues.Add("GovUk_Text_NewPassword", "NewPassword1");
            requestFormValues.Add("GovUk_Text_ConfirmNewPassword", "NewPassword1");

            var controller = new ControllerBuilder<WebUI.Controllers.Account.ChangePasswordController>()
                .WithLoggedInUser(user)
                .WithRequestFormValues(requestFormValues)
                .WithDatabaseObjects(user)
                .WithMockUriHelper()
                .Build();

            // Act
            controller.ChangePasswordPost(new ChangePasswordViewModel()).Wait();
            
            // Assert
            bool isExpectedPassword = mockUserRepo.CheckPassword(user, "NewPassword1");
            Assert.IsTrue(isExpectedPassword);
        }
        
        [Test]
        [Description("POST: Email is sent when password is successfully updated")]
        public async Task POST_Email_Is_Sent_When_Password_Is_Successfully_Updated()
        {
            // Arrange
            User user = new UserBuilder().WithPassword("password").Build();
            
            var requestFormValues = new Dictionary<string, StringValues>();
            requestFormValues.Add("GovUk_Text_CurrentPassword", "password");
            requestFormValues.Add("GovUk_Text_NewPassword", "NewPassword1");
            requestFormValues.Add("GovUk_Text_ConfirmNewPassword", "NewPassword1");
            
            var controllerBuilder = new ControllerBuilder<WebUI.Controllers.Account.ChangePasswordController>();
            var controller = controllerBuilder
                .WithLoggedInUser(user)
                .WithRequestFormValues(requestFormValues)
                .WithDatabaseObjects(user)
                .WithMockUriHelper()
                .Build();

            // Act
            controller.ChangePasswordPost(new ChangePasswordViewModel()).Wait();
            
            // Assert
            // Assert that exactly one email is sent
            Assert.AreEqual(1,controllerBuilder.EmailsSent.Count);
            
            NotifyEmail userEmail = controllerBuilder.EmailsSent.FirstOrDefault();
            // Assert that the email sent has the correct email address and template
            Assert.NotNull(userEmail);
            Assert.AreEqual(user.EmailAddress, userEmail.EmailAddress);
            Assert.AreEqual(EmailTemplates.SendChangePasswordCompletedEmail, userEmail.TemplateId, $"Expected the correct templateId to be in the email send queue, expected {EmailTemplates.SendChangePasswordCompletedEmail}");
        }
        
        [Test]
        [Description("POST: Audit log item is saved when password is successfully updated")]
        public async Task POST_Audit_Log_Item_Is_Saved_When_Password_Is_Successfully_Updated()
        {
            // Arrange
            User user = new UserBuilder().WithPassword("password").Build();
            
            var requestFormValues = new Dictionary<string, StringValues>();
            requestFormValues.Add("GovUk_Text_CurrentPassword", "password");
            requestFormValues.Add("GovUk_Text_NewPassword", "NewPassword1");
            requestFormValues.Add("GovUk_Text_ConfirmNewPassword", "NewPassword1");
            
            var controllerBuilder = new ControllerBuilder<WebUI.Controllers.Account.ChangePasswordController>();
            var controller = controllerBuilder
                .WithLoggedInUser(user)
                .WithRequestFormValues(requestFormValues)
                .WithDatabaseObjects(user)
                .WithMockUriHelper()
                .Build();

            // Act
            controller.ChangePasswordPost(new ChangePasswordViewModel()).Wait();
            
            // Assert
            // Assert that exactly one audit log entry is added
            List<AuditLog> auditLogEntries = controllerBuilder.DataRepository.GetAll<AuditLog>().ToList();
            Assert.AreEqual(1, auditLogEntries.Count);
            
            // Assert that the audit log entry audits the correct action
            Assert.AreEqual(AuditedAction.UserChangePassword, auditLogEntries.First().Action);
        }
        
        [Test]
        [Description("POST: Password is not updated when new password has fewer than 8 characters")]
        public async Task POST_Password_Is_Not_Updated_When_New_Password_Has_Fewer_Than_8_Characters()
        {
            // Arrange
            User user = new UserBuilder().WithPassword("password").Build();
            
            var requestFormValues = new Dictionary<string, StringValues>();
            requestFormValues.Add("GovUk_Text_CurrentPassword", "password");
            requestFormValues.Add("GovUk_Text_NewPassword", "Abc1");
            requestFormValues.Add("GovUk_Text_ConfirmNewPassword", "Abc1");

            var controller = new ControllerBuilder<WebUI.Controllers.Account.ChangePasswordController>()
                .WithLoggedInUser(user)
                .WithRequestFormValues(requestFormValues)
                .WithDatabaseObjects(user)
                .WithMockUriHelper()
                .Build();

            // Act
            controller.ChangePasswordPost(new ChangePasswordViewModel()).Wait();
            
            // Assert
            bool isExpectedPassword = mockUserRepo.CheckPassword(user, "password");
            Assert.IsTrue(isExpectedPassword);
        }
        
        [Test]
        [Description("POST: Password is not updated when new password is missing an uppercase letter")]
        public async Task POST_Password_Is_Not_Updated_When_New_Password_Missing_Uppercase_Letter()
        {
            // Arrange
            User user = new UserBuilder().WithPassword("password").Build();
            
            var requestFormValues = new Dictionary<string, StringValues>();
            requestFormValues.Add("GovUk_Text_CurrentPassword", "password");
            requestFormValues.Add("GovUk_Text_NewPassword", "abcdefg1");
            requestFormValues.Add("GovUk_Text_ConfirmNewPassword", "abcdefg1");

            var controller = new ControllerBuilder<WebUI.Controllers.Account.ChangePasswordController>()
                .WithLoggedInUser(user)
                .WithRequestFormValues(requestFormValues)
                .WithDatabaseObjects(user)
                .WithMockUriHelper()
                .Build();

            // Act
            controller.ChangePasswordPost(new ChangePasswordViewModel()).Wait();
            
            // Assert
            bool isExpectedPassword = mockUserRepo.CheckPassword(user, "password");
            Assert.IsTrue(isExpectedPassword);
        }
        
        [Test]
        [Description("POST: Password is not updated when new password is missing a lowercase letter")]
        public async Task POST_Password_Is_Not_Updated_When_New_Password_Missing_Lowercase_Letter()
        {
            // Arrange
            User user = new UserBuilder().WithPassword("password").Build();
            
            var requestFormValues = new Dictionary<string, StringValues>();
            requestFormValues.Add("GovUk_Text_CurrentPassword", "password");
            requestFormValues.Add("GovUk_Text_NewPassword", "ABCDEFG1");
            requestFormValues.Add("GovUk_Text_ConfirmNewPassword", "ABCDEFG1");

            var controller = new ControllerBuilder<WebUI.Controllers.Account.ChangePasswordController>()
                .WithLoggedInUser(user)
                .WithRequestFormValues(requestFormValues)
                .WithDatabaseObjects(user)
                .WithMockUriHelper()
                .Build();

            // Act
            controller.ChangePasswordPost(new ChangePasswordViewModel()).Wait();
            
            // Assert
            bool isExpectedPassword = mockUserRepo.CheckPassword(user, "password");
            Assert.IsTrue(isExpectedPassword);
        }
        
        [Test]
        [Description("POST: Password is not updated when new password is different to the confirmation password")]
        public async Task POST_Password_Is_Not_Updated_When_New_Password_Different_To_Confirmation_Password()
        {
            // Arrange
            User user = new UserBuilder().WithPassword("password").Build();
            
            var requestFormValues = new Dictionary<string, StringValues>();
            requestFormValues.Add("GovUk_Text_CurrentPassword", "password");
            requestFormValues.Add("GovUk_Text_NewPassword", "Password1");
            requestFormValues.Add("GovUk_Text_ConfirmNewPassword", "AnotherPassword1");

            var controller = new ControllerBuilder<WebUI.Controllers.Account.ChangePasswordController>()
                .WithLoggedInUser(user)
                .WithRequestFormValues(requestFormValues)
                .WithDatabaseObjects(user)
                .WithMockUriHelper()
                .Build();

            // Act
            controller.ChangePasswordPost(new ChangePasswordViewModel()).Wait();
            
            // Assert
            bool isExpectedPassword = mockUserRepo.CheckPassword(user, "password");
            Assert.IsTrue(isExpectedPassword);
        }

    }
}

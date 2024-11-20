using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.BusinessLogic.Abstractions;
using GenderPayGap.WebUI.Controllers.Account;
using GenderPayGap.WebUI.ExternalServices;
using GenderPayGap.WebUI.Models.Account;
using GenderPayGap.WebUI.Services;
using GenderPayGap.WebUI.Tests.Builders;
using GenderPayGap.WebUI.Tests.TestHelpers;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Controllers.Account
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
            UiTestHelper.SetDefaultEncryptionKeys();
            
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
        public void POST_Password_Is_Updated_As_Expected()
        {
            // Arrange
            User user = new UserBuilder().WithPassword("password").Build();

            var controller = new ControllerBuilder<ChangePasswordController>()
                .WithLoggedInUser(user)
                .WithDatabaseObjects(user)
                .WithMockUriHelper()
                .Build();

            // Act
            controller.ChangePasswordPost(new ChangePasswordViewModel
            {
                CurrentPassword = "password",
                NewPassword = "NewPassword1",
                ConfirmNewPassword = "NewPassword1"
            });
            
            // Assert
            bool isExpectedPassword = mockUserRepo.CheckPassword(user, "NewPassword1");
            Assert.IsTrue(isExpectedPassword);
        }
        
        [Test]
        [Description("POST: Email is sent when password is successfully updated")]
        public void POST_Email_Is_Sent_When_Password_Is_Successfully_Updated()
        {
            // Arrange
            User user = new UserBuilder().WithPassword("password").Build();

            var controllerBuilder = new ControllerBuilder<ChangePasswordController>();
            var controller = controllerBuilder
                .WithLoggedInUser(user)
                .WithDatabaseObjects(user)
                .WithMockUriHelper()
                .Build();

            // Act
            controller.ChangePasswordPost(new ChangePasswordViewModel
            {
                CurrentPassword = "password",
                NewPassword = "NewPassword1",
                ConfirmNewPassword = "NewPassword1"
            });
            
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
        public void POST_Audit_Log_Item_Is_Saved_When_Password_Is_Successfully_Updated()
        {
            // Arrange
            User user = new UserBuilder().WithPassword("password").Build();

            var controllerBuilder = new ControllerBuilder<ChangePasswordController>();
            var controller = controllerBuilder
                .WithLoggedInUser(user)
                .WithDatabaseObjects(user)
                .WithMockUriHelper()
                .Build();

            // Act
            controller.ChangePasswordPost(new ChangePasswordViewModel
            {
                CurrentPassword = "password",
                NewPassword = "NewPassword1",
                ConfirmNewPassword = "NewPassword1"
            });
            
            // Assert
            // Assert that exactly one audit log entry is added
            List<AuditLog> auditLogEntries = controllerBuilder.DataRepository.GetAll<AuditLog>().ToList();
            Assert.AreEqual(1, auditLogEntries.Count);
            
            // Assert that the audit log entry audits the correct action
            Assert.AreEqual(AuditedAction.UserChangePassword, auditLogEntries.First().Action);
        }
        
        [Test]
        [Description("POST: Password is not updated when new password has fewer than 8 characters")]
        public void POST_Password_Is_Not_Updated_When_New_Password_Has_Fewer_Than_8_Characters()
        {
            // Arrange
            User user = new UserBuilder().WithPassword("password").Build();

            var viewModel = new ChangePasswordViewModel { CurrentPassword = "password", NewPassword = "Abc1", ConfirmNewPassword = "Abc1" };
            
            var controller = new ControllerBuilder<ChangePasswordController>()
                .WithLoggedInUser(user)
                .WithDatabaseObjects(user)
                .WithMockUriHelper()
                .Build();
            
            if (!viewModel.GetType()
                    .GetProperty(nameof(viewModel.NewPassword))
                    .GetCustomAttribute<GpgPasswordValidationAttribute>()
                    .IsValid(viewModel.NewPassword))
            {
                controller.ModelState.AddModelError(nameof(viewModel.NewPassword), "error");
            }

            // Act
            controller.ChangePasswordPost(new ChangePasswordViewModel
            {
                CurrentPassword = "password",
                NewPassword = "Abc1",
                ConfirmNewPassword = "Abc1"
            });
            
            // Assert
            bool isExpectedPassword = mockUserRepo.CheckPassword(user, "password");
            Assert.IsTrue(isExpectedPassword);
        }
        
        [Test]
        [Description("POST: Password is not updated when new password is missing an uppercase letter")]
        public void POST_Password_Is_Not_Updated_When_New_Password_Missing_Uppercase_Letter()
        {
            // Arrange
            User user = new UserBuilder().WithPassword("password").Build();
            
            var viewModel = new ChangePasswordViewModel
            {
                CurrentPassword = "password", NewPassword = "abcdefg1", ConfirmNewPassword = "abcdefg1"
            };

            var controller = new ControllerBuilder<ChangePasswordController>()
                .WithLoggedInUser(user)
                .WithDatabaseObjects(user)
                .WithMockUriHelper()
                .Build();
            
            if (!viewModel.GetType()
                    .GetProperty(nameof(viewModel.NewPassword))
                    .GetCustomAttribute<GpgPasswordValidationAttribute>()
                    .IsValid(viewModel.NewPassword))
            {
                controller.ModelState.AddModelError(nameof(viewModel.NewPassword), "error");
            }

            // Act
            controller.ChangePasswordPost(viewModel);
            
            // Assert
            bool isExpectedPassword = mockUserRepo.CheckPassword(user, "password");
            Assert.IsTrue(isExpectedPassword);
        }
        
        [Test]
        [Description("POST: Password is not updated when new password is missing a lowercase letter")]
        public void POST_Password_Is_Not_Updated_When_New_Password_Missing_Lowercase_Letter()
        {
            // Arrange
            User user = new UserBuilder().WithPassword("password").Build();

            var viewModel = new ChangePasswordViewModel
            {
                CurrentPassword = "password", NewPassword = "ABCDEFG1", ConfirmNewPassword = "ABCDEFG1"
            };

            var controller = new ControllerBuilder<ChangePasswordController>()
                .WithLoggedInUser(user)
                .WithDatabaseObjects(user)
                .WithMockUriHelper()
                .Build();

            if (!viewModel.GetType()
                    .GetProperty(nameof(viewModel.NewPassword))
                    .GetCustomAttribute<GpgPasswordValidationAttribute>()
                    .IsValid(viewModel.NewPassword))
            {
                controller.ModelState.AddModelError(nameof(viewModel.NewPassword), "error");
            }

            // Act
            controller.ChangePasswordPost(viewModel);
            
            // Assert
            bool isExpectedPassword = mockUserRepo.CheckPassword(user, "password");
            Assert.IsTrue(isExpectedPassword);
        }
        
        [Test]
        [Description("POST: Password is not updated when new password is different to the confirmation password")]
        public void POST_Password_Is_Not_Updated_When_New_Password_Different_To_Confirmation_Password()
        {
            // Arrange
            User user = new UserBuilder().WithPassword("password").Build();

            var controller = new ControllerBuilder<ChangePasswordController>()
                .WithLoggedInUser(user)
                .WithDatabaseObjects(user)
                .WithMockUriHelper()
                .Build();

            // Act
            controller.ChangePasswordPost(new ChangePasswordViewModel
            {
                CurrentPassword = "password",
                NewPassword = "Password1",
                ConfirmNewPassword = "AnotherPassword1"
            });
            
            // Assert
            bool isExpectedPassword = mockUserRepo.CheckPassword(user, "password");
            Assert.IsTrue(isExpectedPassword);
        }

        [Test]
        [Description("POST: Password is not updated when old password is incorrect")]
        public void POST_Password_Is_Not_Updated_When_Old_Password_Is_Incorrect()
        {
            // Arrange
            User user = new UserBuilder().WithPassword("password").Build();

            var controller = new ControllerBuilder<ChangePasswordController>()
                .WithLoggedInUser(user)
                .WithDatabaseObjects(user)
                .WithMockUriHelper()
                .Build();

            // Act
            controller.ChangePasswordPost(new ChangePasswordViewModel
            {
                CurrentPassword = "incorrect_password",
                NewPassword = "Password1",
                ConfirmNewPassword = "Password1"
            });
            
            // Assert
            bool isExpectedPassword = mockUserRepo.CheckPassword(user, "password");
            Assert.IsTrue(isExpectedPassword);
        }
        
        [Test]
        [Description("POST: User is logged out if failing to change the password for 5 times")]
        public void POST_User_Is_Logged_Out_If_Failing_To_Change_The_Password_For_Five_Times()
        {
            // Arrange
            User user = new UserBuilder().WithPassword("password").Build();

            var viewModel = new ChangePasswordViewModel
            {
                CurrentPassword = "incorrect_password", NewPassword = "Password1", ConfirmNewPassword = "Password1"
            };

            var controller = new ControllerBuilder<ChangePasswordController>()
                .WithLoggedInUser(user)
                .WithDatabaseObjects(user)
                .WithMockUriHelper()
                .Build();

            // Act
            controller.ChangePasswordPost(viewModel);
            controller.ChangePasswordPost(viewModel);
            controller.ChangePasswordPost(viewModel);
            controller.ChangePasswordPost(viewModel);
            var result = controller.ChangePasswordPost(viewModel) as RedirectToActionResult;
            
            // Assert
            Assert.That(result != null, "Expected RedirectToActionResult");
            Assert.That(result.ActionName == "LoggedOut", "Expected redirect to LoggedOut");
        }
    }
}

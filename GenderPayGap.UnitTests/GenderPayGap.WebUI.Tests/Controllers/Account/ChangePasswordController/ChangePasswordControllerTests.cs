using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.BusinessLogic.Abstractions;
using GenderPayGap.WebUI.Controllers.Account;
using GenderPayGap.WebUI.Models.Account;
using GenderPayGap.WebUI.Services;
using GenderPayGap.WebUI.Tests.Builders;
using GenderPayGap.WebUI.Tests.TestHelpers;
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

            var controller = new ControllerBuilder<ChangePasswordNewController>()
                .WithUserId(user.UserId)
                .WithRequestFormValues(requestFormValues)
                .WithDatabaseObjects(user)
                .Build();
            
            // Required to mock out the Url object when creating the verification URL
            controller.AddMockUriHelperNew(new Uri("https://localhost:44371/mockURL").ToString());
            
            // Act
            controller.ChangePasswordPost(new ChangePasswordNewViewModel()).Wait();
            
            // Assert
            bool isExpectedPassword = await mockUserRepo.CheckPasswordAsync(user, "NewPassword1");
            Assert.IsTrue(isExpectedPassword);
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

            var controller = new ControllerBuilder<ChangePasswordNewController>()
                .WithUserId(user.UserId)
                .WithRequestFormValues(requestFormValues)
                .WithDatabaseObjects(user)
                .Build();
            
            // Required to mock out the Url object when creating the verification URL
            controller.AddMockUriHelperNew(new Uri("https://localhost:44371/mockURL").ToString());
            
            // Act
            controller.ChangePasswordPost(new ChangePasswordNewViewModel()).Wait();
            
            // Assert
            bool isExpectedPassword = await mockUserRepo.CheckPasswordAsync(user, "password");
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

            var controller = new ControllerBuilder<ChangePasswordNewController>()
                .WithUserId(user.UserId)
                .WithRequestFormValues(requestFormValues)
                .WithDatabaseObjects(user)
                .Build();
            
            // Required to mock out the Url object when creating the verification URL
            controller.AddMockUriHelperNew(new Uri("https://localhost:44371/mockURL").ToString());
            
            // Act
            controller.ChangePasswordPost(new ChangePasswordNewViewModel()).Wait();
            
            // Assert
            bool isExpectedPassword = await mockUserRepo.CheckPasswordAsync(user, "password");
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

            var controller = new ControllerBuilder<ChangePasswordNewController>()
                .WithUserId(user.UserId)
                .WithRequestFormValues(requestFormValues)
                .WithDatabaseObjects(user)
                .Build();
            
            // Required to mock out the Url object when creating the verification URL
            controller.AddMockUriHelperNew(new Uri("https://localhost:44371/mockURL").ToString());
            
            // Act
            controller.ChangePasswordPost(new ChangePasswordNewViewModel()).Wait();
            
            // Assert
            bool isExpectedPassword = await mockUserRepo.CheckPasswordAsync(user, "password");
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

            var controller = new ControllerBuilder<ChangePasswordNewController>()
                .WithUserId(user.UserId)
                .WithRequestFormValues(requestFormValues)
                .WithDatabaseObjects(user)
                .Build();
            
            // Required to mock out the Url object when creating the verification URL
            controller.AddMockUriHelperNew(new Uri("https://localhost:44371/mockURL").ToString());
            
            // Act
            controller.ChangePasswordPost(new ChangePasswordNewViewModel()).Wait();
            
            // Assert
            bool isExpectedPassword = await mockUserRepo.CheckPasswordAsync(user, "password");
            Assert.IsTrue(isExpectedPassword);
        }

    }
}

using System.Threading.Tasks;
using GenderPayGap.BusinessLogic.Account.Abstractions;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Tests.TestHelpers;
using GenderPayGap.WebUI.Areas.Account.Abstractions;
using GenderPayGap.WebUI.Areas.Account.ViewServices;
using GenderPayGap.WebUI.BackgroundJobs;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using NUnit.Framework;

namespace Account.ViewServices
{

    [TestFixture]
    [SetCulture("en-GB")]
    public class ChangeEmailViewServiceTests
    {

        private Mock<IUrlHelper> mockUrlHelper;
        private Mock<IUserRepository> mockUserRepo;

        private IChangeEmailViewService testChangeEmailService;

        [SetUp]
        public void BeforeEach()
        {
            mockUserRepo = new Mock<IUserRepository>();
            mockUrlHelper = new Mock<IUrlHelper>();

            var emailSendingService = new EmailSendingService(
                new Mock<IGovNotifyAPI>().Object,
                new Mock<IBackgroundJobsApi>().Object);

            // service under test
            testChangeEmailService = new ChangeEmailViewService(mockUserRepo.Object, mockUrlHelper.Object, emailSendingService);
        }

        [Test]
        public async Task NewEmailMustNotMatchExistingEmail()
        {
            // Arrange
            User testUser = UserHelper.GetNotAdminUserWithoutVerifiedEmailAddress();
            string testNewEmail = testUser.EmailAddress;

            // Act
            ModelStateDictionary actualState = await testChangeEmailService.InitiateChangeEmailAsync(testNewEmail, testUser);

            // Assert
            Assert.AreEqual(1, actualState.ErrorCount, "Expected error count to match");
            Assert.AreEqual(
                "The email address you entered must be different from your current email address",
                actualState["EmailAddress"].Errors[0].ErrorMessage,
                "Expected error message to match");
        }

        [Test]
        public async Task CannotChangeToAnotherActiveUserEmail()
        {
            // Arrange
            User testUser = UserHelper.GetNotAdminUserWithoutVerifiedEmailAddress();
            User existingUser = UserHelper.GetNotAdminUserWithoutVerifiedEmailAddress();

            string testNewEmail = existingUser.EmailAddress;

            mockUserRepo.Setup(x => x.FindByEmailAsync(It.IsAny<string>(), It.IsAny<UserStatuses[]>()))
                .Returns(Task.FromResult(existingUser));

            // Act
            ModelStateDictionary actualState = await testChangeEmailService.InitiateChangeEmailAsync(testNewEmail, testUser);

            // Assert
            Assert.AreEqual(1, actualState.ErrorCount, "Expected error count to match");
            Assert.AreEqual(
                "The email provided is already used by an active account",
                actualState["EmailAddress"].Errors[0].ErrorMessage,
                "Expected error message to match");
        }

    }

}

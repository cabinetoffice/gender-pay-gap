using System;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Tests.Common.Classes;
using GenderPayGap.Tests.Common.TestHelpers;
using GenderPayGap.WebUI.BusinessLogic.Abstractions;
using GenderPayGap.WebUI.Services;
using Moq;
using NUnit.Framework;

namespace Repositories.UserRepository
{

    [TestFixture]
    [SetCulture("en-GB")]
    public class UpdateEmailAsyncTests
    {

        [SetUp]
        public void BeforeEach()
        {
            // mock data 
            mockDataRepo = new Mock<IDataRepository>().SetupGetAll(UserHelpers.CreateUsers());

            var auditLoggerWithMocks = new AuditLogger(Mock.Of<IDataRepository>());

            // service under test
            testUserRepo =
                new GenderPayGap.WebUI.Repositories.UserRepository(
                    mockDataRepo.Object,
                    auditLoggerWithMocks);
        }

        private Mock<IDataRepository> mockDataRepo;

        private IUserRepository testUserRepo;

        [TestCase("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com", "active2@ad5bda75-e514-491b-b74d-4672542cbd15.com")]
        public async Task SavesExpectedEmailFields(string testCurrentEmail, string testNewEmail)
        {
            // Arrange
            var saveChangesCalled = false;
            User testUserToUpdate = await testUserRepo.FindByEmailAsync(testCurrentEmail);

            // pretend user email was last verified 7 days ago
            testUserToUpdate.EmailVerifiedDate = VirtualDateTime.Now.Date.AddDays(-7);

            mockDataRepo.Setup(x => x.SaveChanges())
                .Callback(() => saveChangesCalled = true);

            // Act
            testUserRepo.UpdateEmail(testUserToUpdate, testNewEmail);

            // Assert
            Assert.IsTrue(saveChangesCalled, "Expected SaveChangesAsync to be called");
            Assert.AreEqual(testNewEmail, testUserToUpdate.EmailAddress, "Expected to change email");
            Assert.Zero(
                VirtualDateTime.Now.Subtract(testUserToUpdate.EmailVerifiedDate.Value).Minutes,
                "Expected to change email verify date");
        }

        [TestCase("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.New)]
        [TestCase("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.Unknown)]
        [TestCase("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.Retired)]
        public async Task ThrowsErrorWhenUserStatusIsNotActive(string testCurrentEmail, UserStatuses testStatus)
        {
            // Arrange
            DateTime testEmailVerifiedDate = VirtualDateTime.Now.Date.AddDays(-7);

            User testUserToUpdate = await testUserRepo.FindByEmailAsync(testCurrentEmail);
            testUserToUpdate.Status = testStatus;
            testUserToUpdate.EmailVerifiedDate = testEmailVerifiedDate;

            // Act
            var actualException = Assert.Throws<ArgumentException>(
                () => testUserRepo.UpdateEmail(testUserToUpdate, "change@email.com"));

            // Assert
            Assert.AreEqual(
                "Can only update emails for active users. UserId=23322",
                actualException.Message,
                "Expected exception message to match");
            Assert.AreEqual(testCurrentEmail, testUserToUpdate.EmailAddress, "Expected email address to still be the same");
            Assert.AreEqual(
                testEmailVerifiedDate,
                testUserToUpdate.EmailVerifiedDate.Value,
                "Expected email verify date to still be the same");
            Assert.AreEqual(testStatus, testUserToUpdate.Status, "Expected status to still be the same");
        }

        private static object[] ShouldThrowErrorWhenArgumentIsNullCases = {
            new object[] {
                null, "active1@ad5bda75-e514-491b-b74d-4672542cbd15.com", "Value cannot be null. (Parameter 'userToUpdate')"
            },
            new object[] {new User(), null, "Value cannot be null. (Parameter 'newEmailAddress')"},
            new object[] {new User(), "", "Value cannot be null. (Parameter 'newEmailAddress')"},
            new object[] {new User(), " ", "Value cannot be null. (Parameter 'newEmailAddress')" }
        };

        [TestCaseSource(nameof(ShouldThrowErrorWhenArgumentIsNullCases))]
        public void ThrowsErrorWhenArgumentIsNull(User testUserToUpdateArg, string testNewEmailAddressArg, string expectedErrorMessage)
        {
            // Act
            var actualException = Assert.Throws<ArgumentNullException>(
                () => testUserRepo.UpdateEmail(testUserToUpdateArg, testNewEmailAddressArg));

            // Assert
            Assert.AreEqual(expectedErrorMessage, actualException.Message, "Expected exception message to match");
        }

    }

}

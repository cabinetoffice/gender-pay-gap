using System;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic.Account.Abstractions;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.Tests.Common.Classes;
using GenderPayGap.Tests.Common.TestHelpers;
using Moq;
using NUnit.Framework;

namespace Repositories.UserRepository
{

    [TestFixture]
    [SetCulture("en-GB")]
    public class UpdatePasswordAsyncTests
    {

        [SetUp]
        public void BeforeEach()
        {
            // mock data 
            mockDataRepo = new Mock<IDataRepository>().SetupGetAll(UserHelpers.CreateUsers());

            var auditLoggerWithMocks = new AuditLogger(Mock.Of<IDataRepository>(), Mock.Of<IHttpSession>());

            // service under test
            testUserRepo =
                new GenderPayGap.BusinessLogic.Account.Repositories.UserRepository(
                    mockDataRepo.Object,
                    auditLoggerWithMocks);
        }

        private Mock<IDataRepository> mockDataRepo;

        private IUserRepository testUserRepo;

        [TestCase]
        public async Task SavesNewPasswordHash()
        {
            // Arrange
            var saveChangesCalled = false;
            User testUserToUpdate = await testUserRepo.FindByEmailAsync("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com");
            var testPassword = "__Password123__";

            mockDataRepo.Setup(x => x.SaveChangesAsync())
                .Callback(() => saveChangesCalled = true)
                .Returns(Task.CompletedTask);

            // Act
            testUserRepo.UpdatePassword(testUserToUpdate, testPassword);

            // Assert
            Assert.IsTrue(saveChangesCalled, "Expected SaveChangesAsync to be called");
            Assert.AreEqual(Crypto.GetPBKDF2(testPassword, Convert.FromBase64String(testUserToUpdate.Salt)), testUserToUpdate.PasswordHash, "Expected to change password");
            Assert.AreEqual(HashingAlgorithm.PBKDF2, testUserToUpdate.HashingAlgorithm, "Expected hashing algorithm to change");
        }


        [TestCase("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.New)]
        [TestCase("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.Unknown)]
        [TestCase("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.Retired)]
        public async Task ThrowsErrorWhenUserStatusIsNotActive(string testCurrentEmail, UserStatuses testStatus)
        {
            // Arrange
            User testUserToUpdate = await testUserRepo.FindByEmailAsync(testCurrentEmail);
            DateTime testEmailVerifiedDate = VirtualDateTime.Now.Date.AddDays(-7);
            string testExistingPasswordHash = Crypto.GetPBKDF2("ExistingPassword123", Convert.FromBase64String(testUserToUpdate.Salt));
            testUserToUpdate.PasswordHash = testExistingPasswordHash;
            testUserToUpdate.Status = testStatus;

            // Act
            var actualException = Assert.Throws<ArgumentException>(
                () => testUserRepo.UpdatePassword(testUserToUpdate, "NewPassword123"));

            // Assert
            Assert.AreEqual(
                "Can only update passwords for active users. UserId=23322",
                actualException.Message,
                "Expected exception message to match");
            Assert.AreEqual(testExistingPasswordHash, testUserToUpdate.PasswordHash, "Expected password to still be the same");
            Assert.AreEqual(testStatus, testUserToUpdate.Status, "Expected status to still be the same");
        }

        private static object[] ThrowsErrorWhenArgumentIsNullCases = {
            new object[] {null, "newpassword123", "Value cannot be null. (Parameter 'userToUpdate')"},
            new object[] {new User(), null, "Value cannot be null. (Parameter 'newPassword')"},
            new object[] {new User(), "", "Value cannot be null. (Parameter 'newPassword')"},
            new object[] {new User(), " ", "Value cannot be null. (Parameter 'newPassword')" }
        };

        [TestCaseSource(nameof(ThrowsErrorWhenArgumentIsNullCases))]
        public void ThrowsErrorWhenArgumentIsNull(User testUserArg, string testPasswordArg, string expectedErrorMessage)
        {
            // Act
            var actualException = Assert.Throws<ArgumentNullException>(
                () => testUserRepo.UpdatePassword(testUserArg, testPasswordArg));

            // Assert
            Assert.AreEqual(expectedErrorMessage, actualException.Message, "Expected exception message to match");
        }

    }

}

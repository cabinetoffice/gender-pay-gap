using System;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic.Account.Abstractions;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Services;
using Moq;
using NUnit.Framework;

namespace Repositories.UserRepository
{

    [TestFixture]
    [SetCulture("en-GB")]
    public class CheckPasswordAsyncTests
    {

        [SetUp]
        public void BeforeEach()
        {
            // mock data 
            mockDataRepo = new Mock<IDataRepository>();

            var auditLoggerWithMocks = new AuditLogger(Mock.Of<IDataRepository>());

            // service under test
            testUserRepo =
                new GenderPayGap.WebUI.Repositories.UserRepository(
                    mockDataRepo.Object,
                    auditLoggerWithMocks);
        }

        private Mock<IDataRepository> mockDataRepo;
        private IUserRepository testUserRepo;

        [Test]
        public async Task CorrectPasswordShouldResetLoginAttempts()
        {
            // Arrange
            var saveChangesCalled = false;
            var testPassword = "currentPassword123";
            var salt = "testSalt";
            var testUser = new User {
                PasswordHash = Crypto.GetPBKDF2(testPassword, Convert.FromBase64String(salt)),
                Salt = salt,
                HashingAlgorithm = HashingAlgorithm.PBKDF2, LoginAttempts = 3
            };

            mockDataRepo.Setup(x => x.SaveChangesAsync())
                .Callback(() => saveChangesCalled = true)
                .Returns(Task.CompletedTask);

            // Act
            bool actualResult = await testUserRepo.CheckPasswordAsync(testUser, testPassword);

            // Assert
            Assert.IsTrue(actualResult, "Expected correct password to return true");
            Assert.IsTrue(saveChangesCalled, "Expected save changes to be called");
            Assert.Zero(testUser.LoginAttempts, "Expected user login attempts to be 0");
        }

        [Test]
        public async Task IncorrectPasswordShouldIncreaseLoginAttempts()
        {
            // Arrange
            var testPassword = "currentPassword123";
            var salt = "testSalt";
            var testUser = new User {
                PasswordHash = Crypto.GetPBKDF2("WrongPassword123", Convert.FromBase64String(salt)), Salt = salt, HashingAlgorithm = HashingAlgorithm.PBKDF2, LoginAttempts = 0
            };
            var testAttempts = 3;

            for (var attempt = 1; attempt <= testAttempts; attempt++)
            {
                var saveChangesCalled = false;

                mockDataRepo.Setup(x => x.SaveChangesAsync())
                    .Callback(() => saveChangesCalled = true)
                    .Returns(Task.CompletedTask);

                // Act
                bool actualResult = await testUserRepo.CheckPasswordAsync(testUser, testPassword);

                // Assert
                Assert.IsFalse(actualResult, "Expected wrong password to return false");
                Assert.IsTrue(saveChangesCalled, "Expected save changes to be called");
                Assert.AreEqual(attempt, testUser.LoginAttempts, $"Expected user login attempts to be {attempt}");
            }
        }

    }

}

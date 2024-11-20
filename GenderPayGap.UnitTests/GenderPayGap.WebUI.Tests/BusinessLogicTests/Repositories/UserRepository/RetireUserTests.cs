using System;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Tests.Common.Classes;
using GenderPayGap.Tests.Common.TestHelpers;
using GenderPayGap.WebUI.Repositories;
using GenderPayGap.WebUI.Services;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Moq;
using NUnit.Framework;

namespace Repositories.UserRepository
{

    [TestFixture]
    [SetCulture("en-GB")]
    public class RetireUserTests
    {

        [SetUp]
        public void BeforeEach()
        {
            UiTestHelper.SetDefaultEncryptionKeys();
            
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

        private GenderPayGap.WebUI.Repositories.UserRepository testUserRepo;

        [TestCase]
        public void SavesRetiredStatus()
        {
            // Arrange
            var saveChangesCalled = false;
            User currentUser = testUserRepo.FindByEmail("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com");

            mockDataRepo.Setup(x => x.SaveChanges())
                .Callback(() => saveChangesCalled = true);

            // Act
            testUserRepo.RetireUser(currentUser);

            // Assert
            Assert.IsTrue(saveChangesCalled, "Expected SaveChanges to be called");
            Assert.AreEqual(currentUser.Status, UserStatuses.Retired, "Expected to change status to retired");
            Assert.AreEqual(currentUser.StatusDetails, "User retired", "Expected retire status details to be set");
        }

        [TestCase("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.New)]
        [TestCase("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.Unknown)]
        [TestCase("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.Retired)]
        public void ThrowsErrorWhenUserStatusIsNotActive(string testCurrentEmail, UserStatuses testStatus)
        {
            // Arrange
            DateTime testEmailVerifiedDate = VirtualDateTime.Now.Date.AddDays(-7);
            User currentUser = testUserRepo.FindByEmail(testCurrentEmail);
            currentUser.Status = testStatus;

            // Act
            var actualException = Assert.Throws<ArgumentException>(() => testUserRepo.RetireUser(currentUser));

            // Assert
            Assert.AreEqual("Can only retire active users. UserId=23322", actualException.Message, "Expected exception message to match");
            Assert.AreEqual(testStatus, currentUser.Status, "Expected status to still be the same");
        }

        [TestCase(null, "Value cannot be null. (Parameter 'userToRetire')")]
        public void ThrowsErrorWhenArgumentIsNull(User testUserArg, string expectedErrorMessage)
        {
            // Act
            var actualException = Assert.Throws<ArgumentNullException>(() => testUserRepo.RetireUser(testUserArg));

            // Assert
            Assert.AreEqual(expectedErrorMessage, actualException.Message, "Expected exception message to match");
        }

    }

}

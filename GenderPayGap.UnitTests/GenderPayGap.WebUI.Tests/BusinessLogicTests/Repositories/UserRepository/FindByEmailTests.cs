using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
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
    public class FindByEmailTests
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

        [TestCase("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.Active)]
        [TestCase("new1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.New, UserStatuses.New)]
        [TestCase("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.Active, UserStatuses.Active)]
        [TestCase("retired1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.Retired, UserStatuses.Retired)]
        public void FindsMatchingEmailUsingSingleStatusFilter(string testFindEmail,
            UserStatuses testExpectedStatus,
            params UserStatuses[] testStatusFilter)
        {
            // Act
            User actualUser = testUserRepo.FindByEmail(testFindEmail, testStatusFilter);

            // Assert
            Assert.AreEqual(testFindEmail, actualUser.EmailAddress, "Expected email to match");
            Assert.AreEqual(testExpectedStatus, actualUser.Status, "Expected user status to match");
        }

        [TestCase("new1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.New, UserStatuses.Retired)]
        [TestCase("retired1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.New, UserStatuses.Retired)]
        public void FindsMatchingEmailUsingMultipleStatusFilters(string testFindEmail, params UserStatuses[] testStatusFilters)
        {
            // Act
            User actualUser = testUserRepo.FindByEmail(testFindEmail, testStatusFilters);

            // Assert
            Assert.AreEqual(testFindEmail, actualUser.EmailAddress, "Expected email to match");
            Assert.IsTrue(testStatusFilters.Contains(actualUser.Status), "Expected user status to match");
        }

        [TestCase("missing@ad5bda75-e514-491b-b74d-4672542cbd15.com")]
        public void ReturnsNullWhenEmailDoesNotMatch(string testFindEmail)
        {
            // Act
            User actualUser = testUserRepo.FindByEmail(testFindEmail);

            // Assert
            Assert.IsNull(actualUser, "Expected user to be null");
        }

    }

}

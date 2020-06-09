using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic.Account.Abstractions;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Tests.Common.Classes;
using GenderPayGap.Tests.Common.TestHelpers;
using GenderPayGap.WebUI.Services;
using Moq;
using NUnit.Framework;

namespace Repositories.UserRepository
{

    [TestFixture]
    [SetCulture("en-GB")]
    public class FindBySubjectIdAsyncTests
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

        [TestCase(23322, UserStatuses.Active)]
        [TestCase(23322, UserStatuses.Active, UserStatuses.Active)]
        [TestCase(235251, UserStatuses.New, UserStatuses.New)]
        [TestCase(707643, UserStatuses.Retired, UserStatuses.Retired)]
        public async Task FindsMatchingUserIdUsingSingleStatusFilter(long testFindId,
            UserStatuses testExpectedStatus,
            params UserStatuses[] testStatusFilter)
        {
            // Act
            User actualUser = await testUserRepo.FindBySubjectIdAsync(testFindId, testStatusFilter);

            // Assert
            Assert.AreEqual(testFindId, actualUser.UserId, "Expected user id to match");
            Assert.AreEqual(testExpectedStatus, actualUser.Status, "Expected user status to match");
        }

        [TestCase(235251, UserStatuses.New, UserStatuses.Retired)]
        [TestCase(707643, UserStatuses.New, UserStatuses.Retired)]
        public async Task FindsMatchingUserIdUsingMultipleStatusFilters(long testFindId, params UserStatuses[] testStatusFilters)
        {
            // Act
            User actualUser = await testUserRepo.FindBySubjectIdAsync(testFindId, testStatusFilters);

            // Assert
            Assert.AreEqual(testFindId, actualUser.UserId, "Expected user id to match");
            Assert.IsTrue(testStatusFilters.Contains(actualUser.Status), "Expected user status to match");
        }

        [TestCase(999999999)]
        public async Task ReturnsNullWhenUserIdDoesNotMatch(long testFindId)
        {
            // Act
            User actualUser = await testUserRepo.FindBySubjectIdAsync(testFindId);

            // Assert
            Assert.IsNull(actualUser, "Expected user to be null");
        }

    }

}

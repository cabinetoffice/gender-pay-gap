using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic.Account.Abstractions;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.Tests.Common.Classes;
using GenderPayGap.Tests.Common.TestHelpers;
using Moq;
using NUnit.Framework;

namespace Repositories.UserRepository
{

    [TestFixture]
    [SetCulture("en-GB")]
    public class FindByEmailAsyncTests
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

        [TestCase("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.Active)]
        [TestCase("new1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.New, UserStatuses.New)]
        [TestCase("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.Active, UserStatuses.Active)]
        [TestCase("retired1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.Retired, UserStatuses.Retired)]
        public async Task FindsMatchingEmailUsingSingleStatusFilter(string testFindEmail,
            UserStatuses testExpectedStatus,
            params UserStatuses[] testStatusFilter)
        {
            // Act
            User actualUser = await testUserRepo.FindByEmailAsync(testFindEmail, testStatusFilter);

            // Assert
            Assert.AreEqual(testFindEmail, actualUser.EmailAddress, "Expected email to match");
            Assert.AreEqual(testExpectedStatus, actualUser.Status, "Expected user status to match");
        }

        [TestCase("new1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.New, UserStatuses.Retired)]
        [TestCase("retired1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.New, UserStatuses.Retired)]
        public async Task FindsMatchingEmailUsingMultipleStatusFilters(string testFindEmail, params UserStatuses[] testStatusFilters)
        {
            // Act
            User actualUser = await testUserRepo.FindByEmailAsync(testFindEmail, testStatusFilters);

            // Assert
            Assert.AreEqual(testFindEmail, actualUser.EmailAddress, "Expected email to match");
            Assert.IsTrue(testStatusFilters.Contains(actualUser.Status), "Expected user status to match");
        }

        [TestCase("missing@ad5bda75-e514-491b-b74d-4672542cbd15.com")]
        public async Task ReturnsNullWhenEmailDoesNotMatch(string testFindEmail)
        {
            // Act
            User actualUser = await testUserRepo.FindByEmailAsync(testFindEmail);

            // Assert
            Assert.IsNull(actualUser, "Expected user to be null");
        }

    }

}

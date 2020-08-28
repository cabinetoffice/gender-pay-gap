using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Tests.Common.Classes;
using GenderPayGap.WebUI.Services;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Moq;
using NUnit.Framework;

namespace Repositories.RegistrationRepository
{

    [TestFixture]
    [SetCulture("en-GB")]
    public class RemoveRegistrationAsyncTests
    {

        [SetUp]
        public void BeforeEach()
        {
            // mock data
            GpgDatabaseContext dbContext = AutoFacHelpers.CreateInMemoryTestDatabase(UserOrganisationHelper.CreateRegistrations());

            mockDataRepo = new SqlRepository(dbContext);
            var auditLoggerWithMocks = new AuditLogger(Mock.Of<IDataRepository>());

            // service under test
            testRegistrationRepo =
                new GenderPayGap.WebUI.Repositories.RegistrationRepository(mockDataRepo, auditLoggerWithMocks, null, null);
        }

        private IDataRepository mockDataRepo;

        private GenderPayGap.WebUI.Repositories.RegistrationRepository testRegistrationRepo;

        [Test]
        public async Task UserCanUnregisterAnotherUser()
        {
            // Arrange
            User testUnregisterUser = mockDataRepo.GetAll<User>()
                .AsEnumerable()
                .Where(u => u.EmailAddress == "active1@ad5bda75-e514-491b-b74d-4672542cbd15.com")
                .FirstOrDefault();

            User testActionByUser = mockDataRepo.GetAll<User>()
                .AsEnumerable()
                .Where(u => u.EmailAddress == "active2@ad5bda75-e514-491b-b74d-4672542cbd15.com")
                .FirstOrDefault();

            UserOrganisation testUserOrg = testUnregisterUser.UserOrganisations.FirstOrDefault();

            // Act
            await testRegistrationRepo.RemoveRegistrationAsync(testUserOrg, testActionByUser);

            // Assert user org removed
            Assert.IsNull(mockDataRepo.GetAll<UserOrganisation>().Where(uo => uo == testUserOrg).FirstOrDefault());
        }

        [Test]
        public async Task UserCanUnregisterThemselves()
        {
            // Arrange
            User testUnregisterUser = mockDataRepo.GetAll<User>()
                .AsEnumerable()
                .Where(u => u.EmailAddress == "active1@ad5bda75-e514-491b-b74d-4672542cbd15.com")
                .FirstOrDefault();

            UserOrganisation testUserOrg = testUnregisterUser.UserOrganisations.FirstOrDefault();

            // Act
            await testRegistrationRepo.RemoveRegistrationAsync(testUserOrg, testUnregisterUser);

            // Assert user org removed
            Assert.IsNull(mockDataRepo.GetAll<UserOrganisation>().Where(uo => uo == testUserOrg).FirstOrDefault());
        }

    }

}

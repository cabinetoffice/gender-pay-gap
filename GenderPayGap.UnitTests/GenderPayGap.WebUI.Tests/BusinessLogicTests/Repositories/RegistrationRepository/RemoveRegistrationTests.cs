using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Services;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Moq;

namespace Repositories.RegistrationRepository
{

    [TestFixture]
    [SetCulture("en-GB")]
    public class RemoveRegistrationTests
    {

        private IDataRepository mockDataRepo;
        private GenderPayGap.WebUI.Repositories.RegistrationRepository testRegistrationRepo;

        [SetUp]
        public void BeforeEach()
        {
            UiTestHelper.SetDefaultEncryptionKeys();
            
            // mock data
            GpgDatabaseContext dbContext = InMemoryTestDabaseHelper.CreateInMemoryTestDatabase(UserOrganisationHelper.CreateRegistrations());

            mockDataRepo = new SqlRepository(dbContext);
            var auditLoggerWithMocks = new AuditLogger(Mock.Of<IDataRepository>());

            // service under test
            testRegistrationRepo =
                new GenderPayGap.WebUI.Repositories.RegistrationRepository(mockDataRepo, auditLoggerWithMocks, null, null);
        }

        [TearDown]
        public void TearDown()
        {
            mockDataRepo.Dispose();
        }

        [Test]
        public void UserCanUnregisterAnotherUser()
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
            testRegistrationRepo.RemoveRegistration(testUserOrg, testActionByUser);

            // Assert user org removed
            Assert.IsNull(mockDataRepo.GetAll<UserOrganisation>().Where(uo => uo == testUserOrg).FirstOrDefault());
        }

        [Test]
        public void UserCanUnregisterThemselves()
        {
            // Arrange
            User testUnregisterUser = mockDataRepo.GetAll<User>()
                .AsEnumerable()
                .Where(u => u.EmailAddress == "active1@ad5bda75-e514-491b-b74d-4672542cbd15.com")
                .FirstOrDefault();

            UserOrganisation testUserOrg = testUnregisterUser.UserOrganisations.FirstOrDefault();

            // Act
            testRegistrationRepo.RemoveRegistration(testUserOrg, testUnregisterUser);

            // Assert user org removed
            Assert.IsNull(mockDataRepo.GetAll<UserOrganisation>().Where(uo => uo == testUserOrg).FirstOrDefault());
        }

    }

}

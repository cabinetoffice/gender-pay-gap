﻿using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Services;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Moq;

namespace Repositories.UserRepository
{

    [TestFixture]
    [SetCulture("en-GB")]
    public class RemoveRetiredUserRegistrationsTests
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
        public void UnregistersAllOrganisationsForUser()
        {
            // Arrange
            User testRetiredUser = mockDataRepo.GetAll<User>()
                .AsEnumerable()
                .Where(u => u.EmailAddress == "active1@ad5bda75-e514-491b-b74d-4672542cbd15.com")
                .FirstOrDefault();

            // Act
            testRegistrationRepo.RemoveRetiredUserRegistrations(testRetiredUser);

            // Assert user org removed
            Assert.IsFalse(
                mockDataRepo.GetAll<UserOrganisation>().Any(uo => uo.UserId == testRetiredUser.UserId),
                "Expected no registrations");
        }

    }

}

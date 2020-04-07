using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using GenderPayGap.BusinessLogic;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.Tests.Common.Classes;
using GenderPayGap.Tests.Common.Mocks;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.Tests
{

    // TODO: Move to BL test project :)

    [TestFixture]
    [SetCulture("en-GB")]
    public class ScopeBusinessLogicTests : AssertionHelper
    {

        private static IContainer DIContainer;

        private Mock<IDataRepository> mockDataRepo;
        private Mock<IFileRepository> mockFileRepo;
        private readonly ICommonBusinessLogic testCommonBL = new CommonBusinessLogic(Config.Configuration);

        private IList<Organisation> testOrgData;
        private IList<OrganisationScope> testOrgScopeData;
        public IScopeBusinessLogic testScopeBL;
        private ISearchBusinessLogic testSearchBL;
        private readonly ISearchRepository<EmployerSearchModel> testSearchRepo = new MockSearchRepository();

        #region Test Data

        private void GenerateTestData()
        {
            testOrgData = new List<Organisation>(
                new[] {
                    new Organisation {OrganisationId = 1, EmployerReference = "6B2LF57C"},
                    new Organisation {OrganisationId = 2, EmployerReference = "DR994D7L"},
                    new Organisation {OrganisationId = 3, EmployerReference = "23TYLBLB"},
                    new Organisation {OrganisationId = 4, EmployerReference = "SNGNB4BH"},
                    new Organisation {OrganisationId = 5, EmployerReference = "RWT2TY62"}
                });

            testOrgScopeData = new List<OrganisationScope>(
                new[] {
                    new OrganisationScope {
                        OrganisationScopeId = 15,
                        OrganisationId = 1,
                        Status = ScopeRowStatuses.Active,
                        ScopeStatusDate = VirtualDateTime.Now.AddDays(-51),
                        SnapshotDate = new DateTime(2017, 4, 5)
                    },
                    new OrganisationScope {
                        OrganisationScopeId = 25,
                        OrganisationId = 2,
                        ScopeStatusDate = VirtualDateTime.Now.AddDays(-5),
                        SnapshotDate = new DateTime(2018, 4, 5)
                    },
                    new OrganisationScope {
                        OrganisationScopeId = 35,
                        OrganisationId = 3,
                        ScopeStatusDate = VirtualDateTime.Now.AddDays(-2),
                        SnapshotDate = new DateTime(2017, 4, 5),
                        ContactEmailAddress = "user@test.com",
                        RegisterStatus = RegisterStatuses.RegisterPending
                    },
                    new OrganisationScope {
                        OrganisationScopeId = 45,
                        OrganisationId = 4,
                        ScopeStatusDate = VirtualDateTime.Now.AddDays(-100),
                        SnapshotDate = new DateTime(2017, 4, 5),
                        ScopeStatus = ScopeStatuses.OutOfScope
                    },
                    new OrganisationScope {
                        OrganisationScopeId = 55,
                        OrganisationId = 4,
                        ScopeStatusDate = VirtualDateTime.Now.AddDays(-44),
                        SnapshotDate = new DateTime(2017, 4, 5),
                        ScopeStatus = ScopeStatuses.InScope
                    },
                    new OrganisationScope {
                        OrganisationScopeId = 65,
                        OrganisationId = 4,
                        Status = ScopeRowStatuses.Active,
                        ScopeStatusDate = VirtualDateTime.Now.AddDays(-2),
                        SnapshotDate = new DateTime(2017, 4, 5),
                        ScopeStatus = ScopeStatuses.OutOfScope
                    }
                });

            testOrgData.ForEach(o => o.OrganisationScopes = testOrgScopeData.Where(os => o.OrganisationId == os.OrganisationId).ToList());
        }

        #endregion

        [OneTimeSetUp]
        public static void Init()
        {
            var builder = new ContainerBuilder();


            DIContainer = builder.Build();
        }

        [SetUp]
        public void BeforeEach()
        {
            mockDataRepo = new Mock<IDataRepository>();
            mockFileRepo = new Mock<IFileRepository>();
            testSearchBL = new SearchBusinessLogic(testSearchRepo);
            testScopeBL = new ScopeBusinessLogic(testCommonBL, mockDataRepo.Object, testSearchBL);
            GenerateTestData();
        }

        #region UpdateOrgScopeStatus()

        [Test]
        [Description("UpdateOrgScopeStatus: When Organisation is Found Then Should Save New Scope Record With New Status")]
        public async Task UpdateOrgScopeStatus_When_Organisation_is_Found_Then_Should_Save_New_Scope_Record_With_New_Status()
        {
            // Arrange
            var saveChangesCalled = false;
            var testExistingOrgScope = new OrganisationScope {
                OrganisationScopeId = 123,
                OrganisationId = 3,
                ContactEmailAddress = "existing@test.com",
                ContactFirstname = "Existing Firstname",
                ContactLastname = "Existing Lastname",
                ReadGuidance = true,
                Reason = "Under250",
                ScopeStatus = ScopeStatuses.OutOfScope,
                ScopeStatusDate = VirtualDateTime.Now.AddDays(-20),
                RegisterStatus = RegisterStatuses.RegisterComplete,
                RegisterStatusDate = VirtualDateTime.Now.AddDays(-20)
            };

            mockDataRepo.SetupGetAll(testOrgData, testExistingOrgScope);
            //mockDataRepo.SetupGetAll(testOrgData);
            //mockDataRepo.SetupGetAll(testExistingOrgScope);

            mockDataRepo.Setup(r => r.SaveChangesAsync()).Callback(() => saveChangesCalled = true).Returns(Task.CompletedTask);

            // Act
            OrganisationScope newOrgScope = await testScopeBL.UpdateScopeStatusAsync(123, ScopeStatuses.InScope);

            // Assert
            Expect(newOrgScope != null);
            Organisation expectedOrg = testOrgData.FirstOrDefault(o => o.OrganisationId == testExistingOrgScope.OrganisationId);

            Expect(expectedOrg != null);
            Expect(saveChangesCalled, "Expected SaveChanges() to be called");

            Assert.AreEqual(newOrgScope.OrganisationId, testExistingOrgScope.OrganisationId, "Expected OrganisationId");
            Assert.AreEqual(newOrgScope.ContactEmailAddress, testExistingOrgScope.ContactEmailAddress, "Expected ContactEmailAddress");
            Assert.AreEqual(newOrgScope.ContactFirstname, testExistingOrgScope.ContactFirstname, "Expected ContactFirstname");
            Assert.AreEqual(newOrgScope.ContactLastname, testExistingOrgScope.ContactLastname, "Expected ContactLastname");
            Assert.AreEqual(newOrgScope.ReadGuidance, testExistingOrgScope.ReadGuidance, "Expected ReadGuidance");
            Assert.AreEqual(newOrgScope.Reason, testExistingOrgScope.Reason, "Expected Reason");
            Assert.AreEqual(newOrgScope.ScopeStatus, ScopeStatuses.InScope, "Expected ScopeStatus");
            Assert.Greater(newOrgScope.ScopeStatusDate, testExistingOrgScope.ScopeStatusDate, "Expected ScopeStatusDate");
            Assert.AreEqual(newOrgScope.RegisterStatus, testExistingOrgScope.RegisterStatus, "Expected RegisterStatus");
            Assert.AreEqual(newOrgScope.RegisterStatusDate, testExistingOrgScope.RegisterStatusDate, "Expected RegisterStatusDate");
        }

        #endregion

        #region SaveScope()

        [Test]
        [Description("SaveScope: Retires any previous submissions in the same snapshot year")]
        public async Task SaveScope_retires_any_previous_submissions_in_the_same_snapshot_year()
        {
            Organisation testOrg = testOrgData.Where(o => o.OrganisationId == 4).FirstOrDefault();
            var testNewScope = new OrganisationScope {OrganisationId = 4, SnapshotDate = new DateTime(2017, 4, 5)};
            var saveChangesCalled = false;

            // Mocks
            mockDataRepo.Setup(r => r.GetAll<OrganisationScope>()).Returns(testOrgScopeData.AsQueryable().BuildMock().Object);
            mockDataRepo.Setup(r => r.SaveChangesAsync()).Callback(() => saveChangesCalled = true).Returns(Task.CompletedTask);

            // Test
            await testScopeBL.SaveScopeAsync(testOrg, true, testNewScope);

            // Assert
            Expect(saveChangesCalled, "Expected SaveChanges() to be called");
            Expect(testOrg.OrganisationScopes.Contains(testNewScope), "Expected org.OrganisationScopes to contain new scope");

            // ensure only one submitted record for the current snapshot year
            Expect(testNewScope.Status == ScopeRowStatuses.Active, "Expected new scope status to be submitted");
            Expect(
                testOrg.OrganisationScopes.Count(os => os.Status == ScopeRowStatuses.Active && os.SnapshotDate.Year == 2017) == 1,
                "Expected Count(OrganisationScopes == submitted) to be 1");
        }

        #endregion

        #region GetOrgScopeById()

        [Test]
        [Description("GetOrgScopeById: When ScopeId doesn't Match Then Return Null")]
        public async Task GetOrgScopeById_When_ScopeId_doesnt_Match_Then_Return_Null()
        {
            // Arrange
            mockDataRepo.Setup(r => r.GetAll<OrganisationScope>()).Returns(testOrgScopeData.AsQueryable().BuildMock().Object);

            // Act
            OrganisationScope result = await testScopeBL.GetScopeByIdAsync(123);

            // Assert
            Assert.That(result == null, "Expected to return null when using a non existing scopeId");
        }

        [Test]
        [Description("GetOrgScopeById: When ScopeId Exists Then Return Scope")]
        public async Task GetOrgScopeById_When_ScopeId_Exists_Then_Return_ScopeAsync()
        {
            // Arrange
            mockDataRepo.Setup(r => r.GetAll<OrganisationScope>()).Returns(testOrgScopeData.AsQueryable().BuildMock().Object);

            // Act
            OrganisationScope result = await testScopeBL.GetScopeByIdAsync(35);

            // Assert
            Assert.That(result != null, "The result was null when using an existing scopeId");
            Assert.That(result.OrganisationScopeId == 35, "Expected OrganisationScopeId to match");
            Assert.That(result.OrganisationId == 3, "Expected OrganisationId to match");
            Assert.That(result.ContactEmailAddress == "user@test.com", "Expected ContactEmailAddress to match");
        }

        #endregion

        #region GetOrgScopeByEmployerReference()

        [Test]
        [Description("GetOrgScopeByEmployerReference: When EmployerReference doesn't Match Then Return Null")]
        public async Task GetOrgScopeByEmployerReference_When_EmployerReference_doesnt_Match_Then_Return_Null()
        {
            // Arrange
            var testEmployerRef = "AAAABBBB";
            var testSnapshotYear = 2017;
            mockDataRepo.Setup(r => r.GetAll<Organisation>()).Returns(new List<Organisation>().AsQueryable().BuildMock().Object);

            // Act
            OrganisationScope result = await testScopeBL.GetScopeByEmployerReferenceAsync(testEmployerRef, testSnapshotYear);

            // Assert
            Assert.That(result == null, "Expected to return null when using a non existing EmployerReference");
        }

        [Test]
        [Description("GetOrgScopeByEmployerReference: When EmployerReference Exists But Has No Scope Then Return Null")]
        public async Task GetOrgScopeByEmployerReference_When_EmployerReference_Exists_But_Has_No_Scope_Then_Return_Null()
        {
            // Arrange
            var testEmployerRef = "RWT2TY62";
            var testSnapshotYear = 2017;
            mockDataRepo.Setup(r => r.GetAll<Organisation>()).Returns(testOrgData.AsQueryable().BuildMock().Object);

            // Act
            OrganisationScope result = await testScopeBL.GetScopeByEmployerReferenceAsync("RWT2TY62", testSnapshotYear);

            // Assert
            Assert.That(result == null, "Expected to return null when using an existing EmployerReference that has no scope");
        }

        [Test]
        [Description("GetOrgScopeByEmployerReference: When EmployerReference Matches Then Return OrganisationScope Model")]
        public async Task GetOrgScopeByEmployerReference_When_EmployerReference_Matches_Then_Return_OrganisationScope_ModelAsync()
        {
            // Arrange
            var testEmployerRef = "SNGNB4BH";
            var testSnapshotYear = 2017;
            mockDataRepo.Setup(r => r.GetAll<Organisation>()).Returns(testOrgData.AsQueryable().BuildMock().Object);
            mockDataRepo.Setup(r => r.GetAll<OrganisationScope>()).Returns(testOrgScopeData.AsQueryable().BuildMock().Object);

            // Act
            OrganisationScope result = await testScopeBL.GetScopeByEmployerReferenceAsync(testEmployerRef, testSnapshotYear);

            // Assert
            Assert.That(result != null, "Expected to return a valid OrganisationScope Model");
            Assert.That(result.OrganisationId == 4, "Expected the model to have the same OrganisationId");
            Assert.That(result.OrganisationScopeId == 65, "Expected the model to have the same OrganisationScopeId");
        }

        #endregion

        #region GetLatestScopeForSnapshotYear()

        [Test]
        [Description("GetLatestScopeForSnapshotYear: When SnapshotYear doesn't Match Then Return Null")]
        public async Task GetLatestScopeForSnapshotYear_When_SnapshotYear_doesnt_Match_Then_Return_Null()
        {
            // Arrange
            mockDataRepo.Setup(r => r.GetAll<OrganisationScope>()).Returns(testOrgScopeData.AsQueryable().BuildMock().Object);
            mockDataRepo.Setup(r => r.GetAll<Organisation>()).Returns(testOrgData.AsQueryable().BuildMock().Object);

            // Act
            OrganisationScope result = await testScopeBL.GetLatestScopeBySnapshotYearAsync(4, 2016);

            // Assert
            Assert.That(result == null, "Expected to return null when using an existing EmployerReference that has no scope");
        }

        [Test]
        [Description("GetLatestScopeForSnapshotYear: When SnapshotYear Matches Then Return LatestScopeForSnapshotYear")]
        public async Task GetLatestScopeForSnapshotYear_When_SnapshotYear_Matches_Then_Return_LatestScopeForSnapshotYearAsync()
        {
            // Arrange
            mockDataRepo.Setup(r => r.GetAll<OrganisationScope>()).Returns(testOrgScopeData.AsQueryable().BuildMock().Object);
            mockDataRepo.Setup(r => r.GetAll<Organisation>()).Returns(testOrgData.AsQueryable().BuildMock().Object);

            // Act
            OrganisationScope result = await testScopeBL.GetLatestScopeBySnapshotYearAsync(4, 2017);

            // Assert
            Assert.That(result != null, "Expected to return a valid OrganisationScope Model");
            Assert.That(result.OrganisationId == 4, "Expected the model to have the same OrganisationId");
            Assert.That(result.OrganisationScopeId == 65, "Expected the model to have the same OrganisationScopeId");
        }

        #endregion

        #region GetPendingScopeRegistration()

        [Test]
        [Description("GetPendingScopeRegistration: When EmailAddress doesn't Match Then Return Null")]
        public async Task GetPendingScopeRegistration_When_EmailAddress_doesnt_Match_Then_Return_Null()
        {
            // Arrange
            mockDataRepo.Setup(r => r.GetAll<OrganisationScope>()).Returns(testOrgScopeData.AsQueryable().BuildMock().Object);

            // Act
            OrganisationScope result = await testScopeBL.GetPendingScopeRegistrationAsync("123@123.com");
            // Assert
            Assert.That(result == null, "Expected to return null when using a non existing EmailAddress");
        }

        [Test]
        [Description("GetPendingScopeRegistration: When EmailAddress Matches Then Return OrganisationScope Model")]
        public async Task GetPendingScopeRegistration_When_EmailAddress_Matches_Then_Return_OrganisationScope_ModelAsync()
        {
            // Arrange
            mockDataRepo.Setup(r => r.GetAll<OrganisationScope>()).Returns(testOrgScopeData.AsQueryable().BuildMock().Object);

            // Act
            OrganisationScope result = await testScopeBL.GetPendingScopeRegistrationAsync("user@test.com");

            // Assert
            Assert.That(result != null, "Expected to return a valid OrganisationScope Model");
            Assert.That(result.OrganisationScopeId == 35, "Expected the model to have the same OrganisationScopeId");
        }

        #endregion

    }

}

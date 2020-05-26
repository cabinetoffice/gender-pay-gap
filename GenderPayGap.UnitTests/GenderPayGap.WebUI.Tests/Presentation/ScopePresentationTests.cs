using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.Tests.Common.Classes;
using GenderPayGap.Tests.Common.Mocks;
using GenderPayGap.WebUI.Classes.Services;
using GenderPayGap.WebUI.Models.Scope;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.Tests
{

    [TestFixture(Category = "Presentation")]
    [SetCulture("en-GB")]
    public class ScopePresentationTests : AssertionHelper
    {

        private Mock<IDataRepository> mockDataRepo;
        private Mock<ScopeBusinessLogic> mockScopeBL;
        private readonly ICommonBusinessLogic testCommonBL = new CommonBusinessLogic(Config.Configuration);
        private ScopePresentation testScopePresentation;

        private ISearchBusinessLogic testSearchBL;
        private readonly ISearchRepository<EmployerSearchModel> testSearchRepo = new MockSearchRepository();

        [SetUp]
        public void BeforeEach()
        {
            mockDataRepo = MoqHelpers.CreateMockAsyncDataRepository();

            testSearchBL = new SearchBusinessLogic(testSearchRepo);

            // setup mocks ans ensures they call their implementations. (We override calls per test when need be!)
            mockScopeBL = new Mock<ScopeBusinessLogic>(testCommonBL, mockDataRepo.Object, testSearchBL);
            mockScopeBL.CallBase = true;

            // service under test
            testScopePresentation = new ScopePresentation(mockScopeBL.Object, mockDataRepo.Object);
        }

        #region AuthAndCreateViewModel()

        [Ignore("Needs fixing/deleting")]
        [Test]
        [Description("AuthAndCreateViewModel: When Organisation doesn't exist then throw ArgumentOutOfRangeException")]
        public void AuthAndCreateViewModel_When_Organisation_doesnt_exist_then_throw_ArgumentOutOfRangeException()
        {
            // Arrange
            var testModel = new EnterCodesViewModel {EmployerReference = "ABCDEFG", SecurityToken = ""};

            var testUser = new User {UserId = 1};

            // Act
            TestDelegate testDelegate = async () => await testScopePresentation.CreateScopingViewModelAsync(testModel, testUser);

            // Assert
            Assert.That(
                testDelegate,
                Throws.TypeOf<ArgumentOutOfRangeException>()
                    .With
                    .Message
                    .Contains($"Cannot find organisation with EmployerReference: {testModel.EmployerReference}"));
        }

        [Ignore("Needs fixing/deleting")]
        [Test]
        [Description("AuthAndCreateViewModel: When Previous Scope Submission exists then HasPreviousSubmission should be true")]
        public async Task AuthAndCreateViewModel_When_Previous_Scope_Submission_Exists_Then_HasPreviousSubmission_Should_Be_TrueAsync()
        {
            // Arrange
            var testOrganisationId = 412;
            var testSnapshotYear = 2017;
            var testModel = new EnterCodesViewModel {EmployerReference = "ABCDEFG", SecurityToken = "BBBBSSSS"};

            //mockScopeBL.CallBase = false;
            mockScopeBL.Setup(r => r.GetScopeByEmployerReferenceAsync(It.IsIn(testModel.EmployerReference), It.IsIn(testSnapshotYear)))
                .ReturnsAsync(new OrganisationScope {OrganisationScopeId = 12, ScopeStatus = ScopeStatuses.OutOfScope});

            var testUser = new User {UserId = 1};

            // Act
            ScopingViewModel actualState = await testScopePresentation.CreateScopingViewModelAsync(testModel, testUser);

            // Assert
            //Expect(actualState.HasPrevScope == true, "Expected HasPrevScope to be true");
            //Expect(actualState.PrevOrgScopeId > 0, "Expected PrevOrgScopeId to NOT be null");
            //Expect(actualState.PrevOrgScopeStatus == ScopeStatuses.OutOfScope, "Expected PrevOrgScopeStatus to be OutOfScope");
            Expect(
                actualState.EnterCodes.EmployerReference == testModel.EmployerReference,
                "Expected EmployerReference to be testEmployerRef");
        }

        [Ignore("Needs fixing/deleting")]
        [Test]
        [Description("AuthAndCreateViewModel: When Previous Scope Submission does not exist then HasPreviousSubmission should be false")]
        public void AuthAndCreateViewModel_When_No_Previous_Scope_Submission_Exists_Then_HasPreviousSubmission_Should_Be_False()
        {
            // Arrange
            var testEmployerRef = "ABCDEFG";
            var testSecurityTok = "11113333";
            var testModel = new EnterCodesViewModel {EmployerReference = testEmployerRef, SecurityToken = testSecurityTok};

            mockDataRepo.SetupGetAll(new Organisation());

            var testUser = new User {UserId = 1};

            // Act
            Task<ScopingViewModel> actualModel = testScopePresentation.CreateScopingViewModelAsync(testModel, testUser);

            // Assert
            //Assert.That(actualModel.HasPrevScope == false, "Expected HasPrevScope to be false");
            //Assert.That(actualModel.PrevOrgScopeId == -1, "Expected PrevOrgScopeId to be -1");
        }

        [Ignore("Needs fixing/deleting")]
        [Test]
        [Description("AuthAndCreateViewModel: When SecurityToken has expired then SecurityCodeExpired should be true")]
        public async Task AuthAndCreateViewModel_When_SecurityToken_Has_Expired_Then_SecurityCodeExpired_Should_Be_TrueAsync()
        {
            // Arrange
            var testEmployerRef = "ABCDEFG";
            var testSecurityTok = "11113333";
            var testModel = new EnterCodesViewModel {EmployerReference = testEmployerRef, SecurityToken = testSecurityTok};

            DateTime testExpiredDate = VirtualDateTime.Now.AddDays(-(Global.SecurityCodeExpiryDays + 1));

            //Always returns an organisation and scope
            mockDataRepo.SetupGetAll(new Organisation(), new OrganisationScope());

            var testUser = new User {UserId = 1};

            // Act
            ScopingViewModel actualModel = await testScopePresentation.CreateScopingViewModelAsync(testModel, testUser);

            // Assert
            Assert.That(actualModel.IsSecurityCodeExpired, "Expected SecurityCodeExpired to be true");
        }

        [Ignore("Needs fixing/deleting")]
        [Test]
        [Description("AuthAndCreateViewModel: When SecurityToken is active then SecurityCodeExpired should be false")]
        public async Task AuthAndCreateViewModel_When_SecurityToken_Is_Active_Then_SecurityCodeExpired_Should_Be_FalseAsync()
        {
            // Arrange
            var testEmployerRef = "ABCDEFG";
            var testSecurityTok = "11113333";
            var testModel = new EnterCodesViewModel {EmployerReference = testEmployerRef, SecurityToken = testSecurityTok};

            DateTime testExpiredDate = VirtualDateTime.Now.AddDays(-(Global.SecurityCodeExpiryDays - 1));

            //Always returns an organisation and scope
            mockDataRepo.SetupGetAll(new Organisation(), new OrganisationScope());

            var testUser = new User {UserId = 1};

            // Act
            ScopingViewModel actualModel = await testScopePresentation.CreateScopingViewModelAsync(testModel, testUser);

            // Assert
            Assert.That(actualModel.IsSecurityCodeExpired == false, "Expected SecurityCodeExpired to be false");
        }

        [Ignore("Needs fixing/deleting")]
        [Test]
        [Description("AuthAndCreateViewModel: When Successfull then return OrgName and Address")]
        public async Task AuthAndCreateViewModel_When_Successfull_Then_Return_OrgName_AddressAsync()
        {
            // Arrange
            var testEmployerRef = "ABCDEFG";
            var testSecurityTok = "11113333";
            var testModel = new EnterCodesViewModel {EmployerReference = testEmployerRef, SecurityToken = testSecurityTok};

            var testAddress1 = "Address1";
            var testAddress2 = "Address2";
            var testAddress3 = "testAddress3";
            var testCity = "testCity";
            var testPOBox = "testPOBox";
            var testPostalCode = "testPostalCode";
            var expectedOrgName = "Test Org Name";
            string expectedAddress = $"{testAddress1}, {testAddress2}, {testAddress3}, {testCity}, {testPostalCode}, {testPOBox}";

            //Always returns an organisation and scope
            mockDataRepo.SetupGetAll(new Organisation(), new OrganisationScope());

            var testUser = new User {UserId = 1};

            // Act
            ScopingViewModel actualModel = await testScopePresentation.CreateScopingViewModelAsync(testModel, testUser);

            // Assert
            Assert.That(actualModel.OrganisationName == expectedOrgName, $"Expected OrgName to be {expectedOrgName}");
            Assert.That(actualModel.OrganisationAddress == expectedAddress, $"Expected OrgAddress to be {expectedAddress}");
            //Assert.That(actualModel.AccountingDate.Year == VirtualDateTime.Now.Year, $"Expected AccountingDate year to be {VirtualDateTime.Now.Year}");
        }

        #endregion

        #region SavePresumedScope()

        [Test]
        [Description("SavePresumedScope: When 'EmployerReference' IsNullOrWhiteSpace then throw ArgumentNullException")]
        public void SavePresumedScope_EmployerReference_IsNullOrWhiteSpace()
        {
            // Setup
            string[] whiteSpaces = {null, string.Empty, "   "};

            foreach (string whiteSpace in whiteSpaces)
            {
                Assert.ThrowsAsync<ArgumentNullException>(
                    async () => {
                        var model = new ScopingViewModel {EnterCodes = new EnterCodesViewModel {EmployerReference = whiteSpace}};
                        await testScopePresentation.SavePresumedScopeAsync(model, 2018);
                    });
            }
        }

        [Test]
        [Description("SavePresumedScope: When 'SecurityCodeExpired' true then throw ArgumentOutOfRangeException")]
        public void SavePresumedScope_SecurityCodeExpired_True()
        {
            // Setup
            var testState = new ScopingViewModel();
            testState.IsSecurityCodeExpired = true;
            testState.EnterCodes = new EnterCodesViewModel {EmployerReference = "ABCD1234"};

            Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => testScopePresentation.SavePresumedScopeAsync(testState, 2018));
        }

        [Test]
        [Description("SavePresumedScope: When 'EmployerReference' not found then throw ArgumentOutOfRangeException")]
        public void SavePresumedScope_EmployerReference_Not_Found()
        {
            // Setup
            var testState = new ScopingViewModel();
            testState.IsSecurityCodeExpired = false;
            testState.EnterCodes = new EnterCodesViewModel {EmployerReference = "ZXCV4567"};

            var ex = Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => testScopePresentation.SavePresumedScopeAsync(testState, 2018));

            Assert.That(ex.Message.Contains($"Cannot find organisation with EmployerReference: {testState.EnterCodes.EmployerReference}"));
        }

        [Test]
        [Description("SavePresumedScope: Throw an ArgumentOutOfRangeException when snapshot year is invalid")]
        public void SaveScope_throw_an_ArgumentOutOfRangeException_when_snapshot_year_is_invalid()
        {
            // Setup
            var testState = new ScopingViewModel();
            testState.IsSecurityCodeExpired = false;
            testState.EnterCodes = new EnterCodesViewModel {EmployerReference = "ZXCV4567"};

            var organisations =
                new List<Organisation> {new Organisation {EmployerReference = "ZXCV4567", SectorType = SectorTypes.Private}};
            mockDataRepo.Setup(r => r.GetAll<Organisation>()).Returns(organisations.AsQueryable().BuildMock().Object);

            // greater than the current snapshot year
            int testSnapshotYear = SectorTypes.Private.GetAccountingStartDate().Year + 1;

            var ex = Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => testScopePresentation.SavePresumedScopeAsync(testState, testSnapshotYear));
            Assert.That(ex.Message.Contains("Parameter name: snapshotYear"));

            // less than the prev snapshot year
            testSnapshotYear -= SectorTypes.Private.GetAccountingStartDate().Year - 2;
            ex = Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => testScopePresentation.SavePresumedScopeAsync(testState, testSnapshotYear));

            Assert.That(ex.Message.Contains("Parameter name: snapshotYear"));
        }

        #endregion

        #region CreateViewModelFromUserOrganisation()

        [Ignore("Needs fixing")]
        [Test]
        [Description("CreateViewModelFromUserOrganisation: When HasPrevScope then PrevOrgScopeId is set")]
        public void CreateViewModelFromUserOrganisation_When_HasPrevScope_then_PrevOrgScope_is_set()
        {
            var testOrg = new Organisation
            {
                SectorType = SectorTypes.Private,
                OrganisationAddresses = new List<OrganisationAddress> { new OrganisationAddress() }
            };

            var testLatestScope = new OrganisationScope {
                OrganisationScopeId = 123, ScopeStatus = ScopeStatuses.OutOfScope, Organisation = testOrg
            };

            var testUser = new User {UserId = 1};

            ScopingViewModel actualModel = testScopePresentation.CreateScopingViewModel(testOrg, testUser);
            //Expect(actualModel.HasPrevScope == true, "");
            //Expect(actualModel.PrevOrgScopeId == 123, "");
            //Expect(actualModel.PrevOrgScopeStatus == ScopeStatuses.OutOfScope, "");
        }

        [Ignore("needs fixing")]
        [Test]
        [Description("CreateViewModelFromUserOrganisation: When HasPrevScope then PrevOrgScopeId is not set")]
        public void CreateViewModelFromUserOrganisation_When_HasPrevScope_then_PrevOrgScope_is_not_set()
        {
            //Organisation testOrg = new Organisation { SectorType = SectorTypes.Private, LatestAddress = new OrganisationAddress { } };

            //OrganisationScope testLatestScope = null;

            //int testSnapshotYear = 2017;
            //ScopeStatuses testScopeStatus = ScopeStatuses.InScope;
            //var testUser = new User() { UserId = 1 };

            //var actualModel = testScopePresentation.CreateScopingViewModel(testOrg,testUser);
            //Expect(actualModel.HasPrevScope == false, "");
            //Expect(actualModel.PrevOrgScopeId == -1, "");
            //Expect(actualModel.PrevOrgScopeStatus == ScopeStatuses.Unknown, "");
        }

        #endregion

    }

}

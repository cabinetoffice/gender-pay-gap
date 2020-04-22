using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.Controllers;
using GenderPayGap.WebUI.Models.Organisation;
using GenderPayGap.WebUI.Services;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Controllers
{

    [TestFixture]
    [SetCulture("en-GB")]
    public partial class OrganisationControllerTests : AssertionHelper
    {

        [SetUp]
        public void BeforeEach()
        {
            MockOrganisations = new[]
            {
                new Organisation
                {
                    OrganisationId = 123,
                    OrganisationName = "Org123",
                    OrganisationAddresses = new List<OrganisationAddress>{ new OrganisationAddress() },
                    SectorType = SectorTypes.Private
                },
                new Organisation
                {
                    OrganisationId = 456,
                    OrganisationName = "Org456",
                    OrganisationAddresses = new List<OrganisationAddress>{ new OrganisationAddress() },
                    SectorType = SectorTypes.Private
                }
            };

            MockUsers = new[]
            {
                CreateUser(1, "mockuser1@test.com"),
                CreateUser(2, "mockuser2@test.com"),
                CreateUser(3, "mockuser3@test.com"),
                CreateUser(4, "mockuser4@test.com"),
                CreateUser(5, "mockadmin1@geo.gov.uk")
            };

            Organisation org123 = Array.Find(MockOrganisations, x => x.OrganisationId == 123);
            Organisation org456 = Array.Find(MockOrganisations, x => x.OrganisationId == 456);

            MockUserOrganisations = new[]
            {
                CreateUserOrganisation(org123, 1, VirtualDateTime.Now),
                CreateUserOrganisation(org123, 2, VirtualDateTime.Now),
                CreateUserOrganisation(org123, 3, null),
                CreateUserOrganisation(org456, 4, VirtualDateTime.Now)
            };
        }

        private User[] MockUsers;

        private UserOrganisation[] MockUserOrganisations;

        private Organisation[] MockOrganisations;

        private static UserOrganisation CreateUserOrganisation(Organisation org, long userId, DateTime? pinConfirmedDate)
        {
            return new UserOrganisation
            {
                Organisation = org, UserId = userId, PINConfirmedDate = pinConfirmedDate, Address = org.GetLatestAddress()
            };
        }

        private static Organisation createPrivateOrganisation(long organistationId, string organisationName, int companyNumber)
        {
            return new Organisation
            {
                OrganisationId = organistationId,
                OrganisationName = organisationName,
                SectorType = SectorTypes.Private,
                Status = OrganisationStatuses.Active,
                CompanyNumber = companyNumber.ToString()
            };
        }

        private static User CreateUser(long userId, string emailAddress)
        {
            return new User
            {
                UserId = userId,
                EmailAddress = emailAddress,
                EmailAddressDB = emailAddress,
                Firstname = "FirstName" + userId,
                Lastname = "LastName" + userId,
                EmailVerifiedDate = VirtualDateTime.Now,
                Status = UserStatuses.Active
            };
        }

        [Test]
        [Description("ChangeOrganisationScope GET: When request IsNullOrWhitespace Then Return BadRequest")]
        public void ChangeOrganisationScope_GET_When_request_IsNullOrWhitespace_Then_Return_BadRequest()
        {
            // Arrange
            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "ChangeOrganisationScope");
            mockRouteData.Values.Add("Controller", "Organisation");

            var controller = UiTestHelper.GetController<OrganisationController>(
                MockUsers[0].UserId,
                mockRouteData,
                MockUsers,
                MockUserOrganisations);
            var testRequest = "";

            // Act
            IActionResult actionResult = controller.ChangeOrganisationScope(testRequest);

            // Assert
            var httpStatusResult = actionResult as HttpStatusViewResult;
            Assert.NotNull(httpStatusResult, "httpStatusResult should not be null");
            Assert.AreEqual(httpStatusResult.StatusCode, (int) HttpStatusCode.BadRequest, "Expected the StatusCode to be a 'BadRequest'");
        }

        [Test]
        [Description("ChangeOrganisationScope GET: When User Not Assoc to Org Then Return Forbidden")]
        public void ChangeOrganisationScope_GET_When_User_Not_Assoc_to_Org_Then_Return_Forbidden()
        {
            // Arrange
            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "ChangeOrganisationScope");
            mockRouteData.Values.Add("Controller", "Organisation");

            var testUserId = 4;
            var testOrgId = 123;

            var controller = UiTestHelper.GetController<OrganisationController>(
                testUserId,
                mockRouteData,
                MockUsers,
                MockUserOrganisations);

            string testRequest = Encryption.EncryptAsParams(testOrgId.ToString(), "2017");

            // Act
            IActionResult actionResult = controller.ChangeOrganisationScope(testRequest);

            // Assert
            var httpStatusResult = actionResult as HttpStatusViewResult;
            Assert.NotNull(httpStatusResult, "httpStatusResult should not be null");
            Assert.AreEqual(httpStatusResult.StatusCode, (int) HttpStatusCode.Forbidden, "Expected the StatusCode to be a 'Forbidden'");
        }

        [Test]
        [Description("DeclareScope GET: When Id IsNullOrWhitespace Then Return BadRequest")]
        public async Task DeclareScope_GET_When_Id_IsNullOrWhitespace_Then_Return_BadRequest()
        {
            // Arrange
            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "DeclareScope");
            mockRouteData.Values.Add("Controller", "Organisation");

            var controller = UiTestHelper.GetController<OrganisationController>(
                MockUsers[0].UserId,
                mockRouteData,
                MockUsers,
                MockUserOrganisations);
            var testOrgId = "";

            // Act
            DateTime snapshotDate = SectorTypes.Private.GetAccountingStartDate();

            // Act
            IActionResult actionResult = await controller.DeclareScope(testOrgId);


            // Assert
            var httpStatusResult = actionResult as HttpStatusViewResult;
            Assert.NotNull(httpStatusResult, "httpStatusResult should not be null");
            Assert.AreEqual(httpStatusResult.StatusCode, (int) HttpStatusCode.BadRequest, "Expected the StatusCode to be a 'BadRequest'");
        }

        [Test]
        [Description("DeclareScope GET: When Org no scopes return BadRequest")]
        public async Task DeclareScope_GET_When_NoScopes_Then_Return_BadRequest()
        {
            // Arrange
            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "DeclareScope");
            mockRouteData.Values.Add("Controller", "Organisation");

            var testUserId = 2;
            var testOrgId = 123;

            DateTime lastSnapshotDate = SectorTypes.Private.GetAccountingStartDate().AddYears(-1);

            var mockLastScope = new OrganisationScope
            {
                OrganisationId = testOrgId,
                Status = ScopeRowStatuses.Active,
                ScopeStatus = ScopeStatuses.InScope,
                SnapshotDate = lastSnapshotDate
            };

            var controller = UiTestHelper.GetController<OrganisationController>(
                testUserId,
                mockRouteData,
                MockUsers,
                MockOrganisations,
                MockUserOrganisations,
                mockLastScope);

            Mock<IScopeBusinessLogic> mockScopeBL = AutoFacExtensions.ResolveAsMock<IScopeBusinessLogic>(true);

            string encOrgId = Encryption.EncryptQuerystring(testOrgId.ToString());

            // Act
            IActionResult actionResult = await controller.DeclareScope(encOrgId);

            // Assert
            Expect(actionResult != null, "actionResult should not be null");

            // Assert
            var httpStatusResult = actionResult as HttpStatusViewResult;
            Assert.NotNull(httpStatusResult, "httpStatusResult should not be null");
            Assert.AreEqual(httpStatusResult.StatusCode, (int) HttpStatusCode.BadRequest, "Expected the StatusCode to be a 'BadRequest'");
        }

        [Test]
        [Description("DeclareScope GET: When Org has explicit scopes return BadRequest")]
        public async Task DeclareScope_GET_When_PreviousInScope_Then_Return_BadRequest()
        {
            // Arrange
            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "DeclareScope");
            mockRouteData.Values.Add("Controller", "Organisation");

            var testUserId = 2;
            var testOrgId = 123;

            DateTime lastSnapshotDate = SectorTypes.Private.GetAccountingStartDate().AddYears(-1);

            var mockLastScope = new OrganisationScope
            {
                OrganisationId = testOrgId,
                Status = ScopeRowStatuses.Active,
                ScopeStatus = ScopeStatuses.InScope,
                SnapshotDate = lastSnapshotDate
            };


            var controller = UiTestHelper.GetController<OrganisationController>(
                testUserId,
                mockRouteData,
                MockUsers,
                MockOrganisations,
                MockUserOrganisations,
                mockLastScope);

            Mock<IScopeBusinessLogic> mockScopeBL = AutoFacExtensions.ResolveAsMock<IScopeBusinessLogic>(true);

            string encOrgId = Encryption.EncryptQuerystring(testOrgId.ToString());

            // Act
            IActionResult actionResult = await controller.DeclareScope(encOrgId);

            // Assert
            Expect(actionResult != null, "actionResult should not be null");

            // Assert
            var httpStatusResult = actionResult as HttpStatusViewResult;
            Assert.NotNull(httpStatusResult, "httpStatusResult should not be null");
            Assert.AreEqual(httpStatusResult.StatusCode, (int) HttpStatusCode.BadRequest, "Expected the StatusCode to be a 'BadRequest'");
        }

        [Test]
        [Description("DeclareScope GET: When Org has previous explicit scope return BadRequest")]
        public async Task DeclareScope_GET_When_PreviousOutOfScope_Then_Return_BadRequest()
        {
            // Arrange
            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "DeclareScope");
            mockRouteData.Values.Add("Controller", "Organisation");

            var testUserId = 2;
            var testOrgId = 123;

            DateTime lastSnapshotDate = SectorTypes.Private.GetAccountingStartDate().AddYears(-1);

            var mockLastScope = new OrganisationScope
            {
                OrganisationId = testOrgId,
                Status = ScopeRowStatuses.Active,
                ScopeStatus = ScopeStatuses.OutOfScope,
                SnapshotDate = lastSnapshotDate
            };

            var controller = UiTestHelper.GetController<OrganisationController>(
                testUserId,
                mockRouteData,
                MockUsers,
                MockOrganisations,
                MockUserOrganisations,
                mockLastScope);

            Mock<IScopeBusinessLogic> mockScopeBL = AutoFacExtensions.ResolveAsMock<IScopeBusinessLogic>(true);

            string encOrgId = Encryption.EncryptQuerystring(testOrgId.ToString());

            // Act

            IActionResult result = await controller.DeclareScope(encOrgId);
            var actionResult = result as HttpStatusViewResult;

            // Assert
            Assert.NotNull(actionResult, "httpStatusResult should not be null");
            Assert.AreEqual(actionResult.StatusCode, (int) HttpStatusCode.BadRequest, "Expected the StatusCode to be a 'BadRequest'");
        }

        [Test]
        [Description("DeclareScope GET: When User Not Assoc to Org Then Return Forbidden")]
        public async Task DeclareScope_GET_When_User_Not_Assoc_to_Org_Then_Return_Forbidden()
        {
            // Arrange
            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "DeclareScope");
            mockRouteData.Values.Add("Controller", "Organisation");

            var testUserId = 4;
            var testOrgId = 123;

            var controller = UiTestHelper.GetController<OrganisationController>(
                testUserId,
                mockRouteData,
                MockUsers,
                MockUserOrganisations);

            string encOrgId = Encryption.EncryptQuerystring(testOrgId.ToString());
            DateTime snapshotDate = SectorTypes.Private.GetAccountingStartDate();

            // Act
            IActionResult actionResult = await controller.DeclareScope(encOrgId);

            // Assert
            var httpStatusResult = actionResult as HttpStatusViewResult;
            Assert.NotNull(httpStatusResult, "httpStatusResult should not be null");
            Assert.AreEqual(httpStatusResult.StatusCode, (int) HttpStatusCode.Forbidden, "Expected the StatusCode to be a 'Forbidden'");
        }

        [Test]
        [Description("DeclareScope GET: When User Not completed registration Return Forbidden")]
        public async Task DeclareScope_GET_When_User_Not_FullyRegistered_Then_Return_Forbidden()
        {
            // Arrange
            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "DeclareScope");
            mockRouteData.Values.Add("Controller", "Organisation");

            var testUserId = 3;
            var testOrgId = 123;

            var controller = UiTestHelper.GetController<OrganisationController>(
                testUserId,
                mockRouteData,
                MockUsers,
                MockUserOrganisations);

            string encOrgId = Encryption.EncryptQuerystring(testOrgId.ToString());
            DateTime snapshotDate = SectorTypes.Private.GetAccountingStartDate();

            // Act
            IActionResult actionResult = await controller.DeclareScope(encOrgId);

            // Assert
            var httpStatusResult = actionResult as HttpStatusViewResult;
            Assert.NotNull(httpStatusResult, "httpStatusResult should not be null");
            Assert.AreEqual(httpStatusResult.StatusCode, (int) HttpStatusCode.Forbidden, "Expected the StatusCode to be a 'Forbidden'");
        }

        [Test]
        [Description("DeclareScope POST: When Id IsNullOrWhitespace Then Return BadRequest")]
        public async Task DeclareScope_POST_When_Id_IsNullOrWhitespace_Then_Return_BadRequestAsync()
        {
            // Arrange
            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "DeclareScope");
            mockRouteData.Values.Add("Controller", "Organisation");

            var controller = UiTestHelper.GetController<OrganisationController>(
                MockUsers[0].UserId,
                mockRouteData,
                MockUsers,
                MockUserOrganisations);
            var testOrgId = "";
            var model = new DeclareScopeModel();
            DateTime snapshotDate = SectorTypes.Private.GetAccountingStartDate();

            // Act
            IActionResult actionResult = await controller.DeclareScope(model, testOrgId);

            // Assert
            var httpStatusResult = actionResult as HttpStatusViewResult;
            Assert.NotNull(httpStatusResult, "httpStatusResult should not be null");
            Assert.AreEqual(httpStatusResult.StatusCode, (int) HttpStatusCode.BadRequest, "Expected the StatusCode to be a 'BadRequest'");
        }

        [Test]
        [Description("DeclareScope POST: When Model Last presumed year in scope save new scope")]
        public async Task DeclareScope_POST_When_Model_LastYearPresumedInScope_Then_SaveInScopeAsync()
        {
            // Arrange
            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "DeclareScope");
            mockRouteData.Values.Add("Controller", "Organisation");

            User currentUser = MockUsers.First(u => u.UserId == 2);
            var testUserId = 2;

            var testOrgId = 123;
            DateTime thisSnapshotDate = SectorTypes.Private.GetAccountingStartDate();
            DateTime lastSnapshotDate = thisSnapshotDate.AddYears(-1);

            var mockLastScope = new OrganisationScope
            {
                OrganisationId = testOrgId,
                Status = ScopeRowStatuses.Active,
                ScopeStatus = ScopeStatuses.PresumedInScope,
                SnapshotDate = lastSnapshotDate
            };

            var mockThisScope = new OrganisationScope
            {
                OrganisationId = testOrgId,
                Status = ScopeRowStatuses.Active,
                ScopeStatus = ScopeStatuses.OutOfScope,
                SnapshotDate = thisSnapshotDate
            };

            var controller = UiTestHelper.GetController<OrganisationController>(
                testUserId,
                mockRouteData,
                MockUsers,
                MockOrganisations,
                MockUserOrganisations,
                mockLastScope,
                mockThisScope);

            Mock<IScopeBusinessLogic> mockScopeBL = AutoFacExtensions.ResolveAsMock<IScopeBusinessLogic>(true);

            string encOrgId = Encryption.EncryptQuerystring(testOrgId.ToString());
            var model = new DeclareScopeModel {SnapshotDate = lastSnapshotDate, ScopeStatus = ScopeStatuses.InScope};

            // Act
            var actionResult = await controller.DeclareScope(model, encOrgId) as ViewResult;

            // Assert
            Expect(actionResult != null, "actionResult should not be null");

            // Assert
            Expect(actionResult.ViewName.EqualsI("ScopeDeclared"), "Expected ScopeDeclared view");
            Expect(actionResult.ViewData.ModelState.IsValid, "Expected Valid viewstate");
            IQueryable<OrganisationScope> orgScopes =
                controller.DataRepository.GetAll<OrganisationScope>().Where(os => os.OrganisationId == testOrgId);

            Expect(orgScopes.Count() == 3, "Expected three organisation scopes");

            //Check the new expicit scope is correct
            OrganisationScope newScope =
                orgScopes.FirstOrDefault(os => os.SnapshotDate == lastSnapshotDate && os.Status == ScopeRowStatuses.Active);
            Expect(newScope != null, "Submitted scope expected ");
            Expect(newScope.ContactEmailAddress == currentUser.EmailAddress, "Expected user email address to be saved with scope");
            Expect(newScope.ContactFirstname == currentUser.Firstname, "Expected user first name to be saved with scope");
            Expect(newScope.ContactLastname == currentUser.Lastname, "Expected user last name to be saved with scope");
            Expect(newScope.ScopeStatus == model.ScopeStatus.Value, "Expected new last years scope status to be same as model");

            //Check the old presumed scope is correct
            OrganisationScope oldScope =
                orgScopes.FirstOrDefault(os => os.SnapshotDate == lastSnapshotDate && os.Status == ScopeRowStatuses.Retired);
            Expect(oldScope != null, "Retired scope expected");
            Expect(oldScope.ScopeStatus == mockLastScope.ScopeStatus, "Expected old scope to be presumed scope");
            Expect(oldScope.ScopeStatusDate < newScope.ScopeStatusDate, "Expected oldscope status to be older than new scope");
            Expect(oldScope.OrganisationScopeId == mockLastScope.OrganisationScopeId, "Expected old scope to be same original");
        }

        [Test]
        [Description("DeclareScope POST: When User Not Assoc to Org Then Return Forbidden")]
        public async Task DeclareScope_POST_When_User_Not_Assoc_to_Org_Then_Return_ForbiddenAsync()
        {
            // Arrange
            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "DeclareScope");
            mockRouteData.Values.Add("Controller", "Organisation");

            var testUserId = 4;
            var testOrgId = 123;

            var controller = UiTestHelper.GetController<OrganisationController>(
                testUserId,
                mockRouteData,
                MockUsers,
                MockUserOrganisations);

            string encOrgId = Encryption.EncryptQuerystring(testOrgId.ToString());
            DateTime snapshotDate = SectorTypes.Private.GetAccountingStartDate();
            var model = new DeclareScopeModel();

            // Act
            IActionResult actionResult = await controller.DeclareScope(model, encOrgId);

            // Assert
            var httpStatusResult = actionResult as HttpStatusViewResult;
            Assert.NotNull(httpStatusResult, "httpStatusResult should not be null");
            Assert.AreEqual(httpStatusResult.StatusCode, (int) HttpStatusCode.Forbidden, "Expected the StatusCode to be a 'Forbidden'");
        }

        [Test]
        [Description("DeclareScope POST: When User Not completed registration Return Forbidden")]
        public async Task DeclareScope_POST_When_User_Not_FullyRegistered_Then_Return_ForbiddenAsync()
        {
            // Arrange
            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "DeclareScope");
            mockRouteData.Values.Add("Controller", "Organisation");

            var testUserId = 3;
            var testOrgId = 123;

            var controller = UiTestHelper.GetController<OrganisationController>(
                testUserId,
                mockRouteData,
                MockUsers,
                MockUserOrganisations);

            string encOrgId = Encryption.EncryptQuerystring(testOrgId.ToString());
            DateTime snapshotDate = SectorTypes.Private.GetAccountingStartDate();
            var model = new DeclareScopeModel();

            // Act
            IActionResult actionResult = await controller.DeclareScope(model, encOrgId);

            // Assert
            var httpStatusResult = actionResult as HttpStatusViewResult;
            Assert.NotNull(httpStatusResult, "httpStatusResult should not be null");
            Assert.AreEqual(httpStatusResult.StatusCode, (int) HttpStatusCode.Forbidden, "Expected the StatusCode to be a 'Forbidden'");
        }

        [Test]
        [Description("RemoveOrganisation GET: When No Other User Assoc and Impersonated by Admin Then Returns ConfirmRemove Action")]
        public void RemoveOrganisation_GET_When_No_Other_User_Assoc_and_Impersonated_By_Admin_Then_Returns_ConfirmRemove_Action()
        {
            // Arrange
            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "RemoveOrganisation");
            mockRouteData.Values.Add("Controller", "Organisation");

            var testUserId = 1;
            var testOrgId = 123;

            var controller = UiTestHelper.GetController<OrganisationController>(
                testUserId,
                mockRouteData,
                MockUsers,
                MockUserOrganisations);
            controller.Session["OriginalUser"] = "mockadmin1@geo.gov.uk";

            string encOrgId = Encryption.EncryptQuerystring(testOrgId.ToString());
            string encUserId = Encryption.EncryptQuerystring(testUserId.ToString());

            // Act
            IActionResult actionResult = controller.RemoveOrganisation(encOrgId, encUserId);

            // Assert
            var viewResult = actionResult as ViewResult;
            Assert.NotNull(viewResult, "viewResult should not be null");
            Assert.AreEqual("ConfirmRemove", viewResult.ViewName, "Expected the ViewName to be 'ConfirmRemove'");

            var model = viewResult.Model as RemoveOrganisationModel;
            Assert.NotNull(model, "viewResult.Model should not be null");
            Assert.AreEqual(model.EncOrganisationId, encOrgId, "Expected model.EncOrganisationId to match the id param");
        }

        [Test]
        [Description("RemoveOrganisation GET: When No Other User Assoc to Org Then Return Forbidden")]
        public void RemoveOrganisation_GET_When_No_Other_User_Assoc_to_Org_Then_Return_Forbidden()
        {
            // Arrange
            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "RemoveOrganisation");
            mockRouteData.Values.Add("Controller", "Organisation");

            var testUserId = 4;
            var testOrgId = 123;

            var controller = UiTestHelper.GetController<OrganisationController>(
                testUserId,
                mockRouteData,
                MockUsers,
                MockUserOrganisations);

            string encOrgId = Encryption.EncryptQuerystring(testOrgId.ToString());
            string encUserId = Encryption.EncryptQuerystring(testUserId.ToString());

            // Act
            IActionResult actionResult = controller.RemoveOrganisation(encOrgId, encUserId);

            // Assert
            var httpStatusResult = actionResult as HttpStatusViewResult;
            Assert.NotNull(httpStatusResult, "httpStatusResult should not be null");
            Assert.AreEqual(httpStatusResult.StatusCode, (int) HttpStatusCode.Forbidden, "Expected the StatusCode to be a 'Forbidden'");
        }

        [Test]
        [Description("RemoveOrganisation GET: When OrgId IsNullOrWhitespace Then Return BadRequest")]
        public void RemoveOrganisation_GET_When_OrgId_IsNullOrWhitespace_Then_Return_BadRequest()
        {
            // Arrange
            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "RemoveOrganisation");
            mockRouteData.Values.Add("Controller", "Organisation");

            var controller = UiTestHelper.GetController<OrganisationController>(
                MockUsers[0].UserId,
                mockRouteData,
                MockUsers,
                MockUserOrganisations);
            var testOrgId = "";
            var testUserId = "";

            // Act
            IActionResult actionResult = controller.RemoveOrganisation(testOrgId, testUserId);

            // Assert
            var httpStatusResult = actionResult as HttpStatusViewResult;
            Assert.NotNull(httpStatusResult, "httpStatusResult should not be null");
            Assert.AreEqual(httpStatusResult.StatusCode, (int) HttpStatusCode.BadRequest, "Expected the StatusCode to be a 'BadRequest'");
        }

        [Test]
        [Description("RemoveOrganisation GET: When user Org Not Assoc to Org Then Return Forbidden")]
        public void RemoveOrganisation_GET_When_User_Org_Not_Assoc_to_Then_Return_Forbidden()
        {
            // Arrange
            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "RemoveOrganisation");
            mockRouteData.Values.Add("Controller", "Organisation");

            var testUserId = 4;
            var testOrgId = 123; // not in user 4's UserOrganisations

            var controller = UiTestHelper.GetController<OrganisationController>(
                testUserId,
                mockRouteData,
                MockUsers,
                MockUserOrganisations);

            string encOrgId = Encryption.EncryptQuerystring(testOrgId.ToString());
            string encUserId = Encryption.EncryptQuerystring(testUserId.ToString());

            // Act
            IActionResult actionResult = controller.RemoveOrganisation(encOrgId, encUserId);

            // Assert
            var httpStatusResult = actionResult as HttpStatusViewResult;
            Assert.NotNull(httpStatusResult, "httpStatusResult should not be null");
            Assert.AreEqual(httpStatusResult.StatusCode, (int) HttpStatusCode.Forbidden, "Expected the StatusCode to be a 'Forbidden'");
        }

        [Test]
        [Description("RemoveOrganisation POST: When Removal Complete Which Creates Orphan Organisation Then Email GEO")]
        public async Task RemoveOrganisation_POST_When_Removal_Complete_Which_Creates_Orphan_Organisation_Then_Email_GEO()
        {
            // Arrange

            User user = CreateUser(1, "user1@test.com");
            Organisation organisation = createPrivateOrganisation(100, "Company1", 12345678);
            UserOrganisation userOrganisation = CreateUserOrganisation(organisation, user.UserId, VirtualDateTime.Now);

            var routeData = new RouteData();
            routeData.Values.Add("Action", "RemoveOrganisation");
            routeData.Values.Add("Controller", "Organisation");

            var testModel = new RemoveOrganisationModel
            {
                EncOrganisationId = Encryption.EncryptQuerystring(organisation.OrganisationId.ToString()),
                EncUserId = Encryption.EncryptQuerystring(user.UserId.ToString())
            };

            var controller = UiTestHelper.GetController<OrganisationController>(
                user.UserId,
                routeData,
                user,
                organisation,
                userOrganisation);

            organisation.OrganisationScopes.Add(new OrganisationScope
            {
                Status = ScopeRowStatuses.Active,
                ScopeStatus = ScopeStatuses.InScope,
                SnapshotDate = SectorTypes.Private.GetAccountingStartDate()
            });

            UiTestHelper.MockBackgroundJobsApi
                .Setup(q => q.AddEmailToQueue(It.IsAny<NotifyEmail>()));

            // Act
            var result = await controller.RemoveOrganisation(testModel) as RedirectToActionResult;

            // Assert
            UiTestHelper.MockBackgroundJobsApi.Verify(
                x => x.AddEmailToQueue(It.Is<NotifyEmail>(inst => inst.EmailAddress.Contains(user.EmailAddress))),
                Times.Once(),
                "Expected the current user's email address to be in the email send queue");
            UiTestHelper.MockBackgroundJobsApi.Verify(
                x => x.AddEmailToQueue(
                    It.Is<NotifyEmail>(inst => inst.TemplateId.Contains(EmailTemplates.RemovedUserFromOrganisationEmail))),
                Times.Exactly(1),
                $"Expected the correct templateId to be in the email send queue, expected {EmailTemplates.RemovedUserFromOrganisationEmail}");

            Assert.NotNull(result, "redirectResult should not be null");
            Assert.AreEqual("RemoveOrganisationCompleted", result.ActionName, "Expected the ViewName to be 'RemoveOrganisationCompleted'");
        }

        [Test]
        [Description("RemoveOrganisation POST: When Removal Complete Then Sends User Emails")]
        public async Task RemoveOrganisation_POST_When_User_Org_Removed_Then_Sends_User_Email()
        {
            // Arrange
            User user1 = CreateUser(1, "user1@test.com");
            User user2 = CreateUser(2, "user2@test.com");
            Organisation organisation = createPrivateOrganisation(100, "Company1", 12345678);
            UserOrganisation userOrganisation1 = CreateUserOrganisation(organisation, user1.UserId, VirtualDateTime.Now);
            UserOrganisation userOrganisation2 = CreateUserOrganisation(organisation, user2.UserId, VirtualDateTime.Now);

            var routeData = new RouteData();
            routeData.Values.Add("Action", "RemoveOrganisation");
            routeData.Values.Add("Controller", "Organisation");

            var testModel = new RemoveOrganisationModel
            {
                EncOrganisationId = Encryption.EncryptQuerystring(organisation.OrganisationId.ToString()),
                EncUserId = Encryption.EncryptQuerystring(user1.UserId.ToString())
            };

            var controller = UiTestHelper.GetController<OrganisationController>(
                user1.UserId,
                routeData,
                user1,
                user2,
                organisation,
                userOrganisation1,
                userOrganisation2);

            organisation.OrganisationScopes.Add(new OrganisationScope
            {
                Status = ScopeRowStatuses.Active,
                ScopeStatus = ScopeStatuses.InScope,
                SnapshotDate = SectorTypes.Private.GetAccountingStartDate()
            });

            UiTestHelper.MockBackgroundJobsApi
                .Setup(q => q.AddEmailToQueue(It.IsAny<NotifyEmail>()));

            // Act
            var result = await controller.RemoveOrganisation(testModel) as RedirectToActionResult;

            // Assert$
            UiTestHelper.MockBackgroundJobsApi.Verify(
                x => x.AddEmailToQueue(It.Is<NotifyEmail>(inst => inst.EmailAddress.Contains(user1.EmailAddress))),
                Times.Once(),
                "Expected the current user's email address to be in the email send queue");
            UiTestHelper.MockBackgroundJobsApi.Verify(
                x => x.AddEmailToQueue(
                    It.Is<NotifyEmail>(inst => inst.TemplateId.Contains(EmailTemplates.RemovedUserFromOrganisationEmail))),
                Times.Exactly(2),
                $"Expected the correct templateId to be in the email send queue, expected {EmailTemplates.RemovedUserFromOrganisationEmail}");
            UiTestHelper.MockBackgroundJobsApi.Verify(
                x => x.AddEmailToQueue(It.Is<NotifyEmail>(inst => inst.EmailAddress.Contains(user2.EmailAddress))),
                Times.Once(),
                "Expected the other user of the same organisation's email address to be in the email send queue");

            string geoDistributionList = Config.GetAppSetting("GEODistributionList");
            UiTestHelper.MockBackgroundJobsApi.Verify(
                x => x.AddEmailToQueue(It.Is<NotifyEmail>(inst => inst.TemplateId.Contains(EmailTemplates.SendGeoOrphanOrganisationEmail))),
                Times.Never(),
                $"Didnt expect the GEO Email addresses using {EmailTemplates.SendGeoOrphanOrganisationEmail} to be in the email send queue");
            UiTestHelper.MockBackgroundJobsApi.Verify(
                x => x.AddEmailToQueue(It.Is<NotifyEmail>(inst => inst.EmailAddress.Contains(geoDistributionList))),
                Times.Never(),
                "Didnt expect the GEO Email addresses to be in the email send queue");

            Assert.NotNull(result, "redirectResult should not be null");
            Assert.AreEqual("RemoveOrganisationCompleted", result.ActionName, "Expected the ViewName to be 'RemoveOrganisationCompleted'");
        }

    }

}

using System;
using System.Collections.Generic;
using System.Net;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Controllers;
using GenderPayGap.WebUI.ExternalServices;
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
        public void RemoveOrganisation_POST_When_Removal_Complete_Which_Creates_Orphan_Organisation_Then_Email_GEO()
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
            var result = controller.RemoveOrganisation(testModel) as RedirectToActionResult;

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
        public void RemoveOrganisation_POST_When_User_Org_Removed_Then_Sends_User_Email()
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
            var result = controller.RemoveOrganisation(testModel) as RedirectToActionResult;

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

            List<string> geoDistributionList = Global.GeoDistributionList;
            UiTestHelper.MockBackgroundJobsApi.Verify(
                x => x.AddEmailToQueue(It.Is<NotifyEmail>(inst => inst.TemplateId.Contains(EmailTemplates.SendGeoOrphanOrganisationEmail))),
                Times.Never(),
                $"Didnt expect the GEO Email addresses using {EmailTemplates.SendGeoOrphanOrganisationEmail} to be in the email send queue");
            UiTestHelper.MockBackgroundJobsApi.Verify(
                x => x.AddEmailToQueue(It.Is<NotifyEmail>(inst => geoDistributionList.Contains(inst.EmailAddress))),
                Times.Never(),
                "Didnt expect the GEO Email addresses to be in the email send queue");

            Assert.NotNull(result, "redirectResult should not be null");
            Assert.AreEqual("RemoveOrganisationCompleted", result.ActionName, "Expected the ViewName to be 'RemoveOrganisationCompleted'");
        }

    }

}

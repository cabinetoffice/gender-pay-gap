using System;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Tests.TestHelpers;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Controllers;
using GenderPayGap.WebUI.ExternalServices;
using GenderPayGap.WebUI.Models.Register;
using GenderPayGap.WebUI.Services;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Controllers.Registration
{
    [TestFixture]
    [SetCulture("en-GB")]
    public partial class RegisterControllerTests
    {

        private static UserOrganisation CreateUserOrganisation(Organisation org, long userId, DateTime? pinConfirmedDate)
        {
            return new UserOrganisation {
                Organisation = org, UserId = userId, PINConfirmedDate = pinConfirmedDate, Address = new OrganisationAddress()
            };
        }

        private static Organisation createPrivateOrganisation(long organisationId, string organisationName, int companyNumber)
        {
            return new Organisation {
                OrganisationId = organisationId,
                OrganisationName = organisationName,
                SectorType = SectorTypes.Private,
                Status = OrganisationStatuses.Active,
                CompanyNumber = companyNumber.ToString()
            };
        }

        private static Organisation createPublicOrganisation(long organisationId, string organisationName, int companyNumber)
        {
            return new Organisation {
                OrganisationId = organisationId,
                OrganisationName = organisationName,
                SectorType = SectorTypes.Public,
                Status = OrganisationStatuses.Active,
                CompanyNumber = companyNumber.ToString()
            };
        }

        private static User CreateUser(long userId, string emailAddress)
        {
            return new User {
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
        [Description("RegisterController POST: When User Added To Private Organisation Then Email Existing Users")]
        public async Task RegisterController_POST_When_User_Added_To_Private_Organisation_Then_Email_Existing_Users()
        {
            // Arrange
            var organisationId = 100;
            Organisation organisation = createPrivateOrganisation(organisationId, "Company1", 12345678);
            User existingUser1 = CreateUser(1, "user1@test.com");
            User existingUser2 = CreateUser(2, "user2@test.com");
            User newUser = CreateUser(3, "user3@test.com");
            UserOrganisation existingUserOrganisation1 = CreateUserOrganisation(organisation, existingUser1.UserId, VirtualDateTime.Now);
            UserOrganisation existingUserOrganisation2 = CreateUserOrganisation(organisation, existingUser2.UserId, VirtualDateTime.Now);
            UserOrganisation newUserOrganisation = CreateUserOrganisation(organisation, newUser.UserId, VirtualDateTime.Now);
            newUserOrganisation.PIN = "B5EC243";
            newUserOrganisation.PINConfirmedDate = null;
            newUserOrganisation.PINSentDate = VirtualDateTime.Now.AddDays(100000);

            var routeData = new RouteData();
            routeData.Values.Add("Action", "ActivateService");
            routeData.Values.Add("Controller", "Register");

            var testModel = new CompleteViewModel {PIN = "B5EC243", OrganisationId = organisationId};

            var controller = UiTestHelper.GetController<RegisterController>(
                newUser.UserId,
                routeData,
                organisation,
                existingUser1,
                existingUser2,
                newUser,
                existingUserOrganisation1,
                existingUserOrganisation2,
                newUserOrganisation);
            controller.ReportingOrganisationId = organisationId;

            UiTestHelper.MockBackgroundJobsApi
                .Setup(q => q.AddEmailToQueue(It.IsAny<NotifyEmail>()));

            // Act
            await controller.ActivateService(testModel);

            //ASSERT:
            UiTestHelper.MockBackgroundJobsApi.Verify(
                x => x.AddEmailToQueue(It.Is<NotifyEmail>(inst => inst.EmailAddress.Contains(existingUser1.EmailAddress))),
                Times.Once(),
                "Expected the existingUser1's email address to be in the email send queue");
            UiTestHelper.MockBackgroundJobsApi.Verify(
                x => x.AddEmailToQueue(It.Is<NotifyEmail>(inst => inst.EmailAddress.Contains(existingUser2.EmailAddress))),
                Times.Once(),
                "Expected the existingUser2's email address to be in the email send queue");
            UiTestHelper.MockBackgroundJobsApi.Verify(
                x => x.AddEmailToQueue(It.Is<NotifyEmail>(inst => inst.TemplateId.Contains(EmailTemplates.UserAddedToOrganisationEmail))),
                Times.Exactly(2),
                $"Expected the correct templateId to be in the email send queue, expected {EmailTemplates.UserAddedToOrganisationEmail}");
            UiTestHelper.MockBackgroundJobsApi.Verify(
                x => x.AddEmailToQueue(It.Is<NotifyEmail>(inst => inst.EmailAddress.Contains(newUser.EmailAddress))),
                Times.Never,
                "Do not expect new user's email address to be in the email send queue");
        }

        [Test]
        [Description("RegisterController POST: When User Added To Public Organisation Then Email Existing Users")]
        public async Task RegisterController_POST_When_User_Added_To_Public_Organisation_Then_Email_Existing_Users()
        {
            // Arrange
            var organisationId = 100;
            Organisation organisation = createPublicOrganisation(organisationId, "Company1", 12345678);
            User existingUser1 = CreateUser(1, "user1@test.com");
            User existingUser2 = CreateUser(2, "user2@test.com");
            User newUser = CreateUser(3, "user3@test.com");
            UserOrganisation existingUserOrganisation1 = CreateUserOrganisation(organisation, existingUser1.UserId, VirtualDateTime.Now);
            UserOrganisation existingUserOrganisation2 = CreateUserOrganisation(organisation, existingUser2.UserId, VirtualDateTime.Now);
            UserOrganisation newUserOrganisation = CreateUserOrganisation(organisation, newUser.UserId, VirtualDateTime.Now);
            newUserOrganisation.PINConfirmedDate = null;
            User govEqualitiesOfficeUser = UserHelper.GetGovEqualitiesOfficeUser();
            govEqualitiesOfficeUser.EmailVerifiedDate = VirtualDateTime.Now;

            var routeData = new RouteData();
            routeData.Values.Add("Action", "ReviewRequest");
            routeData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(
                govEqualitiesOfficeUser.UserId,
                routeData,
                organisation,
                existingUser1,
                existingUser2,
                newUser,
                existingUserOrganisation1,
                existingUserOrganisation2,
                newUserOrganisation,
                govEqualitiesOfficeUser);

            var testModel = new OrganisationViewModel {ReviewCode = newUserOrganisation.GetReviewCode()};
            controller.StashModel(testModel);

            UiTestHelper.MockBackgroundJobsApi
                .Setup(q => q.AddEmailToQueue(It.IsAny<NotifyEmail>()));

            // Act
            await controller.ReviewRequest(testModel, "approve");

            //ASSERT:
            UiTestHelper.MockBackgroundJobsApi.Verify(
                x => x.AddEmailToQueue(It.Is<NotifyEmail>(inst => inst.EmailAddress.Contains(existingUser1.EmailAddress))),
                Times.Once(),
                "Expected the existingUser1's email address to be in the email send queue");
            UiTestHelper.MockBackgroundJobsApi.Verify(
                x => x.AddEmailToQueue(It.Is<NotifyEmail>(inst => inst.EmailAddress.Contains(existingUser2.EmailAddress))),
                Times.Once(),
                "Expected the existingUser2's email address to be in the email send queue");
            UiTestHelper.MockBackgroundJobsApi.Verify(
                x => x.AddEmailToQueue(It.Is<NotifyEmail>(inst => inst.TemplateId.Contains(EmailTemplates.UserAddedToOrganisationEmail))),
                Times.Exactly(2),
                $"Expected the correct templateId to be in the email send queue, expected {EmailTemplates.UserAddedToOrganisationEmail}");
            UiTestHelper.MockBackgroundJobsApi.Verify(
                x => x.AddEmailToQueue(It.Is<NotifyEmail>(inst => inst.EmailAddress.Contains(newUser.EmailAddress))),
                Times.Never,
                "Do not expect new user's email address to be in the email send queue");
        }
    }
}

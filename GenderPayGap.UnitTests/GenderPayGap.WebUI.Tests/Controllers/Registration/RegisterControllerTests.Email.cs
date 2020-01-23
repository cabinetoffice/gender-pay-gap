using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Tests.TestHelpers;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Controllers;
using GenderPayGap.WebUI.Models.Register;
using GenderPayGap.WebUI.Services;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
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
        [Description("RegisterController POST: When User Added To Organisation Via Fast Track Then Email Existing Users")]
        public async Task RegisterController_POST_When_User_Added_To_Organisation_Via_Fast_Track_Then_Email_Existing_Users()
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

            newUserOrganisation.PINConfirmedDate = null;

            var employers = new PagedResult<EmployerRecord> {Results = new List<EmployerRecord>()};

            var employer = new EmployerRecord {
                OrganisationId = organisationId,
                OrganisationName = organisation.OrganisationName,
                CompanyNumber = organisation.CompanyNumber,
                SectorType = SectorTypes.Private,
                Address1 = "Address 1",
                Address2 = "Address 2",
                Address3 = "Address 3",
                City = "City",
                County = "County",
                Country = "UK",
                PostCode = "NW5 1TL",
                PoBox = "Po Box"
            };
            employers.Results.Add(employer);

            var routeData = new RouteData();
            routeData.Values.Add("Action", "ConfirmOrganisation");
            routeData.Values.Add("Controller", "Register");

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

            var testModel = new OrganisationViewModel {
                SectorType = SectorTypes.Private,
                OrganisationName = organisation.OrganisationName,
                Employers = employers,
                IsFastTrackAuthorised = true
            };
            controller.StashModel(testModel);

            var mockNotifyEmailQueue = new Mock<IQueue>();
            Program.MvcApplication.SendNotifyEmailQueue = mockNotifyEmailQueue.Object;
            mockNotifyEmailQueue
                .Setup(q => q.AddMessageAsync(It.IsAny<NotifyEmail>()));

            // Act
            await controller.ConfirmOrganisation(testModel, "confirm");

            //ASSERT:
            mockNotifyEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<NotifyEmail>(inst => inst.EmailAddress.Contains(existingUser1.EmailAddress))),
                Times.Once(),
                "Expected the existingUser1's email address to be in the email send queue");
            mockNotifyEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<NotifyEmail>(inst => inst.EmailAddress.Contains(existingUser2.EmailAddress))),
                Times.Once(),
                "Expected the existingUser2's email address to be in the email send queue");
            mockNotifyEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<NotifyEmail>(inst => inst.TemplateId.Contains(EmailTemplates.UserAddedToOrganisationEmail))),
                Times.Exactly(2),
                $"Expected the correct templateId to be in the email send queue, expected {EmailTemplates.UserAddedToOrganisationEmail}");
            mockNotifyEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<NotifyEmail>(inst => inst.EmailAddress.Contains(newUser.EmailAddress))),
                Times.Never,
                "Do not expect new user's email address to be in the email send queue");
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
            newUserOrganisation.PINSentDate = DateTime.Now.AddDays(100000);

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

            var mockNotifyEmailQueue = new Mock<IQueue>();
            Program.MvcApplication.SendNotifyEmailQueue = mockNotifyEmailQueue.Object;
            mockNotifyEmailQueue
                .Setup(q => q.AddMessageAsync(It.IsAny<NotifyEmail>()));

            // Act
            await controller.ActivateService(testModel);

            //ASSERT:
            mockNotifyEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<NotifyEmail>(inst => inst.EmailAddress.Contains(existingUser1.EmailAddress))),
                Times.Once(),
                "Expected the existingUser1's email address to be in the email send queue");
            mockNotifyEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<NotifyEmail>(inst => inst.EmailAddress.Contains(existingUser2.EmailAddress))),
                Times.Once(),
                "Expected the existingUser2's email address to be in the email send queue");
            mockNotifyEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<NotifyEmail>(inst => inst.TemplateId.Contains(EmailTemplates.UserAddedToOrganisationEmail))),
                Times.Exactly(2),
                $"Expected the correct templateId to be in the email send queue, expected {EmailTemplates.UserAddedToOrganisationEmail}");
            mockNotifyEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<NotifyEmail>(inst => inst.EmailAddress.Contains(newUser.EmailAddress))),
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

            var mockNotifyEmailQueue = new Mock<IQueue>();
            Program.MvcApplication.SendNotifyEmailQueue = mockNotifyEmailQueue.Object;
            mockNotifyEmailQueue
                .Setup(q => q.AddMessageAsync(It.IsAny<NotifyEmail>()));

            // Act
            await controller.ReviewRequest(testModel, "approve");

            //ASSERT:
            mockNotifyEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<NotifyEmail>(inst => inst.EmailAddress.Contains(existingUser1.EmailAddress))),
                Times.Once(),
                "Expected the existingUser1's email address to be in the email send queue");
            mockNotifyEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<NotifyEmail>(inst => inst.EmailAddress.Contains(existingUser2.EmailAddress))),
                Times.Once(),
                "Expected the existingUser2's email address to be in the email send queue");
            mockNotifyEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<NotifyEmail>(inst => inst.TemplateId.Contains(EmailTemplates.UserAddedToOrganisationEmail))),
                Times.Exactly(2),
                $"Expected the correct templateId to be in the email send queue, expected {EmailTemplates.UserAddedToOrganisationEmail}");
            mockNotifyEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<NotifyEmail>(inst => inst.EmailAddress.Contains(newUser.EmailAddress))),
                Times.Never,
                "Do not expect new user's email address to be in the email send queue");
        }

        [Test]
        [Description("Ensure the Step2 succeeds when all fields are good")]
        public async Task RegisterController_VerifyEmail_GET_RedirectResult_Success() //Registration complete
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var code = "abcdefg";
            var user = new User {
                UserId = 1,
                EmailAddress = "test@hotmail.com",
                EmailVerifiedDate = null,
                EmailVerifySendDate = VirtualDateTime.Now,
                EmailVerifyHash = Crypto.GetSHA512Checksum(code)
            };

            //Set the user up as if finished step1 which is email known etc but not sent
            var routeData = new RouteData();
            routeData.Values.Add("Action", "VerifyEmail");
            routeData.Values.Add("Controller", "Register");

            var model = new VerifyViewModel();

            //var controller = UiTestHelper.GetController<RegisterController>();
            var controller = UiTestHelper.GetController<RegisterController>(1, routeData, user);
            controller.AddMockQuerystringValue("code", code);

            //ACT:
            //2.Run and get the result of the test
            var result = await controller.VerifyEmail(code) as RedirectToActionResult;

            //ASSERT:
            Assert.NotNull(result, "Expected RedirectToActionResult");

            //Check the user is return the confirmation view
            Assert.That(result.ActionName == "EmailConfirmed", "Email is has not been confirmed!");

            //Check the user verification is now marked as sent
            Assert.NotNull(user.EmailVerifiedDate, "Email is has not been confirmed!");

            //Check a verification has been set against user 
            Assert.That(user.Status == UserStatuses.Active, "Email is has not been confirmed!");
        }

        [Test]
        [Description("Ensure the Step2 user verification succeeds")]
        public async Task RegisterController_VerifyEmail_GET_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User {UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = null};

            var organisation = new Organisation {OrganisationId = 1};
            var userOrganisation = new UserOrganisation {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = 1,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "0"
            };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "VerifyEmail");
            routeData.Values.Add("Controller", "Register");

            //ARRANGE:
            //1.Arrange the test setup variables
            var model = new VerifyViewModel();
            model.EmailAddress = "test@hotmail.com";
            model.Resend = false;

            //Set model as if email

            // model.Sent = true;
            model.UserId = 1;

            // model.WrongCode = false;

            //var controller = UiTestHelper.GetController<RegisterController>();
            var controller = UiTestHelper.GetController<RegisterController>(1, routeData, user /*, userOrganisation*/);
            controller.Bind(model);

            //ACT:
            //2.Run and get the result of the test

            var result = await controller.VerifyEmail(model) as ViewResult;
            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "VerifyEmail", "Incorrect view returned,Verification is incomplete");
            Assert.That(
                result.Model != null && result.Model.GetType() == typeof(VerifyViewModel),
                "Expected VerifyViewModel or Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");
        }

        [Test]
        [Description("Ensure the Step2 succeeds when is verified and an email is sent")]
        public async Task RegisterController_VerifyEmail_GET_ViewResult_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User {
                UserId = 1,
                EmailAddress = "test@hotmail.com",
                EmailVerifiedDate = null,
                EmailVerifySendDate = null,
                Status = UserStatuses.New
            };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "VerifyEmail");
            routeData.Values.Add("Controller", "Register");

            //simulate a model to stash
            var model = new RegisterViewModel();
            model.EmailAddress = "test@hotmail.com";
            model.ConfirmEmailAddress = "test@hotmail.com";
            model.FirstName = "Kingsley";
            model.LastName = "Eweka";
            model.JobTitle = "Dev";
            model.Password = "K1ngsl3y3w3ka";
            model.ConfirmPassword = "K1ngsl3y3w3ka";

            var controller = UiTestHelper.GetController<RegisterController>(1, routeData, user);
            controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = await controller.VerifyEmail() as ViewResult;

            var resultModel = result.Model as VerifyViewModel;

            result.ViewData.ModelState.Clear();

            //ASSERT:
            //Ensure confirmation view is returned
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == "VerifyEmail", "Incorrect view returned");

            //Ensure the model is not null and it is correct
            Assert.NotNull(result.Model as VerifyViewModel, "Expected VerifyViewModel");
            Assert.That(result.ViewData.ModelState.IsValid, " Model is not valid");
            //Assert.AreEqual(result.ViewData.ModelState.IsValidField("EmailAddress"), "Email is not a match or is invalid");

            //ensure user is marked as verified
            Assert.AreEqual(resultModel.Sent, true, "Expected VerifyViewModel");

            //Check the user has a verified send date 
            Assert.NotNull(user.EmailVerifySendDate, "Email is has not been confirmed!");
        }

        [Test]
        [Ignore("Not implemented")]
        [Description("RegisterController.VerifyEmail GET: When PendingFastrack Setting load and clear PendingFastrack")]
        public void RegisterController_VerifyEmail_GET_When_PendingFastrackUserSetting_Then_SetPendingFastrack()
        {
            throw new NotImplementedException();
        }

    }
}

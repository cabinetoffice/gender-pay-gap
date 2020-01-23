using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.Tests.Common.Classes;
using GenderPayGap.Tests.Common.TestHelpers;
using GenderPayGap.Tests.TestHelpers;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Controllers;
using GenderPayGap.WebUI.Models.Register;
using GenderPayGap.WebUI.Tests;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.Tests
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class RegistrationTests : AssertionHelper
    {

        #region ChooseOrganisation

        [Test]
        [Description("Ensure the Choose Organisation form is returned for the current user to choose an organisation")]
        public void ChooseOrganisation_Get_Success()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User {UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now};

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.ChooseOrganisation));
            routeData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user);
            //controller.StashModel(model);

            var orgModel = new OrganisationViewModel {ManualRegistration = false};
            controller.StashModel(orgModel);

            //ACT:
            var result = controller.ChooseOrganisation() as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == nameof(RegisterController.ChooseOrganisation), "Incorrect view returned");
            Assert.NotNull(result.Model as OrganisationViewModel, "Expected OrganisationViewModel");
            Assert.That(result.Model.GetType() == typeof(OrganisationViewModel), "Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");
        }

        #endregion

        #region ActivateService

        [Test]
        [Description("Check registration completes successfully when correct pin entered ")]
        public async Task ActivateService_CorrectPIN_ServiceActivated()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User {UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now};
            var org = new Organisation {OrganisationId = 1, SectorType = SectorTypes.Private, Status = OrganisationStatuses.Active};
            var address = new OrganisationAddress {AddressId = 1, OrganisationId = 1, Organisation = org, Status = AddressStatuses.Pending};
            var pin = "ASDFG";
            var userOrg = new UserOrganisation {
                UserId = 1,
                OrganisationId = 1,
                PINSentDate = VirtualDateTime.Now,
                PIN = pin,
                AddressId = address.AddressId,
                Address = address
            };

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.ActivateService));
            routeData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user, org, address, userOrg);
            controller.ReportingOrganisationId = org.OrganisationId;

            var model = new CompleteViewModel {PIN = pin};

            //ACT:
            var result = await controller.ActivateService(model) as RedirectToActionResult;

            //ASSERT:
            Expect(result != null, "Expected RedirectToActionResult");
            Expect(result.ActionName == "ServiceActivated", "Expected redirect to ServiceActivated");
            Expect(userOrg.PINConfirmedDate > DateTime.MinValue);
            Expect(userOrg.Organisation.Status == OrganisationStatuses.Active);
            Expect(userOrg.Organisation.LatestAddress.AddressId == address.AddressId);
            Expect(address.Status == AddressStatuses.Active);
        }

        #endregion

        #region ConfirmOrganisation

        [Test(Author = "Oscar Lagatta")]
        [Description("Confirm Organisation Post When Command is Cancel Fastrack")]
        public async Task ConfirmOrganisation_Post_When_Command_Is_Cancel_FasttrackAsync()
        {
            //ConfirmOrganisation(OrganisationViewModel model, string command = null)
            // Arrange
            User mockUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            Organisation mockOrg = OrganisationHelper.GetPublicOrganisation();
            UserOrganisation UserOrg = UserOrganisationHelper.LinkUserWithOrganisation(mockUser, mockOrg);

            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "ServiceActivated");
            mockRouteData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(-1, mockRouteData, mockUser, mockOrg, UserOrg);
            controller.ReportingOrganisationId = mockOrg.OrganisationId;

            var testUri = new Uri("https://localhost/register/activate-service");

            controller.AddMockUriHelper(testUri.ToString(), "ActivateService");

            //Mock the Referrer
            controller.Request.Headers["Referer"] = testUri.ToString();

            // ACT
            var result = await controller.ConfirmOrganisation(new OrganisationViewModel(), "CancelFasttrack") as RedirectToActionResult;

            // ASSERT
            Assert.NotNull(result);
            Assert.That(result.ActionName == "ManageOrganisations");
        }

        #endregion

        #region Test user is enrolled 

        [Test]
        [Description("Ensure IdentityNotMappedException thrown when bad user Id")]
        public void AboutYou_IdentityNotMapped_ThrowException()
        {
            // Arrange
            var controller = UiTestHelper.GetController<RegisterController>(2);

            // Act

            // Assert
            Assert.ThrowsAsync<IdentityNotMappedException>(() => controller.AboutYou(), "Expected IdentityNotMappedException");
        }

        [Test]
        [Description("Ensure IdentityNotMapped action returns error view")]
        public void CustomError_IdentityNotMapped_ReturnsView()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ErrorController>(1);

            // Act
            var result = controller.Default(1000) as ViewResult;
            var model = result.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == "CustomError", "Incorrect view returned");
            Assert.NotNull(model, "Expected ErrorViewModel");
            Assert.That(model.ErrorCode == 1000, "Invalid error code");
            Assert.That(model.Title == "You’re not signed in properly", "Invalid error title");
        }


        [Test]
        [Description("Ensure registered users attempting to reregistered when no verify email is sent is prompted to resend")]
        public async Task AboutYou_NoVerifyUserReRegistering_ErrorWithStep2LinkAsync()
        {
            // Arrange
            var user = new User {UserId = 1};

            var routeData = new RouteData();
            routeData.Values.Add("action", "AboutYou");
            routeData.Values.Add("controller", "register");


            var controller = UiTestHelper.GetController<RegisterController>(1, routeData, user);

            // Act
            var result = await controller.AboutYou() as ViewResult;
            var model = result.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == "CustomError", "Incorrect view returned");
            Assert.NotNull(model, "Expected ErrorViewModel");
            Assert.That(model.ErrorCode == 1100, "Invalid error code");
            Assert.That(model.Title == "You haven’t verified your email address yet", "Invalid error title");
        }

        [Test]
        [Description("Ensure registered users attempting to reregistered when old verify email is sent is prompted to resend")]
        public async Task AboutYou_OldVerifyUserReRegistering_ShowErrorWithReverifyLinkAsync()
        {
            // Arrange
            var user = new User {UserId = 1, EmailVerifySendDate = VirtualDateTime.Now.AddDays(-7)};

            var routeData = new RouteData();
            routeData.Values.Add("action", "AboutYou");
            routeData.Values.Add("controller", "register");

            var controller = UiTestHelper.GetController<RegisterController>(1, routeData, user);

            // Act
            var result = await controller.AboutYou() as ViewResult;
            var model = result.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == "CustomError", "Incorrect view returned");
            Assert.NotNull(model, "Expected ErrorViewModel");
            Assert.That(model.ErrorCode == 1101, "Invalid error code");
            Assert.That(model.Title == "You haven’t verified your email address yet", "Invalid error title");
        }

        [Test]
        [Description(
            "Ensure registered users attempting to reregistered when verify email is recently sent is prompted to check email bit not allowed to resend")]
        public async Task AboutYou_RecentVerifyUserReRegistering_ShowErrorWithNoReverifyLinkAsync()
        {
            // Arrange
            var user = new User {UserId = 1, EmailVerifySendDate = VirtualDateTime.Now.AddHours(-1)};

            var routeData = new RouteData();
            routeData.Values.Add("action", "AboutYou");
            routeData.Values.Add("controller", "register");


            var controller = UiTestHelper.GetController<RegisterController>(1, routeData, user);

            // Act
            var result = await controller.AboutYou() as ViewResult;
            var model = result.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == "CustomError", "Incorrect view returned");
            Assert.NotNull(model, "Expected ErrorViewModel");
            Assert.That(model.ErrorCode == 1103, "Invalid error code");
            Assert.That(model.Title == "You haven’t verified your email address yet", "Invalid error title");
        }

        [Test]
        [Description("Ensure users who have verified email but not setup an organisation are sent to manage organisations page")]
        public async Task AboutYou_VerifiedUserReRegisteringNoOrg_ShowErrorWithContinueRegisterLinkAsync()
        {
            // Arrange
            var user = new User {
                UserId = 1,
                EmailAddress = "test1@domain.com",
                EmailVerifySendDate = VirtualDateTime.Now.AddDays(-3),
                EmailVerifiedDate = VirtualDateTime.Now
            };

            var routeData = new RouteData();
            routeData.Values.Add("action", "AboutYou");
            routeData.Values.Add("controller", "register");


            var controller = UiTestHelper.GetController<RegisterController>(1, routeData, user);

            // Act
            var result = await controller.AboutYou() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result, "Expected RedirectToActionResult");
            Assert.That(result.ActionName == "ManageOrganisations", "Expected redirect to ManageOrganisations");
        }

        [Test]
        [Description("Ensure users attempting to reregister is redirected to manage organisations")]
        public async Task AboutYou_UserReRegisterNoPinSent_RedirectToManageOrgsAsync()
        {
            // Arrange
            var user = new User {
                UserId = 1,
                EmailAddress = "test1@domain.com",
                EmailVerifySendDate = VirtualDateTime.Now,
                EmailVerifiedDate = VirtualDateTime.Now
            };
            var userOrg = new UserOrganisation {UserId = 1, OrganisationId = 1};

            var routeData = new RouteData();
            routeData.Values.Add("action", "AboutYou");
            routeData.Values.Add("controller", "register");

            var controller = UiTestHelper.GetController<RegisterController>(1, routeData, user, userOrg);

            // Act
            var result = await controller.AboutYou() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result, "Expected RedirectToActionResult");
            Assert.That(result.ActionName == "ManageOrganisations", "Incorrect redirect returned");
        }

        [Test]
        [Description("Ensure users attempting to reregister when old PIN is sent is prompted to resend")]
        public async Task AboutYou_OldPINReRegister_ShowErrorWithResendLinkAsync()
        {
            // Arrange
            var user = new User {
                UserId = 1,
                EmailAddress = "test1@domain.com",
                EmailVerifySendDate = VirtualDateTime.Now,
                EmailVerifiedDate = VirtualDateTime.Now
            };
            var org = new Organisation {OrganisationId = 1, SectorType = SectorTypes.Private};
            var userOrg = new UserOrganisation {
                UserId = 1,
                Organisation = org,
                OrganisationId = 1,
                PINSentDate = VirtualDateTime.Now.AddDays(0 - Global.PinInPostExpiryDays - 1)
            };

            var routeData = new RouteData();
            routeData.Values.Add("action", "AboutYou");
            routeData.Values.Add("controller", "register");

            var controller = UiTestHelper.GetController<RegisterController>(1, routeData, org, user, userOrg);
            controller.ReportingOrganisationId = userOrg.OrganisationId;
            // Act
            var result = await controller.AboutYou() as ViewResult;
            var model = result.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == "CustomError", "Incorrect view returned");
            Assert.NotNull(model, "Expected ErrorViewModel");
            Assert.That(model.ErrorCode == 1106, "Invalid error code");
            Assert.That(model.Title == "Your PIN has expired", "Invalid error title");
        }

        [Test]
        [Description("Ensure users attempting to reregister when verify email is recently sent is redirected to ActivateService")]
        public async Task AboutYou_RecentPINReRegister_ActivateServiceAsync()
        {
            // Arrange
            var user = new User {
                UserId = 1,
                EmailAddress = "test1@domain.com",
                EmailVerifySendDate = VirtualDateTime.Now,
                EmailVerifiedDate = VirtualDateTime.Now
            };
            var org = new Organisation {OrganisationId = 1, SectorType = SectorTypes.Private};
            var userOrg = new UserOrganisation {
                UserId = 1, Organisation = org, OrganisationId = 1, PINSentDate = VirtualDateTime.Now.AddHours(-1)
            };

            var routeData = new RouteData();
            routeData.Values.Add("action", "AboutYou");
            routeData.Values.Add("controller", "register");


            var controller = UiTestHelper.GetController<RegisterController>(1, routeData, org, user, userOrg);
            controller.ReportingOrganisationId = userOrg.OrganisationId;
            // Act
            var result = await controller.AboutYou() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result, "Expected RedirectToActionResult");
            Assert.That(result.ActionName == "ActivateService", "Expected redirect to activate service");
        }

        [Test]
        [Description("Ensure registered users attempting to reregister are redirected to manage orgs")]
        public async Task AboutYou_ReRegister_RedirectToManageOrgsAsync()
        {
            // Arrange
            var user = new User {
                UserId = 1,
                EmailAddress = "test1@domain.com",
                EmailVerifySendDate = VirtualDateTime.Now,
                EmailVerifiedDate = VirtualDateTime.Now
            };
            var org = new Organisation {OrganisationId = 1, SectorType = SectorTypes.Private};
            var userOrg = new UserOrganisation {
                UserId = 1,
                Organisation = org,
                OrganisationId = 1,
                PINSentDate = VirtualDateTime.Now.AddHours(-1),
                PINConfirmedDate = VirtualDateTime.Now
            };

            var routeData = new RouteData();
            routeData.Values.Add("action", "AboutYou");
            routeData.Values.Add("controller", "register");

            var controller = UiTestHelper.GetController<RegisterController>(1, routeData, org, user, userOrg);

            // Act
            var result = await controller.AboutYou() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result, "Expected RedirectToActionResult");
            Assert.That(result.ActionName == "ManageOrganisations", "Expected redirect to ManageOrganisations");
        }

        #endregion

        #region Test start of enrollment

        [Test]
        [Description("Ensure the AboutYou action returns an empty form when there is no user logged in")]
        public async Task AboutYou_NotLoggedIn_ShowEmptyFormAsync()
        {
            var routeData = new RouteData();
            routeData.Values.Add("action", "AboutYou");
            routeData.Values.Add("controller", "register");

            // Arrange
            var controller = UiTestHelper.GetController<RegisterController>(0, routeData);

            // Act
            var result = await controller.AboutYou() as ViewResult;
            var model = result.Model as RegisterViewModel;

            // Assert
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == "AboutYou", "Incorrect view returned");
            Assert.NotNull(model, "Expected RegisterViewModel");
            Assert.Null(model.EmailAddress, "Expected empty email address");
            Assert.Null(model.FirstName, "Expected empty first name");
            Assert.Null(model.LastName, "Expected empty last name");
            Assert.Null(model.JobTitle, "Expected empty job title");
        }

        [Test]
        [Description("Ensure the AboutYou fails when any field is empty")]
        public async Task AboutYou_EmptyFields_ShowAllErrorsAsync()
        {
            // Arrange
            var model = new RegisterViewModel();
            model.EmailAddress = "";
            model.ConfirmEmailAddress = " ";
            model.FirstName = "";
            model.LastName = "";
            model.JobTitle = "";
            model.Password = "";
            model.ConfirmPassword = " ";


            var controller = UiTestHelper.GetController<RegisterController>();
            controller.Bind(model);

            // Act
            var result = await controller.AboutYou(model) as ViewResult;
            // Assert
            Assert.Multiple(
                () => {
                    Assert.NotNull(result, "Expected ViewResult");
                    Assert.That(result.ViewName == "AboutYou", "Incorrect view returned");
                    Assert.NotNull(result.Model as RegisterViewModel, "Expected RegisterViewModel");
                    Assert.AreEqual(result.ViewData.ModelState.IsValidField("EmailAddress"), false, "Expected email failure");
                    Assert.AreEqual(
                        result.ViewData.ModelState.IsValidField("ConfirmEmailAddress"),
                        false,
                        "Expected confirm email failure");
                    Assert.AreEqual(result.ViewData.ModelState.IsValidField("FirstName"), false, "Expected first name failure");
                    Assert.AreEqual(result.ViewData.ModelState.IsValidField("LastName"), false, "Expected last name failure");
                    Assert.AreEqual(result.ViewData.ModelState.IsValidField("JobTitle"), false, "Expected jobtitle failure");
                    Assert.AreEqual(result.ViewData.ModelState.IsValidField("Password"), false, "Expected password failure");
                    Assert.AreEqual(result.ViewData.ModelState.IsValidField("ConfirmPassword"), false, "Expected confirm password failure");
                });
        }

        [Test]
        [Description("Ensure the AboutYou succeeds when all fields are good")]
        public async Task AboutYou_GoodFields_NoErrorAsync()
        {
            //ARRANGE:
            var routeData = new RouteData();
            routeData.Values.Add("action", "AboutYou");
            routeData.Values.Add("controller", "register");

            var expectedModel = new RegisterViewModel();
            expectedModel.EmailAddress = "test@hotmail.com";
            expectedModel.ConfirmEmailAddress = "test@hotmail.com";
            expectedModel.FirstName = "TestFirstName";
            expectedModel.LastName = "TestLastName";
            expectedModel.JobTitle = "TestJobTitle";
            expectedModel.Password = "P@ssword1!";
            expectedModel.ConfirmPassword = "P@ssword1!";

            var controller = UiTestHelper.GetController<RegisterController>(0, routeData);
            controller.Bind(expectedModel);

            //ACT:
            //Get the result of the test
            var result = await controller.AboutYou(expectedModel) as RedirectToActionResult;

            //check that the result is not null
            Assert.NotNull(result, "Expected RedirectToActionResult");

            //check that the redirection went to the right url step.
            Assert.That(result.ActionName == "VerifyEmail", "Redirected to the wrong view");

            //check that the model stashed preserved with the redirect is equal to what is expected the Arrange values here
            //Retreive the model stashed preserved with the redirect.
            var actualModel = controller.UnstashModel<RegisterViewModel>();

            //Check that the unstashed model is not null
            Assert.NotNull(expectedModel, "Expected RegisterViewModel");

            //ASSERT:
            // Verify the values from the result that was stashed is equal tothat of the Arrange values here
            actualModel.Compare(expectedModel);
        }

        [Test]
        [Description("Ensure the AboutYou fails when email and confirmation mismatch")]
        public async Task AboutYou_EmailMismatch_ShowErrorAsync()
        {
            // Arrange
            var model = new RegisterViewModel();
            model.EmailAddress = "test@hotmail.com";
            model.ConfirmEmailAddress = "test1@hotmail.com";
            model.FirstName = "TestFirstName";
            model.LastName = "TestLastName";
            model.JobTitle = "TestJobTitle";
            model.Password = "P@ssword11!";
            model.ConfirmPassword = "P@ssword11!";

            var controller = UiTestHelper.GetController<RegisterController>();
            controller.Bind(model);

            // Act
            var result = await controller.AboutYou(model) as ViewResult;

            // Assert
            Assert.That(!result.ViewData.ModelState.IsValid, "Email compare should have failed");
        }

        [Test]
        [Description("Ensure the AboutYou fails when password and confirmation dont match")]
        public async Task AboutYou_PasswordMismatch_ShowErrorAsync()
        {
            // Arrange
            var model = new RegisterViewModel();
            model.EmailAddress = "test@hotmail.com";
            model.ConfirmEmailAddress = "test@hotmail.com";
            model.FirstName = "TestFirstName";
            model.LastName = "TestLastName";
            model.JobTitle = "TestJobTitle";
            model.Password = "P@ssword1!";
            model.ConfirmPassword = "P@ssword11!";

            var controller = UiTestHelper.GetController<RegisterController>();
            controller.Bind(model);

            // Act
            var result = await controller.AboutYou(model) as ViewResult;

            // Assert
            Assert.That(!result.ViewData.ModelState.IsValid, "Password compare should have failed");
        }

        [Test]
        [Description("Ensure the AboutYou fails when password is too short")]
        public async Task AboutYou_ShortPassword_ShowErrorAsync()
        {
            // Arrange
            var model = new RegisterViewModel();
            model.EmailAddress = "test@hotmail.com";
            model.ConfirmEmailAddress = "test@hotmail.com";
            model.FirstName = "TestFirstName";
            model.LastName = "TestLastName";
            model.JobTitle = "TestJobTitle";
            model.Password = "Passwor";
            model.ConfirmPassword = "Passwor";

            var controller = UiTestHelper.GetController<RegisterController>();
            controller.Bind(model);

            // Act
            var result = await controller.AboutYou(model) as ViewResult;

            // Assert
            Assert.That(!result.ViewData.ModelState.IsValid, "Short password compare should have failed");
        }

        [Test]
        [Description("Ensure the AboutYou fails when password contains 'password'")]
        public async Task AboutYou_PasswordContainsPassword_ShowErrorAsync()
        {
            // Arrange
            var model = new RegisterViewModel();
            model.EmailAddress = "test@hotmail.com";
            model.ConfirmEmailAddress = "test@hotmail.com";
            model.FirstName = "TestFirstName";
            model.LastName = "TestLastName";
            model.JobTitle = "TestJobTitle";
            model.Password = "Password1";
            model.ConfirmPassword = "Password1";

            var controller = UiTestHelper.GetController<RegisterController>();
            controller.Bind(model);

            // Act
            var result = await controller.AboutYou(model) as ViewResult;

            // Assert
            Assert.That(!result.ViewData.ModelState.IsValid, "Password containing 'password' should have failed");
        }

        [Test]
        [Description("Ensure the AboutYou fails when a bad password expression is entered")]
        public async Task AboutYou_BadPasswordExpression_ShowErrorAsync()
        {
            // Arrange
            var model = new RegisterViewModel();
            model.EmailAddress = "test@hotmail.com";
            model.ConfirmEmailAddress = "test@hotmail.com";
            model.FirstName = "TestFirstName";
            model.LastName = "TestLastName";
            model.JobTitle = "TestJobTitle";
            model.Password = "p@ssword";
            model.ConfirmPassword = "P@ssword";

            var controller = UiTestHelper.GetController<RegisterController>();
            controller.Bind(model);

            // Act
            var result = await controller.AboutYou(model) as ViewResult;

            // Assert
            Assert.That(!result.ViewData.ModelState.IsValid, "Password expression should have failed");
        }

        #endregion

        #region Test CheckUserRegisteredOk - about you

        [Test]
        [Description("Ensure AboutYou redirects to manage organisations when user has no orgs")]
        public async Task AboutYou_Get_RegisteredUserToSelectOrgTypeAsync()
        {
            //ARRANGE:
            //create a user who does not exist in the db
            var user = new User {
                UserId = 1, Status = UserStatuses.Active, EmailVerifiedDate = VirtualDateTime.Now, EmailAddress = "test1@domain.com"
            };

            var routeData = new RouteData();
            routeData.Values.Add("action", "AboutYou");
            routeData.Values.Add("Controller", "register");

            //Stash an object to pass in for  this.ClearStash()
            var controller = UiTestHelper.GetController<RegisterController>(1, routeData, user);

            //ACT:
            var result = await controller.AboutYou() as RedirectToActionResult;

            //ASSERT:
            Assert.NotNull(result, "Expected RedirectResult");
            Assert.AreEqual(nameof(OrganisationController.ManageOrganisations), result.ActionName);
        }

        [Test]
        [Description("Ensure AboutYou redirects to manage organisations page when ReportingOrganisationId is not set")]
        public async Task AboutYou_Get_RegisteredUserToManageOrgsAsync()
        {
            //ARRANGE:
            //create a user who does not exist in the db
            var user = new User {
                UserId = 1, Status = UserStatuses.Active, EmailVerifiedDate = VirtualDateTime.Now, EmailAddress = "test1@domain.com"
            };
            var org = new Organisation {OrganisationId = 1};
            var userOrg = new UserOrganisation {UserId = 1, OrganisationId = 1, PINConfirmedDate = VirtualDateTime.Now};

            var routeData = new RouteData();
            routeData.Values.Add("action", "AboutYou");
            routeData.Values.Add("Controller", "register");

            //Stash an object to pass in for  this.ClearStash()
            var controller = UiTestHelper.GetController<RegisterController>(1, routeData, user, org, userOrg);

            //ACT:
            var result = await controller.AboutYou() as RedirectToActionResult;

            //ASSERT:
            Assert.NotNull(result, "Expected RedirectToActionResult");
            Assert.AreEqual(result.ControllerName, "Organisation", "Expected action");
            Assert.AreEqual(result.ActionName, "ManageOrganisations", "Expected action");
        }

        #endregion

        #region AboutYou

        [Test]
        [Description("Ensure that a new registration form is returned for a user to register")]
        public async Task AboutYou_Get_NewRegistration_SuccessAsync()
        {
            //ARRANGE:
            //create a user who does not exist in the db
            var user = new User {UserId = 0};

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "AboutYou");
            routeData.Values.Add("Controller", "Register");

            //Stash an object to pass in for this.ClearStash()
            //var model = new RegisterViewModel();
            var controller = UiTestHelper.GetController<RegisterController>(0, routeData);

            //controller.StashModel(model);

            //ACT:
            var result = await controller.AboutYou() as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "AboutYou", "Incorrect view returned");
            Assert.That(
                result.Model != null && result.Model.GetType() == typeof(RegisterViewModel),
                "Expected RegisterViewModel or Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");
        }

        [Test]
        [Description(
            "Ensure that the new registration form filled with correct values is sent successfully when all fields values are valid")]
        public async Task AboutYou_Post_SuccessAsync()
        {
            //ARRANGE:
            //create a user who does not exist in the db
            //var user = new User() { UserId = 0 };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "AboutYou");
            routeData.Values.Add("Controller", "Register");


            //1.Arrange the test setup variables
            var expectedModel = new RegisterViewModel {
                EmailAddress = "test@hotmail.com",
                ConfirmEmailAddress = "test@hotmail.com",
                FirstName = "Kingsley",
                LastName = "Eweka",
                JobTitle = "Dev",
                Password = "K1ngsl3y3w3ka",
                ConfirmPassword = "K1ngsl3y3w3ka"
            };

            var controller = UiTestHelper.GetController<RegisterController>();
            controller.Bind(expectedModel);

            //ACT:
            //2.Run and get the result of the test
            var result = await controller.AboutYou(expectedModel) as RedirectToActionResult;

            //ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToActionResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.ActionName == "VerifyEmail", "Expected a RedirectToActionResult to VerifyEmail");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var actualModel = controller.UnstashModel<RegisterViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(expectedModel, "Expected RegisterViewModel");

            //7.Verify the values from the result that was stashed matches that of the Arrange values here
            actualModel.Compare(expectedModel);
        }

        #endregion

        #region VerifyEmail

        [Test]
        [Description("Ensure the Step2 succeeds when is verified and an email is sent")]
        public async Task VerifyEmail_Get_ViewResult_SuccessAsync()
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

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            var resultModel = result.Model as VerifyViewModel;

            //Ensure confirmation view is returned
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
        [Description("Ensure the Step2 succeeds when all fields are good")]
        public async Task VerifyEmail_Get_RedirectResult_SuccessAsync() //Registration complete
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

            //Set the user up as if finished AboutYou which is email known etc but not sent
            var routeData = new RouteData();
            routeData.Values.Add("Action", "VerifyEmail");
            routeData.Values.Add("Controller", "Register");

            var model = new VerifyViewModel();

            //var controller = UiTestHelper.GetController<RegisterController>();
            var controller = UiTestHelper.GetController<RegisterController>(1, routeData, user);

            //Add the code to the querystring 
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
        public async Task VerifyEmail_Post_SuccessAsync()
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
            Assert.That(result.ViewName == "VerifyEmail", "Incorrect view returned,Verifiction is incomplete");
            Assert.That(
                result.Model != null && result.Model.GetType() == typeof(VerifyViewModel),
                "Expected VerifyViewModel or Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");
        }

        #endregion

        #region OrganisationType

        [Test]
        [Description("Ensure the Organisation type form is returned for the current user ")]
        public void OrganisationType_Get_Success()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User {UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now};

            var routeData = new RouteData();
            routeData.Values.Add("Action", "OrganisationType");
            routeData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user);
            //controller.StashModel(model);

            //ACT:
            var result = controller.OrganisationType() as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "OrganisationType", "Incorrect view returned");
            Assert.NotNull(result.Model as OrganisationViewModel, "Expected OrganisationViewModel");
            Assert.That(result.Model.GetType() == typeof(OrganisationViewModel), "Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");
        }
        
        [Test]
        [Description("Registration Controller Organisation Type When User Not Registered")]
        public void OrganisationType_Get_When_User_Not_Registered()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User {UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = null};

            var routeData = new RouteData();
            routeData.Values.Add("Action", "OrganisationType");
            routeData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user);

            //ACT:
            var result = controller.OrganisationType() as ViewResult;

            //ASSERT
            Assert.NotNull(result, "Expected ViewResult");
            Assert.AreEqual("You haven’t verified your email address yet", ((ErrorViewModel) result.Model).Title);
        }

        [Ignore("Needs fixing/deleting")]
        [Test]
        [Description("Check registration completes successfully during fasttrack registration")]
        public void OrganisationType_FastTrackRegistration_ServiceActivated()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User {UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now};
            var org = new Organisation {OrganisationId = 1, SectorType = SectorTypes.Private};
            var address = new OrganisationAddress {AddressId = 1, OrganisationId = 1, Organisation = org, Status = AddressStatuses.Pending};
            var orgScope = new OrganisationScope {
                OrganisationId = org.OrganisationId,
                RegisterStatus = RegisterStatuses.RegisterPending,
                ContactEmailAddress = user.EmailAddress
            };

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.OrganisationType));
            routeData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user, org, address, orgScope);

            Mock<IScopeBusinessLogic> mockScopeBL = AutoFacExtensions.ResolveAsMock<IScopeBusinessLogic>(true);
            //ACT:
            var result = controller.OrganisationType() as RedirectToActionResult;
            UserOrganisation userOrg = controller.DataRepository.GetAll<UserOrganisation>()
                .FirstOrDefault(uo => uo.UserId == user.UserId && uo.OrganisationId == orgScope.OrganisationId);

            //ASSERT:
            Expect(result != null, "Expected RedirectToActionResult");
            Expect(result.ActionName == "ServiceActivated", "Expected redirect to ServiceActivated");
            Expect(userOrg.PINConfirmedDate > DateTime.MinValue);
            Expect(userOrg.Organisation.Status == OrganisationStatuses.Active);
            Expect(userOrg.Organisation.LatestAddress.AddressId == address.AddressId);
            Expect(controller.ReportingOrganisationId == org.OrganisationId);
            Expect(address.Status == AddressStatuses.Active);
            Expect(orgScope.RegisterStatus == RegisterStatuses.RegisterComplete);
            Expect(orgScope.RegisterStatusDate > DateTime.MinValue);
        }

        [Test]
        [Description("Private Sector:Ensure the Organisation type form is confirmed and sent successfully")]
        public void OrganisationType_Post_PrivateSector_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User {UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now};
            //var organisation = new Organisation() { OrganisationId = 1 };
            //var userOrganisation = new UserOrganisation() { OrganisationId = 1, Organisation = organisation, UserId = 1, PINConfirmedDate = VirtualDateTime.Now, PINHash = "0" };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "OrganisationType");
            routeData.Values.Add("Controller", "Register");

            var actualModel = new OrganisationViewModel {ManualRegistration = false, SectorType = SectorTypes.Private};

            var controller = UiTestHelper.GetController<RegisterController>(1, routeData, user /*, userOrganisation, organisation*/);
            controller.Bind(actualModel);

            //Stash the object for the unstash to happen in code
            controller.StashModel(actualModel);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.OrganisationType(actualModel) as RedirectToActionResult;

            //ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToActionResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.ActionName == "OrganisationSearch", "Redirected to the wrong view");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var expectedModel = controller.UnstashModel<OrganisationViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(expectedModel, "Expected OrganisationViewModel");

            //8.verify that it was private sector was selected
            actualModel.Compare(expectedModel);
        }

        [Test]
        [Description("Public Sector:Ensure the Organisation type form is confirmed and sent successfully")]
        public void OrganisationType_Post_PublicSector_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User {UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now};

            //var organisation = new Organisation() { OrganisationId = 1 };
            //var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = VirtualDateTime.Now, PINHash = "0" };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "OrganisationType");
            routeData.Values.Add("Controller", "Register");

            var actualModel = new OrganisationViewModel {ManualRegistration = false, SectorType = SectorTypes.Public};

            var controller = UiTestHelper.GetController<RegisterController>(1, routeData, user);
            controller.Bind(actualModel);

            //Stash the object for the unstash to happen in code
            controller.StashModel(actualModel);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.OrganisationType(actualModel) as RedirectToActionResult;

            //ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToActionResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.ActionName == "OrganisationSearch", "Redirected to the wrong view");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var expectedModel = controller.UnstashModel<OrganisationViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(expectedModel, "Expected OrganisationViewModel");

            //7.Verify the values from the result that was stashed matches that of the Arrange values here
            actualModel.Compare(expectedModel);
        }

        [Test(Author = "Oscar Lagatta")]
        [Description("Organisation t ectorTypes.Private, SectorTypes.Public")]
        public void OrganisationType_Post_When_Sector_Type_Unknown()
        {
            // Arrange
            User mockUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            Organisation mockOrg = OrganisationHelper.GetPublicOrganisation();
            UserOrganisation mockUserOrg = UserOrganisationHelper.LinkUserWithOrganisation(mockUser, mockOrg);

            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "ServiceActivated");
            mockRouteData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(-1, mockRouteData, mockUser, mockOrg, mockUserOrg);
            controller.ReportingOrganisationId = mockOrg.OrganisationId;

            var testUri = new Uri("https://localhost/register/activate-service");
            controller.AddMockUriHelper(testUri.ToString(), "ActivateService");

            //Mock the Referrer
            controller.Request.Headers["Referer"] = testUri.ToString();

            controller.StashModel(new OrganisationViewModel());

            // ACT
            var result = controller.OrganisationType(new OrganisationViewModel {SectorType = SectorTypes.Unknown}) as ViewResult;

            // ASSERT
            Assert.NotNull(result);
            Assert.That(result.ViewName == "OrganisationType");
            Assert.NotNull(result.Model as OrganisationViewModel);
            Assert.That(!result.ViewData.ModelState.IsValid);
        }
        
        #endregion

        #region OrganisationSearch

        [Test]
        [Description("Ensure the Organisation search form is returned for the current user ")]
        public void OrganisationSearch_Get_Success()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User {UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now};

            var routeData = new RouteData();
            routeData.Values.Add("Action", "OrganisationSearch");
            routeData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user);
            //controller.StashModel(model);

            var orgModel = new OrganisationViewModel {ManualRegistration = false};
            controller.StashModel(orgModel);

            //ACT:
            var result = controller.OrganisationSearch() as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "OrganisationSearch", "Incorrect view returned");
            Assert.NotNull(result.Model as OrganisationViewModel, "Expected OrganisationViewModel");
            Assert.That(result.Model.GetType() == typeof(OrganisationViewModel), "Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");

            // var controller = UiTestHelper.GetController<RegisterController>();
            // controller.PublicSectorRepository.Insert(new EmployerRecord());
        }

        [Test]
        [Description(
            "Ensure that organisation search form has a search text in its field sent successfully and a a matching record is returned")]
        public async Task OrganisationSearch_Post_PrivateSector_SuccessAsync()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User {UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now};
            //var organisation = new Organisation() { OrganisationId = 1 };
            //var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = VirtualDateTime.Now, PINHash = "0" };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "OrganisationSearch");
            routeData.Values.Add("Controller", "Register");

            //search text in model
            var expectedModel = new OrganisationViewModel {
                Employers = new PagedResult<EmployerRecord>(),
                SearchText = "smith ltd",
                ManualRegistration = false,
                SectorType = SectorTypes.Private,
                CompanyNumber = "456GT657",
                Country = "UK",
                Postcode = "nw1 5re"
            };


            var controller = UiTestHelper.GetController<RegisterController>(1, routeData, user);
            controller.Bind(expectedModel);

            //insert  some records into the db...
            controller.PrivateSectorRepository.Insert(
                new EmployerRecord {
                    OrganisationName = "acme inc",
                    Address1 = "123",
                    Address2 = "EverGreen Terrace",
                    CompanyNumber = "123QA432",
                    Country = "UK",
                    PostCode = "e12 3eq"
                });

            controller.PrivateSectorRepository.Insert(
                new EmployerRecord {
                    OrganisationName = "smith ltd",
                    Address1 = "45",
                    Address2 = "iverson rd",
                    CompanyNumber = "456GT657",
                    Country = "UK",
                    PostCode = "nw1 5re"
                });

            controller.PrivateSectorRepository.Insert(
                new EmployerRecord {
                    OrganisationName = "smith & Wes ltd",
                    Address1 = "45",
                    Address2 = "iverson rd",
                    CompanyNumber = "456GT657",
                    Country = "UK",
                    PostCode = "nw1 5re"
                });

            controller.PrivateSectorRepository.Insert(
                new EmployerRecord {
                    OrganisationName = "smithers and sons ltd",
                    Address1 = "45",
                    Address2 = "iverson rd",
                    CompanyNumber = "456GT657",
                    Country = "UK",
                    PostCode = "nw1 5re"
                });

            controller.PrivateSectorRepository.Insert(
                new EmployerRecord {
                    OrganisationName = "excetera ltd",
                    Address1 = "123",
                    Address2 = "Venice avenue ",
                    CompanyNumber = "123QA432",
                    Country = "UK",
                    PostCode = "w1 9eaz"
                });

            //Stash the object for the unstash to happen in code
            controller.StashModel(expectedModel);

            //ACT:
            //2.Run and get the result of the test
            var result = await controller.OrganisationSearch(expectedModel) as RedirectToActionResult;

            //3.If the redirection successfull retrieve the model stash sent with the redirect.
            //returned from the MockPrivateEmployerRepository db then stashed and then unstashed
            var actualModel = controller.UnstashModel<OrganisationViewModel>();

            //check that the search returned a match in the db
            //var sResult     = controller.DataRepository.GetAll<OrganisationViewModel>().Where(o => o.CompanyNumber == resultModel.CompanyNumber);
            //var pagedResult =  controller.PrivateSectorRepository.Search(model.SearchText, 1, Settings.Default.EmployerPageSize);

            //ASSERT:
            //4.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToActionResult");
            Assert.That(result.ActionName == nameof(RegisterController.ChooseOrganisation), "Redirected to the wrong view");

            //5.check that the stashed model with the redirect is not null.
            Assert.NotNull(actualModel, "Expected OrganisationViewModel");

            //6.Verify the values from the result that was stashed matches that of the Arrange values here
            actualModel.Compare(expectedModel);
        }

        [Test]
        [Description("Ensure the Step4 succeeds when all fields are good")]
        public async Task OrganisationSearch_Post_PublicSector_SuccessAsync()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User {UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now};

            //var organisation = new Organisation() { OrganisationId = 1 };
            //var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = VirtualDateTime.Now, PINHash = "0" };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "OrganisationSearch");
            routeData.Values.Add("Controller", "Register");

            var expectedModel = new OrganisationViewModel {
                Employers = new PagedResult<EmployerRecord>(),
                SearchText = "5 Boroughs Partnership NHS Foundation Trust",
                ManualRegistration = false,
                SectorType = SectorTypes.Public
            };


            var controller = UiTestHelper.GetController<RegisterController>(1, routeData, user);
            controller.Bind(expectedModel);

            //insert  some records into the db...
            controller.PublicSectorRepository.Insert(
                new EmployerRecord {OrganisationName = "2Gether NHS Foundation Trust", EmailDomains = "nhs.uk"});
            controller.PublicSectorRepository.Insert(
                new EmployerRecord {OrganisationName = "5 Boroughs Partnership NHS Foundation Trust", EmailDomains = "nhs.uk"});
            controller.PublicSectorRepository.Insert(
                new EmployerRecord {OrganisationName = "Abbots Langley Parish Council", EmailDomains = "abbotslangley-pc.gov.uk"});
            controller.PublicSectorRepository.Insert(
                new EmployerRecord {OrganisationName = "Aberdeen City Council", EmailDomains = "aberdeencityandshire-sdpa.gov.uk"});
            controller.PublicSectorRepository.Insert(
                new EmployerRecord {OrganisationName = "Aberdeenshire Council", EmailDomains = "aberdeenshire.gov.uk"});

            //Stash the object for the unstash to happen in code
            controller.StashModel(expectedModel);

            //ACT:
            //2.Run and get the result of the test
            var result = await controller.OrganisationSearch(expectedModel) as RedirectToActionResult;


            //ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToActionResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.ActionName == nameof(RegisterController.ChooseOrganisation), "Redirected to the wrong view");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var actualModel = controller.UnstashModel<OrganisationViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(actualModel, "Expected OrganisationViewModel");

            //8.verify that it was private sector was selected
            actualModel.Compare(expectedModel);
        }

        #endregion

    }
}

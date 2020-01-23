using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using GenderPayGap.Core;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Tests.Common.TestHelpers;
using GenderPayGap.Tests.TestHelpers;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Classes.Services;
using GenderPayGap.WebUI.Controllers;
using GenderPayGap.WebUI.Models.Register;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.Tests
{

    [TestFixture]
    [SetCulture("en-GB")]
    public class RegisterControllerTests : AssertionHelper
    {

        #region GET AboutYou()

        [Test]
        [Description("RegisterController.AboutYou GET: When PendingFastrack Then ViewModel Should Contain Scope Contact Details")]
        public async Task
            RegisterController_AboutYou_GET_When_PendingFastrack_Cookie_Then_ViewModel_Should_Contain_Scope_Contact_DetailsAsync()
        {
            // Arrange
            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "AboutYou");
            mockRouteData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(0, mockRouteData);
            await controller.Cache.RemoveAsync($"{controller.HttpContext.GetUserHostAddress()}:lastFasttrackDate");

            // Ensure we call the scope service GetScopeFromFastTrackCode implementation
            Mock<IScopePresentation> mockScopeBL = Mock.Get(controller.ScopePresentation);
            mockScopeBL.CallBase = true;

            //Populate the PendingFasttrackCodes
            controller.PendingFasttrackCodes =
                "EmployerRef:SecurityCode:ExpectedContactFirstname:ExpectedContactLastname:ExpectedContactEmailAddress";

            // Act
            IActionResult actionResult = await controller.AboutYou();

            // Assert
            Assert.NotNull(actionResult, "ViewResult should not be null");

            var viewResult = actionResult as ViewResult;
            Assert.AreEqual(viewResult.ViewName, "AboutYou", "Expected the ViewName to be 'AboutYou'");

            var viewModel = (RegisterViewModel) viewResult.Model;
            Assert.AreEqual(viewModel.FirstName, "ExpectedContactFirstname", "Expected the FirstName to be 'ExpectedContactFirstname'");
            Assert.AreEqual(viewModel.LastName, "ExpectedContactLastname", "Expected the LastName to be 'ExpectedContactLastname'");
            Assert.AreEqual(
                viewModel.EmailAddress,
                "ExpectedContactEmailAddress",
                "Expected the EmailAddress to be 'ExpectedContactEmailAddress'");
        }

        #endregion

        #region POST AboutYou()

        [Test]
        [Ignore("msande")]
        [Description("RegisterController.AboutYou POST: When PendingFastrack Then Save in the user settings")]
        public async Task RegisterController_AboutYou_POST_When_PendingFastrack_Then_SaveInUserSettingsAsync()
        {
            // Arrange
            var user = new User {UserId = 0};

            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "AboutYou");
            mockRouteData.Values.Add("Controller", "Register");

            var mockViewModel = new RegisterViewModel {EmailAddress = "mock@test.com", Password = "12345678"};

            var controller = UiTestHelper.GetController<RegisterController>(0, mockRouteData);

            // Ensure we call the scope service GetScopeFromFastTrackCode implementation
            Mock<IScopePresentation> mockScopeBL = Mock.Get(controller.ScopePresentation);
            mockScopeBL.CallBase = true;

            //Populate the PendingFasttrackCodes
            controller.PendingFasttrackCodes =
                "EmployerRef:SecurityCode:ExpectedContactFirstname:ExpectedContactLastname:ExpectedContactEmailAddress";

            // Act
            var result = await controller.AboutYou(mockViewModel) as RedirectToActionResult;
            object stashedModel = controller.Session[controller + ":Model"];

            // Assert
            Assert.NotNull(result, "RedirectToActionResult should not be null");
            Assert.AreEqual(result.ActionName, "VerifyEmail", "Expected the Action to be 'VerifyEmail'");
            Assert.AreEqual(result.ControllerName, "Register", "Expected the Controller to be 'Register'");

            // Assert User Setting
            UserSetting userSetting = controller.DataRepository.GetAll<User>()
                .FirstOrDefault()
                .UserSettings.FirstOrDefault(x => x.Key == UserSettingKeys.PendingFasttrackCodes);
            Assert.AreEqual(
                userSetting.Value,
                "EmployerRef:SecurityCode",
                "Expected the UserSetting to contain employer reference and security code");
        }

        #endregion

        [Test]
        [Ignore("Not implemented")]
        [Description("RegisterController.VerifyEmail GET: When PendingFastrack Setting load and clear PendingFastrack")]
        public void RegisterController_VerifyEmail_GET_When_PendingFastrackUserSetting_Then_SetPendingFastrack()
        {
            throw new NotImplementedException();
        }

        #region GET OrganisationType()

        [Ignore("Not implemented")]
        [Test]
        [Description("RegisterController.OrganisationType GET: When PendingFastrack Then start fasttrack registration")]
        public void RegisterController_OrganisationType_GET_When_PendingFastrack_Then_StartFastTrackRegistration()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region GET ServiceActivated()

        [Ignore("Needs fixing/deleting")]
        [Test]
        [Description("RegisterController.ServiceActivated GET: When OrgScope is Not Null Then Return Expected ViewData")]
        public void RegisterController_ServiceActivated_GET_When_OrgScope_is_Not_Null_Then_Return_Expected_ViewData()
        {
            // Arrange
            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "ServiceActivated");
            mockRouteData.Values.Add("Controller", "Register");

            var mockOrg = new Organisation {
                OrganisationId = 52425, SectorType = SectorTypes.Private, OrganisationName = "Mock Organisation Ltd"
            };

            var mockUser = new User {UserId = 87654, EmailAddress = "mock@test.com", EmailVerifiedDate = VirtualDateTime.Now};

            var mockReg = new UserOrganisation {UserId = 87654, OrganisationId = 52425, PINConfirmedDate = VirtualDateTime.Now};

            var controller = UiTestHelper.GetController<RegisterController>(87654, mockRouteData, mockUser, mockOrg, mockReg);
            controller.ReportingOrganisationId = mockOrg.OrganisationId;

            var testUri = new Uri("https://localhost/register/activate-service");
            controller.AddMockUriHelper(testUri.ToString(), "ActivateService");

            //Mock the Referrer
            controller.Request.Headers["Referer"] = testUri.ToString();

            // Act
            var viewResult = controller.ServiceActivated() as ViewResult;

            // Assert
            Assert.NotNull(viewResult, "ViewResult should not be null");
            Assert.AreEqual(viewResult.ViewName, "ServiceActivated", "Expected the ViewName to be 'ServiceActivated'");

            // Assert ViewData
            Expect(controller.ViewBag.OrganisationName == mockOrg.OrganisationName, "Expected OrganisationName");
        }

        #endregion


        #region Confirm Organisation

        [Test(Author = "Oscar Lagatta")]
        [Description("Register Controller ConfirmOrganisation When User User Not Registered")]
        public async Task RegisterController_ConfirmOrganisation_When_User_Not_RegisteredAsync()
        {
            // Arrange
            User mockUser = UserHelper.GetNotAdminUserWithoutVerifiedEmailAddress();

            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "ServiceActivated");
            mockRouteData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(-1, mockRouteData, mockUser);

            var result = await controller.ConfirmOrganisation() as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("You haven’t verified your email address yet", ((ErrorViewModel) result.Model).Title);
        }

        [Test(Author = "Oscar Lagatta")]
        [Description(
            "RegisterController.ConfirmOrganisation GET When Cannot Load Employers From Session Then Return Error View Model 1112")]
        public async Task RegisterController_ConfirmOrganisation_When_Cannot_Load_Employers_From_Session_Then_Return_ErrorViewModelAsync()
        {
            // Arrange
            User mockUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            Organisation mockOrg = OrganisationHelper.GetPublicOrganisation();
            UserOrganisation mockUserOrg = UserOrganisationHelper.LinkUserWithOrganisation(mockUser, mockOrg);

            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "ServiceActivated");
            mockRouteData.Values.Add("Controller", "Register");

            Return mockReturn = ReturnHelper.GetNewReturnForOrganisationAndYear(mockUserOrg, Global.FirstReportingYear);

            OrganisationHelper.LinkOrganisationAndReturn(mockOrg, mockReturn);

            var controller = UiTestHelper.GetController<RegisterController>(-1, mockRouteData, mockUser, mockOrg, mockUserOrg, mockReturn);
            controller.ReportingOrganisationId = mockOrg.OrganisationId;

            var testUri = new Uri("https://localhost/register/activate-service");
            controller.AddMockUriHelper(testUri.ToString(), "ActivateService");

            //Mock the Referrer
            controller.Request.Headers["Referer"] = testUri.ToString();


            // ACT
            var result = await controller.ConfirmOrganisation() as ViewResult;

            // ASSERT
            Assert.NotNull(result);
            Assert.AreEqual(((ErrorViewModel) result.Model).ErrorCode, 1112);
        }

        [Test]
        [Description("")]
        public async Task RegisterController_ConfirmOrganisation_Get_SuccessAsync()
        {
            // Arrange
            User mockUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            Organisation mockOrg = OrganisationHelper.GetPublicOrganisation();
            UserOrganisation mockUserOrg = UserOrganisationHelper.LinkUserWithOrganisation(mockUser, mockOrg);

            Return mockReturn = ReturnHelper.GetNewReturnForOrganisationAndYear(mockUserOrg, Global.FirstReportingYear);

            OrganisationHelper.LinkOrganisationAndReturn(mockOrg, mockReturn);

            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "ServiceActivated");
            mockRouteData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(-1, mockRouteData, mockUser, mockOrg, mockUserOrg, mockReturn);
            controller.ReportingOrganisationId = mockOrg.OrganisationId;

            controller.StashModel(new OrganisationViewModel());

            var testUri = new Uri("https://localhost/register/activate-service");
            controller.AddMockUriHelper(testUri.ToString(), "ActivateService");

            //Mock the Referrer
            controller.Request.Headers["Referer"] = testUri.ToString();

            var result = await controller.ConfirmOrganisation() as ViewResult;

            Assert.NotNull(result);

            Assert.AreEqual("ConfirmOrganisation", result.ViewName);
        }

        // 

        #endregion

    }

}

using System;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Tests.Common.TestHelpers;
using GenderPayGap.Tests.TestHelpers;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Controllers;
using GenderPayGap.WebUI.Models.Register;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using NUnit.Framework;

namespace GenderPayGap.Tests
{

    [TestFixture]
    [SetCulture("en-GB")]
    public class RegisterControllerTests : AssertionHelper
    {
        
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

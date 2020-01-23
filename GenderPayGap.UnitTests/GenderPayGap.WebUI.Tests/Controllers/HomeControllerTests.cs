using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Controllers;
using GenderPayGap.WebUI.Models.Home;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using NUnit.Framework;

namespace GenderPayGap.Tests
{

    [TestFixture]
    [SetCulture("en-GB")]
    public class HomeControllerTests
    {

        [Test]
        [Description(
            "RegisterController.ReadPrivacyStatement POST: When PrivacyStatementModel.Accept is Yes Then Redirect to ManageOrganisations")]
        public async Task
            RegisterController_ReadPrivacyStatement_POST_When_PrivacyStatementModel_Accept_is_Yes_Then_Redirect_to_ManageOrganisationsAsync()
        {
            // Arrange
            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "ReadPrivacyStatement");
            mockRouteData.Values.Add("Controller", "Home");

            var mockUser = new User {
                UserId = 87654,
                EmailAddress = "mock@test.com",
                EmailVerifiedDate = VirtualDateTime.Now,
                UserSettings = new HashSet<UserSetting>()
            };

            var mockModel = new PrivacyStatementModel {Accept = "Yes"};
            string testDate = VirtualDateTime.Now.ToString();

            var controller = UiTestHelper.GetController<HomeController>(-1, mockRouteData, mockUser);

            // Act
            var result = await controller.PrivacyPolicy("Continue") as RedirectToActionResult;

            // Assert
            Assert.NotNull(result, "RedirectToActionResult should not be null");
            Assert.AreEqual("ManageOrganisations", result.ActionName, "Expected the Action to be 'ManageOrganisations'");
            Assert.AreEqual("Organisation", result.ControllerName, "Expected the Controller to be 'Home'");

            // Assert User Settings
            UserSetting acceptedSetting = mockUser.UserSettings.FirstOrDefault(u => u.Key == UserSettingKeys.AcceptedPrivacyStatement);
            Assert.AreEqual(1, mockUser.UserSettings.Count, "UserSettings should have one element on the list");
            Assert.NotNull(acceptedSetting, "AcceptedPrivacyStatement setting should exist");
            Assert.GreaterOrEqual(
                DateTime.Parse(acceptedSetting.Value),
                DateTime.Parse(testDate),
                "AcceptedPrivacyStatement value should be a new date");
        }

    }

}

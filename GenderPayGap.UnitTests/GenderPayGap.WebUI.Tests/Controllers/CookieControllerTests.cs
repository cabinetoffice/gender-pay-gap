using GenderPayGap.WebUI.Controllers;
using GenderPayGap.WebUI.Cookies;
using GenderPayGap.WebUI.Models.Cookie;
using GenderPayGap.WebUI.Tests.Builders;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Controllers
{
    public class CookieControllerTests
    {

        #region CookieConsent

        [Test]
        public void CookieController_CookieConsent_Redirect_To_HomePage()
        {
            // Arrange
            var controllerBuilder = new ControllerBuilder<CookieController>();
            var controller = controllerBuilder
                .Build();
            var cookieConsent = new CookieConsent {AdditionalCookies = "accept"};

            // Act
            var result = controller.CookieConsent(cookieConsent) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(result.ControllerName, "Viewing");
            Assert.AreEqual(result.ActionName, "Index");
        }

        [Test]
        public void CookieController_CookieConsent_Cookies_Accepted()
        {
            // Arrange
            var controllerBuilder = new ControllerBuilder<CookieController>();
            var controller = controllerBuilder
                .Build();
            var cookieConsent = new CookieConsent {AdditionalCookies = "accept"};
            var expectedCookieSettings = new CookieSettings
            {
                GoogleAnalyticsGpg = true, GoogleAnalyticsGovUk = true, RememberSettings = true
            };

            // Act
            controller.CookieConsent(cookieConsent);

            // Assert
            controller.AssertCookieAdded("cookie_settings", JsonConvert.SerializeObject(expectedCookieSettings));
            controller.AssertCookieAdded("seen_cookie_message", "{\"Version\":1}");
        }

        [Test]
        public void CookieController_CookieConsent_Cookies_Rejected()
        {
            // Arrange
            var controllerBuilder = new ControllerBuilder<CookieController>();
            var controller = controllerBuilder
                .Build();
            var cookieConsent = new CookieConsent {AdditionalCookies = "reject"};
            var expectedCookieSettings = new CookieSettings
            {
                GoogleAnalyticsGpg = false, GoogleAnalyticsGovUk = false, RememberSettings = false
            };

            // Act
            controller.CookieConsent(cookieConsent);

            // Assert
            controller.AssertCookieAdded("cookie_settings", JsonConvert.SerializeObject(expectedCookieSettings));
            controller.AssertCookieAdded("seen_cookie_message", "{\"Version\":1}");
        }

        #endregion

    }
}

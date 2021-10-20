using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.WebUI.Cookies;
using GenderPayGap.WebUI.Models.Cookie;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    public class CookieController : Controller
    {

        [HttpGet("/cookies")]
        public IActionResult CookieSettingsGet()
        {
            CookieSettings cookieSettings = CookieHelper.GetCookieSettingsCookie(Request);

            var cookieSettingsViewModel = new CookieSettingsViewModel
            {
                GoogleAnalyticsGpg = cookieSettings.GoogleAnalyticsGpg ? "On" : "Off",
                GoogleAnalyticsGovUk = cookieSettings.GoogleAnalyticsGovUk ? "On" : "Off",
                RememberSettings = cookieSettings.RememberSettings ? "On" : "Off"
            };

            return View("CookieSettings", cookieSettingsViewModel);
        }

        [HttpPost("/cookies")]
        [ValidateAntiForgeryToken]
        public IActionResult CookieSettingsPost(CookieSettingsViewModel cookieSettingsViewModel)
        {
            var cookieSettings = new CookieSettings
            {
                GoogleAnalyticsGpg = cookieSettingsViewModel != null && cookieSettingsViewModel.GoogleAnalyticsGpg == "On",
                GoogleAnalyticsGovUk = cookieSettingsViewModel != null && cookieSettingsViewModel.GoogleAnalyticsGovUk == "On",
                RememberSettings = cookieSettingsViewModel != null && cookieSettingsViewModel.RememberSettings == "On"
            };

            CookieHelper.SetCookieSettingsCookie(Response, cookieSettings);
            CookieHelper.SetSeenCookieMessageCookie(Response);

            cookieSettingsViewModel.ChangesHaveBeenSaved = true;

            CustomLogger.Information(
                "Updated cookie settings",
                new
                {
                    CookieSettings = cookieSettings,
                    HttpRequestMethod = HttpContext.Request.Method,
                    HttpRequestPath = HttpContext.Request.Path.Value
                });

            return View("CookieSettings", cookieSettingsViewModel);
        }

        [HttpPost("/cookie-consent")]
        [ValidateAntiForgeryToken]
        public IActionResult CookieConsent(CookieConsent consent)
        {
            var additionalCookiesConsent = consent.AdditionalCookies == "accept";
            var cookieSettings = new CookieSettings
            {
                GoogleAnalyticsGpg = additionalCookiesConsent,
                GoogleAnalyticsGovUk = additionalCookiesConsent,
                RememberSettings = additionalCookiesConsent
            };

            CookieHelper.SetCookieSettingsCookie(Response, cookieSettings);
            CookieHelper.SetSeenCookieMessageCookie(Response);

            return RedirectToAction("Index", "Viewing");
        }

        [HttpGet("/cookie-details")]
        public IActionResult CookieDetails()
        {
            return View("CookieDetails");
        }

    }
}

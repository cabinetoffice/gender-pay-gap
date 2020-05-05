using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic.Repositories;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Core.Models.Cookies;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Classes.Services;
using GenderPayGap.WebUI.Views.Home;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    public class HomeController : BaseController
    {

        #region Constructors

        public HomeController(
            IHttpCache cache,
            IHttpSession session,
            IScopePresentation scopeUIService,
            IDataRepository dataRepository,
            IWebTracker webTracker) : base(cache, session, dataRepository, webTracker)
        {
            ScopePresentation = scopeUIService;
        }

        #endregion

        #region Dependencies

        public IScopePresentation ScopePresentation { get; }

        #endregion

        [HttpGet("~/ping")]
        public IActionResult Ping()
        {
            return new OkResult(); // OK = 200
        }

        [HttpGet("~/sign-out")]
        public IActionResult SignOut()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View("SignOut");
            }

            string returnUrl = Url.Action("SignOut", "Home", null, "https");

            return LogoutUser(returnUrl);
        }

        [HttpGet("~/session-expired")]
        public IActionResult SessionExpired()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View("SessionExpired");
            }

            return LogoutUser(Url.Action("SessionExpired", "Home", null, "https"));
        }

        #region Contact Us

        [HttpGet("~/contact-us")]
        public IActionResult ContactUs()
        {
            return View("ContactUs");
        }

        #endregion

        #region Initialisation

        /// <summary>
        ///     This action is only used to warm up this controller on initialisation
        /// </summary>
        /// <returns></returns>
        [HttpGet("Init")]
        public IActionResult Init()
        {
            return new EmptyResult();
        }

        #endregion

        [HttpGet("~/report-concerns")]
        public IActionResult ReportConcerns()
        {
            return View(nameof(ReportConcerns));
        }

        [HttpGet("~/cookies")]
        public IActionResult CookieSettingsGet()
        {
            CookieSettings cookieSettings = CookieHelper.GetCookieSettingsCookie(Request);

            var cookieSettingsViewModel = new CookieSettingsViewModel {
                GoogleAnalyticsGpg = cookieSettings.GoogleAnalyticsGpg ? "On" : "Off",
                GoogleAnalyticsGovUk = cookieSettings.GoogleAnalyticsGovUk ? "On" : "Off",
                ApplicationInsights = cookieSettings.ApplicationInsights ? "On" : "Off",
                RememberSettings = cookieSettings.RememberSettings ? "On" : "Off"
            };

            return View("CookieSettings", cookieSettingsViewModel);
        }

        [HttpPost("~/cookies")]
        public IActionResult CookieSettingsPost(CookieSettingsViewModel cookieSettingsViewModel)
        {
            var cookieSettings = new CookieSettings {
                GoogleAnalyticsGpg = cookieSettingsViewModel != null && cookieSettingsViewModel.GoogleAnalyticsGpg == "On",
                GoogleAnalyticsGovUk = cookieSettingsViewModel != null && cookieSettingsViewModel.GoogleAnalyticsGovUk == "On",
                ApplicationInsights = cookieSettingsViewModel != null && cookieSettingsViewModel.ApplicationInsights == "On",
                RememberSettings = cookieSettingsViewModel != null && cookieSettingsViewModel.RememberSettings == "On"
            };

            CookieHelper.SetCookieSettingsCookie(Response, cookieSettings);
            CookieHelper.SetSeenCookieMessageCookie(Response);

            cookieSettingsViewModel.ChangesHaveBeenSaved = true;

            CustomLogger.Information(
                "Updated cookie settings",
                new {
                    CookieSettings = cookieSettings,
                    HttpRequestMethod = HttpContext.Request.Method,
                    HttpRequestPath = HttpContext.Request.Path.Value
                });

            return View("CookieSettings", cookieSettingsViewModel);
        }

        [HttpPost("~/accept-all-cookies")]
        public IActionResult AcceptAllCookies()
        {
            var cookieSettings = new CookieSettings {
                GoogleAnalyticsGpg = true, GoogleAnalyticsGovUk = true, ApplicationInsights = true, RememberSettings = true
            };

            CookieHelper.SetCookieSettingsCookie(Response, cookieSettings);
            CookieHelper.SetSeenCookieMessageCookie(Response);

            return RedirectToAction("Index", "Viewing");
        }

        [HttpGet("~/cookie-details")]
        public IActionResult CookieDetails()
        {
            return View("CookieDetails");
        }

        #region PrivacyPolicy

        [HttpGet("~/privacy-policy")]
        public IActionResult PrivacyPolicy()
        {
            return View("PrivacyPolicy", null);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [HttpPost("~/privacy-policy")]
        public async Task<IActionResult> PrivacyPolicy(string command)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("PrivacyPolicy");
            }

            //Ensure user has completed the registration process
            User currentUser;
            IActionResult checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult == null)
            {
                return checkResult;
            }

            if (!IsImpersonatingUser && !CurrentUser.IsAdministrator())
            {
                // check if the user has accepted the privacy statement
                DateTime? hasReadPrivacy = currentUser.AcceptedPrivacyStatement;
                if (hasReadPrivacy == null || hasReadPrivacy.ToDateTime() < Global.PrivacyChangedDate)
                {
                    currentUser.AcceptedPrivacyStatement = VirtualDateTime.Now;
                    await DataRepository.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(OrganisationController.ManageOrganisations), "Organisation");
        }

        #endregion

    }
}

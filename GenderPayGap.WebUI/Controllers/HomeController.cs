using System;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.Classes;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    public class HomeController : BaseController
    {

        public HomeController(
            IHttpCache cache,
            IHttpSession session,
            IDataRepository dataRepository,
            IWebTracker webTracker) : base(cache, session, dataRepository, webTracker)
        {
        }


        [HttpGet("~/ping")]
        public IActionResult Ping()
        {
            return new OkResult(); // OK = 200
        }

        #region Contact Us

        [HttpGet("~/contact-us")]
        public IActionResult ContactUs()
        {
            return View("ContactUs");
        }

        #endregion

        [HttpGet("~/report-concerns")]
        public IActionResult ReportConcerns()
        {
            return View(nameof(ReportConcerns));
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

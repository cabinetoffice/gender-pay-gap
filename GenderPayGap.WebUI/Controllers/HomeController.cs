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


        [HttpGet("ping")]
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

        [HttpGet("report-concerns")]
        public IActionResult ReportConcerns()
        {
            return View(nameof(ReportConcerns));
        }

        #region Accessibility Statement
        
        [HttpGet("accessibility-statement")]
        public IActionResult AccessibilityStatement()
        {
            return View("AccessibilityStatement");
        }
        
        #endregion

    }
}

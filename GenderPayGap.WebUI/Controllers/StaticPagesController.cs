using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    public class StaticPagesController : Controller
    {

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return new OkResult(); // OK = 200
        }

        [HttpGet("~/contact-us")]
        public IActionResult ContactUs()
        {
            return View("ContactUs");
        }

        [HttpGet("report-concerns")]
        public IActionResult ReportConcerns()
        {
            return View("ReportConcerns");
        }

        [HttpGet("accessibility-statement")]
        public IActionResult AccessibilityStatement()
        {
            return View("AccessibilityStatement");
        }
        
    }
}

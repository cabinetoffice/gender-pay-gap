using GenderPayGap.WebUI.Classes;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    [Route("go")]
    public class GoController : Controller
    {

        private IWebTracker webTracker;

        public GoController(IWebTracker webTracker)
        {
            this.webTracker = webTracker;
        }

        [HttpGet("register")]
        public IActionResult Register()
        {
            webTracker.TrackPageView(this);
            //disable:DoNotUseRedirectWithReturnUrls
            return Redirect("https://www.gov.uk/report-gender-pay-gap-data");
        }

        [HttpGet("guidance")]
        public IActionResult Guidance()
        {
            webTracker.TrackPageView(this);
            //disable:DoNotUseRedirectWithReturnUrls
            return Redirect("https://www.gov.uk/guidance/gender-pay-gap-who-needs-to-report");
        }

        [HttpGet("report")]
        public IActionResult Report()
        {
            webTracker.TrackPageView(this);
            //disable:DoNotUseRedirectWithReturnUrls
            return Redirect("https://www.gov.uk/report-gender-pay-gap-data");
        }

        [HttpGet("actions")]
        public IActionResult Actions()
        {
            webTracker.TrackPageView(this);
            //disable:DoNotUseRedirectWithReturnUrls
            return Redirect("https://www.gov.uk/government/uploads/system/uploads/attachment_data/file/664017/Gender_pay_gap_-_actions_for_employers.pdf");
        }

    }
}

using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    public class RedirectController : Controller
    {
        // This class contains redirects for outdated URLs
        // Some of these URLs might appear in emails / printed letters, so we want to redirect to an appropriate page, rather than showing a 404

        private IWebTracker webTracker;

        public RedirectController(IWebTracker webTracker)
        {
            this.webTracker = webTracker;
        }
        
        [HttpGet("activate-service")]
        public IActionResult ActivateService()
        {
            return RedirectToAction("ManageOrganisationsGet", "ManageOrganisations");
        }

        // This was the old url given out to create a user account that was shared. Since this was changed
        // they now get a number of user support queries asking for an up to date link. Instead redirect the 
        // old link to the new address.
        [HttpGet("Register/about-you")]
        public IActionResult AboutYou()
        {
            return RedirectToActionPermanent("CreateUserAccountGet", "AccountCreation");
        }

        [HttpGet("go/register")]
        public IActionResult Register()
        {
            webTracker.TrackPageView(this);
            //disable:DoNotUseRedirectWithReturnUrls
            return Redirect("https://www.gov.uk/report-gender-pay-gap-data");
        }
        
        [HttpGet("go/guidance")]
        public IActionResult Guidance()
        {
            webTracker.TrackPageView(this);
            //disable:DoNotUseRedirectWithReturnUrls
            return Redirect("https://www.gov.uk/guidance/gender-pay-gap-who-needs-to-report");
        }
        
        [HttpGet("go/report")]
        public IActionResult Report()
        {
            webTracker.TrackPageView(this);
            //disable:DoNotUseRedirectWithReturnUrls
            return Redirect("https://www.gov.uk/report-gender-pay-gap-data");
        }

        [HttpGet("go/actions")]
        public IActionResult Actions()
        {
            webTracker.TrackPageView(this);
            //disable:DoNotUseRedirectWithReturnUrls
            return Redirect("https://www.gov.uk/government/publications/gender-pay-gap-reporting-guidance-for-employers/closing-your-gender-pay-gap");
        }

        [HttpGet("guidance/eight-ways-to-understand-your-organisations-gender-pay-gap/overview")]
        [HttpGet("guidance/eight-ways-to-understand-your-organisations-gender-pay-gap/1")]
        [HttpGet("guidance/eight-ways-to-understand-your-organisations-gender-pay-gap/2")]
        [HttpGet("guidance/eight-ways-to-understand-your-organisations-gender-pay-gap/3")]
        [HttpGet("guidance/eight-ways-to-understand-your-organisations-gender-pay-gap/4")]
        [HttpGet("guidance/eight-ways-to-understand-your-organisations-gender-pay-gap/5")]
        [HttpGet("guidance/eight-ways-to-understand-your-organisations-gender-pay-gap/6")]
        [HttpGet("guidance/eight-ways-to-understand-your-organisations-gender-pay-gap/7")]
        [HttpGet("guidance/eight-ways-to-understand-your-organisations-gender-pay-gap/8")]
        public IActionResult EightWaysGuidanceRedirect()
        {
            //disable:DoNotUseRedirectWithReturnUrls
            return Redirect("https://www.gov.uk/government/publications/gender-pay-gap-reporting-guidance-for-employers/closing-your-gender-pay-gap");
        }
        
        [HttpGet("actions-to-close-the-gap")]
        [HttpGet("actions-to-close-the-gap/effective-actions")]
        [HttpGet("actions-to-close-the-gap/promising-actions")]
        [HttpGet("actions-to-close-the-gap/actions-with-mixed-results")]
        public IActionResult ActionHubRedirects()
        {
            //disable:DoNotUseRedirectWithReturnUrls
            return Redirect("https://www.gov.uk/government/publications/gender-pay-gap-reporting-guidance-for-employers/closing-your-gender-pay-gap");
        }
        
        [HttpGet("/public/assets/pdf/action-plan-guidance.pdf")]
        [HttpGet("/public/assets/pdf/Evidence-based_actions_for_employers.pdf")]
        [HttpGet("/public/assets/pdf/gender-pay-gap-explained.pdf")]
        [HttpGet("/public/assets/pdf/GEO_Actions_to_Close_the_Gender_Pay_Gap.pdf")]
        [HttpGet("/public/assets/pdf/understand-your-gender-pay-gap.pdf")]
        public IActionResult GuidancePdfRedirects()
        {
            //disable:DoNotUseRedirectWithReturnUrls
            return Redirect("https://www.gov.uk/government/publications/gender-pay-gap-reporting-guidance-for-employers/closing-your-gender-pay-gap");
        }
        
        [HttpGet("/Employer/{employerIdentifier}")]
        public IActionResult ViewEmployerPage(string employerIdentifier, int? page = 1)
        {
            long organisationId = ControllerHelper.DeObfuscateOrganisationIdOrThrow404(employerIdentifier);

            return RedirectToAction("Employer", "ViewReports", new {organisationId});
        }

    }
}

using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    public class RedirectController : Controller
    {
        // This class contains redirects for outdated URLs
        // Some of these URLs might appear in emails / printed letters, so we want to redirect to an appropriate page, rather than showing a 404

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

    }
}

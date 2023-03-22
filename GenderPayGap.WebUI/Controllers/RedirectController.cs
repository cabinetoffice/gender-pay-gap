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

    }
}

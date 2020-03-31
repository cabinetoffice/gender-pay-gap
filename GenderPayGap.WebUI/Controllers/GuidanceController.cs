using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    [Route("guidance")]
    public class GuidanceController : Controller
    {

        [HttpGet("eight-ways-to-understand-your-organisations-gender-pay-gap")]
        public IActionResult EightWaysToUnderstandYourOrganisationsGpg()
        {
            return View("EightWaysToUnderstandYourOrganisationsGpg/Overview");
        }

    }
}

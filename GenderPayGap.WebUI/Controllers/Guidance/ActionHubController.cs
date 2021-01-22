using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Guidance
{
    [Route("actions-to-close-the-gap")]
    public class ActionHubController : Controller
    {
        // GET: ActionHub
        public IActionResult Overview()
        {
            return View("Pages/Overview");
        }

        [HttpGet("effective-actions")]
        public IActionResult Effective()
        {
            return View("Pages/Effective");
        }

        [HttpGet("promising-actions")]
        public IActionResult Promising()
        {
            return View("Pages/Promising");
        }

        [HttpGet("actions-with-mixed-results")]
        public IActionResult MixedResult()
        {
            return View("Pages/MixedResult");
        }
    }
}
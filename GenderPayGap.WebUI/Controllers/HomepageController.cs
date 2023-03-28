using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    public class HomepageController : Controller
    {

        [HttpGet("/")]
        public IActionResult Index()
        {
            return View("Index");
        }

        [HttpGet("viewing")]
        public IActionResult ViewingRedirect()
        {
            return RedirectToActionPermanent("Index");
        }

    }
}

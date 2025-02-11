using Microsoft.AspNetCore.Mvc;

namespace HoldingPage.Controllers
{
    public class HomepageController : Controller
    {

        [HttpGet("/")]
        public IActionResult Index()
        {
            return View("Index");
        }

        [HttpGet("/health-check")]
        public IActionResult HealthCheck()
        {
            return View("Index");
        }

    }
}

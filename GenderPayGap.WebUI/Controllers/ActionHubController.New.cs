using GenderPayGap.Core;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{

    [Route("actions-to-close-the-gap")]
    public partial class ActionHubController : Controller
    {
        private bool UseNewActionHub()
        {
            return VirtualDateTime.Now >= Global.ActionHubSwitchOverDate;
        }

        // GET: ActionHub
        [HttpGet]
        public IActionResult Overview()
        {
            if (UseNewActionHub())
            {
                return Overview2();
            }

            return Overview1();
        }

        [NonAction]
        public IActionResult Overview2()
        {
            //This is required so the tagHelper looks up based on this action name not the actual OverviewAction
            RouteData.Values.Add("SitemapAction", "Overview2");
            return View("/Views/ActionHubNew/Overview.cshtml");
        }

        [HttpGet("leadership-and-accountability")]
        public IActionResult Leadership()
        {
            if (UseNewActionHub())
            {
                return View("/Views/ActionHubNew/Leadership.cshtml");
            }

            return new HttpNotFoundResult();
        }

        [HttpGet("hiring-and-selection")]
        public IActionResult Hiring()
        {
            if (UseNewActionHub())
            {
                return View("/Views/ActionHubNew/Hiring.cshtml");
            }

            return new HttpNotFoundResult();
        }

        [HttpGet("talent-management-learning-and-development")]
        public IActionResult Talent()
        {
            if (UseNewActionHub())
            {
                return View("/Views/ActionHubNew/Talent.cshtml");
            }

            return new HttpNotFoundResult();
        }

        [HttpGet("workplace-flexibility")]
        public IActionResult Workplace()
        {
            if (UseNewActionHub())
            {
                return View("/Views/ActionHubNew/Workplace.cshtml");
            }

            return new HttpNotFoundResult();
        }

        [HttpGet("further-reading")]
        public IActionResult Reading()
        {
            if (UseNewActionHub())
            {
                return View("/Views/ActionHubNew/Reading.cshtml");
            }

            return new HttpNotFoundResult();
        }

    }
}

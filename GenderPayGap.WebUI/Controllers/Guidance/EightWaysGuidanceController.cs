using System.Collections.Generic;
using GenderPayGap.WebUI.Views.Components.PaginationLinks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Guidance
{
    [Route("guidance/eight-ways-to-understand-your-organisations-gender-pay-gap")]
    public class EightWaysGuidanceController : Controller
    {

        [HttpGet("overview")]
        public IActionResult EightWaysOverview()
        {
            return View("Pages/Overview");
        }

        [HttpGet("1")]
        public IActionResult EightWaysDoPeopleGetStuck()
        {
            return View("Pages/DoPeopleGetStuck");
        }

        [HttpGet("2")]
        public IActionResult EightWaysIsThereGenderImbalance()
        {
            return View("Pages/IsThereGenderImbalance");
        }

        [HttpGet("3")]
        public IActionResult EightWaysAreWomenMoreLikely()
        {
            return View("Pages/AreWomenMoreLikely");
        }

        [HttpGet("4")]
        public IActionResult EightWaysDoMenAndWomenLeave()
        {
            return View("Pages/DoMenAndWomenLeave");
        }

        [HttpGet("5")]
        public IActionResult EightWaysDoParticularAspectsOfPay()
        {
            return View("Pages/DoParticularAspectsOfPay");
        }

        [HttpGet("6")]
        public IActionResult EightWaysDoMenAndWomenReceive()
        {
            return View("Pages/DoMenAndWomenReceive");
        }

        [HttpGet("7")]
        public IActionResult EightWaysAreYouDoingAllThatYouCan()
        {
            return View("Pages/AreYouDoingAllThatYouCan");
        }

        [HttpGet("8")]
        public IActionResult EightWaysAreYouSupportingBoth()
        {
            return View("Pages/AreYouSupportingBoth");
        }

        public static PaginationPages GetPagesInThisGuidance(IUrlHelper helper, HttpRequest Request)
        {
            var pages = new List<PaginationPage>
            {
                new PaginationPage
                {
                    Title = "Overview",
                    Url = helper.Action("EightWaysOverview", "EightWaysGuidance")
                },
                new PaginationPage
                {
                    Title = "Do people get ‘stuck’ at certain levels within your organisation?",
                    Url = helper.Action("EightWaysDoPeopleGetStuck", "EightWaysGuidance")
                },
                new PaginationPage
                {
                    Title = "Is there gender imbalance in your promotions?",
                    Url = helper.Action("EightWaysIsThereGenderImbalance", "EightWaysGuidance")
                },
                new PaginationPage
                {
                    Title = "Are women more likely to be recruited into lower paid roles in your organisation?",
                    Url = helper.Action("EightWaysAreWomenMoreLikely", "EightWaysGuidance")
                },
                new PaginationPage
                {
                    Title = "Do men and women leave your organisation at different rates?",
                    Url = helper.Action("EightWaysDoMenAndWomenLeave", "EightWaysGuidance")
                },
                new PaginationPage
                {
                    Title = "Do particular aspects of pay (such as starting salaries and bonuses) differ by gender?",
                    Url = helper.Action("EightWaysDoParticularAspectsOfPay", "EightWaysGuidance")
                },
                new PaginationPage
                {
                    Title = "Do men and women receive different performance scores on average?",
                    Url = helper.Action("EightWaysDoMenAndWomenReceive", "EightWaysGuidance")
                },
                new PaginationPage
                {
                    Title = "Are you doing all that you can to support part-time employees to progress?",
                    Url = helper.Action("EightWaysAreYouDoingAllThatYouCan", "EightWaysGuidance")
                },
                new PaginationPage
                {
                    Title = "Are you supporting both men and women to take on caring responsibilities?",
                    Url = helper.Action("EightWaysAreYouSupportingBoth", "EightWaysGuidance")
                },
            };

            foreach (PaginationPage page in pages)
            {
                string currentRelativeUrl = $"{Request.PathBase}{Request.Path}{Request.QueryString}";
                string currentFullUrl = $"{Request.Scheme}://{Request.Host}{currentRelativeUrl}";
                page.IsCurrentPage = (page.Url == currentFullUrl) || (page.Url == currentRelativeUrl);
            }

            return new PaginationPages
            {
                Pages = pages
            };
        }

    }
}

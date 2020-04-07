using System.Collections.Generic;
using GenderPayGap.WebUI.Views.Components.PaginationLinks;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    [Route("guidance")]
    public class GuidanceController : Controller
    {

        [HttpGet("eight-ways-to-understand-your-organisations-gender-pay-gap/1")]
        public IActionResult EightWaysOverview()
        {
            return View("EightWaysToUnderstandYourOrganisationsGpg/Pages/Overview");
        }

        [HttpGet("eight-ways-to-understand-your-organisations-gender-pay-gap/2")]
        public IActionResult EightWaysDoPeopleGetStuck()
        {
            return View("EightWaysToUnderstandYourOrganisationsGpg/Pages/DoPeopleGetStuck");
        }

        [HttpGet("eight-ways-to-understand-your-organisations-gender-pay-gap/3")]
        public IActionResult EightWaysIsThereGenderImbalance()
        {
            return View("EightWaysToUnderstandYourOrganisationsGpg/Pages/IsThereGenderImbalance");
        }

        [HttpGet("eight-ways-to-understand-your-organisations-gender-pay-gap/4")]
        public IActionResult EightWaysAreWomenMoreLikely()
        {
            return View("EightWaysToUnderstandYourOrganisationsGpg/Pages/AreWomenMoreLikely");
        }

        [HttpGet("eight-ways-to-understand-your-organisations-gender-pay-gap/5")]
        public IActionResult EightWaysDoMenAndWomenLeave()
        {
            return View("EightWaysToUnderstandYourOrganisationsGpg/Pages/DoMenAndWomenLeave");
        }

        [HttpGet("eight-ways-to-understand-your-organisations-gender-pay-gap/6")]
        public IActionResult EightWaysDoParticularAspectsOfPay()
        {
            return View("EightWaysToUnderstandYourOrganisationsGpg/Pages/DoParticularAspectsOfPay");
        }

        [HttpGet("eight-ways-to-understand-your-organisations-gender-pay-gap/7")]
        public IActionResult EightWaysDoMenAndWomenReceive()
        {
            return View("EightWaysToUnderstandYourOrganisationsGpg/Pages/DoMenAndWomenReceive");
        }

        [HttpGet("eight-ways-to-understand-your-organisations-gender-pay-gap/8")]
        public IActionResult EightWaysAreYouDoingAllThatYouCan()
        {
            return View("EightWaysToUnderstandYourOrganisationsGpg/Pages/AreYouDoingAllThatYouCan");
        }

        [HttpGet("eight-ways-to-understand-your-organisations-gender-pay-gap/9")]
        public IActionResult EightWaysAreYouSupportingBoth()
        {
            return View("EightWaysToUnderstandYourOrganisationsGpg/Pages/AreYouSupportingBoth");
        }

        public static PaginationPages GetPagesInThisGuidance(IUrlHelper helper)
        {
            return new PaginationPages
            {
                Pages = new List<PaginationPage>
                {
                    new PaginationPage
                    {
                        Title = "Overview",
                        Url = helper.Action("EightWaysOverview", "Guidance")
                    },
                    new PaginationPage
                    {
                        Title = "Do people get ‘stuck’ at certain levels within your organisation?",
                        Url = helper.Action("EightWaysDoPeopleGetStuck", "Guidance")
                    },
                    new PaginationPage
                    {
                        Title = "Is there gender imbalance in your promotions?",
                        Url = helper.Action("EightWaysIsThereGenderImbalance", "Guidance")
                    },
                    new PaginationPage
                    {
                        Title = "Are women more likely to be recruited into lower paid roles in your organisation?",
                        Url = helper.Action("EightWaysAreWomenMoreLikely", "Guidance")
                    },
                    new PaginationPage
                    {
                        Title = "Do men and women leave your organisation at different rates?",
                        Url = helper.Action("EightWaysDoMenAndWomenLeave", "Guidance")
                    },
                    new PaginationPage
                    {
                        Title = "Do particular aspects of pay (such as starting salaries and bonuses) differ by gender?",
                        Url = helper.Action("EightWaysDoParticularAspectsOfPay", "Guidance")
                    },
                    new PaginationPage
                    {
                        Title = "Do men and women receive different performance scores on average?",
                        Url = helper.Action("EightWaysDoMenAndWomenReceive", "Guidance")
                    },
                    new PaginationPage
                    {
                        Title = "Are you doing all that you can to support part-time employees to progress?",
                        Url = helper.Action("EightWaysAreYouDoingAllThatYouCan", "Guidance")
                    },
                    new PaginationPage
                    {
                        Title = "Are you supporting both men and women to take on caring responsibilities?",
                        Url = helper.Action("EightWaysAreYouSupportingBoth", "Guidance")
                    },
                }
            };
        }

    }
}

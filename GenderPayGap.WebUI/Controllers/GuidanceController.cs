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

    }
}

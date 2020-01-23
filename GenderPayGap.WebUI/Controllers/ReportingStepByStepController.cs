using GenderPayGap.Core;
using GenderPayGap.Core.Models.HttpResultModels;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.ReportingStepByStep
{
    public class ReportingStepByStepController : Controller
    {

        [HttpGet("reporting-step-by-step")]
        public IActionResult StepByStepStandalone()
        {
            if (FeatureFlagHelper.IsFeatureEnabled(FeatureFlag.ReportingStepByStep))
            {
                return View("../ReportingStepByStep/StandalonePage");
            }
            else
            {
                return new HttpNotFoundResult();
            }
        }

        [HttpGet("reporting-step-by-step/find-out-what-the-gender-pay-gap-is")]
        public IActionResult Step1Task1()
        {
            if (FeatureFlagHelper.IsFeatureEnabled(FeatureFlag.ReportingStepByStep))
            {
                return View("../ReportingStepByStep/Step1FindOutWhatTheGpgIs");
            }
            else
            {
                return new HttpNotFoundResult();
            }
        }
        
        [HttpGet("reporting-step-by-step/report")]
        public IActionResult Step6Task1()
        {
            if (FeatureFlagHelper.IsFeatureEnabled(FeatureFlag.ReportingStepByStep))
            {
                return View("../ReportingStepByStep/Step6Task1");
            }
            else
            {
                return new HttpNotFoundResult();
            }
        }
    }
}

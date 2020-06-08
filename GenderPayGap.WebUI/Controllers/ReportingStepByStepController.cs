using System.Threading.Tasks;
using AutoMapper;
using GenderPayGap.Core;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.WebUI.Classes.Presentation;
using GenderPayGap.WebUI.Models;
using GenderPayGap.WebUI.Models.Search;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.ReportingStepByStep
{
    public class ReportingStepByStepController : Controller
    {
        public IViewingService ViewingService { get; }
        public ISearchViewService SearchViewService { get; }
        public ICompareViewService CompareViewService { get; }

        public ReportingStepByStepController(
            IViewingService viewingService, 
            ISearchViewService searchViewService,
            ICompareViewService compareViewService)
        {
            ViewingService = viewingService;
            SearchViewService = searchViewService;
            CompareViewService = compareViewService;
        }

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

        [HttpGet("reporting-step-by-step/view-and-compare-organisations")]
        public async Task<IActionResult> Step1Task2([FromQuery] SearchResultsQuery searchQuery, string orderBy = "relevance")
        {
            if (FeatureFlagHelper.IsFeatureEnabled(FeatureFlag.ReportingStepByStep))
            {
                //When never searched in this session
                if (string.IsNullOrWhiteSpace(SearchViewService.LastSearchParameters))
                {
                    //If no compare employers in session then load employers from the cookie
                    if (CompareViewService.BasketItemCount == 0)
                    {
                        CompareViewService.LoadComparedEmployersFromCookie();
                    }
                }

                // ensure parameters are valid
                if (!searchQuery.TryValidateSearchParams(out HttpStatusViewResult result))
                {
                    return result;
                }

                // generate result view model
                var searchParams = Mapper.Map<EmployerSearchParameters>(searchQuery);
                SearchViewModel model = await ViewingService.SearchAsync(searchParams, orderBy);

                    ViewBag.ReturnUrl = SearchViewService.GetLastSearchUrl();

                    ViewBag.BasketViewModel = new CompareBasketViewModel {
                        CanAddEmployers = false, CanViewCompare = CompareViewService.BasketItemCount > 1, CanClearCompare = true
                    };
                return View("../ReportingStepByStep/Step1Task2", model);
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

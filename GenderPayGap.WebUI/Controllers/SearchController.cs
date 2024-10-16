using System.Linq;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Classes.Presentation;
using GenderPayGap.WebUI.Models.Search;
using GenderPayGap.WebUI.Search;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    public class SearchController : Controller
    {

        private readonly ICompareViewService compareViewService;
        private readonly IDataRepository dataRepository;
        private readonly ViewingSearchServiceNew viewingSearchService;
        
        public SearchController(
            ICompareViewService compareViewService,
            IDataRepository dataRepository,
            ViewingSearchServiceNew viewingSearchService)
        {
            this.compareViewService = compareViewService;
            this.dataRepository = dataRepository;
            this.viewingSearchService = viewingSearchService;
        }

        [HttpGet("search")]
        public IActionResult SearchPage(SearchPageViewModel viewModel)
        {
            compareViewService.LoadComparedEmployersFromCookie();

            PopulateViewModel(viewModel);
            
            return View("SearchPage", viewModel);
        }

        private void PopulateViewModel(SearchPageViewModel viewModel)
        {
            viewModel.PossibleSectors = dataRepository.GetAll<SicSection>().ToList();
            viewModel.PossibleReportedLateYears =
                ReportingYearsHelper.GetReportingYears()
                    .Where(year => year != ReportingYearsHelper.GetCurrentReportingYear())
                    .OrderByDescending(year => year)
                    .ToList();
        }

        [HttpGet("search-api")]
        public IActionResult SearchApi(SearchPageViewModel viewModel)
        {
            return Json(viewingSearchService.Search(viewModel));
        }


    }
}

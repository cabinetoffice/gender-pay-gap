using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Classes.Presentation;
using GenderPayGap.WebUI.Models.Search;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    public class SearchController : Controller
    {

        private readonly ICompareViewService compareViewService;
        private readonly IDataRepository dataRepository;
        
        public SearchController(ICompareViewService compareViewService, IDataRepository dataRepository)
        {
            this.compareViewService = compareViewService;
            this.dataRepository = dataRepository;
        }

        [HttpGet("search")]
        public IActionResult SearchPage(SearchPageViewModel viewModel)
        {
            compareViewService.LoadComparedEmployersFromCookie();

            viewModel.PossibleSectors = dataRepository.GetAll<SicSection>().ToList();
            
            return View("SearchPage", viewModel);
        }


    }
}
